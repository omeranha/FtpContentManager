using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Avalonia.Platform.Storage;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data;
using FTPcontentManager.Src.Readers.Stfs;
using FTPcontentManager.Src.Models;
using FTPcontentManager.Src.Constants;
using FTPcontentManager.Src.Attributes;
namespace FTPcontentManager.Views;

public partial class MainWindow : Window
{
	public string ftpIp = String.Empty;
	public string ftpUsername = "xboxftp"; // Default username
	public string ftpPassword = String.Empty;
	public string ftpPort = "21"; // Default FTP port
	public const string FtpCredentialsFile = "ftp_credentials.txt";
	private readonly ObservableCollection<FileListItem> fileListItems = [];

	public MainWindow() {
		InitializeComponent();
		window_filelist.ItemsSource = fileListItems;
		Opened += MainWindow_Opened;
	}

	private async void MainWindow_Opened(object? sender, EventArgs e) {
		if (!File.Exists(FtpCredentialsFile)) return;

		var credentials = await File.ReadAllLinesAsync(FtpCredentialsFile);
		if (credentials.Length < 4 || Uri.CheckHostName(credentials[0]) != UriHostNameType.IPv4) {
			await MessageBox("Invalid or incomplete credentials file.");
			return;
		}

		(ftpIp, ftpUsername, ftpPassword, ftpPort) = (credentials[0], credentials[1], credentials[2], credentials[3]);
		try {
			await ListItems();
		} catch (Exception ex) {
			await MessageBox($"FTP connection failed: {ex.Message}");
		}
	}

	private FtpWebRequest FtpRequest(string requestUri) {
		var request = (FtpWebRequest)WebRequest.Create($"ftp://{ftpIp}:{ftpPort}{requestUri}");
		request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
		request.UseBinary = true;
		request.KeepAlive = false;
		return request;
	}

	public async Task ListItems(string? folder = "/") {
		folder ??= "/";
		fileListItems.Clear();

		if (folder != "/") {
			var index = folder.LastIndexOf('/');
			string parentPath = (index > 0) ? folder[..index] : "/";
			fileListItems.Add(new FileListItem("[..]", 0, string.Empty, ItemType.Parent, parentPath));
		}
		var items = await GetFolderItems(folder);
		foreach (var item in items.OrderBy(i => i.Type != ItemType.Directory).ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase)) {
			fileListItems.Add(item);
		}

