using System;


namespace Rotate.Pictures.Utility
{
	public interface ICommand<in T>
	{
		/// <summary>Occurs when changes occur that affect whether or not the command should execute.</summary>
		event EventHandler CanExecuteChanged;

		/// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
		/// <returns>
		/// <see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.</returns>
		bool CanExecute(T parameter);

		/// <summary>Defines the method to be called when the command is invoked.</summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
		void Execute(T parameter);
	}
}
