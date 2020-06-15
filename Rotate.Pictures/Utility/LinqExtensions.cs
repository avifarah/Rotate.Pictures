using System;
using System.Collections.Generic;
using System.Linq;


namespace Rotate.Pictures.Utility
{
	/// <summary>
	/// Allows Linq methods Except, Intersect and Union specify a comparer method externally and optionally provide HashCode externally.
	/// </summary>
	public static class LinqExtensions
	{
		public static IEnumerable<T> Except<T>(this IEnumerable<T> a, IEnumerable<T> b, Func<T, T, bool> cmpr, Func<T, int> hashCode = null)
			=> a.Except(b, new LinqComparer<T>(cmpr, hashCode));

		public static IEnumerable<T> Intersect<T>(this IEnumerable<T> a, IEnumerable<T> b, Func<T, T, bool> cmpr, Func<T, int> hashCode = null)
			=> a.Intersect(b, new LinqComparer<T>(cmpr, hashCode));

		public static IEnumerable<T> Union<T>(this IEnumerable<T> a, IEnumerable<T> b, Func<T, T, bool> cmpr, Func<T, int> hashCode = null)
			=> a.Union(b, new LinqComparer<T>(cmpr, hashCode));
	}
}
