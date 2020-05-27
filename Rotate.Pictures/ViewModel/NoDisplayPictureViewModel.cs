using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.Utility;

namespace Rotate.Pictures.ViewModel
{
	public class NoDisplayPictureViewModel : INotifyPropertyChanged
	{
		public class NoDisplayItem
		{
			public string ColumnPicIndex;
			public string ColunPath;
		}

		private IEnumerable<int> _noDisplayPics;

		private IDictionary<int, string> _noDisplayDic;

		public NoDisplayPictureViewModel()
		{
			LoadCommands();
			RegisterMessages();
		}

		private void RegisterMessages()
		{
			Messenger<NoDisplayPicturesMessage>.DefaultMessenger.Register(this, OnNoDisplayList, MessageContext.NoDisplayPicture);
		}

		private void UnregisterMessages()
		{
			Messenger<NoDisplayPicturesMessage>.DefaultMessenger.Unregister(this, MessageContext.NoDisplayPicture);
		}

		private void OnNoDisplayList(NoDisplayPicturesMessage noDisplayParam)
		{
			_noDisplayPics = noDisplayParam.Param.NoDisplayPics;
			_noDisplayDic = noDisplayParam.Param.NoDisplayDic;
			foreach (var item in _noDisplayDic)
				_noDisplayItems.Add(new NoDisplayItem { ColumnPicIndex = item.Key.ToString(), ColunPath = item.Value });
		}


		private List<NoDisplayItem> _noDisplayItems = new List<NoDisplayItem>();

		public List<NoDisplayItem> NoDisplayItems
		{
			get => _noDisplayItems;
			set
			{
				_noDisplayItems = value;
				OnPropertyChanged();
			}
		}

		private void LoadCommands()
		{
			ExitCommand = new CustomCommand(ExitDialog);
		}

		public ICommand ExitCommand { get; set; }

		private void ExitDialog(object _)
		{
			UnregisterMessages();
			Messenger<CloseDialog>.DefaultMessenger.Send(new CloseDialog(), MessageContext.NoDisplayPicture);
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		#endregion
	}
}
