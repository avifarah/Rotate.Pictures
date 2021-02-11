using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.Utility;

namespace Rotate.Pictures.ViewModel
{
	public class NoDisplayItem : INotifyPropertyChanged
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public NoDisplayItem()
		{
			LoadCommands();
			RegisterMessages();
		}

		private void LoadCommands()
		{
			DeleteCommand = new CustomCommand<string>(DeleteItemAction);
		}

		private void RegisterMessages()
		{
		}

		private void UnregisterMessages()
		{
		}

		/// <summary>Delete the picture from the no-display-pics</summary>
		public ICommand DeleteCommand { get; set; }

		/// <summary>
		/// Delete item:
		///		NoDisplayItem -- starts here
		///			When the button bearing the delete icon is selected The
		///			DeleteCommand is activated passing control here
		/// 
		///		Main window ViewModel -- message here
		///			We need to pass control to the main window ViewModel.  The
		///			Main Window  VM contains a reference to the Main Window Model
		///			where the deletion needs to take place.
		/// 
		///		To the NoDisplayPictureViewModel	-- message here
		///			Where NoDisplayItems needs updating
		/// </summary>
		/// <param name="pictureIndex">A string type</param>
		private void DeleteItemAction(string pictureIndex)
		{
			if (pictureIndex == null)
			{
				Log.Error($"pictureIndex cannot be null in DeleteItemAction(..)");
				MessageBox.Show(
					"Cannot delete picture from the collection of Do-Not-Display.  Picture ID is not provided.  " +
					$"You may manually delete the picture from the {Environment.GetCommandLineArgs()[0]}.Config file",
					"No Display Item");
				return;
			}

			var rc = int.TryParse(pictureIndex, out var picIndex);
			if (!rc)
			{
				Log.Error($"pictureIndex cannot be null in DeleteItemAction(..)");
				MessageBox.Show(
					"Cannot delete picture from the collection of Do not display.  Picture ID is not provided.  " +
					$"You may manually delete the picture from the {Environment.GetCommandLineArgs()[0]}.Config file",
					"No Display Item");
				return;
			}

			var removeFromDoNotDisplay = new DoNotDisplayMessage(picIndex);
			Messenger<DoNotDisplayMessage>.Instance.Send(removeFromDoNotDisplay, MessageContext.DeleteFromDoNotDisplay);
		}

		private string _columnIndex;

		public string ColumnPicIndex
		{
			get => _columnIndex;
			set
			{
				_columnIndex = value;
				OnPropertyChanged();
			}
		}

		private string _columnPath;

		public string ColumnPath
		{
			get => _columnPath;
			set
			{
				_columnPath = value;
				OnPropertyChanged();
			}
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public override string ToString() => $"{ColumnPicIndex}. {ColumnPath}";

		#endregion
	}
}
