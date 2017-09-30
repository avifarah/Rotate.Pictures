using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures.Model
{
	/// <summary>
	/// Repository is a file repository
	/// </summary>
	public class PictureModel
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>Collection of all pictures in all directories supplied by the user in the configuration file</summary>
		private readonly PictureCollection _picCollection = new PictureCollection();

		/// <summary>Extensions to consider</summary>
		private List<string> _extions;

		private readonly Random _rand = new Random();
		private Task _taskModel;
		private CancellationTokenSource _cts;

		/// <summary>
		/// .ctor
		/// Retrieve pictures asynchrounously
		/// </summary>
		public PictureModel() => Restart();

		public void Restart()
		{
			if (_taskModel != null)
			{
				_cts.Cancel();
				try
				{
					// Note that _taskModel.Wait() will not work.  We need to 
					// ContinueWith(..).Wait() in order to successfully wait for
					// the task to complete successfully.
					_taskModel.ContinueWith(a => { }).Wait();
					_cts.Dispose();
				}
				catch (AggregateException ae) { Log.Error($"Message: {ae.Flatten()}", ae); }
				catch (Exception e) { Log.Error($"Message: {e.Message}", e); }
			}

			// Clearing out picture collection, _picCollection, does not clear out the
			// SelectionTracker. _picCollection is responsible for the pictures in the
			// directories provided by the configuration's key="Initial Folders" while
			// SelectionTracker is responsible for trackig pictures that were previously
			// displayed.
			_picCollection.Clear();

			_extions = ConfigValue.Inst.FileExtensionsToConsider();
			_cts = new CancellationTokenSource();
			_taskModel = Task.Run(() => RetrievePictures(), _cts.Token);
		}

		/// <summary>
		/// Retrieve next picture randomly
		/// </summary>
		/// <returns></returns>
		public string GetNextPicture()
		{
			var cnt = _picCollection.Count;
			if (cnt == 0)
			{
				var pic1 = ConfigValue.Inst.FirstPictureToDisplay();
				if (string.IsNullOrWhiteSpace(pic1) || !File.Exists(pic1)) return null;
				return pic1;
			}

			var index = _rand.Next(cnt);
			return _picCollection[index];
		}

		/// <summary>
		/// Retrieve all pictures from all directories
		/// </summary>
		private void RetrievePictures()
		{
			var dirs = ConfigValue.Inst.InitialPictureDirectories();
			foreach (var dir in dirs)
			{
				if (Directory.Exists(dir))
					RetrievePictures(dir);
			}
		}

		/// <summary>
		/// Retrieve pictures in a specific directory
		/// </summary>
		/// <param name="dir"></param>
		private void RetrievePictures(string dir)
		{
			var files = Directory.GetFiles(dir);
			var rightFiles = files.Where(fl => _extions.Any(e => fl.EndsWith(e, StringComparison.CurrentCultureIgnoreCase)));

			_picCollection.AddRange(rightFiles);

			var dirs = Directory.GetDirectories(dir);
			foreach (var d in dirs)
				RetrievePictures(d);
		}
	}
}
