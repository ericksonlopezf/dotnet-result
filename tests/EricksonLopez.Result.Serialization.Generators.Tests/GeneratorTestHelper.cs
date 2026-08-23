// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EricksonLopez.Result.Serialization.Generators.Tests;

public static class GeneratorTestHelper
{
    private static string EnsureUsings(string source)
    {
        var usings = "";
        if (!source.Contains("using System;"))
            usings += "using System;\n";
        if (!source.Contains("using System.Text.Json.Serialization;"))
            usings += "using System.Text.Json.Serialization;\n";
        if (!source.Contains("using EricksonLopez.Result;"))
            usings += "using EricksonLopez.Result;\n";
        return usings.Length > 0 ? usings + source : source;
    }

    public static (ImmutableArray<GeneratedSourceResult> Sources, ImmutableArray<Diagnostic> Diagnostics) RunGenerator<TGenerator>(
        string source,
        IEnumerable<MetadataReference>? additionalReferences = null,
        string? assemblyName = "TestAssembly",
        string? assemblyVersion = null,
        string? informationalVersion = null)
        where TGenerator : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(EnsureUsings(source));

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Text.Json.JsonSerializer).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Text.Json.Serialization.JsonSerializerContext).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(EricksonLopez.Result.Result).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(EricksonLopez.Result.Serialization.ResultJsonConverter).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
        };

        if (additionalReferences != null)
        {
            references.AddRange(additionalReferences);
        }

        var attributesSource = new List<string>();
        if (!string.IsNullOrEmpty(informationalVersion))
        {
            attributesSource.Add($"[assembly: System.Reflection.AssemblyInformationalVersion(\"{informationalVersion}\")]");
        }
        if (!string.IsNullOrEmpty(assemblyVersion))
        {
            attributesSource.Add($"[assembly: System.Reflection.AssemblyVersion(\"{assemblyVersion}\")]");
        }

        var syntaxTrees = new List<SyntaxTree> { syntaxTree };
        if (attributesSource.Count > 0)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(string.Join("\n", attributesSource)));
        }

        var compilation = CSharpCompilation.Create(
            assemblyName ?? "TestAssembly",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new TGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult();
        var generatedSources = runResult.Results.SelectMany(r => r.GeneratedSources).ToImmutableArray();

        return (generatedSources, diagnostics);
    }
}

