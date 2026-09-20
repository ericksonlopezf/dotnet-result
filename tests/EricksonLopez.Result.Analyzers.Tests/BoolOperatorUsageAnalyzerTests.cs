// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading.Tasks;
using EricksonLopez.Result.Analyzers;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Result.Analyzers.Tests;

public class BoolOperatorUsageAnalyzerTests
{
    [Fact]
    public void RESULT013_Descriptor_Properties_AreAccurate()
    {
        var analyzer = new BoolOperatorUsageAnalyzer();
        var diagnostics = analyzer.SupportedDiagnostics;
        var rule = Assert.Single(diagnostics);

        Assert.Equal("RESULT013", rule.Id);
        Assert.Equal("Avoid implicit bool conversion of Result in condition contexts", rule.Title.ToString());
        Assert.Equal("Usage", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Contains("implicit boolean conversion", rule.MessageFormat.ToString());
        Assert.Contains("uninitialized", rule.Description.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md#RESULT013", rule.HelpLinkUri);
    }

    [Fact]
    public async Task RESULT013_Triggers_On_If_Statement_With_Result()
    {
        const string source = @"
public class TestService
{
    public void Check(EricksonLopez.Result.Result result)
    {
        if (result)
        {
            int x = 1;
        }
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BoolOperatorUsageAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT013");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
        Assert.Contains("result", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT013_Triggers_On_If_Statement_With_Cast_ResultOfT()
    {
        const string source = @"
public class TestService
{
    public void Check(EricksonLopez.Result.Result<int> result)
    {
        if ((EricksonLopez.Result.Result)result)
        {
            int x = 1;
        }
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BoolOperatorUsageAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT013");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
    }

    [Fact]
    public async Task RESULT013_Triggers_On_Conditional_Ternary_Expression()
    {
        const string source = @"
public class TestService
{
    public string Check(EricksonLopez.Result.Result result)
    {
        return result ? ""yes"" : ""no"";
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BoolOperatorUsageAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT013");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
    }

    [Fact]
    public async Task RESULT013_Triggers_On_While_Statement()
    {
        const string source = @"
public class TestService
{
    public void Loop(EricksonLopez.Result.Result result)
    {
        while (result)
        {
            break;
        }
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BoolOperatorUsageAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT013");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
    }

    [Fact]
    public async Task RESULT013_Triggers_On_DoWhile_Statement()
    {
        const string source = @"
public class TestService
{
    public void Loop(EricksonLopez.Result.Result result)
    {
        do
        {
            break;
        } while (result);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BoolOperatorUsageAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT013");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
    }

    [Fact]
    public async Task RESULT013_DoesNotTrigger_When_Using_IsSuccess()
    {
        const string source = @"
public class TestService
{
    public void Check(EricksonLopez.Result.Result result)
    {
        if (result.IsSuccess)
        {
            int x = 1;
        }
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BoolOperatorUsageAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT013");
    }

    [Fact]
    public async Task RESULT013_DoesNotTrigger_When_Using_IsFailure()
    {
        const string source = @"
public class TestService
{
    public void Check(EricksonLopez.Result.Result result)
    {
        if (result.IsFailure)
        {
            int x = 1;
        }
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BoolOperatorUsageAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT013");
    }

    [Fact]
    public async Task RESULT013_DoesNotTrigger_On_Boolean_Variable()
    {
        const string source = @"
public class TestService
{
    public void Check(bool isValid)
    {
        if (isValid)
        {
            int x = 1;
        }
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BoolOperatorUsageAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT013");
    }

    [Fact]
    public async Task RESULT013_CodeFix_Rewrites_If_To_IsSuccess()
    {
        const string source = @"
public class TestService
{
    public void Check(EricksonLopez.Result.Result result)
    {
        if (result)
        {
            int x = 1;
        }
    }
}";
        var fixedCode = await AnalyzerTestHelper.ApplyCodeFixAsync<BoolOperatorUsageAnalyzer, BoolOperatorUsageCodeFix>(source, "RESULT013");
        Assert.Contains("if (result.IsSuccess)", fixedCode);
        Assert.DoesNotContain("if (result)\n", fixedCode);
    }

    [Fact]
    public async Task RESULT013_CodeFix_Offers_Both_IsSuccess_And_IsFailure_Actions()
    {
        const string source = @"
public class TestService
{
    public void Check(EricksonLopez.Result.Result result)
    {
        if (result)
        {
            int x = 1;
        }
    }
}";
        var actions = await AnalyzerTestHelper.GetCodeActionsAsync<BoolOperatorUsageAnalyzer, BoolOperatorUsageCodeFix>(source, "RESULT013");
        Assert.Equal(2, actions.Length);
        Assert.Contains(actions, a => a.Title == "Use .IsSuccess");
        Assert.Contains(actions, a => a.Title == "Use .IsFailure");
    }

    [Fact]
    public async Task RESULT013_CodeFix_Rewrites_Parenthesized_Expression_Without_Double_Wrapping()
    {
        const string source = @"
public class TestService
{
    public void Check(EricksonLopez.Result.Result result)
    {
        if ((result))
        {
            int x = 1;
        }
    }
}";
        var fixedCode = await AnalyzerTestHelper.ApplyCodeFixAsync<BoolOperatorUsageAnalyzer, BoolOperatorUsageCodeFix>(source, "RESULT013");
        Assert.Contains("(result.IsSuccess)", fixedCode);
    }

    [Fact]
    public async Task RESULT013_CodeFix_Rewrites_Cast_Expression_With_Parentheses()
    {
        const string source = @"
public class TestService
{
    public void Check(EricksonLopez.Result.Result<int> result)
    {
        if ((EricksonLopez.Result.Result)result)
        {
            int x = 1;
        }
    }
}";
        var fixedCode = await AnalyzerTestHelper.ApplyCodeFixAsync<BoolOperatorUsageAnalyzer, BoolOperatorUsageCodeFix>(source, "RESULT013");
        Assert.Contains("((EricksonLopez.Result.Result)result).IsSuccess", fixedCode);
    }
}
