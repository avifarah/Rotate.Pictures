using System.Windows.Input;
using Rotate.Pictures.Service;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures.ViewModel
{
	public class MediaPlayerViewModel
	{
		public MediaPlayerViewModel()
		{
			LoadCommands();
		}

		public IMediaService MediaService {get; private set;}

		private void LoadCommands()
		{
			LoadedCommand = new CustomCommand<IMediaService>(OnLoaded);
			PlayCommand = new CustomCommand(OnPlay);
			PauseCommand = new CustomCommand(OnPause);
			StopCommand = new CustomCommand(OnStop);
			RewindCommand = new CustomCommand(OnRewind);
			FastForwardCommand = new CustomCommand(OnFastForward);
		}

		public ICommand<IMediaService> LoadedCommand { get; set; }

		public ICommand PlayCommand { get; set; }

		public ICommand PauseCommand { get; set; }

		public ICommand StopCommand { get; set; }

		public ICommand RewindCommand { get; set; }

		public ICommand FastForwardCommand { get; set; }

		private void OnLoaded(IMediaService mediaService) => MediaService = mediaService;

		private void OnPlay(object obj) => MediaService.Play();

		private void OnPause(object obj) => MediaService.Pause();

		private void OnStop(object obj) => MediaService.Stop();

		private void OnRewind(object obj) => MediaService.Rewind();

		private void OnFastForward(object obj) => MediaService.FastForward();
	}
}
