using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.Utility;

// TODO: ColumnPath cannot be resolved in the View,
// TODO: Media and image occupy the same space, one is visible the other is not.  Make sure that object is always set to a valid instance.
// TODO: .	Do the same thing that we did to the main window.
// TODO: Target="TbPicturePath" and Target=FilePath pose an error
namespace Rotate.Pictures.ViewModel
{
	public partial class NoDisplayPictureViewModel : INotifyPropertyChanged
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public NoDisplayPictureViewModel()
		{
            NoDisplayItems = new ObservableCollection<NoDisplayItem>();
			LoadCommands();
			RegisterMessages();

			var configValue = ConfigValueProvider.Default;
			RepositoryFilePath = configValue.FilePathToSavePicturesToAvoid();
		}

		private void OnNoDisplayList(NoDisplayPicturesMessage noDisplayParam)
		{
			var noDisplayDic = noDisplayParam.Param.NoDisplayDic;
			_noDisplayItems.Clear();

			foreach (var item in noDisplayDic)
				_noDisplayItems.Add(new NoDisplayItem { ColumnPicIndex = item.Key.ToString(), ColumnPath = item.Value });

			OnPropertyChanged(nameof(NoDisplayItems));
		}

		private void OnFromMainToDoNotDisplay(DoNotDisplayMessage obj)
		{
			if (obj == null)
			{
				Log.Error($"noDisplay cannot be null in OnFromMainToDoNotDisplay(..)");
				MessageBox.Show(@"Cannot delete picture from the collection of Do-Not-Display.  Picture ID is not provided.  " +
						$"You may manually delete the picture from the {Environment.GetCommandLineArgs()[0]}.Config file",
					@"No Display Picture View Model");
				return;
			}

			var pic = NoDisplayItems.FirstOrDefault(p => p.ColumnPicIndex == obj.PicIndex.ToString());
			if (pic == null) return;
			NoDisplayItems.Remove(pic);
			OnPropertyChanged(nameof(NoDisplayItems));
		}

		public string NoDisplayWarning
		{
			get => "Change to the contents of the picture repositories deems this picture mapping inaccurate.  "
			       + "However, saving and retrieving the picture set will overcome this shortcoming.";
			set { }
		}

		private ObservableCollection<NoDisplayItem> _noDisplayItems;

		public ObservableCollection<NoDisplayItem> NoDisplayItems
		{
			get => _noDisplayItems;
			set
			{
				_noDisplayItems = value;
				OnPropertyChanged();
			}
		}

		private NoDisplayItem _currentNoDisplayItem;

		public NoDisplayItem CurrentNoDisplayItem
		{
			get => _currentNoDisplayItem;
			set
			{
				_currentNoDisplayItem = value;
				OnPropertyChanged();
			}
		}

		private int _currentNoDisplayIndex;

		public int CurrentNoDisplayIndex
		{
			get => _currentNoDisplayIndex;
			set
			{
				_currentNoDisplayIndex = value;
				OnPropertyChanged();
			}
		}

		private string _textPicturePath;

		public string TextPicturePath
		{
			get => _textPicturePath;
			set
			{
				_textPicturePath = value;
				OnPropertyChanged();
			}
		}

		private string _repositoryFilePath;

