

namespace Rotate.Pictures.MessageCommunication
{
	/// <summary>
	/// Purpose:
	///		Communication payload of the interval between pictures in seconds
	/// </summary>
	public sealed class SetIntervalMessage : IVmCommunication
	{
		public int SetInterval { get; }

		public SetIntervalMessage(float interval) => SetInterval = (int)(interval * 1000.0F);
	}
}
