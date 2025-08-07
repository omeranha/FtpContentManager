using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using Avalonia.Platform.Storage;
using System.Collections.ObjectModel;
using System.Data;
using FtpContentManager.Src.Readers.Stfs;
using FtpContentManager.Src.Models;
using FtpContentManager.Src.Constants;
using FtpContentManager.Src.Attributes;
using FtpContentManager.Src;
namespace FtpContentManager.Views;

public partial class MainWindow : Window {
	public string FtpIp { get; set; } = string.Empty;
	public string FtpUsername { get; set; } = "xboxftp"; // Default username
	public string FtpPassword { get; set; } = string.Empty;
	public string FtpPort { get; set; } = "21"; // Default FTP port
	public const string FtpCredentialsFile = "ftp_credentials.txt";
	private readonly ObservableCollection<FileListItem> _fileListItems = [];
	public FtpClient? FtpClient { get; set; }

	public MainWindow() {
		InitializeComponent();
		window_filelist.ItemsSource = _fileListItems;
		Opened += MainWindow_Opened;
		Closed += MainWindow_Closed;
	}

	private async void MainWindow_Opened(object? sender, EventArgs e) {
		if (!File.Exists(FtpCredentialsFile)) return;

		var credentials = await File.ReadAllLinesAsync(FtpCredentialsFile);
		if (credentials.Length < 4 || Uri.CheckHostName(credentials[0]) != UriHostNameType.IPv4) {
			await MessageBox("Invalid or incomplete credentials file.");
			return;
		}

		FtpClient?.Dispose();
		(FtpIp, FtpUsername, FtpPassword, FtpPort) = (credentials[0], credentials[1], credentials[2], credentials[3]);
		FtpClient = new FtpClient(FtpIp, FtpUsername, FtpPassword, int.Parse(FtpPort));
		Title = $"FTP Content Manager: Connecting to {FtpIp}...";
		try {
			if (await FtpClient.ConnectAsync()) {
				Title = "FTP Content Manager: Connected to " + FtpIp;
				await ListItems();
			}
		} catch (Exception ex) {
			Title = "FTP Content Manager";
			await MessageBox($"FTP connection failed: {ex.Message}");
			FtpClient?.Dispose();
			FtpClient = null;
		}
	}

	private void MainWindow_Closed(object? sender, EventArgs e) {
		FtpClient?.Dispose();
		FtpClient = null;
	}

	public async Task ListItems(string? folder = "/") {
		if (FtpClient == null) return;

		folder ??= "/";
		_fileListItems.Clear();

		WorkingDirectory.Text = folder;
		if (folder != "/") {
			var index = folder.LastIndexOf('/');
			string parentPath = (index > 0) ? folder[..index] : "/";
			_fileListItems.Add(new FileListItem("[..]", 0, string.Empty, ItemType.Parent, parentPath));
			// Hdd1, Usb1, Usb2
			if (folder.Length == 5) {
				var disks = await FtpClient.GetStorageInfo();
				disks.TryGetValue(folder.Substring(1, 4), out string? diskInfo);
				DiskInfo.Text = diskInfo;
			}
		} else {
			DiskInfo.Text = string.Empty;
		}
		var items = await FtpClient.GetFolderItems(folder);
		foreach (var item in items.OrderBy(i => i.Type != ItemType.Directory).ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase)) {
			_fileListItems.Add(item);
		}

