using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
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
			var configValue = ConfigValueProvider.Default;
			_originalDepth = configValue.MaxPictureTrackerDepth();
		}

		private int _depth;
		private int _originalDepth;

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
			Messenger<BufferDepthMessage>.Instance.Register(this, OnSetBufferDepth, MessageContext.SelectedBufferDepth);

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

		private void CancelAction()
		{
			Messenger<BufferDepthMessage>.Instance.Unregister(this, MessageContext.SelectedBufferDepth);
			Messenger<CloseDialog>.Instance.Send(new CloseDialog(), MessageContext.CloseBufferDepth);
		}

		private void OkAction()
		{
			if (Depth < 3 * _originalDepth / 4)
			{
				var msg = $"You are about to shrink the buffer depth from {_originalDepth} to {Depth}";
				var res = MessageBox.Show($"{msg}{Environment.NewLine}{Environment.NewLine}Are you sure?",
					"Rotating.Pictures", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
				if (res == MessageBoxResult.No) return;
			}
			Messenger<BufferDepthMessage>.Instance.Send(new BufferDepthMessage(Depth), MessageContext.BufferDepth);
			CancelAction();
		}

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		#endregion

	}
}
