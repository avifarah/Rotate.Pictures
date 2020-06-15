using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Rotate.Pictures.Annotations;
using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures.ViewModel
{
	public sealed class IntervalBetweenPicturesViewModel : INotifyPropertyChanged, IDataErrorInfo
	{
		public IntervalBetweenPicturesViewModel()
		{
			LoadCommands();
			RegisterMessages();
		}

		private float _setIntervalBeweenPictures;

		public float SetIntervalBetweenPictures
		{
			get => _setIntervalBeweenPictures;
			set
			{
				_setIntervalBeweenPictures = value;
				OnPropertyChanged();
			}
		}

		#region RegisterMessages

		private void RegisterMessages() => 
			Messenger<SelectedIntervalMessage>.Instance.Register(this, OnSelectedIntervalBetweenPictures, MessageContext.SelectedIntervalViewModel);

		private void OnSelectedIntervalBetweenPictures(SelectedIntervalMessage selectedInterval) => SetIntervalBetweenPictures = selectedInterval.SelectedInterval;

		#endregion

		#region Command

		private void LoadCommands()
		{
			CancelCommand = new CustomCommand(CancelAct);
			OkCommand = new CustomCommand(OkAct);
		}

		public ICommand CancelCommand { get; set; }

		public ICommand OkCommand { get; set; }

		private void CancelAct()
		{
			Messenger<SelectedIntervalMessage>.Instance.Unregister(this, MessageContext.SelectedIntervalViewModel);
			Messenger<CloseDialog>.Instance.Send(new CloseDialog(), MessageContext.CloseIntervalBetweenPictures);
		}

		private void OkAct()
		{
			Messenger<SetIntervalMessage>.Instance.Send(new SetIntervalMessage(SetIntervalBetweenPictures), MessageContext.SetIntervalBetweenPictures);
			CancelAct();
		}

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		private void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		#endregion

		#region Implementation of IDataErrorInfo

		public string this[string propertyName]
		{
			get
			{
				propertyName = propertyName ?? string.Empty;
				if (propertyName != string.Empty && propertyName != nameof(SetIntervalBetweenPictures)) return string.Empty;

				string result = (SetIntervalBetweenPictures > 0F) ? string.Empty : "Name cannot be blank!";
				return result;
			}
		}

		public string Error => this[string.Empty];

		#endregion
	}
}
