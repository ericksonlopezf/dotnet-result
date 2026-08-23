// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result.Analyzers;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Result.Analyzers.Tests;

public class TraceOutcomeWithoutMetricsAnalyzerTests
{
    [Fact]
    public void RESULT_OTEL_001_Descriptor_Properties_AreAccurate()
    {
        var analyzer = new TraceOutcomeWithoutMetricsAnalyzer();
        var diagnostics = analyzer.SupportedDiagnostics;
        var rule = Assert.Single(diagnostics);

        Assert.Equal("RESULT_OTEL_001", rule.Id);
        Assert.Equal("TraceOutcome/TraceOnFailure/TraceOnSuccess called without metrics instance", rule.Title.ToString());
        Assert.Equal(
            "'{0}' is called without a 'metrics' argument — no ResultMetrics counters will be recorded. " +
            "Pass your DI-injected ResultMetrics instance via the 'metrics' parameter, or suppress this " +
            "hint if using static mode (ResultMetrics.StaticTrackSuccess/StaticTrackFailure) separately.",
            rule.MessageFormat.ToString());
        Assert.Equal("EricksonLopez.Result.OpenTelemetry", rule.Category);
        Assert.Equal(DiagnosticSeverity.Info, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal(
            "When TraceOutcome, TraceOnFailure, or TraceOnSuccess are called without the 'metrics' parameter, " +
            "only the Activity is annotated — no metrics counters are incremented. " +
            "If you use services.AddResultMetrics() (DI mode), you must pass the injected ResultMetrics " +
            "instance via the 'metrics' parameter to record metrics. " +
            "If you use static mode (ResultMetrics.StaticTrack*), suppress this hint.",
            rule.Description.ToString());
        Assert.Equal("https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md#RESULT_OTEL_001", rule.HelpLinkUri);
    }

    [Fact]
    public async Task RESULT_OTEL_001_Triggers_When_Metrics_Omitted_On_TraceOutcome()
    {
        const string source = @"
using EricksonLopez.Result;
using EricksonLopez.Result.OpenTelemetry;

public class TestClass
{
    public void Method()
    {
        var result = Result.Success();
        result.TraceOutcome(""Operation"");
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TraceOutcomeWithoutMetricsAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT_OTEL_001");
        Assert.Equal(DiagnosticSeverity.Info, diag.Severity);
    }

    [Fact]
    public async Task RESULT_OTEL_001_Triggers_When_Metrics_Omitted_On_TraceOnFailure()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var result = Result.Failure(Error.NotFound(""test"", ""test""));
        result.TraceOnFailure(""Operation"");
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TraceOutcomeWithoutMetricsAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT_OTEL_001");
        Assert.Equal(DiagnosticSeverity.Info, diag.Severity);
    }

    [Fact]
    public async Task RESULT_OTEL_001_Triggers_When_Metrics_Omitted_On_TraceOnSuccess()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var result = Result.Success();
        result.TraceOnSuccess(""Operation"");
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TraceOutcomeWithoutMetricsAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT_OTEL_001");
        Assert.Equal(DiagnosticSeverity.Info, diag.Severity);
    }

    [Fact]
    public async Task RESULT_OTEL_001_Triggers_When_Metrics_Explicitly_Null()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var result = Result.Success();
        result.TraceOutcome(""Operation"", metrics: null);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TraceOutcomeWithoutMetricsAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT_OTEL_001");
        Assert.Equal(DiagnosticSeverity.Info, diag.Severity);
    }

    [Fact]
    public async Task RESULT_OTEL_001_DoesNotTrigger_When_Metrics_Provided_As_NamedArgument()
    {
        const string source = @"

public class TestClass
{
    public void Method(ResultMetrics metrics)
    {
        var result = Result.Success();
        result.TraceOutcome(""Operation"", metrics: metrics);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TraceOutcomeWithoutMetricsAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT_OTEL_001");
    }

    [Fact]
    public async Task RESULT_OTEL_001_DoesNotTrigger_When_Metrics_Provided_As_PositionalArgument()
    {
        const string source = @"

public class TestClass
{
    public void Method(ResultMetrics metrics)
    {
        var result = Result.Success();
        result.TraceOutcome(""Operation"", null, metrics);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TraceOutcomeWithoutMetricsAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT_OTEL_001");
    }

    [Fact]
    public async Task RESULT_OTEL_001_DoesNotTrigger_On_OtherMethod_On_ResultActivityExtensions()
    {
        const string source = @"
namespace EricksonLopez.Result.OpenTelemetry
{
    public static class ResultActivityExtensions
    {
        public static void OtherMethod() {}
    }

    public class TestClass
    {
        public void Method()
        {
            ResultActivityExtensions.OtherMethod();
        }
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TraceOutcomeWithoutMetricsAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT_OTEL_001_DoesNotTrigger_On_UnrelatedMethod()
    {
        const string source = @"
public class TestClass
{
    public void OtherMethod(string operation) {}

    public void Method()
    {
        OtherMethod(""Operation"");
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TraceOutcomeWithoutMetricsAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT_OTEL_001_DoesNotTrigger_When_MethodNameMatches_But_TypeDiffers()
    {
        const string source = @"
public static class OtherClass
{
    public static void TraceOutcome(string operation) {}
}

public class TestClass
{
    public void Method()
    {
        OtherClass.TraceOutcome(""Operation"");
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TraceOutcomeWithoutMetricsAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT_OTEL_001_DoesNotTrigger_When_MethodNameMatches_And_ClassNameMatches_But_NamespaceDiffers()
    {
        const string source = @"
namespace OtherNamespace
{
    public static class ResultActivityExtensions
    {
        public static void TraceOutcome(string operation) {}
    }

    public class TestClass
    {
        public void Method()
        {
            ResultActivityExtensions.TraceOutcome(""Operation"");
        }
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TraceOutcomeWithoutMetricsAnalyzer>(source);
        Assert.Empty(diagnostics);
    }
}




