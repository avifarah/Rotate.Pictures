using System;

namespace Rotate.Pictures.EventAggregator
{
	public interface IEventAggregator
	{
		void Subscribe(object subscriber);

		void Publish<TEvent>(TEvent eventToPublish) where TEvent : EventArgs;
	}
}
