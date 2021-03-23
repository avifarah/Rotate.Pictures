using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Rotate.Pictures.Utility;

namespace Rotate.Pictures.Converter
{
	public sealed class LocalStillPictureVisibleConverter : IValueConverter
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<string> _stillPictures;

		public LocalStillPictureVisibleConverter()
		{
			var configValue = ConfigValueProvider.Default;
			_stillPictures = configValue.StillPictureExtensions();
		}

		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var fileNm = value as string;
			if (string.IsNullOrWhiteSpace(fileNm)) return false;
			var isStillPic = _stillPictures.Any(s => string.Compare(new FileInfo(fileNm).Extension, s, StringComparison.OrdinalIgnoreCase) == 0);
			//Debug.WriteLine($"LocalStillPictureVisibleConverter.   Value: {value}");
			return isStillPic ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Log.Error($"{nameof(ConvertBack)} is not implemented for {nameof(LocalStillPictureVisibleConverter)}");
			throw new NotImplementedException();
		}

		#endregion
	}
}
