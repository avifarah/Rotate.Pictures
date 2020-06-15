using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures.Converter
{
	/// <summary>
	/// Purpose:
	///		Allow XAML to know if file is a motion picture
	/// </summary>
	public class IsMotionPictureConverter : IValueConverter
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Implementation of IValueConverter

		/// <summary>
		/// Converts file name to a boolean value depending on the extension being that of a motion picture 
		/// </summary>
		/// <returns>True if extension of file name is that of motion picture</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var fileNm = value as string;
			var rc = fileNm.IsMotionPicture();
			return rc;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Log.Error($"{nameof(ConvertBack)} is not implemented for {nameof(IsMotionPictureConverter)}");
			throw new NotImplementedException();
		}

		#endregion
	}
}
