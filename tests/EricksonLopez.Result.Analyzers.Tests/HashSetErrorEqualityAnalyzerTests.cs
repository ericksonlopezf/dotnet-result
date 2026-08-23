// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Analyzers;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Result.Analyzers.Tests;

public class HashSetErrorEqualityAnalyzerTests
{
    [Fact]
    public void RESULT007_Descriptor_Properties_AreAccurate()
    {
        var analyzer = new HashSetErrorEqualityAnalyzer();
        var diagnostics = analyzer.SupportedDiagnostics;
        var rule = Assert.Single(diagnostics);

        Assert.Equal("RESULT007", rule.Id);
        Assert.Equal("Missing ErrorEqualityComparer.Strict in collection or LINQ deduplication", rule.Title.ToString());
        Assert.Equal(
            "'{0}' uses Error equality without an explicit comparer. Error.Equals is shallow (semantic fields only) and ignores TraceId and Metadata. Use ErrorEqualityComparer.Strict to prevent silent deduplication, or ErrorEqualityComparer.Default if semantic deduplication is intentional.",
            rule.MessageFormat.ToString());
        Assert.Equal("Reliability", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal(
            "Error.Equals() only compares Code, Description, Type, Severity, and Retryability. " +
            "It intentionally ignores TraceId, CorrelationId, and Metadata. When errors are stored in a " +
            "HashSet<Error>, used as Dictionary keys, or processed with LINQ Distinct/GroupBy/ToHashSet, " +
            "errors with identical codes but different TraceIds will be silently deduplicated. " +
            "Pass ErrorEqualityComparer.Strict to compare all fields, or ErrorEqualityComparer.Default " +
            "if semantic deduplication is explicitly desired.",
            rule.Description.ToString());
        Assert.Equal("https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md#RESULT007", rule.HelpLinkUri);
    }

    [Fact]
    public async Task RESULT007_TriggersOn_HashSet_WithoutComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var set = new HashSet<Error>();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT007");
        Assert.Contains("'HashSet<Error>'", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT007_TriggersOn_HashSet_WithCapacity_WithoutComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var set = new HashSet<Error>(10);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT007");
        Assert.Contains("'HashSet<Error>'", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT007_TriggersOn_Dictionary_WithoutComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var dict = new Dictionary<Error, string>();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT007");
        Assert.Contains("'Dictionary<Error, ...>'", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT007_TriggersOn_Dictionary_WithCapacity_WithoutComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var dict = new Dictionary<Error, string>(10);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT007");
        Assert.Contains("'Dictionary<Error, ...>'", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT007_TriggersOn_HashSet_WithExplicitNullComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var set = new HashSet<Error>((IEqualityComparer<Error>)null);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT007");
        Assert.Contains("'HashSet<Error>'", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT007_TriggersOn_Dictionary_WithExplicitNullComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var dict = new Dictionary<Error, string>((IEqualityComparer<Error>)null);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT007");
        Assert.Contains("'Dictionary<Error, ...>'", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_HashSet_WithComparer()
    {
        const string source = @"

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
    public async Task RESULT007_DoesNotTriggerOn_HashSet_WithCapacityAndComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var set = new HashSet<Error>(10, ErrorEqualityComparer.Strict);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_HashSet_WithDefaultComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var set = new HashSet<Error>(ErrorEqualityComparer.Default);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_Dictionary_WithComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var dict = new Dictionary<Error, string>(ErrorEqualityComparer.Strict);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_Dictionary_WithCapacityAndComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var dict = new Dictionary<Error, string>(10, ErrorEqualityComparer.Strict);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_HashSet_OfOtherType()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var set = new HashSet<string>();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_Dictionary_WithOtherKeyType()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var dict = new Dictionary<string, Error>();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_OtherCollectionTypes()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var list = new List<Error>();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT007_TriggersOn_Linq_Distinct_WithoutComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var distinct = errors.Distinct();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT007");
        Assert.Contains("'Distinct<Error>'", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT007_TriggersOn_Linq_Distinct_WithExplicitNullComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var distinct = errors.Distinct((IEqualityComparer<Error>)null);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT007");
        Assert.Contains("'Distinct<Error>'", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT007_TriggersOn_Linq_DistinctBy_WithoutComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var distinct = errors.DistinctBy(e => e.Code);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT007");
        Assert.Contains("'DistinctBy<Error>'", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT007_TriggersOn_Linq_DistinctBy_WithExplicitNullComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var distinct = errors.DistinctBy(e => e.Code, (IEqualityComparer<string>)null);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT007");
        Assert.Contains("'DistinctBy<Error>'", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT007_TriggersOn_Linq_GroupBy_WithoutComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var grouped = errors.GroupBy(e => e.Code);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT007");
        Assert.Contains("'GroupBy<Error>'", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT007_TriggersOn_Linq_GroupBy_WithExplicitNullComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var grouped = errors.GroupBy(e => e.Code, (IEqualityComparer<string>)null);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT007");
        Assert.Contains("'GroupBy<Error>'", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT007_TriggersOn_Linq_ToHashSet_WithoutComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var set = errors.ToHashSet();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT007");
        Assert.Contains("'ToHashSet<Error>'", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT007_TriggersOn_Linq_ToHashSet_WithExplicitNullComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var set = errors.ToHashSet((IEqualityComparer<Error>)null);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT007");
        Assert.Contains("'ToHashSet<Error>'", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_Linq_Distinct_WithComparer()
    {
        const string source = @"

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

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_Linq_DistinctBy_WithComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var distinct = errors.DistinctBy(e => e.Code, StringComparer.Ordinal);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_Linq_GroupBy_WithComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var grouped = errors.GroupBy(e => e.Code, StringComparer.Ordinal);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_Linq_ToHashSet_WithComparer()
    {
        const string source = @"

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var set = errors.ToHashSet(ErrorEqualityComparer.Strict);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_Linq_Distinct_OnNonErrorSequence()
    {
        const string source = @"

public class TestClass
{
    public void Method(IEnumerable<int> numbers)
    {
        var distinct = numbers.Distinct();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_CustomLinqMethod_WithSameName_OutsideSystemLinq()
    {
        const string source = @"

namespace MyExtensions
{
    public static class Ext
    {
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> src) => src;
    }
}

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var distinct = MyExtensions.Ext.Distinct(errors);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT007_DoesNotTriggerOn_OtherLinqMethods()
    {
        const string source = @"

public class TestClass
{
    public void Method(IEnumerable<Error> errors)
    {
        var filtered = errors.Where(e => e.Code != """").Select(e => e.Description);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<HashSetErrorEqualityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }
}



