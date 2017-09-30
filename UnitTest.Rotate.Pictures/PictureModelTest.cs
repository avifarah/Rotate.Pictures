using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		public PictureModelTest()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext _testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get => _testContextInstance;
			set => _testContextInstance = value;
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
		public void PictureModelPostiveTest()
		{
			// Arrange
			string pic = null;
			ConfigValue.Inst.SetInitialPictureDirectories(null);
			var model = new PictureModel();
			var t = Task.Delay(2000);
			t.Wait();

			// Act
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
