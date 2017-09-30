using System.Collections;
using System.Collections.Generic;


namespace Rotate.Pictures.Utility
{
	/// <summary>
	/// This class keeps track of pictures in the set of folders given as initial-folders
	/// </summary>
	public class PictureCollection : IList<string>
	{
		private SynchronizedCollection<string> _picCollection = new SynchronizedCollection<string>();

		public string this[int index]
		{
			get => _picCollection[index];
			set => _picCollection[index] = value;
		}

		public int Count => _picCollection.Count;

		public bool IsReadOnly => true;

		public void Add(string item) => _picCollection.Add(item);

		public void AddRange(IEnumerable<string> items)
		{
			foreach (var item in items)
				Add(item);
		}

		public void Clear() => _picCollection = new SynchronizedCollection<string>();

		public bool Contains(string item) => _picCollection.Contains(item);

		public void CopyTo(string[] array, int arrayIndex) => _picCollection.CopyTo(array, arrayIndex);

		public IEnumerator<string> GetEnumerator() => _picCollection.GetEnumerator();

		public int IndexOf(string item) => _picCollection.IndexOf(item);

		public void Insert(int index, string item) => _picCollection.Insert(index, item);

		public bool Remove(string item) => _picCollection.Remove(item);

		public void RemoveAt(int index) => _picCollection.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_picCollection).GetEnumerator();
	}
}
