using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Rotate.Pictures.Utility;

namespace Rotate.Pictures.Model
{
	/// <summary>
	/// This class keeps track of pictures in the set of folders given as initial-folders
	/// </summary>
	public class PictureCollection : IList<string>
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// A list of pictures.  Key is the path into the picture.
		/// Using PicPathToIndex allows us to index into the collection by the integer index.
		/// </summary>
		protected ThreadSafeList<string> PicCollection = new();

		/// <summary>
		/// The same collection as PicCollection mapping the path as the key into the picIndex integer.
		/// Reminder: picIndex is the index as in the PicCollection including "holes"  (holes are pictures that are
		/// not to be displayed).
		/// </summary>
		protected ConcurrentDictionary<string, int> PicPathToIndex = new();

		public string this[int index]
		{
			get
			{
				if (index >= PicCollection.Count)
				{
					Log.Error($"Index is out of bounds.  Index requested: {index} is too large.  " +
					          $"PicCollection.Count: {PicCollection.Count}.{Environment.NewLine}" +
							  $"StackTrace:{Environment.NewLine}" +
							  $"{DebugStackTrace.GetStackFrameString()}");
					var inx = index % PicCollection.Count;
					return PicCollection[inx];
				}

				if (index < 0)
				{
					Log.Error($"Index is out of bounds.  Index requested: {index} is negative.{Environment.NewLine}" +
					          $"StackTrace:{Environment.NewLine}" +
					          $"{DebugStackTrace.GetStackFrameString()}");
					for (var inx = (index + PicCollection.Count) % PicCollection.Count; ; inx = (index + PicCollection.Count) % PicCollection.Count)
						if (inx > 0) return PicCollection[inx];
				}

				return PicCollection[index];
			}
			set
			{
				PicCollection[index] = value;
				PicPathToIndex.TryAdd(value, index);
			}
		}

		public int this[string path]
		{
			get
			{
				// It is possible that in the very beginning, requesting a previous picture
				// gets to a point that the previous picture is not part of PicPathToIndex
				// In which case we do nothing.
				if (PicPathToIndex.ContainsKey(path)) return PicPathToIndex[path];

				Log.Warn($"Path of picture: \"{path}\" is not found in picture collection");
				return 0;
			}

			set
			{
				var errMsg = $"Cannot add {value}, using the set property operation, a key, path, mapping to an index";
				Log.Error(errMsg);
				throw new InvalidOperationException(errMsg);
			}
		}

		public int Count => PicCollection.Count;

		public bool IsReadOnly => true;

		public void Add(string item)
		{
			if (string.IsNullOrEmpty(item))
			{
				var errMsg = $"Error: {nameof(item)} cannot be empty";
				Log.Error(errMsg);
				throw new ArgumentException(errMsg, nameof(item));
			}

			PicCollection.Add(item);
			var inx = PicCollection.Count - 1;
			PicPathToIndex.TryAdd(item, inx);
		}

		public void AddRange(IEnumerable<string> items)
		{
			foreach (var item in items)
				Add(item);
		}

		public void Clear()
		{
			PicCollection = new ThreadSafeList<string>();
			PicPathToIndex = new ConcurrentDictionary<string, int>();
		}

		public bool Contains(string item) => PicCollection.Contains(item);

		public void CopyTo(string[] array, int arrayIndex)
		{
			const string errMsg = "CopyTo(string[], int) is not supported in PictureCollection";
			Log.Error(errMsg);
			throw new InvalidOperationException(errMsg);
		}

		public IEnumerator<string> GetEnumerator() => PicCollection.GetEnumerator();

		public int IndexOf(string item)
		{
			if (string.IsNullOrEmpty(item))
			{
				var errMsg = $"Error: {nameof(item)} cannot be empty";
				Log.Error(errMsg);
				throw new ArgumentException(errMsg, nameof(item));
			}

			return PicPathToIndex[item];
		}

		public void Insert(int index, string item)
		{
			const string errMsg = "Insert(int, string) is not supported in PictureCollection";
			Log.Error(errMsg);
			throw new InvalidOperationException(errMsg);
		}

		/// <summary>
		/// In order to support this method we need to make sure that PicPathToIndex
		/// is consistent with PicCollection for all indices from (item's index) on
		/// </summary>
		public bool Remove(string item)
		{
			const string errMsg = "Remove(string) is not supported";
			Log.Error(errMsg);
			throw new InvalidOperationException(errMsg);
		}

		/// <summary>
		/// In order to support this method we need to make sure that PicPathToIndex
		/// is consistent with PicCollection for all indices from (item's index) on
		/// </summary>
		public void RemoveAt(int index)
		{
			const string errMsg = "Remove(int) is not supported";
			Log.Error(errMsg);
			throw new InvalidOperationException(errMsg);
		}

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)PicCollection).GetEnumerator();
	}
}
