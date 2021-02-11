using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace Rotate.Pictures.Converter
{
	/// <summary>
	/// Purpose:
	///		This converter will change running status to Green/Red colors
	/// </summary>
	public sealed class StartStopRotateConverter : IValueConverter
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var running = (bool?)value;
			if (!running.HasValue) return "Red";
			return running.Value ? "Green" : "Red";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Log.Error($"{nameof(ConvertBack)} is not implemented for {nameof(StartStopRotateConverter)}");
			throw new NotImplementedException();
		}
	}
}
