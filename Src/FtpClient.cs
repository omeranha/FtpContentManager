using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Threading;

namespace FtpContentManager.Src {
	public partial class FtpClient(string hostname, string username, string password, int port) : IDisposable {
		private TcpClient? client;
		private NetworkStream? controlStream;
		private StreamReader? controlReader;
		private StreamWriter? controlWriter;
		private readonly string _hostname = hostname;
		private readonly string _username = username;
		private readonly string _password = password;
		private readonly int _port = port;
		private string _workingDirectory = "/";
		private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
		public bool IsConnected { get; private set; } = false;

		private FtpWebRequest FtpRequest(string requestUri) {
			var request = (FtpWebRequest)WebRequest.Create($"ftp://{_hostname}:{_port}{requestUri}");
			request.Credentials = new NetworkCredential(_username, _password);
			request.UseBinary = true;
			request.KeepAlive = false;
			return request;
		}

		public async Task<bool> ConnectAsync() {
			if (IsConnected || client != null) {
				Disconnect();
			}

			try {
				client = new TcpClient();
				await client.ConnectAsync(_hostname, _port);
				controlStream = client.GetStream();
				controlReader = new StreamReader(controlStream, Encoding.ASCII);
				controlWriter = new StreamWriter(controlStream, Encoding.ASCII) { AutoFlush = true };

				await ReadResponseAsync(); // read welcome message
				await controlWriter.WriteLineAsync($"USER {_username}");
				await ReadResponseAsync();
				await controlWriter.WriteLineAsync($"PASS {_password}");
				await ReadResponseAsync();
				await controlWriter.WriteLineAsync("TYPE I"); // set binary mode
				await ReadResponseAsync();
				IsConnected = true;
				return true;
			} catch (Exception) {
				Disconnect();
				throw new InvalidOperationException("Failed to connect to FTP server");
			}
		}

		public void Disconnect() {
			if (!IsConnected && client == null) return;

			controlWriter?.WriteLine("QUIT"); // be polite
			controlWriter?.Flush();
			controlReader?.Dispose();
			controlWriter?.Dispose();
			controlStream?.Dispose();
			client?.Close();
			client?.Dispose();

			controlReader = null;
			controlWriter = null;
			controlStream = null;
			client = null;
			IsConnected = false;
		}

		public void Dispose() {
			Disconnect();
			_connectionSemaphore?.Dispose();
			GC.SuppressFinalize(this);
		}

		private async Task<string> ReadResponseAsync() {
			if (controlReader == null) throw new InvalidOperationException("Not connected");

			var responses = new List<string>();
			string? firstLine = await controlReader.ReadLineAsync() ?? throw new InvalidOperationException("Connection lost");

			if (firstLine.Length > 0 && (firstLine[0] == '4' || firstLine[0] == '5')) {
				throw new Exception($"FTP Error: {firstLine}");
			}

			// if the first line ends with '-', it means the response is multi-line
			if (firstLine.Length < 4 || firstLine[3] != '-') {
				return firstLine;
			}

			responses.Add(firstLine);
			string responseCode = firstLine[..3];
			string? line;
			while ((line = await controlReader.ReadLineAsync()) != null) {
				responses.Add(line);
				// this indicates the end of the multi-line response
				if (line.StartsWith(responseCode + " ")) {
					break;
				}
			}
			return string.Join("\n", responses);
		}

		public async Task<string> SendCommandAsync(string command) {
			await _connectionSemaphore.WaitAsync();
			try {
				if (controlWriter == null || !IsConnected) {
					if (!IsConnected) {
						await ConnectAsync();
					}
				}

				if (controlWriter == null) {
					throw new InvalidOperationException("Failed to establish connection");
				}

				try {
					await controlWriter.WriteLineAsync(command);
					return await ReadResponseAsync();
				} catch (Exception) {
					// connection lost, disconnect first to clean up, then try to reconnect once
					Disconnect();
					await ConnectAsync();
					if (controlWriter == null) {
						throw new InvalidOperationException("Failed to re-establish connection");
					}
					await controlWriter.WriteLineAsync(command);
					return await ReadResponseAsync();
				}
			} finally {
				_connectionSemaphore.Release();
			}
		}

		private async Task<string> SendCommandInternalAsync(string command) {
			if (controlWriter == null || !IsConnected) {
				if (!IsConnected) {
					await ConnectAsync();
				}
			}

			if (controlWriter == null) {
				throw new InvalidOperationException("Failed to establish connection");
			}

			await controlWriter.WriteLineAsync(command);
			return await ReadResponseAsync();
		}

