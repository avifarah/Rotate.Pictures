using System.Reflection;
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
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


		public StretchDialogService() : base(() => new StretchModeView()) { }

		public override void ShowDetailDialog(object param)
		{
			WinDialog = WinCreate();

			var mode = (SelectedStretchMode)param;
			Messenger<SelectedStretchModeMessage>.Instance.Send(new SelectedStretchModeMessage(mode), MessageContext.SelectedStretchModeViewModel);

			ShowDetailDialog();
		}
	}
}
