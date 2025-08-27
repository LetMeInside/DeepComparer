# Roslyn Analyzer for DeepComparer

This guide explains how to create a **Roslyn Analyzer** that enforces best practices for using `DeepComparer` in your .NET 9 projects.

---

## Purpose
- Warn developers when `DeepComparer.CompareProperties` or `ComparePropertiesWithReport` is called **without explicitly passing CompareOptions**.
- Suggests using `CompareOptions` for:
  - Custom simple-type predicates
  - Max depth settings
  - Circular reference handling

---

## Analyzer Implementation

### 1. Create Analyzer Project
```bash
dotnet new analyzer -n DeepComparerAnalyzer
```

2. Sample Analyzer Code

```csharp
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DeepComparerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CompareWithoutOptionsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DC0001";
        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "DeepComparer Compare called without CompareOptions",
            "Consider passing a CompareOptions object for better configurability",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            if (symbol == null)
                return;

            if (symbol.ContainingType.Name == "DeepComparer" &&
                symbol.Name.StartsWith("CompareProperties") &&
                !symbol.Parameters.Any(p => p.Type.Name == "CompareOptions"))
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
```

---

## Usage

1. Build the analyzer project.

2. Reference the output .dll in your main project:

```xml
<ItemGroup>
    <Analyzer Include="..\DeepComparerAnalyzer\bin\Debug\netstandard2.0\DeepComparerAnalyzer.dll" />
</ItemGroup>
```

3. Rebuild your solution.

4. Calls like:

```csharp
DeepComparer.CompareProperties(obj1, obj2);
```
will trigger a DC0001 warning.

---

## Future Extensions

Analyzer to detect:

- Excessive object depth (suggest MaxDepth override).
  - Missing circular reference detection in risky object graphs.
  - ReactiveUI collections missing a custom predicate.

## References

- Roslyn Analyzer Tutorial:
https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix

- Microsoft.CodeAnalysis NuGet:
https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix

