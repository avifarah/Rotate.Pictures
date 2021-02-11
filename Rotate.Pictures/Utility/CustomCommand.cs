using System;
using System.Reflection;
using System.Windows.Input;
using log4net;

namespace Rotate.Pictures.Utility
{
	/// <summary>
	/// Purpose:
	///		Implements a custom command
	/// </summary>
	public sealed class CustomCommand : ICommand
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Action<object> _execute;
		private readonly Predicate<object> _canExecute;

		/// <summary>
		/// execute have no parameters
		/// no canExecute function parameter
		/// </summary>
		public CustomCommand(Action execute) : this(obj => execute()) { }

		/// <summary>
		/// execute has no parameters
		/// canExecute has no parameters
		/// </summary>
		public CustomCommand(Action execute, Func<bool> canExecute) : this(obj => execute(), obj => canExecute()) {}

		/// <summary>
		/// execute has an object parameter
		/// canExecute has an object parameters or is missing
		/// </summary>
		public CustomCommand(Action<object> execute, Predicate<object> canExecute = null)
		{
			if (execute == null)
			{
				var errMsg = $"{nameof(execute)} cannot be null";
				Log.Error(errMsg);
				throw new ArgumentNullException(nameof(execute), errMsg);
			}

			bool CanExecuteDefault(object _) => true;
			_execute = execute;
			_canExecute = canExecute ?? CanExecuteDefault;
		}

		/// <summary>
		/// Implementation of the CanExecute
		/// </summary>
		public bool CanExecute(object parameter) => _canExecute(parameter);

		/// <summary>
		/// Implementation of Execute
		/// </summary>
		public void Execute(object parameter) => _execute(parameter);

		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}
	}

	/// <summary>
	/// The CustomCommand{T} does not have an implementation of no parameter
	/// execute or canExecute.  It will make no sense to have a type for a
	/// non existent parameter.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class CustomCommand<T> : ICommand<T>, ICommand
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Action<T> _execute;
		private readonly Predicate<T> _canExecute;

		public CustomCommand(Action<T> execute, Predicate<T> canExecute = null)
		{
			if (execute == null)
			{
				var errMsg = $"{nameof(execute)} cannot be null";
				Log.Error($"{nameof(execute)} cannot be null");
				throw new ArgumentNullException(nameof(execute), errMsg);
			}

			bool CanExecuteDefault(T _) => true;
			_execute = execute;
			_canExecute = canExecute ?? CanExecuteDefault;
		}

		public bool CanExecute(T parameter) => _canExecute(parameter);

		public void Execute(T parameter) => _execute(parameter);

		bool ICommand.CanExecute(object parameter) => CanExecute((T)parameter);

		void ICommand.Execute(object parameter) => Execute((T)parameter);

		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}
	}
}
