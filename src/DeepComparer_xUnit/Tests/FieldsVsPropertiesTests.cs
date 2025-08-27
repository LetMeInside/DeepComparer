
using DeepComparer_xUnit.Models;
using Xunit;
using DeepComparerNS;

namespace DeepComparer_xUnit.Tests
{
    public class FieldsVsPropertiesTests
    {
        [Fact]
        public void PropertiesOnly_Ignores_Fields()
        {
            var a = new WithFields();
            var b = new WithFields();
            b.PublicField = 42; // different field

            // propertiesOnly = true (default) => ignore field difference
            Assert.True(DeepComparer.CompareProperties(a, b));
        }

        [Fact]
        public void IncludingFields_Detects_Field_Differences()
        {
            var a = new WithFields();
            var b = new WithFields();
            b.PublicField = 42;

            // propertiesOnly = false => compare fields too
            var eq = DeepComparer.CompareProperties(a, b, propertiesOnly: false);
            Assert.False(eq);

            var report = DeepComparer.ComparePropertiesWithReport(a, b, propertiesOnly: false);
            Assert.False(report.AreEqual);
            Assert.Contains(report.Differences, d => d.Contains("PublicField"));
        }

        [Fact]
        public void IncludingNonPublicFields_Detects_PrivateField_Differences()
        {
            var a = new WithFields();
            var b = new WithFields();
            a.SetPrivateField(1);
            b.SetPrivateField(999);

            var report = DeepComparer.ComparePropertiesWithReport(a, b, publicOnly: false, propertiesOnly: false);
            Assert.False(report.AreEqual);
            Assert.Contains(report.Differences, d => d.Contains("PrivateField"));
        }
    }
}
