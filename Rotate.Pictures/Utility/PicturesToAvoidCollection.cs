using System;
using System.Collections.Generic;
using System.Linq;

namespace Rotate.Pictures.Utility
{
	public class PicturesToAvoidCollection
	{
		/// <summary>
		/// key (flatIndex)		:	int, lowerRangeLimit flat index (where the "holes", 
		///							the missing pictures, are compressed.  The index of 
		///							the pictures out of the random number generator
		///							[0 .. n - avoided-count).
		/// value (picIndex)	:	int, index of picture collection: [0 .. n).  This 
		///							index includes the pictures that are not to be shown.
		/// <remarks>
		/// I contemplated between the name "flatIndex" and "runningIndex" and decided
		/// to use name "flatIndex" as it indicates that the index has no "holes",
		/// more reminiscent of the index out of the random number generator.  While the
		/// name "runningIndex" could also be reminiscent of the second index which is
		/// the picture index.
		///
		/// I decided to call the second index the picIndex (Picture Index) which does
		/// have "holes" (pictures not to be used), as per user request.
		/// </remarks>
		/// </summary>
		private readonly Dictionary<int, int> _avoidPics = new Dictionary<int, int>();

		/// <summary>
		/// The algorithm for avoided pictures relies on the fact that _orderedKeys
		/// is in an ascending order
		/// </summary>
		private List<int> _orderedKeys = new List<int>();

		/// <summary>
		/// Note calling Default will generate a new empty pictures-To-Avoid list.
		/// Obviously, the list will be empty.  At which point flatIndex equals
		/// picIndex
		/// </summary>
		public static readonly PicturesToAvoidCollection Default = new PicturesToAvoidCollection(ConfigValue.Inst.PicturesToAvoidSorted().ToList());

		/// <summary>
		/// The index of this list is the picIndex as opposed to flatIndex
		/// </summary>
		private readonly List<int> _orderedPicturesToAvoid = new List<int>();

		public int Count => _orderedKeys.Count;

		public PicturesToAvoidCollection(List<int> picsToAvoid)
		{
			if (picsToAvoid == null) throw new ArgumentException($"{nameof(picsToAvoid)} may not be null", nameof(picsToAvoid));
			picsToAvoid.Sort();
			_orderedPicturesToAvoid.AddRange(picsToAvoid);
			PopulatePicsToAvoid();
		}

		/// <summary>
		/// Name:	PopulatePicsToAvoid
		/// Purpose:
		///		Populates the _avoidPics dictionary
		///		Thereafter, in order to translate the index from a flatIndex to the picIndex
		///		we need add the appropriate _avoidPics entry, see: 
		///		<see cref="Rotate.Pictures.Utility.PicturesToAvoidCollection.GetPictureIndexFromFlatIndex"/>
		/// 
		///	Nomenclature:
		///		flatIndex:	The one obtained from the random number generator
		///					(Range: [0 .. n - avoided-count)).
		///		picIndex:	The index into the pictures (Range: [0 .. n)).
		/// 
		/// Explanation of algorithm:
		///		Say that total pictures count = 30
		///			pictures to avoid: 3, 10, 11, 12, 13, 14, 25 (this is: 3, 10-14, 25)
		///
		///		One possible algorithm:
		///		<code>
		///			// Input: freeIndex
		///			// Output: picIndex
		///			var picIndex = 0;
		///			for (var i = 0; i &lt; flatIndex; ++i)
		///			{
		///				while (_avoidPics.ContainsKey(picIndex)) ++picIndex;
		///				++picIndex;
		///			}
		///			while (_avoidPics.ContainsKey(picIndex)) ++picIndex;
		///			return picIndex;
		///		</code>
		///
		///		We can check if an index is in pictures-to-avoid set by setting a dictionary
		///		as follows:
		///			_avoidPics[ 3] = 1			// We cannot have picture-index = 3, therefore 
		///										// .. skip 3 by adding 1 to flatIndex.
		///			_avoidPics[10] = 5			// If we encounter picture-index = 10 then skip 
		///										// .. all range, picture-index = flatIndex + 5 
		///			_avoidPics[25] = 1
		///		Then check against _avoidPics.Contains(index).  For this data structure to work
		///		we need to introduce a new algorithm.
		/// 
		///		This algorithm will need to first find the greatest-small-index
		///			Explanation of the greatest-small-index:
		///				looking for flat-index 13, the dictionary contains smaller indices: 3
		///				and 10, but we are looking for the greatest of the 2.  Then we need to 
		///				sum _avoidPics[3] and _avoidPics[10] to the flatIndex in order to come
		///				up with the correct picIndex.
		/// 
		///		This is getting complicated.  We can improve upon this algorithm as follows:
		///										// If flatIndex &lt; 3 then picIndex = flatIndex
		///										// Thereafter we need to add _avoidPics[3] to the
		///										// next picIndex range.
		///			_avoidPics[     3] = 1		// if flatIndex >= 3 and &lt; 9 (10 - 1) 
		///										// .. (where 10 is the original flatIndex and 1
		///										// .. is the _avoidPics[3]) then
		///										// .. picIndex = flatIndex + _avoidPics[3]
		///			_avoidPics[10 - 1] = 6		// If flatIndex >= 9 (10 - 1) and &lt; 19
		///										// .. (19 = 25 next index in _avoidPics -
		///										// .. new _avoidPics[10]: 25 - 6)
		///										// .. then picIndex = flatIndex + _avoidPics[9]
		///			_avoidPics[25 - 6] = 7		// If flatIndex >= 19 (25 - 6) then
		///										// .. picIndex = flatIndex + _avoidPics[19]
		///
		///		Now the code to convert flatIndex to picIndex is as follows:
		///		<code>
		///			upperSmallestIndex = FindGreatestSmallerIndex(flatIndex);
		///			if flatIndex is &lt; _avoidPics.SmallestIndex
		///				picIndex = flatIndex;
		///			else
		///				picIndex = flatIndex + _avoidPics[upperSmallestIndex];
		///		</code>
		///
		///		Now the FindGreatestSmallerIndex(..) can be a binary search.
		///
		///		Conclusion: This algorithm avoids the running through all the values 0 .. flatIndex
		///		in order to find out the value of the picIndex.
		///
		///		Discussion: This is nice, though in real life, it is of questionable consequences
		///		since it is not a real-time application where the cycling through a few 1,000s of
		///		pictures or even 10s of 1,000s of pictures, is not a big deal for each time
		///		interval between pictures.  Nevertheless, it is simple enough to implement and I
		///		felt that it was worth the extra effort.
		/// </summary>
		private void PopulatePicsToAvoid()
		{
			var inx = 0;
			foreach (var x in _orderedPicturesToAvoid)
			{
				var flatIndex = x - inx;
				_avoidPics[flatIndex] = ++inx;
			}

			_orderedKeys = _avoidPics.Keys.ToList();
		}

