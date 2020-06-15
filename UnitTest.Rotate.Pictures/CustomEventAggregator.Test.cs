using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rotate.Pictures.EventAggregator;

namespace UnitTest.Rotate.Pictures
{
	[TestClass]
	public class CustomEventAggregatorTest
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
			ConfigurationManager.RefreshSection("appSettings");
		}

		// Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup() { }

		#endregion

		[TestMethod]
		public void TestEmptyContentPayload()
		{
			// Arrange
			var expected = 1;
			var p = new Publisher0();
			var s = new Subscriber0();

			// Act
			p.Publish();

			// Assert
			Assert.AreEqual(expected, s.Data);
		}

		[TestMethod]
		public void TestEvent1Publisher1Subscriber()
		{
			// Arrange
			var expected = 1;
			var p1 = new Publisher1(expected);
			var s1 = new Subscriber1();

			// Act
			p1.Publish1();

			// Assert
			Assert.IsNotNull(s1);
			Assert.IsNotNull(s1.Data);
			Assert.AreEqual(expected, s1.Data.EventData);
		}

		[TestMethod]
		public void TestEvent1Publisher2Subscriber()
		{
			// Arrange
			var expected = 2;
			var p1 = new Publisher1(expected);
			var s1 = new Subscriber1();
			var s12 = new Subscriber12();

			// Act
			p1.Publish1();

			// Assert
			Assert.IsNotNull(s1);
			Assert.IsNotNull(s1.Data);
			Assert.AreEqual(expected, s1.Data.EventData);

			Assert.IsNotNull(s12);
			Assert.IsNotNull(s12.Data1);
			Assert.AreEqual(expected, s12.Data1.EventData);
		}

		[TestMethod]
		public void TestEvent2Publisher1Subscriber()
		{
			// Arrange
			var expected = 4;
			var p1 = new Publisher1(3);
			var p2 = new Publisher1(expected);
			var s1 = new Subscriber1();

			// Act
			p1.Publish1();
			p2.Publish1();

			// Assert
			Assert.AreEqual(expected, s1.Data.EventData);
			Assert.AreEqual(2, s1.Count);
		}

		[TestMethod]
		public void TestEvent2PublisherMultipleSubscriber()
		{
			// Arrange
			var expected = 5;
			var expStr = "Five";
			var p1 = new Publisher1(expected);
			var p2 = new Publisher2(expStr);
			var s1 = new Subscriber1();
			var s2 = new Subscriber2();
			var s12 = new Subscriber12();

			// Act
			p1.Publish1();
			p2.Publish2();

			// Assert
			Assert.AreEqual(expected, s1.Data.EventData);
			Assert.AreEqual(expStr, s2.Data.EventData);
			Assert.AreEqual(expected, s12.Data1.EventData);
			Assert.AreEqual(expStr, s12.Data2.EventData);
		}

		[TestMethod]
		public void MultithreadedPublisherSubscriber()
		{
				// Arrange
				var expected1 = Enumerable.Range(0, 10).ToList();
				var expected2 = Enumerable.Range(0, 10).Select(x => $"--{x}--").ToList();
				var expected12 = Enumerable.Range(0, 10).Select(x => $"EventWrapper1. EventData: {x}")
					.Union(Enumerable.Range(0, 10).Select(x => $"EventWrapper2. EventData: --{x}--"))
					.ToList();
				var s1 = new Subscriber1();
				var s2 = new Subscriber2();
				var s12 = new Subscriber12();

				var t1 = Task.Run(() => {
					for (var i = 0; i < 10; ++i)
						new Publisher1(i).Publish1();
				});

				var t2 = Task.Run(() => {
					for (var i = 0; i < 10; ++i)
						new Publisher2($"--{i}--").Publish2();
				});

				// Act
				Task.WaitAll(new Task[] { t1, t2 });

				// Assert
				Assert.AreEqual(expected1.Count, s1.Results.Count);
				foreach (var s1r in s1.Results)
					Assert.IsTrue(expected1.Contains(s1r));

				Assert.AreEqual(expected2.Count, s2.Results.Count);
				foreach (var s2r in s2.Results)
					Assert.IsTrue(expected2.Contains(s2r));

				Assert.AreEqual(expected12.Count, s12.Results.Count);
				foreach (var s12r in s12.Results)
					Assert.IsTrue(expected12.Contains(s12r));
		}
	}

	class Publisher0
	{
		public void Publish() => CustomEventAggregator.Inst.Publish(new PictureLoadingDoneEventArgs(true));
	}

	class Publisher1
	{
		private readonly EventWrapper1 _e;

		public Publisher1(int data) => _e = new EventWrapper1(data);

		public void Publish1() => CustomEventAggregator.Inst.Publish(_e);
	}

	class Publisher2
	{
		private EventWrapper2 _e;

		public Publisher2(string data) => _e = new EventWrapper2(data);

		public void Publish2() => CustomEventAggregator.Inst.Publish(_e);
	}

	class Subscriber0 : ISubscriber<PictureLoadingDoneEventArgs>
	{
		public int Data { get; private set; }

		public Subscriber0() => CustomEventAggregator.Inst.Subscribe(this);

		public void OnEvent(PictureLoadingDoneEventArgs _) => ++Data;
	}

	class Subscriber1 : ISubscriber<EventWrapper1>
	{
		public List<int> Results = new List<int>();

		public EventWrapper1 Data { get; private set; }

		public int Count { get; private set; }

		public Subscriber1()
		{
			Results.Clear();
			CustomEventAggregator.Inst.Subscribe(this);
		}

		public void OnEvent(EventWrapper1 e)
		{
			Data = e;
			++Count;
			Results.Add(Data.EventData);
		}
	}

	class Subscriber2 : ISubscriber<EventWrapper2>
	{
		public List<string> Results = new List<string>();

		public EventWrapper2 Data { get; private set; }

		public Subscriber2()
		{
			Results.Clear();
			CustomEventAggregator.Inst.Subscribe(this);
		}

		public void OnEvent(EventWrapper2 e)
		{
			Data = e;
			Results.Add(Data.EventData);
		}
	}

	class Subscriber12 : ISubscriber<EventWrapper1>, ISubscriber<EventWrapper2>
	{
		public List<string> Results = new List<string>();

		public EventWrapper1 Data1 { get; private set; }

		public EventWrapper2 Data2 { get; private set; }

		public Subscriber12()
		{
			Results.Clear();
			CustomEventAggregator.Inst.Subscribe(this);
		}

		public void OnEvent(EventWrapper1 e)
		{
			Data1 = e;
			Results.Add($"EventWrapper1. EventData: {e.EventData}");
		}

		public void OnEvent(EventWrapper2 e)
		{
			Data2 = e;
			Results.Add($"EventWrapper2. EventData: {e.EventData}");
		}
	}

	class EventWrapper1 : EventArgs
	{
		public int EventData { get; }

		public EventWrapper1(int eventData) => EventData = eventData;
	}

	class EventWrapper2 : EventArgs
	{
		public string EventData { get; }

		public EventWrapper2(string eventData) => EventData = eventData;
	}
}
