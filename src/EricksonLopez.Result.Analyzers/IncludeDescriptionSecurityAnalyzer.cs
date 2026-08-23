// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using EricksonLopez.Result;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Roslyn diagnostic analyzer (RESULT009) that warns when <c>ResultHttpOptions.IncludeDescription</c>
/// is set directly to <see langword="true"/> without an environment guard, which may expose
/// internal error descriptions (including exception messages, file paths, or PII) in HTTP
/// ProblemDetails responses in production environments.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IncludeDescriptionSecurityAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic identifier for this analyzer rule.</summary>
    public const string DiagnosticId = "RESULT009";

    private const string ResultHttpOptionsTypeName = "EricksonLopez.Result.AspNetCore.ResultHttpOptions";
    private const string IncludeDescriptionPropertyName = "IncludeDescription";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "ResultHttpOptions.IncludeDescription set to true without environment guard",
        messageFormat: "Setting 'IncludeDescription = true' unconditionally may expose internal error descriptions " +
                       "(exception messages, paths, PII) in HTTP ProblemDetails responses in production. " +
                       "Use 'options.IncludeDescriptionInDevelopment(env)' or 'options.IncludeDescription = env.IsDevelopment()' instead.",
        category: "Security",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ResultHttpOptions.IncludeDescription = true causes error descriptions to be included in " +
                     "the HTTP ProblemDetails body in all environments, including production. " +
                     "This can expose sensitive data such as exception messages, file system paths, " +
                     "database connection strings, and PII. " +
                     "Use IncludeDescriptionInDevelopment(env) to safely restrict exposure to development only.",
        helpLinkUri: "https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md#RESULT009");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // We look for simple assignment operations: options.IncludeDescription = true
        context.RegisterOperationAction(AnalyzeSimpleAssignment, OperationKind.SimpleAssignment);
    }

    private static void AnalyzeSimpleAssignment(OperationAnalysisContext context)
    {
        var assignment = (ISimpleAssignmentOperation)context.Operation;

        // The left-hand side must be a property reference on ResultHttpOptions.IncludeDescription
        if (assignment.Target is not IPropertyReferenceOperation propRef)
            return;

        if (!string.Equals(propRef.Property.Name, IncludeDescriptionPropertyName, StringComparison.Ordinal) ||
            propRef.Property.ContainingType.ToDisplayString() != ResultHttpOptionsTypeName)
        {
            return;
        }

        // The right-hand side must be the boolean constant 'true'
        if (assignment.Value.ConstantValue is not { HasValue: true, Value: true })
            return;

        var diagnostic = Diagnostic.Create(Rule, assignment.Syntax.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}

