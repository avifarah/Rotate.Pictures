using System;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using Rotate.Pictures.EventAggregator;

namespace Rotate.Pictures.Utility
{
	/// <summary>
	/// See external reference:
	///	<see cref="https://docs.microsoft.com/en-us/dotnet/desktop/wpf/advanced/markup-extensions-and-wpf-xaml?view=netframeworkdesktop-4.8"/>
	/// See also <see cref="http://www.jonathanantoine.com/2011/09/23/wpf-4-5-part-6-markup-extensions-for-events-are-cools/"/>
	///			 <see cref="http://www.jonathanantoine.com/2011/09/23/wpf-4-5s-markupextension-invoke-a-method-on-the-viewmodel-datacontext-when-an-event-is-raised/"/>
	///
	/// A MarkupExtension, a class have to inherit from MarkupExtesion and implement the abstract ProvideValue method.
	/// </summary>
	public class MediaPlayerOnOpened : MarkupExtension
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private string ActionName { get; }

		public MediaPlayerOnOpened(string actionName)
		{
			ActionName = actionName;
		}

		/// <summary>
		/// It is called by the framework which provide an IServiceProvider object as a parameter.
		///
		/// This serviceProvider is a dependency resolver which you can use to retrieve a service
		/// named IProvideValueTarget. Then it will be used to obtain the property targeted by the
		/// MarkupExtension (you can get the Targeted object too with it).
		///
		/// This property is the Event’s accessor (the one which is called when you subscribe to
		/// it with the ‘+=’ syntax). Then, reflection has to be used to find the Type of the
		/// handler of the aimed event.
		///
		/// Once this is done, a Delegate can be created and returned as a provided value by this
		/// MarkupExtension.
		/// </summary>
		/// <param name="serviceProvider"></param>
		/// <returns></returns>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget targetProvider))
			{
				var msg = $"In {MethodBase.GetCurrentMethod().DeclaringType}.{MethodBase.GetCurrentMethod().Name}() failed to retrieve service for targetProvider.";
				Log.Error(msg);
				throw new InvalidOperationException(msg);
			}

			if (targetProvider.TargetObject is not FrameworkElement)
			{
				var msg = $"In {MethodBase.GetCurrentMethod().DeclaringType}.{MethodBase.GetCurrentMethod().Name}() can only operate on a FrameworkElement.";
				Log.Error(msg);
				throw new InvalidOperationException(msg);
			}

			var targetEventAddMethod = targetProvider.TargetProperty as MethodInfo;		// or EventInfo
			if (targetEventAddMethod == null)
			{
				var msg = $"In {MethodBase.GetCurrentMethod().DeclaringType}.{MethodBase.GetCurrentMethod().Name}() failed to get targetEventAddMethod.";
				Log.Error(msg);
				throw new InvalidOperationException(msg);
			}

			// Retrieve the handler of the event
			var pars = targetEventAddMethod.GetParameters();
			if (pars.Length < 2)
			{
				var msg = $"In {MethodBase.GetCurrentMethod().DeclaringType}.{MethodBase.GetCurrentMethod().Name}() failed to get parameter[1].";
				Log.Error(msg);
				throw new InvalidOperationException(msg);
			}

			var delegateType = pars[1].ParameterType;
			if (delegateType == null)
			{
				var msg = $"In {MethodBase.GetCurrentMethod().DeclaringType}.{MethodBase.GetCurrentMethod().Name}() failed to get delegateType.";
				Log.Error(msg);
				throw new InvalidOperationException(msg);
			}

			//Retrieves the method info of the proxy handler
			var methodInfo = GetType().GetMethod("MyProxyHandler", BindingFlags.NonPublic | BindingFlags.Instance);
			if (methodInfo == null)
			{
				var msg = $"In {MethodBase.GetCurrentMethod().DeclaringType}.{MethodBase.GetCurrentMethod().Name}() failed to find handler.";
				Log.Error(msg);
				throw new InvalidOperationException(msg);
			}

			// Create a delegate to the proxy handler on the markupExtension
			var returnedDelegate = Delegate.CreateDelegate(delegateType, this, methodInfo);

			return returnedDelegate;
		}

		void MyProxyHandler(object sender, EventArgs e)
		{
			var target = sender as FrameworkElement;
			var dataContext = target?.DataContext;
			if (dataContext == null) return;

			//get the method on the datacontext from its name
			var methodInfo = dataContext.GetType().GetMethod(ActionName, BindingFlags.Public | BindingFlags.Instance);
			methodInfo?.Invoke(dataContext, null);
		}

		void OnOpened(object sender, EventArgs e)
		{
			// Here something can be performed.
			//FrameworkElement target = sender as FrameworkElement;
			//if (target == null) return;
			//var dataContext = target.DataContext;
			//if (dataContext == null) return;

			////get the method on the datacontext from its name
			//MethodInfo methodInfo = dataContext.GetType().GetMethod(ActionName, BindingFlags.Public | BindingFlags.Instance);
			//methodInfo.Invoke(dataContext, null);

			// Set IsMotionPicturePlaying to false
			CustomEventAggregator.Inst.Publish(new MotionPicturePlayingEventArgs(false));
			CustomEventAggregator.Inst.Publish(new SliderPositionEventArgs(0.0, ConfigValue.Inst.IntervalBetweenPictures(), 0.0));

			// Set IsMotionPicturePlaying to true
			CustomEventAggregator.Inst.Publish(new MotionPicturePlayingEventArgs(true));
		}
	}
}
