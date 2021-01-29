using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Rotate.Pictures.EventAggregator;
using Rotate.Pictures.Utility;

namespace Rotate.Pictures.Model
{
	public class PicturesToAvoidCollection : ISubscriber<PictureLoadingDoneEventArgs>
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
		protected readonly Dictionary<int, int> FlatToPicIndexMapping = new();

		/// <summary>
		/// Avoid paths collection
		/// </summary>
		private ThreadSafeList<string> _avoidPicPaths = new();

		/// <summary>
		/// The algorithm for avoided pictures relies on the fact that _orderedKeys, flatIndex list,
		/// is in an ascending order.
		/// </summary>
		private List<int> _orderedKeys = new();

		/// <summary>PicIndex</summary>
		private readonly List<int> _orderedPicturesToAvoid = new();

		//private readonly object _populateSync = new();

		private readonly IPictureModel _parent;

		private readonly IConfigValue _configValue;

		/// <summary>Is done loading paths flag</summary>
		private int _isDoneLoading;

		private const int WaitForPicturesToLoad = 100;		// milliseconds

		/// <summary>If a picture to avoid is added before the list of pictures is loaded then the picture to avoid did not take place</summary>
		private bool _populatePicIndexMappingAndKeysDone = true;

		/// <summary>
		/// Similar to the flag but allows for a wait
		/// </summary>
		public ManualResetEvent IsDoneLoadingEvent = new(false);

		public int Count => _orderedKeys.Count;

		public IReadOnlyList<int> PicturesToAvoid => _orderedPicturesToAvoid;

		public PicturesToAvoidCollection(IPictureModel parent, IConfigValue configValue)
		{
			_parent = parent;
			_configValue = configValue;
			//Debug.WriteLine($"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}().  _orderedPicturesToAvoid: ({string.Join("; ", _orderedPicturesToAvoid)}){Environment.NewLine}{DebugStackTrace.GetStackFrameString()}");
			Task.Run(Initialize);
		}

		/// <summary>
		/// We need to populate the following structures:
		/// 	_avoidPicPaths:				A Collection of pic paths (initially read from a flat file)
		///		_orderedPicturesToAvoid		Collection{int}, pic indices
		///		FlatToPicIndexMapping		Dictionary{int, int}, flat to pic index mapping
		///		_orderedKeys:				A list of flat indices
		/// </summary>
		private void Initialize()
		{
			// _avoidPicPaths from a file
			var picIndices = _configValue.PicturesToAvoidPaths().Where(p => !_avoidPicPaths.Contains(p));
			foreach (var path in picIndices) _avoidPicPaths.Add(path);

			// Wait for all paths to be read
			_parent.RetrievedEvent.WaitOne();

			// _orderedPicturesToAvoid
			foreach (var picPath in _avoidPicPaths)
			{
				var picIndex = _parent.PicPathToIndex(picPath);
				if (!_orderedPicturesToAvoid.Contains(picIndex))
					_orderedPicturesToAvoid.Add(picIndex);
			}
			_orderedPicturesToAvoid.Sort();

			PopulatePicIndexMappingAndKeys();

			// We are done with pictures to avoid.  Announce it!SS
			IsDoneLoadingEvent.Set();
			Interlocked.Exchange(ref _isDoneLoading, 1);
		}

		public bool IsDoneLoading => Interlocked.CompareExchange(ref _isDoneLoading, 1, 1) == 1;

		public void ClearPicsToAvoid()
		{
			FlatToPicIndexMapping.Clear();
			_orderedKeys.Clear();
			_orderedPicturesToAvoid.Clear();
			_configValue.UpdatePicturesToAvoid(null);
		}

		/// <summary>
		/// Purpose:
		///		Add a picture to the list of pictures to avoid
		///
		/// Algorithm needs to affect
		/// 		private readonly Dictionary{int, int} FlatToPicIndexMapping
		///			private readonly Collection{string} _avoidPicPaths
		///			private List{int} _orderedKeys
		///			private readonly Collection{int} _orderedPicturesToAvoid
		/// </summary>
		/// <param name="picIndex"></param>
		/// <returns></returns>
		public bool AddPictureToAvoid(int picIndex)
		{
			if (_orderedPicturesToAvoid.Contains(picIndex)) return false;

			_orderedPicturesToAvoid.Add(picIndex);
			_orderedPicturesToAvoid.Sort();
			_avoidPicPaths = RepopulatePicturesPathToAvoid(_orderedPicturesToAvoid);
			PopulatePicIndexMappingAndKeys();
			_configValue.UpdatePicturesToAvoid(_avoidPicPaths);

			return true;
		}

		public bool RemovePictureToAvoid(int picIndex)
		{
			if (!_orderedPicturesToAvoid.Contains(picIndex))
			{
				Log.Error($"Unexpected picture index:  Need to remove non existent picIndex: {picIndex} from the list ({string.Join(",", _orderedPicturesToAvoid)})");
				return false;
			}

			_orderedPicturesToAvoid.Remove(picIndex);
			_orderedPicturesToAvoid.Sort();
			_avoidPicPaths = RepopulatePicturesPathToAvoid(_orderedPicturesToAvoid);
			PopulatePicIndexMappingAndKeys();
			_configValue.UpdatePicturesToAvoid(_avoidPicPaths);

			return true;
		}

