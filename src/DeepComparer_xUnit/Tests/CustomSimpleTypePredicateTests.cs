using DeepComparerNS;
using System;
using Xunit;


namespace DeepComparer_xUnit.Tests
{
    public class CustomSimpleTypePredicateTests
    {
        /* before:
        private class ComplexType
        {
            public string SerializedData { get; set; } = string.Empty;
        }
        after: */
        public class ComplexType
        {
            public string SerializedData { get; set; } = string.Empty;

            public override bool Equals(object? obj) =>
                obj is ComplexType other && SerializedData == other.SerializedData;

            public override int GetHashCode() => SerializedData?.GetHashCode() ?? 0;
        }

        [Fact]
        public void CustomPredicate_TreatsComplexTypeAsSimple_EqualValues()
        {
            var obj1 = new ComplexType { SerializedData = "ABC123" };
            var obj2 = new ComplexType { SerializedData = "ABC123" };

            var options = new CompareOptions
            {
                MaxDepth = 20,
                OnMaxDepthReached = DepthBehavior.TreatAsDifferent,
                CustomSimpleTypePredicate = t => t == typeof(ComplexType)
            };

            var result = DeepComparer.ComparePropertiesWithReport(obj1, obj2,true, true, true, options);
            Assert.True(result.AreEqual);
        }

        [Fact]
        public void CustomPredicate_TreatsComplexTypeAsSimple_DifferentValues()
        {
            var obj1 = new ComplexType { SerializedData = "ABC123" };
            var obj2 = new ComplexType { SerializedData = "XYZ789" };

            //var ctx = new DeepComparer.CompareContext(
            //    publicOnly: true,
            //    propertiesOnly: true,
            //    jsonIgnore: true
            //)
            //{
            //    CustomSimpleTypePredicate = t => t == typeof(ComplexType)
            //};
            var options = new CompareOptions()
            {
                MaxDepth = 20,
                OnMaxDepthReached = DepthBehavior.TreatAsDifferent,
                CustomSimpleTypePredicate = t => t == typeof(ComplexType)
            };
            var result = DeepComparer.ComparePropertiesWithReport(obj1, obj2, true, true,true, options);
            Assert.False(result.AreEqual);
            Assert.Contains(result.Differences, d => d.Contains("Values differ"));
        }
    }
}
