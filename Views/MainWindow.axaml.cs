using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System;
using Avalonia.Platform;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using FluentFTP;
using Avalonia.Platform.Storage;
using System.Collections.ObjectModel;
using FTPcontentManager.Src.Stfs;
using FTPcontentManager.Src.Models;
using System.Collections.Generic;
using System.Data;
namespace FTPcontentManager.Views;

public partial class MainWindow : Window
{
	private AsyncFtpClient client = new();
	private string ftpIp = String.Empty;
	private string ftpUsername = "xboxftp"; // Default username
	private string ftpPassword = String.Empty;
	private string ftpPort = "21"; // Default FTP port
	private readonly Bitmap folderIcon = new(AssetLoader.Open(new Uri("avares://FTPcontentManager/Assets/folder.png")));
	private readonly Bitmap fileIcon = new(AssetLoader.Open(new Uri("avares://FTPcontentManager/Assets/file.png")));
	private readonly Bitmap upIcon = new(AssetLoader.Open(new Uri("avares://FTPcontentManager/Assets/up.png")));
	private const string FtpCredentialsFile = "ftp_credentials.txt";
	private readonly ObservableCollection<FileListItem> fileListItems = [];

	public record FtpItemInfo(string Path, FtpObjectType Type);

	public MainWindow() {
		InitializeComponent();
		ip.AddHandler(TextInputEvent, TextBox_CheckDigits, RoutingStrategies.Tunnel);
		port.AddHandler(TextInputEvent, TextBox_CheckDigits, RoutingStrategies.Tunnel);
		window_filelist.ItemsSource = fileListItems;
		Opened += MainWindow_Opened;
	}

	private async void MainWindow_Opened(object? sender, EventArgs e) {
		const string ftpCredentialsFile = "ftp_credentials.txt";
		if (!File.Exists(ftpCredentialsFile)) return;

		var credentials = await File.ReadAllLinesAsync(ftpCredentialsFile);
		if (credentials.Length < 4) return;

		if (Uri.CheckHostName(credentials[0]) != UriHostNameType.IPv4) {
			await MessageBox("Invalid IP address format in credentials file.");
			return;
		}

		ftpIp = credentials[0];
		ftpUsername = credentials[1];
		ftpPassword = credentials[2];
		ftpPort = credentials[3];
		ip.Text = ftpIp;
		username.Text = ftpUsername;
		password.Text = ftpPassword;
		port.Text = ftpPort;
		try {
			client = new AsyncFtpClient(ftpIp, ftpUsername, ftpPassword, int.Parse(ftpPort));
			await client.AutoConnect();
			await EnterContent();
		} catch (Exception ex) {
			await MessageBox($"FTP connection failed: {ex.Message}");
		}
	}

	private async Task LoadFolder(string folder) {
		await client.SetWorkingDirectory(folder);
		url.Content = await client.GetWorkingDirectory();
		fileListItems.Clear();

		fileListItems.Add(new FileListItem(new FtpListItem(), "[..]", upIcon));
		var items = (await client.GetListing()).OrderBy(i => i.Type != FtpObjectType.Directory)
			.ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
			.ToList();
		foreach (var item in items) {
			if (item.Type == FtpObjectType.Directory) {
				fileListItems.Add(new FileListItem(item, item.Name, folderIcon));
			} else if (item.Type == FtpObjectType.File) {
				fileListItems.Add(new FileListItem(item, item.Name, fileIcon));
			}
		}

		if (folder.EndsWith("0000000000000000")) {
			_ = ChangeFolderNames();
		} else if (folder.EndsWith("000B0000/") || folder.EndsWith("00000002/")) {
			_ = ChangeFileNames();
		}
	}

	private async Task ChangeFolderNames() {
		foreach (var item in fileListItems.ToList()) {
			if (item.DisplayName == "[..]" || item.Item.Type == FtpObjectType.File) continue;

			bool found = false;
			string[] subfolderPaths = [$"{item.Item.FullName}/00007000", $"{item.Item.FullName}/000D0000"];
			foreach (var subfolderPath in subfolderPaths) {
				var items = await ListItemPaths(subfolderPath);
				foreach (var listItem in items) {
					if (listItem.Type != FtpObjectType.File) continue;

					var svod = await ReadSvod(listItem.Path, StfsPackage.DefaultHeaderSizeVersion1);
					if (svod != null && svod.IsValid) {
						item.DisplayName = svod.DisplayName;
						if (svod.ThumbnailImage != null) {
							item.Icon = new Bitmap(new MemoryStream(svod.ThumbnailImage));
						}
						found = true;
						break;
					}
				}
				if (found) break;
			}
		}
	}

	private async Task ChangeFileNames() {
		foreach (var item in fileListItems.ToList()) {
			if (item.DisplayName == "[..]" || item.Item.Type == FtpObjectType.Directory) continue;

			var svod = await ReadSvod(item.Item.FullName, StfsPackage.DefaultHeaderSizeVersion1 + 12);
			if (svod != null && svod.IsValid) {
				item.DisplayName = svod.DisplayName;
				if (svod.ThumbnailImage != null) {
					item.Icon = new Bitmap(new MemoryStream(svod.ThumbnailImage));
				}
			}
		}
	}

