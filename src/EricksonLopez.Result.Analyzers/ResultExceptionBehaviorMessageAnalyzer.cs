using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Roslyn diagnostic analyzer (RESULT010) that warns when <c>ex.Message</c> is used
/// inside the errorFactory delegate for <c>AddResultExceptionBehavior</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ResultExceptionBehaviorMessageAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "RESULT010";

    private const string ExtensionClassName = "ResultMediatRExtensions";
    private const string MethodName = "AddResultExceptionBehavior";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Avoid using Exception.Message in ResultExceptionBehavior",
        messageFormat: "Using Exception.Message in errorFactory may expose sensitive internal details (connection strings, paths) in production. Use a static message or sanitize the exception.",
        category: "Security",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Directly exposing Exception.Message into the Result Error description can leak PII or internal system details if returned via HTTP responses. Use a safe static description instead.",
        helpLinkUri: "https://github.com/ericksonlopez/dotnet-result/blob/main/docs/analyzers.md#RESULT010");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;

        if (method.Name != MethodName)
            return;

        if (method.ContainingType?.Name != ExtensionClassName)
            return;

        foreach (var arg in invocation.Arguments)
        {
            if (arg.Parameter?.Name == "errorFactory" && arg.Value != null)
            {
                var walker = new ExceptionMessageWalker();
                walker.Visit(arg.Value);

                if (walker.FoundExceptionMessage)
                {
                    var diagnostic = Diagnostic.Create(Rule, arg.Syntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private sealed class ExceptionMessageWalker : OperationWalker
    {
        public bool FoundExceptionMessage { get; private set; }

        public override void VisitPropertyReference(IPropertyReferenceOperation operation)
        {
            if (operation.Property.Name == "Message")
            {
                var type = operation.Property.ContainingType;
                while (type != null)
                {
                    if (type.Name == "Exception" && type.ContainingNamespace?.Name == "System")
                    {
                        FoundExceptionMessage = true;
                        break;
                    }
                    type = type.BaseType;
                }
            }
            
            base.VisitPropertyReference(operation);
        }
    }
}
