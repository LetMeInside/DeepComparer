using DeepComparer_xUnit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeepComparerNS;


namespace DeepComparer_xUnit.Tests
{
    public class BasicEqualityTests
    {
        [Fact]
        public void SimpleTypes_AreEqual()
        {
            Assert.True(DeepComparer.Compare(5, 5));
            Assert.True(DeepComparer.Compare("abc", "abc"));
        }


        [Fact]
        public void SimpleTypes_AreNotEqual()
        {
            Assert.False(DeepComparer.Compare(5, 6));
            Assert.False(DeepComparer.Compare("abc", "def"));
        }

        [Fact]
        public void NullHandling()
        {
            Person? a = null;
            Person? b = new Person();
            Assert.False(DeepComparer.Compare(a, b));
            Assert.True(DeepComparer.Compare<Person?>(null, null));
        }

        [Fact]
        public void TypeMismatch_IsNotEqual()
        {
            object a = 5;
            object b = "5";
            Assert.False(DeepComparer.Compare(a, b));
        }

        [Fact]
        public void ComplexObjects_Identical_AreEqual()
        {
            var p1 = new Person { Name = "A", Age = 30 };
            var p2 = new Person { Name = "A", Age = 30 };

            Assert.True(DeepComparer.Compare(p1, p2));

            var report = DeepComparer.CompareWithReport(p1, p2);
            Assert.True(report.AreEqual);
            Assert.Empty(report.Differences);
        }
    }
}
