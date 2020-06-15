using System;

namespace Rotate.Pictures.Model
{
	/// <summary>
	/// Purpose:
	///		Handler communication for processing a directory
	/// </summary>
	public class PictureRetrievingEventArgs : EventArgs
	{
		public PictureRetrievingEventArgs(string currentPictureDirectory)
		{
			CurrentPictureDirectory = currentPictureDirectory;
		}

		public string CurrentPictureDirectory { get; }
	}
}
