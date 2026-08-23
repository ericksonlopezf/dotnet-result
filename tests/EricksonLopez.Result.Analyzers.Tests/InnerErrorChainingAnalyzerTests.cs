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

public class InnerErrorChainingAnalyzerTests
{
    [Fact]
    public void RESULT006_Descriptor_Properties_AreAccurate()
    {
        var analyzer = new InnerErrorChainingAnalyzer();
        var diagnostics = analyzer.SupportedDiagnostics;
        var rule = Assert.Single(diagnostics);

        Assert.Equal("RESULT006", rule.Id);
        Assert.Equal("Chained ErrorBuilder.WithInnerError() calls are O(n\u00b2)", rule.Title.ToString());
        Assert.Equal("{0} chained ErrorBuilder.WithInnerError() calls cause O(n\u00b2) ImmutableArray copying; use WithInnerErrors(IEnumerable<Error>) to add all inner errors in a single O(n) operation", rule.MessageFormat.ToString());
        Assert.Equal("Performance", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal(
            "ErrorBuilder is a readonly struct with copy-on-write semantics. Each WithInnerError(Error) call " +
            "creates a new ImmutableArray<Error> with one additional element - an O(n) copy per call. " +
            "Chaining N calls produces O(n\u00b2) total copying. When adding 2 or more inner errors, use " +
            "WithInnerErrors(IEnumerable<Error>) to create the ImmutableArray once in O(n).",
            rule.Description.ToString());
        Assert.Equal("https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/error-builder.md#inner-errors", rule.HelpLinkUri);
    }

    [Fact]
    public async Task RESULT006_TriggersOn_Chained_WithInnerError_2Times()
    {
        const string source = @"

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
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT006");
        Assert.Equal("Chained ErrorBuilder.WithInnerError() calls are O(n\u00b2)", diag.Descriptor.Title.ToString());
        Assert.Equal("2 chained ErrorBuilder.WithInnerError() calls cause O(n\u00b2) ImmutableArray copying; use WithInnerErrors(IEnumerable<Error>) to add all inner errors in a single O(n) operation", diag.GetMessage());
        var sourceText = diag.Location.SourceTree!.GetText().GetSubText(diag.Location.SourceSpan).ToString();
        Assert.EndsWith(".WithInnerError(e2)", sourceText.Trim());
    }

    [Fact]
    public async Task RESULT006_TriggersOn_Chained_WithInnerError_3Times()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        var e2 = Error.Create(""e2"", ""2"").Build();
        var e3 = Error.Create(""e3"", ""3"").Build();
        
        var builder = Error.Create(""code"", ""desc"")
            .WithInnerError(e1)
            .WithInnerError(e2)
            .WithInnerError(e3);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT006");
        Assert.Equal("3 chained ErrorBuilder.WithInnerError() calls cause O(n\u00b2) ImmutableArray copying; use WithInnerErrors(IEnumerable<Error>) to add all inner errors in a single O(n) operation", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT006_TriggersOn_Chained_WithInnerError_4Times()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        var e2 = Error.Create(""e2"", ""2"").Build();
        var e3 = Error.Create(""e3"", ""3"").Build();
        var e4 = Error.Create(""e4"", ""4"").Build();
        
        var builder = Error.Create(""code"", ""desc"")
            .WithInnerError(e1)
            .WithInnerError(e2)
            .WithInnerError(e3)
            .WithInnerError(e4);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT006");
        Assert.Equal("4 chained ErrorBuilder.WithInnerError() calls cause O(n\u00b2) ImmutableArray copying; use WithInnerErrors(IEnumerable<Error>) to add all inner errors in a single O(n) operation", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT006_TriggersOn_Chained_WithInnerError_5Times()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        var e2 = Error.Create(""e2"", ""2"").Build();
        var e3 = Error.Create(""e3"", ""3"").Build();
        var e4 = Error.Create(""e4"", ""4"").Build();
        var e5 = Error.Create(""e5"", ""5"").Build();
        
        var builder = Error.Create(""code"", ""desc"")
            .WithInnerError(e1)
            .WithInnerError(e2)
            .WithInnerError(e3)
            .WithInnerError(e4)
            .WithInnerError(e5);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT006");
        Assert.Equal("5 chained ErrorBuilder.WithInnerError() calls cause O(n\u00b2) ImmutableArray copying; use WithInnerErrors(IEnumerable<Error>) to add all inner errors in a single O(n) operation", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT006_DoesNotTrigger_When_Single_WithInnerError()
    {
        const string source = @"

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
        const string source = @"

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

    [Fact]
    public async Task RESULT006_DoesNotTrigger_When_ContainingTypeIsNotErrorBuilder()
    {
        const string source = @"

public class CustomClass
{
    public CustomClass WithInnerError(Error e) => this;

    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        var e2 = Error.Create(""e2"", ""2"").Build();
        
        var custom = new CustomClass()
            .WithInnerError(e1)
            .WithInnerError(e2);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT006_DoesNotTrigger_When_OuterCallIsOnDifferentContainingType()
    {
        const string source = @"

public static class CustomExtensions
{
    public static ErrorBuilder CustomInner(this ErrorBuilder b, Error e) => b;
}

public class TestClass
{
    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        var e2 = Error.Create(""e2"", ""2"").Build();
        
        var builder = Error.Create(""code"", ""desc"")
            .WithInnerError(e1)
            .CustomInner(e2);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT006_DoesNotTrigger_When_OuterCallIsOneParamMethodWithDifferentName()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        
        var builder = Error.Create(""code"", ""desc"")
            .WithInnerError(e1)
            .WithType(ErrorType.Validation);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT006_DoesNotTrigger_When_MethodNameIsNotWithInnerError()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"")
            .WithType(ErrorType.Validation)
            .WithSeverity(ErrorSeverity.Critical);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT006_DoesNotTrigger_When_MethodHasDifferentParameters()
    {
        const string source = @"

public static class Extensions
{
    public static ErrorBuilder WithInnerError(this ErrorBuilder b, Error e, bool flag) => b;
}

public class TestClass
{
    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        var e2 = Error.Create(""e2"", ""2"").Build();
        
        var builder = Error.Create(""code"", ""desc"")
            .WithInnerError(e1, true)
            .WithInnerError(e2, false);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT006_ParentInvocation_OnDifferentType_DoesNotBlockOutermost()
    {
        const string source = @"

public class OtherBuilder
{
    public static OtherBuilder Wrap(ErrorBuilder b) => new();
    public OtherBuilder WithInnerError(Error e) => this;
}

public class TestClass
{
    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        var e2 = Error.Create(""e2"", ""2"").Build();
        var e3 = Error.Create(""e3"", ""3"").Build();
        
        var builder = OtherBuilder.Wrap(
            Error.Create(""code"", ""desc"")
                .WithInnerError(e1)
                .WithInnerError(e2)
        ).WithInnerError(e3);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT006");
        Assert.Equal("2 chained ErrorBuilder.WithInnerError() calls cause O(n\u00b2) ImmutableArray copying; use WithInnerErrors(IEnumerable<Error>) to add all inner errors in a single O(n) operation", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT006_ParentInvocation_WithDifferentParameterCount_DoesNotBlockOutermost()
    {
        const string source = @"

public static class Extensions
{
    public static ErrorBuilder WithInnerError(this ErrorBuilder b, Error e, bool flag) => b;
}

public class TestClass
{
    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        var e2 = Error.Create(""e2"", ""2"").Build();
        var e3 = Error.Create(""e3"", ""3"").Build();
        
        var builder = Error.Create(""code"", ""desc"")
            .WithInnerError(e1)
            .WithInnerError(e2)
            .WithInnerError(e3, true);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT006");
        Assert.Equal("2 chained ErrorBuilder.WithInnerError() calls cause O(n\u00b2) ImmutableArray copying; use WithInnerErrors(IEnumerable<Error>) to add all inner errors in a single O(n) operation", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT006_Receiver_IsOtherMethod_CountsOnlyWithInnerError()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        var e2 = Error.Create(""e2"", ""2"").Build();
        
        var builder = Error.Create(""code"", ""desc"")
            .WithType(ErrorType.Validation)
            .WithInnerError(e1)
            .WithInnerError(e2);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT006");
        Assert.Equal("2 chained ErrorBuilder.WithInnerError() calls cause O(n\u00b2) ImmutableArray copying; use WithInnerErrors(IEnumerable<Error>) to add all inner errors in a single O(n) operation", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT006_Chain_Interrupted_By_Build_ResetsCount()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var e1 = Error.Create(""e1"", ""1"").Build();
        var e2 = Error.Create(""e2"", ""2"").Build();
        
        var builder = Error.Create(""code"", ""desc"")
            .WithInnerError(e1)
            .Build()
            .ToBuilder()
            .WithInnerError(e2);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<InnerErrorChainingAnalyzer>(source);
        Assert.Empty(diagnostics);
    }
}



