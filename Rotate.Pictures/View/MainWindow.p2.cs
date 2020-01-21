using System.Windows;

namespace Rotate.Pictures.View
{
	public partial class MainWindow
	{
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property == Window.WindowStateProperty)
			{
				var oldState = (WindowState)e.OldValue;
				OldWindowSizeState.Text = oldState.ToString();

				var newState = (WindowState)e.NewValue;
				WindowSizeState.Text = newState.ToString();
			}
		}
    }
}
