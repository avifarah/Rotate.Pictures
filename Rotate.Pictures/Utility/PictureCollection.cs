using System.Collections;
using System.Collections.Generic;


namespace Rotate.Pictures.Utility
{
	/// <summary>
	/// This class keeps track of pictures in the set of folders given as initial-folders
	/// </summary>
	public class PictureCollection : IList<string>
	{
		protected SynchronizedCollection<string> PicCollection = new SynchronizedCollection<string>();

		//public PictureCollection() { }

		protected PicturesToAvoidCollection AvoidCollection = PicturesToAvoidCollection.Default; 

		public string this[int index]
		{
			get => PicCollection[index];
			set => PicCollection[index] = value;
		}

		public int Count => PicCollection.Count;

		public bool IsReadOnly => true;

		public void Add(string item) => PicCollection.Add(item);

		public void AddRange(IEnumerable<string> items)
		{
			foreach (var item in items)
				Add(item);
		}

		public void Clear() => PicCollection = new SynchronizedCollection<string>();

		public bool Contains(string item) => PicCollection.Contains(item);

		public void CopyTo(string[] array, int arrayIndex) => PicCollection.CopyTo(array, arrayIndex);

		public IEnumerator<string> GetEnumerator() => PicCollection.GetEnumerator();

		public int IndexOf(string item) => PicCollection.IndexOf(item);

		public void Insert(int index, string item) => PicCollection.Insert(index, item);

		public bool Remove(string item) => PicCollection.Remove(item);

		public void RemoveAt(int index) => PicCollection.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)PicCollection).GetEnumerator();
	}
}
