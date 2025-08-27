# DeepComparer

[![Build Status](https://github.com/YourGitHubUser/DeepComparer/actions/workflows/ci.yml/badge.svg)](https://github.com/YourGitHubUser/DeepComparer/actions/workflows/ci.yml)
[![NuGet Version](https://img.shields.io/nuget/v/DeepComparer.svg)](https://www.nuget.org/packages/DeepComparer)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

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
## Documentation

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