		if (folder.EndsWith("Content")) {
			_ = GetGamerTagsFromFolder();
		} else if (folder.EndsWith("0000000000000000") || folder.EndsWith("000B0000") || folder.EndsWith("00000002") || folder.EndsWith("00007000") || folder.EndsWith("000D0000")) {
			_ = LoadGameMetadata();
		}
	}

	private async Task LoadGameMetadata() {
		if (FtpClient == null) return;
		foreach (var item in _fileListItems.ToList()) {
			if (item.Type == ItemType.Parent) continue;

			if (item.Type == ItemType.Directory) {
				bool found = false;
				string[] subfolderPaths = [$"{item.Path}/00007000", $"{item.Path}/000D0000"];
				foreach (var subfolderPath in subfolderPaths) {
					if (found) break;
					try {
						var items = await FtpClient.GetFolderItems(subfolderPath);
						foreach (var subItem in items) {
							var svod = await ReadSvod($"{subfolderPath}/{subItem.Name}", StfsPackage.DefaultHeaderSizeVersion1);
							if (svod == null || !svod.IsValid) continue;
							item.DisplayName = $"{svod.DisplayName} [{item.Name}]";
							if (svod.ThumbnailImage != null) {
								item.Icon = new Bitmap(new MemoryStream(svod.ThumbnailImage));
							}
							found = true;
							break;
						}
					} catch (Exception) {
						// if 00007000 folder does not exist, it's a XBLA game
						continue;
					}
				}
			} else if (item.Type == ItemType.File) {
				var svod = await ReadSvod(item.Path, StfsPackage.DefaultHeaderSizeVersion1 + 12);
				if (svod == null || !svod.IsValid) continue;
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

	private async Task<SvodPackage?> ReadSvod(string path, long header = -1) {
		if (FtpClient == null) return null;
		var bytes = await FtpClient.DownloadFileBytes(path, (int)header);
		if (bytes == null) return null;
		return ModelFactory.GetModel<SvodPackage>(bytes);
	}

	private async Task GetGamerTagsFromFolder() {
		if (FtpClient == null) return;
		foreach (var item in _fileListItems.ToList()) {
			if (item.Type == ItemType.Parent || item.Name == "0000000000000000") continue;
			var bytes = await FtpClient.DownloadFileBytes($"{item.Path}/FFFE07D1/00010000/{item.Name}");
			try {
				var stfs = ModelFactory.GetModel<StfsPackage>(bytes);
				stfs.ExtractAccount();
				item.DisplayName = stfs.Account.GamerTag;
				if (stfs.ThumbnailImage != null) {
					item.Icon = new Bitmap(new MemoryStream(stfs.ThumbnailImage));
				}
			} catch (Exception) {
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
						if (await MessageBox($"Are you sure you want to delete {selectedItem.DisplayName}?", true, "Yes", "No")) {
							await DeleteFtpItem(selectedItem);
						}
						break;
					}
				case Key.Enter: {
						if (window_filelist.SelectedItem is not FileListItem selectedItem) return;
						if (selectedItem.Type == ItemType.File) {
							await DownloadFile(selectedItem.Path);
						} else {
							await ListItems(selectedItem.Path);
						}
						break;
					}
				case Key.Back:
					var actualPath = WorkingDirectory.Text;
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
		if (FtpClient == null) return;
		try {
			if (item.Type == ItemType.Directory) {
				foreach (var subItem in await FtpClient.GetFolderItems(item.Path)) {
					await DeleteFtpItem(subItem);
				}
				await FtpClient.ExecuteCommandAsync("RMD", item.Path);
			} else {
				await FtpClient.ExecuteCommandAsync("DELE", item.Path);
			}
			await ListItems(WorkingDirectory.Text);
		} catch (Exception ex) {
			await MessageBox($"Error deleting {item.Name}: {ex.Message}");
		}
	}

	private async Task DownloadFile(string path) {
		if (FtpClient == null) return;
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
				var bytes = await FtpClient.DownloadFileBytes(path);
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

	private async Task DownloadFolder(string path, string? destinationPath = null) {
		if (FtpClient == null) return;
		string folderName = Path.GetFileName(path);
		try {
			if (destinationPath == null) {
				var picker = new FolderPickerOpenOptions {
					Title = "Select Destination Folder",
					AllowMultiple = false
				};
				var result = await StorageProvider.OpenFolderPickerAsync(picker);
				if (result.Count == 0) return;
				destinationPath = result[0].Path.LocalPath;
			}
			if (!string.IsNullOrEmpty(destinationPath)) {
				string fullPath = Path.Combine(destinationPath, folderName);
				if (!Directory.Exists(fullPath)) {
					Directory.CreateDirectory(fullPath);
				}

				var items = await FtpClient.GetFolderItems(path);
				foreach (var item in items) {
					if (item.Type == ItemType.File) {
						var bytes = await FtpClient.DownloadFileBytes(item.Path);
						if (bytes != null && bytes.Length > 0) {
							string filePath = Path.Combine(fullPath, item.Name);
							await File.WriteAllBytesAsync(filePath, bytes);
						}
					} else if (item.Type == ItemType.Directory) {
						await DownloadFolder(item.Path, fullPath);
					}
				}
				await MessageBox($"Downloaded folder {folderName}");
			}
		} catch (Exception ex) {
			await MessageBox($"Error downloading folder {folderName}: {ex.Message}");
		}
	}

	private async Task Upload(string name, string? remoteFolder, string path) {
		if (FtpClient == null) return;
		string remotePath = string.IsNullOrEmpty(remoteFolder) || remoteFolder == "/" ? name : $"{remoteFolder.TrimEnd('/')}/{name}";
		if (File.Exists(path)) {
			await FtpClient.UploadFileAsync(path, remotePath);
		} else if (Directory.Exists(path)) {
			try {
				await FtpClient.ExecuteCommandAsync("MKD", remotePath);
			} catch (Exception) {
				// directory already exists, ignore
			}

			var subItems = Directory.GetFileSystemEntries(path);
			foreach (string subItem in subItems) {
				var subName = Path.GetFileName(subItem);
				await Upload(subName, remotePath, subItem);
			}
		}

		await ListItems(WorkingDirectory.Text);
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
				await Upload(file.Name, WorkingDirectory.Text, file.Path.LocalPath.ToString());
				await MessageBox($"Uploaded {file.Name} successfully.");
			} catch (Exception ex) {
				await MessageBox($"Error uploading {file.Name}: {ex.Message}");
			}
		}
	}

	private async void Filelist_DoubleTapped(object? sender, TappedEventArgs e) {
		if (window_filelist.SelectedItem is FileListItem selected) {
			if (selected.Type == ItemType.Directory || selected.Type == ItemType.Parent) {
				await ListItems(selected.Path);
			} else if (selected.Type == ItemType.File) {
				await DownloadFile(selected.Path);
			}
		}
	}

	public async Task<bool> MessageBox(string message, bool showCancelButton = false, string okButtonText = "Ok", string cancelButtonText = "Cancel") {
		var msgBox = new MessageBox(message, showCancelButton, okButtonText, cancelButtonText);
		await msgBox.ShowDialog(this);
		return msgBox.Result == true;
	}

	private async void OpenSettings(object? sender, RoutedEventArgs e) {
		var settingsWindow = new SettingsWindow(this);
		await settingsWindow.ShowDialog<bool?>(this);
	}

	private async void ContextMenu_Download(object? sender, RoutedEventArgs e) {
		if (window_filelist.SelectedItem is FileListItem selectedItem && selectedItem.Type != ItemType.Parent) {
			if (selectedItem.Type == ItemType.Directory) {
				await DownloadFolder(selectedItem.Path);
			} else {
				await DownloadFile(selectedItem.Path);
			}
		}
	}

	private async void ContextMenu_Delete(object? sender, RoutedEventArgs e) {
		if (window_filelist.SelectedItem is FileListItem selectedItem && selectedItem.Type != ItemType.Parent) {
			if (await MessageBox($"Are you sure you want to delete {selectedItem.DisplayName}?", true, "Yes", "No")) {
				await DeleteFtpItem(selectedItem);
			}
		}
	}

	private async void UploadToFolder_Click(object? sender, RoutedEventArgs e) {
		var picker = new FolderPickerOpenOptions {
			Title = "Select items to Upload",
			AllowMultiple = true
		};

		var result = await StorageProvider.OpenFolderPickerAsync(picker);
		if (result == null || result.Count == 0) return;
		foreach (var item in result) {
			try {
				await Upload(item.Name, WorkingDirectory.Text, item.Path.LocalPath.ToString());
				await MessageBox($"Uploaded {item.Name} successfully.");
			} catch (Exception ex) {
				await MessageBox($"Error uploading {item.Name}: {ex.Message}");
			}
		}
	}

	private async void Disconnect_Click(object? sender, RoutedEventArgs e) {
		if (FtpClient != null) {
			if (FtpClient.IsConnected) {
				FtpClient.Disconnect();
			}
			FtpClient.Dispose();
			FtpClient = null;
			Title = "FTP Content Manager";
			WorkingDirectory.Text = string.Empty;
			DiskInfo.Text = string.Empty;
			_fileListItems.Clear();
		} else {
			await MessageBox("You are not connected to any FTP server.");
		}
	}
}