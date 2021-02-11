using System.Reflection;
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
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public FileTypeToRotateService() : base(() => new FileTypesToRotateView()) { }

		#region Overrides of DialogService

		public override void ShowDetailDialog(object param)
		{
			// Set WinDialog
			WinDialog = WinCreate();

			var metadata = param as PictureMetaDataTransmission;
			Messenger<SelectedMetadataMessage>.Instance.Send(new SelectedMetadataMessage(metadata), MessageContext.SelectedMetadataViewModel);

			base.ShowDetailDialog();
		}

		#endregion
	}
}