		public int GetPictureIndexFromFlatIndex(int flatIndex)
		{
			if (FlatToPicIndexMapping.Count == 0) return flatIndex;

			var upperSmallestIndex = FindGreatestSmallerIndex(flatIndex);
			return upperSmallestIndex < 0 ? flatIndex : flatIndex + FlatToPicIndexMapping[upperSmallestIndex];
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

		public bool IsPictureToAvoid(int index) => _orderedPicturesToAvoid.Contains(index);

		private int _populatePictureMappingFlag = 0;

		/// <summary>
		/// Name:	PopulatePicIndexMappingAndKeys
		/// Purpose:
		///		Populates the FlatToPicIndexMapping dictionary.  Thereafter, in order to translate 
		///		the index from a flatIndex to picIndex we need to add the appropriate
		///		FlatToPicIndexMapping entry, <see cref="GetPictureIndexFromFlatIndex(int)"/>
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
		///			for (var i = 0; i &lt; n - avoided-count; ++i)
		///			{
		///				while (FlatToPicIndexMapping.ContainsKey(picIndex)) ++picIndex;
		///				++picIndex;
		///			}
		///			while (FlatToPicIndexMapping.ContainsKey(picIndex)) ++picIndex;
		///			return picIndex;
		///		</code>
		///
		///		We can check if an index is in pictures-to-avoid set by setting a dictionary
		///		as follows:
		///			FlatToPicIndexMapping[ 3] = 1		// We cannot have picture-index = 3, therefore 
		///												// .. skip 3 by adding 1 to flatIndex.
		///			FlatToPicIndexMapping[10] = 5		// If we encounter picture-index = 10 then skip 
		///												// .. all range, picture-index = flatIndex + 5 
		///			FlatToPicIndexMapping[25] = 1
		///		Then check against _flatToPicIndexMapping.Contains(index).  For this data structure to work
		///		we need to introduce a new algorithm.
		/// 
		///		This algorithm will need to first find the greatest-small-index
		///			Explanation of the greatest-small-index:
		///				looking for flat-index 13, the dictionary contains smaller indices: 3
		///				and 10, but we are looking for the greatest of the 2.  Then we need to 
		///				sum FlatToPicIndexMapping[3] and FlatToPicIndexMapping[10] to the flatIndex in order to come
		///				up with the correct picIndex.
		/// 
		///		This is getting complicated.  We can improve upon this algorithm as follows:
		///												// If flatIndex &lt; 3 then picIndex = flatIndex
		///												// Thereafter we need to add FlatToPicIndexMapping[3] to the
		///												// next picIndex range.
		///			FlatToPicIndexMapping[     3] = 1	// if flatIndex >= 3 and &lt; 9 (10 - 1) 
		///												// .. (where 10 is the original flatIndex and 1
		///												// .. is the FlatToPicIndexMapping[3]) then
		///												// .. picIndex = flatIndex + FlatToPicIndexMapping[3]
		///			FlatToPicIndexMapping[10 - 1] = 6	// If flatIndex >= 9 (10 - 1) and &lt; 19
		///												// .. (19 = 25 next index in FlatToPicIndexMapping -
		///												// .. new FlatToPicIndexMapping[10]: 25 - 6)
		///												// .. then picIndex = flatIndex + FlatToPicIndexMapping[9]
		///			FlatToPicIndexMapping[25 - 6] = 7	// If flatIndex >= 19 (25 - 6) then
		///												// .. picIndex = flatIndex + FlatToPicIndexMapping[19]
		///
		///		Now the code to convert flatIndex to picIndex is as follows:
		///		<code>
		///			upperSmallestIndex = FindGreatestSmallerIndex(flatIndex);
		///			if flatIndex is &lt; FlatToPicIndexMapping.SmallestIndex
		///				picIndex = flatIndex;
		///			else
		///				picIndex = flatIndex + FlatToPicIndexMapping[upperSmallestIndex];
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
		private void PopulatePicIndexMappingAndKeys()
		{
			// Make sure parent is done loading
			var isLoading = Interlocked.CompareExchange(ref _populatePictureMappingFlag, 1, 1) == 1;
			var success = _parent.RetrievedEvent.WaitOne(WaitForPicturesToLoad);
			if (!success || isLoading)
			{
				_populatePicIndexMappingAndKeysDone = false;
				return;
			}

			Interlocked.CompareExchange(ref _populatePictureMappingFlag, 1, 0);

			try
			{
				FlatToPicIndexMapping.Clear();
				if (!_orderedPicturesToAvoid.Any())
				{
					FlatToPicIndexMapping.Clear();
					_orderedKeys.Clear();
					return;
				}

				var inx = 0;
				foreach (var x in _orderedPicturesToAvoid)
				{
					var flatIndex = x - inx;
					FlatToPicIndexMapping[flatIndex] = ++inx;
				}

				_orderedKeys = FlatToPicIndexMapping.Keys.ToList();
				_orderedKeys.Sort();
			}
			finally
			{
				Interlocked.CompareExchange(ref _populatePictureMappingFlag, 0, 1);
				CustomEventAggregator.Inst.Publish(new PictureLoadingDoneEventArgs(true));
			}
		}

		private ThreadSafeList<string> RepopulatePicturesPathToAvoid(IEnumerable<int> orderedPicturesToAvoid)
		{
			var avoidPicPaths = new ThreadSafeList<string>();
			foreach (var picIndex in orderedPicturesToAvoid)
				avoidPicPaths.Add(_parent.PicIndexToPath(picIndex));

			return avoidPicPaths;
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

		public void OnEvent(PictureLoadingDoneEventArgs e)
		{
			if (!_populatePicIndexMappingAndKeysDone)
				Task.Run(Initialize);
		}
	}
}
