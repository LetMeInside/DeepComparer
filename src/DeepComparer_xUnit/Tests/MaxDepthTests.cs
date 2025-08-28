
using DeepComparer_xUnit.Models;
using Xunit;
using DeepComparerNS;

namespace DeepComparer_xUnit.Tests
{
    public class MaxDepthTests
    {
        private static Node MakeChain(int length)
        {
            var head = new Node { Value = 0 };
            var cur = head;
            for (int i = 1; i < length; i++)
            {
                cur.Next = new Node { Value = i };
                cur = cur.Next;
            }
            return head;
        }

        [Fact]
        public void TreatAsEqual_When_MaxDepth_Reached()
        {
            var a = MakeChain(30);
            var b = MakeChain(30);
            // Make them diverge deep
            var cur = b;
            for (int i = 0; i < 21; i++) cur = cur!.Next!;
            cur!.Value = 999;

            var opts = new CompareOptions
            {
                MaxDepth = 20,
                OnMaxDepthReached = DepthBehavior.TreatAsEqual
            };

            // Despite a deep difference, we treat as equal after depth limit
            Assert.True(DeepComparer.CompareProperties(a, b, options: opts));

            var report = DeepComparer.ComparePropertiesWithReport(a, b, options: opts);
            Assert.True(report.AreEqual);
        }

        [Fact]
        public void TreatAsDifferent_When_MaxDepth_Reached()
        {
            var a = MakeChain(30);
            var b = MakeChain(30);
            var cur = b;
            for (int i = 0; i < 21; i++) cur = cur!.Next!;
            cur!.Value = 999;

            var opts = new CompareOptions
            {
                MaxDepth = 20,
                OnMaxDepthReached = DepthBehavior.TreatAsDifferent
            };

            Assert.False(DeepComparer.CompareProperties(a, b, options: opts));

            var report = DeepComparer.ComparePropertiesWithReport(a, b, options: opts);
            Assert.False(report.AreEqual);
            Assert.Contains(report.Differences, d => d.Contains("Depth limit"));
        }

        [Fact]
        public void LogDifference_When_MaxDepth_Reached()
        {
            var a = MakeChain(30);
            var b = MakeChain(30);
            var cur = b;
            for (int i = 0; i < 21; i++) cur = cur!.Next!;
            cur!.Value = 999;

            var opts = new CompareOptions
            {
                MaxDepth = 20,
                OnMaxDepthReached = DepthBehavior.LogDifference
            };

            // Should still be considered equal overall (depth exceeded but we only log)
            Assert.True(DeepComparer.CompareProperties(a, b, options: opts));

            var report = DeepComparer.ComparePropertiesWithReport(a, b, options: opts);
            Assert.True(report.AreEqual);
            // But we log a difference line about depth
            Assert.Contains(report.Differences, d => d.Contains("Depth limit"));
        }
    }
}
