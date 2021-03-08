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

		public sealed class SecondTestCommunication : IVmCommunication
		{
			public int Test2 = 0;
		}

		private void OnTestCommunication(TestCommunication obj) => ++obj.Test;

		private void OnSecondTestCommunication(SecondTestCommunication obj) => obj.Test2 += 100;

		[TestMethod]
		public void DefaultMessengerTest()
		{
			// Arrange
			// Uses default context
			Messenger<TestCommunication>.Instance.Register(this, OnTestCommunication);

			// Act
			// No context is specified in the Send(..) => Send(..) uses default context
			var m = new TestCommunication();
			Messenger<TestCommunication>.Instance.Send(m);

			// Assert
			Assert.AreEqual(1, m.Test);

			// Cleanup
			// No context is specified => the Unregister(..) uses default context.
			Messenger<TestCommunication>.Instance.Unregister(this);
		}

		[TestMethod]
		public void SecondMessengerTest()
		{
			// Arrange
			// Explicitly named context.  Name could be any unique string.
			var context = nameof(SecondTestCommunication);
			Messenger<SecondTestCommunication>.Instance.Register(this, OnSecondTestCommunication, context);

			// Act
			var m = new SecondTestCommunication();
			Messenger<SecondTestCommunication>.Instance.Send(m, context);

			// Assert
			Assert.AreEqual(100, m.Test2);

			// Cleanup
			Messenger<SecondTestCommunication>.Instance.Unregister(this, context);
		}

		[TestMethod]
		public void TwoTestCommunicationClasses_TwoContextMessengerTest()
		{
			// Arrange
			// Explicitly named context.  Name could be any unique string.
			// Use different contexts to employ different IVmCommunication implementations.
			const string context1 = nameof(TestCommunication);
			const string context2 = nameof(SecondTestCommunication);
			Messenger<TestCommunication>.Instance.Register(this, OnTestCommunication, context1);
			Messenger<SecondTestCommunication>.Instance.Register(this, OnSecondTestCommunication, context2);

			var m1 = new TestCommunication();
			var m2 = new SecondTestCommunication();

			// Act
			Messenger<TestCommunication>.Instance.Send(m1, context1);
			Messenger<SecondTestCommunication>.Instance.Send(m2, context2);

			// Assert
			Assert.AreEqual(1, m1.Test);
			Assert.AreEqual(100, m2.Test2);

			// Cleanup
			Messenger<TestCommunication>.Instance.Unregister(this, context1);
			Messenger<SecondTestCommunication>.Instance.Unregister(this, context2);
		}
	}
}
