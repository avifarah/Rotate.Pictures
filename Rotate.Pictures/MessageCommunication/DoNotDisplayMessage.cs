
namespace Rotate.Pictures.MessageCommunication
{
	/// <summary>
	/// Purpose:
	///		Communication payload a picture index to add to the DoNot display collection
	/// </summary>
	public class DoNotDisplayMessage : IVmCommunication
	{
		public int PicIndex { get; }

		public DoNotDisplayMessage(int picIndex) => PicIndex = picIndex;
	}
}
