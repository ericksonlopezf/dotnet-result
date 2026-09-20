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
/// Provides a code fix provider for <c>RESULT007</c> — Missing ErrorEqualityComparer.Strict in collection or LINQ deduplication.
/// </summary>
/// <remarks>
/// Adds an explicit <c>ErrorEqualityComparer.Strict</c> (or <c>ErrorEqualityComparer.Default</c>) argument
/// to collection instantiations or LINQ deduplication calls.
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(HashSetErrorEqualityCodeFix)), Shared]
public sealed class HashSetErrorEqualityCodeFix : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(HashSetErrorEqualityAnalyzer.DiagnosticId);

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

        var creation = node.FirstAncestorOrSelf<BaseObjectCreationExpressionSyntax>();
        var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();

        if (creation == null && invocation == null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use ErrorEqualityComparer.Strict",
                createChangedDocument: _ => Task.FromResult(ApplyComparerFix(context.Document, root, creation, invocation, "ErrorEqualityComparer.Strict")),
                equivalenceKey: "UseStrictComparer"),
            diagnostic);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use ErrorEqualityComparer.Default",
                createChangedDocument: _ => Task.FromResult(ApplyComparerFix(context.Document, root, creation, invocation, "ErrorEqualityComparer.Default")),
                equivalenceKey: "UseDefaultComparer"),
            diagnostic);
    }

    private static Document ApplyComparerFix(
        Document document,
        SyntaxNode root,
        BaseObjectCreationExpressionSyntax? creation,
        InvocationExpressionSyntax? invocation,
        string comparerExpression)
    {
        var comparerArg = SyntaxFactory.Argument(SyntaxFactory.ParseExpression(comparerExpression));

        if (creation != null)
        {
            var currentArgs = creation.ArgumentList ?? SyntaxFactory.ArgumentList();
            var newArgs = currentArgs.AddArguments(comparerArg);
            var newCreation = creation.WithArgumentList(newArgs);
            var newRoot = root.ReplaceNode(creation, newCreation);
            return document.WithSyntaxRoot(newRoot);
        }

        if (invocation != null)
        {
            var currentArgs = invocation.ArgumentList;
            var newArgs = currentArgs.AddArguments(comparerArg);
            var newInvocation = invocation.WithArgumentList(newArgs);
            var newRoot = root.ReplaceNode(invocation, newInvocation);
            return document.WithSyntaxRoot(newRoot);
        }

        return document;
    }
}
