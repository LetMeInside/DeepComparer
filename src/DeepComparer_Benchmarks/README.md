# DeepComparer Benchmarks

This project contains performance benchmarks for the **DeepComparer** library using [BenchmarkDotNet](https://benchmarkdotnet.org/).

---

## Running Benchmarks

1. **Navigate to the Benchmarks directory:**

```bash
   cd Benchmarks
```

2. Run in Release mode (required for accurate results):

```bash
   dotnet run -c Release
```

4. Optional: Filter specific benchmarks

```bash
   dotnet run -c Release -- --filter *Compare_Objects*
```

# BenchmarkDotNet Output

## BenchmarkDotNet will produce:

- Detailed performance metrics (time, memory allocations)

- Reports in BenchmarkDotNet.Artifacts/results/

## Example Results

(Sample output for illustrative purposes)

| Method	                | Mean	   | Error	   | StdDev	     | Allocated
|---------------------------|---------:|----------:|------------:|------------:|
| Compare_Objects	        | 1.245 us | 0.010 us  | 0.009 us	 | 2.34 KB

## Adding New Benchmarks

- Create a new class in Benchmarks/ ending with Benchmarks.cs.

- Annotate with [MemoryDiagnoser] to capture memory usage.

- Add [Benchmark] methods for each scenario.

- Re-run benchmarks.

## Notes

- Benchmarks should be run on a clean build.

- Run multiple times to verify consistent results.

- Avoid running on battery power to reduce CPU throttling effects.

