using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
namespace FTPcontentManager.Views;

public partial class SettingsWindow : Window {
	private readonly MainWindow _mainWindow;

	public SettingsWindow(MainWindow mainWindow) {
		InitializeComponent();
		ip.AddHandler(TextInputEvent, TextBox_CheckDigits, RoutingStrategies.Tunnel);
		port.AddHandler(TextInputEvent, TextBox_CheckDigits, RoutingStrategies.Tunnel);
		_mainWindow = mainWindow;
		ip.Text = mainWindow.ftpIp;
		username.Text = mainWindow.ftpUsername;
		password.Text = mainWindow.ftpPassword;
		port.Text = mainWindow.ftpPort;
	}

	private void Close_Click(object? sender, RoutedEventArgs e) {
		Close();
	}

	private async void Connect_Click(object? sender, RoutedEventArgs e) {
		if (string.IsNullOrWhiteSpace(ip.Text) || string.IsNullOrWhiteSpace(username.Text) ||
				string.IsNullOrWhiteSpace(password.Text) || string.IsNullOrWhiteSpace(port.Text)) {
			await _mainWindow.MessageBox("Please fill in all fields.");
			return;
		}

		if (Uri.CheckHostName(ip.Text) != UriHostNameType.IPv4) {
			await _mainWindow.MessageBox("Invalid IP address format.");
			return;
		}

		await File.WriteAllTextAsync("ftp_credentials.txt", $"{ip.Text}\n{username.Text}\n{password.Text}\n{port.Text}");
		(_mainWindow.ftpIp, _mainWindow.ftpUsername, _mainWindow.ftpPassword, _mainWindow.ftpPort) = (ip.Text, username.Text, password.Text, port.Text);
		try {
			await _mainWindow.ListItems();
		} catch (Exception ex) {
			await _mainWindow.MessageBox($"FTP connection failed: {ex.Message}");
		}
	}

	private void TextBox_CheckDigits(object? sender, TextInputEventArgs e) {
		if (sender is not TextBox textBox || string.IsNullOrEmpty(e.Text)) return;

		char input = e.Text[0];
		if (!char.IsDigit(input) && !(textBox.Name == "ip" && input == '.')) {
			e.Handled = true;
		}
	}
}
