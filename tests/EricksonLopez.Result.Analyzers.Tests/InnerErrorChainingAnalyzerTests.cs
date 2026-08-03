using System.Threading.Tasks;
using Xunit;
using EricksonLopez.Result.Analyzers;

namespace EricksonLopez.Result.Analyzers.Tests;

public class InnerErrorChainingAnalyzerTests
{
    [Fact]
    public async Task RESULT006_TriggersOn_Chained_WithInnerError()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        var e2 = Error.Create(""e2"", ""2"").Build();
        
        var builder = Error.Create(""code"", ""desc"")
            .WithInnerError(e1)
            .WithInnerError(e2);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT006");
    }

    [Fact]
    public async Task RESULT006_DoesNotTrigger_When_Single_WithInnerError()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        
        var builder = Error.Create(""code"", ""desc"")
            .WithInnerError(e1);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT006");
    }

    [Fact]
    public async Task RESULT006_DoesNotTrigger_When_WithInnerErrors()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        var e2 = Error.Create(""e2"", ""2"").Build();
        
        var builder = Error.Create(""code"", ""desc"")
            .WithInnerErrors(new[] { e1, e2 });
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT006");
    }

    [Fact]
    public async Task RESULT006_DoesNotTrigger_When_Chained_Variables_Instead_Of_Fluent()
    {
        // Wait, if it's assigned to variables, the analyzer currently doesn't track variables.
        // It only tracks fluent chaining. That's a limitation but acceptable for this rule.
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        var e2 = Error.Create(""e2"", ""2"").Build();
        
        var builder1 = Error.Create(""code"", ""desc"").WithInnerError(e1);
        var builder2 = builder1.WithInnerError(e2);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT006");
    }
}

