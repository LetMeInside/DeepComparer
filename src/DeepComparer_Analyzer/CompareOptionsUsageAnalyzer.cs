using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DeepComparer.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CompareOptionsUsageAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DC001";
        private static readonly LocalizableString Title =
            "CompareProperties should be called with CompareOptions";
        private static readonly LocalizableString MessageFormat =
            "Call to CompareProperties does not pass a CompareOptions instance";
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not InvocationExpressionSyntax invocation)
                return;

            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
                return;

            // Check method name
            if (methodSymbol.Name != "CompareProperties")
                return;

            // Check containing type
            if (methodSymbol.ContainingType?.Name != "DeepComparer")
                return;

            // If parameter list does not include CompareOptions
            bool hasCompareOptionsParam = methodSymbol.Parameters.Any(p =>
                p.Type.Name == "CompareOptions");

            // If method does not expect CompareOptions, skip
            if (!hasCompareOptionsParam)
                return;

            // Check invocation arguments
            if (invocation.ArgumentList.Arguments.All(arg =>
                context.SemanticModel.GetTypeInfo(arg.Expression).Type?.Name != "CompareOptions"))
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
