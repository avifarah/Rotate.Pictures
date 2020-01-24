using System;


namespace Rotate.Pictures.Utility
{
	public static class StretchModeUtil
	{
		public static SelectedStretchMode TextToMode(this string text)
		{
			return text switch {
				"Fill" => SelectedStretchMode.Fill,
				"None" => SelectedStretchMode.None,
				"Uniform" => SelectedStretchMode.Uniform,
				"UniformToFill" => SelectedStretchMode.UniformToFill,
				_ => SelectedStretchMode.Uniform,
			};
		}

		public static string ModeToText(this SelectedStretchMode mode)
		{
			return mode switch {
				SelectedStretchMode.Fill => "Fill",
				SelectedStretchMode.None => "None",
				SelectedStretchMode.Uniform => "Uniform",
				SelectedStretchMode.UniformToFill => "UniformToFill",
				_ => throw new ArgumentException(@"Stretch mode is not recognized", nameof(mode))
			};
		}
	}
}
