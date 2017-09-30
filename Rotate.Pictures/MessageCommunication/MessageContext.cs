

namespace Rotate.Pictures.MessageCommunication
{
	/// <summary>
	/// Message Context the suffix of the message is the recepient
	/// See Register message:
	///			Messenger.DefaultMessenger.Register<message-type>(this /* usually = recepient */, message-instance, context /* entry in this class */);		//</message-type>
	/// and
	///			Messenter.DefaultMessenger.Send(message-instance, context /* matching the one in the Register counterpart */);
	/// 
	/// <remarks>
	///		I prefer to use the context for all message transmission because there can only be one message registered to a recepient with context == null.
	/// </remarks>
	/// </summary>
	public static class MessageContext
	{
		public static readonly string CloseBufferDepth = "CloseBufferDepth MainViewModel";
		public static readonly string CloseFileTypes = "CloseFileTypes MainViewModel";
		public static readonly string CloseIntervalBetweenPictures = "CloseIntervalBetweenPictureMessage MainViewModel";
		public static readonly string CloseStretchMode = "CloseStretchMode MainViewModel";
		public static readonly string SetStretchMode = "SetStretchMode MainViewModel";
		public static readonly string SetIntervalBetweenPictures = "SetIntervalBetweenPictures MainViewModel";
		public static readonly string SetMetadata = "SelectedBufferDepth MainViewModel";
		public static readonly string BufferDepth = "BufferDepth MainViewModel";
		public static readonly string BackImageCommand = "BackImageCommand MainViewModel";
		public static readonly string NextImageCommand = "NextImageCommand MainViewModel";
		public static readonly string MotionValueChanged = "MotionValueChanged MainViewModel";
		public static readonly string DragMotionStarted = "DragMotionStarted MainViewModel";
		public static readonly string DragMotionCompleted = "DragMotionCompleted MainViewModel";

		public static readonly string SelectedBufferDepth = "BufferDepth PictureBufferDepthService";
		public static readonly string SelectedMetadataViewModel = "SelectedBufferDepth FileTypesToRotateViewModel";
		public static readonly string SelectedIntervalViewModel = "SelectedInterval IntervalBetweenPicturesViewModel";
		public static readonly string SelectedStretchModeViewModel = "SelectedStretchMode StretchDialogViewModel";
	}
}
