
namespace Rotate.Pictures.MessageCommunication
{
	/// <summary>
	/// Purpose:
	///		Communication payload of a picture to be added/removed from the DoNotDisplay collection.
	///		Action is to be taken by picture path
	/// </summary>
	public class DoNotDisplayPathMessage : IVmCommunication
	{
		public string PicPath { get; }

		public DoNotDisplayPathMessage(string picPath) => PicPath = picPath;
	}
}
