using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rotate.Pictures.Utility;

namespace UnitTest.Rotate.Pictures
{
	/// <summary>
	/// Summary description for ImageExtTest
	/// </summary>
	[TestClass]
	public class ImageExtTest
	{
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
			_jpg = CreateAnEmptyFile(".jpg");
			_emptyAvi = CreateAnEmptyFile(".avi");
		}

		// Use TestCleanup to run code after each test has run
		[TestCleanup()]
		public void MyTestCleanup()
		{
			RemoveFile(_jpg);
			RemoveFile(_emptyAvi);
		}

		#endregion

		[TestMethod]
		public void AviEmptyTest()
		{
			// Arrange

			// Act 
			var rc = _emptyAvi.IsPictureValidFormat();

			// Assert
			Assert.IsFalse(rc);
		}

		private string _jpg;
		private string _emptyAvi;

		private string CreateAnEmptyFile(string fileNameExt)
		{
			var tempPath = Path.GetTempFileName();
			var fnNoExt = Path.GetFileNameWithoutExtension(tempPath);
			var tempDir = Path.GetDirectoryName(tempPath);
			var tempPathNewExt = Path.Combine(tempDir, $"{fnNoExt}{(fileNameExt[0] == '.' ? string.Empty : ".")}{fileNameExt}");
			File.Move(tempPath, tempPathNewExt);
			return tempPathNewExt;
		}

		private bool RemoveFile(string fileNameExt)
		{
			if (File.Exists(fileNameExt))
				File.Delete(fileNameExt);
			return true;
		}
	}
}
