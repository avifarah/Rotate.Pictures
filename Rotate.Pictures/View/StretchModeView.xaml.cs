using System.Windows;
using Rotate.Pictures.Utility;

namespace Rotate.Pictures.View
{
	/// <summary>
	/// Interaction logic for StretchModeView.xaml
	/// </summary>
	public partial class StretchModeView : Window
	{
		public StretchModeView()
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