	private async Task<SvodPackage?> ReadSvod(string path, long header = -1) {
		var bytes = await DownloadFileBytes(path, (int)header);
		if (bytes == null) return null;
		return ModelFactory.GetModel<SvodPackage>(bytes);
	}

	private async Task EnterContent() {
		string[] storages = [ "hdd1", "usb0", "usb1", "usb2" ];
		foreach (var item in await client.GetListing()) {
			if (storages.Any(root => item.Name.Equals(root, StringComparison.OrdinalIgnoreCase))) {
				await LoadFolder($"{item.Name}/Content/0000000000000000");
				break;
			}
		}
	}

	private async Task<List<FtpItemInfo>> ListItemPaths(string path) {
		string ftpPath = path.TrimStart('/');
		var request = (FtpWebRequest)WebRequest.Create($"ftp://{ftpIp}:{ftpPort}/{ftpPath}/");
		request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
		request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
		request.UseBinary = true;
		request.KeepAlive = false;

		using var response = (FtpWebResponse)await request.GetResponseAsync();
		using var reader = new StreamReader(response.GetResponseStream());
		var items = new List<FtpItemInfo>();
		string? line;
		while ((line = await reader.ReadLineAsync()) != null) {
			if (string.IsNullOrWhiteSpace(line)) continue;
			var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			string permissions = tokens[0];
			bool isDir = permissions[0] == 'd';
			string name = string.Join(" ", tokens.Skip(8));
			string iconKey = isDir ? "folder" : "file";
			FtpObjectType type = isDir ? FtpObjectType.Directory : FtpObjectType.File;
			string itemPath = path.TrimEnd('/') + "/" + name;
			items.Add(new FtpItemInfo(itemPath, type));
		}

		return items;
	}

	private async Task<byte[]> DownloadFileBytes(string path, int stopPosition = 0) {
		try {
			var request = (FtpWebRequest)WebRequest.Create($"ftp://{ftpIp}:{ftpPort}{path}");
			request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
			request.Method = WebRequestMethods.Ftp.DownloadFile;
			request.UseBinary = true;
			request.KeepAlive = false;
			using var response = (FtpWebResponse)await request.GetResponseAsync();
			using var responseStream = response.GetResponseStream();
			using var memoryStream = new MemoryStream();
			await responseStream.CopyToAsync(memoryStream);
			var allBytes = memoryStream.ToArray();
			if (stopPosition > 0 && stopPosition < allBytes.Length)
				return allBytes.Take(stopPosition).ToArray();
			return allBytes;
		} catch (Exception ex) {
			await MessageBox($"Error downloading file bytes: {ex.Message}");
			return [];
		}
	}

	private async void Connect_Click(object? sender, RoutedEventArgs e) {
		if (string.IsNullOrWhiteSpace(ip.Text) || string.IsNullOrWhiteSpace(username.Text) || string.IsNullOrWhiteSpace(password.Text) || string.IsNullOrWhiteSpace(port.Text)) {
			await MessageBox("Please fill in all fields.");
			return;
		}

		if (Uri.CheckHostName(ip.Text) != UriHostNameType.IPv4) {
			await MessageBox("Invalid IP address format.");
			return;
		}

		await File.WriteAllTextAsync(FtpCredentialsFile, $"{ip.Text}\n{username.Text}\n{password.Text}\n{port.Text}");
		ftpIp = ip.Text;
		ftpUsername = username.Text;
		ftpPassword = password.Text;
		ftpPort = port.Text;
		try {
			client = new AsyncFtpClient(ftpIp, ftpUsername, ftpPassword);
			await client.AutoConnect();
			await EnterContent();
		} catch (Exception ex) {
			await MessageBox($"FTP connection failed: {ex.Message}");
		}
	}

