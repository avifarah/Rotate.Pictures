using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using System.Configuration;
using System.Globalization;
using System.IO;


namespace Rotate.Pictures.Utility
{
	public class ConfigValue : IConfigValue
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly Lazy<ConfigValue> _inst = new Lazy<ConfigValue>(() => new ConfigValue());
		public static readonly ConfigValue Inst = _inst.Value;

		private readonly Dictionary<string, string> _configValues;

		private ConfigValue() => _configValues = ConfigurationManager.AppSettings.AllKeys.ToDictionary(key => key, key => ConfigurationManager.AppSettings[key]);

		private string[] _initialPictureDirectories;

		public void SetInitialPictureDirectories(string dirs)
		{
			if (string.IsNullOrWhiteSpace(dirs))
			{
				_initialPictureDirectories = null;
				return;
			}

			_initialPictureDirectories = dirs.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		}

		private const string InitialPictureDirectoriesKey = "Initial Folders";

		public string[] InitialPictureDirectories()
		{
			if (_initialPictureDirectories != null) return _initialPictureDirectories;

			string[] defaultFolder = { @"C:\Pictures" };
			var rawConfig = ReadConfigValue(InitialPictureDirectoriesKey);
			if (rawConfig == null)
			{
				Log.Error($"Configuration appSettings entry \"{InitialPictureDirectoriesKey}\" is missing.");
				return defaultFolder;
			}

			_initialPictureDirectories = rawConfig.Split(';');
			return _initialPictureDirectories;
		}

		public void UpdateInitialPictureDirectories(string directory) => WriteConfigValue(InitialPictureDirectoriesKey, directory);

		private const int DefPictureBufferDepth = 1000;

		private int _maxTrackingDepth = -1;

		public int SetMaxTrackingDepth(int depth)
		{
			if (depth <= 0)
			{
				_maxTrackingDepth = 0;
				return DefPictureBufferDepth;
			}

			return _maxTrackingDepth = depth;
		}

		private const string MaxPictureTrackerDepthKey = "Max picture tracker depth";

		public int MaxPictureTrackerDepth()
		{
			if (_maxTrackingDepth > 0) return _maxTrackingDepth;

			var raw = ReadConfigValue(MaxPictureTrackerDepthKey);
			if (raw == null)
			{
				Log.Error($"Missing configuration appSettings entry {MaxPictureTrackerDepthKey}");
				return DefPictureBufferDepth;
			}

			var rc = int.TryParse(raw, out int depth);
			if (!rc)
			{
				Log.Error($"Configuration appSettings entry {MaxPictureTrackerDepthKey}'s value is not a valid integer");
				_maxTrackingDepth = DefPictureBufferDepth;
			}
			else
				_maxTrackingDepth = depth;

			return _maxTrackingDepth;
		}

		public void UpdateMaxPictureTrackerDepth(int depth) => WriteConfigValue(MaxPictureTrackerDepthKey, depth.ToString());

		public List<string> FileExtensionsToConsider()
		{
			List<string> MotionFromConfig(List<string> stillExtensions)
			{
				if (_motionExt != null)
				{
					if (stillExtensions == null) return _motionExt;

					var ext = stillExtensions.Union(
							_motionExt, (x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase) == 0,
							x => x.ToLower().GetHashCode())
						.ToList();
					return ext;
				}

				var raw2 = ReadConfigValue(MotionPictureExtensionsKey);
				if (raw2 == null)
					Log.Error($"Missing configuration appSettings entry {MotionPictureExtensionsKey}");
				else
					stillExtensions = stillExtensions.Union(
							raw2.Split(';').ToList(),
							(x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase) == 0,
							x => x.ToLower().GetHashCode())
						.ToList();
				return stillExtensions;
			}

			var defaultExtensions = _defStillExt.Union(
					_defMotionExt, (x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase) == 0,
					x => x.ToLower().GetHashCode())
				.ToList();

			List<string> extensions = null;
			if (_stillExt != null)
				extensions = _stillExt;
			else
			{
				var raw1 = ReadConfigValue(StillPictureExtensionsKey);
				if (raw1 == null)
					Log.Error($"Missing configuration appSettings entry {StillPictureExtensionsKey}");
				else
					extensions = raw1.Split(';').ToList();
			}

			extensions = extensions == null ? MotionPictures() : MotionFromConfig(extensions);
			var extToConsider = extensions ?? defaultExtensions;
			return extToConsider;
		}

		private readonly List<string> _defStillExt = new List<string> { ".jpg", ".bmp", ".gif", ".png", ".psd", ".tif" };

		public string RestoreStillExtensions => string.Join(";", _defStillExt.ToArray());

		private List<string> _stillExt;

		public void SetStillExtension(string stillExt) => _stillExt = StringToExtensionArray(stillExt).ToList();


		private const string StillPictureExtensionsKey = "Still pictures";

