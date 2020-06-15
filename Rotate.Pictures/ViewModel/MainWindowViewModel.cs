﻿using System;
using System.IO;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
		private readonly IntervalBetweenPicturesService _intervalBetweenPicturesService;
		private readonly FileTypeToRotateService _pictureMetadataService;
		private readonly PictureBufferDepthService _pictureBufferService;
		private readonly NoDisplayPictureService _noDisplayPictureService;

		private const int RetryPictureCount = 5;

		public MainWindowViewModel()
		{
			_model = (PictureModel)ModelFactory.Inst.Create("PictureFileRepository");

			_stretchSvc = new StretchDialogService();
			_intervalBetweenPicturesService = new IntervalBetweenPicturesService();
			_pictureMetadataService = new FileTypeToRotateService();
			_pictureBufferService = new PictureBufferDepthService();
			_noDisplayPictureService = new NoDisplayPictureService();

			_visualHeartbeatTmr = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ConfigValue.Inst.VisualHeartbeat()), IsEnabled = true };
			_visualHeartbeatTmr.Tick += VisualHeartBeatUpdate;

			CurrentPicture = ConfigValue.Inst.FirstPictureToDisplay();
			if (!File.Exists(CurrentPicture))
			{
				var errMsg = $"First picture file: {CurrentPicture} cannot be found.  Check configuration file";
				Log.Error(errMsg);
				MessageBox.Show(errMsg, "Rotating.Pictures [1]");
				CurrentPicture = null;
			}
			else if (!CurrentPicture.IsPictureValidFormat())
				CurrentPicture = null;

			ImageStretch = ConfigValue.Inst.ImageStretch();
			InitialRotationMode = ConfigValue.Inst.RotatingPicturesInit();
			RotationRunning = InitialRotationMode;

			_picChangeTmr = new DispatcherTimer {
				Interval = TimeSpan.FromMilliseconds(IntervalBetweenPictures),
				IsEnabled = RotationRunning
			};
			_picChangeTmr.Tick += (sender, args) => RetrieveNextPicture();

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
				IsMotionRunning = _currentPicture.IsMotionPicture();
				OnPropertyChanged();
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

		private int _visHeartBeatValue = ConfigValue.Inst.VisualHeartbeat();
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
			var cnt = (double)IntervalBetweenPictures / ConfigValue.Inst.VisualHeartbeat();
			_intervalBetweenPics += (double)_intervalProgressBarMax / (cnt - 1);
			VisHeartBeatValue = (int)_intervalBetweenPics;
		}

		private void ResetHeartBeat()
		{
			_intervalBetweenPics = 0.0;
			_visualHeartbeatTmr.Start();
		}

		#region ISubscriber<PictureLoadingDoneEventArgs>

		public void OnEvent(PictureLoadingDoneEventArgs e) => IsModelDoneLoadingPictures = e.RetrieveCompleted;

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
			ConfigValue.Inst.UpdateImageToStretch(stretchMode.Mode);
		}

		private void OnSetIntervalBetweenPictures(SetIntervalMessage intervalMsg)
		{
			var interval = intervalMsg.SetInterval;
			IntervalBetweenPictures = interval;

			ConfigValue.Inst.UpdateIntervalBetweenPictures(IntervalBetweenPictures);
		}

		private void OnCloseIntervalBetweenPictures() => _intervalBetweenPicturesService.CloseDetailDialog();

		private void OnCancelFileTypes() => _pictureMetadataService.CloseDetailDialog();

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
			_model.SelectionTrackerSetMaxPictureDepth(depth);
		}

		private void OnCloseBufferDepth() => _pictureBufferService.CloseDetailDialog();

		private void OnCloseNoDisplayPictures() => _noDisplayPictureService.CloseDetailDialog();

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
			for (var i = 0; i < _model.Count; ++i)
			{
				var pic = _model.SelectionTrackerPrev();
				if (!File.Exists(pic))
					Log.Error($"File: {pic} cannot be found.  File may have been deleted after the program started");
				else if (pic.IsPictureValidFormat())
				{
					CurrentPicture = pic;
					if (pic.IsPictureValidFormat())
						break;
				}
				// else do not change picture
			}

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
				for (var i = 0; i < _model.Count; ++i)
				{
					var pic = _model.SelectionTrackerNext();
					if (!File.Exists(pic))
						Log.Error($"File: {pic} cannot be found.  Picture may have been deleted after program started");
					else if (pic.IsPictureValidFormat())
					{
						CurrentPicture = pic;
						if (pic.IsPictureValidFormat())
							break;
					}
					// else do not change picture
				}
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
			NextImageMove();
		}

		private void SetTimeBetweenPictures() => _intervalBetweenPicturesService.ShowDetailDialog(IntervalBetweenPictures);

		private void SetSelectedStrechMode()
		{
			var mode = ImageStretch.TextToMode();
			_stretchSvc.ShowDetailDialog(mode);
		}

		private void SetPicturesMetaData()
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

		private void SetPictureBufferDepth()
		{
			var depth = ConfigValue.Inst.MaxPictureTrackerDepth();
			_pictureBufferService.ShowDetailDialog(depth);
		}

		private void ManageNoDisplayList()
		{
			var picsToAvoid = _model.PicturesToAvoid;
			var picsToAvoidDic = picsToAvoid.ToDictionary(p => p, p => _model.PicIndexToPath(p));
			var param = new NoDisplayPicturesMessage.NoDisplayParam(picsToAvoid, picsToAvoidDic);
			_noDisplayPictureService.ShowDetailDialog(param);
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
