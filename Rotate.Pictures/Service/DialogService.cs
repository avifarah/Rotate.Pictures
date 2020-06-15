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

		/// <summary>
		/// When you override this method the first thing that needs to happen is: set WinDialog
		/// </summary>
		public virtual void ShowDetailDialog()
		{
			if (WinDialog == null)
			{
				const string errMsg = "WinDialog is expected to be set as first thing in the calling Dialog";
				Log.Error(errMsg);
				throw new Exception(errMsg);
			}

			try
			{
				WinDialog?.ShowDialog();
			}
			catch (Exception e)
			{
				Log.Error("Error while processing File Type dialog", e);
			}
		}

		/// <summary>
		/// When you override this method the first thing that needs to happen is: set WinDialog
		/// </summary>
		public virtual void ShowDetailDialog(object param) => ShowDetailDialog();

		public virtual void CloseDetailDialog() => WinDialog?.Close();
	}
}
