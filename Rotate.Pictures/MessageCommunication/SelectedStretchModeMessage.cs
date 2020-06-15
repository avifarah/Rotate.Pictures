using Rotate.Pictures.Utility;

namespace Rotate.Pictures.MessageCommunication
{
	/// <summary>
	/// Purpose:
	///		Communication payload for the stretch mode of the display
	/// </summary>
	public sealed class SelectedStretchModeMessage : IVmCommunication
	{
		public SelectedStretchMode Mode { get; }

		public SelectedStretchModeMessage(SelectedStretchMode mode) => Mode = mode;
	}
}
