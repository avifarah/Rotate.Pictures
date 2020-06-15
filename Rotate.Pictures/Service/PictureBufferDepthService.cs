using System.Reflection;
using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.View;


namespace Rotate.Pictures.Service
{
	/// <summary>
	/// Show Buffer dialog
	/// </summary>
	public class PictureBufferDepthService : DialogService
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public PictureBufferDepthService() : base(() => new BufferDepthView()) { }

		#region Overrides of DialogService

		public override void ShowDetailDialog(object param)
		{
			// Set WinDialog as first thing
			WinDialog = WinCreate();

			var depth = (int)param;
			Messenger<BufferDepthMessage>.Instance.Send(new BufferDepthMessage(depth), MessageContext.SelectedBufferDepth);

			ShowDetailDialog();
		}

		#endregion
	}
}
