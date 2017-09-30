using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.View;


namespace Rotate.Pictures.Service
{
	/// <summary>
	/// Show IntervalBetweenPictures dialog
	/// </summary>
	public sealed class IntervalBetweenPicturesService : DialogService
	{
		public IntervalBetweenPicturesService() : base(() => new IntervalBetweenPicturesView()) { }

		public override void ShowDetailDialog(object param)
		{
			WinDialog = WinCreate();

			Messenger<SelectedIntervalMessage>.DefaultMessenger.Send(new SelectedIntervalMessage((int)param), MessageContext.SelectedIntervalViewModel);

			WinDialog?.ShowDialog();
		}
	}
}
