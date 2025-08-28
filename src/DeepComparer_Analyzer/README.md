# DeepComparer Roslyn Analyzer

This project will provide compile-time diagnostics to help developers use the **DeepComparer** library correctly and efficiently.

---

## Goals
- Identify common misuse patterns at compile time.
- Suggest best practices for performance and reliability.
- Provide automated code fixes where appropriate.

---

## Planned Features
- **Missing CompareOptions Warning**  
  Warn when `DeepComparer.Compare` is called without a `CompareOptions` instance.

- **Overuse of MaxDepth**  
  Highlight cases where extremely high `MaxDepth` values may impact performance.

- **Ignored Collections**  
  Warn when collection types that cannot be compared are passed without handling.

---

## Current Status
- Initial setup with a **basic diagnostic** that checks if `Compare` is called without `CompareOptions`.

---

## Roadmap
1. Implement first diagnostic and code fix.
2. Expand rules for performance and safety checks.
3. Publish as a NuGet package for team integration.

---

## Running the Analyzer
- Open the solution in Visual Studio or Rider.
- Build the Analyzer project.
- Reference it as an analyzer in other projects or use `dotnet pack` to create a NuGet package.

---

## License
This analyzer is released under the MIT License.
