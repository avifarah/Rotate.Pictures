using System;

namespace Rotate.Pictures.Model
{
	public class PictureRetrievingEventArgs : EventArgs
	{
		public PictureRetrievingEventArgs(string currentPictureDirectory)
		{
			CurrentPictureDirectory = currentPictureDirectory;
		}

		public string CurrentPictureDirectory { get; }
	}
}
