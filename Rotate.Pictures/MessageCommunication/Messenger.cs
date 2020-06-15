using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Rotate.Pictures.MessageCommunication
{
	public abstract class MessageBase
	{
		protected static readonly object SyncLock = new object();
	}

	/// <summary>
	/// Purpose:
	///		Communicate through Messenger
	/// </summary>
	/// <typeparam name="TPayload">This is the payload of the message</typeparam>
	public class Messenger<TPayload> : MessageBase where TPayload : IVmCommunication
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly ConcurrentDictionary<MessengerKey, object> MsgRepository = new ConcurrentDictionary<MessengerKey, object>();

		private readonly object _internalContext = new object();

		#region Instance property

		private static volatile Messenger<TPayload> _instance;

		/// <summary>Gets the single instance of the Messenger.</summary>
		public static Messenger<TPayload> Instance
		{
			get
			{
				if (_instance != null) return _instance;

				lock (SyncLock)
				{
					if (_instance != null) return _instance;
					_instance = new Messenger<TPayload>();
				}

				return _instance;
			}
		}

		#endregion

		/// <summary>Initializes a new instance of the Messenger class.</summary>
		private Messenger() { }

		public void Register(object recipient, Action sendMessageToRecipientsAction)
			=> Register(recipient, _ => sendMessageToRecipientsAction(), _internalContext);

		/// <summary>
		/// Registers a recipient for a type of message T.
		/// The sendMessageToRecipientsAction parameter will be executed
		/// when a corresponding message is sent.
		/// <see cref="Register(object, Action{TPayload}, object)"/>
		/// </summary>
		public void Register(object recipient, Action<TPayload> sendMessageToRecipientsAction)
			=> Register(recipient, sendMessageToRecipientsAction, _internalContext);

		public void Register(object recipient, Action sendMessageToRecipientsAction, object context)
			=> Register(recipient, _ => sendMessageToRecipientsAction(), context);

		/// <summary>
		/// Registers a recipient for a type of message T and a matching context.
		/// The sendMessageToRecipientsAction parameter will be executed
		/// when a corresponding message is sent.
		/// </summary>
		/// <typeparam name="TPayload">Type of message</typeparam>
		/// <param name="recipient">In many cases the recipient is the "this" reference.  This is the recipient who implements the action.</param>
		/// <param name="sendMessageToRecipientsAction">Action to be taken with the message</param>
		/// <param name="context">Allows for the messages to be part of a context</param>
		public void Register(object recipient, Action<TPayload> sendMessageToRecipientsAction, object context)
		{
			var key = new MessengerKey(recipient, context);
			MsgRepository.TryAdd(key, sendMessageToRecipientsAction);
		}

		/// <summary>
		/// Unregisters a messenger recipient completely. After this method is executed, the recipient will
		/// no longer receive any messages.
		/// <see cref="Unregister(object, object)"/>
		/// </summary>
		public void Unregister(object recipient) => Unregister(recipient, _internalContext);

		/// <summary>
		/// Unregisters a messenger recipient with a matching context completely. After this method is executed, the recipient will
		/// no longer receive any messages.
		/// </summary>
		/// <param name="recipient">The same recipient who was given during the registration</param>
		/// <param name="context">The same context that was given during registration</param>
		public void Unregister(object recipient, object context)
		{
			var key = new MessengerKey(recipient, context);
			MsgRepository.TryRemove(key, out _);
		}

		/// <summary>
		/// Sends a message to registered recipients. The message will reach all recipients that are
		/// registered for this message type.
		/// <see cref="Send(TPayload,object)"/>
		/// </summary>
		public void Send(TPayload message) => Send(message, _internalContext);

		/// <summary>
		/// Sends a message to registered recipients. The message will reach all recipients that are
		/// registered for this message type and matching context.
		/// </summary>
		/// <typeparam name="TPayload">The type of the message to send</typeparam>
		/// <param name="message">The message itself</param>
		/// <param name="context">Same context as per registration</param>
		public void Send(TPayload message, object context)
		{
			bool MessagePredicate(object cxt, KeyValuePair<MessengerKey, object> keyValue)
			{
				if (context == null) return keyValue.Key == null;
				return keyValue.Key.Context != null && keyValue.Key.Context.Equals(cxt);
			}

			var result = MsgRepository.Where(r => MessagePredicate(context, r));
			var sendMessagesActions = result.Select(x => x.Value).OfType<Action<TPayload>>();
			sendMessagesActions.ToList().ForEach(act => act(message));
		}

		protected class MessengerKey : IEquatable<MessengerKey>
		{
			/// <summary>Recipient is the object containing the action</summary>
			public object Recipient { get; }

			/// <summary>
			/// Context is a separator for messages.
			/// Context allows more than one message name to be routed to the correct recipient where
			/// the distinction between the recipient destinations is the context.
			/// </summary>
			public object Context { get; }

			/// <summary>Initializes a new instance of the MessengerKey class.</summary>
			public MessengerKey(object recipient, object context)
			{
				Recipient = recipient;
				Context = context;
			}

			/// <summary>Determines whether the specified MessengerKey is equal to the current MessengerKey.</summary>
			protected bool Equals(MessengerKey other) => Equals(Recipient, other.Recipient) && Equals(Context, other.Context);

			bool IEquatable<MessengerKey>.Equals(MessengerKey other) => Equals(other);

			/// <summary>Determines whether the specified MessengerKey is equal to the current MessengerKey.</summary>
			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != GetType()) return false;

				return Equals((MessengerKey)obj);
			}

			/// <summary>Serves as a hash function for a particular type.</summary>
			public override int GetHashCode()
			{
				var rHash = Recipient?.GetHashCode() ?? 0;
				var cHash = Context?.GetHashCode() ?? 0;
				unchecked { return (rHash * 397) ^ cHash; }
			}
		}
	}
}
