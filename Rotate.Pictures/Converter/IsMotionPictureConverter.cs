using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures.Converter
{
	public class IsMotionPictureConverter : IValueConverter
	{
		private static List<string> _motionPictures;

		public IsMotionPictureConverter() => _motionPictures = ConfigValue.Inst.MotionPictures();

		#region Implementation of IValueConverter

		/// <summary>
		/// Converts file name to a boolean value depending on the extension being that of a motion picture 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns>True if extension of file name is that of motion picture</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var fileNm = value as string;
			if (string.IsNullOrWhiteSpace(fileNm)) return false;
			return _motionPictures.Any(s => string.Compare(new FileInfo(fileNm).Extension, s, StringComparison.OrdinalIgnoreCase) == 0);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

		#endregion
	}
}
