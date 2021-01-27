using System;
using System.IO;
using System.Reflection;
using log4net;

namespace Rotate.Pictures.Utility
{
	public static class FileExt
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static bool IsLocked(this string filePath)
		{
			var fileInfo = new FileInfo(filePath);
			return fileInfo.IsLocked();
		}

		public static bool IsLocked(this FileInfo fileInfo)
		{
			var filePath = fileInfo.FullName;
			try
			{
				var fs = File.OpenWrite(filePath);
				fs.Close();
				return false;
			}
			catch (Exception ex)
			{
				Log.Info($"File: \"{filePath}\" is locked.", ex);
				return true;
			}
		}
	}
}
