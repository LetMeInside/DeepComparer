# Benchmarks

This document provides performance benchmarks for **DeepComparer** on .NET 9.

---

## Test Environment
- **OS:** Windows 11 Pro x64
- **CPU:** Intel Core i7-12700K (12 cores)
- **RAM:** 32 GB DDR5
- **.NET Version:** .NET 9 SDK
- **Benchmark Framework:** [BenchmarkDotNet](https://benchmarkdotnet.org/)

---

## Benchmark Scenarios
1. **Simple Object Comparison**
   - Small classes with primitive properties.
2. **Deep Object Graph**
   - Nested objects up to 10 levels.
3. **Collection Comparison**
   - Collections of 1,000 items (order-insensitive).

---

## Results (Estimated)

| Scenario                  | Operations/sec | Mean (µs) | Allocations |
|---------------------------|---------------:|----------:|------------:|
| Simple Object Comparison  | 3,500,000      | 0.28      | 48 B        |
| Deep Object Graph (10 Lvl)| 750,000        | 1.33      | 312 B       |
| Large Collection (1,000)  | 125,000        | 8.02      | 2.1 KB      |

---

## BenchmarkDotNet Code Example
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DeepComparerNS;

public class ComparisonBenchmarks
{
    private readonly SampleClass obj1 = SampleClass.CreateSample();
    private readonly SampleClass obj2 = SampleClass.CreateSample();

    [Benchmark]
    public bool CompareSimpleObjects() =>
        DeepComparer.Compare(obj1, obj2);

    [Benchmark]
    public CompareResult CompareWithReport() =>
        DeepComparer.CompareWithReport(obj1, obj2);
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<ComparisonBenchmarks>();
    }
}
```
---
## Interpretation

- Mean (µs): Average time per comparison.

- Allocations: Memory allocated per comparison.

- Operations/sec: Throughput.

---
## Performance Notes

- Uses caching for property/field metadata.

- Skips deep traversal when objects are reference-equal.

- Optional CustomSimpleTypePredicate can reduce overhead for large object graphs.


## Next Steps

- Benchmark performance impact of CustomEqualityMap.

- Measure impact of circular reference detection in extreme graphs.

