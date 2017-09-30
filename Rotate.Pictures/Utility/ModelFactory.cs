using System;
using System.Reflection;
using Rotate.Pictures.Model;


namespace Rotate.Pictures.Utility
{
	/// <summary>
	/// Use this Factory to separate the ViewModel from the model.
	/// In our case we have one model class: PictureModel
	/// </summary>
	public sealed class ModelFactory
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static Lazy<ModelFactory> _inst = new Lazy<ModelFactory>(() => new ModelFactory());

		public static readonly ModelFactory Inst = _inst.Value;

		private ModelFactory() { }

		public object Create(string modelName)
		{
			switch (modelName)
			{
				case "PictureFileRepository":
					return new PictureModel();

				default:
					Log.Error($"{modelName} is not a valid Model");
					return null;
			}
		}
	}
}
