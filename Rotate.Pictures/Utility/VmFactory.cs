using System;
using System.Reflection;

namespace Rotate.Pictures.Utility
{
	/// <summary>
	/// Use this Factory to divorces the View from the ViewModel.
	/// </summary>
	public sealed class VmFactory
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly Lazy<VmFactory> _inst = new Lazy<VmFactory>(() => new VmFactory());

		public static readonly VmFactory Inst = _inst.Value;

		private VmFactory() { }

		public object CreateVm(object callingView)
		{
			Type viewModel = GetVmType(callingView);
			if (viewModel == null) return null;

			return Activator.CreateInstance(viewModel);
		}

		private Type GetVmType(object callingView)
		{
			var callerName = callingView.GetType().FullName;
			if (callerName == null)
			{
				Log.Error($"callerName is null and did not generate a ViewModel class");
				return null;
			}

			var viewModelName = callerName.Replace("Rotate.Pictures.View.", "Rotate.Pictures.ViewModel.") + (callerName.EndsWith("View") ? "Model" : "ViewModel");
			var viewModel = Type.GetType(viewModelName);
			if (viewModel == null)
			{
				Log.Error($"{callerName} did not generate a ViewModel class");
				return null;
			}

			return viewModel;
		}
	}
}