		url.Content = folder;
		if (folder.EndsWith("0000000000000000")) {
			_ = ChangeFolderNames();
		} else if (folder.EndsWith("Content")) {
			_ = ChangeAccountFolders();
		} else if (folder.EndsWith("000B0000") || folder.EndsWith("00000002") || folder.EndsWith("00007000") || folder.EndsWith("000D0000")) {
			_ = ChangeFileNames();
		}
	}

	private async Task<List<FileListItem>> GetFolderItems(string folder) {
		var items = new List<FileListItem>();
		var request = FtpRequest($"/{Uri.EscapeDataString(folder.TrimStart('/'))}/");
		request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
		using var response = (FtpWebResponse)await request.GetResponseAsync();
		using var reader = new StreamReader(response.GetResponseStream());
		string? line;
		while ((line = await reader.ReadLineAsync()) != null) {
			// "drwxrwxrwx   1 root root             0 Jan 01  2000 Hdd1"
			if (string.IsNullOrWhiteSpace(line)) continue;
			var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			bool isDir = tokens[0][0] == 'd';
			string name = string.Join(" ", tokens.Skip(8));
			var type = isDir ? ItemType.Directory : ItemType.File;
			var size = isDir ? 0 : long.Parse(tokens[4]);
			var date = $"{tokens[5]} {tokens[6]} {tokens[7]}";
			string itemPath = folder.TrimEnd('/') + "/" + name;
			items.Add(new FileListItem(name, size, date, type, itemPath));
		}
		return items;
	}

	private async Task ChangeFolderNames() {
		foreach (var item in fileListItems.ToList()) {
			if (item.Type != ItemType.Directory) continue;

			bool found = false;
			string[] subfolderPaths = [$"{item.Path}/00007000", $"{item.Path}/000D0000"];
			foreach (var subfolderPath in subfolderPaths) {
				if (found) break;

				var request = FtpRequest($"/{Uri.EscapeDataString(subfolderPath.TrimStart('/'))}/");
				request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
				FtpWebResponse? response = null;
				try {
					response = (FtpWebResponse)await request.GetResponseAsync();
				} catch (WebException) {
					// if 00007000 folder does not exist, it's a XBLA game
					continue;
				}

				using var reader = new StreamReader(response.GetResponseStream());
				string? line;
				while ((line = await reader.ReadLineAsync()) != null) {
					if (string.IsNullOrWhiteSpace(line)) continue;

					var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					string name = string.Join(" ", tokens.Skip(8));
					var type = (tokens[0][0] == 'd') ? ItemType.Directory : ItemType.File;
					if (type != ItemType.File) continue;

					var svod = await ReadSvod($"{subfolderPath}/{name}", StfsPackage.DefaultHeaderSizeVersion1);
					if (svod == null || !svod.IsValid) continue;
					item.DisplayName = $"{svod.DisplayName} [{item.Name}]";
					if (svod.ThumbnailImage != null) {
						item.Icon = new Bitmap(new MemoryStream(svod.ThumbnailImage));
					}
					found = true;
					break;
				}
			}
		}
	}

	private async Task<SvodPackage?> ReadSvod(string path, long header = -1) {
		var bytes = await DownloadFileBytes(path, (int)header);
		if (bytes == null) return null;
		return ModelFactory.GetModel<SvodPackage>(bytes);
	}

	private async Task<byte[]> DownloadFileBytes(string path, int stopPosition = 0) {
		var request = FtpRequest(path);
		request.UsePassive = false;
		var memoryStream = new MemoryStream();
		try {
			using var response = (FtpWebResponse)await request.GetResponseAsync();
			using var responseStream = response.GetResponseStream();
			int bufferSize = stopPosition > 0 ? stopPosition : 81920;
			byte[] buffer = new byte[bufferSize];
			long totalRead = 0;
			int bytesRead;
			while (stopPosition == 0 || totalRead < stopPosition) {
				int readLength = stopPosition == 0 ? buffer.Length : (int)Math.Min(buffer.Length, stopPosition - totalRead);
				bytesRead = await responseStream.ReadAsync(buffer.AsMemory(0, readLength));
				if (bytesRead <= 0) break;
				await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead));
				totalRead += bytesRead;
			}
		} catch (WebException) {
			// when working with large files, sometimes it catches a 550 exception even with the bytes been downloaded correctly
			// so just ignore it
		}
		return memoryStream.ToArray();
	}

	private async Task ChangeFileNames() {
		foreach (var item in fileListItems.ToList()) {
			if (item.Type != ItemType.File) continue;

			var svod = await ReadSvod(item.Path, StfsPackage.DefaultHeaderSizeVersion1 + 12);
			if (svod != null && svod.IsValid) {
				item.DisplayName = svod.DisplayName;
				if (svod.InstallerType == InstallerType.TitleUpdate) {
					var version = svod.ReadValue<Version>(StfsPackage.DefaultHeaderSizeVersion1 + 8, 4, new BinaryDataAttribute(EndianType.BigEndian));
					item.DisplayName += $" (TU{version.Build})";
				}
				if (svod.ThumbnailImage != null) {
					item.Icon = new Bitmap(new MemoryStream(svod.ThumbnailImage));
				}
			}
		}
	}

	private async Task ChangeAccountFolders() {
		foreach (var item in fileListItems.ToList()) {
			if (item.Type == ItemType.Parent || item.Name == "0000000000000000") continue;
			var bytes = await DownloadFileBytes($"{item.Path}/FFFE07D1/00010000/{item.Name}");
			try {
				var stfs = ModelFactory.GetModel<StfsPackage>(bytes);
				stfs.ExtractAccount();
				item.DisplayName = stfs.Account.GamerTag;
				if (stfs.ThumbnailImage != null) {
					item.Icon = new Bitmap(new MemoryStream(stfs.ThumbnailImage));
				}
			} catch (Exception ex) {
				await MessageBox($"Error reading STFS package for {item.Name}: {ex.Message}");
				continue;
			}
		}
	}

	protected override async void OnKeyDown(KeyEventArgs e) {
		if (FocusManager?.GetFocusedElement() is TextBox) return;

		var items = window_filelist.Items?.Cast<FileListItem>().ToList();
		if (items is { Count: > 1 }) {
			int currentIndex = window_filelist.SelectedIndex;
			switch (e.Key) {
				case Key.Up:
					window_filelist.SelectedIndex = (currentIndex <= 0) ? items.Count - 1 : currentIndex - 1;
					e.Handled = true;
					break;
				case Key.Down:
					window_filelist.SelectedIndex = (currentIndex < 0 || currentIndex >= items.Count - 1) ? 1 : currentIndex + 1;
					break;
				case Key.Home:
					window_filelist.SelectedIndex = 1;
					break;
				case Key.End:
					window_filelist.SelectedIndex = items.Count - 1;
					break;
				case Key.PageUp:
					window_filelist.SelectedIndex = (currentIndex <= 0) ? 1 : Math.Max(1, currentIndex - 10);
					break;
				case Key.PageDown:
					window_filelist.SelectedIndex = (currentIndex <= 0) ? Math.Min(items.Count - 1, 11) : Math.Min(items.Count - 1, currentIndex + 10);
					break;
				case Key.Delete: {
						if (window_filelist.SelectedItem is not FileListItem selectedItem || selectedItem.Type == ItemType.Parent) return;
						if (!await MessageBox($"Are you sure you want to delete {selectedItem.DisplayName}?", "Confirm Delete", ButtonEnum.YesNo)) return;
						try {
							await DeleteFtpItem(selectedItem);
						} catch (Exception ex) {
							await MessageBox($"Error deleting {selectedItem.Name}: {ex.Message}");
						}
					break;
				}
				case Key.Enter: {
						if (window_filelist.SelectedItem is not FileListItem selectedItem) return;
						if (selectedItem.Type == ItemType.File) {
							try {
								await DownloadFile(selectedItem.Path);
							} catch (Exception ex) {
								await MessageBox($"Error downloading {selectedItem.Name}: {ex.Message}");
							}
						} else {
							await ListItems(selectedItem.Path);
						}
					break;
				}
				case Key.Back:
					var actualPath = url.Content?.ToString();
					if (actualPath == null || actualPath == "/") return;
					var index = actualPath.LastIndexOf('/');
					string parentPath = (index > 0) ? actualPath[..index] : "/";
					await ListItems(parentPath);
					break;
			}
		}
		e.Handled = true;
	}

	private async Task DeleteFtpItem(FileListItem item) {
		var request = FtpRequest(item.Path);
		if (item.Type == ItemType.Directory) {
			foreach (var subItem in await GetFolderItems(item.Path)) {
				await DeleteFtpItem(subItem);
			}
			request.Method = WebRequestMethods.Ftp.RemoveDirectory;
		} else {
			request.Method = WebRequestMethods.Ftp.DeleteFile;
		}
		using var response = (FtpWebResponse)await request.GetResponseAsync();
		await ListItems(url.Content?.ToString());
	}

	private async Task DownloadFile(string path) {
		string name = Path.GetFileName(path);
		try {
			var picker = new FolderPickerOpenOptions {
				Title = "Select Destination Folder",
				AllowMultiple = false
			};
			var result = await StorageProvider.OpenFolderPickerAsync(picker);
			if (result.Count == 0) return;
			string destinationPath = result[0].Path.LocalPath;
			if (!string.IsNullOrEmpty(destinationPath)) {
				var bytes = await DownloadFileBytes(path);
				if (bytes == null || bytes.Length == 0) {
					await MessageBox("Failed to download file or file is empty.");
					return;
				}
				string fullPath = Path.Combine(destinationPath, name);
				await File.WriteAllBytesAsync(fullPath, bytes);
				await MessageBox($"Downloaded {name}");
			}
		} catch (Exception ex) {
			await MessageBox($"Error downloading {name}: {ex.Message}");
		}
	}

	private void Filelist_DragOver(object? sender, DragEventArgs e) {
		e.DragEffects = e.Data.GetFiles() != null ? DragDropEffects.Copy : DragDropEffects.None;
		if (e.DragEffects == DragDropEffects.None) {
			e.Handled = true;
		}
	}

	private async void Filelist_Drop(object? sender, DragEventArgs e) {
		var droppedFiles = e.Data.GetFiles();
		if (droppedFiles == null) return;

		foreach (var file in droppedFiles) {
			try {
				await Upload(file.Name, url.Content?.ToString(), file.Path.LocalPath.ToString());
				await MessageBox($"Uploaded {file.Name} successfully.");
			} catch (Exception ex) {
				await MessageBox($"Error uploading {file.Name}: {ex.Message}");
			}
		}
	}

	private async Task Upload(string name, string? remoteFolder, string path) {
		string remotePath = string.IsNullOrEmpty(remoteFolder) ? $"/{name}" : $"{remoteFolder}/{name}";

		var request = FtpRequest(remotePath);
		if (File.Exists(path)) {
			request.Method = WebRequestMethods.Ftp.UploadFile;
			using var fileStream = File.OpenRead(path);
			using var requestStream = await request.GetRequestStreamAsync();
			await fileStream.CopyToAsync(requestStream);
			using var response = (FtpWebResponse)await request.GetResponseAsync();
		} else if (Directory.Exists(path)) {
			request.Method = WebRequestMethods.Ftp.MakeDirectory;
			try {
				using var _ = (FtpWebResponse)await request.GetResponseAsync();
			} catch (WebException) {
				// directory may already exist, ignore
			}

			var subItems = Directory.GetFileSystemEntries(path);
			foreach (string subItem in subItems) {
				var subName = Path.GetFileName(subItem);
				await Upload(subName, remotePath, subItem);
			}
		}

		await ListItems(url.Content?.ToString());
	}

	private async void Filelist_DoubleTapped(object? sender, TappedEventArgs e) {
		if (window_filelist.SelectedItem is FileListItem selected) {
			if (selected.Type != ItemType.File) {
				await ListItems(selected.Path);
			} else {
				await DownloadFile(selected.Path);
			}
		}
	}

	public async Task<bool> MessageBox(string message, string title = "FtpContentManager", ButtonEnum buttons = ButtonEnum.Ok) {
		var box = MessageBoxManager.GetMessageBoxStandard(title, message, buttons);
		return await box.ShowAsync() == ButtonResult.Yes;
	}

	private async void Settings_Click(object? sender, RoutedEventArgs e) {
		var settingsWindow = new SettingsWindow(this);
		await settingsWindow.ShowDialog<bool?>(this);
	}
}
