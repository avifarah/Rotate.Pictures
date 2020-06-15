using System;
using System.Collections.Generic;
using System.Linq;

namespace Rotate.Pictures.Utility
{
	/// <summary>
	/// Purpose:
	///		Retrieve all exceptions of a deeply nested exception
	/// </summary>
	public static class ExceptionExt
	{
		public static string ExceptionFullMessage(this Exception exception)
		{
			var message = string.Join(Environment.NewLine, exception.GetExceptions().Select((e, i) => $"{i,3}.  {e.Message}"));
			return message;
		}

		public static string ExceptionFullMessageWithStack(this Exception exception)
		{
			var message = string.Join($"{Environment.NewLine}{Environment.NewLine}{new string('-', 25)}",
				exception.GetExceptions().Select((e, i) => $"{i,3}.  {e.Message}{Environment.NewLine}{e.StackTrace}"));
			return message;
		}

		private static IEnumerable<Exception> GetExceptions(this Exception exception)
		{
			for (var iEx = exception; iEx != null; iEx = iEx.InnerException)
			{
				if (iEx is AggregateException ax)
				{
					var fx = ax.Flatten();
					foreach (var e in fx.InnerExceptions)
						yield return e;
				}
				else
					yield return iEx;
			}
		}
	}
}
