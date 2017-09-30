using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Rotate.Pictures.Converter;


namespace Rotate.Pictures.Utility
{
	public static class StringUtil
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly List<string> Trues = new List<string> { "True", "T", "OK", "K", "Yes", "Y", "Positive", "P", "+", "1" };
		private static readonly List<string> Falses = new List<string> { "False", "F", "No", "N", "Negative", "-", "0" };

		public static bool IsTrue(this string text) => Trues.Any(t => string.Compare(text, t, StringComparison.OrdinalIgnoreCase) == 0);

		public static bool IsFalse(this string text) => Falses.Any(f => string.Compare(text, f, StringComparison.OrdinalIgnoreCase) == 0);

		private static readonly Regex FloatNumericRegex = new Regex(@"[0-9\.\,]+");

		public static bool IsFloatNumeric(this string text) => FloatNumericRegex.IsMatch(text);

		private static readonly Regex IntNumericRegex = new Regex(@"[0-9]+");

		public static bool IsIntNumeric(this string text) => IntNumericRegex.IsMatch(text);

		public static bool IsMotionPicture(this string fileName)
		{
			var motionConverter = new IsMotionPictureConverter();
			var rc = motionConverter.Convert(fileName, typeof(bool), null, CultureInfo.CurrentUICulture);
			return (bool)(rc ?? false);
		}
	}
}
