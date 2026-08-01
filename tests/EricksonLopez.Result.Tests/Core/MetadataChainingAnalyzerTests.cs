using System.Threading.Tasks;
using Xunit;
using EricksonLopez.Result.Analyzers;

namespace EricksonLopez.Result.Tests.Core;

public class MetadataChainingAnalyzerTests
{
    [Fact]
    public async Task RESULT005_TriggersOn_Error_WithMetadata_Chained3Times()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var error = Error.Create(""code"", ""desc"").Build()
            .WithMetadata(""a"", 1)
            .WithMetadata(""b"", 2)
            .WithMetadata(""c"", 3);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT005");
    }

    [Fact]
    public async Task RESULT005_DoesNotTrigger_On_Error_WithMetadata_Chained2Times()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var error = Error.Create(""code"", ""desc"").Build()
            .WithMetadata(""a"", 1)
            .WithMetadata(""b"", 2);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT005");
    }

    [Fact]
    public async Task RESULT005_TriggersOn_ErrorBuilder_WithMetadata_Chained3Times()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"")
            .WithMetadata(""a"", 1)
            .WithMetadata(""b"", 2)
            .WithMetadata(""c"", 3);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT005");
    }

    [Fact]
    public async Task RESULT005_DoesNotTrigger_On_ErrorBuilder_WithMetadata_Chained2Times()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"")
            .WithMetadata(""a"", 1)
            .WithMetadata(""b"", 2);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT005");
    }

    [Fact]
    public async Task RESULT005_DoesNotTrigger_On_WithMetadata_BatchOverload()
    {
        const string source = @"
using System.Collections.Generic;
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"")
            .WithMetadata(new Dictionary<string, object?> { { ""a"", 1 }, { ""b"", 2 }, { ""c"", 3 } });
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT005");
    }
}
