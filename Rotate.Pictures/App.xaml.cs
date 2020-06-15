using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public App()
		{
			log4net.Config.XmlConfigurator.Configure();

			SetupExceptionHandling();
		}

		private void SetupExceptionHandling()
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += UnhandledExceptionHandler;

			DispatcherUnhandledException += (s, e) => {
				Log.Error("Application.Current.DispatcherUnhandledException", e.Exception);
				e.Handled = true;
			};

			TaskScheduler.UnobservedTaskException += (s, e) => {
				Log.Error("TaskScheduler.UnobservedTaskException", e.Exception);
				e.SetObserved();
			};
		}

		static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
		{
			var e = (Exception)args.ExceptionObject;
			Log.Error(e.ExceptionFullMessageWithStack());
		}
	}
}
