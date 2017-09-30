

namespace Rotate.Pictures.MessageCommunication
{
	public sealed class BufferDepthMessage : IVmCommunication
	{
		public int BufferDepth { get; }

		public BufferDepthMessage(int bufferDepth) => BufferDepth = bufferDepth;
	}
}
