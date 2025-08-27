# DeepComparer_xUnit

This project contains the automated test suite for the **DeepComparer** library using the [xUnit](https://xunit.net/) framework.

---

## Running Tests

### Command Line
From the repository root:

```bash
dotnet test
```

This will build the library and run all unit tests.

## Visual Studio / Rider

Open the solution in Visual Studio or JetBrains Rider.

Use the Test Explorer to run tests individually or in bulk.

## Test Coverage

### Core Comparison Logic

- Equality checks across simple and complex types.
- Handling of collections (including ReactiveUI types like SourceList and SourceCache).

### Custom Simple-Type Predicates

- Verify user-defined types can be treated as simple.

### Circular Reference Prevention

Ensures deep object graphs do not cause infinite recursion.

### Performance & Edge Cases

- Deeply nested objects.
- Objects with ignored properties or private members.

## Test Structure

- CustomSimpleTypePredicateTests.cs – Tests for treating custom types as simple.
- DeepComparisonTests.cs – Core functional tests for object comparison.
- PerformanceTests.cs – Optional baseline checks for performance characteristics.

## Notes

- Tests target .NET 9 to match the library.

- Benchmarking is separated into the Benchmarks project.

- Ensure the library builds in Release mode for performance-oriented tests.

