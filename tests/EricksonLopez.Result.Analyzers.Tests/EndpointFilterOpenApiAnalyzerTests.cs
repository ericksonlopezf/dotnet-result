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

public class EndpointFilterOpenApiAnalyzerTests
{
    [Fact]
    public void RESULT008_Descriptor_Properties_AreAccurate()
    {
        var analyzer = new EndpointFilterOpenApiAnalyzer();
        var diagnostics = analyzer.SupportedDiagnostics;
        var rule = Assert.Single(diagnostics);

        Assert.Equal("RESULT008", rule.Id);
        Assert.Equal("ResultEndpointFilter hides OpenAPI metadata without explicit Produces<T>()", rule.Title.ToString());
        Assert.Equal(
            "The automatic ResultEndpointFilter returns an untyped object to OpenAPI. Call .Produces<T>() or .ProducesProblem() on this endpoint, or use .ToHttpResult<T>() directly in the handler instead.",
            rule.MessageFormat.ToString());
        Assert.Equal("Usage", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal(
            "ResultEndpointFilter returns typed data at runtime but returns object? for its API Explorer schema. You must explicitly declare your types using Produces<T> to prevent schema degradation.",
            rule.Description.ToString());
        Assert.Equal("https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md#RESULT008", rule.HelpLinkUri);
    }

    [Fact]
    public async Task RESULT008_TriggersOn_AddResultEndpointFilter_Without_Produces()
    {
        const string source = @"
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
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT008");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
    }

    [Fact]
    public async Task RESULT008_TriggersOn_AddResultEndpointFilter_Static_WithoutArguments()
    {
        const string source = @"
namespace EricksonLopez.Result.AspNetCore
{
    public static class Extensions
    {
        public static object AddResultEndpointFilter() => null!;
    }
}

public class TestClass
{
    public void Configure()
    {
        EricksonLopez.Result.AspNetCore.Extensions.AddResultEndpointFilter();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<EndpointFilterOpenApiAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT008");
        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
    }

    [Fact]
    public async Task RESULT008_TriggersOn_AddResultEndpointFilter_WithOtherNonProducesMethods()
    {
        const string source = @"
namespace EricksonLopez.Result.AspNetCore
{
    public static class Extensions
    {
        public static object AddResultEndpointFilter(this object endpoint) => endpoint;
        public static object WithName(this object endpoint, string name) => endpoint;
        public static object WithTags(this object endpoint, string tag) => endpoint;
        public static object RequireAuthorization(this object endpoint) => endpoint;
    }
}

public class TestClass
{
    public void Configure(object endpoint)
    {
        EricksonLopez.Result.AspNetCore.Extensions.RequireAuthorization(
            EricksonLopez.Result.AspNetCore.Extensions.AddResultEndpointFilter(
                EricksonLopez.Result.AspNetCore.Extensions.WithTags(
                    EricksonLopez.Result.AspNetCore.Extensions.WithName(endpoint, ""Test""),
                    ""Tag""
                )
            )
        );
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<EndpointFilterOpenApiAnalyzer>(source);
        Assert.Single(diagnostics, d => d.Id == "RESULT008");
    }

    [Fact]
    public async Task RESULT008_DoesNotTrigger_When_Produces_Follows_Immediately()
    {
        const string source = @"
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
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT008_DoesNotTrigger_When_Produces_Precedes_Immediately()
    {
        const string source = @"
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
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT008_DoesNotTrigger_When_Produces_Follows_MultipleHops()
    {
        const string source = @"
namespace EricksonLopez.Result.AspNetCore
{
    public static class Extensions
    {
        public static object AddResultEndpointFilter(this object endpoint) => endpoint;
        public static object WithName(this object endpoint, string name) => endpoint;
        public static object Produces<T>(this object endpoint) => endpoint;
        public static object RequireAuthorization(this object endpoint) => endpoint;
    }
}

public class TestClass
{
    public void Configure(object endpoint)
    {
        EricksonLopez.Result.AspNetCore.Extensions.RequireAuthorization(
            EricksonLopez.Result.AspNetCore.Extensions.Produces<string>(
                EricksonLopez.Result.AspNetCore.Extensions.WithName(
                    EricksonLopez.Result.AspNetCore.Extensions.AddResultEndpointFilter(endpoint),
                    ""GetUsers""
                )
            )
        );
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<EndpointFilterOpenApiAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT008_DoesNotTrigger_When_Produces_Precedes_MultipleHops()
    {
        const string source = @"
namespace EricksonLopez.Result.AspNetCore
{
    public static class Extensions
    {
        public static object AddResultEndpointFilter(this object endpoint) => endpoint;
        public static object WithName(this object endpoint, string name) => endpoint;
        public static object Produces<T>(this object endpoint) => endpoint;
    }
}

public class TestClass
{
    public void Configure(object endpoint)
    {
        EricksonLopez.Result.AspNetCore.Extensions.AddResultEndpointFilter(
            EricksonLopez.Result.AspNetCore.Extensions.WithName(
                EricksonLopez.Result.AspNetCore.Extensions.Produces<string>(endpoint),
                ""GetUsers""
            )
        );
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<EndpointFilterOpenApiAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT008_DoesNotTrigger_When_ProducesProblem_Follows()
    {
        const string source = @"
namespace EricksonLopez.Result.AspNetCore
{
    public static class Extensions
    {
        public static object AddResultEndpointFilter(this object endpoint) => endpoint;
        public static object ProducesProblem(this object endpoint, int statusCode) => endpoint;
    }
}

public class TestClass
{
    public void Configure(object endpoint)
    {
        EricksonLopez.Result.AspNetCore.Extensions.ProducesProblem(
            EricksonLopez.Result.AspNetCore.Extensions.AddResultEndpointFilter(endpoint),
            400
        );
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<EndpointFilterOpenApiAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT008_DoesNotTrigger_When_ProducesValidationProblem_Precedes()
    {
        const string source = @"
namespace EricksonLopez.Result.AspNetCore
{
    public static class Extensions
    {
        public static object AddResultEndpointFilter(this object endpoint) => endpoint;
        public static object ProducesValidationProblem(this object endpoint) => endpoint;
    }
}

public class TestClass
{
    public void Configure(object endpoint)
    {
        EricksonLopez.Result.AspNetCore.Extensions.AddResultEndpointFilter(
            EricksonLopez.Result.AspNetCore.Extensions.ProducesValidationProblem(endpoint)
        );
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<EndpointFilterOpenApiAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT008_DoesNotTrigger_On_OtherMethodNames()
    {
        const string source = @"
namespace EricksonLopez.Result.AspNetCore
{
    public static class Extensions
    {
        public static object AddOtherFilter(this object endpoint) => endpoint;
    }
}

public class TestClass
{
    public void Configure(object endpoint)
    {
        EricksonLopez.Result.AspNetCore.Extensions.AddOtherFilter(endpoint);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<EndpointFilterOpenApiAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT008_TriggersOn_InstanceMethod_AddResultEndpointFilter_Without_Produces()
    {
        const string source = @"
public class Builder
{
    public Builder AddResultEndpointFilter() => this;
    public Builder WithName(string name) => this;
}

public class TestClass
{
    public void Configure(Builder builder)
    {
        builder.WithName(""test"").AddResultEndpointFilter();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<EndpointFilterOpenApiAnalyzer>(source);
        Assert.Single(diagnostics, d => d.Id == "RESULT008");
    }

    [Fact]
    public async Task RESULT008_DoesNotTriggerOn_InstanceMethod_When_Produces_Precedes()
    {
        const string source = @"
public class Builder
{
    public Builder AddResultEndpointFilter() => this;
    public Builder Produces<T>() => this;
}

public class TestClass
{
    public void Configure(Builder builder)
    {
        builder.Produces<int>().AddResultEndpointFilter();
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<EndpointFilterOpenApiAnalyzer>(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RESULT008_TriggersOn_AddResultEndpointFilter_InVariableAssignmentAndReturn()
    {
        const string source = @"
namespace EricksonLopez.Result.AspNetCore
{
    public static class Extensions
    {
        public static object AddResultEndpointFilter(this object endpoint) => endpoint;
    }
}

public class TestClass
{
    public object Configure(object endpoint)
    {
        var local = EricksonLopez.Result.AspNetCore.Extensions.AddResultEndpointFilter(endpoint);
        return EricksonLopez.Result.AspNetCore.Extensions.AddResultEndpointFilter(local);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<EndpointFilterOpenApiAnalyzer>(source);
        Assert.Equal(2, diagnostics.Length);
    }
}





