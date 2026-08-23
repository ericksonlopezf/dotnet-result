// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace EricksonLopez.Result.Analyzers.Tests;

public class ErrorBuilderDiscardedReturnAnalyzerTests
{
    [Theory]
    [InlineData("builder.WithType(ErrorType.Domain);")]
    [InlineData("builder.WithSeverity(ErrorSeverity.Critical);")]
    [InlineData("builder.WithRetryability(ErrorRetryability.Transient);")]
    [InlineData("builder.WithDescriptionKey(\"key\");")]
    [InlineData("builder.WithTraceId(\"trace\");")]
    [InlineData("builder.WithCorrelationId(\"corr\");")]
    [InlineData("builder.WithMetadata(\"k\", \"v\");")]
    [InlineData("builder.WithInnerError(Error.Failure(\"c\", \"d\"));")]
    [InlineData("builder.WithInnerErrors(new[] { Error.Failure(\"c\", \"d\") });")]
    public async Task RESULT003_TriggersOn_AllTrackedWithMethods_WhenDiscarded(string methodCall)
    {
        string source = $@"
using EricksonLopez.Result;

public class TestClass
{{
    public void Method()
    {{
        var builder = Error.Create(""code"", ""desc"");
        {methodCall}
    }}
}}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ErrorBuilderDiscardedReturnAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT003");
        Assert.Equal(DiagnosticSeverity.Error, diag.Severity);
    }

