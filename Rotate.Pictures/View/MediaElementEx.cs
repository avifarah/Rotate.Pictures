using System.Windows;
using System.Windows.Controls;

namespace Rotate.Pictures.View
{
	public class MediaElementEx : MediaElement
	{
		//
		// MediaPlayerPlay
		//
		/// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElementEx.PlayAlternative" /> dependency property.</summary>
		/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElementEx.PlayAlternative" /> dependency property.</returns>
		public static readonly DependencyProperty PlayAlternativeProperty = DependencyProperty.Register(
			nameof(PlayAlternative),
			typeof(MediaState),
			typeof(MediaElementEx),
			new FrameworkPropertyMetadata(MediaState.Play, FrameworkPropertyMetadataOptions.None, PlayAlternativePropertyChanged));

		/// <summary>Gets or sets the PlayAlternative and returns MediaState <see cref="T:System.Windows.Controls.MediaState" />. </summary>
		/// <returns>The load behavior <see cref="T:System.Windows.Controls.MediaState" /> set for the media. The default value is <see cref="F:System.Windows.Controls.MediaState.MediaPlayerPlay" />.</returns>
		public MediaState PlayAlternative
		{
			get => (MediaState)GetValue(PlayAlternativeProperty);
			set => SetValue(PlayAlternativeProperty, value);
		}

		private static void PlayAlternativePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MediaElement)d)?.Play();
		}

		//
		// Pause
		//
		public static readonly DependencyProperty PauseAlternativeProperty = DependencyProperty.Register(
			nameof(PauseAlternative),
			typeof(MediaState),
			typeof(MediaElementEx),
			new FrameworkPropertyMetadata(MediaState.Pause, FrameworkPropertyMetadataOptions.None, PauseAlternativePropertyChanged));

		public MediaState PauseAlternative
		{
			get => (MediaState)GetValue(PauseAlternativeProperty);
			set => SetValue(PauseAlternativeProperty, value);
		}

		private static void PauseAlternativePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MediaElement)d)?.Pause();
		}

		//
		// Stop
		//
		public static readonly DependencyProperty StopAlternativeProperty = DependencyProperty.Register(
			nameof(StopAlternative),
			typeof(MediaState),
			typeof(MediaElementEx),
			new FrameworkPropertyMetadata(MediaState.Stop, FrameworkPropertyMetadataOptions.None, StopAlternativePropertyChanged));

		public MediaState StopAlternative
		{
			get => (MediaState)GetValue(StopAlternativeProperty);
			set => SetValue(StopAlternativeProperty, value);
		}

		private static void StopAlternativePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MediaElement)d)?.Stop();
		}

		///// <summary>Stops and resets media to be played from the beginning.</summary>
		///// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.MediaElement.Clock" /> property is not <see langword="null" />.</exception>
		//public void Stop() => this._helper.SetState(MediaState.Stop);

		///// <summary>Closes the media.</summary>
		//public void Close() => this._helper.SetState(MediaState.Close);
	}
}
