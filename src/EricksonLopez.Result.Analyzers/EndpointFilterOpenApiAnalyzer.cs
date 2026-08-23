// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using EricksonLopez.Result;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Roslyn diagnostic analyzer that warns when <c>AddResultEndpointFilter()</c> is called
/// without a corresponding <c>.Produces&lt;T&gt;()</c> call in the fluent chain.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EndpointFilterOpenApiAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic identifier for this analyzer rule.</summary>
    public const string DiagnosticId = "RESULT008";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "ResultEndpointFilter hides OpenAPI metadata without explicit Produces<T>()",
        messageFormat: "The automatic ResultEndpointFilter returns an untyped object to OpenAPI. Call .Produces<T>() or .ProducesProblem() on this endpoint, or use .ToHttpResult<T>() directly in the handler instead.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ResultEndpointFilter returns typed data at runtime but returns object? for its API Explorer schema. You must explicitly declare your types using Produces<T> to prevent schema degradation.",
        helpLinkUri: "https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md#RESULT008");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;

        if (!string.Equals(method.Name, "AddResultEndpointFilter", StringComparison.Ordinal))
            return;

        // Traverse UP the fluent chain (extension methods wrap the previous call in IArgumentOperation)
        var currentParent = context.Operation.Parent;
        while (currentParent is IInvocationOperation or IArgumentOperation)
        {
            if (currentParent is IInvocationOperation parentInvocation)
            {
                if (parentInvocation.TargetMethod.Name.StartsWith("Produces", StringComparison.Ordinal))
                {
                    return; // Found Produces up the chain
                }
            }

            currentParent = currentParent.Parent;
        }

        // Traverse DOWN the fluent chain (if Produces was called before AddResultEndpointFilter)
        var currentChild = GetReceiver(invocation);
        while (currentChild is IInvocationOperation childInvocation)
        {
            if (childInvocation.TargetMethod.Name.StartsWith("Produces", StringComparison.Ordinal))
            {
                return; // Found Produces down the chain
            }

            currentChild = GetReceiver(childInvocation);
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
    }

    private static IOperation? GetReceiver(IInvocationOperation invocation)
    {
        if (invocation.Instance != null)
            return invocation.Instance;

        return invocation.Arguments.Length > 0 ? invocation.Arguments[0].Value : null;
    }
}

