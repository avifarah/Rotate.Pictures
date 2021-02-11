using Rotate.Pictures.Utility;

namespace Rotate.Pictures.MessageCommunication
{
	/// <summary>
	/// Purpose:
	///		Communication payload the metadata to the pictures
	/// </summary>
	public sealed class SelectedMetadataMessage : IVmCommunication
	{
		private readonly PictureMetaDataTransmission _metadata;

		public SelectedMetadataMessage(PictureMetaDataTransmission metadata) => _metadata = metadata;

		public string PictureFolder => _metadata.PictureFolder;

		public string FirstPictureToDisplay => _metadata.FirstPictureToDisplay;

		public string StillPictureExtensions => _metadata.StillPictureExtensions;

		public string MotionPictureExtensions => _metadata.MotionPictureExtensions;
	}
}
