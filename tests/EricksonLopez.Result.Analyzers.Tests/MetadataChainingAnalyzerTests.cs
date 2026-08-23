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

public class MetadataChainingAnalyzerTests
{
    [Fact]
    public void RESULT005_Descriptor_RuleError_Properties_AreAccurate()
    {
        var analyzer = new MetadataChainingAnalyzer();
        var diagnostics = analyzer.SupportedDiagnostics;
        var ruleError = Assert.Single(diagnostics, d => d.Title.ToString().Contains("Error.WithMetadata"));

        Assert.Equal("RESULT005", ruleError.Id);
        Assert.Equal("Chained Error.WithMetadata() calls create multiple Error copies", ruleError.Title.ToString());
        Assert.Equal("{0} chained Error.WithMetadata() calls create {0} intermediate Error heap copies; use WithMetadata(IReadOnlyDictionary<string, object?>) or ToBuilder() to apply all entries in a single allocation", ruleError.MessageFormat.ToString());
        Assert.Equal("Performance", ruleError.Category);
        Assert.Equal(DiagnosticSeverity.Warning, ruleError.DefaultSeverity);
        Assert.True(ruleError.IsEnabledByDefault);
        Assert.Equal(
            "Error is an immutable class. Each Error.WithMetadata(string, object) call returns a new Error " +
            "with the entry added, creating N intermediate copies for N chained calls. When 3 or more " +
            "WithMetadata calls are chained, use the batch overload WithMetadata(IReadOnlyDictionary<string, object?>) " +
            "or ToBuilder() to apply all entries efficiently in a single allocation.",
            ruleError.Description.ToString());
        Assert.Equal("https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/error-builder.md#batch-metadata", ruleError.HelpLinkUri);
    }

    [Fact]
    public void RESULT005_Descriptor_RuleBuilder_Properties_AreAccurate()
    {
        var analyzer = new MetadataChainingAnalyzer();
        var diagnostics = analyzer.SupportedDiagnostics;
        var ruleBuilder = Assert.Single(diagnostics, d => d.Title.ToString().Contains("ErrorBuilder.WithMetadata"));

        Assert.Equal("RESULT005", ruleBuilder.Id);
        Assert.Equal("Chained ErrorBuilder.WithMetadata() calls create multiple ImmutableDictionary mutations", ruleBuilder.Title.ToString());
        Assert.Equal("{0} chained ErrorBuilder.WithMetadata() calls each perform an O(log k) AVL-tree mutation; use WithMetadata(IReadOnlyDictionary<string, object?>) or WithMetadata(IEnumerable<KeyValuePair<string, object>>) to batch all entries in a single AddRange call", ruleBuilder.MessageFormat.ToString());
        Assert.Equal("Performance", ruleBuilder.Category);
        Assert.Equal(DiagnosticSeverity.Warning, ruleBuilder.DefaultSeverity);
        Assert.True(ruleBuilder.IsEnabledByDefault);
        Assert.Equal(
            "ErrorBuilder is a readonly struct. Each ErrorBuilder.WithMetadata(string, object?) call performs " +
            "an O(log k) mutation on the backing ImmutableDictionary, creating N intermediate dictionary nodes " +
            "for N chained calls. When 3 or more WithMetadata calls are chained, use the batch overload " +
            "WithMetadata(IReadOnlyDictionary<string, object?>) or WithMetadata(IEnumerable<KeyValuePair<string, object>>) " +
            "to apply all entries with a single AddRange call.",
            ruleBuilder.Description.ToString());
        Assert.Equal("https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/error-builder.md#batch-metadata", ruleBuilder.HelpLinkUri);
    }

    [Fact]
    public async Task RESULT005_TriggersOn_Error_WithMetadata_Chained3Times()
    {
        const string source = @"

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
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT005");
        Assert.Equal("Chained Error.WithMetadata() calls create multiple Error copies", diag.Descriptor.Title.ToString());
        Assert.Equal("3 chained Error.WithMetadata() calls create 3 intermediate Error heap copies; use WithMetadata(IReadOnlyDictionary<string, object?>) or ToBuilder() to apply all entries in a single allocation", diag.GetMessage());
        var sourceText = diag.Location.SourceTree!.GetText().GetSubText(diag.Location.SourceSpan).ToString();
        Assert.EndsWith(".WithMetadata(\"c\", 3)", sourceText.Trim());
    }

