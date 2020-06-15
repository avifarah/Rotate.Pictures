using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Rotate.Pictures.Utility
{
	/// <summary>
	/// Purpose:
	///		return stack trace
	/// </summary>
	public static class DebugStackTrace
	{
		public static IEnumerable<string> GetStackFrames(int depthLimit = 7)
		{
			var st = new StackTrace(true);
			for (var i = 1; i < Math.Min(depthLimit, st.FrameCount); i++)
			{
				var sf = st.GetFrame(i);
				var fn = sf.GetFileName();
				var ln = sf.GetFileLineNumber();
				var m = sf.GetMethod()?.Name;
				var paramsInfo = sf.GetMethod()?.GetParameters();

				if (string.IsNullOrEmpty(fn)) continue;

				var parms = paramsInfo == null
					? "<NoParameters>"
					: string.Join(",", paramsInfo.Select(pi => $"{pi.ParameterType.Name} {pi.Name}").ToArray());

				var lineNumber = ln == 0 ? "<NoLineNumber>" : ln.ToString();
				var methodName = string.IsNullOrEmpty(m) ? "<NoMethodName>" : m;
				var parmNames = string.IsNullOrEmpty(parms) ? "<NoParameters>" : parms;
				yield return $"{i,3}.  [{fn}][{lineNumber}] {methodName}({parmNames}) [column: {sf.GetFileColumnNumber()}]";
			}
		}

		public static string GetStackFrameString(int depthLimit = 7)
		{
			var stackFrames = GetStackFrames(depthLimit + 1);
			return string.Join(Environment.NewLine, stackFrames);
		}
	}
}
