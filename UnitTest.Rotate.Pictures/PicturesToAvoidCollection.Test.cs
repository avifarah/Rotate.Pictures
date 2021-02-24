using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rotate.Pictures.Model;
using Rotate.Pictures.Utility;

namespace UnitTest.Rotate.Pictures
{
	[TestClass]
	public class PicturesToAvoidCollectionTest
	{
		//public PicturesToAvoidCollectionTest() { }

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
		}

		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		public static void MyClassCleanup()
		{
		}

		// Use TestInitialize to run code before running each test 
		[TestInitialize()]
		public void MyTestInitialize()
		{
			ConfigValue.Inst.UpdatePicturesToAvoid(new List<string>(), p => 1);

			// PictureModel
			//		public int CurrentPicIndex { get; set; }
			//		public IReadOnlyList<int> PicturesToAvoid => _avoidCollection.PicturesToAvoid;
			//		public override string PicIndexToPath(int picIndex) => _picCollection[picIndex];
			//		public override int PicPathToIndex(string path) => _picCollection[path];
			//		public bool IsPictureToAvoid(int index) => _avoidCollection.IsPictureToAvoid(index);
			//		public bool IsPictureToAvoid(string path) => _avoidCollection.IsPictureToAvoid(PicPathToIndex(path));
			//		public bool IsCollectionContains(string path) => _picCollection.Contains(path);
			//		public string GetNextPicture()
			//		public void AddPictureToAvoid(int picToAvoid) => _avoidCollection.AddPictureToAvoid(picToAvoid);
			//		public void RemovePictureToAvoid(int picToAvoid) => _avoidCollection.RemovePictureToAvoid(picToAvoid);

			//		public void SelectionTrackerAppend(string pic) => _selectionTracker.Append(pic);
			//		public void SelectionTrackerSetMaxPictureDepth(int depth) => _selectionTracker.SetMaxPictureDepth(depth);
			//		public bool SelectionTrackerAtHead => _selectionTracker.AtHead;
			//		public bool SelectionTrackerAtTail => _selectionTracker.AtTail;
			//		public string SelectionTrackerPrev() => _selectionTracker.Prev();
			//		public string SelectionTrackerNext() => _selectionTracker.Next();
			//		public int SelectionTrackerCount => _selectionTracker.Count;
	}

	// Use TestCleanup to run code after each test has run
	// [TestCleanup()]
	public void MyTestCleanup()
		{
		}

		#endregion

		[TestMethod]
		public void AvoidFirstPictureTest()
		{
			// Arrange
			var mockConfig = new Mock<IConfigValue>();
			var picN = 10;
			var path = @"dir\fldr";
			mockConfig.Setup(mc => mc.StillPictureExtensions()).Returns(new List<string> { ".s1", ".s2" });
			mockConfig.Setup(mc => mc.MotionPictures()).Returns(new List<string> { ".m1", ".m2" });
			mockConfig.Setup(mc => mc.InitialPictureDirectories()).Returns(new[] { path });
			mockConfig.Setup(mc => mc.FileExtensionsToConsider()).Returns(new List<string> { ".s1", ".m1" });
			mockConfig.Setup(mc => mc.FirstPictureToDisplay()).Returns(path);

			var pathToAvoid = @"dir\fldr\a";
			var picToAvoidPath = pathToAvoid;
			var picToAvoidPaths = new List<string> { picToAvoidPath };
			mockConfig.Setup(mc => mc.PicturesToAvoidPaths()).Returns(picToAvoidPaths);
			mockConfig.Setup(mc => mc.MaxPictureTrackerDepth()).Returns(5);
			var configValue = mockConfig.Object;

			//var model = new MockPicModel(configValue, new List<string> { path }, new List<int> { 0 },
			//	configValue.StillPictureExtensions().Union(configValue.MotionPictures()).Union(configValue.FileExtensionsToConsider()));
			//var picsToAvoid = new PicturesToAvoidCollection(model, configValue);
			//picsToAvoid.AddPictureToAvoid(model.PicPathToIndex(picToAvoidPath));

			//for (var flatIndex = 0; flatIndex < picN - picToAvoidPaths.Count; ++flatIndex)
			//{
			//	// Act
			//	var pictureIndex = picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex);

			//	// Assert
			//	Assert.AreEqual(flatIndex + 1, pictureIndex);
			//}
		}

