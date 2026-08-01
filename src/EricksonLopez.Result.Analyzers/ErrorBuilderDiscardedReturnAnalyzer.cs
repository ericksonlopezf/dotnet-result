using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Roslyn diagnostic analyzer that warns when the return value of
/// <c>ErrorBuilder.With*()</c> methods is discarded.
/// </summary>
/// <remarks>
/// <para>
/// Rule RESULT003: "Return value of ErrorBuilder.{MethodName}() is discarded. ErrorBuilder is a struct;
/// the mutated copy is lost. Assign the result or chain the call."
/// </para>
/// <para>
/// <c>ErrorBuilder</c> is a <c>readonly struct</c> with copy-on-write semantics. Its <c>With*()</c>
/// methods return a <em>new copy</em> with the requested change applied; they do NOT mutate the
/// original builder. When the return value is discarded (e.g., standalone statement), the new copy
/// (with the change) is silently lost, leading to bugs:
/// <code>
/// var builder = Error.Create("code", "desc");
/// builder.WithType(ErrorType.Domain);  // ⚠ return value discarded — mutation lost
/// var error = builder.Build();          // type is still Failure, not Domain
/// </code>
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ErrorBuilderDiscardedReturnAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "RESULT003";

    private const string ErrorBuilderFullName = "EricksonLopez.Result.ErrorBuilder";

    // Method names on ErrorBuilder that return ErrorBuilder and should not have their return value discarded
    private static readonly ImmutableHashSet<string> TrackedMethods = ImmutableHashSet.Create(
        "WithType",
        "WithSeverity",
        "WithRetryability",
        "WithDescriptionKey",
        "WithTraceId",
        "WithCorrelationId",
        "WithMetadata",
        "WithInnerError",
        "WithInnerErrors");

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "ErrorBuilder method return value is discarded",
        messageFormat: "Return value of ErrorBuilder.{0}() is discarded, the new copy with the change is lost; assign the result or chain the call",
        category: "Usage",
        // Discarding the return value of an ErrorBuilder With*() method is ALWAYS a bug:
        // ErrorBuilder is a readonly struct and With*() methods return a new copy — the original
        // is not mutated. The discarded copy means the change is silently lost. This must be
        // an Error (not a Warning) to prevent silent data loss bugs from slipping through code review.
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ErrorBuilder is a readonly struct with copy-on-write semantics. Its With*() methods return a new copy with the requested change; they do NOT mutate the original. If the return value is not captured, the change is silently lost. Either reassign the result (builder = builder.WithType(...)) or chain calls fluently (Error.Create(...).WithType(...).Build()).",
        helpLinkUri: "https://github.com/ericksonlopez/dotnet-result/blob/main/docs/error-builder.md");

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

        // Check if the method is on ErrorBuilder
        var method = invocation.TargetMethod;
        if (method.ContainingType?.ToDisplayString() != ErrorBuilderFullName) return;

        // Check if it's one of the tracked With* methods
        if (!TrackedMethods.Contains(method.Name)) return;

        // Check if the return value is discarded
        bool isDiscarded = false;

        if (invocation.Parent is IExpressionStatementOperation)
        {
            isDiscarded = true;
        }
        else if (invocation.Parent is IAssignmentOperation assignment && assignment.Target is IDiscardOperation)
        {
            isDiscarded = true;
        }
        else if (invocation.Parent is IVariableInitializerOperation initializer && 
                 initializer.Parent is IVariableDeclaratorOperation declarator && 
                 declarator.Symbol.Name == "_")
        {
            isDiscarded = true;
        }

        if (isDiscarded)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                invocation.Syntax.GetLocation(),
                method.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
