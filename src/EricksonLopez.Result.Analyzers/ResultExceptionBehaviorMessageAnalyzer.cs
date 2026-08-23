// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using EricksonLopez.Result;
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
    /// <summary>The diagnostic identifier for this analyzer rule.</summary>
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
        helpLinkUri: "https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md#RESULT010");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
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

        if (!string.Equals(method.Name, MethodName, StringComparison.Ordinal))
            return;

        if (!string.Equals(method.ContainingType.Name, ExtensionClassName, StringComparison.Ordinal))
            return;

        foreach (var arg in invocation.Arguments)
        {
            if (string.Equals(arg.Parameter!.Name, "errorFactory", StringComparison.Ordinal))
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
            if (string.Equals(operation.Property.Name, "Message", StringComparison.Ordinal) &&
                IsOrDerivesFromException(operation.Property.ContainingType))
            {
                FoundExceptionMessage = true;
            }

            base.VisitPropertyReference(operation);
        }

        private static bool IsOrDerivesFromException(INamedTypeSymbol? type)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                if (string.Equals(current.Name, "Exception", StringComparison.Ordinal) &&
                    string.Equals(current.ContainingNamespace!.Name, "System", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

