using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;


namespace Rotate.Pictures.MessageCommunication
{
	public class CommandRequest : IVmCommunication { }

	public class PropertyChangedCommandRequest : CommandRequest
	{
		public RoutedPropertyChangedEventArgs<double> PropertyChangedEventArgs { get;  }

		public PropertyChangedCommandRequest(RoutedPropertyChangedEventArgs<double> e) => PropertyChangedEventArgs = e;
	}

	public class DragMotionStartedCommandRequest : CommandRequest
	{
		public DragStartedEventArgs StartedEventArgs { get; }

		public DragMotionStartedCommandRequest(DragStartedEventArgs e) => StartedEventArgs = e;
	}

	public class DragMotionCompletedCommandRequest : CommandRequest
	{
		public MediaElement MediaPlayer { get; }

		public Slider SliderPosition { get; }

		public DragCompletedEventArgs CompletedEventArgs { get; }

		public DragMotionCompletedCommandRequest(MediaElement player, Slider position, DragCompletedEventArgs e)
		{
			MediaPlayer = player;
			SliderPosition = position;
			CompletedEventArgs = e;
		}
	}
}
