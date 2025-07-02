using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Security.Cryptography;
using Avalonia.Platform;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using FluentFTP;

namespace FTPcontentManager.Views;

public partial class MainWindow : Window
{
	AsyncFtpClient client = new();
	private string ftpUrl = "ftp://";
	private string ftpUsername = string.Empty;
	private string ftpPassword = string.Empty;
	private readonly Dictionary<string, Bitmap> iconCache = [];
	private readonly string iconCacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IconCache");
	private readonly Bitmap folderIcon = new(AssetLoader.Open(new Uri("avares://FTPcontentManager/Assets/folder.png")));
	private readonly Bitmap fileIcon = new(AssetLoader.Open(new Uri("avares://FTPcontentManager/Assets/file.png")));
	private readonly Bitmap upIcon = new(AssetLoader.Open(new Uri("avares://FTPcontentManager/Assets/up.png")));

	public MainWindow()
	{
		InitializeComponent();
		iconCache["folder"] = folderIcon;
		iconCache["file"] = fileIcon;
		iconCache["up"] = upIcon;
		((IconKeyToBitmapConverter)Resources["IconKeyToBitmapConverter"]).IconCache = iconCache;
		((IconKeyToBitmapConverter)Resources["IconKeyToBitmapConverter"]).DefaultIcon = fileIcon;
		if (!Directory.Exists(iconCacheDir)) {
			Directory.CreateDirectory(iconCacheDir);
		}
		Opened += MainWindow_Opened;
	}

	private async void MainWindow_Opened(object? sender, EventArgs e) {
		const string ftpCredentialsFile = "ftp_credentials.txt";
		if (!File.Exists(ftpCredentialsFile)) return;

		var credentials = await File.ReadAllLinesAsync(ftpCredentialsFile);
		if (credentials.Length < 3) return;

		if (Uri.CheckHostName(credentials[0]) != UriHostNameType.IPv4) {
			await ShowMessage("Invalid IP address format in credentials file.");
			return;
		}

		ftpUsername = credentials[1];
		ftpPassword = credentials[2];
		ftpUrl = $"ftp://{credentials[0]}/";

		try {
			await Dispatcher.UIThread.InvokeAsync(() => {
				lbl_url.Content = ftpUrl;
				ip.Text = credentials[0];
				username.Text = ftpUsername;
				password.Text = ftpPassword;
			});

			client = new AsyncFtpClient(credentials[0], credentials[1], credentials[2]);
			await client.AutoConnect();

			string[] storageRoots = { "Hdd1", "Usb0", "Usb1" };
			await Refresh_FileListAsync("Hdd1");
		} catch (Exception ex) {
			await ShowMessage($"FTP connection failed: {ex.Message}");
		}
	}

	private async Task Refresh_FileListAsync(string folder) {
		foreach (FtpListItem item in await client.GetListing(folder)) {
			window_filelist.Items.Add(item);
		}
	}

	/*
	protected override void OnKeyDown(KeyEventArgs e) {
		if (FocusManager?.GetFocusedElement() is TextBox)
			return;

		var items = window_filelist.Items?.Cast<FtpItem>().ToList();
		if (items != null && items.Count > 1) // at least [..] + 1 real item
		{
			int currentIndex = window_filelist.SelectedIndex;

			switch (e.Key) {
				case Key.Up:
					if (currentIndex <= 0 || currentIndex == -1)
						window_filelist.SelectedIndex = items.Count - 1; // wrap to last
					else
						window_filelist.SelectedIndex = currentIndex - 1;
					e.Handled = true;
					return;
				case Key.Down:
					if (currentIndex == -1 || currentIndex == 0)
						window_filelist.SelectedIndex = 1; // skip [..] on first down
					else if (currentIndex >= items.Count - 1)
						window_filelist.SelectedIndex = 1; // wrap to first real item
					else
						window_filelist.SelectedIndex = currentIndex + 1;
					e.Handled = true;
					return;
				case Key.Home:
					window_filelist.SelectedIndex = 1; // first real item
					e.Handled = true;
					return;
				case Key.End:
					window_filelist.SelectedIndex = items.Count - 1;
					e.Handled = true;
					return;
				case Key.PageUp:
					if (currentIndex == -1 || currentIndex == 0)
						window_filelist.SelectedIndex = 1;
					else
						window_filelist.SelectedIndex = Math.Max(1, currentIndex - 10);
					e.Handled = true;
					return;
				case Key.PageDown:
					if (currentIndex == -1 || currentIndex == 0)
						window_filelist.SelectedIndex = Math.Min(items.Count - 1, 1 + 10);
					else
						window_filelist.SelectedIndex = Math.Min(items.Count - 1, currentIndex + 10);
					e.Handled = true;
					return;
			}
		}

		base.OnKeyDown(e);
		Filelist_KeyDown(window_filelist, e);
	}
	*/

