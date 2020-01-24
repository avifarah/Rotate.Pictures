using System.IO;
using System.Linq;
using System.Windows.Input;
using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures.ViewModel
{
	public class FileTypesToRotateViewModel : ViewModelBase
	{
		public FileTypesToRotateViewModel()
		{
			RegisterMessages();
			LoadCommands();
			LoadRules();
		}

		private string _pictureFolders;

		public string PictureFolders
		{
			get => _pictureFolders;
			set
			{
				_pictureFolders = value;
				OnPropertyChanged();
			}
		}

		private string _firstPictureToDisplayToDisplay;

		public string FirstPictureToDisplay
		{
			get => _firstPictureToDisplayToDisplay;
			set
			{
				_firstPictureToDisplayToDisplay = value;
				OnPropertyChanged();
			}
		}

		private string _stillPictureExtensions;

		public string StillPictureExtensions
		{
			get => _stillPictureExtensions;
			set
			{
				_stillPictureExtensions = value;
				OnPropertyChanged();
			}
		}

		private string _motionPictureExtensions;

		public string MotionPictureExtensions
		{
			get => _motionPictureExtensions;
			set
			{
				_motionPictureExtensions = value;
				OnPropertyChanged();
			}
		}

		#region Register Messages

		private void RegisterMessages() =>
			Messenger<SelectedMetadataMessage>.DefaultMessenger.Register(this, OnMetaDataProcess, MessageContext.SelectedMetadataViewModel);

		private void OnMetaDataProcess(SelectedMetadataMessage metadata)
		{
			if (metadata == null)
			{
				PictureFolders = string.Join(";", ConfigValue.Inst.InitialPictureDirectories());
				FirstPictureToDisplay = ConfigValue.Inst.FirstPictureToDisplay();
				StillPictureExtensions = string.Join(";", ConfigValue.Inst.StillPictureExtensions().ToArray());
				MotionPictureExtensions = string.Join(";", ConfigValue.Inst.MotionPictures().ToArray());
				return;
			}

			PictureFolders = metadata.PictureFolder;
			FirstPictureToDisplay = metadata.FirstPictureToDisplay;
			StillPictureExtensions = metadata.StillPictureExtensions;
			MotionPictureExtensions = metadata.MotionPictureExtensions;
		}

		#endregion

		#region Command

		private void LoadCommands()
		{
			CancelCommand = new CustomCommand(CancelAction);
			OkCommand = new CustomCommand(OkAction, CanOk);
			RestoreStillExtCommand = new CustomCommand(RestoreStillExts);
			RestoreMotionExtCommand = new CustomCommand(RestoreMotionExt);
		}

		public ICommand CancelCommand { get; set; }

		public ICommand OkCommand { get; set; }

		public ICommand RestoreStillExtCommand { get; set; }

		public ICommand RestoreMotionExtCommand { get; set; }

		private void CancelAction(object _)
		{
			Messenger<SelectedMetadataMessage>.DefaultMessenger.Unregister(this, MessageContext.SelectedMetadataViewModel);
			Messenger<CloseDialog>.DefaultMessenger.Send(new CloseDialog(), MessageContext.CloseFileTypes);
		}

		private void RestoreStillExts(object _) => StillPictureExtensions = ConfigValue.Inst.RestoreStillExtensions;

		private void RestoreMotionExt(object _) => MotionPictureExtensions = ConfigValue.Inst.RestoreMotionExtensions;

		private void OkAction(object _)
		{
			var metadata = new PictureMetaDataTransmission {
				PictureFolder = PictureFolders,
				FirstPictureToDisplay = FirstPictureToDisplay,
				StillPictureExtensions = StillPictureExtensions,
				MotionPictureExtensions = MotionPictureExtensions
			};
			Messenger<SelectedMetadataMessage>.DefaultMessenger.Send(new SelectedMetadataMessage(metadata), MessageContext.SetMetadata);
			CancelAction(null);
		}

		private bool CanOk(object _) => !HasErrors;

		#endregion

		#region Rules

		private void LoadRules()
		{
			bool ValidPictureFolder()
			{
				var rc = string.IsNullOrWhiteSpace(PictureFolders);
				if (rc) return false;
				var dirs = PictureFolders.Split(";".ToCharArray());
				return dirs.All(dir => !string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir));
			}
			AddRule(nameof(PictureFolders), ValidPictureFolder, "Invalid InitialFolder.  All directories should exist");

			bool ValidFirstPictureToDisplay() => string.IsNullOrWhiteSpace(FirstPictureToDisplay) || File.Exists(FirstPictureToDisplay);
			AddRule(nameof(FirstPictureToDisplay), ValidFirstPictureToDisplay, "First picture cannot be found");

			bool ValidExtensions(string extenstions)
			{
				if (string.IsNullOrWhiteSpace(extenstions)) return true;
				return extenstions.Split(";".ToCharArray()).All(s => s.StartsWith("."));
			}

			AddRule(nameof(StillPictureExtensions), () => ValidExtensions(StillPictureExtensions), "Still picture extensions must start with a period");
			AddRule(nameof(MotionPictureExtensions), () => ValidExtensions(MotionPictureExtensions), "Motion picture extension must start with a period");
		}

		#endregion
	}
}
