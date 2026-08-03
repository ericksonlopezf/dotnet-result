using System.Linq;
using System.Threading.Tasks;
using Xunit;
using EricksonLopez.Result.Analyzers;

namespace EricksonLopez.Result.Analyzers.Tests;

public class ClosureCaptureAnalyzerTests
{
    [Fact]
    public async Task RESULT004_TriggersOn_Map_Capturing_Local()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        int myLocal = 5;
        return result.Map(x => x + myLocal);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_NoCapture()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        return result.Map(x => x + 1);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_UsingStateOverload()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        int myLocal = 5;
        return result.Map(myLocal, (state, x) => x + state);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_CodeFix_AppliesCorrectly()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        int myLocal = 5;
        return result.Map(x => x + myLocal);
    }
}";
        
        var fixedSource = await AnalyzerTestHelper.ApplyCodeFixAsync<ClosureCaptureAnalyzer, ClosureCaptureCodeFix>(source, "RESULT004");
        
        // The code fix provider registers two fixes. The first one (which ApplyCodeFixAsync picks)
        // inserts the 'static' modifier to the lambda.
        Assert.Contains("result.Map(static x => x + myLocal)", fixedSource);
    }
}

