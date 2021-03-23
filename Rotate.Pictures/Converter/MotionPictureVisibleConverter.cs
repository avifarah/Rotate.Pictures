using System;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Rotate.Pictures.Utility;

namespace Rotate.Pictures.Converter
{
	/// <summary>
	/// Purpose:
	///		XAML code needs to determine if motion picture is to be displayed or still picture,
	///		They will both occupy the same space and one will be collapsed.  This class,
	///		MotionPictureVisibleConverter, will display or collapse the motion picture.
	/// </summary>
	public class MotionPictureVisibleConverter : IValueConverter
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<string> _motionExtensions;

		public MotionPictureVisibleConverter()
		{
			var configValue = ConfigValueProvider.Default;
			_motionExtensions = configValue.MotionPictures();
		}

		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var fn = value as string;
			if (string.IsNullOrWhiteSpace(fn)) return false;
			var rc = _motionExtensions.Any(s => string.Compare(new FileInfo(fn).Extension, s, StringComparison.OrdinalIgnoreCase) == 0);
			//Debug.WriteLine($"MotionPictureVisibleConverter.  Value: {value}");
			return rc ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Log.Error($"{nameof(ConvertBack)} is not implemented for {nameof(MotionPictureVisibleConverter)}");
			throw new NotImplementedException();
		}

		#endregion
	}
}
