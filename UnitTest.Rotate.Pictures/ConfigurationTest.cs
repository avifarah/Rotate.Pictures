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
			var dirs = ConfigValue.Inst.InitialPictureDirectories();

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
			depth = ConfigValue.Inst.MaxPictureTrackerDepth();

			// Assert
			//<add key = "Max picture tracker depth" value="9999"/>
			Assert.AreEqual(9999, depth);
		}

		[TestMethod]
		public void ExtensionsToConsiderTest()
		{
			// Arrange
			ConfigValue.Inst.SetStillExtension(string.Empty);
			ConfigValue.Inst.SetMotionExtension(string.Empty);
			var expected = new List<string> { ".jpg", ".png", ".bmp", ".avi", ".jpeg", ".Peggy", ".Ben" };

			// Act
			var actual = ConfigValue.Inst.FileExtensionsToConsider();

			// Assert
			Assert.AreEqual(expected.Count, actual.Count);
			for (var i = 0; i < actual.Count; ++i)
				Assert.AreEqual(expected[i], actual[i]);
		}

		[TestMethod]
		public void ExtensionsToConder2Test()
		{
			// Arrange
			ConfigValue.Inst.SetStillExtension(".Abc;.Def;.Ghi");
			ConfigValue.Inst.SetMotionExtension(".Jkl;.Mno");
			var expected = new List<string> { ".Abc", ".Def", ".Ghi", ".Jkl", ".Mno" };

			// Act
			var actual = ConfigValue.Inst.FileExtensionsToConsider();

			// Assert
			Assert.AreEqual(expected.Count, actual.Count);
			for (var i = 0; i < actual.Count; ++i)
				Assert.AreEqual(expected[i], actual[i]);
		}

		[TestMethod]
		public void ExtenstionsToConsider3Test()
		{
			// Arrange
			ConfigValue.Inst.SetStillExtension(".Abc;.Def;.Ghi");
			ConfigValue.Inst.SetMotionExtension(null);
			var expected = new List<string> { ".Abc", ".Def", ".Ghi" };
			expected.AddRange(".avi;.jpeg;.Peggy;.Ben".Split(new[] { ';' }).ToList());

			// Act
			var actual = ConfigValue.Inst.FileExtensionsToConsider();

			// Assert
			Assert.AreEqual(expected.Count, actual.Count);
			for (var i = 0; i < actual.Count; ++i)
				Assert.AreEqual(expected[i], actual[i]);
		}

		[TestMethod]
		public void ExtenstionsToConsider4Test()
		{
			// Arrange
			ConfigValue.Inst.SetStillExtension(string.Empty);
			ConfigValue.Inst.SetMotionExtension(".Abc;.Def;.Ghi");
			var expected = ".jpg;.png;.bmp".Split(new[] { ';' }).ToList();
			expected.AddRange(".Abc;.Def;.Ghi".Split(new[] { ';' }).ToList());

			// Act
			var actual = ConfigValue.Inst.FileExtensionsToConsider();

			// Assert
			Assert.AreEqual(expected.Count, actual.Count);
			for (var i = 0; i < actual.Count; ++i)
				Assert.AreEqual(expected[i], actual[i]);
		}

		[TestMethod]
		public void RestoreStillExtensionsTest()
		{
			var expected = new List<string> { ".jpg", ".bmp", ".gif", ".png", ".psd", ".tif" };

			var actual = ConfigValue.Inst.RestoreStillExtensions.Split(new[] { ';' }).ToList();

			Assert.AreEqual(expected.Count, actual.Count);
			for (int i = 0; i < actual.Count; ++i)
				Assert.AreEqual(expected[i], actual[i]);
		}

		[TestMethod]
		public void RestoreMotionExtensionsTest()
		{
			var expected = new List<string> { ".mov", ".avi", ".mpg", ".mp4", ".wmv", ".3gp" };

			var actual = ConfigValue.Inst.RestoreMotionExtensions.Split(new[] { ';' }).ToList();

			Assert.AreEqual(expected.Count, actual.Count);
			for (int i = 0; i < actual.Count; ++i)
				Assert.AreEqual(expected[i], actual[i]);
		}

		[TestMethod]
		public void ImageStrechValueTest()
		{
			// Arrange
			string actual;
			//<add key = "Image stretch" value="fILL"/>
			var expected = "Fill";

			// Act
			actual = ConfigValue.Inst.ImageStretch();

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void SetInitialPictureDirectoriesTest()
		{
			// Arrange
			var expected = new string[] { "Abc", "Def" };
			ConfigValue.Inst.SetInitialPictureDirectories("Abc;Def");

			// Act
			var actual = ConfigValue.Inst.InitialPictureDirectories();

			// Assert
			Assert.AreEqual(expected.Length, actual.Length);
			for (int i = 0; i < actual.Length; ++i)
				Assert.AreEqual(expected[i], actual[i]);
		}
	}
}
