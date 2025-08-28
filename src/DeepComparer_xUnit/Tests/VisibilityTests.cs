using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepComparer_xUnit.Models;
using Xunit;
using DeepComparerNS;

namespace DeepComparer_xUnit
{
    namespace Tests
    {
        public class VisibilityTests
        {
            [Fact]
            public void PublicOnly_Ignores_Private_Protected()
            {
                var a = new WithVisibility { PublicInt = 1 };
                var b = new WithVisibility { PublicInt = 1 };

                // diverge private/protected
                a.SetPrivate(10);
                a.SetProtected("X");
                b.SetPrivate(20);
                b.SetProtected("Y");

                // publicOnly = true (default) => still equal
                Assert.True(DeepComparer.Compare(a, b));
            }

            [Fact]
            public void IncludingNonPublic_Detects_Differences()
            {
                var a = new WithVisibility { PublicInt = 1 };
                var b = new WithVisibility { PublicInt = 1 };

                a.SetPrivate(10);
                b.SetPrivate(20);

                // publicOnly = false => should spot difference
                var report = DeepComparer.CompareWithReport(a, b, publicOnly: false);
                Assert.False(report.AreEqual);
                Assert.Contains(report.Differences, d => d.Contains("PrivateInt"));
            }
        }
    }
}
