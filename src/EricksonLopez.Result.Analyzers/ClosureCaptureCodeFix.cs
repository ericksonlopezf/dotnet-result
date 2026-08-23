// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// CodeFixProvider for <c>RESULT004</c> — Closure capture in Result pipeline methods.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ClosureCaptureCodeFix)), Shared]
public sealed class ClosureCaptureCodeFix : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ClosureCaptureAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var node = root.FindNode(diagnosticSpan);
        var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>()
            ?? node.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
        if (invocation == null) return;

        // Find the lambda(s) inside the invocation
        var lambdas = invocation.ArgumentList.Arguments
            .Select(a => a.Expression)
            .OfType<LambdaExpressionSyntax>()
            .Where(l => !l.Modifiers.Any(SyntaxKind.StaticKeyword))
            .ToList();

        if (lambdas.Count == 0) return;

        // Fix 1: Add 'static' modifier to the lambda.
        // This intentionally causes CS8820 compile errors for each captured variable,
        // making captures explicit in the error list. The developer then refactors to the
        // TState overload to resolve the errors.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "[RESULT004] Reveal captures: make lambda 'static' (then switch to TState overload)",
                createChangedDocument: c => MakeLambdaStaticAsync(context.Document, root, lambdas, c),
                equivalenceKey: nameof(ClosureCaptureCodeFix) + "_static"),
            diagnostic);

        // Fix 2: Insert a guidance comment above the flagged invocation showing the
        // zero-allocation TState rewrite pattern. Unlike Fix 1, this does not break the
        // build — it guides the developer on HOW to rewrite with the method name inline.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "[RESULT004] Insert TState rewrite guidance comment (shows zero-allocation pattern)",
                createChangedDocument: c => InsertTStateGuidanceCommentAsync(context.Document, root, invocation, c),
                equivalenceKey: nameof(ClosureCaptureCodeFix) + "_guidance"),
            diagnostic);
    }

    private static Task<Document> MakeLambdaStaticAsync(
        Document document,
        SyntaxNode root,
        List<LambdaExpressionSyntax> lambdas,
        CancellationToken cancellationToken)
    {
        var editor = new Microsoft.CodeAnalysis.Editing.SyntaxEditor(root, document.Project.Solution.Services);

        foreach (var lambda in lambdas)
        {
            var staticToken = SyntaxFactory.Token(SyntaxKind.StaticKeyword).WithTrailingTrivia(SyntaxFactory.Space);
            var newModifiers = lambda.Modifiers.Insert(0, staticToken);
            LambdaExpressionSyntax newLambda = lambda is SimpleLambdaExpressionSyntax simple
                ? (LambdaExpressionSyntax)simple.WithModifiers(newModifiers)
                : ((ParenthesizedLambdaExpressionSyntax)lambda).WithModifiers(newModifiers);

            editor.ReplaceNode(lambda, newLambda);
        }

        var newRoot = editor.GetChangedRoot();
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }

    private static Task<Document> InsertTStateGuidanceCommentAsync(
        Document document,
        SyntaxNode root,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        // Extract the method name to personalise the guidance comment
        var methodName = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            _ => "Method"
        };

        SyntaxNode targetNode = invocation.FirstAncestorOrSelf<StatementSyntax>() ?? (SyntaxNode)invocation;

        // Build a comment showing before/after with the actual method name
        var commentLines = new[]
        {
            $"// RESULT004: Eliminate closure \u2014 use the TState overload to pass captured variables as a parameter.",
            $"// Before (allocates closure):   result.{methodName}(x => DoWork(captured, x))",
            $"// After  (zero-allocation):     result.{methodName}(captured, static (state, x) => DoWork(state, x))",
            $"// For 'this' captures:          result.{methodName}(this, static (self, x) => self.DoWork(x))",
            $"// Remove this comment after applying the TState rewrite.",
        };

        var triviaList = new List<SyntaxTrivia>();
        foreach (var line in commentLines)
        {
            triviaList.Add(SyntaxFactory.Comment(line));
            triviaList.Add(SyntaxFactory.ElasticCarriageReturnLineFeed);
        }

        var newLeading = SyntaxFactory.TriviaList(triviaList).AddRange(targetNode.GetLeadingTrivia());
        var newRoot = root.ReplaceNode(targetNode, targetNode.WithLeadingTrivia(newLeading));

        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}




