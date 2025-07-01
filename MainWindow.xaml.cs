using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FTPcontentManager
{
	public class FtpFileInfo
	{
		public string Name { get; set; }
		public string LastModified { get; set; }
		public string Size { get; set; }
		public bool IsDirectory { get; set; }
		public string IconKey { get; set; }
		public BitmapImage Icon { get; set; } = null!;
		public string TitleId { get; set; }
		public string DisplayName { get; set; }

		public FtpFileInfo(string name, string lastModified, string size, bool isDirectory, string iconKey, string titleid, string? displayName = null) {
			Name = name;
			LastModified = lastModified;
			Size = size;
			IsDirectory = isDirectory;
			IconKey = iconKey;
			TitleId = titleid;
			DisplayName = displayName ?? name;
		}
	}

	public partial class MainWindow : Window
	{
		private string ftpUrl = "ftp://";
		private List<FtpFileInfo> folder = [];
		private readonly Dictionary<string, BitmapImage> iconCache = [];
		private readonly string iconCacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IconCache");
		private readonly BitmapImage folderIcon = new(new Uri("pack://application:,,,/Resources/folder.png"));
		private readonly BitmapImage fileIcon = new(new Uri("pack://application:,,,/Resources/file.png"));
		private readonly BitmapImage upIcon = new(new Uri("pack://application:,,,/Resources/up.png"));

		public MainWindow() {
			InitializeComponent();
			Loaded += MainWindow_Loaded;
			if (!Directory.Exists(iconCacheDir))
				Directory.CreateDirectory(iconCacheDir);
			filelist.KeyDown += Filelist_KeyDown;
		}

		private async void MainWindow_Loaded(object sender, RoutedEventArgs e) {
			const string ftpCredentialsFile = "ftp_credentials.txt";
			if (!File.Exists(ftpCredentialsFile)) return;

			var credentials = await File.ReadAllLinesAsync(ftpCredentialsFile);
			if (credentials.Length < 3) return;

			if (Uri.CheckHostName(credentials[0]) != UriHostNameType.IPv4) {
				MessageBox.Show("Invalid IP address format in credentials file.");
				return;
			}

			ip.Text = credentials[0];
			username.Text = credentials[1];
			password.Password = credentials[2];
			ftpUrl = $"ftp://{ip.Text}/";
			lbl_url.Content = ftpUrl;

			await RefreshFileListAsync(ftpUrl, username.Text, password.Password, autoEnterContent: true);
		}

		private async void Connect_Click(object sender, RoutedEventArgs e) {
			if (string.IsNullOrWhiteSpace(ip.Text) || string.IsNullOrWhiteSpace(username.Text) || string.IsNullOrWhiteSpace(password.Password)) {
				MessageBox.Show("Please fill in all fields.");
				return;
			}

			const string ftpCredentialsFile = "ftp_credentials.txt";
			await File.WriteAllTextAsync(ftpCredentialsFile, $"{ip.Text}\n{username.Text}\n{password.Password}");
			await ConnectFtpAsync(ip.Text, username.Text, password.Password);
		}

		private async Task ConnectFtpAsync(string ip, string user, string pass) {
			ftpUrl = $"ftp://{ip}/";
			lbl_url.Content = ftpUrl;
			await RefreshFileListAsync(ftpUrl, user, pass);
		}

		private async Task RefreshFileListAsync(string url, string user, string pass, bool autoEnterContent = false) {
			try {
				var fileList = await GetFtpFileListAsync(url, user, pass);
				AssignIcons(fileList);

				// Auto-enter Content/0000000000000000/ if found
				if (autoEnterContent) {
					var contentFolder = fileList.FirstOrDefault(item =>
						item.Name is "Hdd1" or "Usb0" or "Usb1");
					if (contentFolder != null) {
						ftpUrl = $"{url}{contentFolder.Name}/Content/0000000000000000/";
						lbl_url.Content = ftpUrl;
						fileList = await GetFtpFileListAsync(ftpUrl, user, pass);
						AssignIcons(fileList);
					}
				}

				filelist.ItemsSource = fileList;
			} catch (WebException ex) {
				MessageBox.Show($"Error connecting to FTP server: {ex.Message}");
			}
		}

		private static async Task<List<FtpFileInfo>> GetFtpFileListAsync(string ftpUrl, string ftpUsername, string ftpPassword) {
			var fileList = new List<FtpFileInfo>
			{
				new("[..]", string.Empty, string.Empty, true, "up", string.Empty)
			};

			var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
			request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
			request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
			request.UsePassive = true;
			request.UseBinary = true;
			request.KeepAlive = false;

			using var response = (FtpWebResponse)await request.GetResponseAsync();
			using var reader = new StreamReader(response.GetResponseStream());

			string? line;
			while ((line = await reader.ReadLineAsync()) != null) {
				if (string.IsNullOrWhiteSpace(line)) continue;

				var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				if (tokens.Length < 9) {
					fileList.Add(new FtpFileInfo(line, string.Empty, string.Empty, false, "file", string.Empty));
					continue;
				}

				string permissions = tokens[0];
				bool isDir = permissions[0] == 'd';
				string sizeStr = tokens[4];
				string lastModified = $"{tokens[5]} {tokens[6]} {tokens[7]}";
				string name = string.Join(" ", tokens.Skip(8));
				string titleid = name;

				long.TryParse(sizeStr, out long size);
				string displaySize = isDir ? "—" : FormatSize(size);

				string iconKey = isDir ? "folder" : "file";

				string displayName = name; // Default to original name

				// Special handling for /0000000000000000/ folder
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

				fileList.Add(new FtpFileInfo(name, lastModified, displaySize, isDir, iconKey, titleid, displayName));

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

		private void AssignIcons(IEnumerable<FtpFileInfo> fileList) {
			foreach (var item in fileList) {
				switch (item.IconKey) {
					case "folder":
						item.Icon = folderIcon;
						break;
					case "file":
						item.Icon = fileIcon;
						break;
					case "up":
						item.Icon = upIcon;
						break;
					default:
						if (item.IconKey.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
							if (iconCache.TryGetValue(item.IconKey, out var cachedIcon)) {
								item.Icon = cachedIcon;
							} else {
								item.Icon = fileIcon;
								string cacheFile = GetIconCachePath(item.IconKey);
								if (File.Exists(cacheFile)) {
									try {
										var bitmap = new BitmapImage();
										using (var stream = File.OpenRead(cacheFile)) {
											bitmap.BeginInit();
											bitmap.CacheOption = BitmapCacheOption.OnLoad;
											bitmap.StreamSource = stream;
											bitmap.EndInit();
											bitmap.Freeze();
										}
										iconCache[item.IconKey] = bitmap;
										item.Icon = bitmap;
									} catch {
										_ = TryDownloadAndCacheIconAsync(item, fileIcon);
									}
								} else {
									_ = TryDownloadAndCacheIconAsync(item, fileIcon);
								}
							}
						} else {
							item.Icon = fileIcon;
						}
						break;
				}
			}
		}

		private async Task TryDownloadAndCacheIconAsync(FtpFileInfo item, BitmapImage fallbackIcon) {
			var iconKey = item.IconKey;
			string cacheFile = GetIconCachePath(iconKey);
			try {
				using var webClient = new WebClient();
				byte[] data = await webClient.DownloadDataTaskAsync(iconKey);
				await File.WriteAllBytesAsync(cacheFile, data);

				await Dispatcher.InvokeAsync(() => {
					try {
						var bitmap = new BitmapImage();
						using var stream = new MemoryStream(data);
						bitmap.BeginInit();
						bitmap.CacheOption = BitmapCacheOption.OnLoad;
						bitmap.StreamSource = stream;
						bitmap.EndInit();
						bitmap.Freeze();
						iconCache[iconKey] = bitmap;
						item.Icon = bitmap;
						filelist.Items.Refresh();
					} catch {
						item.Icon = fallbackIcon;
					}
				});
			} catch {
				// Ignore, keep default icon
			}
		}

		private string GetIconCachePath(string url) {
			using var sha1 = SHA1.Create();
			byte[] hash = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(url));
			string hashString = BitConverter.ToString(hash).Replace("-", "");
			return Path.Combine(iconCacheDir, $"{hashString}.png");
		}

		private async void BackFolder() {
			string url = ftpUrl.TrimEnd('/');
			int lastSlash = url.LastIndexOf('/');
			if (lastSlash <= "ftp://".Length) {
				MessageBox.Show("You are already at the root directory.");
				return;
			}

			ftpUrl = url[..(lastSlash + 1)];
			lbl_url.Content = ftpUrl;
			await RefreshFileListAsync(ftpUrl, username.Text, password.Password);
		}

		private async void EnterFolder() {
			if (filelist.SelectedItem is not FtpFileInfo selectedFile || !selectedFile.IsDirectory)
				return;

			if (selectedFile.Name == "[..]") {
				BackFolder();
				return;
			}

			string newUrl = ftpUrl.EndsWith("/") ? ftpUrl : $"{ftpUrl}/";
			newUrl += $"{selectedFile.Name}/";
			ftpUrl = newUrl;
			lbl_url.Content = ftpUrl;
			await RefreshFileListAsync(newUrl, username.Text, password.Password);
		}

		private void Filelist_DragOver(object sender, DragEventArgs e) {
			e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
			e.Handled = true;
		}

		private async void Filelist_Drop(object sender, DragEventArgs e) {
			if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

			string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
			foreach (string path in droppedFiles) {
				if (Directory.Exists(path))
					await UploadDirectoryToFtpAsync(path, ftpUrl, username.Text, password.Password);
				else if (File.Exists(path))
					await UploadFileToFtpAsync(path, ftpUrl, username.Text, password.Password);
			}
		}

		private async Task UploadFileToFtpAsync(string localFile, string ftpUrl, string user, string pass) {
			string fileName = Path.GetFileName(localFile);
			string uploadUrl = $"{ftpUrl.TrimEnd('/')}/{fileName}";

			var request = (FtpWebRequest)WebRequest.Create(uploadUrl);
			request.Method = WebRequestMethods.Ftp.UploadFile;
			request.Credentials = new NetworkCredential(user, pass);

			using (var fileStream = File.OpenRead(localFile))
			using (var ftpStream = await request.GetRequestStreamAsync()) {
				await fileStream.CopyToAsync(ftpStream);
			}

			await Dispatcher.InvokeAsync(async () => {
				MessageBox.Show($"Upload of '{fileName}' successful.", "FTP Upload", MessageBoxButton.OK, MessageBoxImage.Information);
				await RefreshFileListAsync(ftpUrl, user, pass);
			});
		}

		private async Task UploadDirectoryToFtpAsync(string localDir, string ftpUrl, string user, string pass) {
			string dirName = Path.GetFileName(localDir);
			string newFtpDirUrl = $"{ftpUrl.TrimEnd('/')}/{dirName}";

			var dirRequest = (FtpWebRequest)WebRequest.Create(newFtpDirUrl);
			dirRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
			dirRequest.Credentials = new NetworkCredential(user, pass);

			try {
				using var _ = (FtpWebResponse)await dirRequest.GetResponseAsync();
			} catch (WebException) {
				// Directory may already exist, ignore error
			}

			foreach (string file in Directory.GetFiles(localDir))
				await UploadFileToFtpAsync(file, newFtpDirUrl, user, pass);

			foreach (string subDir in Directory.GetDirectories(localDir))
				await UploadDirectoryToFtpAsync(subDir, newFtpDirUrl, user, pass);

			await Dispatcher.InvokeAsync(async () => {
				MessageBox.Show($"Upload of directory '{dirName}' successful.", "FTP Upload", MessageBoxButton.OK, MessageBoxImage.Information);
				await RefreshFileListAsync(ftpUrl, user, pass);
			});
		}

		private async void Filelist_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
			if (e.Key == System.Windows.Input.Key.Back) {
				BackFolder();
			} else if (e.Key == System.Windows.Input.Key.Enter) {
				EnterFolder();
			} else if (e.Key == System.Windows.Input.Key.Delete) {
				if (filelist.SelectedItem is not FtpFileInfo selected || selected.Name == "[..]") return;

				var result = MessageBox.Show(
					$"Are you sure you want to delete '{selected.Name}'?",
					"Confirm Delete",
					MessageBoxButton.YesNo,
					MessageBoxImage.Warning);

				if (result != MessageBoxResult.Yes) return;

				try {
					await DeleteFtpItemAsync(selected, ftpUrl, username.Text, password.Password);
					MessageBox.Show($"'{selected.Name}' deleted successfully.", "FTP Delete", MessageBoxButton.OK, MessageBoxImage.Information);
					await RefreshFileListAsync(ftpUrl, username.Text, password.Password);
				} catch (Exception ex) {
					MessageBox.Show($"Error deleting '{selected.Name}': {ex.Message}", "FTP Delete", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private async Task DeleteFtpItemAsync(FtpFileInfo item, string ftpUrl, string user, string pass) {
			string itemUrl = $"{ftpUrl.TrimEnd('/')}/{item.Name}";

			var request = (FtpWebRequest)WebRequest.Create(itemUrl);
			request.Credentials = new NetworkCredential(user, pass);

			if (item.IsDirectory) {
				var subItems = await GetFtpFileListAsync($"{itemUrl}/", user, pass);
				foreach (var subItem in subItems) {
					if (subItem.Name == "[..]") continue;
					await DeleteFtpItemAsync(subItem, $"{itemUrl}/", user, pass);
				}
				request.Method = WebRequestMethods.Ftp.RemoveDirectory;
			} else {
				request.Method = WebRequestMethods.Ftp.DeleteFile;
			}

			using var _ = (FtpWebResponse)await request.GetResponseAsync();
		}

		private void filelist_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			EnterFolder();
		}
	}
}
