

namespace Rotate.Pictures.MessageCommunication
{
	/// <summary>
	/// Purpose:
	///		Communication payload interval between pictures taken from seconds to milliseconds
	/// </summary>
	public sealed class SelectedIntervalMessage : IVmCommunication
	{
		public float SelectedInterval { get; }

		public SelectedIntervalMessage(int selectedInterval) => SelectedInterval = selectedInterval / 1000.0F;
	}
}
