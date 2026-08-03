using System.Threading.Tasks;
using Xunit;
using EricksonLopez.Result.Analyzers;

namespace EricksonLopez.Result.Analyzers.Tests;

public class HashSetErrorEqualityAnalyzerTests
{
    [Fact]
    public async Task RESULT007_TriggersOn_HashSet_WithoutComparer()
    {
        const string source = @"
using System.Collections.Generic;
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var set = new HashSet<Error>();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT007");
    }

    [Fact]
    public async Task RESULT007_TriggersOn_Dictionary_WithoutComparer()
    {
        const string source = @"
using System.Collections.Generic;
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var dict = new Dictionary<Error, string>();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT007");
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_HashSet_WithComparer()
    {
        const string source = @"
using System.Collections.Generic;
using EricksonLopez.Result;

public class TestClass
{
    public void Method()
    {
        var set = new HashSet<Error>(ErrorEqualityComparer.Strict);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT007");
    }

    [Fact]
    public async Task RESULT007_TriggersOn_Linq_Distinct_WithoutComparer()
    {
        const string source = @"
using System.Collections.Generic;
using System.Linq;
using EricksonLopez.Result;

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var distinct = errors.Distinct();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT007");
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_Linq_Distinct_WithComparer()
    {
        const string source = @"
using System.Collections.Generic;
using System.Linq;
using EricksonLopez.Result;

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var distinct = errors.Distinct(ErrorEqualityComparer.Strict);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT007");
    }
}

