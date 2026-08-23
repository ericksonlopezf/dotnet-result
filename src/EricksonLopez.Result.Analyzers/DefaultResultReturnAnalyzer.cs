// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Roslyn diagnostic analyzer (RESULT012) that warns when <c>default</c> or <c>default(Result)</c> is returned
/// from a method returning <see cref="Result"/> or <c>Result&lt;T&gt;</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DefaultResultReturnAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic identifier for this analyzer rule.</summary>
    public const string DiagnosticId = "RESULT012";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Avoid returning default(Result) or default(Result<T>)",
        messageFormat: "Returning default produces an uninitialized Result state. Return Result.Success(...) or Result.Failure(...) instead.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "An uninitialized Result struct evaluates to false and will throw InvalidOperationException when accessed by monadic operators. Always return an explicit Result.Success() or Result.Failure().",
        helpLinkUri: "https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md#RESULT012");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeReturn, OperationKind.Return);
    }

    private static void AnalyzeReturn(OperationAnalysisContext context)
    {
        var returnOperation = (IReturnOperation)context.Operation;
        var unwrapped = returnOperation.ReturnedValue;

        while (unwrapped is IConversionOperation conversion)
        {
            unwrapped = conversion.Operand;
        }

        if (unwrapped is not IDefaultValueOperation)
            return;

        var type = returnOperation.ReturnedValue!.Type;
        if (IsResultType(type!))
        {
            var diagnostic = Diagnostic.Create(Rule, returnOperation.Syntax.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsResultType(ITypeSymbol type)
    {
        var ns = type.ContainingNamespace!.ToDisplayString();
        if (!ns.EndsWith("Result", StringComparison.Ordinal))
        {
            return false;
        }

        return string.Equals(type.Name, "Result", StringComparison.Ordinal);
    }
}
