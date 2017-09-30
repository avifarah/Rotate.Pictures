using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rotate.Pictures.Utility;


namespace UnitTest.Rotate.Pictures
{
	[TestClass]
	public class LinqExtensionsTest
	{
		class Person
		{
			public string Father { get; set; }

			public string Mother { get; set; }

			public string Name { get; set; }
		}

		private List<Person> _abrahamFamily;
		private List<Person> _isaacFamily;

		[TestInitialize]
		public void InitializeTest()
		{
			_abrahamFamily = new List<Person> {
				new Person { Father = "Terah", Name = "Abraham" },
				new Person { Father = "Terah", Name = "Sarah" },
				new Person { Name = "Hagar" },
				new Person { Father = "Abraham", Mother = "Hagar", Name = "Ishmael" },
				new Person { Father = "Abraham", Mother = "Sarah", Name = "Isaac" }
			};

			_isaacFamily = new List<Person> {
				new Person { Father = "Abraham", Mother = "Sarah", Name = "Isaac" },
				new Person { Father = "Bethuel", Name = "Rebecca" },
				new Person { Father = "Isaac", Mother = "Rebecca", Name = "Esau" },
				new Person { Father = "Isaac", Mother = "Rebecca", Name = "Jacob" }
			};
		}

		[TestMethod]
		public void ExceptTest()
		{
			// Arrange
			var expected = new List<Person> {
				new Person { Father = "Terah", Name = "Abraham" },
				new Person { Father = "Terah", Name = "Sarah" },
				new Person { Name = "Hagar" },
				new Person { Father = "Abraham", Mother = "Hagar", Name = "Ishmael" },
			};

			// Act
			var actual = _abrahamFamily.Except(_isaacFamily, (x, y) => x.Name == y.Name, x => x.Name.GetHashCode()).ToList();

			// Assert
			Assert.AreEqual(expected.Count, actual.Count);
			foreach (var a in actual)
				Assert.IsTrue(expected.Any(p => p.Name == a.Name));
		}

		[TestMethod]
		public void IntersectTest()
		{
			// Arrange
			var expected = new List<Person> {
				new Person { Father = "Abraham", Mother = "Sarah", Name = "Isaac" }
			};

			// Act
			var actual = _abrahamFamily.Intersect(_isaacFamily, (x, y) => x.Name == y.Name, x => x.Name.GetHashCode()).ToList();

			// Assert
			Assert.AreEqual(expected.Count, actual.Count);
			foreach (var a in actual)
				Assert.IsTrue(expected.Any(p => p.Name == a.Name));
		}

		[TestMethod]
		public void UnionTest()
		{
			// Arrange
			var expected = new List<Person> {
				new Person { Father = "Terah", Name = "Abraham" },
				new Person { Father = "Terah", Name = "Sarah" },
				new Person { Name = "Hagar" },
				new Person { Father = "Abraham", Mother = "Hagar", Name = "Ishmael" },
				new Person { Father = "Abraham", Mother = "Sarah", Name = "Isaac" },
				new Person { Father = "Bethuel", Name = "Rebecca" },
				new Person { Father = "Isaac", Mother = "Rebecca", Name = "Esau" },
				new Person { Father = "Isaac", Mother = "Rebecca", Name = "Jacob" }
			};

			// Act
			var actual = _abrahamFamily.Union(_isaacFamily, (x, y) => x.Name == y.Name, x => x.Name.GetHashCode()).ToList();

			// Assert
			Assert.AreEqual(expected.Count, actual.Count);
			foreach (var a in actual)
				Assert.IsTrue(expected.Any(p => p.Name == a.Name));
		}
	}
}
