using System.Threading.Tasks;
using Xunit;
using EricksonLopez.Result.Analyzers;

namespace EricksonLopez.Result.Analyzers.Tests;

public class EndpointFilterOpenApiAnalyzerTests
{
    [Fact]
    public async Task RESULT008_TriggersOn_AddResultEndpointFilter_Without_Produces()
    {
        const string source = @"
using System;

namespace EricksonLopez.Result.AspNetCore
{
    public static class Extensions
    {
        public static object AddResultEndpointFilter(this object endpoint) => endpoint;
    }
}

public class TestClass
{
    public void Configure(object endpoint)
    {
        EricksonLopez.Result.AspNetCore.Extensions.AddResultEndpointFilter(endpoint);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<EndpointFilterOpenApiAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT008");
    }

    [Fact]
    public async Task RESULT008_DoesNotTrigger_When_Produces_Follows()
    {
        const string source = @"
using System;

namespace EricksonLopez.Result.AspNetCore
{
    public static class Extensions
    {
        public static object AddResultEndpointFilter(this object endpoint) => endpoint;
        public static object Produces<T>(this object endpoint) => endpoint;
    }
}

public class TestClass
{
    public void Configure(object endpoint)
    {
        EricksonLopez.Result.AspNetCore.Extensions.Produces<int>(
            EricksonLopez.Result.AspNetCore.Extensions.AddResultEndpointFilter(endpoint)
        );
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<EndpointFilterOpenApiAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT008");
    }

    [Fact]
    public async Task RESULT008_DoesNotTrigger_When_Produces_Precedes()
    {
        const string source = @"
using System;

namespace EricksonLopez.Result.AspNetCore
{
    public static class Extensions
    {
        public static object AddResultEndpointFilter(this object endpoint) => endpoint;
        public static object Produces<T>(this object endpoint) => endpoint;
    }
}

public class TestClass
{
    public void Configure(object endpoint)
    {
        EricksonLopez.Result.AspNetCore.Extensions.AddResultEndpointFilter(
            EricksonLopez.Result.AspNetCore.Extensions.Produces<int>(endpoint)
        );
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<EndpointFilterOpenApiAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT008");
    }
}

