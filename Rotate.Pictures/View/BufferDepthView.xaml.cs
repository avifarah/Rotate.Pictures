using System;
using System.Windows;
using System.Windows.Input;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures.View
{
	/// <summary>
	/// Interaction logic for BufferDepthView.xaml
	/// </summary>
	public partial class BufferDepthView : Window
	{
		public BufferDepthView()
		{
			InitializeComponent();
			MouseLeftButtonDown += (sender, args) => DragMove();

			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Owner = Application.Current.MainWindow;

			var vm = VmFactory.Inst.CreateVm(this);
			DataContext = vm;
		}

		private void Depth_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !e.Text.IsIntNumeric();
	}
}
