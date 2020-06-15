using System.Reflection;
using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.View;
using MessageBox = System.Windows.MessageBox;

namespace Rotate.Pictures.Service
{
	public class NoDisplayPictureService : DialogService
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public NoDisplayPictureService() : base(() => new NoDisplayPictureView()) { }

		#region Overrides of DialogService

		public override void ShowDetailDialog(object param)
		{
			// Set WinDialog
			WinDialog = WinCreate();

			if (param == null)
			{
				const string errMsg = "No display information is provided.  Cannot proceed";
				Log.Error(errMsg);
				MessageBox.Show(errMsg, "No Display picture");
				WinDialog.Close();
				return;
			}

			var noDisplayParam = (NoDisplayPicturesMessage.NoDisplayParam)param;
			Messenger<NoDisplayPicturesMessage>.Instance.Send(new NoDisplayPicturesMessage(noDisplayParam), MessageContext.NoDisplayPicture);

			ShowDetailDialog();
		}

		#endregion
	}
}
