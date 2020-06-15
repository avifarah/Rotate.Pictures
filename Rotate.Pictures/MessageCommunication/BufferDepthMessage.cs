

namespace Rotate.Pictures.MessageCommunication
{
	/// <summary>
	/// Purpose:
	///		Communicate payload of buffer depth.
	///		This data gets/sets the buffer of pictures to go back through history.
	/// </summary>
	public sealed class BufferDepthMessage : IVmCommunication
	{
		public int BufferDepth { get; }

		public BufferDepthMessage(int bufferDepth) => BufferDepth = bufferDepth;
	}
}
