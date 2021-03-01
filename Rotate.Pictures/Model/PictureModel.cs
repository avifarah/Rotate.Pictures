using System;
using System.Collections.Generic;
//using System.Diagnostics;
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
	/// This interface will make it easier to test.
	/// The difference between the interface and the base abstract class is the
	///			ManualResetEventSlim RetrievedEvent
	/// which means that for the code we will need to either use the base abstract class
	/// or cast it to the base abstract class in order to use the RetrievedEvent.
	///
	/// The reason we need
	///			ManualResetEventSlim RetrievedEvent
	/// is to be able to signal the end of reading all pictures.  The reason we need it as
	/// a variable as opposed to a property is that a property will not work.
	/// </summary>
	public interface IPictureModel
	{
		int PicPathToIndex(string path);

		string PicIndexToPath(int picIndex);

		bool IsPicturesRetrieving { get; }
	}

	public abstract class PictureModelBase : IPictureModel
	{
        public ManualResetEventSlim RetrievedEvent;

		public abstract int PicPathToIndex(string path);

		public abstract string PicIndexToPath(int picIndex);

		public abstract bool IsPicturesRetrieving { get; }
	}

	/// <summary>
	/// Repository for picture collection
	/// </summary>
	public class PictureModel : PictureModelBase
	{
		protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Collection of all pictures in all directories supplied by the user in the configuration file
		/// </summary>
		protected readonly PictureCollection PicCollection = new();

		/// <summary>
		/// Collection of pictures to avoid
		/// </summary>
		private readonly PicturesToAvoidCollection _avoidCollection;

		private readonly SelectionTracker _selectionTracker;

		/// <summary>
		/// When equals True then all pictures are retrieved and set in _picCollection
		/// </summary>
		private volatile int _retrieved;

		/// <summary>
		/// File extensions to consider
		/// </summary>
		private List<string> _extensionList;

		private readonly Random _rand = new();
		private readonly Task _taskModel;
		private readonly CancellationTokenSource _cts;

		private readonly IConfigValue _configValue;

		public event EventHandler<PictureRetrievingEventArgs> OnPictureRetrieving = delegate { };

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

		/// <summary>
		/// Mark as virtual in order to be able to unit test
		/// </summary>
		public override string PicIndexToPath(int picIndex) => PicCollection[picIndex];

		/// <summary>
		/// Mark as virtual in order to be able to unit test
		/// </summary>
		public override int PicPathToIndex(string path) => PicCollection[path];

		public bool IsPictureToAvoid(int index) => _avoidCollection.IsPictureToAvoid(index);

		public bool IsPictureToAvoid(string path) => _avoidCollection.IsPictureToAvoid(PicPathToIndex(path));

		public bool IsCollectionContains(string path) => PicCollection.Contains(path);

		/// <summary>
		/// .ctor
		/// Retrieve pictures asynchronously
		/// </summary>
		public PictureModel(IConfigValue configValue)
		{
            //Debug.WriteLine($"{MethodBase.GetCurrentMethod().DeclaringType}.{MethodBase.GetCurrentMethod().Name}(..)");
			_configValue = configValue;

            RetrievedEvent = new(false);

            _selectionTracker = new SelectionTracker(this, configValue.MaxPictureTrackerDepth());
			_avoidCollection = new PicturesToAvoidCollection(this, configValue);
            _cts = new CancellationTokenSource();
			_taskModel = Restart(_cts);
		}

		public Task Restart(CancellationTokenSource cts = null)
        {
            cts ??= _cts;

			if (_taskModel != null)
			{
				cts.Cancel();
				try
				{
					// Note that _taskModel.Wait() will not work.  We need to 
					// ContinueWith(..).Wait() in order to successfully wait for
					// the task to complete successfully.
					_taskModel.ContinueWith(a => { }).Wait();
					cts.Dispose();
				}
				catch (AggregateException ae) { Log.Error($"Message: {ae.Flatten()}", ae); }
				catch (Exception e) { Log.Error($"Message: {e.Message}", e); }
			}

			// Clearing out picture collection, _picCollection, does not clear out the SelectionTracker. 
			// _picCollection is responsible for the pictures in the directories provided by the 
			// configuration's key="Initial Folders" while SelectionTracker is responsible for tracking
			// pictures that were previously displayed.
			PicCollection.Clear();
			_selectionTracker.ClearTracker();
			ClearDoNotDisplayCollection();

			_extensionList = _configValue.FileExtensionsToConsider();
			return Task.Factory.StartNew(t => RetrievePictures((CancellationToken)(t ?? cts.Token)),
                        cts.Token, cts.Token, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
		}

		public void ClearDoNotDisplayCollection() => _avoidCollection.ClearPicsToAvoid();

		/// <summary>
		/// Retrieve next picture randomly
		/// </summary>
		/// <returns></returns>
		public string GetNextPicture()
		{
			var cnt = PicCollection.Count;
			if (cnt == 0)
			{
				var pic1 = _configValue.FirstPictureToDisplay();
				if (string.IsNullOrWhiteSpace(pic1) || !File.Exists(pic1)) return null;
				return pic1;
			}

			var flatIndex = _rand.Next(cnt);
			CurrentPicIndex = _avoidCollection.GetPictureIndexFromFlatIndex(flatIndex);
			var pic = PicCollection[CurrentPicIndex];
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

		public bool AddPictureToAvoid(int picToAvoid) => _avoidCollection.AddPictureToAvoid(picToAvoid);

		public void RemovePictureToAvoid(int picToAvoid) => _avoidCollection.RemovePictureToAvoid(picToAvoid);

		public override bool IsPicturesRetrieving
		{
			get
			{
				var retrieved = Interlocked.CompareExchange(ref _retrieved, 1, 1);
				return retrieved == 0;
			}
		}

		public int Count => PicCollection.Count;

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
		protected virtual void RetrievePictures(CancellationToken ct)
		{
			RetrievedEvent.Reset();
			// Using Interlocked.Exchange is an overkill since _retrieved is an int, for which
			// assignment is an atomic operation.
			Interlocked.Exchange(ref _retrieved, 0);
			CustomEventAggregator.Inst.Publish(new PictureLoadingDoneEventArgs(false));

			var dirs = _configValue.InitialPictureDirectories();
			foreach (var dir in dirs)
			{
				if (Directory.Exists(dir))
					RetrievePictures(dir, ct);
			}

			Interlocked.Exchange(ref _retrieved, 1);
			RetrievedEvent.Set();
			CustomEventAggregator.Inst.Publish(new PictureLoadingDoneEventArgs(true));

			if (PicCollection.Count == 0)
			{
				const string errMsg = "No picture could be retrieved.  Please check configuration file for error";
				Log.Error(errMsg);
				MessageBox.Show(errMsg, "Rotating.Pictures Model");
			}
		}

		/// <summary>
		/// Retrieve pictures in a specific directory
		/// </summary>
		private bool RetrievePictures(string dir, CancellationToken ct)
		{
			// Start by checking cancellation token
			if (ct.IsCancellationRequested) return false;

			// Publish an event to announce the "Retrieving Now" directory
			PictureRetrieving(dir);

			var files = Directory.GetFiles(dir);
			var rightFiles = files.Where(fl => _extensionList.Any(e => fl.EndsWith(e, StringComparison.CurrentCultureIgnoreCase)));
			var rightFormatted = rightFiles.Where(f => f.IsPictureValidFormat()).ToList();
			//Log.Debug(string.Join(Environment.NewLine, rightFormatted.Select((p, i) => $"({i,4}, \"{p}\")")));
			PicCollection.AddRange(rightFormatted);

			// Done processing local files, check for cancellation token
			if (ct.IsCancellationRequested) return false;

			var dirs = Directory.GetDirectories(dir);
			foreach (var d in dirs)
			{
				var rc = RetrievePictures(d, ct);
				if (!rc) return false;
			}

			return true;
		}

		private void PictureRetrieving(string picDirectory) => OnPictureRetrieving(this, new PictureRetrievingEventArgs(picDirectory));
	}
}