		[TestMethod]
		public void AvoidMiddlePictureTest()
		{
			// Arrange
			var picN = 10;
			var mockConfig = new Mock<IConfigValue>();
			mockConfig.Setup(mc => mc.StillPictureExtensions()).Returns(new List<string> { ".jpg", ".png", ".bmp" });
			mockConfig.Setup(mc => mc.MotionPictures()).Returns(new List<string> { ".avi", ".jpeg", ".Peggy", ".Ben" });
			mockConfig.Setup(mc => mc.InitialPictureDirectories()).Returns(new[] { @"C:\dev\Rotating.Pictures\Testing" });
			mockConfig.Setup(mc => mc.FileExtensionsToConsider()).Returns(new List<string> { ".jpg", ".png", ".bmp", ".avi", ".jpeg", ".Peggy", ".Ben" });
			mockConfig.Setup(mc => mc.FirstPictureToDisplay()).Returns(@"A");

			var picToAvoidPath = Path.GetFullPath(@"G");		// index: 6
			var picToAvoidPaths = new List<string> { picToAvoidPath };
			var model = new PictureModel(mockConfig.Object);
			mockConfig.Setup(mc => mc.PicturesToAvoidPaths()).Returns(picToAvoidPaths);
			mockConfig.Setup(mc => mc.MaxPictureTrackerDepth()).Returns(5);
			var configValue = mockConfig.Object;

			var picsToAvoid = new PicturesToAvoidCollection(model, configValue);
			foreach (var path in picToAvoidPaths)
				picsToAvoid.AddPictureToAvoid(model.PicPathToIndex(path));

			for (var flatIndex = 0; flatIndex < picN - picToAvoidPaths.Count; ++flatIndex)
			{
				// Act
				var pictureIndex = picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex);

				// Assert
				var expected = flatIndex < 6 ? flatIndex : flatIndex + 1;
				Assert.AreEqual(expected, pictureIndex);
			}
		}

		[TestMethod]
		public void AvoidLastPictureTest()
		{
			// Arrange
			// TODO: test
			var mockValue = new Mock<IConfigValue>();
			var picN = 10;
			var picToAvoidPath = Path.GetFullPath(@"..\..\..\..\Testing\2007-02-04-2043-00\IMG_2410_edited-1.jpg");
			var picIndicesToAvoid = new List<string> { picToAvoidPath };
			var model = new PictureModel(mockValue.Object);
			var picsToAvoid = new PicturesToAvoidCollection(model, mockValue.Object);
			picsToAvoid.AddPictureToAvoid(model.PicPathToIndex(picToAvoidPath));

			for (var flatIndex = 0; flatIndex < picN - picIndicesToAvoid.Count; ++flatIndex)
			{
				// Act
				var pictureIndex = picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex);

				// Assert
				Assert.AreEqual(flatIndex, pictureIndex);
			}
		}

		[TestMethod]
		public void AvoidRangeOfPictureTest()
		{
			// Arrange
			// TODO: test
			var picN = 10;
			var picIndicesToAvoid = new List<string> {
				Path.GetFullPath(@"..\..\..\..\Testing\2007-02-01-1403-05\IMG_2352.jpg"),			// Index: 2
				Path.GetFullPath(@"..\..\..\..\Testing\2007-02-01-1403-05\IMG_2352_edited-1.jpg"),	// Index: 3
				Path.GetFullPath(@"..\..\..\..\Testing\2007-02-01-1403-05\IMG_2353.jpg"),			// Index: 4
				Path.GetFullPath(@"..\..\..\..\Testing\2007-02-01-1403-05\IMG_2353_edited-2.jpg")	// Index: 5
			};
			var mockValue = new Mock<IConfigValue>();
			mockValue.Setup(mc => mc.PicturesToAvoidPaths()).Returns(picIndicesToAvoid.ToArray());
			mockValue.Setup(mc => mc.InitialPictureDirectories()).Returns(new[] { @"C:\dev\Rotating.Pictures\Testing" });
			mockValue.Setup(mc => mc.FileExtensionsToConsider()).Returns(new List<string> { ".jpg", ".png", ".bmp", ".avi", ".jpeg", ".Peggy", ".Ben" });
			mockValue.Setup(mc => mc.StillPictureExtensions()).Returns(new List<string> { ".jpg", ".png", ".bmp" });
			mockValue.Setup(mc => mc.MotionPictures()).Returns(new List<string> { ".avi", ".jpeg", ".Peggy", ".Ben" });
			mockValue.Setup(mc => mc.FirstPictureToDisplay()).Returns(@"C:\dev\Rotating.Pictures\Testing\Ben\IMG_0840-1.JPG");
			mockValue.Setup(mc => mc.MaxPictureTrackerDepth()).Returns(5);
			var configValue = mockValue.Object;

			var model = new PictureModel(configValue);
			var picsToAvoid = new PicturesToAvoidCollection(model, mockValue.Object);
			foreach (var pic in picIndicesToAvoid)
				picsToAvoid.AddPictureToAvoid(model.PicPathToIndex(pic));

			for (var flatIndex = 0; flatIndex < picN - picIndicesToAvoid.Count; ++flatIndex)
			{
				// Act
				var pictureIndex = picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex);

				// Assert
				var expected = flatIndex < 2 ? flatIndex : flatIndex + picIndicesToAvoid.Count;
				Assert.AreEqual(expected, pictureIndex);
			}
		}

