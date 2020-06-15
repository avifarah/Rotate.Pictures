using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Rotate.Pictures.EventAggregator
{
	/// <summary>
	/// Purpose:
	///		Provides an Aggregator for the EventAggregator.  This is the station where both Subscriber and
	///		Publisher access as this class as a pass through for events.
	///
	///		There are a few ways to implement an EventAggregator, this design is the one recommended by
	///		Jeremy D Miller, the architect of StructureMap
	/// 
	/// Usage:
	///		1.	You need an aggregate event, which is an extension class of the EventArgs class
	///		2.	The publisher, may be more than one, say a model class, needs to fire an event using the
	///			eventAggregator.  The publisher will issue a command, like:
	///				CustomEventAggregator.Inst.Publish({The event arg class instance that act as data-transport, from point 1});
	///			Other than this registration there is no more dependence on the EventAggregator class.
	///			Therefore, if the class instance was injected then we will have no dependency at all.
	/// 
	///		3.	The subscriber, say a ViewModel class needs to listen to the events published by the
	///			appropriate model class, will implements the ISubscriber{the event arg class, from point 1} interface.
	///			The subscriber class may implement more than one ISubscriber{} interfaces.  Each of
	///			the ISubscriber{} interfaces will need an implementation of an OnEvent(T e) method.  The various
	///			OnEvent(T e) methods are distinguished by the various types of T.
	///		4.	The subscriber class will need to call:
	///				CustomEventAggregator.Inst.Subscribe(this);
	///			before it can listen to events published.  Other than this registration we have no more coupling between
	///			The subscriber and the event aggregator.
	/// </summary>
	public class CustomEventAggregator : IEventAggregator
	{
		private readonly Dictionary<Type, List<WeakReference>> _eventSubscriberLists = new Dictionary<Type, List<WeakReference>>();
		private readonly object _lock = new object();

		private CustomEventAggregator() { }
		private static Lazy<CustomEventAggregator> _inst = new Lazy<CustomEventAggregator>(() => new CustomEventAggregator());
		public static CustomEventAggregator Inst = _inst.Value;

		public void Publish<TEvent>(TEvent eventToPublish) where TEvent : EventArgs
		{
			var subscriberType = typeof(ISubscriber<>).MakeGenericType(typeof(TEvent));
			var subscribers = GetSubscribers(subscriberType);
			var subscribersToRemove = new List<WeakReference>();

			foreach (var weakSubscriber in subscribers)
			{
				if (weakSubscriber.IsAlive)
				{
					var subscriber = (ISubscriber<TEvent>)weakSubscriber.Target;
					if (SynchronizationContext.Current == null)
					{
						subscriber.OnEvent(eventToPublish);
					}
					else
					{
						var syncContext = SynchronizationContext.Current;
						syncContext.Post(s => subscriber.OnEvent(eventToPublish), null);
					}
				}
				else
				{
					subscribersToRemove.Add(weakSubscriber);
				}
			}

			if (!subscribersToRemove.Any()) return;
			lock (_lock)
			{
				foreach (var remove in subscribersToRemove)
					subscribers.Remove(remove);
			}
		}

		public void Subscribe(object subscriber)
		{
			lock (_lock)
			{
				var interfaces = subscriber.GetType().GetInterfaces();
				var subscriberTypes = interfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISubscriber<>));

				var weakReference = new WeakReference(subscriber);
				foreach (var subscriberType in subscriberTypes)
				{
					var subscribers = GetSubscribers(subscriberType);
					subscribers.Add(weakReference);
				}
			}
		}

		private List<WeakReference> GetSubscribers(Type subscriberType)
		{
			List<WeakReference> subscribers;
			lock (_lock)
			{
				var found = _eventSubscriberLists.TryGetValue(subscriberType, out subscribers);
				if (found) return subscribers;

				subscribers = new List<WeakReference>();
				_eventSubscriberLists.Add(subscriberType, subscribers);
			}
			return subscribers;
		}
	}
}
