using System.Collections.Generic;
using System.Windows.Forms;
using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.View;
using MessageBox = System.Windows.MessageBox;

namespace Rotate.Pictures.Service
{
	public class NoDisplayPictureService : DialogService
	{
		public NoDisplayPictureService() : base(() => new NoDisplayPictureView()) { }

		#region Overrides of DialogService

		public override void ShowDetailDialog(object param)
		{
			WinDialog = WinCreate();

			if (param == null)
			{
				MessageBox.Show("No display information is provided.  Cannot proceed", "No Display picture");
				WinDialog.Close();
				return;
			}

			var noDisplayParam = (NoDisplayPicturesMessage.NoDisplayParam)param;
			Messenger<NoDisplayPicturesMessage>.DefaultMessenger.Send(new NoDisplayPicturesMessage(noDisplayParam), MessageContext.NoDisplayPicture);
			WinDialog?.ShowDialog();
		}

		#endregion
	}
}
