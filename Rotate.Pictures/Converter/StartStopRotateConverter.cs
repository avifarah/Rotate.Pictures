using System;
using System.Globalization;
using System.Windows.Data;


namespace Rotate.Pictures.Converter
{
	public sealed class StartStopRotateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var running = (bool?)value;
			if (!running.HasValue) return "Red";
			return running.Value ? "Green" : "Red";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}
