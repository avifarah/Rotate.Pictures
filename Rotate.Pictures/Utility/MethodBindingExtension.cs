using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rotate.Pictures.Utility
{
	/// <summary>
	/// See external reference
	///		<see cref="https://docs.microsoft.com/en-us/dotnet/desktop/wpf/advanced/markup-extensions-and-wpf-xaml?view=netframeworkdesktop-4.8"/>
	/// </summary>
	public class MethodBindingExtension : MarkupExtension
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly object[] _methodArguments;

		private string _methodName { get; }

		private PropertyPath _methodTargetPath { get; }

		public MethodBindingExtension(string path) : this(path, new object[] { null }) { }

		public MethodBindingExtension(string path, params object[] arguments)
		{
			_methodArguments = arguments ?? new object[0];

			var pathSeparatorIndex = path.LastIndexOf('.');
			if (pathSeparatorIndex != -1)
				_methodTargetPath = new PropertyPath(path.Substring(0, pathSeparatorIndex), null);

			_methodName = path.Substring(pathSeparatorIndex + 1);
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var targetProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
			if (targetProvider == null)
			{
				var msg = $"In {MethodBase.GetCurrentMethod().DeclaringType}.{MethodBase.GetCurrentMethod().Name}() failed to retrieve service for targetProvider.";
				Log.Error(msg);
				throw new InvalidOperationException(msg);
			}

			var targetEventAddMethod = targetProvider.TargetProperty as MethodInfo;
			if (targetEventAddMethod == null)
			{
				var msg = $"In {MethodBase.GetCurrentMethod().DeclaringType}.{MethodBase.GetCurrentMethod().Name}() failed to get targetEventAddMethod.";
				Log.Error(msg);
				throw new InvalidOperationException(msg);
			}

			// Retrieve the handler of the event
			var pars = targetEventAddMethod.GetParameters();
			var delegateType = pars[1].ParameterType;
			if (delegateType == null)
			{
				var msg = $"In {MethodBase.GetCurrentMethod().DeclaringType}.{MethodBase.GetCurrentMethod().Name}() failed to get delegateType.";
				Log.Error(msg);
				throw new InvalidOperationException(msg);
			}

			// Retrieves the method info of the proxy handler
			var methodInfo = GetType().GetMethod("MyMarkupExtensionInternalHandler", BindingFlags.NonPublic | BindingFlags.Instance);
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

		void MyMarkupExtensionInternalHandler(object sender, EventArgs e)
		{
			// Here something can be performed.
		}
	}
}
