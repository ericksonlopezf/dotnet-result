using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Roslyn diagnostic analyzer that warns when <c>ErrorBuilder.WithInnerError(Error)</c>
/// is chained 2 or more times consecutively, causing O(n²) ImmutableArray copying.
/// </summary>
/// <remarks>
/// <para>
/// Rule RESULT006: "Chained ErrorBuilder.WithInnerError() calls are O(n²).
/// Use WithInnerErrors(IEnumerable&lt;Error&gt;) to add multiple inner errors efficiently."
/// </para>
/// <para>
/// <c>ErrorBuilder</c> is a <c>readonly struct</c> with copy-on-write semantics.
/// Each call to <c>WithInnerError(Error)</c> creates a new <c>ImmutableArray&lt;Error&gt;</c>
/// with one additional element (O(n) copy per call). Chaining N calls is O(n²) total.
/// </para>
/// <para>
/// Example — O(n²) (warned when 2+ chained calls):
/// <code>
/// var builder = Error.Create("code", "desc")
///     .WithInnerError(e1)
///     .WithInnerError(e2)
///     .WithInnerError(e3); // 3 ImmutableArray copies, each larger than the previous
/// </code>
/// Recommended — O(n):
/// <code>
/// var builder = Error.Create("code", "desc")
///     .WithInnerErrors(new[] { e1, e2, e3 }); // single ImmutableArray creation
/// </code>
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InnerErrorChainingAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "RESULT006";

    private const string ErrorBuilderFullName = "EricksonLopez.Result.ErrorBuilder";
    private const string WithInnerErrorMethodName = "WithInnerError";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Chained ErrorBuilder.WithInnerError() calls are O(n²)",
        messageFormat: "{0} chained ErrorBuilder.WithInnerError() calls cause O(n\u00b2) ImmutableArray copying; use WithInnerErrors(IEnumerable<Error>) to add all inner errors in a single O(n) operation",
        category: "Performance",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
            "ErrorBuilder is a readonly struct with copy-on-write semantics. Each WithInnerError(Error) call " +
            "creates a new ImmutableArray<Error> with one additional element — an O(n) copy per call. " +
            "Chaining N calls produces O(n²) total copying. When adding 2 or more inner errors, use " +
            "WithInnerErrors(IEnumerable<Error>) to create the ImmutableArray once in O(n).",
        helpLinkUri: "https://github.com/ericksonlopez/dotnet-result/blob/main/docs/error-builder.md#inner-errors");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

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

        if (!string.Equals(method.Name, WithInnerErrorMethodName, System.StringComparison.Ordinal)) return;
        if (method.Parameters.Length != 1) return;
        if (method.ContainingType?.ToDisplayString() != ErrorBuilderFullName) return;

        // Only report on the outermost call in a chain — inner calls are silently skipped
        if (IsInnerCallInChain(invocation)) return;

        int chainLength = CountChainLength(invocation);
        if (chainLength < 2) return;

        var diagnostic = Diagnostic.Create(
            Rule,
            invocation.Syntax.GetLocation(),
            chainLength);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsInnerCallInChain(IInvocationOperation invocation)
    {
        var parent = invocation.Parent;
        return parent is IInvocationOperation parentInvocation
            && string.Equals(parentInvocation.TargetMethod.Name, WithInnerErrorMethodName, System.StringComparison.Ordinal)
            && parentInvocation.TargetMethod.Parameters.Length == 1
            && parentInvocation.TargetMethod.ContainingType?.ToDisplayString() == ErrorBuilderFullName;
    }

    private static int CountChainLength(IInvocationOperation outermost)
    {
        int count = 1;
        var current = outermost;

        while (true)
        {
            var receiver = current.Instance;
            if (receiver is IInvocationOperation receiverInvocation
                && string.Equals(receiverInvocation.TargetMethod.Name, WithInnerErrorMethodName, System.StringComparison.Ordinal)
                && receiverInvocation.TargetMethod.Parameters.Length == 1
                && receiverInvocation.TargetMethod.ContainingType?.ToDisplayString() == ErrorBuilderFullName)
            {
                count++;
                current = receiverInvocation;
            }
            else
            {
                break;
            }
        }

        return count;
    }
}
