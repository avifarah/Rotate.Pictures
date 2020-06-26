using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rotate.Pictures.Utility;


namespace UnitTest.Rotate.Pictures
{
	[TestClass]
	public class ConfigurationTest
	{
		[TestMethod]
		public void InitialFoldersTest()
		{
			// Arrange

			// Act
			var configValue = ConfigValueProvider.Default;
			var dirs = configValue.InitialPictureDirectories();

			// Assert
			//<add key="Initial Folders" value="M:\Pictures;G:\Pictures;\\xyz\Pic\Pictures\Pics"/>
			Assert.AreEqual(3, dirs.Length);
			Assert.AreEqual(@"M:\Pictures", dirs[0]);
			Assert.AreEqual(@"G:\Pictures", dirs[1]);
			Assert.AreEqual(@"\\xyz\Pic\Pictures\Pics", dirs[2]);
		}

		[TestMethod]
		public void MaxPictureTrackerDepthTest()
		{
			// Arrange
			int depth;

			// Act
			var configValue = ConfigValueProvider.Default;
			depth = configValue.MaxPictureTrackerDepth();

			// Assert
			//<add key = "Max picture tracker depth" value="9999"/>
			Assert.AreEqual(9999, depth);
		}

		[TestMethod]
		public void ExtensionsToConsiderTest()
		{
			// Arrange
			var configValue = ConfigValueProvider.Default;
			configValue.SetStillExtension(string.Empty);
			configValue.SetMotionExtension(string.Empty);
			var expected = new List<string>();

			// Act
			var actual = configValue.FileExtensionsToConsider();

			// Assert
			Assert.AreEqual(expected.Count, actual.Count);
			for (var i = 0; i < actual.Count; ++i)
				Assert.AreEqual(expected[i], actual[i]);
		}

		[TestMethod]
		public void ExtensionsToConder2Test()
		{
			// Arrange
			var configValue = ConfigValueProvider.Default;
			configValue.SetStillExtension(".Abc;.Def;.Ghi");
			configValue.SetMotionExtension(".Jkl;.Mno");
			var expected = new List<string> { ".Abc", ".Def", ".Ghi", ".Jkl", ".Mno" };

			// Act
			var actual = configValue.FileExtensionsToConsider();

			// Assert
			Assert.AreEqual(expected.Count, actual.Count);
			for (var i = 0; i < actual.Count; ++i)
				Assert.AreEqual(expected[i], actual[i]);
		}

		[TestMethod]
		public void ExtenstionsToConsider3Test()
		{
			// Arrange
			var configValue = ConfigValueProvider.Default;
			configValue.SetStillExtension(".Abc;.Def;.Ghi");
			configValue.SetMotionExtension(null);
			var expected = new List<string> { ".Abc", ".Def", ".Ghi" };

			// Act
			var actual = configValue.FileExtensionsToConsider();

			// Assert
			Assert.AreEqual(expected.Count, actual.Count);
			for (var i = 0; i < actual.Count; ++i)
				Assert.AreEqual(expected[i], actual[i]);
		}

		[TestMethod]
		public void ExtenstionsToConsider4Test()
		{
			// Arrange
			var configValue = ConfigValueProvider.Default;
			configValue.SetStillExtension(string.Empty);
			configValue.SetMotionExtension(".Abc;.Def;.Ghi");
			var expected = ".Abc;.Def;.Ghi".Split(new[] { ';' }).ToList();
			//expected.AddRange(".jpg;.png;.bmp".Split(new[] { ';' }).ToList());

			// Act
			var actual = configValue.FileExtensionsToConsider();

			// Assert
			Assert.AreEqual(expected.Count, actual.Count);
			for (var i = 0; i < actual.Count; ++i)
				Assert.AreEqual(expected[i], actual[i]);
		}

		[TestMethod]
		public void RestoreStillExtensionsTest()
		{
			var configValue = ConfigValueProvider.Default;
			var expected = new List<string> { ".jpg", ".bmp", ".gif", ".png", ".psd", ".tif" };

			var actual = configValue.RestoreStillExtensions.Split(new[] { ';' }).ToList();

			Assert.AreEqual(expected.Count, actual.Count);
			for (int i = 0; i < actual.Count; ++i)
				Assert.AreEqual(expected[i], actual[i]);
		}

		[TestMethod]
		public void RestoreMotionExtensionsTest()
		{
			var configValue = ConfigValueProvider.Default;
			var expected = new List<string> { ".mov", ".avi", ".mpg", ".mp4", ".wmv", ".3gp" };

			var actual = configValue.RestoreMotionExtensions.Split(new[] { ';' }).ToList();

			Assert.AreEqual(expected.Count, actual.Count);
			for (int i = 0; i < actual.Count; ++i)
				Assert.AreEqual(expected[i], actual[i]);
		}

		[TestMethod]
		public void ImageStrechValueTest()
		{
			// Arrange
			string actual;
			var configValue = ConfigValueProvider.Default;
			//<add key = "Image stretch" value="fILL"/>
			var expected = "Fill";

			// Act
			actual = configValue.ImageStretch();

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void SetInitialPictureDirectoriesTest()
		{
			// Arrange
			var expected = new string[] { "Abc", "Def" };
			var configValue = ConfigValueProvider.Default;
			configValue.SetInitialPictureDirectories("Abc;Def");

			// Act
			var actual = configValue.InitialPictureDirectories();

			// Assert
			Assert.AreEqual(expected.Length, actual.Length);
			for (int i = 0; i < actual.Length; ++i)
				Assert.AreEqual(expected[i], actual[i]);
		}
	}
}
