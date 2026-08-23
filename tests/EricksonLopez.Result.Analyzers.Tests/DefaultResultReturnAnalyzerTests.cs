// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading.Tasks;
using EricksonLopez.Result.Analyzers;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Result.Analyzers.Tests;

public class DefaultResultReturnAnalyzerTests
{
    [Fact]
    public void RESULT012_Descriptor_Properties_AreAccurate()
    {
        var analyzer = new DefaultResultReturnAnalyzer();
        var diagnostics = analyzer.SupportedDiagnostics;
        var rule = Assert.Single(diagnostics);

        Assert.Equal("RESULT012", rule.Id);
        Assert.Equal("Avoid returning default(Result) or default(Result<T>)", rule.Title.ToString());
        Assert.Equal("Usage", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal("Returning default produces an uninitialized Result state. Return Result.Success(...) or Result.Failure(...) instead.", rule.MessageFormat.ToString());
        Assert.Equal("An uninitialized Result struct evaluates to false and will throw InvalidOperationException when accessed by monadic operators. Always return an explicit Result.Success() or Result.Failure().", rule.Description.ToString());
        Assert.Equal("https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md#RESULT012", rule.HelpLinkUri);
    }

    [Fact]
    public async Task RESULT012_Triggers_When_Returning_Default_Result()
    {
        const string source = @"
namespace EricksonLopez.Result
{
    public readonly struct Result
    {
        public static Result Success() => new Result();
    }
}

public class TestService
{
    public EricksonLopez.Result.Result GetStatus()
    {
        return default;
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DefaultResultReturnAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT012");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
    }

    [Fact]
    public async Task RESULT012_Triggers_When_Returning_Default_ResultOfT()
    {
        const string source = @"
namespace EricksonLopez.Result
{
    public readonly struct Result<T>
    {
    }
}

public class TestService
{
    public EricksonLopez.Result.Result<int> GetCount()
    {
        return default(EricksonLopez.Result.Result<int>);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DefaultResultReturnAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT012");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
    }

    [Fact]
    public async Task RESULT012_DoesNotTrigger_When_Returning_Explicit_Value()
    {
        const string source = @"
namespace EricksonLopez.Result
{
    public readonly struct Result
    {
        public static Result Success() => new Result();
    }
}

public class TestService
{
    public EricksonLopez.Result.Result GetStatus()
    {
        return EricksonLopez.Result.Result.Success();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DefaultResultReturnAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT012");
    }

    [Fact]
    public async Task RESULT012_DoesNotTrigger_When_Method_IsVoid_Or_ReturnsNullOperation()
    {
        const string source = @"
public class TestService
{
    public void DoSomething()
    {
        return;
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DefaultResultReturnAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT012");
    }

    [Fact]
    public async Task RESULT012_DoesNotTrigger_When_Returning_Default_OfNonResultType()
    {
        const string source = @"
public class TestService
{
    public int GetNumber()
    {
        return default;
    }

    public string? GetText()
    {
        return default(string);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DefaultResultReturnAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT012");
    }

    [Fact]
    public async Task RESULT012_DoesNotTrigger_When_Type_IsGlobalNamespace_Result()
    {
        const string source = @"
public struct Result
{
}

public class TestService
{
    public Result GetResult()
    {
        return default;
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DefaultResultReturnAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT012");
    }

    [Fact]
    public async Task RESULT012_DoesNotTrigger_When_Namespace_DoesNotEndWith_Result()
    {
        const string source = @"
namespace OtherNamespace.Domain
{
    public struct Result
    {
    }
}

public class TestService
{
    public OtherNamespace.Domain.Result GetResult()
    {
        return default;
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DefaultResultReturnAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT012");
    }

    [Fact]
    public async Task RESULT012_DoesNotTrigger_When_Type_NameIsNot_Result()
    {
        const string source = @"
namespace EricksonLopez.Result
{
    public class Error
    {
    }
}

public class TestService
{
    public EricksonLopez.Result.Error? GetError()
    {
        return default;
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DefaultResultReturnAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT012");
    }
}
