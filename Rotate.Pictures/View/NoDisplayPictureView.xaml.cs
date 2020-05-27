using System.Windows;
using Rotate.Pictures.Utility;

namespace Rotate.Pictures.View
{
	/// <summary>
	/// Interaction logic for NoDisplayPictureView.xaml
	/// </summary>
	public partial class NoDisplayPictureView : Window
	{
		public NoDisplayPictureView()
		{
			InitializeComponent();

			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Owner = Application.Current.MainWindow;

			var vm = VmFactory.Inst.CreateVm(this);
			DataContext = vm;
		}
	}
}
