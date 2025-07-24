using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
namespace FtpContentManager.Views;

public partial class MessageBox : Window, INotifyPropertyChanged {
	public new event PropertyChangedEventHandler? PropertyChanged;

	private string? _message;
	public string? Message
	{
		get => _message;
		set
		{
			if (_message != value)
			{
				_message = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
			}
		}
	}

	private bool _showCancelButton;
	public bool ShowCancelButton
	{
		get => _showCancelButton;
		set
		{
			if (_showCancelButton != value)
			{
				_showCancelButton = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowCancelButton)));
			}
		}
	}

	private string _okButtonText = "Ok";
	public string OkButtonText
	{
		get => _okButtonText;
		set
		{
			if (_okButtonText != value)
			{
				_okButtonText = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OkButtonText)));
			}
		}
	}

	private string _cancelButtonText = "Cancel";
	public string CancelButtonText
	{
		get => _cancelButtonText;
		set
		{
			if (_cancelButtonText != value)
			{
				_cancelButtonText = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CancelButtonText)));
			}
		}
	}

	public bool? Result { get; private set; }

	public MessageBox(string message, bool showCancelButton = false, string okButtonText = "Ok", string cancelButtonText = "Cancel")
	{
		Message = message;
		ShowCancelButton = showCancelButton;
		OkButtonText = okButtonText;
		CancelButtonText = cancelButtonText;
		DataContext = this;
		InitializeComponent();
	}

	private void Ok_Click(object? sender, RoutedEventArgs e) {
		Result = true;
		Close();
	}

	private void Cancel_Click(object? sender, RoutedEventArgs e) {
		Result = false;
		Close();
	}
}
