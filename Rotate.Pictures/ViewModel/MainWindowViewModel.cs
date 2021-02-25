using System;
using System.IO;
using System.ComponentModel;
//using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel.PeerResolvers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Rotate.Pictures.EventAggregator;
using Rotate.Pictures.Utility;
using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.Model;
using Rotate.Pictures.Service;

namespace Rotate.Pictures.ViewModel
{
	public class MainWindowViewModel : INotifyPropertyChanged, ISubscriber<PictureLoadingDoneEventArgs>
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly DispatcherTimer _picChangeTmr;
		private readonly DispatcherTimer _visualHeartbeatTmr;
		private readonly PictureModel _model;

		private readonly StretchDialogService _stretchSvc;
		private readonly IntervalBetweenPicturesService _intervalBetweenPicturesSvc;
		private readonly FileTypeToRotateService _pictureMetadataSvc;
		private readonly PictureBufferDepthService _pictureBufferSvc;
		private readonly NoDisplayPictureService _noDisplayPictureSvc;

		private const int RetryPictureCount = 5;

		private readonly IConfigValue _configValue;

		public MainWindowViewModel()
		{
			//Debug.WriteLine($"{MethodBase.GetCurrentMethod().DeclaringType}.{MethodBase.GetCurrentMethod().Name}(..)");
			_configValue = ConfigValueProvider.Default;

			// Newing up the Model hear, marries the model to the MainWindow (hard dependency).  I feel that this is OK.
			// Having it married it is OK to hook an event
			_model = (PictureModel)ModelFactory.Inst.Create("PictureFileRepository", _configValue);
			_model.OnPictureRetrieving += Model_OnPictureRetrieving;

			_stretchSvc = new StretchDialogService();
			_intervalBetweenPicturesSvc = new IntervalBetweenPicturesService();
			_pictureMetadataSvc = new FileTypeToRotateService();
			_pictureBufferSvc = new PictureBufferDepthService();
			_noDisplayPictureSvc = new NoDisplayPictureService();

			// Use the local variables like, _intervalBetweenPictures, _visHeartBeatInterval, etc, as opposed
			// to their properties like, IntervalBetweenPictures, VisHeartBeatInterval, etc, because we
			// initialize the property and need not need the side effects the occur with their properties.
			_intervalBetweenPictures = _configValue.IntervalBetweenPictures();
            _visHeartBeatInterval = _configValue.VisualHeartbeat();
			_visualHeartbeatTmr = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_configValue.VisualHeartbeat()), IsEnabled = true };
			_visualHeartbeatTmr.Tick += VisualHeartBeatUpdate;

			CurrentPicture = _configValue.FirstPictureToDisplay();
			if (!File.Exists(CurrentPicture))
			{
				var errMsg = $"First picture file: {CurrentPicture} cannot be found.  Check configuration file";
				Log.Error(errMsg);
				MessageBox.Show(errMsg, "Rotating.Pictures [1]");
				CurrentPicture = null;
			}
			else if (!CurrentPicture.IsPictureValidFormat())
				CurrentPicture = null;

			ImageStretch = _configValue.ImageStretch();
			InitialRotationMode = _configValue.RotatingPicturesInit();
			RotationRunning = InitialRotationMode;

			_picChangeTmr = new DispatcherTimer {
				Interval = TimeSpan.FromMilliseconds(IntervalBetweenPictures),
				IsEnabled = RotationRunning
			};
			_picChangeTmr.Tick += (_, _) => RetrieveNextPicture();

			_model.SelectionTrackerAppend(CurrentPicture);

			if (CurrentPicture == null && !File.Exists(CurrentPicture))
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

			CustomEventAggregator.Inst.Subscribe(this);

			// Initialize fields
			IsModelDoneLoadingPictures = !_model.IsPicturesRetrieving;
			DirRetrievingVisible = Visibility.Visible;
		}

		private void Model_OnPictureRetrieving(object sender, PictureRetrievingEventArgs e)
		{
			DirectoryRetrievingNow = e.CurrentPictureDirectory;
		}

		private string _dirRetrievingNow;

		public string DirectoryRetrievingNow
		{
			get => _dirRetrievingNow;
			set
			{
				_dirRetrievingNow = value;
				OnPropertyChanged();
			}
		}

		private Visibility _dirRetrievingVisibility;

		public Visibility DirRetrievingVisible
		{
			get => _dirRetrievingVisibility;
			set
			{
				_dirRetrievingVisibility = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// The ViewModel CurrentPicture represents the displayed current picture
		/// </summary>
		private string _currentPicture;

		public string CurrentPicture
		{
			get => _currentPicture;
			set
			{
				_currentPicture = value;
				IsMotionRunning = _currentPicture.IsMotionPicture(_configValue.MotionPictures());
				OnPropertyChanged();
				OnPropertyChanged(nameof(DisplayCurrentPic));
			}
		}

		public string DisplayCurrentPic
		{
			get => _currentPicture;
			set
			{
				var displayPic = value;
				var isInCollection = _model.IsCollectionContains(displayPic);
				var isToAvoid = _model.IsPictureToAvoid(displayPic);
				var isValidFormat = displayPic.IsPictureValidFormat();

				if (isInCollection && !isToAvoid && isValidFormat)
				{
					CurrentPicture = displayPic;
					_model.SelectionTrackerAppend(CurrentPicture);

					ResetHeartBeat();
				}
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
				CurrentPictureColumnSpan = IsModelDoneLoadingPictures ? 3 : 1;
			}
		}

		private string _sliderVisibility;

		public string SliderVisibility
		{
			get => _sliderVisibility;
			set
			{
				_sliderVisibility = value ?? "visible";
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

		private int _intervalBetweenPictures;

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
				_configValue.UpdateOnStartRotatingPicture(_initialRotationMode);
				OnPropertyChanged();
			}
		}

		private int _visHeartBeatInterval;
		private double _intervalBetweenPics;

		public int VisHeartBeatInterval
		{
			get => _visHeartBeatInterval;
			set
			{
				_visHeartBeatInterval = value;
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

		private bool _isModelDoneLoadingPictures;

		public bool IsModelDoneLoadingPictures
		{
			get => _isModelDoneLoadingPictures;
			set
			{
				_isModelDoneLoadingPictures = value;
				OnPropertyChanged();
			}
		}

		private void RetrieveNextPicture()
		{
			// If window is minimized then do not advance pictures
			if (WindowSizeState == WindowState.Minimized) return;

			string pic = null;
			for (var i = 0; i < RetryPictureCount; ++i)
			{
				// _model.GetNextPicture() checks for File.Exists(..)
				pic = _model.GetNextPicture();
				if (pic == null) continue;
				if (pic.IsPictureValidFormat()) break;
			}

			if (pic == null || !pic.IsPictureValidFormat())
				Log.Error($"File: {pic} cannot be found after {RetryPictureCount}.  Files may have been deleted after the program started");

			CurrentPicture = pic;
			_model.SelectionTrackerAppend(CurrentPicture);
			
			ResetHeartBeat();
		}

		private void VisualHeartBeatUpdate(object sender, EventArgs e)
		{
			var cnt = (double)IntervalBetweenPictures / _configValue.VisualHeartbeat();
			_intervalBetweenPics += _intervalProgressBarMax / (cnt - 1D);
			VisHeartBeatInterval = (int)_intervalBetweenPics;
		}

		private void ResetHeartBeat()
		{
			_intervalBetweenPics = 0.0;
			_visualHeartbeatTmr.Start();
		}

		#region ISubscriber<PictureLoadingDoneEventArgs>

		public void OnEvent(PictureLoadingDoneEventArgs e)
		{
			IsModelDoneLoadingPictures = e.RetrieveCompleted;
			DirRetrievingVisible = Visibility.Collapsed;
			DirectoryRetrievingNow = string.Empty;
			CurrentPictureColumnSpan = IsModelDoneLoadingPictures ? 3 : 1;
		}

		#endregion

		#region Inner VM Communication Message Handling

		private void RegisterMessages()
		{
			Messenger<CloseDialog>.Instance.Register(this, OnCloseStretchMode, MessageContext.CloseStretchMode);
			Messenger<SelectedStretchModeMessage>.Instance.Register(this, OnSetStretchMode, MessageContext.SetStretchMode);
			Messenger<SetIntervalMessage>.Instance.Register(this, OnSetIntervalBetweenPictures, MessageContext.SetIntervalBetweenPictures);
			Messenger<CloseDialog>.Instance.Register(this, OnCloseIntervalBetweenPictures, MessageContext.CloseIntervalBetweenPictures);
			Messenger<CloseDialog>.Instance.Register(this, OnCancelFileTypes, MessageContext.CloseFileTypes);
			Messenger<SelectedMetadataMessage>.Instance.Register(this, OnSetMetadataAction, MessageContext.SetMetadata);
			Messenger<BufferDepthMessage>.Instance.Register(this, OnBufferDepthAction, MessageContext.BufferDepth);
			Messenger<CloseDialog>.Instance.Register(this, OnCloseBufferDepth, MessageContext.CloseBufferDepth);
			Messenger<CommandRequest>.Instance.Register(this, BackImageMove, MessageContext.BackImageCommand);
			Messenger<CommandRequest>.Instance.Register(this, NextImageMove, MessageContext.NextImageCommand);
			Messenger<CloseDialog>.Instance.Register(this, OnCloseNoDisplayPictures, MessageContext.NoDisplayPicture);
			Messenger<DoNotDisplayMessage>.Instance.Register(this, OnDeleteFromDoNotDisplay, MessageContext.DeleteFromDoNotDisplay);
			Messenger<DoNotDisplayPathMessage>.Instance.Register(this, OnAddToDoNotDisplay, MessageContext.MainWindowViewModel);
			Messenger<ClearDoNotDisplayPathMessage>.Instance.Register(this, OnClearDoNotDisplay, MessageContext.MainWindowViewModel);
			Messenger<LoadNoDisplayPicturesMessage>.Instance.Register(this, OnLoadNoDisplayPictures, MessageContext.MainWindowViewModel);
		}

		private void UnregisterMessages()
		{
			Messenger<CloseDialog>.Instance.Unregister(this, MessageContext.CloseStretchMode);
			Messenger<SelectedStretchModeMessage>.Instance.Unregister(this, MessageContext.SetStretchMode);
			Messenger<SetIntervalMessage>.Instance.Unregister(this, MessageContext.SetIntervalBetweenPictures);
			Messenger<CloseDialog>.Instance.Unregister(this, MessageContext.CloseIntervalBetweenPictures);
			Messenger<CloseDialog>.Instance.Unregister(this, MessageContext.CloseFileTypes);
			Messenger<SelectedMetadataMessage>.Instance.Unregister(this, MessageContext.SetMetadata);
			Messenger<BufferDepthMessage>.Instance.Unregister(this, MessageContext.BufferDepth);
			Messenger<CloseDialog>.Instance.Unregister(this, MessageContext.CloseBufferDepth);
			Messenger<CommandRequest>.Instance.Unregister(this, MessageContext.BackImageCommand);
			Messenger<CommandRequest>.Instance.Unregister(this, MessageContext.NextImageCommand);
			Messenger<CloseDialog>.Instance.Unregister(this, MessageContext.NoDisplayPicture);
			Messenger<DoNotDisplayMessage>.Instance.Unregister(this, MessageContext.DeleteFromDoNotDisplay);
			Messenger<DoNotDisplayPathMessage>.Instance.Unregister(this, MessageContext.MainWindowViewModel);
			Messenger<ClearDoNotDisplayPathMessage>.Instance.Unregister(this, MessageContext.MainWindowViewModel);
			Messenger<LoadNoDisplayPicturesMessage>.Instance.Unregister(this, MessageContext.MainWindowViewModel);
		}

		private void OnCloseStretchMode() => _stretchSvc.CloseDetailDialog();

		private void OnSetStretchMode(SelectedStretchModeMessage stretchMode)
		{
			ImageStretch = stretchMode.Mode.ToString();
			_configValue.UpdateImageToStretch(stretchMode.Mode);
		}

		private void OnSetIntervalBetweenPictures(SetIntervalMessage intervalMsg)
		{
			var interval = intervalMsg.SetInterval;
			IntervalBetweenPictures = interval;

			_configValue.UpdateIntervalBetweenPictures(IntervalBetweenPictures);
		}

		private void OnCloseIntervalBetweenPictures() => _intervalBetweenPicturesSvc.CloseDetailDialog();

		private void OnCancelFileTypes() => _pictureMetadataSvc.CloseDetailDialog();

		private void OnSetMetadataAction(SelectedMetadataMessage metadata)
		{
			var pictureFolder = metadata.PictureFolder;
			if (!string.IsNullOrWhiteSpace(pictureFolder))
			{
				_configValue.UpdateInitialPictureDirectories(pictureFolder);
				_configValue.SetInitialPictureDirectories(pictureFolder);
			}

			var firstPicture = metadata.FirstPictureToDisplay;
			_configValue.UpdateFirstPictureToDisplay(firstPicture);
			_configValue.SetFirstPic(firstPicture);

			var stillExt = metadata.StillPictureExtensions;
			_configValue.UpdateStillPictureExtensions(stillExt);
			_configValue.SetStillExtension(stillExt);

			var motionExt = metadata.MotionPictureExtensions;
			_configValue.UpdateMotionPictures(motionExt);
			_configValue.SetMotionExtension(motionExt);

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

			_configValue.UpdateMaxPictureTrackerDepth(depth);
			_configValue.SetMaxTrackingDepth(depth);
			_model.SelectionTrackerSetMaxPictureDepth(depth);
		}

		private void OnCloseBufferDepth() => _pictureBufferSvc.CloseDetailDialog();

		private void OnCloseNoDisplayPictures() => _noDisplayPictureSvc.CloseDetailDialog();

		private void OnDeleteFromDoNotDisplay(DoNotDisplayMessage removeNoDisplayMessage)
		{
			var picIndex = removeNoDisplayMessage.PicIndex;
			_model.RemovePictureToAvoid(picIndex);
			Messenger<DoNotDisplayMessage>.Instance.Send(new DoNotDisplayMessage(picIndex), MessageContext.FromMainToDoNotDisplay);
			var path = _model.PicIndexToPath(removeNoDisplayMessage.PicIndex);
			Messenger<DoNotDisplayPathMessage>.Instance.Send(new DoNotDisplayPathMessage(path), MessageContext.FromMainToDoNotDisplay);
		}

		private void OnAddToDoNotDisplay(DoNotDisplayPathMessage path)
		{
			var inx = _model.PicPathToIndex(path.PicPath);
			_model.AddPictureToAvoid(inx);

			var picsToAvoid = _model.PicturesToAvoid;
			var picsToAvoidDic = picsToAvoid.ToDictionary(p => p, p => _model.PicIndexToPath(p));
			var parm = new NoDisplayPicturesMessage.NoDisplayParam(picsToAvoid, picsToAvoidDic);
			Messenger<NoDisplayPicturesMessage>.Instance.Send(new NoDisplayPicturesMessage(parm), MessageContext.NoDisplayPicture);
		}

		private void OnClearDoNotDisplay()
		{
			_model.ClearDoNotDisplayCollection();
			Messenger<ClearDoNotDisplayPathMessage>.Instance.Send(new ClearDoNotDisplayPathMessage(), MessageContext.NoDisplayPicture);
		}

		private void OnLoadNoDisplayPictures(LoadNoDisplayPicturesMessage paths)
		{
			// Clear all pictures to avoid
			_model.ClearDoNotDisplayCollection();

			// Add all pictures from paths
			foreach (var path in paths.PicturesToAvoid)
			{
				var inx = _model.PicPathToIndex(path);
				_model.AddPictureToAvoid(inx);
			}

			var picsToAvoid = _model.PicturesToAvoid;
			var picsToAvoidDic = picsToAvoid.ToDictionary(p => p, p => _model.PicIndexToPath(p));
			var parm = new NoDisplayPicturesMessage.NoDisplayParam(picsToAvoid, picsToAvoidDic);
			Messenger<NoDisplayPicturesMessage>.Instance.Send(new NoDisplayPicturesMessage(parm), MessageContext.NoDisplayPicture);
		}

		#endregion

		#region Command

		private void LoadCommands()
		{
			StopStartCommand = new CustomCommand(StopStartRotation);
			BackImageCommand = new CustomCommand(BackImageMove, CanBackImageMove);
			NextImageCommand = new CustomCommand(NextImageMove);
			DoNotShowImageCommand = new CustomCommand(DoNotShowImage);
			SetTimeBetweenPicturesCommand = new CustomCommand(SetTimeBetweenPictures);
			SetSelectedStrechModeCommand = new CustomCommand(SetSelectedStrechMode);
			SetPicturesMetaDataCommand = new CustomCommand(SetPicturesMetaData);
			SetPictureBufferDepthCommand = new CustomCommand(SetPictureBufferDepth);
			ManageNoDisplayListCommand = new CustomCommand(ManageNoDisplayList);
			PalyCommand = new CustomCommand(Play, CanPlay);
			WindowClosing = new CustomCommand(WindowClosingAction);
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

		public ICommand WindowClosing { get; set; }

		private void StopStartRotation() => RotationRunning = !RotationRunning;

		public bool CanPlay() => !IsMotionRunning;

		public void Play() => IsMotionRunning = true;

		private bool CanBackImageMove() => !_model.SelectionTrackerAtHead;

		public void BackImageMove()
		{
			int i = 0;
			for (i = 0; i < _model.SelectionTrackerCount; ++i)
			{
				// _model.SelectionTrackerPrev() checks for File.Exists(..)
				var pic = _model.SelectionTrackerPrev();
				if (pic.IsPictureValidFormat())
				{
					if (pic == null)
					{
						Log.Error("Cannot move image back");
						return;
					}

					if (pic.IsPictureValidFormat())
					{
						CurrentPicture = pic;
						break;
					}
				}
			}
			if (i == _model.SelectionTrackerCount) return;

			ResetHeartBeat();

			if (!RotationRunning) return;
			// No need to check minimized state because we cannot select the back-image-button if the window is minimized
			//if (WindowSizeState == WindowState.Minimized) return;

			_picChangeTmr.Stop();
			_picChangeTmr.Start();
			ResetHeartBeat();
		}

		public void NextImageMove()
		{
			if (_model.SelectionTrackerAtTail)
				RetrieveNextPicture();
			else
			{
				int i;
				for (i = 0; i < _model.SelectionTrackerCount; ++i)
				{
					var pic = _model.SelectionTrackerNext();
					if (pic == null) return;
					if (pic.IsPictureValidFormat())
					{
						if (pic.IsPictureValidFormat())
						{
							CurrentPicture = pic;
							break;
						}
					}
				}

				if (i == _model.SelectionTrackerCount) return;
			}

			if (!RotationRunning) return;

			// No need to check minimized state because we cannot select the next-image-button if the window is minimized
			//if (WindowSizeState == WindowState.Minimized) return;

			_picChangeTmr.Stop();
			_picChangeTmr.Start();
			ResetHeartBeat();
		}

		public void DoNotShowImage()
		{
			var inx = _model.PicPathToIndex(CurrentPicture);
			_model.AddPictureToAvoid(inx);
            DoNotDisplayUtil.SaveDoNotDisplay(_model.PicturesToAvoid.Select(pi => _model.PicIndexToPath(pi)), ConfigValue.Inst.FilePathToSavePicturesToAvoid());
			NextImageMove();
		}

		private void SetTimeBetweenPictures() => _intervalBetweenPicturesSvc.ShowDetailDialog(IntervalBetweenPictures);

		private void SetSelectedStrechMode()
		{
			var mode = ImageStretch.TextToMode();
			_stretchSvc.ShowDetailDialog(mode);
		}

		private void SetPicturesMetaData()
		{
			var picFolder = string.Join(";", _configValue.InitialPictureDirectories());
			var firstPicture = _configValue.FirstPictureToDisplay();
			var stillExtentions = string.Join(";", _configValue.StillPictureExtensions());
			var motionExtentions = string.Join(";", _configValue.MotionPictures());
			var metaData = new PictureMetaDataTransmission {
				PictureFolder = picFolder,
				FirstPictureToDisplay = firstPicture,
				StillPictureExtensions = stillExtentions,
				MotionPictureExtensions = motionExtentions
			};
			_pictureMetadataSvc.ShowDetailDialog(metaData);
		}

		private void SetPictureBufferDepth()
		{
			var depth = _configValue.MaxPictureTrackerDepth();
			_pictureBufferSvc.ShowDetailDialog(depth);
		}

		private void ManageNoDisplayList()
		{
			var picsToAvoid = _model.PicturesToAvoid;
			var picsToAvoidDic = picsToAvoid.ToDictionary(p => p, p => _model.PicIndexToPath(p));
			var param = new NoDisplayPicturesMessage.NoDisplayParam(picsToAvoid, picsToAvoidDic);
			_noDisplayPictureSvc.ShowDetailDialog(param);
		}

		private void WindowClosingAction() => UnregisterMessages();

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