	protected override async void OnKeyDown(KeyEventArgs e) {
		if (FocusManager?.GetFocusedElement() is TextBox) return;

		var items = window_filelist.Items?.Cast<FileListItem>().ToList();
		if (items is { Count: > 1 }) {
			int currentIndex = window_filelist.SelectedIndex;
			switch (e.Key) {
				case Key.Up:
					window_filelist.SelectedIndex = currentIndex <= 0 ? items.Count - 1 : currentIndex - 1;
					e.Handled = true;
					return;
				case Key.Down:
					window_filelist.SelectedIndex = currentIndex <= 0 ? 1 :
						currentIndex >= items.Count - 1 ? 1 : currentIndex + 1;
					e.Handled = true;
					return;
				case Key.Home:
					window_filelist.SelectedIndex = 1;
					e.Handled = true;
					return;
				case Key.End:
					window_filelist.SelectedIndex = items.Count - 1;
					e.Handled = true;
					return;
				case Key.PageUp:
					window_filelist.SelectedIndex = currentIndex is -1 or 0 ? 1 : Math.Max(1, currentIndex - 10);
					e.Handled = true;
					return;
				case Key.PageDown:
					window_filelist.SelectedIndex = currentIndex is -1 or 0
						? Math.Min(items.Count - 1, 11) : Math.Min(items.Count - 1, currentIndex + 10);
					e.Handled = true;
					return;
				case Key.Delete:
					if (window_filelist.SelectedItem is not FileListItem selected || selected.DisplayName == "[..]") return;
					if (!await MessageBox($"Are you sure you want to delete {selected.DisplayName}?", "Confirm Delete", ButtonEnum.YesNo)) return;
					try {
						var workingDir = await client.GetWorkingDirectory();
						await DeleteFtpItem(selected.Item);
						await LoadFolder(workingDir);
					} catch (Exception ex) {
						await MessageBox($"Error deleting {selected.GetName()}: {ex.Message}");
					}
					e.Handled = true;
					return;
				case Key.Enter:
					if (window_filelist.SelectedItem is not FileListItem selectedItem) return;
					if (selectedItem.Item.Type == FtpObjectType.File && selectedItem.DisplayName != "[..]") {
						DownloadFile(selectedItem.GetName(), selectedItem.DisplayName);
					} else {
						await EnterFolder();
					}
					e.Handled = true;
					return;
				case Key.Back:
					await BackFolder();
					e.Handled = true;
					return;
			}
		}
	}

	private async Task DeleteFtpItem(FtpListItem item) {
		if (item.Type == FtpObjectType.Directory) {
			await client.SetWorkingDirectory(item.FullName);
			foreach (var subItem in await client.GetListing()) {
				await DeleteFtpItem(subItem);
			}
			var request = (FtpWebRequest)WebRequest.Create($"ftp://{ftpIp}:{ftpPort}{item.FullName}");
			request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
			request.Method = WebRequestMethods.Ftp.RemoveDirectory;
			await request.GetResponseAsync();
		} else {
			await client.DeleteFile(item.Name);
		}
	}

	private async void DownloadFile(string filename, string displayname) {
		try {
			var picker = new FolderPickerOpenOptions {
				Title = "Select Destination Folder",
				AllowMultiple = false
			};
			var result = await this.StorageProvider.OpenFolderPickerAsync(picker);
			if (result.Count == 0) return;
			string destinationPath = result[0].Path.LocalPath;
			if (!string.IsNullOrEmpty(destinationPath)) {
				await client.DownloadFile(destinationPath + filename, displayname);
				await MessageBox($"Downloaded {displayname}");
			}
		} catch (Exception ex) {
			await MessageBox($"Error downloading {displayname}: {ex.Message}");
		}
	}


	private async Task BackFolder() {
		string currentDir = await client.GetWorkingDirectory();
		int lastSlash = currentDir.LastIndexOf('/');
		if (lastSlash <= 0) {
			await MessageBox("You are already at the root directory.");
			return;
		}
		await LoadFolder(currentDir[..lastSlash]);
	}

	private async Task EnterFolder() {
		if (window_filelist.SelectedItem is not FileListItem folder) return;
		if (folder.DisplayName == "[..]") {
			await BackFolder();
			return;
		}
		await LoadFolder($"{folder.GetName()}/");
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

		string workingDir = await client.GetWorkingDirectory();
		foreach (var file in droppedFiles) {
			string path = file.Path.LocalPath;
			try {
				if (File.Exists(path)) {
					await client.UploadFile(path, $"{workingDir}/{file.Name}");
				} else if (Directory.Exists(path)) {
					await client.UploadDirectory(path, $"{workingDir}/{file.Name}");
				}
				await MessageBox($"{file.Name} uploaded successfully.");
				await LoadFolder(workingDir);
			} catch (Exception ex) {
				await MessageBox($"Error uploading {file.Name}: {ex.Message}");
			}
		}
	}

	private async void Filelist_DoubleTapped(object? sender, TappedEventArgs e) {
		if (window_filelist.SelectedItem is FileListItem selected) {
			bool isFile = selected.Type == FtpObjectType.File && selected.DisplayName != "[..]";
			if (isFile) {
				DownloadFile(selected.GetName(), selected.DisplayName);
			} else {
				await EnterFolder();
			}
		}
	}

	private static async Task<bool> MessageBox(string message, string title = "Caption", ButtonEnum buttons = ButtonEnum.Ok) {
		var box = MessageBoxManager.GetMessageBoxStandard(title, message, buttons);
		return await box.ShowAsync() == ButtonResult.Yes;
	}

	private void TextBox_CheckDigits(object? sender, TextInputEventArgs e) {
		if (sender is not TextBox textBox || string.IsNullOrEmpty(e.Text)) return;

		char input = e.Text[0];
		if (!char.IsDigit(input) && !(textBox.Name == "ip" && input == '.')) {
			e.Handled = true;
		}
	}
}