	private async void Connect_Click(object? sender, RoutedEventArgs e) {
		if (string.IsNullOrWhiteSpace(ip.Text) || string.IsNullOrWhiteSpace(username.Text) || string.IsNullOrWhiteSpace(password.Text)) {
			await ShowMessage("Please fill in all fields.");
			return;
		}

		if (Uri.CheckHostName(ip.Text) != UriHostNameType.IPv4) {
			await ShowMessage("Invalid IP address format.");
			return;
		}

		ftpUsername = username.Text;
		ftpPassword = password.Text;
		const string ftpCredentialsFile = "ftp_credentials.txt";
		await File.WriteAllTextAsync(ftpCredentialsFile, $"{ip.Text}\n{ftpUsername}\n{ftpPassword}");
		await ConnectFtpAsync(ip.Text, ftpUsername, ftpPassword);
	}

	private async Task ConnectFtpAsync(string ip, string user, string pass) {
		ftpUrl = $"ftp://{ip}/";
		lbl_url.Content = ftpUrl;

	}

	/*
	private static async Task<List<FtpListItem>> GetFtpFileListAsync(AsyncFtpClient client, string ftpUrl) {
		var fileList = new List<FtpListItem> {
	};

		var uri = new Uri(ftpUrl);
		await client.ConnectAsync();

		foreach (var item in await client.GetListingAsync(uri.AbsolutePath)) {
			bool isDir = item.Type == FtpFileSystemObjectType.Directory;
			string name = item.Name;
			string lastModified = item.Modified.ToString();
			string size = isDir ? "—" : FormatSize(item.Size);
			string iconKey = isDir ? "folder" : "file";
			string displayName = name;

			fileList.Add(new FtpItem(name, lastModified, size, isDir, iconKey, displayName));
		}

		return fileList;

		static string FormatSize(long bytes) {
			if (bytes < 1024) return $"{bytes} B";
			double kb = bytes / 1024.0;
			if (kb < 1024) return $"{kb:F1} KB";
			double mb = kb / 1024.0;
			if (mb < 1024) return $"{mb:F1} MB";
			double gb = mb / 1024.0;
			return $"{gb:F2} GB";
		}
	}
	*/

	/*
	 if (ftpUrl.Contains("/Content/", StringComparison.OrdinalIgnoreCase)) {
				if (File.Exists("gamelist_xbox360.csv")) {
					var gameName = File.ReadLines("gamelist_xbox360.csv")
						.Skip(1)
						.Select(csvline => csvline.Split('\t'))
						.FirstOrDefault(columns => columns.Length >= 3 && columns[0].Equals(titleid, StringComparison.OrdinalIgnoreCase));
					if (gameName != null)
						displayName = $"{gameName[2]} [{titleid}]";
				}
				if (titleid != "00000001") {
					iconKey = $"http://xboxunity.net/Resources/Lib/Icon.php?tid={titleid}";
				}
			}
	*/

