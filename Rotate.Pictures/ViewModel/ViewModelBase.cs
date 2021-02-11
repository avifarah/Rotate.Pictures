using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Rotate.Pictures.ViewModel
{
	public abstract class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
							PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		#endregion

		#region IDataErrorInfo

		public string this[string propertyName]
		{
			get
			{
				if (!_errBinderMap.ContainsKey(propertyName)) return null;
				_errBinderMap[propertyName].ErrEvaluate();
				return _errBinderMap[propertyName].Error;
			}
		}

		public string Error
		{
			get
			{
				var errors = _errBinderMap.Values.Where(b => b.HasError).Select(b => b.Error);
				return string.Join(Environment.NewLine, errors);
			}
		}

		#endregion

		private readonly Dictionary<string, ErrBinder> _errBinderMap = new Dictionary<string, ErrBinder>();

		protected void AddRule(string propertyName, Func<bool> ruleDelegate, string errorMessage)
		{
			var rv = new ErrBinder(ruleDelegate, errorMessage);
			_errBinderMap.Add(propertyName, rv);
		}

		protected bool HasErrors
		{
			get
			{
				var values = _errBinderMap.Values.ToList();
				values.ForEach(b => b.ErrEvaluate());
				return values.Any(b => b.HasError);
			}
		}

		private class ErrBinder
		{
			private readonly Func<bool> _ruleValidate;
			private readonly string _message;

			internal ErrBinder(Func<bool> ruleValidate, string message)
			{
				_ruleValidate = ruleValidate;
				_message = message;
			}

			internal string Error { get; set; }

			internal bool HasError { get; set; }

			internal void ErrEvaluate()
			{
				Error = null;
				HasError = false;
				try
				{
					var rc = _ruleValidate();
					HasError = !rc;
					if (rc) return;

					Error = _message;
					HasError = true;
				}
				catch (Exception e)
				{
					Error = e.Message;
					HasError = true;
				}
			}
		}
	}
}
