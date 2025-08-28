# DeepComparer

[![Build Status](https://github.com/LetMeInside/DeepComparer/actions/workflows/ci.yml/badge.svg)](https://github.com/LetMeInside/DeepComparer/actions/workflows/ci.yml)
[![NuGet Version](https://img.shields.io/nuget/v/DeepComparer.svg)](https://www.nuget.org/packages/DeepComparer)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

<p align="center">
  <img src="src/DeepComparer/assets/logo.png" alt="DeepComparer Logo" width="200"/>
</p>

DeepComparer is a **high-performance object comparison library** for .NET 9, built for scenarios where:
- Reflection-based deep comparison is needed.
- Complex objects, collections, and WPF types must be supported.
- Fine-grained control over comparison behavior is required.

---

## Features
- Recursive comparison of objects, including nested properties.
- Configurable handling of:
  - Public vs private members
  - Properties vs fields
  - JSON-ignored properties
- Collection comparison by item count and unordered deep equality.
- Support for **ReactiveUI SourceList** and **SourceCache**.
- Thread-safe and performance-optimized with static inline caching.
- Circular reference detection to prevent infinite recursion.
- Custom simple-type predicate for specialized comparison rules.
- Optional comparison reports with detailed difference information.

---

## Quick Start
```csharp
using DeepComparerNS;

// A tiny POCO for the examples
public sealed class Person
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
}

var obj1 = new Person { Name = "Alice", Age = 30 };
var obj2 = new Person { Name = "Alice", Age = 31 };

// Basic equality check
var areEqual = DeepComparer.CompareProperties(obj1, obj2);

// Detailed comparison with difference report
var result = DeepComparer.ComparePropertiesWithReport(obj1, obj2);
if (!result.AreEqual)
{
    foreach (var diff in result.Differences)
        Console.WriteLine(diff);
}
```

## Additional Comparison Configuration

```csharp
// ===============================
// Basic usage samples for DeepComparer
// ===============================

using DeepComparerNS; // the root namespace of DeepComparer

// A tiny POCO for the examples
public sealed class Person
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
}

public static class DeepComparerReadmeSamples
{
    public static void Run()
    {
        var obj1 = new Person { Name = "Alice", Age = 30 };
        var obj2 = new Person { Name = "Alice", Age = 31 };

        // --------------------------------------------------------------------
        // 1) Using Boolean Toggles
        //    Signature: CompareProperties<T>(T obj1, T obj2, bool publicOnly = true, bool propertiesOnly = true, bool jsonIgnore = true)
        //
        //    - publicOnly:
        //        true  → only public members are compared
        //        false → public + non-public (protected/private) are compared
        //    - propertiesOnly:
        //        true  → only properties are compared (fields ignored)
        //        false → properties and fields are compared
        //    - jsonIgnore:
        //        true  → members marked with [Newtonsoft.Json.JsonIgnore] are ignored
        //        false → members marked with [JsonIgnore] are included in comparison
        // --------------------------------------------------------------------
        bool areEqualWithToggles = DeepComparer.CompareProperties(
            obj1,
            obj2,
            publicOnly: true,
            propertiesOnly: false,
            jsonIgnore: false
        );

        // --------------------------------------------------------------------
        // 2) Using CompareOptions (recommended for clarity/extensibility)
        //
        //    Defaults (as configured in your library):
        //      - MaxDepth = 20
        //      - OnMaxDepthReached = DepthBehavior.TreatAsDifferent
        //
        //    OnMaxDepthReached behavior:
        //      - DepthBehavior.TreatAsEqual      → stop descending; consider that branch equal
        //      - DepthBehavior.TreatAsDifferent  → stop descending; flag that branch as different (default)
        //      - DepthBehavior.LogDifference     → stop descending; log a warning to debug output
        //
        //    CustomSimpleTypePredicate:
        //      - A Func<Type,bool> to mark additional types as “simple” (compared directly),
        //        e.g., t => t == typeof(MyValueObject)
        // --------------------------------------------------------------------
        var options = new CompareOptions
        {
            MaxDepth = 20,
            OnMaxDepthReached = DepthBehavior.TreatAsDifferent, // matches your default
            CustomSimpleTypePredicate = null
        };

        bool areEqualWithOptions = CompareProperties(
            obj1,
            obj2,
            publicOnly: true,
            propertiesOnly: false,
            jsonIgnore: false,
            options: options
        );

        // (Optional) If you want a detailed report instead of just a bool:
        // var report = DeepComparer.ComparePropertiesWithReport(obj1, obj2, true, false, false, options);
        // if (!report.AreEqual) { foreach (var d in report.Differences) System.Diagnostics.Debug.WriteLine(d); }
    }
}

```
### Boolean Toggle Reference

| Parameter       | Type  | Default | Description |
|-----------------|-------|---------|-------------|
| `publicOnly`    | bool  | `true`  | `true` → Only public members are compared. `false` → Public + non-public (protected/private) are compared. |
| `propertiesOnly`| bool  | `true`  | `true` → Only properties are compared (fields ignored). `false` → Properties and fields are compared. |
| `jsonIgnore`    | bool  | `true`  | `true` → Members marked with `[JsonIgnore]` are ignored. `false` → They are included in comparison. |

### CompareOptions Reference

| Option                     | Type                                   | Default Value                   | Description |
|---------------------------|----------------------------------------|---------------------------------|-------------|
| `MaxDepth`                | int                                    | `20`                            | Maximum recursion depth for nested objects. |
| `DepthBehavior`           | `DepthBehavior` enum                  | `DepthBehavior.Ignore`          | Behavior when maximum depth is reached: <br> - `Ignore`: Skips further nested comparisons.<br> - `TreatAsDifferent`: Marks remaining properties as different and logs a warning.<br> - `LogDifference`: Skips comparison but logs a warning. |
| `CustomSimpleTypePredicate` | `Func<Type, bool>?`                   | `null`                          | Allows specifying a custom function to determine what is considered a "simple type". |


## Documentation

See the full [documentation index](docs/index.md) for all guides and references.

- Workflow Diagram
- Benchmark Results
- ReactiveUI Examples
- Roslyn Analyzer Integration
- Versioning Guide
- Release Checklist
- Contributing Guidelines


## Installation

```powershell
dotnet add package DeepComparer
```

## License

MIT License. See LICENSE

