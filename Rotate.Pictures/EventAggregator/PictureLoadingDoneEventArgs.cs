using System;

namespace Rotate.Pictures.EventAggregator
{
	/// <summary>
	/// An event data (EventArgs) that we pass around.
	///
	/// This EventArgs notifies that the system is done loading pictures.
	/// The system has gone through all the directories and subdirectories
	/// as per Configuration file.
	/// </summary>
	public class PictureLoadingDoneEventArgs : EventArgs
	{
		public bool RetrieveCompleted { get; }

		public PictureLoadingDoneEventArgs(bool retrieveCompleted) => RetrieveCompleted = retrieveCompleted;
	}
}
