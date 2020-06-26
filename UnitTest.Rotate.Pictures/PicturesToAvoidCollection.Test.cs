using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }

		// Use TestInitialize to run code before running each test 
		[TestInitialize()]
		public void MyTestInitialize()
		{
			ConfigValue.Inst.UpdatePicturesToAvoid(new List<string>());
		}

		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void AvoidFirstPictureTest()
		{
			// Arrange
			// TODO: test
			var mockConfig = new Mock<IConfigValue>();
			var picN = 10;
			mockConfig.Setup(mc => mc.StillPictureExtensions()).Returns(new List<string> { ".jpg", ".png", ".bmp" });
			mockConfig.Setup(mc => mc.MotionPictures()).Returns(new List<string> { ".avi", ".jpeg", ".Peggy", ".Ben" });
			mockConfig.Setup(mc => mc.InitialPictureDirectories()).Returns(new[] { @"C:\dev\Rotating.Pictures\Testing" });
			mockConfig.Setup(mc => mc.FileExtensionsToConsider()).Returns(new List<string> { ".jpg", ".png", ".bmp", ".avi", ".jpeg", ".Peggy", ".Ben" });
			mockConfig.Setup(mc => mc.FirstPictureToDisplay()).Returns(@"C:\dev\Rotating.Pictures\Testing\Ben\IMG_0840-1.JPG");

			// C:\dev\Rotating.Pictures\Testing\2007-02-01-1403-05\IMG_2351.jpg
			var picToAvoidPath = Path.GetFullPath(@"..\..\..\..\Testing\2007-02-01-1403-05\IMG_2351.jpg");
			var picToAvoidPaths = new List<string> { picToAvoidPath };
			mockConfig.Setup(mc => mc.PicturesToAvoidPaths()).Returns(picToAvoidPaths);
			mockConfig.Setup(mc => mc.MaxPictureTrackerDepth()).Returns(5);
			var configValue = mockConfig.Object;

			var model = new PictureModel(configValue);
			var picsToAvoid = new PicturesToAvoidCollection(model, configValue);
			picsToAvoid.AddPictureToAvoid(model.PicPathToIndex(picToAvoidPath));

			for (var flatIndex = 0; flatIndex < picN - picToAvoidPaths.Count; ++flatIndex)
			{
				// Act
				var pictureIndex = picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex);

				// Assert
				Assert.AreEqual(flatIndex + 1, pictureIndex);
			}
		}

		[TestMethod]
		public void AvoidMiddlePictureTest()
		{
			// Arrange
			// TODO: test
			var mockConfig = new Mock<IConfigValue>();
			var picN = 10;
			var picToAvoidPath = Path.GetFullPath(@"..\..\..\..\Testing\2007-02-01-1403-05\IMG_2355.jpg");
			var picIndicesToAvoid = new List<string> { picToAvoidPath };
			var model = new PictureModel(mockConfig.Object);
			var picsToAvoid = new PicturesToAvoidCollection(model, mockConfig.Object);
			picsToAvoid.AddPictureToAvoid(model.PicPathToIndex(picToAvoidPath));

			for (var flatIndex = 0; flatIndex < picN - picIndicesToAvoid.Count; ++flatIndex)
			{
				// Act
				var pictureIndex = picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex);

				// Assert
				var expected = flatIndex < 5 ? flatIndex : flatIndex + 1;
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
			{
				Debug.WriteLine($"pic: {pic,80}\t\tmodel.PicPathToIndex(pic): {model.PicPathToIndex(pic)}");
				picsToAvoid.AddPictureToAvoid(model.PicPathToIndex(pic));
			}

			for (var flatIndex = 0; flatIndex < picN - picIndicesToAvoid.Count; ++flatIndex)
			{
				// Act
				var pictureIndex = picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex);
				Debug.WriteLine($"picsToAvoid.GetPictureIndexFromFlatIndex(flatIndex(={flatIndex})): {pictureIndex}");

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
	}
}
