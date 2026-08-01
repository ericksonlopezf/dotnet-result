using System.Collections.Generic;
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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ClosureCaptureCodeFix)), Shared]
public sealed class ClosureCaptureCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ClosureCaptureAnalyzer.DiagnosticId);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var invocation = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>().FirstOrDefault();
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

            LambdaExpressionSyntax newLambda = lambda switch
            {
                SimpleLambdaExpressionSyntax simple => simple.WithModifiers(newModifiers),
                ParenthesizedLambdaExpressionSyntax parenthesized => parenthesized.WithModifiers(newModifiers),
                _ => lambda
            };

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

        // Determine the leading indentation so the comment aligns with existing code
        var statement = invocation.FirstAncestorOrSelf<StatementSyntax>();
        var indentation = statement != null
            ? string.Concat(statement.GetLeadingTrivia()
                .Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia))
                .Select(t => t.ToFullString()))
            : string.Empty;

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
            triviaList.Add(SyntaxFactory.Comment(indentation + line));
            triviaList.Add(SyntaxFactory.ElasticCarriageReturnLineFeed);
        }

        SyntaxNode targetNode = statement ?? (SyntaxNode)invocation;
        var newLeading = SyntaxFactory.TriviaList(triviaList).AddRange(targetNode.GetLeadingTrivia());
        var newRoot = root.ReplaceNode(targetNode, targetNode.WithLeadingTrivia(newLeading));

        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}
