// Copyright © Erickson Lopez. MIT License.
using AwesomeAssertions;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Serialization.Generators.Tests;

public class ResultMetricsVersionGeneratorTests
{
    [Fact]
    public void Emits_Version_From_AssemblyInformationalVersion_WithCommitHash()
    {
        var source = "// empty source";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultMetricsVersionGenerator>(
            source,
            informationalVersion: "2.3.4+abcdef123456");

        diagnostics.Should().BeEmpty();
        sources.Should().HaveCount(1);
        sources[0].HintName.Should().Be("ResultMetricsVersionConstants.g.cs");

        var text = sources[0].SourceText.ToString();
        text.Should().Contain("namespace EricksonLopez.Result.OpenTelemetry");
        text.Should().Contain("internal static class ResultMetricsVersionConstants");
        text.Should().Contain("internal const string Version = \"2.3.4\";");
    }

    [Fact]
    public void Emits_Version_From_AssemblyInformationalVersion_WithoutCommitHash()
    {
        var source = "// empty source";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultMetricsVersionGenerator>(
            source,
            informationalVersion: "3.1.0");

        diagnostics.Should().BeEmpty();
        sources.Should().HaveCount(1);

        var text = sources[0].SourceText.ToString();
        text.Should().Contain("internal const string Version = \"3.1.0\";");
    }

    [Fact]
    public void Emits_Version_From_AssemblyVersion_WhenInformationalVersionMissing()
    {
        var source = "// empty source";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultMetricsVersionGenerator>(
            source,
            assemblyVersion: "4.5.6.0");

        diagnostics.Should().BeEmpty();
        sources.Should().HaveCount(1);

        var text = sources[0].SourceText.ToString();
        text.Should().Contain("internal const string Version = \"4.5.6\";");
    }

    [Fact]
    public void Emits_Fallback_Version_WhenNoVersionAttributesPresent()
    {
        var source = "// empty source";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultMetricsVersionGenerator>(source);

        diagnostics.Should().BeEmpty();
        sources.Should().HaveCount(1);

        var text = sources[0].SourceText.ToString();
        text.Should().Contain("internal const string Version = \"0.0.0\";"); // Default assembly version in roslyn compilation without attribute is 0.0.0
    }

    [Fact]
    public void Emits_Empty_Version_WhenInformationalVersion_StartsWithPlus()
    {
        var source = "// empty source";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultMetricsVersionGenerator>(
            source,
            informationalVersion: "+shaonly");

        diagnostics.Should().BeEmpty();
        sources.Should().HaveCount(1);

        var text = sources[0].SourceText.ToString();
        text.Should().Contain("internal const string Version = \"\";");
    }

    [Fact]
    public void Falls_Back_To_Assembly_Version_When_InformationalVersion_Has_No_Arguments()
    {
        var source = @"
[assembly: CustomNamespace.AssemblyInformationalVersionAttribute]

namespace CustomNamespace
{
    using System;
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyInformationalVersionAttribute : Attribute
    {
        public AssemblyInformationalVersionAttribute() { }
    }
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultMetricsVersionGenerator>(source);
        diagnostics.Should().BeEmpty();
        sources.Should().HaveCount(1);

        var text = sources[0].SourceText.ToString();
        text.Should().Contain("internal const string Version = \"0.0.0\";");
    }
}

