using System;
using System.Collections.Generic;
using System.Linq;


namespace Rotate.Pictures.Utility
{
	public static class ExceptionUtil
	{
		public static string ExceptionMessages(this Exception ex)
		{
			string ExtractMessage(Exception e)
			{
				var ae = e as AggregateException;
				return $"{ae?.Flatten().Message ?? e.Message}{Environment.NewLine}";
			}

			var messages = string.Join(Environment.NewLine, ex.GetInnerExceptions().Select(ExtractMessage));
			return messages;
		}

		public static string ExceptionMessageAndStackTrace(this Exception ex)
		{
			string ExtractMessageAndStackTrace(Exception e)
			{
				var ae = e as AggregateException;
				return $"{ae?.Flatten().Message ?? e.Message}{Environment.NewLine}{e.StackTrace}{Environment.NewLine}";
			}

			var msg = string.Join(Environment.NewLine, ex.GetInnerExceptions().Select(ExtractMessageAndStackTrace));
			return msg;
		}

		private static IEnumerable<Exception> GetInnerExceptions(this Exception ex)
		{
			for (var e = ex; e != null; e = e.InnerException)
				yield return e;
		}
	}
}