	/*
	private void AssignIcons(IEnumerable<FtpItem> fileList) {
		foreach (var item in fileList) {
			if (item.IconKey is "folder" or "file" or "up") continue;

			if (item.IconKey.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
				if (iconCache.ContainsKey(item.IconKey)) return;

				string cacheFile = GetIconCachePath(item.IconKey);
				if (File.Exists(cacheFile)) {
					try {
						using var stream = File.OpenRead(cacheFile);
						var bitmap = new Bitmap(stream);
						iconCache[item.IconKey] = bitmap;
					} catch {
						_ = DownloadIcon(item, fileIcon);
					}
				} else {
					_ = DownloadIcon(item, fileIcon);
				}
			}
		}
	}


	private async Task DownloadIcon(FtpItem item, Bitmap fallbackIcon) {
		var iconKey = item.IconKey;
		string cacheFile = GetIconCachePath(iconKey);
		try {
			using var webClient = new WebClient();
			byte[] data = await webClient.DownloadDataTaskAsync(iconKey);
			await File.WriteAllBytesAsync(cacheFile, data);

			await Dispatcher.UIThread.InvokeAsync(async () =>
			{
				try {
					using var stream = new MemoryStream(data);
					var bitmap = new Bitmap(stream);
					iconCache[iconKey] = bitmap;
					// Refresh the file list to update the icon
					await RefreshFileListAsync(ftpUrl, ftpUsername, ftpPassword);
				} catch {
					// Ignore icon load errors
				}
			});
		} catch {
			// Ignore, keep default icon
		}
	}
	*/


