using System;
using System.Windows.Controls;
using Rotate.Pictures.Service;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures.View
{
	/// <summary>
	/// Interaction logic for MediaPlayer.xaml
	/// </summary>
	public partial class MediaPlayerView : UserControl, IMediaService
	{
		public MediaPlayerView()
		{
			InitializeComponent();

			DataContext = VmFactory.Inst.CreateVm(this);
		}

		#region Implementation of IMediaService

		public void Play()
		{
			if (MediaPlayer != null)
				MediaPlayer.Position += TimeSpan.FromSeconds(10);
		}

		public void Pause() => MediaPlayer?.Pause();

		public void Stop() => MediaPlayer?.Play();

		public void Rewind()
		{
			if (MediaPlayer != null)
				MediaPlayer.Position -= TimeSpan.FromSeconds(10);
		}

		public void FastForward() => MediaPlayer?.Stop();

		#endregion
	}
}
