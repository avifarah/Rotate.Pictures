using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using Rotate.Pictures.Utility;

namespace Rotate.Pictures.Converter
{
	public class StillPictureImageConverter : IValueConverter
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private string _previousPath;

		/// <summary>
		/// Converts filename to image
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var fileNm = value as string;
			var ext = Path.GetExtension(fileNm);
			if (ConfigValue.Inst.StillPictureExtensions().Contains(ext))
			{
				_previousPath = fileNm;
				return fileNm;
			}

			//Debug.WriteLine($"IsMotionPictureConverter.  Value: {value}");
			return _previousPath;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Log.Error($"{nameof(ConvertBack)} is not implemented for {nameof(IsMotionPictureConverter)}");
			throw new NotImplementedException();
		}
	}
}
