using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
//using System.Reflection;


namespace Rotate.Pictures.MessageCommunication
{
	public class MessageBase
	{
		protected static readonly object SyncLock = new object();
	}

	public class Messenger<T> : MessageBase where T : IVmCommunication
	{
		//private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly ConcurrentDictionary<MessengerKey, object> MsgRepository = new ConcurrentDictionary<MessengerKey, object>();

		#region DefaultMessenger property

		private static volatile Messenger<T> _instance;

		/// <summary>
		/// Gets the single instance of the Messenger.
		/// </summary>
		public static Messenger<T> DefaultMessenger
		{
			get
			{
				if (_instance != null) return _instance;

				lock (SyncLock)
				{
					if (_instance != null) return _instance;
					_instance = new Messenger<T>();
				}

				return _instance;
			}
		}

		#endregion

		/// <summary>
		/// Initializes a new instance of the Messenger class.
		/// </summary>
		private Messenger() { }

		/// <summary>
		/// Registers a recipient for a type of message T. The sendMessageToRecipientsAction parameter will be executed
		/// when a corresponding message is sent.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="recipient"></param>
		/// <param name="sendMessageToRecipientsAction"></param>
		public void Register(object recipient, Action<T> sendMessageToRecipientsAction) => Register(recipient, sendMessageToRecipientsAction, null);

		/// <summary>
		/// Registers a recipient for a type of message T and a matching context. The sendMessageToRecipientsAction parameter will be executed
		/// when a corresponding message is sent.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="recipient"></param>
		/// <param name="sendMessageToRecipientsAction"></param>
		/// <param name="context"></param>
		public void Register(object recipient, Action<T> sendMessageToRecipientsAction, object context)
		{
			var key = new MessengerKey(recipient, context);
			MsgRepository.TryAdd(key, sendMessageToRecipientsAction);
		}

		/// <summary>
		/// Unregisters a messenger recipient completely. After this method is executed, the recipient will
		/// no longer receive any messages.
		/// </summary>
		/// <param name="recipient"></param>
		public void Unregister(object recipient) => Unregister(recipient, null);

		/// <summary>
		/// Unregisters a messenger recipient with a matching context completely. After this method is executed, the recipient will
		/// no longer receive any messages.
		/// </summary>
		/// <param name="recipient"></param>
		/// <param name="context"></param>
		public void Unregister(object recipient, object context)
		{
			var key = new MessengerKey(recipient, context);
			MsgRepository.TryRemove(key, out var sendMessageToRecipientsAction);
		}

		/// <summary>
		/// Sends a message to registered recipients. The message will reach all recipients that are
		/// registered for this message type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		public void Send(T message) => Send(message, null);

		/// <summary>
		/// Sends a message to registered recipients. The message will reach all recipients that are
		/// registered for this message type and matching context.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		/// <param name="context"></param>
		public void Send(T message, object context)
		{
			bool MessagePredicate(object cxt, KeyValuePair<MessengerKey, object> keyValue)
			{
				if (context == null) return keyValue.Key == null;
				return keyValue.Key.Context != null && keyValue.Key.Context.Equals(cxt);
			}

			var result = MsgRepository.Where(r => MessagePredicate(context, r));
			foreach (var sendMessageToRecipientsAction in result.Select(x => x.Value).OfType<Action<T>>())
				sendMessageToRecipientsAction(message);
		}

		protected class MessengerKey : IEquatable<MessengerKey>
		{
			public object Recipient { get; }

			public object Context { get; }

			/// <summary>
			/// Initializes a new instance of the MessengerKey class.
			/// </summary>
			/// <param name="recipient"></param>
			/// <param name="context"></param>
			public MessengerKey(object recipient, object context)
			{
				Recipient = recipient;
				Context = context;
			}

			/// <summary>
			/// Determines whether the specified MessengerKey is equal to the current MessengerKey.
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			protected bool Equals(MessengerKey other) => Equals(Recipient, other.Recipient) && Equals(Context, other.Context);

			bool IEquatable<MessengerKey>.Equals(MessengerKey other) => Equals(other);

			/// <summary>
			/// Determines whether the specified MessengerKey is equal to the current MessengerKey.
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != GetType()) return false;

				return Equals((MessengerKey)obj);
			}

			/// <summary>
			/// Serves as a hash function for a particular type. 
			/// </summary>
			/// <returns></returns>
			public override int GetHashCode()
			{
				unchecked
				{
					return ((Recipient != null ? Recipient.GetHashCode() : 0) * 397) ^ (Context != null ? Context.GetHashCode() : 0);
				}
			}
		}
	}
}
