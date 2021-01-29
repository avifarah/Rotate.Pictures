using System;
using System.Collections;
using System.Collections.Generic;

namespace Rotate.Pictures.Model
{
	public class ThreadSafeList<T> : IList<T>
	{
		private readonly List<T> _list = new();

		private readonly object _sync = new();

		public T this[int index]
		{
			get { lock (_sync) return _list[index]; }
			set { lock (_sync) _list[index] = value; }
		}

		public int Count
		{
			get
			{
				lock (_sync) return _list.Count;
			}
		}

		public bool IsReadOnly => true;

		public void Add(T item)
		{
			lock (_sync) _list.Add(item);
		}

		public void Clear()
		{
			lock (_sync) _list.Clear();
		}

		public bool Contains(T item)
		{
			lock (_sync) return _list.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex) => throw new NotSupportedException("ThreadSafeList does not support CopyTo(..)");

		public IEnumerator<T> GetEnumerator()
		{
			lock (_sync) return _list.GetEnumerator();
		}

		public int IndexOf(T item)
		{
			lock (_sync) return _list.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			lock (_sync) _list.Insert(index, item);
		}

		public bool Remove(T item) => throw new NotSupportedException("ThreadSafeList does not support Remove(..)");

		public void RemoveAt(int index) => throw new NotSupportedException("ThreadSafeList does not support RemoveAt(..)");

		IEnumerator IEnumerable.GetEnumerator()
		{
			lock (_sync) return _list.GetEnumerator();
		}
	}
}
