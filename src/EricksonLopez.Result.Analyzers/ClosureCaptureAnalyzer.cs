// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using EricksonLopez.Result;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Roslyn diagnostic analyzer that warns when lambda arguments to
/// <c>Result.Map()</c>, <c>Result.Bind()</c>, <c>Result.TapOnSuccess()</c>,
/// <c>Result.TapOnFailure()</c>, <c>Result.Ensure()</c>, <c>Result.MapError()</c>,
/// <c>Result.MapFailure()</c>, <c>Result.Execute()</c>, or <c>Result.Inspect()</c> capture
/// local variables or <c>this</c> from the enclosing method, causing a heap-allocated closure.
/// </summary>
/// <remarks>
/// <para>
/// Rule RESULT004: "Lambda in {MethodName}() captures {CaptureCount} local variable(s)
/// ({VariableNames}): use the {MethodName}(TState, ...) overload to avoid the allocation."
/// </para>
/// <para>
/// <c>Result</c>/<c>Result&lt;T&gt;</c> provides <c>TState</c> overloads for all monadic methods
/// (Map, Bind, TapOnSuccess, TapOnFailure, Ensure, MapError, Inspect) that allow the caller to
/// pass captured state as a parameter instead of creating a heap-allocated closure delegate.
/// When a lambda in a hot path captures local variables or <c>this</c>, this allocates a new
/// closure object on every call.
/// </para>
/// <para>
/// Example — closure allocation (warned):
/// <code>
/// var userId = GetUserId();
/// return result.Map(x => new Dto(userId, x)); // allocates closure capturing userId
///
/// // Also warned — capturing `this`:
/// return result.Map(x => this.ProcessOrder(x)); // allocates closure capturing this
/// </code>
/// Recommended — zero closure allocation:
/// <code>
/// var userId = GetUserId();
/// return result.Map(userId, (uid, x) => new Dto(uid, x)); // no closure
/// </code>
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ClosureCaptureAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic identifier for this analyzer rule.</summary>
    public const string DiagnosticId = "RESULT004";

    // Methods on Result / Result<T> that have TState overloads and should be checked.
    // Only methods that actually exist in the API are listed here.
    private static readonly ImmutableHashSet<string> TrackedMethods = ImmutableHashSet.Create(
        "Map",
        "Bind",
        "TapOnSuccess",
        "TapOnFailure",
        "Ensure",
        "MapError",
        "Inspect",
        "Match",      // Result<T>.Match<TOut>(TState, Func<TState,TValue,TOut>, Func<TState,Error,TOut>) overload exists
        "Execute",    // Result<T>.Execute(TState, Action<TState,TValue>, Action<TState,Error>) overload exists
        "MapFailure", // Result<T>.MapFailure<TState,TOut>(TState, Func<TState,Error,TOut>, TOut successDefault) overload exists
        "Recover");   // Result<T>.Recover(TState, Func<TState,Error,Result<T>>) overload exists

    private const string ResultFullName = "EricksonLopez.Result.Result";
    private const string ResultOfTFullName = "EricksonLopez.Result.Result<T>";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Lambda captures locals — use the TState overload to avoid closure allocation",
        messageFormat: "Lambda in {0}() captures {1} local variable(s) ({2}); use the {0}(TState, ...) overload to avoid the allocation",
        category: "Performance",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
            "Result<T> methods (Map, Bind, TapOnSuccess, TapOnFailure, Ensure, MapError, Inspect) provide " +
            "TState overloads that accept an additional state parameter, eliminating the need for a closure " +
            "delegate. When a lambda captures local variables or 'this' from the enclosing scope, the JIT " +
            "allocates a closure object on every invocation. Pass captured values as the TState parameter " +
            "instead. Example: change 'result.Map(x => Process(id, x))' to 'result.Map(id, (i, x) => Process(i, x))'. " +
            "For 'this' captures, pass the relevant member or 'this' as state: 'result.Map(this, (self, x) => self.Process(x))'.",
        helpLinkUri: "https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/performance.md#closure-free-pipelines");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;

        // Only check the tracked method names
        if (!TrackedMethods.Contains(method.Name)) return;

        var containingType = method.ContainingType;
        if (containingType.ContainingNamespace.ToDisplayString() != "EricksonLopez.Result") return;

        var metadataName = containingType.OriginalDefinition.MetadataName;
        if (metadataName != "Result" && metadataName != "Result`1") return;

        // Only report if the method is NOT already using the TState overload
        if (IsAlreadyUsingStateOverload(method)) return;

        // Find lambda arguments that capture local variables or 'this' from the enclosing scope
        var allCapturedNames = new List<string>();
        foreach (var arg in invocation.Arguments)
        {
            var capturedNames = GetCapturedNames(arg.Value, context.CancellationToken);
            allCapturedNames.AddRange(capturedNames);
        }

        if (allCapturedNames.Count == 0) return;

        // Report diagnostic at the invocation expression span
        var diagnostic = Diagnostic.Create(
            Rule,
            invocation.Syntax.GetLocation(),
            method.Name,
            allCapturedNames.Count,
            string.Join(", ", allCapturedNames));

        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Returns the distinct names of local variables and 'this' captured by a lambda or anonymous
    /// method expression. Returns an empty list for non-lambda operations.
    /// </summary>
    internal static List<string> GetCapturedNames(IOperation operation, CancellationToken ct)
    {
        var result = new List<string>();
        var syntax = operation.Syntax;
        if (syntax is not (LambdaExpressionSyntax or AnonymousMethodExpressionSyntax))
            return result;

        // Static lambdas (static x => ...) cannot capture locals by language guarantee.
        // The compiler enforces this, so there is no need to scan for captures.
        if (syntax is LambdaExpressionSyntax lambdaSyntaxCheck
            && lambdaSyntaxCheck.Modifiers.Any(SyntaxKind.StaticKeyword))
            return result;

        var semanticModel = operation.SemanticModel!;

        // Lambda's own parameters — these are NOT captures from the enclosing scope
        var lambdaParameters = GetLambdaParameters(syntax);
        // Use (Name, SymbolKind) as deduplication key to correctly handle name-shadowing:
        // two variables with the same name in different scopes (outer method param and inner
        // loop variable) are distinct captures and should be counted separately.
        var capturedLocals = new HashSet<(string Name, SymbolKind Kind)>();

        foreach (var identifier in syntax.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            var symbol = semanticModel.GetSymbolInfo(identifier, ct).Symbol;
            if (symbol is (ILocalSymbol or IParameterSymbol) && !lambdaParameters.Contains(symbol.Name))
            {
                capturedLocals.Add((symbol.Name, symbol.Kind));
            }
        }

        // Additionally, detect captures of 'this' via ThisExpressionSyntax nodes.
        // 'this' is not an ILocalSymbol or IParameterSymbol — it is represented as a
        // ThisExpressionSyntax in the syntax tree. Additionally, implicit 'this' accesses
        // (accessing instance fields/properties/methods without an explicit receiver) also
        // create a closure that captures 'this'. We detect both patterns here.
        bool capturesThis = syntax.DescendantNodes().Any(node =>
            node is ThisExpressionSyntax ||
            (node is IdentifierNameSyntax identifierNode
             && !(node.Parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Expression != node)
             && semanticModel.GetSymbolInfo(identifierNode, ct).Symbol is { IsStatic: false, ContainingType: not null } symbol
             && symbol is (IFieldSymbol or IPropertySymbol or IEventSymbol or IMethodSymbol)));

        // Collect all capture names in a stable, deduplicated order
        foreach (var (name, _) in capturedLocals)
        {
            result.Add(name);
        }

        if (capturesThis)
        {
            result.Add("this");
        }

        return result;
    }

    private static HashSet<string> GetLambdaParameters(SyntaxNode lambdaSyntax)
    {
        var result = new HashSet<string>(System.StringComparer.Ordinal);
        if (lambdaSyntax is SimpleLambdaExpressionSyntax simpleLambda)
        {
            result.Add(simpleLambda.Parameter.Identifier.Text);
        }
        else if (lambdaSyntax is ParenthesizedLambdaExpressionSyntax parenLambda)
        {
            foreach (var param in parenLambda.ParameterList.Parameters)
                result.Add(param.Identifier.Text);
        }
        else if (lambdaSyntax is AnonymousMethodExpressionSyntax anonMethod)
        {
            if (anonMethod.ParameterList is not null)
            {
                foreach (var param in anonMethod.ParameterList.Parameters)
                    result.Add(param.Identifier.Text);
            }
        }
        return result;
    }

    /// <summary>
    /// Checks whether the method is already the TState overload by verifying that the first
    /// parameter is a non-delegate, non-primitive generic-typed parameter (i.e., TState).
    /// </summary>
    private static bool IsAlreadyUsingStateOverload(IMethodSymbol method)
        => method.OriginalDefinition.Parameters.Length >= 2 && method.OriginalDefinition.Parameters[0].Type.TypeKind == TypeKind.TypeParameter;
}




