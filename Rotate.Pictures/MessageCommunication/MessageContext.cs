
namespace Rotate.Pictures.MessageCommunication
{
	/// <summary>
	/// Purpose:
	///		Message Context is to the suffix of the message distinguishing between different recipients subscribing to the
	///		same message delivery content.
	/// 
	///		See Register message:
	///			Messenger.Instance.Register{message-type}(this/*recipient*/, message-instance, context/*entry in this class*/);
	///		and
	///			Messenger.Instance.Send(message-instance, context/*matching the one in the Register counterpart*/);
	///
	/// <remarks>
	///		The MessageContext may be any object.
	/// </remarks>
	/// </summary>
	public static class MessageContext
	{
		public static readonly object CloseBufferDepth = "CloseBufferDepth MainViewModel";
		public static readonly object CloseFileTypes = "CloseFileTypes MainViewModel";
		public static readonly object CloseIntervalBetweenPictures = "CloseIntervalBetweenPictureMessage MainViewModel";
		public static readonly object CloseStretchMode = "CloseStretchMode MainViewModel";
		public static readonly object SetStretchMode = "SetStretchMode MainViewModel";
		public static readonly object SetIntervalBetweenPictures = "SetIntervalBetweenPictures MainViewModel";
		public static readonly object SetMetadata = "SelectedBufferDepth MainViewModel";
		public static readonly object BufferDepth = "BufferDepth MainViewModel";
		public static readonly object BackImageCommand = "BackImageCommand MainViewModel";
		public static readonly object NextImageCommand = "NextImageCommand MainViewModel";
		public static readonly object NoDisplayPicture = "NoDisplayPicture MainVewModel";
		public static readonly object DeleteFromDoNotDisplay = "DoNotDisplayMessage";
		public static readonly object FromMainToDoNotDisplay = "FromMainToDoNotDisplay";
		public static readonly object MainWindowViewModel = "MainWindowViewModel";

		public static readonly object SelectedBufferDepth = "BufferDepth PictureBufferDepthService";
		public static readonly object SelectedMetadataViewModel = "SelectedBufferDepth FileTypesToRotateViewModel";
		public static readonly object SelectedIntervalViewModel = "SelectedInterval IntervalBetweenPicturesViewModel";
		public static readonly object SelectedStretchModeViewModel = "SelectedStretchMode StretchDialogViewModel";
	}
}