		public async Task<List<FileListItem>> GetFolderItems(string folder) {
			await _connectionSemaphore.WaitAsync();
			try {
				var items = new List<FileListItem>();
				await SendCommandInternalAsync($"CWD {folder}");
				_workingDirectory = folder;
				string pasvResponse = await SendCommandInternalAsync("PASV");
				var (address, port) = ParsePasvResponse(pasvResponse);
				var listTask = SendCommandInternalAsync("LIST");

				using var dataClient = new TcpClient();
				await dataClient.ConnectAsync(address, port);
				using var dataStream = dataClient.GetStream();
				using var dataReader = new StreamReader(dataStream, Encoding.ASCII);
				await listTask;

				string? line;
				while ((line = await dataReader.ReadLineAsync()) != null) {
					// "drwxrwxrwx   1 root root             0 Jan 01  2000 Hdd1"
					if (string.IsNullOrWhiteSpace(line)) continue;
					var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (tokens.Length < 9) continue;

					bool isDir = tokens[0][0] == 'd';
					string name = string.Join(" ", tokens.Skip(8));
					var type = isDir ? ItemType.Directory : ItemType.File;
					var size = isDir ? 0 : long.Parse(tokens[4]);
					var date = $"{tokens[5]} {tokens[6]} {tokens[7]}";
					string itemPath = folder.TrimEnd('/') + "/" + name;
					items.Add(new FileListItem(name, size, date, type, itemPath));
				}

				await ReadResponseAsync(); // read final response
				return items;
			} finally {
				_connectionSemaphore.Release();
			}
		}

		public async Task<long> GetFolderSize(string folder) {
			long totalSize = 0;
			var items = await GetFolderItems(folder);
			foreach (var item in items) {
				if (item.Type == ItemType.Directory) {
					totalSize += await GetFolderSize(item.Path);
				} else {
					totalSize += item.Size;
				}
			}
			return totalSize;
		}

		// using FtpWebRequest to download file bytes due issues in directory changing and passive mode
		public async Task<byte[]> DownloadFileBytes(string path, int stopPosition = 0) {
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
			}
			return memoryStream.ToArray();
		}

		public async Task UploadFileAsync(string localPath, string remotePath) {
			var (directory, filename) = SplitPath(remotePath);
			await ChangeDirectory(directory);

			string pasvResponse = await SendCommandAsync("PASV");
			var (address, port) = ParsePasvResponse(pasvResponse);
			var storTask = SendCommandAsync($"STOR {filename}");

			using var dataClient = new TcpClient();
			await dataClient.ConnectAsync(address, port);
			using var dataStream = dataClient.GetStream();
			using var fileStream = File.OpenRead(localPath);
			await storTask;

			await fileStream.CopyToAsync(dataStream);
			dataStream.Close();
			dataClient.Close();
			await ReadResponseAsync();
		}

		public async Task ExecuteCommandAsync(string command, string remotePath) {
			// "DELE", "RMD", "MKD"
			var (directory, name) = SplitPath(remotePath);
			await ChangeDirectory(directory);
			await SendCommandAsync($"{command} {name}");
		}

		[GeneratedRegex(@"\((\d+),(\d+),(\d+),(\d+),(\d+),(\d+)\)")]
		private static partial Regex MyRegex();
		private static (IPAddress Address, int Port) ParsePasvResponse(string response) {
			// "227 Entering Passive Mode (192,168,1,1,xx,xx)"
			var match = MyRegex().Match(response);
			if (!match.Success) throw new Exception("Invalid PASV response");

			var groups = match.Groups;
			string ip = $"{groups[1].Value}.{groups[2].Value}.{groups[3].Value}.{groups[4].Value}";
			int port = int.Parse(groups[5].Value) * 256 + int.Parse(groups[6].Value);

			return (IPAddress.Parse(ip), port);
		}

		private static (string Directory, string Filename) SplitPath(string path) {
			string directory = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "/";
			if (directory == "" || directory == ".") directory = "/";
			string filename = Path.GetFileName(path);
			return (directory, filename);
		}

		private async Task ChangeDirectory(string directory) {
			if (directory != _workingDirectory) {
				string cdPath = directory == "/" ? "/" : directory.TrimStart('/');
				await SendCommandAsync($"CWD {cdPath}");
				_workingDirectory = directory;
			}
		}

		public async Task<Dictionary<string, string>> GetStorageInfo() {
			var response = await SendCommandAsync("SITE STORAGEINFO");
			var disks = new Dictionary<string, string>();
			var list = response.Split(["\n"], StringSplitOptions.RemoveEmptyEntries).ToList();
			// "Hdd1: 100GB / 120GB [80% used]"
			foreach (var line in list) {
				int splitter = line.IndexOf(':');
				if (splitter > 0) {
					string name = line[..splitter].Trim();
					string info = line[(splitter + 1)..].Trim();
					disks.Add(name, info);
				}
			}
			return disks;
		}
	}
}