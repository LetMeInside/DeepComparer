using BenchmarkDotNet.Attributes;
using DeepComparerNS;
using System.Collections.Generic;

namespace DeepComparer.Benchmarks
{
    [MemoryDiagnoser]
    public class DeepComparerBenchmarks
    {
        private SampleData _obj1;
        private SampleData _obj2;

        [GlobalSetup]
        public void Setup()
        {
            _obj1 = new SampleData
            {
                Id = 1,
                Name = "Benchmark Test",
                Values = new List<int> { 1, 2, 3, 4, 5 }
            };

            _obj2 = new SampleData
            {
                Id = 1,
                Name = "Benchmark Test",
                Values = new List<int> { 1, 2, 3, 4, 5 }
            };
        }

        [Benchmark]
        public bool Compare_Objects()
        {
            return DeepComparerNS.DeepComparer.CompareProperties(_obj1, _obj2);
        }

        public class SampleData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<int> Values { get; set; }
        }
    }
}
