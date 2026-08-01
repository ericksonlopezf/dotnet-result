using System.Threading.Tasks;
using Xunit;
using EricksonLopez.Result.Analyzers;

namespace EricksonLopez.Result.Tests.Core;

public class ErrorBuilderDiscardedReturnAnalyzerTests
{
    [Fact]
    public async Task RESULT003_TriggersOn_DiscardedReturnValue()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"");
        builder.WithType(ErrorType.Domain); // Discarded!
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ErrorBuilderDiscardedReturnAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT003");
    }

    [Fact]
    public async Task RESULT003_TriggersOn_ExplicitDiscard_Var()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"");
        var _ = builder.WithType(ErrorType.Domain); // Discarded via var _
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ErrorBuilderDiscardedReturnAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT003");
    }

    [Fact]
    public async Task RESULT003_TriggersOn_ExplicitDiscard_Assignment()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"");
        _ = builder.WithType(ErrorType.Domain); // Discarded via _ =
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ErrorBuilderDiscardedReturnAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT003");
    }

    [Fact]
    public async Task RESULT003_DoesNotTrigger_When_Assigned()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"");
        var b2 = builder.WithType(ErrorType.Domain); // Assigned!
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ErrorBuilderDiscardedReturnAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT003");
    }

    [Fact]
    public async Task RESULT003_DoesNotTrigger_When_Chained()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var error = Error.Create(""code"", ""desc"")
                         .WithType(ErrorType.Domain) // Chained!
                         .Build();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ErrorBuilderDiscardedReturnAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT003");
    }

    [Fact]
    public async Task RESULT003_CodeFix_AssignsToExistingLocal()
    {
        const string source = @"
using EricksonLopez.Result;

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
    public async Task RESULT003_CodeFix_IntroducesNewLocal()
    {
        const string source = @"
using EricksonLopez.Result;

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
}
