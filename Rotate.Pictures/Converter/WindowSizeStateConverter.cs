using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Rotate.Pictures.Converter
{
	public class WindowSizeStateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value?.ToString() ?? "Normal";

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return WindowState.Normal;
			var rc = Enum.TryParse<WindowState>(value.ToString(), true, out var windowSizeState);
			if (!rc) return null;
			return windowSizeState;
		}
	}
}
