using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FtpContentManager.Src;
namespace FtpContentManager.Views;

public partial class SettingsWindow : Window {
	private readonly MainWindow _mainWindow;

	public SettingsWindow(MainWindow mainWindow) {
		InitializeComponent();
		ip.AddHandler(TextInputEvent, TextBox_CheckDigits, RoutingStrategies.Tunnel);
		port.AddHandler(TextInputEvent, TextBox_CheckDigits, RoutingStrategies.Tunnel);

		_mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
		ip.Text = mainWindow.FtpIp ?? string.Empty;
		username.Text = mainWindow.FtpUsername ?? string.Empty;
		password.Text = mainWindow.FtpPassword ?? string.Empty;
		port.Text = mainWindow.FtpPort ?? string.Empty;
	}

	private void Close_Click(object? sender, RoutedEventArgs e) {
		Close();
	}

	private async void Connect_Click(object? sender, RoutedEventArgs e) {
		if (string.IsNullOrWhiteSpace(ip.Text) || string.IsNullOrWhiteSpace(username.Text) ||
			string.IsNullOrWhiteSpace(password.Text) || string.IsNullOrWhiteSpace(port.Text)) {
			await MessageBox("Please fill in all fields.");
			return;
		}

		if (!(Uri.CheckHostName(ip.Text) == UriHostNameType.IPv4) || !IPAddress.TryParse(ip.Text, out _)) {
			await MessageBox("Invalid IP address format.");
			return;
		}

		if (!int.TryParse(port.Text, out int portNumber) || portNumber <= 0 || portNumber > 65535) {
			await MessageBox("Port must be between 1 and 65535.");
			return;
		}

		try {
			await File.WriteAllTextAsync("ftp_credentials.txt", $"{ip.Text}\n{username.Text}\n{password.Text}\n{port.Text}");

			var existingClient = _mainWindow.FtpClient;
			if (existingClient?.IsConnected == true) {
				existingClient.Disconnect();
			}

			(_mainWindow.FtpIp, _mainWindow.FtpUsername, _mainWindow.FtpPassword, _mainWindow.FtpPort) = (ip.Text, username.Text, password.Text, port.Text);
			var ftpClient = new FtpClient(_mainWindow.FtpIp, _mainWindow.FtpUsername, _mainWindow.FtpPassword, portNumber);
			_mainWindow.Title = $"FTP Content Manager: Connecting to {_mainWindow.FtpIp}...";
			_mainWindow.FtpClient = ftpClient;
			if (await ftpClient.ConnectAsync()) {
				_mainWindow.Title = $"FTP Content Manager: Connected to {_mainWindow.FtpIp}";
				await _mainWindow.ListItems();
				Close();
			}
		} catch (Exception ex) {
			_mainWindow.Title = "FTP Content Manager";
			await MessageBox($"FTP connection failed: {ex.Message}");
			_mainWindow.FtpClient?.Dispose();
			_mainWindow.FtpClient = null;
		}
	}

	private void TextBox_CheckDigits(object? sender, TextInputEventArgs e) {
		if (sender is not TextBox textBox || string.IsNullOrEmpty(e.Text)) return;

		char input = e.Text[0];
		if (!char.IsDigit(input) && !(textBox.Name == "ip" && input == '.')) {
			e.Handled = true;
		}
	}
	
	public async Task<bool> MessageBox(string message, bool showCancelButton = false, string okButtonText = "Ok", string cancelButtonText = "Cancel") {
		var msgBox = new MessageBox(message, showCancelButton, okButtonText, cancelButtonText);
		await msgBox.ShowDialog(this);
		return msgBox.Result == true;
	}
}