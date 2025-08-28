
using DeepComparer_xUnit.Models;
using Xunit;
using DeepComparerNS;

namespace DeepComparer_xUnit.Tests
{
    public class JsonIgnoreTests
    {
        [Fact]
        public void JsonIgnored_Property_IsSkipped_When_Enabled()
        {
            var a = new WithJsonIgnore { Secret = "A", Visible = "V" };
            var b = new WithJsonIgnore { Secret = "B", Visible = "V" };

            // jsonIgnore = true (default): Secret difference ignored
            Assert.True(DeepComparer.Compare(a, b));
        }

        [Fact]
        public void JsonIgnored_Property_IsCompared_When_Disabled()
        {
            var a = new WithJsonIgnore { Secret = "A", Visible = "V" };
            var b = new WithJsonIgnore { Secret = "B", Visible = "V" };

            // jsonIgnore = false: Secret is compared -> not equal
            var report = DeepComparer.CompareWithReport(a, b, jsonIgnore: false);
            Assert.False(report.AreEqual);
            Assert.Contains(report.Differences, d => d.Contains("Secret"));
        }
    }
}
