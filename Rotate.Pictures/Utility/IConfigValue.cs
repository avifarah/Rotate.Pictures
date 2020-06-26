using System.Collections.Generic;

namespace Rotate.Pictures.Utility
{
	public interface IConfigValue
	{
		void SetInitialPictureDirectories(string dirs);

		List<string> FileExtensionsToConsider();

		string RestoreStillExtensions { get; }

		string FilePathToSavePicturesToAvoid();
		
		string FirstPictureToDisplay();

		bool RotatingPicturesInit();

		string ImageStretch();
		
		string[] InitialPictureDirectories();
		
		int IntervalBetweenPictures();
		
		int MaxPictureTrackerDepth();
		
		double MediaFastForward();
		
		List<string> MotionPictures();
		
		IEnumerable<string> PicturesToAvoidPaths();
		
		int SetMaxTrackingDepth(int depth);
		
		void SetMotionExtension(string motionExt);
		
		void SetStillExtension(string stillExt);
		
		List<string> StillPictureExtensions();
		
		void UpdateFirstPictureToDisplay(string firstPicture);
		
		void UpdateImageToStretch(SelectedStretchMode mode);
		
		void UpdateInitialPictureDirectories(string directory);
		
		void UpdateIntervalBetweenPictures(int intervalBetweenPics);

		void SetFirstPic(string firstPic);

		void UpdateMaxPictureTrackerDepth(int depth);
		
		void UpdateMotionPictures(string motionPicExt);
		
		void UpdateOnStartRotatingPicture(bool initialRotatingMode);
		
		void UpdatePicturesToAvoid(IEnumerable<string> picsToAvoid = null);
		
		void UpdateStillPictureExtensions(string stillPictureExt);

		string RestoreMotionExtensions { get; }

		int VisualHeartbeat();
	}
}