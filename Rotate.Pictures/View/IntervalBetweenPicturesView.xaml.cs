using System;
using System.Windows;
using System.Windows.Input;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures.View
{
	/// <summary>
	/// Interaction logic for IntervalBetweenPicturesView.xaml
	/// </summary>
	public partial class IntervalBetweenPicturesView : Window
	{
		public IntervalBetweenPicturesView()
		{
			InitializeComponent();

			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Owner = Application.Current.MainWindow;

			var vm = VmFactory.Inst.CreateVm(this);
			DataContext = vm;
		}

		private void Interval_OnPreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !e.Text.IsFloatNumeric();
	}
}
