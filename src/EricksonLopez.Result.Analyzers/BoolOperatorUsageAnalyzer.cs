// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Represents a Roslyn diagnostic analyzer (RESULT013) that warns when <see cref="Result"/> or <c>Result&lt;T&gt;</c>
/// is used directly in a condition context (e.g. <c>if (result)</c>, <c>result ? a : b</c>, <c>while (result)</c>).
/// </summary>
/// <remarks>
/// <para>
/// An uninitialized <see cref="Result"/> struct (<c>default(Result)</c>) has a state value of 0 (<c>Uninitialized</c>).
/// Its <c>operator true</c> returns <c>false</c> without throwing, causing uninitialized results to silently
/// evaluate as failure in boolean conditions.
/// </para>
/// <para>
/// Explicitly checking <c>.IsSuccess</c> or <c>.IsFailure</c> avoids this ambiguity and makes the intention explicit.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BoolOperatorUsageAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Gets the diagnostic identifier for this analyzer rule.</summary>
    public const string DiagnosticId = "RESULT013";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Avoid implicit bool conversion of Result in condition contexts",
        messageFormat: "Using '{0}' directly as a condition relies on implicit boolean conversion where uninitialized results evaluate to false. Use '{0}.IsSuccess' or '{0}.IsFailure' instead.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "An uninitialized Result struct evaluates to false via operator true and will silently behave as failure in if/conditional expressions. Explicitly check IsSuccess or IsFailure instead.",
        helpLinkUri: "https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md#RESULT013");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        context.RegisterSyntaxNodeAction(AnalyzeConditionalExpression, SyntaxKind.ConditionalExpression);
        context.RegisterSyntaxNodeAction(AnalyzeWhileStatement, SyntaxKind.WhileStatement);
        context.RegisterSyntaxNodeAction(AnalyzeDoStatement, SyntaxKind.DoStatement);
    }

    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;
        CheckCondition(context, ifStatement.Condition);
    }

    private static void AnalyzeConditionalExpression(SyntaxNodeAnalysisContext context)
    {
        var conditional = (ConditionalExpressionSyntax)context.Node;
        CheckCondition(context, conditional.Condition);
    }

    private static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
    {
        var whileStatement = (WhileStatementSyntax)context.Node;
        CheckCondition(context, whileStatement.Condition);
    }

    private static void AnalyzeDoStatement(SyntaxNodeAnalysisContext context)
    {
        var doStatement = (DoStatementSyntax)context.Node;
        CheckCondition(context, doStatement.Condition);
    }

    private static void CheckCondition(SyntaxNodeAnalysisContext context, ExpressionSyntax condition)
    {
        var unwrapped = UnwrapParentheses(condition);
        var typeInfo = context.SemanticModel.GetTypeInfo(unwrapped, context.CancellationToken);

        var type = typeInfo.Type;
        if (type != null && IsResultType(type))
        {
            var diagnostic = Diagnostic.Create(Rule, unwrapped.GetLocation(), unwrapped.ToString());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static ExpressionSyntax UnwrapParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }
        return expression;
    }

    private static bool IsResultType(ITypeSymbol type)
    {
        var ns = type.ContainingNamespace?.ToDisplayString();
        if (ns == null || !ns.EndsWith("Result", StringComparison.Ordinal))
        {
            return false;
        }

        return string.Equals(type.Name, "Result", StringComparison.Ordinal);
    }
}