		public string RepositoryFilePath
		{
			get => _repositoryFilePath;
			set
			{
				_repositoryFilePath = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(RepositoryFilePathError));
			}
		}

		private string _repositoryFilePathError;

		public string RepositoryFilePathError
		{
			get
			{
				var (_, msg) = CanSaveErrorString();
				_repositoryFilePathError = msg;
				return _repositoryFilePathError;
			}

			set
			{
				_repositoryFilePathError = value;
				OnPropertyChanged();
			}
		}

		private void LoadCommands()
		{
			ExitCommand = new CustomCommand(ExitDialog);
			AddToNoDisplayCommand = new CustomCommand(AddToNoDisplayAction, CanAddToNoDisplay);
			ClearCommand = new CustomCommand(ClearAction);
			RetrieveRepositoryCommand = new CustomCommand(RetrieveRepositoryAction, CanRetrieveRepositoryAction);
			SaveRepositoryCommand = new CustomCommand(SaveRepositoryAction, CanSaveRepositoryAction);
		}

		public ICommand ExitCommand { get; set; }

		public ICommand AddToNoDisplayCommand { get; set; }

		public ICommand ClearCommand { get; set; }

		public ICommand RetrieveRepositoryCommand { get; set; }
		
		public ICommand SaveRepositoryCommand { get; set; }

		private bool CanAddToNoDisplay()
		{
			if (string.IsNullOrEmpty(TextPicturePath)) return false;
			return File.Exists(TextPicturePath);
		}

		private void AddToNoDisplayAction() => 
			Messenger<DoNotDisplayPathMessage>.Instance.Send(new DoNotDisplayPathMessage(TextPicturePath), MessageContext.MainWindowViewModel);

		private void ClearAction() =>
			Messenger<ClearDoNotDisplayPathMessage>.Instance.Send(new ClearDoNotDisplayPathMessage(), MessageContext.MainWindowViewModel);

		/// <summary>
		/// Repository file must exist
		/// </summary>
		private bool CanRetrieveRepositoryAction()
		{
			if (string.IsNullOrEmpty(RepositoryFilePath)) return false;

			try
			{
				var fileName = Environment.ExpandEnvironmentVariables(RepositoryFilePath);
				var fullFn = Path.GetFullPath(fileName);
				var rc = File.Exists(fullFn);
				if (!rc) return false;

				var attrs = File.GetAttributes(fullFn);
				if (attrs == FileAttributes.Normal) return true;
				if ((attrs & FileAttributes.Offline) == FileAttributes.Offline)
				{
					Log.Error($"File: \"{fullFn}\" is off-line");
					return false;
				}

				return true;
			}
			catch (Exception e)
			{
				Log.Error($"Error in path: \"{RepositoryFilePath}\"", e);
				return false;
			}
		}

		private void RetrieveRepositoryAction()
		{
			var fileName = Environment.ExpandEnvironmentVariables(RepositoryFilePath);
            var picsToAvoid = DoNotDisplayUtil.RetrieveDoNotDisplay(fileName);
			var loadNoDisplay = new LoadNoDisplayPicturesMessage(picsToAvoid);
			Messenger<LoadNoDisplayPicturesMessage>.Instance.Send(loadNoDisplay, MessageContext.MainWindowViewModel);

			MessageBox.Show(@"Retrieved", @"Rotating.Pictures");
		}

		/// <summary>
		/// Repository file must be a valid path
		/// In effect RepositoryFilePath's directory must exist 
		/// </summary>
		private bool CanSaveRepositoryAction()
		{
			if (string.IsNullOrEmpty(RepositoryFilePath)) return false;

			var (rc, _) = CanSaveErrorString();
			return rc;
		}

		private (bool, string) CanSaveErrorString()
        {
			// None of the attributes == 0 there fore there is no need to check against this possibility, (x & 0 == 0 --always!).
            bool AttrBitCheck(FileAttributes fileAttributes, FileAttributes check) => (fileAttributes & check) == check;

            try
			{
				var fileName = Environment.ExpandEnvironmentVariables(RepositoryFilePath);
				var fullFn = Path.GetFullPath(fileName);
				var rc = Directory.Exists(fullFn);
				if (rc) return (false, string.Empty);

				var dir = Path.GetDirectoryName(fullFn);
				rc = Directory.Exists(dir);
				if (!rc) return (false, string.Empty);

				// If file exists make sure that we can write to it
				rc = File.Exists(fullFn);
				if (!rc) return (true, string.Empty);

				var attrs = File.GetAttributes(fullFn);
				if (AttrBitCheck(attrs, FileAttributes.Normal)) return (true, string.Empty);
				if (AttrBitCheck(attrs, FileAttributes.ReadOnly)) return (false, "File is read-only");
				if (AttrBitCheck(attrs, FileAttributes.Hidden)) return (false, "File is hidden");
				if (AttrBitCheck(attrs, FileAttributes.System)) return (false, "File is a system-file");
				if (AttrBitCheck(attrs, FileAttributes.Directory)) return (false, "File is a directory");
				if (AttrBitCheck(attrs, FileAttributes.Offline)) return (false, "File is off-line");
				return (true, string.Empty);
			}
			catch (Exception e)
			{
				return (false, $"File is invalid.  Internal message: {e.Message}");
			}
		}

		private void SaveRepositoryAction()
        {
            DoNotDisplayUtil.SaveDoNotDisplay(NoDisplayItems.Select(item => item.ColumnPath), RepositoryFilePath);
            MessageBox.Show("Saved", "Rotating.Pictures");
        }

		private void RegisterMessages()
		{
			Messenger<NoDisplayPicturesMessage>.Instance.Register(this, OnNoDisplayList, MessageContext.NoDisplayPicture);
			Messenger<DoNotDisplayMessage>.Instance.Register(this, OnFromMainToDoNotDisplay, MessageContext.FromMainToDoNotDisplay);
			Messenger<DoNotDisplayPathMessage>.Instance.Register(this, OnDisplayDeletedPath, MessageContext.FromMainToDoNotDisplay);
			Messenger<ClearDoNotDisplayPathMessage>.Instance.Register(this, OnClearDoNotDisplay, MessageContext.NoDisplayPicture);
		}

		private void UnregisterMessages()
		{
			Messenger<NoDisplayPicturesMessage>.Instance.Unregister(this, MessageContext.NoDisplayPicture);
			Messenger<DoNotDisplayMessage>.Instance.Unregister(this, MessageContext.FromMainToDoNotDisplay);
			Messenger<DoNotDisplayPathMessage>.Instance.Unregister(this, MessageContext.FromMainToDoNotDisplay);
			Messenger<ClearDoNotDisplayPathMessage>.Instance.Unregister(this, MessageContext.NoDisplayPicture);
		}

		private void OnDisplayDeletedPath(DoNotDisplayPathMessage noDisplay) => TextPicturePath = noDisplay.PicPath;

		private void OnClearDoNotDisplay()
		{
			NoDisplayItems.Clear();
			OnPropertyChanged(nameof(NoDisplayItems));
		}

		private void ExitDialog()
		{
			UnregisterMessages();
			Messenger<CloseDialog>.Instance.Send(new CloseDialog(), MessageContext.NoDisplayPicture);
		}

#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

#endregion
	}
}
