using Rotate.Pictures.Utility;


namespace Rotate.Pictures.MessageCommunication
{
	public sealed class SelectedMetadataMessage : IVmCommunication
	{
		private readonly PictureMetaDataTransmission _metadata;

		public SelectedMetadataMessage(PictureMetaDataTransmission metadata) => _metadata = metadata;

		public string PictureFlder => _metadata.PictureFolder;

		public string FirstPictureToDisplay => _metadata.FirstPictureToDisplay;

		public string StillPictureExtensions => _metadata.StillPictureExtensions;

		public string MotionPictureExtensions => _metadata.MotionPictureExtensions;
	}
}
