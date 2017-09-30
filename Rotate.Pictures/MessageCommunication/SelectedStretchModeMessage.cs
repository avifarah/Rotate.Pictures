using Rotate.Pictures.Utility;


namespace Rotate.Pictures.MessageCommunication
{
	public sealed class SelectedStretchModeMessage : IVmCommunication
	{
		public SelectedStretchMode Mode { get; }

		public SelectedStretchModeMessage(SelectedStretchMode mode) => Mode = mode;
	}
}
