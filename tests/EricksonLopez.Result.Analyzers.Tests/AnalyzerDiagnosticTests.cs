using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using EricksonLopez.Result.Analyzers;

namespace EricksonLopez.Result.Analyzers.Tests;

/// <summary>
/// Roslyn diagnostic tests for RESULT009 (IncludeDescriptionSecurityAnalyzer)
/// and the LINQ extension of RESULT007 (HashSetErrorEqualityAnalyzer).
/// Uses Microsoft.CodeAnalysis.CSharp directly (no testing framework dependency).
/// </summary>
public class AnalyzerDiagnosticTests
{
    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static ImmutableArray<Diagnostic> GetDiagnostics<TAnalyzer>(string source)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.HashSet<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            // netstandard references for Distinct/GroupBy etc.
        };

        // Add System.Runtime ref
        var runtimeRef = MetadataReference.CreateFromFile(
            System.IO.Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.dll"));
        if (System.IO.File.Exists(runtimeRef.Display!))
            references.Add(runtimeRef);

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));

        var analyzer = new TAnalyzer();
        var compilationWithAnalyzer = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(analyzer),
            new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty));

        return compilationWithAnalyzer.GetAnalyzerDiagnosticsAsync(CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    // ─── RESULT009: IncludeDescriptionSecurityAnalyzer ────────────────────────

    [Fact]
    public void RESULT009_TriggersOn_IncludeDescription_True_Literal()
    {
        // Arrange: code that sets IncludeDescription = true as a literal
        const string source = @"
namespace EricksonLopez.Result.AspNetCore
{
    public class ResultHttpOptions
    {
        public bool IncludeDescription { get; set; }
    }
}

namespace TestCode
{
    using EricksonLopez.Result.AspNetCore;
    public class Startup
    {
        public void Configure(ResultHttpOptions options)
        {
            options.IncludeDescription = true;  // RESULT009: should warn
        }
    }
}";

        var diagnostics = GetDiagnostics<IncludeDescriptionSecurityAnalyzer>(source);

        Assert.Contains(diagnostics, d => d.Id == IncludeDescriptionSecurityAnalyzer.DiagnosticId);
    }

    [Fact]
    public void RESULT009_DoesNotTrigger_When_IncludeDescription_SetToVariable()
    {
        // Arrange: code that uses env.IsDevelopment() — safe pattern
        const string source = @"
namespace EricksonLopez.Result.AspNetCore
{
    public class ResultHttpOptions
    {
        public bool IncludeDescription { get; set; }
    }
}

namespace TestCode
{
    using EricksonLopez.Result.AspNetCore;
    public class Startup
    {
        public void Configure(ResultHttpOptions options, bool isDev)
        {
            options.IncludeDescription = isDev;  // safe — not a literal
        }
    }
}";

        var diagnostics = GetDiagnostics<IncludeDescriptionSecurityAnalyzer>(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == IncludeDescriptionSecurityAnalyzer.DiagnosticId);
    }

    [Fact]
    public void RESULT009_DoesNotTrigger_When_IncludeDescription_SetToFalse()
    {
        // Arrange: setting to false is always safe
        const string source = @"
namespace EricksonLopez.Result.AspNetCore
{
    public class ResultHttpOptions
    {
        public bool IncludeDescription { get; set; }
    }
}

namespace TestCode
{
    using EricksonLopez.Result.AspNetCore;
    public class Startup
    {
        public void Configure(ResultHttpOptions options)
        {
            options.IncludeDescription = false;  // safe — false is not a risk
        }
    }
}";

        var diagnostics = GetDiagnostics<IncludeDescriptionSecurityAnalyzer>(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == IncludeDescriptionSecurityAnalyzer.DiagnosticId);
    }

    [Fact]
    public void RESULT009_DiagnosticId_IsCorrect()
    {
        var analyzer = new IncludeDescriptionSecurityAnalyzer();
        Assert.Contains(analyzer.SupportedDiagnostics, d => d.Id == "RESULT009");
    }

    [Fact]
    public void RESULT009_DefaultSeverity_IsWarning()
    {
        var analyzer = new IncludeDescriptionSecurityAnalyzer();
        var rule = analyzer.SupportedDiagnostics.Single(d => d.Id == "RESULT009");
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
    }

    [Fact]
    public void RESULT009_Category_IsSecurity()
    {
        var analyzer = new IncludeDescriptionSecurityAnalyzer();
        var rule = analyzer.SupportedDiagnostics.Single(d => d.Id == "RESULT009");
        Assert.Equal("Security", rule.Category);
    }

    // ─── RESULT007 metadata: HashSetErrorEqualityAnalyzer ────────────────────

    [Fact]
    public void RESULT007_DiagnosticId_IsCorrect()
    {
        var analyzer = new HashSetErrorEqualityAnalyzer();
        Assert.Contains(analyzer.SupportedDiagnostics, d => d.Id == "RESULT007");
    }

    [Fact]
    public void RESULT007_DefaultSeverity_IsWarning()
    {
        var analyzer = new HashSetErrorEqualityAnalyzer();
        var rule = analyzer.SupportedDiagnostics.Single(d => d.Id == "RESULT007");
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
    }

    [Fact]
    public void RESULT007_Category_IsUsage()
    {
        var analyzer = new HashSetErrorEqualityAnalyzer();
        var rule = analyzer.SupportedDiagnostics.Single(d => d.Id == "RESULT007");
        Assert.Equal("Usage", rule.Category);
    }

    [Fact]
    public void RESULT007_SupportsDiagnostics_ContainsExpectedRule()
    {
        var analyzer = new HashSetErrorEqualityAnalyzer();
        Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("RESULT007", analyzer.SupportedDiagnostics[0].Id);
    }
}

