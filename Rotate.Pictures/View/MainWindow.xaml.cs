using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.Utility;

namespace Rotate.Pictures.View
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private bool _isMePlaying = true;
		private readonly DispatcherTimer _tmr;

		private bool _isMotionPicturePlaying;

		private readonly IConfigValue _configValue;

		public MainWindow()
		{
			InitializeComponent();

			_configValue = ConfigValueProvider.Default;

			_tmr = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_configValue.VisualHeartbeat()), IsEnabled = true };
			_tmr.Tick += VisualTimerTick;
			_tmr.Start();

			MeSliderVolume.Value = ConfigValue.Inst.MediaVolume;

			DataContext = VmFactory.Inst.CreateVm(this);
		}

		private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				NextPicture.Focus();
				e.Handled = true;
			}
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property != WindowStateProperty) return;

			var oldState = (WindowState)e.OldValue;
			OldWindowSizeState.Text = oldState.ToString();

			var newState = (WindowState)e.NewValue;
			WindowSizeState.Text = newState.ToString();
		}

		private void MePlayer_OnMediaOpened(object sender, RoutedEventArgs e)
		{
			_isMotionPicturePlaying = false;
			if (!MePlayer.NaturalDuration.HasTimeSpan) return;

			MeSliderPosition.Minimum = 0.0;
			MeSliderPosition.Maximum = MePlayer.NaturalDuration.TimeSpan.TotalSeconds;
			MeSliderPosition.Value = 0.0;
			_isMotionPicturePlaying = true;
		}

		private void VisualTimerTick(object sender, EventArgs e)
		{
			if (MePlayer.Source == null) return;
			if (!MePlayer.NaturalDuration.HasTimeSpan) return;

			if (!_isMotionPicturePlaying)
			{
				if (!MePlayer.NaturalDuration.HasTimeSpan) return;
				MeSliderPosition.Minimum = 0.0;
				MeSliderPosition.Maximum = MePlayer.NaturalDuration.TimeSpan.TotalSeconds;
				MeSliderPosition.Value = 0.0;
				_isMotionPicturePlaying = true;
			}

			MeSliderPosition.Value = MePlayer.Position.TotalSeconds;
		}

		/// <summary>
		/// Handles keyboard shortcut
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BackImageMove(object sender, ExecutedRoutedEventArgs e) =>
			Messenger<CommandRequest>.Instance.Send(new CommandRequest(), MessageContext.BackImageCommand);

		/// <summary>
		/// Handles keyboard shortcut
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NextImageMove(object sender, ExecutedRoutedEventArgs e) =>
			Messenger<CommandRequest>.Instance.Send(new CommandRequest(), MessageContext.NextImageCommand);

		private void MotionDragStarted(object sender, DragStartedEventArgs e) => _tmr.Stop();

		private void MotionDragCompleted(object sender, DragCompletedEventArgs e)
		{
			ChangeMotionPosition();
			_tmr.Start();
		}

		private void OnMotionMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_tmr.Stop();
			ChangeMotionPosition();
			_tmr.Start();
		}

		private void MediaCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = !_isMePlaying;

		private void MediaPlay(object sender, ExecutedRoutedEventArgs e)
		{
			MePlayer.LoadedBehavior = MediaState.Manual;
			MePlayer.Play();
			_isMePlaying = true;
		}

		private void MediaCanPause(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = _isMePlaying;

		private void MediaPause(object sender, ExecutedRoutedEventArgs e)
		{
			MePlayer.LoadedBehavior = MediaState.Manual;
			MePlayer.Pause();
			_isMePlaying = false;
		}

		private void MediaCanStop(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = _isMePlaying;

		private void MediaStop(object sender, ExecutedRoutedEventArgs e)
		{
			MePlayer.LoadedBehavior = MediaState.Manual;
			MePlayer.Stop();
			_isMePlaying = false;
		}

		private void MediaCanFastForward(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = _isMePlaying;

		private void MediaFastForward(object sender, ExecutedRoutedEventArgs e)
		{
			var msPosition = MePlayer.Position.TotalMilliseconds;
			MePlayer.LoadedBehavior = MediaState.Manual;
			if (!MePlayer.NaturalDuration.HasTimeSpan)
			{
				MePlayer.Pause();
				MePlayer.Position = TimeSpan.FromMilliseconds(msPosition);
				MePlayer.Play();
				if (!MePlayer.NaturalDuration.HasTimeSpan) return;
			}

			var newPosition = MePlayer.Position + TimeSpan.FromSeconds(_configValue.MediaFastForward());
			MePlayer.Position = newPosition.TotalSeconds <= MePlayer.NaturalDuration.TimeSpan.TotalSeconds ? newPosition : MePlayer.NaturalDuration.TimeSpan;
		}

		private void MediaCanRewind(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = _isMePlaying;

		private void MediaRewind(object sender, ExecutedRoutedEventArgs e)
		{
			MePlayer.LoadedBehavior = MediaState.Manual;
			var newPosition = MePlayer.Position - TimeSpan.FromSeconds(_configValue.MediaFastForward());
			MePlayer.Position = newPosition.TotalSeconds >= 0 ? newPosition : TimeSpan.FromSeconds(0.0);
		}

		private bool _isVolumeGt0 = true;
		private double _lastVolume;

		private void MediaToggleVolume(object sender, ExecutedRoutedEventArgs e)
		{
			MePlayer.LoadedBehavior = MediaState.Manual;
			if (_isVolumeGt0)
			{
				_lastVolume = MePlayer.Volume;
				MePlayer.Volume = 0;
			}
			else
				MePlayer.Volume = _lastVolume;
			_isVolumeGt0 = !_isVolumeGt0;
		}

		private void ChangeMotionPosition()
		{
			// TODO: Find a way to set MePlayer.Position from the ViewModel
			// https://stackoverflow.com/questions/22606067/mediaelement-play-from-within-viewmodel/56181630#56181630
			MePlayer.Position = TimeSpan.FromSeconds(MeSliderPosition.Value);
		}

#if DEBUG
		private void LogMePlayer([CallerMemberName] string methodName = null)
		{
			try
			{
				if (MePlayer == null) { Log.Error("MePlayer is null"); return; }

				var playerInfo = new StringBuilder($"Caller method: {methodName}{Environment.NewLine}");
				playerInfo.Append($"Position: {MePlayer.Position.TotalSeconds}.  ");
				playerInfo.Append($"Volume: {MePlayer.Volume}.  ");
				playerInfo.Append($"Balance: {MePlayer.Balance}.  ");
				playerInfo.Append($"BufferingProgress: {MePlayer.BufferingProgress}.  ");
				playerInfo.Append($"CanPause: {MePlayer.CanPause}.");
				playerInfo.AppendLine();

				if (MePlayer.Clock != null)
				{
					playerInfo.Append($"Clocks:{Environment.NewLine}\t\tTimeLine.Source: \"{MePlayer.Clock.Timeline.Source.LocalPath}\", accRatio: {MePlayer.Clock.Timeline.AccelerationRatio}, autoRev: {MePlayer.Clock.Timeline.AutoReverse},");
					playerInfo.AppendLine($"\t\tBegTIme: {(MePlayer.Clock.Timeline.BeginTime.HasValue ? MePlayer.Clock.Timeline.BeginTime.Value.TotalSeconds.ToString(CultureInfo.CurrentUICulture) : "NULL")}, canFreeze: {MePlayer.Clock.Timeline.CanFreeze}");
					playerInfo.AppendLine($"\t\tDecelerationRatio: {MePlayer.Clock.Timeline.DecelerationRatio}, DependencyObjectType: {MePlayer.Clock.Timeline.DependencyObjectType.GetType().FullName}, MePlayer.Clock.Timeline.Dispatcher, Duration: {MePlayer.Clock.Timeline.Duration.TimeSpan.TotalSeconds},");
					playerInfo.AppendLine($"\t\tMePlayer.Clock.Timeline.FillBehavior, HasAnimatedProperties: {MePlayer.Clock.Timeline.HasAnimatedProperties}, AccelerationRatio: {MePlayer.Clock.Timeline.AccelerationRatio}, AutoReverse: {MePlayer.Clock.Timeline.AutoReverse},");
					playerInfo.AppendLine($"\t\tBeginTime: {(MePlayer.Clock.Timeline.BeginTime.HasValue ? MePlayer.Clock.Timeline.BeginTime.Value.TotalSeconds.ToString(CultureInfo.CurrentUICulture) : "Null")}, IsFrozen: {MePlayer.Clock.Timeline.IsFrozen}, IsSealed: {MePlayer.Clock.Timeline.IsSealed},");
					playerInfo.AppendLine($"\t\tName: {MePlayer.Clock.Timeline.Name}");
					playerInfo.AppendLine($"\tMePlayer.Clock.Controller, CurrentGlobalSpeed: {MePlayer.Clock.CurrentGlobalSpeed},  CurrentIteration: {MePlayer.Clock.CurrentIteration},  CurrentIteration: {MePlayer.Clock.CurrentIteration}");
					playerInfo.AppendLine($"\tCurrentProgress: {MePlayer.Clock.CurrentProgress}, CurrentState: {MePlayer.Clock.CurrentState}, CurrentTime: {(MePlayer.Clock.CurrentTime.HasValue ? MePlayer.Clock.CurrentTime.Value.TotalSeconds.ToString(CultureInfo.CurrentUICulture) : "null")},");
					playerInfo.AppendLine($"\tMePlayer.Clock.Dispatcher, HasControllableRoot: {MePlayer.Clock.HasControllableRoot}, IsPaused: {MePlayer.Clock.IsPaused}, NaturalDuration: {MePlayer.Clock.NaturalDuration.TimeSpan.TotalSeconds},");
					playerInfo.AppendLine("\tMePlayer.Clock.Parent,");
				}
				playerInfo.AppendLine($"DownloadProgress: {MePlayer.DownloadProgress}, HasAudio: {MePlayer.HasAudio}, HasVideo: {MePlayer.HasVideo}, IsBuffering: {MePlayer.IsBuffering}, IsMuted: {MePlayer.IsMuted}");

				playerInfo.Append(MePlayer.NaturalDuration.HasTimeSpan
					? $"Duration: {MePlayer.NaturalDuration.TimeSpan.TotalSeconds}.  "
					: "Duration has no timespan  ");
				playerInfo.AppendLine($"LoadedBehavior: {MePlayer.LoadedBehavior}.  VideoHeight: {MePlayer.NaturalVideoHeight}.  ScrubbingEnabled: {MePlayer.ScrubbingEnabled}");

				playerInfo.Append(MePlayer.Source == null ? "MePlayer.Source is null.  " : $"Source: \"{MePlayer.Source.LocalPath}\".  ");

				playerInfo.AppendLine($"SpeedRatio: {MePlayer.SpeedRatio}.");
				playerInfo.AppendLine($"Width: {MePlayer.NaturalVideoWidth}.");
				playerInfo.AppendLine($"Stretch: {MePlayer.Stretch}.");
				playerInfo.AppendLine($"Stretch Direction: {MePlayer.StretchDirection}");
				playerInfo.AppendLine();

				Log.Info(playerInfo.ToString());
			}
			catch (Exception e)
			{
				Log.Error("LogMePlayer logging failed", e);
			}
		}
#endif

	}
}
