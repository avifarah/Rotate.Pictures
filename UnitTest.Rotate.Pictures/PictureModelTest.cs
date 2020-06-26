using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rotate.Pictures.Model;
using Rotate.Pictures.Utility;

namespace UnitTest.Rotate.Pictures
{
	/// <summary>
	/// Summary description for PictureModelTest
	/// </summary>
	[TestClass]
	public class PictureModelTest
	{
		//public PictureModelTest() { }

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		//public TestContext TestContext { get; set; }

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
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void PictureModelPositiveTest()
		{
			// Arrange
			var mockConfig = new Mock<IConfigValue>();
			mockConfig.Setup(mc => mc.StillPictureExtensions()).Returns(new List<string> { ".jpg", ".png", ".bmp" });
			mockConfig.Setup(mc => mc.MotionPictures()).Returns(new List<string> { ".avi", ".jpeg", ".Peggy", ".Ben" });
			mockConfig.Setup(mc => mc.InitialPictureDirectories()).Returns(new [] { @"C:\dev\Rotating.Pictures\Testing" });
			mockConfig.Setup(mc => mc.FileExtensionsToConsider()).Returns(new List<string> { ".jpg", ".png", ".bmp", ".avi", ".jpeg", ".Peggy", ".Ben" });
			mockConfig.Setup(mc => mc.FirstPictureToDisplay()).Returns(@"C:\dev\Rotating.Pictures\Testing\Ben\IMG_0840-1.JPG");
			mockConfig.Setup(mc => mc.PicturesToAvoidPaths()).Returns(new List<string>());
			mockConfig.Setup(mc => mc.MaxPictureTrackerDepth()).Returns(5);
			var configValue = mockConfig.Object;

			var model = new PictureModel(configValue);

			var t = Task.Delay(2000);
			t.Wait();

			// Act
			string pic = null;
			var retrieving = model.IsPicturesRetrieving;

			for (int i = 0; i < 5 && pic == null; ++i)
			{
				pic = model.GetNextPicture();
				t = Task.Delay(2);
				t.Wait();
			}

			// Assert
			Assert.IsNotNull(pic);
		}
	}
}
