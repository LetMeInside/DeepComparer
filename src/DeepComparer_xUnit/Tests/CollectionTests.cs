using System.Collections.Generic;
using DeepComparer_xUnit.Models;
using Xunit;
using DeepComparerNS;

namespace DeepComparer_xUnit.Tests
{
    public class CollectionTests
    {
        [Fact]
        public void Unordered_Lists_Equal_By_Items()
        {
            var a = new WithCollections { Numbers = new List<int> { 1, 2, 3 } };
            var b = new WithCollections { Numbers = new List<int> { 3, 1, 2 } };

            Assert.True(DeepComparer.Compare(a, b));
        }

        [Fact]
        public void Different_Count_Makes_Collections_NotEqual()
        {
            var a = new WithCollections { Numbers = new List<int> { 1, 2, 3 } };
            var b = new WithCollections { Numbers = new List<int> { 1, 2, 3, 4 } };

            Assert.False(DeepComparer.Compare(a, b));
        }

        [Fact]
        public void Different_Items_Makes_Collections_NotEqual()
        {
            var a = new WithCollections { Numbers = [1, 2, 3] };
            var b = new WithCollections { Numbers = [1, 2, 4] };

            var eq = DeepComparer.Compare(a, b);
            Assert.False(eq);

            var report = DeepComparer.CompareWithReport(a, b);
            Assert.False(report.AreEqual);
            Assert.Contains(report.Differences, d => d.Contains("Numbers"));
        }

        [Fact]
        public void HashSets_Unordered_Are_Equal()
        {
            var a = new WithCollections { Tags = new HashSet<string> { "x", "y" } };
            var b = new WithCollections { Tags = new HashSet<string> { "y", "x" } };

            Assert.True(DeepComparer.Compare(a, b));
        }

        [Fact]
        public void Dictionaries_Compare_KeyValuePairs()
        {
            var a = new WithCollections();
            a.Scores["a"] = 1; a.Scores["b"] = 2;

            var b = new WithCollections();
            b.Scores["b"] = 2; b.Scores["a"] = 1;

            Assert.True(DeepComparer.Compare(a, b));
        }

        [Fact]
        public void FakeSourceList_Items_Are_Compared()
        {
            var a = new WithCollections();
            a.FakeList.Items.AddRange(new[] { "a", "b", "c" });

            var b = new WithCollections();
            b.FakeList.Items.AddRange(new[] { "c", "b", "a" });

            Assert.True(DeepComparer.Compare(a, b));
        }

        [Fact]
        public void FakeSourceCache_Values_Are_Compared_Unordered()
        {
            var a = new WithCollections();
            a.FakeCache.Inner["k1"] = 10;
            a.FakeCache.Inner["k2"] = 20;

            var b = new WithCollections();
            b.FakeCache.Inner["x"] = 20;
            b.FakeCache.Inner["y"] = 10;

            Assert.True(DeepComparer.Compare(a, b));
        }

        [Fact]
        public void Collections_Of_Complex_Types_Unordered()
        {
            var p1 = new Person { Name = "A", Age = 1 };
            var p2 = new Person { Name = "B", Age = 2 };
            var p3 = new Person { Name = "C", Age = 3 };

            var x = new WithCollections { Numbers = new List<int>() };
            var y = new WithCollections { Numbers = new List<int>() };

            // Instead, test Person lists via Persons container
            var a = new Person { Name = "root", Friends = new() { p1, p2, p3 } };
            var b = new Person { Name = "root", Friends = new() { p3, p1, p2 } };

            Assert.True(DeepComparer.Compare(a, b));
        }
    }
}
