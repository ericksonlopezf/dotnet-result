// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Provides a code fix provider for <c>RESULT013</c> — Avoid implicit bool conversion of Result in condition contexts.
/// </summary>
/// <remarks>
/// Rewrites conditions using <c>result</c> to explicit checks:
/// <code>
/// // Before (RESULT013)
/// if (result)
///
/// // After
/// if (result.IsSuccess)
/// </code>
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BoolOperatorUsageCodeFix)), Shared]
public sealed class BoolOperatorUsageCodeFix : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(BoolOperatorUsageAnalyzer.DiagnosticId);

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
        if (node is not ExpressionSyntax expression) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use .IsSuccess",
                createChangedDocument: _ => Task.FromResult(ReplaceWithMemberAccess(context.Document, root, expression, "IsSuccess")),
                equivalenceKey: "UseIsSuccess"),
            diagnostic);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use .IsFailure",
                createChangedDocument: _ => Task.FromResult(ReplaceWithMemberAccess(context.Document, root, expression, "IsFailure")),
                equivalenceKey: "UseIsFailure"),
            diagnostic);
    }

    private static Document ReplaceWithMemberAccess(
        Document document,
        SyntaxNode root,
        ExpressionSyntax expression,
        string memberName)
    {
        ExpressionSyntax target = expression;
        if (expression is not (IdentifierNameSyntax or MemberAccessExpressionSyntax or InvocationExpressionSyntax or ElementAccessExpressionSyntax or ParenthesizedExpressionSyntax))
        {
            target = SyntaxFactory.ParenthesizedExpression(expression.WithoutTrivia());
        }

        var newExpression = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            target.WithoutTrivia(),
            SyntaxFactory.IdentifierName(memberName))
            .WithLeadingTrivia(expression.GetLeadingTrivia())
            .WithTrailingTrivia(expression.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(expression, newExpression);
        return document.WithSyntaxRoot(newRoot);
    }
}