#if false
		[TestMethod]
		public void AvoidMultipleRangeOfPicture_StartWithLowestBoundIndex_OutOfOrder_Test()
		{
			// Arrange
			var picN = 30;
			var picIndicesToAvoid = new List<int> { 0, 4, 5, 6, 10, 11, 15, 29 };
			var picsToAvoid = new PicturesToAvoidCollection(picIndicesToAvoid);

			var pictureIndex = new List<int>();
			var expectedIndex = new List<int> { 1, 2, 3, 7, 8, 9, 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28};

			// Act
			for (var flatIndex = 0; flatIndex < picN - picIndicesToAvoid.Count; ++flatIndex)
			{
				var inx = picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex);
				pictureIndex.Add(inx);
			}

			// Assert
			Assert.AreEqual(expectedIndex.Count, picN - picIndicesToAvoid.Count);
			for (var i = 0; i < expectedIndex.Count; ++i)
				Assert.AreEqual(expectedIndex[i], pictureIndex[i]);
		}

		[TestMethod]
		public void AvoidMultipleRangeOfPicture_OutOfOrderTest()
		{
			// Arrange
			var picN = 30;
			var picIndicesToAvoid = new List<int> { 9, 7, 4, 6, 10, 11, 15, 29, 5 };
			var picsToAvoid = new PicturesToAvoidCollection(picIndicesToAvoid);

			var pictureIndex = new List<int>();
			var expectedIndex = new List<int> { 0, 1, 2, 3, 8, 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28 };

			// Act
			for (var flatIndex = 0; flatIndex < picN - picIndicesToAvoid.Count; ++flatIndex)
			{
				var inx = picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex);
				pictureIndex.Add(inx);
			}

			// Assert
			Assert.AreEqual(expectedIndex.Count, picN - picIndicesToAvoid.Count);
			for (var i = 0; i < expectedIndex.Count; ++i)
				Assert.AreEqual(expectedIndex[i], pictureIndex[i]);
		}

		[TestMethod]
		public void AvoidMultipleRangeOfPicture_NoConsecutive_RangesTest()
		{
			// Arrange
			var picN = 30;
			var picIndicesToAvoid = new List<int> { 9, 5, 20 };
			var picsToAvoid = new PicturesToAvoidCollection(picIndicesToAvoid);

			var pictureIndex = new List<int>();
			var expectedIndex = new List<int> { 0, 1, 2, 3, 4, 6, 7, 8, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 21, 22, 23, 24, 25, 26, 27, 28, 29 };

			// Act
			for (var flatIndex = 0; flatIndex < picN - picIndicesToAvoid.Count; ++flatIndex)
			{
				var inx = picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex);
				pictureIndex.Add(inx);
			}

			// Assert
			Assert.AreEqual(expectedIndex.Count, picN - picIndicesToAvoid.Count);
			for (var i = 0; i < expectedIndex.Count; ++i)
				Assert.AreEqual(expectedIndex[i], pictureIndex[i]);
		}

		[TestMethod]
		public void AvoidMultipleRangeOfPicture_AvoidAllPictures_Test()
		{
			// Arrange
			var picN = 30;
			var picIndicesToAvoid = new List<int>();
			for (var i = 0; i < picN; ++i) picIndicesToAvoid.Add(i);
			//var picsToAvoid = new PicturesToAvoidCollection(picIndicesToAvoid);

			// Act
			var avoidedPicturesCount = picIndicesToAvoid.Count;

			// Assert
			Assert.AreEqual(picN, avoidedPicturesCount);
		}

		[TestMethod]
		public void AvoidMultipleRangeOfPicture_rangeSplitAndOutOfOrder_Test()
		{
			// Arrange
			var picN = 30;
			var picIndicesToAvoid = new List<int> { 15, 17, 18, 19, 20, 21, 24, 11, 16, 12, 14, 10, 13, 9 };
			var picsToAvoid = new PicturesToAvoidCollection(picIndicesToAvoid);
			var expected = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 22, 23, 25, 26, 27, 28, 29 };

			// Act
			var pictureIndex = new List<int>();
			for (var flatIndex = 0; flatIndex < picN - picIndicesToAvoid.Count; ++flatIndex)
				pictureIndex.Add(picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex));

			// Assert
			for (var flatIndex = 0; flatIndex < picN - picIndicesToAvoid.Count; ++flatIndex)
				Assert.AreEqual(expected[flatIndex], pictureIndex[flatIndex]);
		}

		[TestMethod]
		public void AddPictureToAvoidTest()
		{
			// Arrange
			var picN = 10;
			//var picsToAvoid = PicturesToAvoidCollection.Default;
			var picsToAvoid = new PicturesToAvoidCollection(new List<int>());
			picsToAvoid.AddPictureToAvoid(5);
			var pictureIndex = new List<int>();

			// Act
			for (var flatIndex = 0; flatIndex < picN - 1; ++flatIndex)
				pictureIndex.Add(picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex));

			// Assert
			for (var flatIndex = 0; flatIndex < picN - 1; ++flatIndex)
			{
				var expected = flatIndex < 5 ? flatIndex : flatIndex + 1;
				Assert.AreEqual(expected, pictureIndex[flatIndex]);
			}
		}

		[TestMethod]
		public void Add3PicturesToAvoidTest()
		{
			// Arrange
			var picN = 10;
			//var picsToAvoid = PicturesToAvoidCollection.Default;
			var picsToAvoid = new PicturesToAvoidCollection(new List<int>());
			picsToAvoid.AddPictureToAvoid(5);
			picsToAvoid.AddPictureToAvoid(3);
			picsToAvoid.AddPictureToAvoid(7);
			var expected = new List<int> { 0, 1, 2, 4, 6, 8, 9 };
			var pictureIndex = new List<int>();

			// Act
			for (var flatIndex = 0; flatIndex < picN - 3; ++flatIndex)
			{
				var actual = picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex);
				pictureIndex.Add(actual);
			}

			// Assert
			for (var flatIndex = 0; flatIndex < picN - 3; ++flatIndex)
			{
				var exp = expected[flatIndex];
				var act = pictureIndex[flatIndex];
				Assert.AreEqual(exp, act);
			}
		}

		[TestMethod]
		public void AddPicturesToAvoid2RangesTest()
		{
			// Arrange
			var picN = 10;
			var picsToAvoid = PicturesToAvoidCollection.Default;
			picsToAvoid.AddPictureToAvoid(5);
			picsToAvoid.AddPictureToAvoid(3);
			picsToAvoid.AddPictureToAvoid(7);
			picsToAvoid.AddPictureToAvoid(4);
			var expected = new List<int> { 0, 1, 2, 6, 8, 9 };
			var pictureIndex = new List<int>();

			// Act
			for (var flatIndex = 0; flatIndex < picN - 4; ++flatIndex)
				pictureIndex.Add(picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex));

			// Assert
			for (var flatIndex = 0; flatIndex < picN - 4; ++flatIndex)
				Assert.AreEqual(expected[flatIndex], pictureIndex[flatIndex]);
		}
