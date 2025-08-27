
using DeepComparer_xUnit.Models;
using Xunit;
using DeepComparerNS;

namespace DeepComparer_xUnit.Tests
{
    public class RecursiveAndCircularReferenceTests
    {
        [Fact]
        public void Deep_Nesting_Without_Cycle_Is_Handled()
        {
            var a = new Node { Value = 1 };
            var cur = a;
            for (int i = 0; i < 50; i++)
            {
                cur.Next = new Node { Value = i };
                cur = cur.Next;
            }

            var b = new Node { Value = 1 };
            cur = b;
            for (int i = 0; i < 50; i++)
            {
                cur.Next = new Node { Value = i };
                cur = cur.Next;
            }

            Assert.True(DeepComparer.CompareProperties(a, b, true, true, true, new DeepComparer.CompareOptions
            {
                MaxDepth = 100
            }));
        }

        [Fact]
        public void Circular_References_Do_Not_Overflow_And_Can_Be_Equal()
        {
            var a1 = new Person { Name = "A" };
            var a2 = new Person { Name = "B" };
            a1.Friends.Add(a2);
            a2.Friends.Add(a1); // cycle

            var b1 = new Person { Name = "A" };
            var b2 = new Person { Name = "B" };
            b1.Friends.Add(b2);
            b2.Friends.Add(b1); // cycle

            Assert.True(DeepComparer.CompareProperties(a1, b1));
        }

        [Fact]
        public void WeakReferences_Are_Ignored()
        {
            var target1 = new Person { Name = "X" };
            var target2 = new Person { Name = "Y" };

            var a = new WithWeak
            {
                Marker = "same",
                TargetRef = new WeakReference(target1),
                GenericRef = new WeakReference<Person>(target1)
            };
            var b = new WithWeak
            {
                Marker = "same",
                TargetRef = new WeakReference(target2),
                GenericRef = new WeakReference<Person>(target2)
            };

            // Even though the referenced targets differ, WeakReference members are ignored
            Assert.True(DeepComparer.CompareProperties(a, b));
        }
    }
}
