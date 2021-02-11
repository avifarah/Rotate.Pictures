using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rotate.Pictures.Utility;

namespace UnitTest.Rotate.Pictures
{
	/// <summary>
	/// Summary description for StringUtilTest
	/// </summary>
	[TestClass]
	public class StringUtilTest
	{
		public StringUtilTest()
		{
		}

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

		[TestMethod]
		public void IsTrueSuccess1Test()
		{
			// Arrange
			const string text = "true";

			// Act
			var t = text.IsTrue();
			var f = text.IsFalse();

			// Assert
			Assert.IsTrue(t);
			Assert.IsFalse(f);
		}

		[TestMethod]
		public void IsTrueSuccess2Test()
		{
			// Arrange
			const string text = "tRUe";

			// Act
			var t = text.IsTrue();
			var f = text.IsFalse();

			// Assert
			Assert.IsTrue(t);
			Assert.IsFalse(f);
		}

		[TestMethod]
		public void IsTrueSuccess3Test()
		{
			// Arrange
			const string text = "t";

			// Act
			var t = text.IsTrue();
			var f = text.IsFalse();

			// Assert
			Assert.IsTrue(t);
			Assert.IsFalse(f);
		}

		[TestMethod]
		public void IsTrueSuccess4Test()
		{
			// Arrange
			const string text = "oK";

			// Act
			var t = text.IsTrue();
			var f = text.IsFalse();

			// Assert
			Assert.IsTrue(t);
			Assert.IsFalse(f);
		}

		[TestMethod]
		public void IsTrueSuccess5Test()
		{
			// Arrange
			const string text = "k";

			// Act
			var t = text.IsTrue();
			var f = text.IsFalse();

			// Assert
			Assert.IsTrue(t);
			Assert.IsFalse(f);
		}

		[TestMethod]
		public void IsTrueSuccess6Test()
		{
			// Arrange
			const string text = "yES";

			// Act
			var t = text.IsTrue();
			var f = text.IsFalse();

			// Assert
			Assert.IsTrue(t);
			Assert.IsFalse(f);
		}

		[TestMethod]
		public void IsTrueSuccess7Test()
		{
			// Arrange
			const string text = "y";

			// Act
			var t = text.IsTrue();
			var f = text.IsFalse();

			// Assert
			Assert.IsTrue(t);
			Assert.IsFalse(f);
		}

		[TestMethod]
		public void IsTrueSuccess8Test()
		{
			// Arrange
			const string text = "positive";

			// Act
			var t = text.IsTrue();
			var f = text.IsFalse();

			// Assert
			Assert.IsTrue(t);
			Assert.IsFalse(f);
		}

		[TestMethod]
		public void IsTrueSuccess9Test()
		{
			// Arrange
			const string text = "p";

			// Act
			var t = text.IsTrue();
			var f = text.IsFalse();

			// Assert
			Assert.IsTrue(t);
			Assert.IsFalse(f);
		}

		[TestMethod]
		public void IsTrueSuccess10Test()
		{
			// Arrange
			const string text = "+";

			// Act
			var t = text.IsTrue();
			var f = text.IsFalse();

			// Assert
			Assert.IsTrue(t);
			Assert.IsFalse(f);
		}

		[TestMethod]
		public void IsTrueSuccess11Test()
		{
			// Arrange
			const string text = "1";

			// Act
			var t = text.IsTrue();
			var f = text.IsFalse();

			// Assert
			Assert.IsTrue(t);
			Assert.IsFalse(f);
		}

		[TestMethod]
		public void IsTrueFailure1Test()
		{
			// Arrange
			const string text = "Abc";

			// Act
			var t = text.IsTrue();
			var f = text.IsFalse();

			// Assert
			Assert.IsFalse(t);
			Assert.IsFalse(f);
		}

		[TestMethod]
		public void IsTrueFailure2Test()
		{
			// Arrange
			const string text = "&";

			// Act
			var t = text.IsTrue();
			var f = text.IsFalse();

			// Assert
			Assert.IsFalse(t);
			Assert.IsFalse(f);
		}

		[TestMethod]
		public void IsTrueFailure3Test()
		{
			// Arrange
			const string text = "3";

			// Act
			var t = text.IsTrue();
			var f = text.IsFalse();

			// Assert
			Assert.IsFalse(t);
			Assert.IsFalse(f);
		}

		[TestMethod]
		public void IsFalseSuccess1Test()
		{
			// Arrange
			const string text = "fAlSe";

			// Act
			var f = text.IsFalse();
			var t = text.IsTrue();

			// Assert
			Assert.IsTrue(f);
			Assert.IsFalse(t);
		}

