using System;
using System.Collections.Generic;


namespace Rotate.Pictures.Utility
{
	/// <summary>
	/// SelectionTracker tracks the selection of pictures through time up to _maxTrackerDepth
	/// </summary>
	public class SelectionTracker
	{
		private static readonly Lazy<SelectionTracker> _inst = new Lazy<SelectionTracker>(() => new SelectionTracker());
		public static readonly SelectionTracker Inst = _inst.Value;

		private readonly int _maxTrackerDepth;
		private readonly List<string> _tracker = new List<string>();
		private int _picturePointer;

		private SelectionTracker()
		{
			_maxTrackerDepth = ConfigValue.Inst.MaxPictureTrackerDepth();
			_picturePointer = 0;
		}

		public void Append(string pic)
		{
			if (_tracker.Count == _maxTrackerDepth) _tracker.RemoveAt(0);
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
			if (_picturePointer < _tracker.Count - 1) ++_picturePointer;
			return _tracker[_picturePointer];
		}

		public string Prev()
		{
			if (_picturePointer > 0) --_picturePointer;
			return _tracker[_picturePointer];
		}

		public void ClearTracker() => _tracker.Clear();

		public int Count => _tracker.Count;

		public bool AtTail => _picturePointer == _tracker.Count - 1;

		public bool AtHead => _picturePointer == 0;
	}
}
