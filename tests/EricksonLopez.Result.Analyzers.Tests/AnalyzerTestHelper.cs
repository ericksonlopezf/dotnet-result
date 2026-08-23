// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace EricksonLopez.Result.Analyzers.Tests;

public static class AnalyzerTestHelper
{
    public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync<TAnalyzer>(string source)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var compilation = CreateCompilation(source);
        var initialDiagnostics = compilation.GetDiagnostics();
        if (initialDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
        {
            throw new System.InvalidOperationException("Compilation failed: " + string.Join("\n", initialDiagnostics));
        }

        var analyzer = new TAnalyzer();
        var compilationWithAnalyzer = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(analyzer),
            new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty));

        var allDiagnostics = await compilationWithAnalyzer.GetAnalyzerDiagnosticsAsync(CancellationToken.None);
        return allDiagnostics;
    }

    public static async Task<string> ApplyCodeFixAsync<TAnalyzer, TCodeFix>(string source, string diagnosticId)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);

        var solution = workspace.CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .AddMetadataReferences(projectId, GetMetadataReferences())
            .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithAllowUnsafe(true).WithNullableContextOptions(NullableContextOptions.Enable))
            .AddDocument(documentId, "Test.cs", SourceText.From(EnsureUsings(source)));

        var document = solution.GetDocument(documentId)!;
        var compilation = await document.Project.GetCompilationAsync();

        var analyzer = new TAnalyzer();
        var compilationWithAnalyzer = compilation!.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(analyzer),
            new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty));

        var diagnostics = await compilationWithAnalyzer.GetAnalyzerDiagnosticsAsync();
        var diagnostic = diagnostics.FirstOrDefault(d => d.Id == diagnosticId);

        if (diagnostic == null)
            return source;

        var codeFixProvider = new TCodeFix();
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
        await codeFixProvider.RegisterCodeFixesAsync(context);

#pragma warning disable S2583 // actions is populated asynchronously/via callback in RegisterCodeFixesAsync
        if (actions.Count == 0)
            return source;
#pragma warning restore S2583

        var operations = await actions[0].GetOperationsAsync(CancellationToken.None);
        var applyChangesOperation = operations.OfType<ApplyChangesOperation>().FirstOrDefault();

        if (applyChangesOperation == null)
            return source;

        var newDocument = applyChangesOperation.ChangedSolution.GetDocument(documentId)!;
        var newText = await newDocument.GetTextAsync();
        return newText.ToString();
    }

    public static async Task<ImmutableArray<CodeAction>> GetCodeActionsAsync<TAnalyzer, TCodeFix>(string source, string diagnosticId)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);

        var solution = workspace.CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .AddMetadataReferences(projectId, GetMetadataReferences())
            .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithAllowUnsafe(true).WithNullableContextOptions(NullableContextOptions.Enable))
            .AddDocument(documentId, "Test.cs", SourceText.From(EnsureUsings(source)));

        var document = solution.GetDocument(documentId)!;
        var compilation = await document.Project.GetCompilationAsync();

        var analyzer = new TAnalyzer();
        var compilationWithAnalyzer = compilation!.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(analyzer),
            new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty));

        var diagnostics = await compilationWithAnalyzer.GetAnalyzerDiagnosticsAsync();
        var diagnostic = diagnostics.FirstOrDefault(d => d.Id == diagnosticId);

        if (diagnostic == null)
            return ImmutableArray<CodeAction>.Empty;

        var codeFixProvider = new TCodeFix();
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
        await codeFixProvider.RegisterCodeFixesAsync(context);

        return actions.ToImmutableArray();
    }

    private static string EnsureUsings(string source)
    {
        var usings = "";
        if (!source.Contains("using System;"))
            usings += "using System;\n";
        if (!source.Contains("using System.Collections.Generic;"))
            usings += "using System.Collections.Generic;\n";
        if (!source.Contains("using System.Linq;"))
            usings += "using System.Linq;\n";
        if (!source.Contains("using System.Threading.Tasks;"))
            usings += "using System.Threading.Tasks;\n";
        if (!source.Contains("using EricksonLopez.Result;"))
            usings += "using EricksonLopez.Result;\n";
        if (!source.Contains("using EricksonLopez.Result.OpenTelemetry;"))
            usings += "using EricksonLopez.Result.OpenTelemetry;\n";
        if (!source.Contains("using EricksonLopez.Result.AspNetCore;"))
            usings += "using EricksonLopez.Result.AspNetCore;\n";
        return usings.Length > 0 ? usings + source : source;
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(EnsureUsings(source));
        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            GetMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithAllowUnsafe(true)
                .WithNullableContextOptions(NullableContextOptions.Enable));
    }

    private static List<MetadataReference> GetMetadataReferences()
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(HashSet<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(EricksonLopez.Result.Result).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(EricksonLopez.Result.OpenTelemetry.ResultActivityExtensions).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Diagnostics.Activity).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(EricksonLopez.Result.AspNetCore.ResultHttpOptions).Assembly.Location)
        };

        var runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
        var coreAssemblies = new[]
        {
            "System.Runtime.dll",
            "System.Collections.dll",
            "System.Linq.dll",
            "System.Linq.Expressions.dll",
            "netstandard.dll",
            "mscorlib.dll"
        };

        foreach (var assemblyName in coreAssemblies)
        {
            var assemblyPath = System.IO.Path.Combine(runtimeDir, assemblyName);
            if (System.IO.File.Exists(assemblyPath))
            {
                references.Add(MetadataReference.CreateFromFile(assemblyPath));
            }
        }

        return references;
    }
}





