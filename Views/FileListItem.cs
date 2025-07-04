using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using FluentFTP;

namespace FTPcontentManager.Views
{
	/// <summary>
	/// Represents a file or directory item in the FTP file list.
	/// </summary>
	internal class FileListItem : INotifyPropertyChanged
	{
		private FtpListItem _item;
		/// <summary>
		/// The underlying FTP list item.
		/// </summary>
		public FtpListItem Item
		{
			get => _item;
			set => SetField(ref _item, value);
		}

		private string _displayName;
		/// <summary>
		/// The display name for the item.
		/// </summary>
		public string DisplayName
		{
			get => _displayName;
			set => SetField(ref _displayName, value);
		}

		/// <summary>
		/// The formatted display size for the item.
		/// </summary>
		public string DisplaySize { get; }

		/// <summary>
		/// The formatted display date for the item.
		/// </summary>
		public string DisplayDate { get; }

		private Bitmap? _icon;
		/// <summary>
		/// The icon representing the item.
		/// </summary>
		public Bitmap? Icon
		{
			get => _icon;
			set => SetField(ref _icon, value);
		}

		/// <summary>
		/// The type of the FTP object.
		/// </summary>
		public FtpObjectType Type => Item.Type;

		/// <summary>
		/// Initializes a new instance of the <see cref="FileListItem"/> class.
		/// </summary>
		public FileListItem(FtpListItem item, string name, Bitmap? icon) {
			_item = item ?? throw new ArgumentNullException(nameof(item));
			_displayName = name ?? item.Name;
			DisplaySize = name == "[..]" ? string.Empty : FormatSize(item.Size, item.Type == FtpObjectType.Directory);
			DisplayDate = name == "[..]" ? string.Empty : $"{item.Modified.ToShortDateString()} {item.Modified.ToShortTimeString()}";
			_icon = icon;
		}

		public FileListItem(FtpListItem item) {
			_item = item;
			_displayName = item.Name;
			DisplaySize = FormatSize(item.Size, item.Type == FtpObjectType.Directory);
			DisplayDate = $"{item.Modified.ToShortDateString()} {item.Modified.ToShortTimeString()}";
		}

		/// <summary>
		/// Returns the name of the item.
		/// </summary>
		public string GetName() => Item.Name;

		/// <summary>
		/// Formats the file size for display.
		/// </summary>
		private static string FormatSize(long bytes, bool isDirectory) {
			if (isDirectory) return string.Empty;
			if (bytes < 1024) return $"{bytes} B";
			double kb = bytes / 1024.0;
			if (kb < 1024) return $"{kb:F1} KB";
			double mb = kb / 1024.0;
			if (mb < 1024) return $"{mb:F1} MB";
			double gb = mb / 1024.0;
			return $"{gb:F2} GB";
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		/// <summary>
		/// Sets the field and raises PropertyChanged if the value changes.
		/// </summary>
		protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
			if (Equals(field, value)) return false;
			field = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			return true;
		}
	}
}