		public List<string> StillPictureExtensions()
		{
			if (_stillExt != null) return _stillExt;

			var raw = ReadConfigValue(StillPictureExtensionsKey);
			if (raw == null)
			{
				Log.Error($"Missing configuration appSettings entry {StillPictureExtensionsKey}");
				return _defStillExt;
			}

			_stillExt = StringToExtensionArray(raw).ToList();
			return _stillExt;
		}

		public void UpdateStillPictureExtensions(string stillPictureExt) => WriteConfigValue(StillPictureExtensionsKey, stillPictureExt);

		private readonly List<string> _defMotionExt = new List<string> { ".mov", ".avi", ".mpg", ".mp4", ".wmv", ".3gp" };

		public string RestoreMotionExtensions => string.Join(";", _defMotionExt.ToArray());

		private List<string> _motionExt;

		public void SetMotionExtension(string motionExt)
		{
			if (string.IsNullOrWhiteSpace(motionExt))
			{
				_motionExt = new List<string>();
				return;
			}

			_motionExt = StringToExtensionArray(motionExt).ToList();
			if (_motionExt == null || _motionExt.Count == 0) return;

			var stillExt = StillPictureExtensions();
			if (stillExt == null) return;

			// If an extension appears in both **Still pictures** and in **Motion pictures**
			// then the extension will be considered to appear in **Still pictures** only.
			_motionExt = _motionExt.Except(
					stillExt,
					(x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase) == 0,
					x => x.ToLower().GetHashCode())
				.ToList();
		}

		private const string MotionPictureExtensionsKey = "Motion pictures";

		public List<string> MotionPictures()
		{
			if (_motionExt != null) return _motionExt;

			var raw = ReadConfigValue(MotionPictureExtensionsKey);
			if (raw == null)
			{
				Log.Error($"Missing configuration appSettings entry {MotionPictureExtensionsKey}");
				return _defMotionExt;
			}

			SetMotionExtension(raw);
			return _motionExt;
		}

		public void UpdateMotionPictures(string motionPicExt) => WriteConfigValue(MotionPictureExtensionsKey, motionPicExt);


		private const string ImageToStretchKey = "Image stretch";

		public string ImageStretch()
		{
			const string defaultStretch = "Uniform";
			var allowedStretchedValues = new List<string> { "Fill", "None", "Uniform", "UniformToFill" };
			var raw = ReadConfigValue(ImageToStretchKey);
			if (raw == null)
			{
				Log.Error($"Missing configuration appSettings entry {ImageToStretchKey}");
				return defaultStretch;
			}

			var stretch = allowedStretchedValues.FirstOrDefault(s => string.Compare(s, raw, StringComparison.CurrentCultureIgnoreCase) == 0);
			return stretch ?? defaultStretch;
		}

		public void UpdateImageToStretch(SelectedStretchMode mode) => WriteConfigValue(ImageToStretchKey, mode.ToString());


		private int _intervalBetweenPics = -1;

		private const string IntervalBetweenPicturesKey = "Timespan between pictures [Seconds]";

		/// <summary>
		/// Output in milliseconds
		/// </summary>
		/// <returns></returns>
		public int IntervalBetweenPictures()
		{
			if (_intervalBetweenPics > 0) return _intervalBetweenPics;

			const int defInterval = 10_000;
			var raw = ReadConfigValue(IntervalBetweenPicturesKey);
			if (raw == null)
			{
				Log.Error($"Missing configuration appSettings entry {IntervalBetweenPicturesKey}");
				return defInterval;
			}

			var rc = double.TryParse(raw, out double dblIntervalSec);
			if (!rc)
			{
				Log.Error($"Configuration appSettings entry {IntervalBetweenPicturesKey}'s value is not a valid number");
				_intervalBetweenPics = defInterval;
			}
			else
				_intervalBetweenPics = (int)(dblIntervalSec * 1000);
			return _intervalBetweenPics;
		}

		public void UpdateIntervalBetweenPictures(int intervalBetweenPics)
		{
			var val = (intervalBetweenPics / 1000.0F).ToString(CultureInfo.CurrentCulture);
			WriteConfigValue(IntervalBetweenPicturesKey, val);
		}

		private string _firstPic;

		public void SetFirstPic(string firstPic) => _firstPic = string.IsNullOrWhiteSpace(firstPic) ? null : firstPic;

		private const string FirstPictureToDisplayKey = "First picture to display";

		public string FirstPictureToDisplay()
		{
			if (!string.IsNullOrWhiteSpace(_firstPic)) return _firstPic;

			var raw = ReadConfigValue(FirstPictureToDisplayKey);
			if (raw == null) Log.Error($"Missing configuration appSettings entry {FirstPictureToDisplayKey}");
			_firstPic = raw;
			return _firstPic;
		}

		public void UpdateFirstPictureToDisplay(string firstPicture) => WriteConfigValue(FirstPictureToDisplayKey, firstPicture);


		private const string RotatingInitKey = "On start image rotating";

