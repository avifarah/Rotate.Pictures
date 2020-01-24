using System;
using System.Reflection;
using System.Windows;


namespace Rotate.Pictures.Service
{
	/// <summary>
	/// Dialog service used as a base class for showing and closing a dialog
	/// </summary>
	public abstract class DialogService
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected readonly Func<Window> WinCreate;

		protected Window WinDialog;

		protected DialogService(Func<Window> windowCreate)
		{
			if (windowCreate == null)
			{
				Log.Error("Window creation had a null lambda");
				throw new ArgumentException(@"Window creation lambda cannot be null", nameof(windowCreate));
			}

			WinCreate = windowCreate;
		}

		public virtual void ShowDetailDialog()
		{
			WinDialog = WinCreate();
			WinDialog?.ShowDialog();
		}

		public virtual void ShowDetailDialog(object param)
		{
			WinDialog = WinCreate();
			WinDialog?.ShowDialog();
		}

		public virtual void CloseDetailDialog() => WinDialog?.Close();
	}
}
