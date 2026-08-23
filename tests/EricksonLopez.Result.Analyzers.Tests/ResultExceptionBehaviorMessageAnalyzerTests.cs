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

public class ResultExceptionBehaviorMessageAnalyzerTests
{
    [Fact]
    public void RESULT010_Descriptor_Properties_AreAccurate()
    {
        var analyzer = new ResultExceptionBehaviorMessageAnalyzer();
        var diagnostics = analyzer.SupportedDiagnostics;
        var rule = Assert.Single(diagnostics);

        Assert.Equal("RESULT010", rule.Id);
        Assert.Equal("Avoid using Exception.Message in ResultExceptionBehavior", rule.Title.ToString());
        Assert.Equal(
            "Using Exception.Message in errorFactory may expose sensitive internal details (connection strings, paths) in production. Use a static message or sanitize the exception.",
            rule.MessageFormat.ToString());
        Assert.Equal("Security", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal(
            "Directly exposing Exception.Message into the Result Error description can leak PII or internal system details if returned via HTTP responses. Use a safe static description instead.",
            rule.Description.ToString());
        Assert.Equal("https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md#RESULT010", rule.HelpLinkUri);
    }

    [Fact]
    public async Task RESULT010_Triggers_When_ExceptionMessage_UsedInLambda()
    {
        const string source = @"

public static class ResultMediatRExtensions
{
    public static void AddResultExceptionBehavior(object services, Func<Exception, string> errorFactory) {}
}

public class TestClass
{
    public void Configure(object services)
    {
        ResultMediatRExtensions.AddResultExceptionBehavior(services, ex => ex.Message);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ResultExceptionBehaviorMessageAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT010");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
    }

    [Fact]
    public async Task RESULT010_Triggers_When_CustomExceptionMessage_UsedInLambda()
    {
        const string source = @"

public class CustomException : Exception {}

public static class ResultMediatRExtensions
{
    public static void AddResultExceptionBehavior(object services, Func<CustomException, string> errorFactory) {}
}

public class TestClass
{
    public void Configure(object services)
    {
        ResultMediatRExtensions.AddResultExceptionBehavior(services, ex => ex.Message);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ResultExceptionBehaviorMessageAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT010");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
    }

    [Fact]
    public async Task RESULT010_Triggers_When_NestedPropertyAccess_On_ExceptionMessage()
    {
        const string source = @"

public class Wrapper
{
    public string Text { get; set; }
}

public static class ResultMediatRExtensions
{
    public static void AddResultExceptionBehavior(object services, Func<Exception, string> errorFactory) {}
}

public class TestClass
{
    public void Configure(object services)
    {
        ResultMediatRExtensions.AddResultExceptionBehavior(services, ex => new Wrapper { Text = ex.Message }.Text);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ResultExceptionBehaviorMessageAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT010");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
    }

    [Fact]
    public async Task RESULT010_DoesNotTrigger_When_StaticString_UsedInLambda()
    {
        const string source = @"

public static class ResultMediatRExtensions
{
    public static void AddResultExceptionBehavior(object services, Func<Exception, string> errorFactory) {}
}

public class TestClass
{
    public void Configure(object services)
    {
        ResultMediatRExtensions.AddResultExceptionBehavior(services, ex => ""An internal error occurred."");
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ResultExceptionBehaviorMessageAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT010_DoesNotTrigger_When_OtherProperty_OnException_Used()
    {
        const string source = @"

public static class ResultMediatRExtensions
{
    public static void AddResultExceptionBehavior(object services, Func<Exception, string> errorFactory) {}
}

public class TestClass
{
    public void Configure(object services)
    {
        ResultMediatRExtensions.AddResultExceptionBehavior(services, ex => ex.GetType().Name);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ResultExceptionBehaviorMessageAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT010_DoesNotTrigger_When_MessageProperty_OnNonExceptionType_Used()
    {
        const string source = @"

public class CustomDto
{
    public string Message => ""safe"";
}

public static class ResultMediatRExtensions
{
    public static void AddResultExceptionBehavior(object services, Func<Exception, string> errorFactory) {}
}

public class TestClass
{
    public void Configure(object services)
    {
        var dto = new CustomDto();
        ResultMediatRExtensions.AddResultExceptionBehavior(services, ex => dto.Message);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ResultExceptionBehaviorMessageAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT010_DoesNotTrigger_On_OtherMethodName()
    {
        const string source = @"

public static class ResultMediatRExtensions
{
    public static void AddOtherBehavior(object services, Func<Exception, string> errorFactory) {}
}

public class TestClass
{
    public void Configure(object services)
    {
        ResultMediatRExtensions.AddOtherBehavior(services, ex => ex.Message);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ResultExceptionBehaviorMessageAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT010_DoesNotTrigger_On_OtherContainingType()
    {
        const string source = @"

public static class OtherExtensions
{
    public static void AddResultExceptionBehavior(object services, Func<Exception, string> errorFactory) {}
}

public class TestClass
{
    public void Configure(object services)
    {
        OtherExtensions.AddResultExceptionBehavior(services, ex => ex.Message);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ResultExceptionBehaviorMessageAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT010_DoesNotTrigger_When_No_errorFactory_Parameter_Passed()
    {
        const string source = @"
public static class ResultMediatRExtensions
{
    public static void AddResultExceptionBehavior(object services) {}
}

public class TestClass
{
    public void Configure(object services)
    {
        ResultMediatRExtensions.AddResultExceptionBehavior(services);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ResultExceptionBehaviorMessageAnalyzer>(source);
        Assert.Empty(diagnostics);
    }
}





