using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
			Messenger<SelectedMetadataMessage>.Instance.Register(this, OnMetaDataProcess, MessageContext.SelectedMetadataViewModel);

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

		private void CancelAction()
		{
			Messenger<SelectedMetadataMessage>.Instance.Unregister(this, MessageContext.SelectedMetadataViewModel);
			Messenger<CloseDialog>.Instance.Send(new CloseDialog(), MessageContext.CloseFileTypes);
		}

		private async void OkAction()
		{
			var metadata = new PictureMetaDataTransmission {
				PictureFolder = PictureFolders,
				FirstPictureToDisplay = FirstPictureToDisplay,
				StillPictureExtensions = StillPictureExtensions,
				MotionPictureExtensions = MotionPictureExtensions
			};
			await Task.Run(() => Messenger<SelectedMetadataMessage>.Instance.Send(new SelectedMetadataMessage(metadata), MessageContext.SetMetadata));

			CancelAction();
	
			MessageBox.Show("Rotation may be frozen for a moment while the meta data updates and pictures are read");
		}

		private bool CanOk() => !HasErrors;

		private void RestoreStillExts() => StillPictureExtensions = ConfigValue.Inst.RestoreStillExtensions;

		private void RestoreMotionExt() => MotionPictureExtensions = ConfigValue.Inst.RestoreMotionExtensions;

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
