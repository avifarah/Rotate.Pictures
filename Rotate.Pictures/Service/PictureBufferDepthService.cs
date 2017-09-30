using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.View;


namespace Rotate.Pictures.Service
{
	/// <summary>
	/// Show Buffer dialog
	/// </summary>
	public class PictureBufferDepthService : DialogService
	{
		public PictureBufferDepthService() : base(() => new BufferDepthView()) { }

		#region Overrides of DialogService

		public override void ShowDetailDialog(object param)
		{
			WinDialog = WinCreate();

			var depth = (int)param;
			Messenger<BufferDepthMessage>.DefaultMessenger.Send(new BufferDepthMessage(depth), MessageContext.SelectedBufferDepth);

			WinDialog?.ShowDialog();
		}

		#endregion
	}
}
