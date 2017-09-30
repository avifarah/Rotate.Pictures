using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.Utility;
using Rotate.Pictures.View;


namespace Rotate.Pictures.Service
{
	/// <summary>
	/// Show MetaData Dialog
	/// </summary>
	public sealed class FileTypeToRotateService : DialogService
	{
		public FileTypeToRotateService() : base(() => new FileTypesToRotateView()) { }

		#region Overrides of DialogService

		public override void ShowDetailDialog(object param)
		{
			WinDialog = WinCreate();

			var metadata = param as PictureMetaDataTransmission;
			Messenger<SelectedMetadataMessage>.DefaultMessenger.Send(new SelectedMetadataMessage(metadata), MessageContext.SelectedMetadataViewModel);

			WinDialog?.ShowDialog();
		}

		#endregion
	}
}
