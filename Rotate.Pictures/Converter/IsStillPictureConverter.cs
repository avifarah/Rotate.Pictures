using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures.Converter
{
	/// <summary>
	/// Purpose:
	///		Allow XAML to determine if picture is a still picture
	/// </summary>
	public sealed class IsStillPictureConverter : IValueConverter
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<string> _stillPictures;

		public IsStillPictureConverter()
		{
			var configValue = ConfigValueProvider.Default;
			_stillPictures = configValue.StillPictureExtensions();
		}

		#region Implementation of IValueConverter

		/// <summary>
		/// Converts file name to a boolean value depending on the extension being that of a still picture 
		/// </summary>
		/// <returns>True if extension is that of still picture</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var fileNm = value as string;
			if (string.IsNullOrWhiteSpace(fileNm)) return false;
			var isStillPic = _stillPictures.Any(s => string.Compare(new FileInfo(fileNm).Extension, s, StringComparison.OrdinalIgnoreCase) == 0);
			//Debug.WriteLine($"IsStillPictureConverter.   Value: {value}");
			return isStillPic;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Log.Error($"{nameof(ConvertBack)} is not implemented for {nameof(IsStillPictureConverter)}");
			throw new NotImplementedException();
		}

		#endregion
	}
}
