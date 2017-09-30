using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.Utility;
using Rotate.Pictures.View;


namespace Rotate.Pictures.Service
{
	/// <summary>
	/// Show StretchDialog
	/// </summary>
	public sealed class StretchDialogService : DialogService
	{
		public StretchDialogService() : base(() => new StretchModeView()) { }

		public override void ShowDetailDialog(object param)
		{
			WinDialog = WinCreate();

			var mode = (SelectedStretchMode)param;
			Messenger<SelectedStretchModeMessage>.DefaultMessenger.Send(new SelectedStretchModeMessage(mode), MessageContext.SelectedStretchModeViewModel);

			WinDialog?.ShowDialog();
		}
	}
}
