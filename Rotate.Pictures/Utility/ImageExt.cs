using System.IO;
using System.Reflection;

namespace Rotate.Pictures.Utility
{
	/// <summary>
	/// Purpose:
	///		Check the validity of an image be it a still image or a motion image
	/// </summary>
	public static class ImageExt
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static bool IsPictureValidFormat(this string fileName)
		{
			if (!File.Exists(fileName)) return false;
			var fi = new FileInfo(fileName);
			return fi.Length != 0;
		}
	}
}

