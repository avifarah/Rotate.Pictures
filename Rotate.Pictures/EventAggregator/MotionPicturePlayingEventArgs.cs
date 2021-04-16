using System;

namespace Rotate.Pictures.EventAggregator
{
	public class MotionPicturePlayingEventArgs : EventArgs
	{
		public bool IsMotionPicturePlaying { get; }

		public MotionPicturePlayingEventArgs(bool IsMotionPicturePlaying)
		{
			IsMotionPicturePlaying = IsMotionPicturePlaying;
		}
	}
}
