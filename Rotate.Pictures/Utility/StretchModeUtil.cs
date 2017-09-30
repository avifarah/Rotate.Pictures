using System;


namespace Rotate.Pictures.Utility
{
	public static class StretchModeUtil
	{
		public static SelectedStretchMode TextToMode(this string text)
		{
			switch (text)
			{
				case "Fill": return SelectedStretchMode.Fill;
				case "None": return SelectedStretchMode.None;
				case "Uniform": return SelectedStretchMode.Uniform;
				case "UniformToFill": return SelectedStretchMode.UniformToFill;
				default: return SelectedStretchMode.Uniform;
			}
		}

		public static string ModeToText(this SelectedStretchMode mode)
		{
			switch (mode)
			{
				case SelectedStretchMode.Fill: return "Fill";
				case SelectedStretchMode.None: return "None";
				case SelectedStretchMode.Uniform: return "Uniform";
				case SelectedStretchMode.UniformToFill: return "UniformToFill";
				default: throw new ArgumentException(@"Stretch mode is not recognized", nameof(mode));
			}
		}
	}
}
