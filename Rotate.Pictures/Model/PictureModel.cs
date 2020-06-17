using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Rotate.Pictures.EventAggregator;
using Rotate.Pictures.Utility;


namespace Rotate.Pictures.Model
{
	/// <summary>
	/// Repository for picture collection
	/// </summary>
	public class PictureModel
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Collection of all pictures in all directories supplied by the user in the configuration file
		/// </summary>
		private readonly PictureCollection _picCollection = new PictureCollection();

		/// <summary>
		/// Collection of pictures to avoid
		/// </summary>
		private readonly PicturesToAvoidCollection _avoidCollection = PicturesToAvoidCollection.Default;

		private readonly SelectionTracker _selectionTracker;

		/// <summary>
		/// When equals True then all pictures are retrieved and set in _picCollection
		/// </summary>
		private volatile int _retrieved;

		/// <summary>
		/// File extensions to consider
		/// </summary>
		private List<string> _extensionList;

		private readonly Random _rand = new Random();
		private Task _taskModel;
		private CancellationTokenSource _cts;

		public event EventHandler<PictureRetrievingEventArgs> PictureRetrievingHandler = delegate { };
		public string RetrievingNow;

		/// <summary>
		/// The current Picture Index
		/// 
		/// Unlike the MainWindowViewModel.CurrentPicture this CurrentPicIndex represents the model's
		/// state of the current picture.
		///
		/// The ViewModel (MainWindowViewModel), may go back and forth in the picture historic stack.  So
		/// when the ViewModel traverses back and forth through the historic stack (forward before it reaches
		/// the model.CurrentPicIndex) then the model.CurrentPicIndex is not pointing to the same picture
		/// as the ViewModel.CurrentPicture.
		/// </summary>
		public int CurrentPicIndex { get; set; }

		public IReadOnlyList<int> PicturesToAvoid => _avoidCollection.PicturesToAvoid;

		public string PicIndexToPath(int picIndex) => _picCollection[picIndex];

		public int PicPathToIndex(string path) => _picCollection[path];

		public bool IsPictureToAvoid(int index) => _avoidCollection.IsPictureToAvoid(index);

		/// <summary>
		/// .ctor
		/// Retrieve pictures asynchronously
		/// </summary>
		public PictureModel()
		{
			_selectionTracker = new SelectionTracker(this);
			Restart();
		}

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
			_selectionTracker.ClearTracker();
			ClearDoNotDisplayCollection();

			_extensionList = ConfigValue.Inst.FileExtensionsToConsider();
			_cts = new CancellationTokenSource();
			_taskModel = Task.Run(RetrievePictures, _cts.Token);
		}

		public void ClearDoNotDisplayCollection() => _avoidCollection.ClearPicsToAvoid();

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
			try
			{
				if (string.IsNullOrWhiteSpace(pic) || !File.Exists(pic))
				{
					Log.Error($"Picture {pic} does not represent an existing file.");
					return null;
				}
				return pic;
			}
			catch (Exception e)
			{
				Log.Error($"Picture {pic} does not represent an existing file.", e);
				return null;
			}
		}

		public void AddPictureToAvoid(int picToAvoid) => _avoidCollection.AddPictureToAvoid(picToAvoid);

		public void RemovePictureToAvoid(int picToAvoid) => _avoidCollection.RemovePictureToAvoid(picToAvoid);

		public bool IsPicturesRetrieving => _retrieved == 0;

		public int Count => _picCollection.Count;

		#region SelectionTracker

		public void SelectionTrackerAppend(string pic) => _selectionTracker.Append(pic);

		public void SelectionTrackerSetMaxPictureDepth(int depth) => _selectionTracker.SetMaxPictureDepth(depth);

		public bool SelectionTrackerAtHead => _selectionTracker.AtHead;

		public bool SelectionTrackerAtTail => _selectionTracker.AtTail;

		public string SelectionTrackerPrev() => _selectionTracker.Prev();

		public string SelectionTrackerNext() => _selectionTracker.Next();

		public int SelectionTrackerCount => _selectionTracker.Count;

		#endregion

		/// <summary>
		/// Retrieve all pictures from all directories
		/// </summary>
		private void RetrievePictures()
		{
			// Using Interlocked.Exchange is an overkill since _retrieved is an int, for which
			// assignment is an atomic operation.
			Interlocked.Exchange(ref _retrieved, 0);
			CustomEventAggregator.Inst.Publish(new PictureLoadingDoneEventArgs(false));

			var dirs = ConfigValue.Inst.InitialPictureDirectories();
			foreach (var dir in dirs)
			{
				if (Directory.Exists(dir))
					RetrievePictures(dir);
			}

			Interlocked.Exchange(ref _retrieved, 1);
			CustomEventAggregator.Inst.Publish(new PictureLoadingDoneEventArgs(true));

			if (_picCollection.Count == 0)
			{
				const string errMsg = "No picture could be retrieved.  Please check configuration file for error";
				Log.Error(errMsg);
				MessageBox.Show(errMsg, "Rotating.Pictures Model");
			}
		}

		/// <summary>
		/// Retrieve pictures in a specific directory
		/// </summary>
		private void RetrievePictures(string dir)
		{
			RetrievingNow = dir;
			OnPictureRetrieving(dir);
			var files = Directory.GetFiles(dir);

			var rightFiles = files.Where(fl => _extensionList.Any(e => fl.EndsWith(e, StringComparison.CurrentCultureIgnoreCase)));
			var rightFormatted = rightFiles.Where(f => f.IsPictureValidFormat()).ToList();
			_picCollection.AddRange(rightFormatted);

			var dirs = Directory.GetDirectories(dir);
			foreach (var d in dirs)
				RetrievePictures(d);
		}

		private void OnPictureRetrieving(string picDirectory) => PictureRetrievingHandler(this, new PictureRetrievingEventArgs(picDirectory));
	}
}
