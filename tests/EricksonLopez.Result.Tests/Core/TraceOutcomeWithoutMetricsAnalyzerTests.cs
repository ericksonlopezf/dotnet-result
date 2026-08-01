using System.Threading.Tasks;
using Xunit;
using EricksonLopez.Result.Analyzers;

namespace EricksonLopez.Result.Tests.Core;

public class TraceOutcomeWithoutMetricsAnalyzerTests
{
    [Fact]
    public async Task RESULT_OTEL_001_Triggers_When_Metrics_Omitted()
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
        Assert.Contains(diagnostics, d => d.Id == "RESULT_OTEL_001");
    }

    [Fact]
    public async Task RESULT_OTEL_001_Triggers_When_Metrics_Explicitly_Null()
    {
        const string source = @"
using EricksonLopez.Result;
using EricksonLopez.Result.OpenTelemetry;

public class TestClass
{
    public void Method()
    {
        var result = Result.Success();
        result.TraceOutcome(""Operation"", metrics: null);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TraceOutcomeWithoutMetricsAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT_OTEL_001");
    }

    [Fact]
    public async Task RESULT_OTEL_001_DoesNotTrigger_When_Metrics_Provided()
    {
        const string source = @"
using EricksonLopez.Result;
using EricksonLopez.Result.OpenTelemetry;
using System.Diagnostics.Metrics;

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
}
