using System;
using System.Collections.Generic;

namespace Rotate.Pictures.Utility
{
	/// <summary>
	/// A "Converter" between SelectedStertchMode and the representative string
	/// </summary>
	public static class StretchModeUtil
	{
		private static readonly Dictionary<string, SelectedStretchMode> AllValues;

		static StretchModeUtil()
		{
			AllValues = new Dictionary<string, SelectedStretchMode>();
			foreach (SelectedStretchMode sm in Enum.GetValues(typeof(SelectedStretchMode)))
				AllValues.Add(sm.ToString(), sm);
		}

		public static SelectedStretchMode TextToMode(this string text)
		{
			if (AllValues.ContainsKey(text)) return AllValues[text];
			return SelectedStretchMode.Uniform;
		}

		public static string ModeToText(this SelectedStretchMode mode) => Enum.GetName(typeof(SelectedStretchMode), mode);
	}
}
