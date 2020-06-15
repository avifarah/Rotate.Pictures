using System.Collections.Generic;

namespace Rotate.Pictures.MessageCommunication
{
	/// <summary>
	/// Purpose:
	///		Communication payload list of pictures not to be displayed
	/// </summary>
	public class LoadNoDisplayPicturesMessage : IVmCommunication
	{
		public List<string> PicturesToAvoid { get; }

		public LoadNoDisplayPicturesMessage(IEnumerable<string> picturesToAvoid)
		{
			PicturesToAvoid = new List<string>();
			PicturesToAvoid.AddRange(picturesToAvoid);
		}
	}
}
