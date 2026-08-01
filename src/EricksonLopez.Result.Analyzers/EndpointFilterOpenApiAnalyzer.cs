using System.Collections.Immutable;
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
    public const string DiagnosticId = "RESULT008";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "ResultEndpointFilter hides OpenAPI metadata without explicit Produces<T>()",
        "The automatic ResultEndpointFilter returns an untyped object to OpenAPI. Call .Produces<T>() or .ProducesProblem() on this endpoint, or use .ToHttpResult<T>() directly in the handler instead.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ResultEndpointFilter returns typed data at runtime but returns object? for its API Explorer schema. You must explicitly declare your types using Produces<T> to prevent schema degradation.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private void AnalyzeInvocation(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation invocation)
            return;

        var method = invocation.TargetMethod;

        if (method.Name != "AddResultEndpointFilter")
            return;

        // Traverse UP the fluent chain (extension methods wrap the previous call in IArgumentOperation)
        var current = context.Operation;
        while (current.Parent != null)
        {
            if (current.Parent is IInvocationOperation parentInvocation)
            {
                var parentMethod = parentInvocation.TargetMethod;
                if (parentMethod.Name.StartsWith("Produces", System.StringComparison.Ordinal))
                {
                    return; // Found Produces up the chain
                }
            }
            // In a fluent chain, the method might be an argument to the next extension method,
            // or an expression statement, etc. We just traverse up the tree.
            current = current.Parent;
            
            // If we hit a block or statement that is not part of the fluent chain expression, we can stop traversing up.
            if (current is IBlockOperation || current is IExpressionStatementOperation)
            {
                break;
            }
        }

        // Traverse DOWN the fluent chain (if Produces was called before AddResultEndpointFilter)
        // For extension methods, the previous call is usually the first argument (index 0).
        // Or if it's an instance method, it's the Instance property.
        var child = invocation.Instance ?? (invocation.Arguments.Length > 0 ? invocation.Arguments[0].Value : null);
        while (child != null)
        {
            if (child is IInvocationOperation childInvocation)
            {
                var childMethod = childInvocation.TargetMethod;
                if (childMethod.Name.StartsWith("Produces", System.StringComparison.Ordinal))
                {
                    return; // Found Produces down the chain
                }
                child = childInvocation.Instance ?? (childInvocation.Arguments.Length > 0 ? childInvocation.Arguments[0].Value : null);
            }
            else if (child is IConversionOperation conversion)
            {
                child = conversion.Operand;
            }
            else if (child is IArgumentOperation argument)
            {
                child = argument.Value;
            }
            else
            {
                break;
            }
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
    }
}
