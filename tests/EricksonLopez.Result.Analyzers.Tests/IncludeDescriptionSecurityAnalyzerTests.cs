// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Analyzers;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Result.Analyzers.Tests;

public class IncludeDescriptionSecurityAnalyzerTests
{
    [Fact]
    public void RESULT009_Descriptor_Properties_AreAccurate()
    {
        var analyzer = new IncludeDescriptionSecurityAnalyzer();
        var diagnostics = analyzer.SupportedDiagnostics;
        var rule = Assert.Single(diagnostics);

        Assert.Equal("RESULT009", rule.Id);
        Assert.Equal("ResultHttpOptions.IncludeDescription set to true without environment guard", rule.Title.ToString());
        Assert.Equal(
            "Setting 'IncludeDescription = true' unconditionally may expose internal error descriptions " +
            "(exception messages, paths, PII) in HTTP ProblemDetails responses in production. " +
            "Use 'options.IncludeDescriptionInDevelopment(env)' or 'options.IncludeDescription = env.IsDevelopment()' instead.",
            rule.MessageFormat.ToString());
        Assert.Equal("Security", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal(
            "ResultHttpOptions.IncludeDescription = true causes error descriptions to be included in " +
            "the HTTP ProblemDetails body in all environments, including production. " +
            "This can expose sensitive data such as exception messages, file system paths, " +
            "database connection strings, and PII. " +
            "Use IncludeDescriptionInDevelopment(env) to safely restrict exposure to development only.",
            rule.Description.ToString());
        Assert.Equal("https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md#RESULT009", rule.HelpLinkUri);
    }

    [Fact]
    public async Task RESULT009_Triggers_On_IncludeDescription_AssignedLiteralTrue()
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
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT009");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
    }

    [Fact]
    public async Task RESULT009_DoesNotTrigger_On_IncludeDescription_AssignedLiteralFalse()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var options = new ResultHttpOptions();
        options.IncludeDescription = false;
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<IncludeDescriptionSecurityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT009_DoesNotTrigger_On_IncludeDescription_AssignedVariable()
    {
        const string source = @"

public class TestClass
{
    public void Method(bool include)
    {
        var options = new ResultHttpOptions();
        options.IncludeDescription = include;
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<IncludeDescriptionSecurityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT009_DoesNotTrigger_On_IncludeDescription_AssignedMethodCall()
    {
        const string source = @"

public class TestClass
{
    private bool IsDev() => true;

    public void Method()
    {
        var options = new ResultHttpOptions();
        options.IncludeDescription = IsDev();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<IncludeDescriptionSecurityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT009_DoesNotTrigger_On_OtherProperty_NamedIncludeDescription_OnOtherType()
    {
        const string source = @"
public class OtherOptions
{
    public bool IncludeDescription { get; set; }
}

public class TestClass
{
    public void Method()
    {
        var options = new OtherOptions();
        options.IncludeDescription = true;
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<IncludeDescriptionSecurityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT009_DoesNotTrigger_On_OtherProperty_OnResultHttpOptions()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var options = new ResultHttpOptions();
        options.DefaultSuccessStatusCode = 200;
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<IncludeDescriptionSecurityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT009_DoesNotTrigger_On_NonPropertyAssignment()
    {
        const string source = @"
public class TestClass
{
    public void Method()
    {
        bool local = false;
        local = true;
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<IncludeDescriptionSecurityAnalyzer>(source);
        Assert.Empty(diagnostics);
    }
}





