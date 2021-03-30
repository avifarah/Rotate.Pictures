using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;

namespace Rotate.Pictures.Utility
{
	public class MethodBindingExtension : MarkupExtension
	{
		/// <summary>
		/// List of attached properties shared by all MethodBindings
		/// </summary>
		private static readonly List<DependencyProperty> StorageProperties = new();

		public override object ProvideValue(IServiceProvider serviceProvider) => throw new NotImplementedException();

		private readonly object[] _methodArguments;

		public string MethodName { get; }

		public PropertyPath MethodTargetPath { get; }

		public MethodBindingExtension(string path) : this(path, new object[] { null }) { }

		public MethodBindingExtension(string path, params object[] arguments)
		{
			_methodArguments = arguments ?? new object[0];

			int pathSeparatorIndex = path.LastIndexOf('.');

			if (pathSeparatorIndex != -1)
			{
				MethodTargetPath = new PropertyPath(path.Substring(0, pathSeparatorIndex), null);
			}

			MethodName = path.Substring(pathSeparatorIndex + 1);
		}

		private DependencyProperty GetUnusedStorageProperty(DependencyObject obj)
		{
			foreach (var property in StorageProperties)
			{
				if (obj.ReadLocalValue(property) == DependencyProperty.UnsetValue)
					return property;
			}

			var newProperty = DependencyProperty.RegisterAttached("Storage" + StorageProperties.Count, typeof(object), typeof(MethodBindingExtension), new PropertyMetadata());
			StorageProperties.Add(newProperty);

			return newProperty;
		}
	}
}
