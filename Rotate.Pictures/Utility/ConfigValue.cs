using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using System.Configuration;
using System.Globalization;


namespace Rotate.Pictures.Utility
{
	public class ConfigValue
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

			_initialPictureDirectories = dirs.Split(new[] { ';' });
		}

		public string[] InitialPictureDirectories()
		{
			if (_initialPictureDirectories != null) return _initialPictureDirectories;

			const string key = "Initial Folders";
			string[] defaultFolder = { @"G:\Pictures" };
			var rawConfig = ReadConfigValue(key);
			if (rawConfig == null)
			{
				Log.Error($"Configuration appSettings entry \"{key}\" is missing.");
				return defaultFolder;
			}

			_initialPictureDirectories = rawConfig.Split(';');
			return _initialPictureDirectories;
		}

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

		public int MaxPictureTrackerDepth()
		{
			if (_maxTrackingDepth > 0) return _maxTrackingDepth;

			const string key = "Max picture tracker depth";
			var raw = ReadConfigValue(key);
			if (raw == null)
			{
				Log.Error($"Missing configuration appSettings entry {key}");
				return DefPictureBufferDepth;
			}

			var rc = int.TryParse(raw, out int depth);
			if (!rc)
			{
				Log.Error($"Configuration appSettings entry {key}'s value is not a valid integer");
				_maxTrackingDepth = DefPictureBufferDepth;
			}
			else
				_maxTrackingDepth = depth;

			return _maxTrackingDepth;
		}

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

				const string key2 = "Motion pictures";
				var raw2 = ReadConfigValue(key2);
				if (raw2 == null)
					Log.Error($"Missing configuration appSettings entry {key2}");
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
				const string key1 = "Still pictures";
				var raw1 = ReadConfigValue(key1);
				if (raw1 == null)
					Log.Error($"Missing configuration appSettings entry {key1}");
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

		public void SetStillExtension(string stillExt)
		{
			if (string.IsNullOrWhiteSpace(stillExt))
			{
				_stillExt = new List<string>();
				return;
			}

			_stillExt = stillExt.Split(new[] { ';' }).Where(s => s.StartsWith(".")).ToList();
		}

		public List<string> StillPictureExtensions()
		{
			if (_stillExt != null) return _stillExt;

			const string key = "Still pictures";
			var raw = ReadConfigValue(key);
			if (raw == null)
			{
				Log.Error($"Missing configuration appSettings entry {key}");
				return _defStillExt;
			}

			_stillExt = raw.Split(';').ToList();
			return _stillExt;
		}

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

			_motionExt = motionExt.Split(new[] { ';' }).Where(m => m.StartsWith(".")).ToList();
			if (_motionExt == null || _motionExt.Count == 0) return;

			var stillExt = StillPictureExtensions();
			if (stillExt == null) return;

			_motionExt = _motionExt.Except(
					stillExt,
					(x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase) == 0,
					x => x.ToLower().GetHashCode())
				.ToList();
		}

		public List<string> MotionPictures()
		{
			if (_motionExt != null) return _motionExt;

			const string key = "Motion pictures";
			var raw = ReadConfigValue(key);
			if (raw == null)
			{
				Log.Error($"Missing configuration appSettings entry {key}");
				return _defMotionExt;
			}

			var motionPics = raw.Split(';').ToList();
			_motionExt = motionPics.Except(
					StillPictureExtensions(),
					(x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase) == 0,
					x => x.ToLower().GetHashCode())
				.ToList();
			return _motionExt;
		}

		public string ImageStretch()
		{
			const string key = "Image stretch";
			const string defaultStretch = "Uniform";
			var allowedStretchedValues = new List<string> { "Fill", "None", "Uniform", "UniformToFill" };
			var raw = ReadConfigValue(key);
			if (raw == null)
			{
				Log.Error($"Missing configuration appSettings entry {key}");
				return defaultStretch;
			}

			var stretch = allowedStretchedValues.FirstOrDefault(s => string.Compare(s, raw, StringComparison.CurrentCultureIgnoreCase) == 0);
			return stretch ?? defaultStretch;
		}

		private int _intervalBetweenPics = -1;

		/// <summary>
		/// Output in milliseconds
		/// </summary>
		/// <returns></returns>
		public int IntervalBetweenPictures()
		{
			if (_intervalBetweenPics > 0) return _intervalBetweenPics;

			const string key = "Timespan between pictures [Seconds]";
			const int defInterval = 10_000;
			var raw = ReadConfigValue(key);
			if (raw == null)
			{
				Log.Error($"Missing configuration appSettings entry {key}");
				return defInterval;
			}

			var rc = double.TryParse(raw, out double dblIntervalSec);
			if (!rc)
			{
				Log.Error($"Configuration appSettings entry {key}'s value is not a valid number");
				_intervalBetweenPics = defInterval;
			}
			else
				_intervalBetweenPics = (int)(dblIntervalSec * 1000);
			return _intervalBetweenPics;
		}

		private string _firstPic;

		public void SetFirstPic(string firstPic) => _firstPic = string.IsNullOrWhiteSpace(firstPic) ? null : firstPic;

		public string FirstPictureToDisplay()
		{
			if (!string.IsNullOrWhiteSpace(_firstPic)) return _firstPic;

			const string key = "First picture to display";
			var raw = ReadConfigValue(key);
			if (raw == null) Log.Error($"Missing configuration appSettings entry {key}");
			_firstPic = raw;
			return _firstPic;
		}

		public bool RotatingPicturesInit()
		{
			const string key = "On start image rotating";
			const bool defRotateInit = true;
			var raw = ReadConfigValue(key);
			if (raw == null)
			{
				Log.Error($"Missing configuration appSettings entry {key}");
				return defRotateInit;
			}

			if (raw.IsTrue()) return true;
			if (raw.IsFalse()) return false;
			Log.Error($"Configuration value for \"{key}\": \"{raw}\", is neither true nor false.  The default value of \"{defRotateInit}\" will be used");
			return defRotateInit;
		}

		public int VisualHeartbeat()
		{
			const int defHeartbeat = 100;
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

		private string ReadConfigValue(string key)
		{
			if (!_configValues.ContainsKey(key)) return null;

			return _configValues[key];
		}
	}
}
