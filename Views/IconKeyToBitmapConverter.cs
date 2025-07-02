using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace FTPcontentManager.Views
{
	public class IconKeyToBitmapConverter : IValueConverter
	{
		public static IconKeyToBitmapConverter Instance { get; } = new();

		public Dictionary<string, Bitmap> IconCache { get; set; } = null!;
		public Bitmap? DefaultIcon { get; set; }

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
			if (value is string key && IconCache != null) {
				if (IconCache.TryGetValue(key, out var bmp))
					return bmp;
			}
			return DefaultIcon;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
			=> throw new NotSupportedException();
	}
}
