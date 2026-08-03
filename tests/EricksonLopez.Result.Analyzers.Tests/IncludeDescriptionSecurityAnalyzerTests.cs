using System.Threading.Tasks;
using Xunit;
using EricksonLopez.Result.Analyzers;

namespace EricksonLopez.Result.Analyzers.Tests;

public class IncludeDescriptionSecurityAnalyzerTests
{
    [Fact]
    public async Task RESULT009_Triggers_On_IncludeDescription_AssignedTrue()
    {
        const string source = @"
using EricksonLopez.Result.AspNetCore;

public class TestClass
{
    public void Method()
    {
        var options = new ResultHttpOptions();
        options.IncludeDescription = true;
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<IncludeDescriptionSecurityAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT009");
    }

    [Fact]
    public async Task RESULT009_DoesNotTrigger_On_IncludeDescription_AssignedFalse()
    {
        const string source = @"
using EricksonLopez.Result.AspNetCore;

public class TestClass
{
    public void Method()
    {
        var options = new ResultHttpOptions();
        options.IncludeDescription = false;
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<IncludeDescriptionSecurityAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT009");
    }

    [Fact]
    public async Task RESULT009_DoesNotTrigger_On_IncludeDescription_AssignedVariable()
    {
        const string source = @"
using EricksonLopez.Result.AspNetCore;

public class TestClass
{
    public void Method(bool include)
    {
        var options = new ResultHttpOptions();
        options.IncludeDescription = include;
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<IncludeDescriptionSecurityAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT009");
    }
}

