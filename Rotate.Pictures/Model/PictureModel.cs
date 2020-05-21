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

		private readonly PicturesToAvoidCollection _avoidCollection = PicturesToAvoidCollection.Default;

		/// <summary>When equals True then all pictures are retrieved and set in _picCollection</summary>
		private volatile int _retrieved;

		/// <summary>Extensions to consider</summary>
		private List<string> _extensionList;

		private readonly Random _rand = new Random();
		private Task _taskModel;
		private CancellationTokenSource _cts;

		public event EventHandler<PictureRetrievingEventArgs> PictureRetrievingHandler = delegate {};
		public string RetrievingNow;

		/// <summary>The current Picture Index/// </summary>
		public int CurrentPicIndex { get; set; }

		/// <summary>
		/// .ctor
		/// Retrieve pictures asynchronously
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

			// Clearing out picture collection, _picCollection, does not clear out the SelectionTracker. 
			// _picCollection is responsible for the pictures in the directories provided by the 
			// configuration's key="Initial Folders" while SelectionTracker is responsible for tracking
			// pictures that were previously displayed.
			_picCollection.Clear();

			_extensionList = ConfigValue.Inst.FileExtensionsToConsider();
			_cts = new CancellationTokenSource();
			_taskModel = Task.Run(RetrievePictures, _cts.Token);
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

			var flatIndex = _rand.Next(cnt);
			CurrentPicIndex = _avoidCollection.GetPictureIndexFromFlatIndex(flatIndex);
			var pic = _picCollection[CurrentPicIndex];
			return pic;
		}

		public void AddPictureToAvoid(int picToAvoid) => _avoidCollection.AddPictureToAvoid(picToAvoid);

		/// <summary>
		/// Retrieve all pictures from all directories
		/// </summary>
		private void RetrievePictures()
		{
			// Using Interlocked.Exchange is an overkill since _retrieved is an int, for which
			// assignment is an atomic operation.
			Interlocked.Exchange(ref _retrieved, 0);

			var dirs = ConfigValue.Inst.InitialPictureDirectories();
			foreach (var dir in dirs)
			{
				if (Directory.Exists(dir))
					RetrievePictures(dir);
			}

			Interlocked.Exchange(ref _retrieved, 1);
		}

		/// <summary>
		/// Retrieve pictures in a specific directory
		/// </summary>
		/// <param name="dir"></param>
		private void RetrievePictures(string dir)
		{
			RetrievingNow = dir;
			OnPictureRetrieving(dir);
			var files = Directory.GetFiles(dir);

			var rightFiles = files.Where(fl => _extensionList.Any(e => fl.EndsWith(e, StringComparison.CurrentCultureIgnoreCase))).ToList();
			_picCollection.AddRange(rightFiles);

			var dirs = Directory.GetDirectories(dir);
			foreach (var d in dirs)
				RetrievePictures(d);
		}

		public bool IsPicturesRetrieving => _retrieved == 0;

		void OnPictureRetrieving(string picDirectory) => PictureRetrievingHandler(this, new PictureRetrievingEventArgs(picDirectory));
	}
}