		public bool AddPictureToAvoid(int picIndex)
		{
			if (_orderedPicturesToAvoid.Contains(picIndex)) return false;

			_orderedPicturesToAvoid.Add(picIndex);
			_orderedPicturesToAvoid.Sort();
			_avoidPics.Clear();
			PopulatePicsToAvoid();
			ConfigValue.Inst.UpdatePicturesToAvoid(_orderedPicturesToAvoid);

			return true;
		}

		public int GetPictureIndexFromFlatIndex(int flatIndex)
		{
			if (_avoidPics.Count == 0) return flatIndex;

			var upperSmallestIndex = FindGreatestSmallerIndex(flatIndex);
			return upperSmallestIndex < 0 ? flatIndex : flatIndex + _avoidPics[upperSmallestIndex];
		}

		public int FindGreatestSmallerIndex(int flatIndex)
		{
			var n = _orderedKeys.Count;
			var lowerRangeLimit = 0;
			var upperRangeLimit = n;
			var midRangePoint = n / 2;
			var index = FindUsi(flatIndex, lowerRangeLimit, upperRangeLimit, midRangePoint);
			return index;
		}

		/// <summary>
		/// FindUsi : Find Upper Smaller Index
		/// </summary>
		/// <param name="flatIndex"></param>
		/// <param name="lowerRangeLimit">Lower limit inclusive</param>
		/// <param name="upperRangeLimit">Upper limit exclusive</param>
		/// <param name="midRangePoint"></param>
		private int FindUsi(int flatIndex, int lowerRangeLimit, int upperRangeLimit, int midRangePoint)
		{
			// If _orderedKeys[midRangePoint] > flatIndex
			// then the result is in the range of [lowerRangeLimit .. midRangePoint)
			if (_orderedKeys[midRangePoint] > flatIndex)
			{
				// This condition will happen when the first picture to skip is not the first
				// picture, say the nth picture, and the random number generator picked lowerRangeLimit number,
				// r, that is less than n.  In which case r < _orderedKeys[0] but there is no
				// range to look through.  This not an issue for any other range's index.
				if (midRangePoint == 0) return -1;

				return FindUsi(flatIndex, lowerRangeLimit, midRangePoint, (lowerRangeLimit + midRangePoint) / 2);
			}

			if (flatIndex == _orderedKeys[midRangePoint]) return _orderedKeys[midRangePoint];

			// Did we reach the end of recursion?
			// Technically we can check for midRangePoint + 1 == upperRangeLimit but there is no harm in the >= check.
			// upperRangeLimit is an excluded upper limit therefore the comparison of midRangePoint+1 to upperRangeLimit.
			if (midRangePoint + 1 >= upperRangeLimit) return _orderedKeys[midRangePoint];

			// if flatIndex < _orderedKeys[midRangePoint] it does not mean that we eliminated the
			// _orderedKeys[midRangePoint] as lowerRangeLimit candidate.  Therefore, we use midRangePoint for the lower-bound
			// (as opposed to setting the next lower-bound to midRangePoint+1)
			return FindUsi(flatIndex, midRangePoint, upperRangeLimit, (midRangePoint + 1 + upperRangeLimit) / 2);
		}
	}
}
