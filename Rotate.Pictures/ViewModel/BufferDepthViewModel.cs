using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Rotate.Pictures.Annotations;
using Rotate.Pictures.MessageCommunication;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures.ViewModel
{
	public class BufferDepthViewModel : INotifyPropertyChanged
	{
		public BufferDepthViewModel()
		{
			RegisterMessages();
			LoadCommands();
		}

		private int _depth;

		public int Depth
		{
			get => _depth;
			set
			{
				_depth = value <= 0 ? 0 : value;
				OnPropertyChanged();
			}
		}

		#region	RegisterMessages

		private void RegisterMessages() =>
			Messenger<BufferDepthMessage>.DefaultMessenger.Register(this, OnSetBufferDepth, MessageContext.SelectedBufferDepth);

		private void OnSetBufferDepth(BufferDepthMessage bufferDepth) => Depth = bufferDepth.BufferDepth;

		#endregion

		#region Commands

		private void LoadCommands()
		{
			CancelCommand = new CustomCommand(CancelAction);
			OkCommand = new CustomCommand(OkAction);
		}

		public ICommand CancelCommand { get; set; }

		public ICommand OkCommand { get; set; }

		private void CancelAction(object _)
		{
			Messenger<BufferDepthMessage>.DefaultMessenger.Unregister(this, MessageContext.SelectedBufferDepth);
			Messenger<CloseDialog>.DefaultMessenger.Send(new CloseDialog(), MessageContext.CloseBufferDepth);
		}

		private void OkAction(object _)
		{
			Messenger<BufferDepthMessage>.DefaultMessenger.Send(new BufferDepthMessage(Depth), MessageContext.BufferDepth);
			CancelAction(null);
		}

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		#endregion

	}
}