		[TestMethod]
		public void IsFalseSuccess2Test()
		{
			// Arrange
			const string text = "f";

			// Act
			var f = text.IsFalse();
			var t = text.IsTrue();

			// Assert
			Assert.IsTrue(f);
			Assert.IsFalse(t);
		}

		[TestMethod]
		public void IsFalseSuccess3Test()
		{
			// Arrange
			const string text = "no";

			// Act
			var f = text.IsFalse();
			var t = text.IsTrue();

			// Assert
			Assert.IsTrue(f);
			Assert.IsFalse(t);
		}

		[TestMethod]
		public void IsFalseSuccess4Test()
		{
			// Arrange
			const string text = "n";

			// Act
			var f = text.IsFalse();
			var t = text.IsTrue();

			// Assert
			Assert.IsTrue(f);
			Assert.IsFalse(t);
		}

		[TestMethod]
		public void IsFalseSuccess5Test()
		{
			// Arrange
			const string text = "nEgAtIvE";

			// Act
			var f = text.IsFalse();
			var t = text.IsTrue();

			// Assert
			Assert.IsTrue(f);
			Assert.IsFalse(t);
		}

		[TestMethod]
		public void IsFalseSuccess6Test()
		{
			// Arrange
			const string text = "-";

			// Act
			var f = text.IsFalse();
			var t = text.IsTrue();

			// Assert
			Assert.IsTrue(f);
			Assert.IsFalse(t);
		}

		[TestMethod]
		public void IsFalseSuccess7Test()
		{
			// Arrange
			const string text = "0";

			// Act
			var f = text.IsFalse();
			var t = text.IsTrue();

			// Assert
			Assert.IsTrue(f);
			Assert.IsFalse(t);
		}

		[TestMethod]
		public void IsFalseFailure1Test()
		{
			// Arrange
			const string text = "Abc";

			// Act
			var f = text.IsFalse();
			var t = text.IsTrue();

			// Assert
			Assert.IsFalse(f);
			Assert.IsFalse(t);
		}

		[TestMethod]
		public void IsFalseFailure2Test()
		{
			// Arrange
			const string text = "#";

			// Act
			var f = text.IsFalse();
			var t = text.IsTrue();

			// Assert
			Assert.IsFalse(f);
			Assert.IsFalse(t);
		}

		[TestMethod]
		public void IsFalseFailure3Test()
		{
			// Arrange
			const string text = "-10";

			// Act
			var f = text.IsFalse();
			var t = text.IsTrue();

			// Assert
			Assert.IsFalse(f);
			Assert.IsFalse(t);
		}

		[TestMethod]
		public void IsFloatNumericSuccess()
		{
			// Arrange
			List<bool> rs = new List<bool>();

			// Act
			foreach (var i in Enumerable.Range(0, 10))
				rs.Add(i.ToString().IsFloatNumeric());

			rs.Add("0.5".IsFloatNumeric());
			rs.Add("01234567890123".IsFloatNumeric());
			rs.Add("12,345,6,7".IsFloatNumeric());
			rs.Add("12,345.".IsFloatNumeric());
			rs.Add("123,456,789,012.345,67".IsFloatNumeric());
			rs.Add("123,456,789,012.345,678,901".IsFloatNumeric());

			// Assert
			foreach (var r in rs)
				Assert.IsTrue(r);
		}

		[TestMethod]
		public void IsFloatNumericFailure()
		{
			// Arrange
			List<bool> rs = new List<bool>();

			// Act
			rs.Add(",".IsFloatNumeric());
			rs.Add(".".IsFloatNumeric());
			rs.Add("a".IsFloatNumeric());
			rs.Add(";".IsFloatNumeric());
			rs.Add(".0123".IsFloatNumeric());
			rs.Add("123,456,789,012.345,678,901.".IsFloatNumeric());	// Trailing second period character (".")
			rs.Add("123,456,789,012.345,678,901,".IsFloatNumeric());	// Trailing comma

			// Assert
			foreach (var r in rs)
				Assert.IsFalse(r);
		}

		[TestMethod]
		public void IsIntNumericSuccess()
		{
			// Arrange
			List<bool> rs = new List<bool>();

			// Act
			foreach (var i in Enumerable.Range(0, 10))
				rs.Add(i.ToString().IsIntNumeric());

			// Assert
			foreach (var r in rs)
				Assert.IsTrue(r);
		}

		[TestMethod]
		public void IsIntNumericFailure()
		{
			// Arrange
			List<bool> rs = new List<bool>();

			// Act
			rs.Add("a".IsIntNumeric());
			rs.Add(".".IsIntNumeric());

			// Assert
			foreach (var r in rs)
				Assert.IsFalse(r);
		}
	}
}
