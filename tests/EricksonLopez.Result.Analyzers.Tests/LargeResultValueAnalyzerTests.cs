using System.Threading.Tasks;
using Xunit;
using EricksonLopez.Result.Analyzers;

namespace EricksonLopez.Result.Analyzers.Tests;

public class LargeResultValueAnalyzerTests
{
    [Fact]
    public async Task RESULT001_TriggersOn_LargeStruct_ReturnType()
    {
        const string source = @"
using EricksonLopez.Result;

public struct LargeStruct
{
    public long A, B, C, D, E; // 5 * 8 = 40 bytes
}

public class TestClass
{
    public Result<LargeStruct> Method() => default(LargeStruct);
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_DoesNotTrigger_On_SmallStruct_ReturnType()
    {
        const string source = @"
using EricksonLopez.Result;

public struct SmallStruct
{
    public long A, B, C; // 3 * 8 = 24 bytes (<= 32)
}

public class TestClass
{
    public Result<SmallStruct> Method() => default(SmallStruct);
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_DoesNotTrigger_On_Class_ReturnType()
    {
        const string source = @"
using EricksonLopez.Result;

public class LargeClass
{
    public long A, B, C, D, E, F, G;
}

public class TestClass
{
    public Result<LargeClass> Method() => new LargeClass();
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_TriggersOn_LargeStruct_Parameter()
    {
        const string source = @"
using EricksonLopez.Result;

public struct LargeStruct
{
    public long A, B, C, D, E; // 40 bytes
}

public class TestClass
{
    public void Method(Result<LargeStruct> p) { }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_TriggersOn_LargeStruct_Property()
    {
        const string source = @"
using EricksonLopez.Result;

public struct LargeStruct
{
    public long A, B, C, D, E; // 40 bytes
}

public class TestClass
{
    public Result<LargeStruct> Property { get; set; }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_TriggersOn_LargeStruct_Field()
    {
        const string source = @"
using EricksonLopez.Result;

public struct LargeStruct
{
    public long A, B, C, D, E; // 40 bytes
}

public class TestClass
{
    public Result<LargeStruct> Field;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT001");
    }
}

