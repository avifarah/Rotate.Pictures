using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Rotate.Pictures.Utility;

namespace Rotate.Pictures.Model
{
	/// <summary>
	/// SelectionTracker tracks the selection of pictures through time up to _maxTrackerDepth
	///
	/// SelectionTracker allows control go back and forth through the history of selection
	/// </summary>
	public class SelectionTracker
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private int _maxTrackerDepth;
		private readonly List<string> _tracker = new List<string>();
		private int _picturePointer;
		private readonly PictureModel _parentPicModel;

		public SelectionTracker(PictureModel parent, int maxTrackerDepth)
		{
			_maxTrackerDepth = maxTrackerDepth;
			_picturePointer = 0;
			_parentPicModel = parent;
		}

		public void Append(string pic)
		{
			while (_tracker.Count >= _maxTrackerDepth)
				_tracker.RemoveAt(0);

			_tracker.Add(pic);
			_picturePointer = _tracker.Count - 1;
		}

		/// <summary>
		/// Remove values in range _picturePointer + 1 .. _tracker.Count and add the new picture into _tracker[++_picturePointer]
		/// </summary>
		/// <param name="pic"></param>
		public void Add(string pic)
		{
			_tracker.RemoveRange(_picturePointer + 1, _tracker.Count - _picturePointer - 1);
			Append(pic);
		}

		public string Next()
		{
			var count = _tracker.Count;
			for (var i = 0; _picturePointer < _tracker.Count - 1; ++i)
			{
				_picturePointer = (_picturePointer + 1) % count;
				var path = _tracker[_picturePointer];
				if (!File.Exists(path))
				{
					Log.Error($"File: {path} cannot be found.  File may have been deleted after the program started");
					continue;
				}

				var inx = _parentPicModel.PicPathToIndex(path);
				if (!_parentPicModel.IsPictureToAvoid(inx)) break;
				if (i == count)
				{
					Log.Error($"Cannot move next");
					return null;
				}
			}
			return _tracker[_picturePointer];
		}

		/// <summary>
		/// Purpose:
		///		Retrieve previous picture
		/// <remarks>
		///		In order to prevent the possibility of an infinite loop we limit the
		///		loop to _tracker.Count.
		///
		///		First picture may not be part of the _parentPicModel collection
		/// </remarks>
		/// </summary>
		/// <returns></returns>
		public string Prev()
		{
			var count = _tracker.Count;
			for (var i = 0; _picturePointer > 0; ++i)
			{
				_picturePointer = (_picturePointer - 1 + count) % count;
				var path = _tracker[_picturePointer];
				if (!File.Exists(path))
				{
					Log.Error($"File: {path} cannot be found.  File may have been deleted after the program started");
					continue;
				}

				var inx = _parentPicModel.PicPathToIndex(path);
				if (!_parentPicModel.IsPictureToAvoid(inx)) break;
				if (i == count)
				{
					Log.Error($"Cannot move previous");
					return null;
				}
			}
			return _tracker[_picturePointer];
		}

		public void ClearTracker() => _tracker.Clear();

		public int Count => _tracker.Count;

		public bool AtTail => _picturePointer == _tracker.Count - 1;

		public bool AtHead => _picturePointer == 0;

		public void SetMaxPictureDepth(int picDepth)
		{
			_maxTrackerDepth = picDepth;

			while (_tracker.Count >= _maxTrackerDepth)
				_tracker.RemoveAt(0);
		}
	}
}
