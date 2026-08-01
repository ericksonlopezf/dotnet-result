using System.Collections.Immutable;
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
/// <remarks>
/// <para>
/// <b>Risk:</b> Setting <c>IncludeDescription = true</c> unconditionally causes error descriptions
/// (including those derived from exceptions via <c>ResultExceptionBehavior</c>) to be included in
/// the HTTP response body in all environments — including production. This can inadvertently leak:
/// <list type="bullet">
///   <item>Database connection strings (from <c>SqlException.Message</c>)</item>
///   <item>File system paths (from <c>FileNotFoundException.Message</c>)</item>
///   <item>Internal service names and IP addresses</item>
///   <item>Personally identifiable information (PII)</item>
/// </list>
/// </para>
/// <para>
/// <b>Secure pattern:</b> Use <c>IncludeDescriptionInDevelopment(env)</c> to restrict exposure
/// to development environments only:
/// <code>
/// services.Configure&lt;ResultHttpOptions&gt;(options =>
///     options.IncludeDescriptionInDevelopment(env));
/// </code>
/// </para>
/// <para>
/// <b>Alternative — explicit conditional:</b>
/// <code>
/// services.Configure&lt;ResultHttpOptions&gt;(options =>
///     options.IncludeDescription = env.IsDevelopment());
/// </code>
/// </para>
/// <para>
/// <b>Suppression:</b> If you intentionally expose descriptions in all environments (e.g., internal APIs
/// with no public access), suppress this diagnostic with a pragma or <c>[SuppressMessage]</c>.
/// </para>
/// <para>
/// Severity: <see cref="DiagnosticSeverity.Warning"/> — information disclosure is a security risk,
/// not merely a style concern.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IncludeDescriptionSecurityAnalyzer : DiagnosticAnalyzer
{
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
        helpLinkUri: "https://github.com/ericksonlopez/dotnet-result/blob/main/docs/analyzers.md#RESULT009");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

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

        // The left-hand side must be a property reference
        if (assignment.Target is not IPropertyReferenceOperation propRef)
            return;

        // Must be the IncludeDescription property
        if (!string.Equals(propRef.Property.Name, IncludeDescriptionPropertyName, System.StringComparison.Ordinal))
            return;

        // Must be on ResultHttpOptions
        var containingType = propRef.Property.ContainingType;
        if (containingType is null)
            return;

        var typeName = containingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                     .Replace("global::", string.Empty);
        if (!string.Equals(typeName, ResultHttpOptionsTypeName, System.StringComparison.Ordinal))
            return;

        // The right-hand side must be the boolean constant 'true'
        // We only flag constant 'true' — not variables or method calls like env.IsDevelopment()
        if (!assignment.Value.ConstantValue.HasValue || assignment.Value.ConstantValue.Value is not true)
            return;

        // At this point: options.IncludeDescription = true was written as a literal 'true'
        // without any conditional wrapping at the assignment level.
        // This is the unsafe pattern — report RESULT009.
        var diagnostic = Diagnostic.Create(Rule, assignment.Syntax.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
