using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rotate.Pictures.Model;
using Rotate.Pictures.Utility;


namespace UnitTest.Rotate.Pictures
{
	/// <summary>
	/// Summary description for SelectionTrackerTest
	/// </summary>
	[TestClass]
	public class SelectionTrackerTest
	{
		//public SelectionTrackerTest() { }

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
			ConfigurationManager.RefreshSection("appSettings");
		}

		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void AppendTest()
		{
			// Arrange
			var selectionTracker = new SelectionTracker(null);
			selectionTracker.ClearTracker();
			for (var i = 0; i < 100; ++i)
				selectionTracker.Append($"{i}.pic");

			// Act
			var actual = selectionTracker.Count;

			// Assert
			Assert.AreEqual(100, actual);
		}

		[TestMethod]
		public void AppendPassedEndTest()
		{
			// Arrange
			var selectionTracker = new SelectionTracker(null);
			var max = ConfigValue.Inst.MaxPictureTrackerDepth();
			for (var i = 0; i < 3 + max; ++i)
				selectionTracker.Append($"{i}.pic");

			// Act
			var actual = selectionTracker.Count;

			// Assert
			Assert.AreEqual(max, actual);
		}
	}
}
