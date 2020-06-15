
namespace Rotate.Pictures.EventAggregator
{
	public interface ISubscriber<T>
	{
		void OnEvent(T e);
	}
}
