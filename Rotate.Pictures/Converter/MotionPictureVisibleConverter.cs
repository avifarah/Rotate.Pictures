using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures.Converter
{
	public class MotionPictureVisibleConverter : IValueConverter
	{
		private static List<string> _motionExtensions;

		public MotionPictureVisibleConverter() => _motionExtensions = ConfigValue.Inst.MotionPictures();

		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var fn = value as string;
			if (string.IsNullOrWhiteSpace(fn)) return false;
			var rc = _motionExtensions.Any(s => string.Compare(new FileInfo(fn).Extension, s, StringComparison.OrdinalIgnoreCase) == 0);
			return rc ? "Visible" : "Collapsed";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

		#endregion
	}
}
