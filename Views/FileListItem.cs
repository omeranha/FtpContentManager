using System;
using System.ComponentModel;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace FtpContentManager.Views
{
	enum ItemType
	{
		File,
		Directory,
		Parent
	}

	internal class FileListItem : INotifyPropertyChanged
	{
		public string Name;
		private string _displayName;
		public string DisplayName {
			get => _displayName;
			set {
				if (_displayName != value) {
					_displayName = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
				}
			}
		}
		public string Size { get; }
		public string Date { get; }
		public ItemType Type;
		public string Path;
		private Bitmap? _icon;
		public Bitmap? Icon {
			get => _icon;
			set {
				if (_icon != value) {
					_icon = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
				}
			}
		}

		public FileListItem(string name, long size, string date, ItemType type, string path) {
			Name = name;
			_displayName = name;
			Size = FormatSize(size, type == ItemType.File);
			Date = date;
			Type = type;
			Path = path;
			switch (type) {
				case ItemType.File:
					Icon = new(AssetLoader.Open(new Uri("avares://FtpContentManager/Assets/file.png")));
					break;
				case ItemType.Directory:
					Icon = new(AssetLoader.Open(new Uri("avares://FtpContentManager/Assets/folder.png")));
					break;
				case ItemType.Parent:
					Icon = new(AssetLoader.Open(new Uri("avares://FtpContentManager/Assets/up.png")));
					break;
			}
		}

		private static string FormatSize(long bytes, bool isFile) {
			if (!isFile) return string.Empty;
			if (bytes < 1024) return $"{bytes} B";
			double kb = bytes / 1024.0;
			if (kb < 1024) return $"{kb:F1} KB";
			double mb = kb / 1024.0;
			if (mb < 1024) return $"{mb:F1} MB";
			double gb = mb / 1024.0;
			return $"{gb:F2} GB";
		}

		public event PropertyChangedEventHandler? PropertyChanged;
	}
}