    [Fact]
    public async Task RESULT003_TriggersOn_ExplicitDiscard_Var()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"");
        var _ = builder.WithType(ErrorType.Domain);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ErrorBuilderDiscardedReturnAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT003");
    }

    [Fact]
    public async Task RESULT003_TriggersOn_ExplicitDiscard_Assignment()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"");
        _ = builder.WithType(ErrorType.Domain);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ErrorBuilderDiscardedReturnAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT003");
    }

    [Fact]
    public async Task RESULT003_DoesNotTrigger_When_Assigned()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"");
        var b2 = builder.WithType(ErrorType.Domain);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ErrorBuilderDiscardedReturnAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT003");
    }

    [Fact]
    public async Task RESULT003_DoesNotTrigger_When_Chained()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var error = Error.Create(""code"", ""desc"")
                         .WithType(ErrorType.Domain)
                         .Build();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ErrorBuilderDiscardedReturnAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT003");
    }

    [Fact]
    public async Task RESULT003_DoesNotTrigger_On_UntrackedMethod_Build()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"");
        builder.Build();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ErrorBuilderDiscardedReturnAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT003");
    }

    [Fact]
    public async Task RESULT003_DoesNotTrigger_On_DifferentType_WithTypeMethod()
    {
        const string source = @"
public class CustomBuilder
{
    public CustomBuilder WithType(int type) => this;
}

public class TestClass
{
    public void Method()
    {
        var builder = new CustomBuilder();
        builder.WithType(1);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ErrorBuilderDiscardedReturnAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT003");
    }

    [Fact]
    public async Task RESULT003_NoDiagnostic_When_DirectMethodInvocation()
    {
        const string source = @"
public class TestClass
{
    public void Method()
    {
        DirectCall();
    }
    private void DirectCall() {}
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ErrorBuilderDiscardedReturnAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT003");
    }

    [Fact]
    public async Task RESULT003_CodeFix_AssignsToExistingLocal()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"");
        builder.WithType(ErrorType.Domain);
    }
}";
        var fixedSource = await AnalyzerTestHelper.ApplyCodeFixAsync<ErrorBuilderDiscardedReturnAnalyzer, ErrorBuilderDiscardedReturnCodeFix>(source, "RESULT003");
        Assert.Contains("builder = builder.WithType(ErrorType.Domain);", fixedSource);
    }

    [Fact]
    public async Task RESULT003_CodeFix_IntroducesNewLocal_When_ReceiverIsNotSimpleIdentifier()
    {
        const string source = @"

public class TestClass
{
    public ErrorBuilder GetBuilder() => Error.Create(""code"", ""desc"");

    public void Method()
    {
        GetBuilder().WithType(ErrorType.Domain);
    }
}";
        var fixedSource = await AnalyzerTestHelper.ApplyCodeFixAsync<ErrorBuilderDiscardedReturnAnalyzer, ErrorBuilderDiscardedReturnCodeFix>(source, "RESULT003");
        Assert.Contains("var builder = GetBuilder().WithType(ErrorType.Domain);", fixedSource);
    }

    [Fact]
    public async Task RESULT003_CodeFix_IntroducesNewLocal_When_MemberAccessReceiver()
    {
        const string source = @"

public class TestClass
{
    private ErrorBuilder builder;

    public void Method()
    {
        this.builder.WithType(ErrorType.Domain);
    }
}";
        var fixedSource = await AnalyzerTestHelper.ApplyCodeFixAsync<ErrorBuilderDiscardedReturnAnalyzer, ErrorBuilderDiscardedReturnCodeFix>(source, "RESULT003");
        Assert.Contains("var builder = this.builder.WithType(ErrorType.Domain);", fixedSource);
    }

    [Fact]
    public async Task RESULT003_CodeFix_ReturnsUnchanged_When_NotExpressionStatement()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"");
        _ = builder.WithType(ErrorType.Domain);
    }
}";
        var fixedSource = await AnalyzerTestHelper.ApplyCodeFixAsync<ErrorBuilderDiscardedReturnAnalyzer, ErrorBuilderDiscardedReturnCodeFix>(source, "RESULT003");
        Assert.Contains("_ = builder.WithType(ErrorType.Domain);", fixedSource);
    }

    [Fact]
    public async Task RESULT003_CodeFix_ReturnsUnchanged_When_DiagnosticNotFound()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"");
        var b2 = builder.WithType(ErrorType.Domain);
    }
}";
        var fixedSource = await AnalyzerTestHelper.ApplyCodeFixAsync<ErrorBuilderDiscardedReturnAnalyzer, ErrorBuilderDiscardedReturnCodeFix>(source, "RESULT003");
        Assert.Equal(source, fixedSource);
    }

    [Fact]
    public void RESULT003_Descriptor_Properties_Are_Correct()
    {
        var analyzer = new ErrorBuilderDiscardedReturnAnalyzer();
        var descriptor = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("RESULT003", descriptor.Id);
        Assert.Equal("ErrorBuilder method return value is discarded", descriptor.Title.ToString());
        Assert.Equal("Usage", descriptor.Category);
        Assert.Equal(DiagnosticSeverity.Error, descriptor.DefaultSeverity);
        Assert.True(descriptor.IsEnabledByDefault);
        Assert.Equal("https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/error-builder.md#discarded-return-analyzer", descriptor.HelpLinkUri);
        Assert.Contains("Return value of ErrorBuilder.{0}() is discarded", descriptor.MessageFormat.ToString());
        Assert.Contains("ErrorBuilder is a readonly struct with copy-on-write semantics", descriptor.Description.ToString());
    }

    [Fact]
    public void RESULT003_CodeFix_Properties_Are_Correct()
    {
        var provider = new ErrorBuilderDiscardedReturnCodeFix();
        var id = Assert.Single(provider.FixableDiagnosticIds);
        Assert.Equal("RESULT003", id);
        Assert.NotNull(provider.GetFixAllProvider());
    }

    [Fact]
    public async Task RESULT003_CodeFix_Registers_AssignToExistingLocal_Action_Properties()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"");
        builder.WithType(ErrorType.Domain);
    }
}";
        var actions = await AnalyzerTestHelper.GetCodeActionsAsync<ErrorBuilderDiscardedReturnAnalyzer, ErrorBuilderDiscardedReturnCodeFix>(source, "RESULT003");
        var action = Assert.Single(actions);
        Assert.Equal("Assign return value to 'builder'", action.Title);
        Assert.Equal("RESULT003_Assign_builder", action.EquivalenceKey);
    }

    [Fact]
    public async Task RESULT003_CodeFix_Registers_IntroduceLocal_Action_Properties()
    {
        const string source = @"

public class TestClass
{
    public ErrorBuilder GetBuilder() => Error.Create(""code"", ""desc"");

    public void Method()
    {
        GetBuilder().WithType(ErrorType.Domain);
    }
}";
        var actions = await AnalyzerTestHelper.GetCodeActionsAsync<ErrorBuilderDiscardedReturnAnalyzer, ErrorBuilderDiscardedReturnCodeFix>(source, "RESULT003");
        var action = Assert.Single(actions);
        Assert.Equal("Assign return value to a new local variable 'builder'", action.Title);
        Assert.Equal("RESULT003_IntroduceLocal", action.EquivalenceKey);
    }

    [Fact]
    public async Task RESULT003_CodeFix_RegistersNoAction_When_InvocationNotFound()
    {
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var solution = workspace.CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .AddDocument(documentId, "Test.cs", SourceText.From("public class Test {}"));
        var document = solution.GetDocument(documentId)!;
        var tree = await document.GetSyntaxTreeAsync();
        var diagnostic = Diagnostic.Create(
            new ErrorBuilderDiscardedReturnAnalyzer().SupportedDiagnostics[0],
            Location.Create(tree!, new Microsoft.CodeAnalysis.Text.TextSpan(0, 0)),
            "WithType");

        var actions = new List<CodeAction>();
        var provider = new ErrorBuilderDiscardedReturnCodeFix();
        var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
        await provider.RegisterCodeFixesAsync(context);

        Assert.Empty(actions);
    }

    [Fact]
    public async Task RESULT003_CodeFix_RegistersNoAction_When_ParentNotExpressionStatement()
    {
        const string source = @"
public class Test
{
    public void Method()
    {
        var builder = Error.Create(""c"", ""d"");
        _ = builder.WithType(ErrorType.Domain);
    }
}";
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var solution = workspace.CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .AddDocument(documentId, "Test.cs", SourceText.From(source));
        var document = solution.GetDocument(documentId)!;
        var tree = await document.GetSyntaxTreeAsync();
        var root = await tree!.GetRootAsync();
        var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
        var diagnostic = Diagnostic.Create(
            new ErrorBuilderDiscardedReturnAnalyzer().SupportedDiagnostics[0],
            invocation.GetLocation(),
            "WithType");

        var actions = new List<CodeAction>();
        var provider = new ErrorBuilderDiscardedReturnCodeFix();
        var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
        await provider.RegisterCodeFixesAsync(context);

        Assert.Empty(actions);
    }
}





