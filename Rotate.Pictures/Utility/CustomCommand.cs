using System;
using System.Windows.Input;


namespace Rotate.Pictures.Utility
{
	public sealed class CustomCommand : ICommand
	{
		private readonly Action<object> _execute;
		private readonly Predicate<object> _canExecute;

		public CustomCommand(Action<object> execute, Predicate<object> canExecute = null)
		{
			bool CanExecuteDefault(object _) => true;
			_execute = execute;
			_canExecute = canExecute ?? CanExecuteDefault;
		}

		public bool CanExecute(object parameter) => _canExecute(parameter);

		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		public void Execute(object parameter) => _execute(parameter);
	}

	public sealed class CustomCommand<T> : ICommand<T>
	{
		private readonly Action<T> _execute;
		private readonly Predicate<T> _canExecute;

		public CustomCommand(Action<T> execute, Predicate<T> canExecute = null)
		{
			bool CanExecuteDefault(T _) => true;
			_execute = execute;
			_canExecute = canExecute ?? CanExecuteDefault;
		}

		public bool CanExecute(T parameter) => _canExecute(parameter);

		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		public void Execute(T parameter) => _execute(parameter);
	}
}
