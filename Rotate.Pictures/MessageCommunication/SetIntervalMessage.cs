

namespace Rotate.Pictures.MessageCommunication
{
	public sealed class SetIntervalMessage : IVmCommunication
	{
		public int SetInterval { get; }

		public SetIntervalMessage(float interval) => SetInterval = (int)(interval * 1000.0F);
	}
}