		public bool RotatingPicturesInit()
		{
			const bool defRotateInit = true;
			var raw = ReadConfigValue(RotatingInitKey);
			if (raw == null)
			{
				Log.Error($"Missing configuration appSettings entry {RotatingInitKey}");
				return defRotateInit;
			}

			if (raw.IsTrue()) return true;
			if (raw.IsFalse()) return false;
			Log.Error($"Configuration value for \"{RotatingInitKey}\": \"{raw}\", is neither true nor false.  The default value of \"{defRotateInit}\" will be used");
			return defRotateInit;
		}

		public void UpdateOnStartRotatingPicture(bool initialRotatingMode) => WriteConfigValue(RotatingInitKey, initialRotatingMode.ToString());

		public int VisualHeartbeat()
		{
			const int defHeartbeat = 400;
			const string key = "Visual heartbeat";
			var raw = ReadConfigValue(key);
			if (raw == null)
			{
				Log.Error($"Missing configuration appSettings entry {key}");
				return defHeartbeat;
			}

			if (string.IsNullOrWhiteSpace(raw)) return defHeartbeat;
			if (string.Compare(raw, "default", StringComparison.OrdinalIgnoreCase) == 0) return defHeartbeat;

			const NumberStyles ns = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
			var rc = int.TryParse(raw, ns, CultureInfo.CurrentUICulture, out int visualHeartbeat);
			if (!rc)
			{
				Log.Error($"Configuration appSettings {key} entry's value is not a valid integer");
				return defHeartbeat;
			}

			return visualHeartbeat;
		}

		public double MediaFastForward()
		{
			const string key = "Fast Forward [Seconds]";
			const double defFastForward = 10.0;
			var raw = ReadConfigValue(key);
			if (raw == null)
			{
				Log.Error($"Missing configuration appSettings entry {key}");
				return defFastForward;
			}

			if (string.IsNullOrWhiteSpace(raw)) return defFastForward;
			if (string.Compare(raw, "default", StringComparison.OrdinalIgnoreCase) == 0) return defFastForward;

			var ns = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowThousands | NumberStyles.AllowTrailingWhite;
			var rc = double.TryParse(raw, ns, CultureInfo.CurrentUICulture, out double mff);
			if (!rc)
			{
				Log.Error($"Configuration appSettings {key} entry's value is not a valid number");
				return defFastForward;
			}

			return mff;
		}

		//private const string PicturesToAvoidKey = "Pictures Indices To Avoid.  Comma separated";
		private const string PicturesToAvoidFileName = "Pictures Path To Avoid.lst";

		public IEnumerable<string> PicturesToAvoidPaths()
		{
			var doNotDisplayPicPaths = new List<string>();
			var doNotDisplayFn = Path.GetFullPath(PicturesToAvoidFileName);
			if (!File.Exists(doNotDisplayFn)) return doNotDisplayPicPaths;

			using var sr = new StreamReader(doNotDisplayFn);
			while (!sr.EndOfStream)
			{
				var path = sr.ReadLine();
				if (!string.IsNullOrEmpty(path))
					doNotDisplayPicPaths.Add(path);
			}

			return doNotDisplayPicPaths;
			//var raw = ReadConfigValue(PicturesToAvoidKey);
			//if (raw == null)
			//{
			//	Log.Error($"Missing configuration appSettings entry {PicturesToAvoidKey}");
			//	return new List<int>();
			//}

			//var sIndices = raw.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			//var inxes = sIndices.Select(si => int.Parse(si));
			//return inxes.ToList();
		}

		public void UpdatePicturesToAvoid(IEnumerable<string> picsToAvoid)
		{
			var doNotDisplayFn = Path.GetFullPath(PicturesToAvoidFileName);

			if (picsToAvoid == null)
			{
				File.WriteAllText(doNotDisplayFn, string.Empty);
				return;
			}

			using var sw = new StreamWriter(doNotDisplayFn, false);
			foreach (var pic in picsToAvoid)
				sw.WriteLine(pic);
		}

		private const string FilePathToSavePicturesToAvoidKey = "FilePath to save Pictures to avoid";

		public string FilePathToSavePicturesToAvoid()
		{
			var filePath = ReadConfigValue(FilePathToSavePicturesToAvoidKey);
			return filePath;
		}

		private string ReadConfigValue(string key)
		{
			if (!_configValues.ContainsKey(key)) return null;

			return _configValues[key];
		}

		private void WriteConfigValue(string key, string value)
		{
			var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var entry = config.AppSettings.Settings[key];
			if (entry == null)
				throw new ArgumentException($"key \"{key}\" is not recognized", nameof(key));

			entry.Value = value;
			config.Save(ConfigurationSaveMode.Modified);

			ConfigurationManager.RefreshSection("appSettings");
		}

		private IEnumerable<string> StringToExtensionArray(string raw)
		{
			if (string.IsNullOrWhiteSpace(raw)) return new List<string>();

			return raw
				.Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Where(s => s.StartsWith(".")).ToList();
		}
	}
}
