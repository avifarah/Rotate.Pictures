using System;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Rotate.Pictures.Utility;
using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.Model;
using Rotate.Pictures.Service;


namespace Rotate.Pictures.ViewModel
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly DispatcherTimer _picChangeTmr;
		private readonly DispatcherTimer _visualHeartbeatTmr;
		private readonly PictureModel _model;
		private bool _userIsDraggingMotionPicSlider;

		private readonly StretchDialogService _stretchSvc;
		private readonly IntervalBetweenPicturesService _intervalBetweenPicturesService;
		private readonly FileTypeToRotateService _pictureMetadataService;
		private readonly PictureBufferDepthService _pictureBufferService;

		public MainWindowViewModel()
		{
			_model = (PictureModel)ModelFactory.Inst.Create("PictureFileRepository");

			_stretchSvc = new StretchDialogService();
			_intervalBetweenPicturesService = new IntervalBetweenPicturesService();
			_pictureMetadataService = new FileTypeToRotateService();
			_pictureBufferService = new PictureBufferDepthService();

			_pic = ConfigValue.Inst.FirstPictureToDisplay();
			_imgStretch = ConfigValue.Inst.ImageStretch();
			_initialRotationMode = ConfigValue.Inst.RotatingPicturesInit();
			_rotationRunning = _initialRotationMode;

			_picChangeTmr = new DispatcherTimer {
				Interval = TimeSpan.FromMilliseconds(IntervalBetweenPictures),
				IsEnabled = RotationRunning
			};
			_picChangeTmr.Tick += (sender, args) => RetrieveNextPicture();

			_visualHeartbeatTmr = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ConfigValue.Inst.VisualHeartbeat()), IsEnabled = true };
			_visualHeartbeatTmr.Tick += VisualHeartBeatUpdate;

			SelectionTracker.Inst.Append(_pic);

			if (_pic == null && !File.Exists(_pic))
			{
				bool succeeded = false;
				for (var i = 0; i < 100 && !succeeded; ++i)
				{
					var t = Task.Delay(10);
					t.Wait();
					try
					{
						RetrieveNextPicture();
						succeeded = true;
					}
					catch { succeeded = false; }
				}
			}

			LoadCommands();
			RegisterMessages();
		}

		private string _pic;

		public string CurrentPicture
		{
			get => _pic;
			set
			{
				_pic = value;
				OnPropertyChanged();
				IsMotionRunning = _pic.IsMotionPicture();
			}
		}

		private string _imgStretch;

		public string ImageStretch
		{
			get => _imgStretch;
			set
			{
				_imgStretch = value;
				OnPropertyChanged();
			}
		}

		private bool _rotationRunning;

		public bool RotationRunning
		{
			get => _rotationRunning;
			set
			{
				_rotationRunning = value;
				OnPropertyChanged();
				if (_picChangeTmr != null) _picChangeTmr.IsEnabled = _rotationRunning;
				SliderVisibility = _rotationRunning ? "Visible" : "Collapsed";
				if (_picChangeTmr != null && IsWindowStateMinimized(WindowSizeState))
				{
					if (_rotationRunning)
					{
						_picChangeTmr.Start();
						ResetHeartBeat();
					}
					else
						_picChangeTmr.Stop();
				}

				_visualHeartbeatTmr.IsEnabled = _rotationRunning;
				CurrentPictureColumnSpan = _rotationRunning ? 1 : 2;
			}
		}

		private string _sliderVisibility;

		public string SliderVisibility
		{
			get => _sliderVisibility;
			set
			{
				_sliderVisibility = value;
				OnPropertyChanged();
			}
		}

		private double _motionPicturePosition;

		public double MotionPicturePosition
		{
			get => _motionPicturePosition;
			set
			{
				_motionPicturePosition = value;
				OnPropertyChanged();
			}
		}

		private WindowState _windowSizeState;

		public WindowState WindowSizeState
		{
			get => _windowSizeState;
			set
			{
				_windowSizeState = value;
				if (!IsWindowStateMinimized(_windowSizeState) && IsWindowStateMinimized(_oldWindowSizeState)) ResetHeartBeat();
				OnPropertyChanged();
			}
		}

		private WindowState _oldWindowSizeState;

		public WindowState OldWindowSizeState
		{
			get => _oldWindowSizeState;
			set
			{
				_oldWindowSizeState = value;
				if (!IsWindowStateMinimized(_windowSizeState) && IsWindowStateMinimized(_oldWindowSizeState)) ResetHeartBeat();
				OnPropertyChanged();
			}
		}

		private int _intervalBetweenPictures = ConfigValue.Inst.IntervalBetweenPictures();

		public int IntervalBetweenPictures
		{
			get => _intervalBetweenPictures;
			set
			{
				if (value <= 0) return;
				_intervalBetweenPictures = value;
				_picChangeTmr.Interval = TimeSpan.FromMilliseconds(_intervalBetweenPictures);
				OnPropertyChanged();

				if (!RotationRunning) return;
				if (IsWindowStateMinimized(WindowSizeState)) return;

				_picChangeTmr.Stop();
				_picChangeTmr.Start();
				ResetHeartBeat();
			}
		}

		private bool _initialRotationMode;

		public bool InitialRotationMode
		{
			get => _initialRotationMode;
			set
			{
				_initialRotationMode = value;
				ConfigValue.Inst.UpdateOnStartRotatingPicture(_initialRotationMode);
				OnPropertyChanged();
			}
		}

		private int _visHeartBeatValue;
		private double _intervalBetweenPics;

		public int VisHeartBeatValue
		{
			get => _visHeartBeatValue;
			set
			{
				_visHeartBeatValue = value;
				OnPropertyChanged();
			}
		}

		private int _intervalProgressBarMax = 1000;

		public int IntervalProgressBarMax
		{
			get => _intervalProgressBarMax;
			set
			{
				_intervalProgressBarMax = value;
				OnPropertyChanged();
			}
		}

		private int _currentPictureColumnSpan = 1;

		public int CurrentPictureColumnSpan
		{
			get => _currentPictureColumnSpan;
			set
			{
				_currentPictureColumnSpan = value;
				OnPropertyChanged();
			}
		}

		private bool _isMotionRunning;

		public bool IsMotionRunning
		{
			get => _isMotionRunning;
			set
			{
				_isMotionRunning = value;
				OnPropertyChanged();
			}
		}

		private void ChangePic(object sender, System.Timers.ElapsedEventArgs e) => RetrieveNextPicture();

		private void RetrieveNextPicture()
		{
			if (WindowSizeState == WindowState.Minimized) return;

			var pic = _model.GetNextPicture();
			if (pic == null) return;
			CurrentPicture = pic;
			SelectionTracker.Inst.Append(_pic);
			ResetHeartBeat();
		}

		private void VisualHeartBeatUpdate(object sender, EventArgs e)
		{
			var cnt = (double)IntervalBetweenPictures / ConfigValue.Inst.VisualHeartbeat();
			_intervalBetweenPics += (double)_intervalProgressBarMax / (cnt - 1);
			VisHeartBeatValue = (int)_intervalBetweenPics;
		}

		private void ResetHeartBeat()
		{
			_intervalBetweenPics = 0.0;
			_visualHeartbeatTmr.Start();
		}

		public void MotionValueChanged(RoutedPropertyChangedEventArgs<double> e) { }

		public void MotionDragCompleted(MediaElement mePlayer, Slider meSliderPosition, DragCompletedEventArgs e)
		{
			_userIsDraggingMotionPicSlider = false;
			mePlayer.Position = TimeSpan.FromSeconds(meSliderPosition.Value);
		}

		public void MotionDragStarted(DragStartedEventArgs e) => _userIsDraggingMotionPicSlider = true;

		public void TimeFrameValueChanged(RoutedPropertyChangedEventArgs<double> e) { }

		#region Inner VM Communication Message Handling

		private void RegisterMessages()
		{
			Messenger<CloseDialog>.DefaultMessenger.Register(this, OnCloseStretchMode, MessageContext.CloseStretchMode);
			Messenger<SelectedStretchModeMessage>.DefaultMessenger.Register(this, OnSetStretchMode, MessageContext.SetStretchMode);
			Messenger<SetIntervalMessage>.DefaultMessenger.Register(this, OnSetIntervalBetweenPictures, MessageContext.SetIntervalBetweenPictures);
			Messenger<CloseDialog>.DefaultMessenger.Register(this, OnCloseIntervalBetweenPictures, MessageContext.CloseIntervalBetweenPictures);
			Messenger<CloseDialog>.DefaultMessenger.Register(this, OnCancelFileTypes, MessageContext.CloseFileTypes);
			Messenger<SelectedMetadataMessage>.DefaultMessenger.Register(this, OnSetMetadataAction, MessageContext.SetMetadata);
			Messenger<BufferDepthMessage>.DefaultMessenger.Register(this, OnBufferDepthAction, MessageContext.BufferDepth);
			Messenger<CloseDialog>.DefaultMessenger.Register(this, OnCloseBufferDepth, MessageContext.CloseBufferDepth);
			Messenger<CommandRequest>.DefaultMessenger.Register(this, BackImageMove, MessageContext.BackImageCommand);
			Messenger<CommandRequest>.DefaultMessenger.Register(this, NextImageMove, MessageContext.NextImageCommand);
			Messenger<PropertyChangedCommandRequest>.DefaultMessenger.Register(this, m => MotionValueChanged(m.PropertyChangedEventArgs), MessageContext.MotionValueChanged);
			Messenger<DragMotionStartedCommandRequest>.DefaultMessenger.Register(this, m => MotionDragStarted(m.StartedEventArgs), MessageContext.DragMotionStarted);
			Messenger<DragMotionCompletedCommandRequest>.DefaultMessenger.Register(this, m => MotionDragCompleted(m.MediaPlayer, m.SliderPosition, m.CompletedEventArgs), MessageContext.DragMotionCompleted);
		}

		private void OnCloseIntervalBetweenPictures(CloseDialog obj) => _intervalBetweenPicturesService.CloseDetailDialog();

		private void OnSetIntervalBetweenPictures(SetIntervalMessage intervalMsg)
		{
			var interval = intervalMsg.SetInterval;
			IntervalBetweenPictures = interval;

			ConfigValue.Inst.UpdateIntervalBetweenPictures(IntervalBetweenPictures);
		}

		private void OnCloseStretchMode(CloseDialog obj) => _stretchSvc.CloseDetailDialog();

		private void OnSetStretchMode(SelectedStretchModeMessage stretchMode) => ConfigValue.Inst.UpdateImageToStretch(stretchMode.Mode);

		private void OnCancelFileTypes(CloseDialog obj) => _pictureMetadataService.CloseDetailDialog();

		private void OnSetMetadataAction(SelectedMetadataMessage metadata)
		{
			var pictureFolder = metadata.PictureFolder;
			if (!string.IsNullOrWhiteSpace(pictureFolder))
			{
				ConfigValue.Inst.UpdateInitialPictureDirectories(pictureFolder);
				ConfigValue.Inst.SetInitialPictureDirectories(pictureFolder);
			}

			var firstPicture = metadata.FirstPictureToDisplay;
			ConfigValue.Inst.UpdateFirstPictureToDisplay(firstPicture);
			ConfigValue.Inst.SetFirstPic(firstPicture);

			var stillExt = metadata.StillPictureExtensions;
			ConfigValue.Inst.UpdateStillPictureExtensions(stillExt);
			ConfigValue.Inst.SetStillExtension(stillExt);

			var motionExt = metadata.MotionPictureExtensions;
			ConfigValue.Inst.UpdateMotionPictures(motionExt);
			ConfigValue.Inst.SetMotionExtension(motionExt);

			// Restart the system
			ResetPictures();
		}

		private void ResetPictures()
		{
			if (RotationRunning && WindowSizeState != WindowState.Minimized)
			{
				_picChangeTmr.Stop();
				_model.Restart();
				_picChangeTmr.Start();
				ResetHeartBeat();
			}
			else
				_model.Restart();
		}

		private void OnBufferDepthAction(BufferDepthMessage bufferDepth)
		{
			var depth = bufferDepth.BufferDepth;
			if (depth <= 0) return;

			ConfigValue.Inst.UpdateMaxPictureTrackerDepth(depth);
			ConfigValue.Inst.SetMaxTrackingDepth(depth);
		}

		private void OnCloseBufferDepth(CloseDialog obj) => _pictureBufferService.CloseDetailDialog();

		#endregion

		#region Command

		private void LoadCommands()
		{
			StopStartCommand = new CustomCommand(StopStartRotation);
			BackImageCommand = new CustomCommand(BackImageMove);
			NextImageCommand = new CustomCommand(NextImageMove);
			DoNotShowImageCommand = new CustomCommand(DoNotShowImage);
			SetTimeBetweenPicturesCommand = new CustomCommand(SetTimeBetweenPictures);
			SetSelectedStrechModeCommand = new CustomCommand(SetSelectedStrechMode);
			SetPicturesMetaDataCommand = new CustomCommand(SetPicturesMetaData);
			SetPictureBufferDepthCommand = new CustomCommand(SetPictureBufferDepth);
			ManageNoDisplayListCommand = new CustomCommand(ManageNoDisplayList);
			PalyCommand = new CustomCommand(Play, CanPlay);
		}

		public ICommand StopStartCommand { get; set; }

		public ICommand BackImageCommand { get; set; }

		public ICommand NextImageCommand { get; set; }

		public ICommand DoNotShowImageCommand { get; set; }

		public ICommand SetTimeBetweenPicturesCommand { get; set; }

		public ICommand SetSelectedStrechModeCommand { get; set; }

		public ICommand SetPicturesMetaDataCommand { get; set; }

		public ICommand SetPictureBufferDepthCommand { get; set; }

		public ICommand ManageNoDisplayListCommand { get; set; }

		public ICommand PalyCommand { get; set; }

		private void StopStartRotation(object _) => RotationRunning = !RotationRunning;

		public void Play(object _) => IsMotionRunning = true;

		public bool CanPlay(object _) => !IsMotionRunning;

		public void BackImageMove(object _)
		{
			CurrentPicture = SelectionTracker.Inst.Prev();
			ResetHeartBeat();

			if (!RotationRunning) return;
			// No need to check minimized state because we cannot select the back-image-button if the window is minimized
			//if (WindowSizeState == WindowState.Minimized) return;

			_picChangeTmr.Stop();
			_picChangeTmr.Start();
			ResetHeartBeat();
		}

		public void NextImageMove(object _)
		{
			if (SelectionTracker.Inst.AtTail)
				RetrieveNextPicture();
			else
				CurrentPicture = SelectionTracker.Inst.Next();

			if (!RotationRunning) return;
			// No need to check minimized state because we cannot select the next-image-button if the window is minimized
			//if (WindowSizeState == WindowState.Minimized) return;

			_picChangeTmr.Stop();
			_picChangeTmr.Start();
			ResetHeartBeat();
		}

		public void DoNotShowImage(object _) => _model.AddPictureToAvoid(_model.CurrentPicIndex);

		private void SetTimeBetweenPictures(object _) => _intervalBetweenPicturesService.ShowDetailDialog(IntervalBetweenPictures);

		private void SetSelectedStrechMode(object _)
		{
			var mode = ImageStretch.TextToMode();
			_stretchSvc.ShowDetailDialog(mode);
		}

		private void SetPicturesMetaData(object _)
		{
			var picFolder = string.Join(";", ConfigValue.Inst.InitialPictureDirectories());
			var firstPicture = ConfigValue.Inst.FirstPictureToDisplay();
			var stillExtentions = string.Join(";", ConfigValue.Inst.StillPictureExtensions());
			var motionExtentions = string.Join(";", ConfigValue.Inst.MotionPictures());
			var metaData = new PictureMetaDataTransmission {
				PictureFolder = picFolder,
				FirstPictureToDisplay = firstPicture,
				StillPictureExtensions = stillExtentions,
				MotionPictureExtensions = motionExtentions
			};
			_pictureMetadataService.ShowDetailDialog(metaData);
		}

		private void SetPictureBufferDepth(object _)
		{
			var depth = ConfigValue.Inst.MaxPictureTrackerDepth();
			_pictureBufferService.ShowDetailDialog(depth);
		}

		private void ManageNoDisplayList(object _)
		{

		}

		#endregion

		#region Utilities

		private bool IsWindowStateMinimized(WindowState state) => state == WindowState.Minimized;

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string propName = "")
		{
			//Log.Info($"propName = {propName}");
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

		#endregion
	}
}
