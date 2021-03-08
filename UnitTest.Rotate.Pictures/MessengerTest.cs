using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rotate.Pictures.MessageCommunication;

namespace UnitTest.Rotate.Pictures
{
	/// <summary>
	/// Summary description for MessengerTest
	/// </summary>
	[TestClass]
	public class MessengerTest
	{
		//public MessengerTest() { }

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

		public sealed class TestCommunication : IVmCommunication
		{
			public int Test = 0;
		}

		[TestMethod]
		public void DefaultMessengerTest()
		{
			// Arrange
			Messenger<TestCommunication>.Instance.Register(this, OnTestCommunication);

			// Act
			var m = new TestCommunication();
			Messenger<TestCommunication>.Instance.Send(m);

			// Assert
			Assert.AreEqual(1, m.Test);

			// Cleanup
			Messenger<TestCommunication>.Instance.Unregister(this);
		}

		private void OnTestCommunication(TestCommunication obj) => ++obj.Test;
	}
}
