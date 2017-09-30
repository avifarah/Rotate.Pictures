using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rotate.Pictures.Utility;


namespace UnitTest.Rotate.Pictures
{
	/// <summary>
	/// Summary description for PictureCollectionTest
	/// </summary>
	[TestClass]
	public class PictureCollectionTest
	{
		public PictureCollectionTest()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get => testContextInstance;
			set => testContextInstance = value;
		}

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
		public void ThisTest()
		{
			// Arrange
			var pc = new PictureCollection { "Abc", "Def", "Ghi" };
			const string expected = "Ghi";

			// Act
			var actual = pc[2];

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void CountTest()
		{
			// Arrange
			var pc = new PictureCollection { "Abc", "Def", "Ghi" };
			const int expected = 3;

			// Act
			var actual = pc.Count;

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void AddTest()
		{
			// Arrange
			var pc = new PictureCollection();
			pc.Clear();

			// Act
			pc.Add("Abc 123");
			pc.Add("Def 456");
			pc.Add("Ghi 789");

			// Assert
			Assert.AreEqual("Abc 123", pc[0]);
			Assert.AreEqual("Def 456", pc[1]);
			Assert.AreEqual("Ghi 789", pc[2]);
		}

		[TestMethod]
		public void ContainsAffirmativeTest()
		{
			// Arrange
			var pc = new PictureCollection { "Abc 123", "Def 456", "Ghi 789" };

			// Act
			var actual = pc.Contains("Def 456");

			// Assert
			Assert.IsTrue(actual);
		}

		[TestMethod]
		public void ContainsNegativeTest()
		{
			// Arrange
			var pc = new PictureCollection { "Abc 123", "Def 456", "Ghi 789" };

			// Act
			var actual = pc.Contains("Def Ghi");

			// Assert
			Assert.IsFalse(actual);
		}
	}
}