	private string GetIconCachePath(string url) {
		using var sha1 = SHA1.Create();
		byte[] hash = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(url));
		string hashString = BitConverter.ToString(hash).Replace("-", "");
		return Path.Combine(iconCacheDir, $"{hashString}.png");
	}

	/*
	private async void BackFolder() {
		string url = ftpUrl.TrimEnd('/');
		int lastSlash = url.LastIndexOf('/');
		if (lastSlash <= "ftp://".Length) {
			await ShowMessage("You are already at the root directory.");
			return;
		}

		ftpUrl = url[..(lastSlash + 1)];
		lbl_url.Content = ftpUrl;
		await RefreshFileListAsync(ftpUrl, ftpUsername, ftpPassword);
	}

	private async void EnterFolder() {
		if (window_filelist.SelectedItem is not FtpItem selectedFile || !selectedFile.IsDirectory)
			return;

		if (selectedFile.Name == "[..]") {
			BackFolder();
			return;
		}

		string newUrl = ftpUrl.EndsWith("/") ? ftpUrl : $"{ftpUrl}/";
		newUrl += $"{selectedFile.Name}/";
		ftpUrl = newUrl;
		lbl_url.Content = ftpUrl;
		await RefreshFileListAsync(newUrl, ftpUsername, ftpPassword);
	}
	*/

	private void Filelist_DragOver(object? sender, DragEventArgs e) {
		if (e.Data.GetFileNames() != null)
			e.DragEffects = DragDropEffects.Copy;
		else
			e.DragEffects = DragDropEffects.None;
		e.Handled = true;
	}

	private async void Filelist_Drop(object? sender, DragEventArgs e) {
		var droppedFiles = e.Data.GetFileNames();
		if (droppedFiles == null)
			return;

		foreach (string path in droppedFiles) {
			if (Directory.Exists(path))
				await UploadDirectoryToFtpAsync(path, ftpUrl, ftpUsername, ftpPassword);
			else if (File.Exists(path))
				await UploadFileToFtpAsync(path, ftpUrl, ftpUsername, ftpPassword);
		}
	}

	private void Filelist_DoubleTapped(object? sender, TappedEventArgs e) {
		//EnterFolder();
	}

	private async Task<bool> FtpFileExistsAsync(string ftpFileUrl, string user, string pass) {
		var request = (FtpWebRequest)WebRequest.Create(ftpFileUrl);
		request.Method = WebRequestMethods.Ftp.GetFileSize;
		request.Credentials = new NetworkCredential(user, pass);
		try {
			using var response = (FtpWebResponse)await request.GetResponseAsync();
			return true;
		} catch (WebException ex) {
			if (ex.Response is FtpWebResponse ftpResponse &&
				ftpResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
				return false;
			return false;
		}
	}

	private async Task UploadFileToFtpAsync(string localFile, string ftpUrl, string user, string pass) {
		string fileName = Path.GetFileName(localFile);
		string uploadUrl = $"{ftpUrl.TrimEnd('/')}/{fileName}";

		// Check if file exists on FTP
		if (await FtpFileExistsAsync(uploadUrl, user, pass)) {
			var overwrite = await ShowConfirm(
				$"The file '{fileName}' already exists on the server. Overwrite?", "Overwrite File");
			if (!overwrite)
				return;
		}

		var request = (FtpWebRequest)WebRequest.Create(uploadUrl);
		request.Method = WebRequestMethods.Ftp.UploadFile;
		request.Credentials = new NetworkCredential(user, pass);

		using (var fileStream = File.OpenRead(localFile))
		using (var ftpStream = await request.GetRequestStreamAsync()) {
			await fileStream.CopyToAsync(ftpStream);
		}

		await Dispatcher.UIThread.InvokeAsync(async () =>
		{
			await ShowMessage($"Upload of '{fileName}' successful.");
			//await RefreshFileListAsync(ftpUrl, user, pass);
		});
	}


	private async Task UploadDirectoryToFtpAsync(string localDir, string ftpUrl, string user, string pass) {
		string dirName = Path.GetFileName(localDir);
		string newFtpDirUrl = $"{ftpUrl.TrimEnd('/')}/{dirName}";

		var request = (FtpWebRequest)WebRequest.Create(newFtpDirUrl);
		request.Method = WebRequestMethods.Ftp.MakeDirectory;
		request.Credentials = new NetworkCredential(user, pass);

		try {
			using var _ = (FtpWebResponse)await request.GetResponseAsync();
		} catch (WebException) { }

		foreach (string file in Directory.GetFiles(localDir)) {
			await UploadFileToFtpAsync(file, newFtpDirUrl, user, pass);
		}
		foreach (string subDir in Directory.GetDirectories(localDir)) {
			await UploadDirectoryToFtpAsync(subDir, newFtpDirUrl, user, pass);
		}

		await Dispatcher.UIThread.InvokeAsync(async () => {
			await ShowMessage($"Successfully uploaded {dirName}!");
			//await RefreshFileListAsync(ftpUrl, user, pass);
		});
	}

	/*
	private async void Filelist_KeyDown(object? sender, KeyEventArgs e) {
		switch (e.Key) {
			case Key.Back:
				BackFolder();
				break;
			case Key.Enter:
				EnterFolder();
				break;
			case Key.Delete:
				if (window_filelist.SelectedItem is not FtpItem selected || selected.Name == "[..]") return;
				var result = await ShowConfirm($"Are you sure you want to delete {(selected.DisplayName ?? selected.Name)}?", "Confirm Delete");
				if (!result) return;
				try {
					await DeleteFtpItemAsync(selected, ftpUrl, ftpUsername, ftpPassword);
					await ShowMessage($"Successfully deleted {selected.Name}.");
					await RefreshFileListAsync(ftpUrl, ftpUsername, ftpPassword);
				} catch (Exception ex) {
					await ShowMessage($"Error deleting '{selected.Name}': {ex.Message}");
				}
				break;
		}
		e.Handled = true;
	}
	*/

	//private static async Task DeleteFtpItemAsync(FtpItem item, string ftpUrl, string user, string pass) {
	//	string itemUrl = $"{ftpUrl.TrimEnd('/')}/{item.Name}";

	//	var request = (FtpWebRequest)WebRequest.Create(itemUrl);
	//	request.Credentials = new NetworkCredential(user, pass);
	//	if (item.IsDirectory) {
	//		var subItems = await GetFtpFileListAsync($"{itemUrl}/", user, pass);
	//		foreach (var subItem in subItems) {
	//			if (subItem.Name == "[..]") continue;
	//			await DeleteFtpItemAsync(subItem, $"{itemUrl}/", user, pass);
	//		}
	//		request.Method = WebRequestMethods.Ftp.RemoveDirectory;
	//	} else {
	//		request.Method = WebRequestMethods.Ftp.DeleteFile;
	//	}

	//	using var _ = (FtpWebResponse)await request.GetResponseAsync();
	//}

	private static async Task ShowMessage(string message) {
		var box = MessageBoxManager.GetMessageBoxStandard("Caption", message);
		await box.ShowAsync();
	}

	private static async Task<bool> ShowConfirm(string message, string title) {
		var box = MessageBoxManager.GetMessageBoxStandard(title, message,ButtonEnum.YesNo);
		var result = await box.ShowAsync();
		return result == ButtonResult.Yes;
	}
}