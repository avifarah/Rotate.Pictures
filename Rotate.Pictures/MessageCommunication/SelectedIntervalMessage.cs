

namespace Rotate.Pictures.MessageCommunication
{
	public sealed class SelectedIntervalMessage : IVmCommunication
	{
		public float SelectedInterval { get; }

		public SelectedIntervalMessage(int selectedInterval) => SelectedInterval = (float)selectedInterval / 1000.0F;
	}
}