#endif

		class MockPicModel : IPictureModel
		{
			private readonly List<string> _pathsToAvoid;

			private List<int> _indicesToAvoid;

			private Dictionary<string, int> _picInxMapping = new Dictionary<string, int>();

			private int _inx;

			private string _pathLetters = new string(Enumerable.Range(0, 'Z' - 'A' + 1).Select(i => (char)('A' + i)).ToArray());	// A .. Z

			private List<string> _extensions;

			public MockPicModel(IConfigValue configValue, List<string> pathsToAvoid, List<int> indicesToAvoid, IEnumerable<string> exts)
			{
				_extensions = exts.ToList();
				_pathsToAvoid = pathsToAvoid;
				_indicesToAvoid = indicesToAvoid;
				_inx = 0;

				RetrievePictures();
			}

			public string PicIndexToPath(int picIndex)
			{
				if (picIndex < 0) return picIndex.ToString();
				if (picIndex == 0) return "A";
				var path = new StringBuilder();
				while (picIndex > 0)
				{
					var inx = picIndex % _pathLetters.Length;
					path.Append(_pathLetters[inx]);
					picIndex = (picIndex - inx) / _pathLetters.Length;
				}

				return path.ToString();
			}

			public int PicPathToIndex(string path)
			{
				var inx = path.ToCharArray().Aggregate(0, (a, l) => a * 26 + char.ToLower(l) - 'a' + 1);
				return inx - 1;
			}

			private void RetrievePictures() => RetrievePictures(CancellationToken.None);

			protected void RetrievePictures(CancellationToken ct)
			{
				if (_extensions == null || _extensions.Count == 0) return;

				var cnt = _extensions.Count;
				//for (int i = 0; i < 100; ++i)
				//	_picCollection.Add($"{Num2Path(i)}.{_extensions[i % cnt]}");
			}

			private string Num2Path(int num)
			{
				var cols = new List<char>();
				var numP = num + 1;
				do
				{
					--numP;
					var ordCol = numP % 26;
					var nxtLet = (char)('A' + ordCol);
					cols.Add(nxtLet);
					numP = (numP - ordCol) / 26;
				}
				while (numP > 0);

				return string.Join(string.Empty, cols.AsEnumerable().Reverse());
			}
		}
	}
}
