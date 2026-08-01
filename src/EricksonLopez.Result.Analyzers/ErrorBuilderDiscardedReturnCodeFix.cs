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
/// CodeFixProvider for <c>RESULT003</c> — ErrorBuilder discarded return value.
/// </summary>
/// <remarks>
/// <para>
/// When the return value of <c>ErrorBuilder.With*()</c> is discarded
/// (a standalone expression statement), this fix rewrites the statement to
/// reassign the result back to the local variable that holds the builder:
/// </para>
/// <code>
/// // Before (RESULT003 — discarded return value)
/// builder.WithType(ErrorType.Domain);
///
/// // After (CodeFix applied)
/// builder = builder.WithType(ErrorType.Domain);
/// </code>
/// <para>
/// The fix also handles the case where the invocation is on a property accessor
/// or method return — in those cases the fix suggests introducing a local variable:
/// </para>
/// <code>
/// // Before
/// GetBuilder().WithType(ErrorType.Domain);
///
/// // After
/// var builder = GetBuilder().WithType(ErrorType.Domain);
/// </code>
/// <para>
/// <b>Scope:</b> This fix operates on a single document and does not perform
/// solution-wide renames or refactors.
/// </para>
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ErrorBuilderDiscardedReturnCodeFix)), Shared]
public sealed class ErrorBuilderDiscardedReturnCodeFix : CodeFixProvider
{
    // Must match the diagnostic ID emitted by ErrorBuilderDiscardedReturnAnalyzer.
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(ErrorBuilderDiscardedReturnAnalyzer.DiagnosticId);

    // Use the BatchFixer equivalence key so the IDE can apply all instances at once.
    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Locate the invocation expression (ErrorBuilder.WithXxx(...)) at the diagnostic location.
        var invocation = root.FindNode(diagnosticSpan)
            .AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault();

        if (invocation is null) return;

        // The parent is guaranteed to be ExpressionStatementSyntax (that's what the analyzer checks).
        if (invocation.Parent is not ExpressionStatementSyntax expressionStatement) return;

        // Determine the left-hand side identifier for the assignment.
        // If the invocation is "identifier.WithXxx(...)", we reassign to "identifier".
        // Otherwise (e.g., "GetBuilder().WithXxx(...)"), introduce a "builder" local.
        string? receiverName = TryGetReceiverIdentifierName(invocation);

        if (receiverName is not null)
        {
            // Fix: transform `receiver.WithXxx(...)` → `receiver = receiver.WithXxx(...)`
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"Assign return value to '{receiverName}'",
                    createChangedDocument: ct => AssignToExistingLocalAsync(
                        context.Document, root, expressionStatement, invocation, receiverName, ct),
                    equivalenceKey: $"RESULT003_Assign_{receiverName}"),
                diagnostic);
        }
        else
        {
            // Fix: introduce a local variable `var builder = GetBuilder().WithXxx(...)`
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Assign return value to a new local variable 'builder'",
                    createChangedDocument: ct => IntroduceLocalVariableAsync(
                        context.Document, root, expressionStatement, invocation, ct),
                    equivalenceKey: "RESULT003_IntroduceLocal"),
                diagnostic);
        }
    }

    // ─── Fix 1: Assign to existing local ─────────────────────────────────────

    private static Task<Document> AssignToExistingLocalAsync(
        Document document,
        SyntaxNode root,
        ExpressionStatementSyntax expressionStatement,
        InvocationExpressionSyntax invocation,
        string receiverName,
        CancellationToken cancellationToken)
    {
        // Build: receiver = receiver.WithXxx(...)
        var receiverIdentifier = SyntaxFactory.IdentifierName(receiverName);
        var assignment = SyntaxFactory.AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            left: receiverIdentifier,
            right: invocation.WithoutLeadingTrivia()); // strip extra whitespace from invocation

        // Wrap in expression statement, preserving the original leading/trailing trivia.
        var newStatement = SyntaxFactory.ExpressionStatement(assignment)
            .WithLeadingTrivia(expressionStatement.GetLeadingTrivia())
            .WithTrailingTrivia(expressionStatement.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(expressionStatement, newStatement);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }

    // ─── Fix 2: Introduce a local variable ───────────────────────────────────

    private static Task<Document> IntroduceLocalVariableAsync(
        Document document,
        SyntaxNode root,
        ExpressionStatementSyntax expressionStatement,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        // Build: var builder = GetBuilder().WithXxx(...)
        var varDeclaration = SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(
                SyntaxFactory.IdentifierName(
                    SyntaxFactory.Identifier(
                        SyntaxFactory.TriviaList(),
                        SyntaxKind.VarKeyword,
                        "var",
                        "var",
                        SyntaxFactory.TriviaList(SyntaxFactory.Space))))
            .WithVariables(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator(
                        SyntaxFactory.Identifier("builder"))
                    .WithInitializer(
                        SyntaxFactory.EqualsValueClause(invocation.WithoutLeadingTrivia())))));

        var newStatement = varDeclaration
            .WithLeadingTrivia(expressionStatement.GetLeadingTrivia())
            .WithTrailingTrivia(expressionStatement.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(expressionStatement, newStatement);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the simple identifier name of the receiver of the invocation, or <see langword="null"/>
    /// if the receiver is not a simple identifier (e.g., it's a method call, property access, or null).
    /// </summary>
    /// <remarks>
    /// Handles:
    ///   • <c>builder.WithType(...)</c>   → returns "builder"
    ///   • <c>this.builder.WithType(...)</c> → returns null (not a simple identifier)
    ///   • <c>GetBuilder().WithType(...)</c> → returns null
    /// </remarks>
    private static string? TryGetReceiverIdentifierName(InvocationExpressionSyntax invocation)
    {
        // The invocation expression is typically a MemberAccessExpression: receiver.Method(args)
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return null;

        // The receiver (left side of the dot) must be a simple identifier.
        return memberAccess.Expression is IdentifierNameSyntax identifierName
            ? identifierName.Identifier.Text
            : null;
    }
}
