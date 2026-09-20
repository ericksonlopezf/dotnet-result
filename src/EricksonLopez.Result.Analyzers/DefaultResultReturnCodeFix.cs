// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Provides a code fix provider for <c>RESULT012</c> — Avoid returning default(Result) or default(Result&lt;T&gt;).
/// </summary>
/// <remarks>
/// Rewrites default returns to explicit <c>Result.Success(...)</c> or <c>Result.Failure(...)</c> expressions:
/// <code>
/// // Before (RESULT012)
/// return default;
///
/// // After
/// return Result.Success();
/// </code>
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DefaultResultReturnCodeFix)), Shared]
public sealed class DefaultResultReturnCodeFix : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(DefaultResultReturnAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        var defaultExpr = FindDefaultExpression(node);
        if (defaultExpr == null) return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        var returnType = GetReturnType(semanticModel, defaultExpr, context.CancellationToken);

        bool isGenericResult = returnType is INamedTypeSymbol named &&
                               named.Name == "Result" &&
                               named.TypeArguments.Length == 1;

        string typeArgName = isGenericResult
            ? ((INamedTypeSymbol)returnType!).TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
            : string.Empty;

        string successExpr = isGenericResult ? $"Result.Success(default({typeArgName})!)" : "Result.Success()";
        string failureExpr = isGenericResult
            ? $"Result.Failure<{typeArgName}>(Error.Failure(\"Error.Code\", \"Error description\"))"
            : "Result.Failure(Error.Failure(\"Error.Code\", \"Error description\"))";

        context.RegisterCodeFix(
            CodeAction.Create(
                title: isGenericResult ? $"Return Result.Success(default({typeArgName})!)" : "Return Result.Success()",
                createChangedDocument: _ => Task.FromResult(ReplaceWithExpression(context.Document, root, defaultExpr, successExpr)),
                equivalenceKey: "ReturnResultSuccess"),
            diagnostic);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: isGenericResult ? $"Return Result.Failure<{typeArgName}>(...)" : "Return Result.Failure(...)",
                createChangedDocument: _ => Task.FromResult(ReplaceWithExpression(context.Document, root, defaultExpr, failureExpr)),
                equivalenceKey: "ReturnResultFailure"),
            diagnostic);
    }

    private static ExpressionSyntax? FindDefaultExpression(SyntaxNode node)
    {
        if (node is ReturnStatementSyntax returnStmt)
        {
            return returnStmt.Expression;
        }

        if (node is ArrowExpressionClauseSyntax arrow)
        {
            return arrow.Expression;
        }

        var defaultExpr = node.DescendantNodesAndSelf()
            .OfType<ExpressionSyntax>()
            .FirstOrDefault(e => e.IsKind(SyntaxKind.DefaultLiteralExpression) || e.IsKind(SyntaxKind.DefaultExpression));

        return defaultExpr ?? (node as ExpressionSyntax);
    }

    private static ITypeSymbol? GetReturnType(SemanticModel? semanticModel, ExpressionSyntax defaultExpr, CancellationToken cancellationToken)
    {
        if (semanticModel == null) return null;

        var typeInfo = semanticModel.GetTypeInfo(defaultExpr, cancellationToken);
        var returnType = typeInfo.ConvertedType ?? typeInfo.Type;
        if (returnType != null) return returnType;

        var methodDecl = defaultExpr.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDecl == null) return null;

        var methodSymbol = semanticModel.GetDeclaredSymbol(methodDecl, cancellationToken);
        return methodSymbol?.ReturnType;
    }

    private static Document ReplaceWithExpression(
        Document document,
        SyntaxNode root,
        ExpressionSyntax targetExpr,
        string newExprText)
    {
        var replacement = SyntaxFactory.ParseExpression(newExprText)
            .WithLeadingTrivia(targetExpr.GetLeadingTrivia())
            .WithTrailingTrivia(targetExpr.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(targetExpr, replacement);
        return document.WithSyntaxRoot(newRoot);
    }
}
