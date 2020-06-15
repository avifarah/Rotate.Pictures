using System.Reflection;
using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.View;


namespace Rotate.Pictures.Service
{
	/// <summary>
	/// Show IntervalBetweenPictures dialog
	/// </summary>
	public sealed class IntervalBetweenPicturesService : DialogService
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public IntervalBetweenPicturesService() : base(() => new IntervalBetweenPicturesView()) { }

		public override void ShowDetailDialog(object param)
		{
			// Set WinDialog as first thing
			WinDialog = WinCreate();

			Messenger<SelectedIntervalMessage>.Instance.Send(new SelectedIntervalMessage((int)param), MessageContext.SelectedIntervalViewModel);

			base.ShowDetailDialog();
		}
	}
}