    [Fact]
    public async Task RESULT005_TriggersOn_Error_WithMetadata_Chained4Times()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var error = Error.Create(""code"", ""desc"").Build()
            .WithMetadata(""a"", 1)
            .WithMetadata(""b"", 2)
            .WithMetadata(""c"", 3)
            .WithMetadata(""d"", 4);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT005");
        Assert.Equal("Chained Error.WithMetadata() calls create multiple Error copies", diag.Descriptor.Title.ToString());
        Assert.Equal("4 chained Error.WithMetadata() calls create 4 intermediate Error heap copies; use WithMetadata(IReadOnlyDictionary<string, object?>) or ToBuilder() to apply all entries in a single allocation", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT005_TriggersOn_ErrorBuilder_WithMetadata_Chained3Times()
    {
        const string source = @"

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
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT005");
        Assert.Equal("Chained ErrorBuilder.WithMetadata() calls create multiple ImmutableDictionary mutations", diag.Descriptor.Title.ToString());
        Assert.Equal("3 chained ErrorBuilder.WithMetadata() calls each perform an O(log k) AVL-tree mutation; use WithMetadata(IReadOnlyDictionary<string, object?>) or WithMetadata(IEnumerable<KeyValuePair<string, object>>) to batch all entries in a single AddRange call", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT005_TriggersOn_ErrorBuilder_WithMetadata_Chained5Times()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"")
            .WithMetadata(""a"", 1)
            .WithMetadata(""b"", 2)
            .WithMetadata(""c"", 3)
            .WithMetadata(""d"", 4)
            .WithMetadata(""e"", 5);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT005");
        Assert.Equal("Chained ErrorBuilder.WithMetadata() calls create multiple ImmutableDictionary mutations", diag.Descriptor.Title.ToString());
        Assert.Equal("5 chained ErrorBuilder.WithMetadata() calls each perform an O(log k) AVL-tree mutation; use WithMetadata(IReadOnlyDictionary<string, object?>) or WithMetadata(IEnumerable<KeyValuePair<string, object>>) to batch all entries in a single AddRange call", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT005_DoesNotTrigger_On_Error_WithMetadata_Chained1Time()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var error = Error.Create(""code"", ""desc"").Build()
            .WithMetadata(""a"", 1);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT005");
    }

    [Fact]
    public async Task RESULT005_DoesNotTrigger_On_Error_WithMetadata_Chained2Times()
    {
        const string source = @"

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
    public async Task RESULT005_DoesNotTrigger_On_ErrorBuilder_WithMetadata_Chained1Time()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"")
            .WithMetadata(""a"", 1);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT005");
    }

    [Fact]
    public async Task RESULT005_DoesNotTrigger_On_ErrorBuilder_WithMetadata_Chained2Times()
    {
        const string source = @"

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

    [Fact]
    public async Task RESULT005_DoesNotTrigger_On_NonWithMetadata_Methods()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"")
            .WithType(ErrorType.Validation)
            .WithSeverity(ErrorSeverity.Critical)
            .WithTraceId(""trace-123"")
            .Build();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT005_DoesNotTrigger_When_FirstParameterIsNotString()
    {
        const string source = @"
public class CustomClass
{
    public CustomClass WithMetadata(int key, object value) => this;

    public void Method()
    {
        var custom = new CustomClass()
            .WithMetadata(1, ""a"")
            .WithMetadata(2, ""b"")
            .WithMetadata(3, ""c"");
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT005_DoesNotTrigger_When_ContainingTypeIsNotErrorOrErrorBuilder()
    {
        const string source = @"
public class CustomClass
{
    public CustomClass WithMetadata(string key, object value) => this;

    public void Method()
    {
        var custom = new CustomClass()
            .WithMetadata(""a"", 1)
            .WithMetadata(""b"", 2)
            .WithMetadata(""c"", 3);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT005_MixedTypesInChain_ResetsCount()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var error = Error.Create(""code"", ""desc"")
            .WithMetadata(""a"", 1)
            .WithMetadata(""b"", 2)
            .Build()
            .WithMetadata(""c"", 3)
            .WithMetadata(""d"", 4);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT005_ParentInvocation_WithSingleParameter_DoesNotBlockOutermost()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var dict = new Dictionary<string, object?> { { ""d"", 4 } };
        var builder = Error.Create(""code"", ""desc"")
            .WithMetadata(""a"", 1)
            .WithMetadata(""b"", 2)
            .WithMetadata(""c"", 3)
            .WithMetadata(dict);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT005");
        Assert.Contains("3 chained ErrorBuilder.WithMetadata()", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT005_ParentInvocation_OnDifferentType_DoesNotBlockOutermost()
    {
        const string source = @"

public class OtherBuilder
{
    public static OtherBuilder Wrap(ErrorBuilder b) => new();
    public OtherBuilder WithMetadata(string key, object value) => this;
}

public class TestClass
{
    public void Method()
    {
        var builder = OtherBuilder.Wrap(
            Error.Create(""code"", ""desc"")
                .WithMetadata(""a"", 1)
                .WithMetadata(""b"", 2)
                .WithMetadata(""c"", 3)
        ).WithMetadata(""d"", 4);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT005");
        Assert.Contains("3 chained ErrorBuilder.WithMetadata()", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT005_ParentInvocation_WithNonStringFirstParam_DoesNotBlockOutermost()
    {
        const string source = @"

public static class Extensions
{
    public static ErrorBuilder WithMetadata(this ErrorBuilder b, int key, object value) => b;
}

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"")
            .WithMetadata(""a"", 1)
            .WithMetadata(""b"", 2)
            .WithMetadata(""c"", 3)
            .WithMetadata(4, ""d"");
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT005");
        Assert.Contains("3 chained ErrorBuilder.WithMetadata()", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT005_Receiver_IsBatchOverload_CountsOnlyTwoParamCalls()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var dict = new Dictionary<string, object?> { { ""base"", 0 } };
        var builder = Error.Create(""code"", ""desc"")
            .WithMetadata(dict)
            .WithMetadata(""a"", 1)
            .WithMetadata(""b"", 2)
            .WithMetadata(""c"", 3);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT005");
        Assert.Contains("3 chained ErrorBuilder.WithMetadata()", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT005_DoesNotTrigger_When_OuterCallIsNotNamedWithMetadata()
    {
        const string source = @"

public static class Extensions
{
    public static Error Foo(this Error e, string key, object value) => e;
}

public class TestClass
{
    public void Method()
    {
        var error = Error.Create(""code"", ""desc"").Build()
            .WithMetadata(""a"", 1)
            .WithMetadata(""b"", 2)
            .Foo(""c"", 3);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT005_DoesNotTrigger_When_OuterCallHasThreeParameters()
    {
        const string source = @"

public static class Extensions
{
    public static Error WithMetadata(this Error e, string key, object value, bool overwrite) => e;
}

public class TestClass
{
    public void Method()
    {
        var error = Error.Create(""code"", ""desc"").Build()
            .WithMetadata(""a"", 1)
            .WithMetadata(""b"", 2)
            .WithMetadata(""c"", 3, true);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT005_DoesNotTrigger_When_OuterCallHasNonStringFirstParameter()
    {
        const string source = @"

public static class Extensions
{
    public static ErrorBuilder WithMetadata(this ErrorBuilder b, int key, object value) => b;
}

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"")
            .WithMetadata(""a"", 1)
            .WithMetadata(""b"", 2)
            .WithMetadata(3, ""c"");
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MetadataChainingAnalyzer>(source);
        Assert.Empty(diagnostics);
    }
}



