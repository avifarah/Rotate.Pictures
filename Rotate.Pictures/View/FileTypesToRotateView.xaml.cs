using System;
using System.Windows;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures.View
{
	/// <summary>
	/// Interaction logic for FileTypesToRotateView.xaml
	/// </summary>
	public partial class FileTypesToRotateView : Window
	{
		public FileTypesToRotateView()
		{
			InitializeComponent();
			MouseLeftButtonDown += (sender, args) => DragMove();

			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Owner = Application.Current.MainWindow;

			var vm = VmFactory.Inst.CreateVm(this);
			DataContext = vm;
		}
	}
}
