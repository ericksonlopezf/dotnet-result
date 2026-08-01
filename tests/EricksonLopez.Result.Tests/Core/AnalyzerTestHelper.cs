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

namespace EricksonLopez.Result.Tests.Core;

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
            .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithNullableContextOptions(NullableContextOptions.Enable))
            .AddDocument(documentId, "Test.cs", SourceText.From(source));

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

        if (actions.Count == 0)
            return source;

        var operations = await actions[0].GetOperationsAsync(CancellationToken.None);
        var applyChangesOperation = operations.OfType<ApplyChangesOperation>().FirstOrDefault();
        
        if (applyChangesOperation == null)
            return source;

        var newDocument = applyChangesOperation.ChangedSolution.GetDocument(documentId)!;
        var newText = await newDocument.GetTextAsync();
        return newText.ToString();
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            GetMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));
    }

    private static List<MetadataReference> GetMetadataReferences()
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.HashSet<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(EricksonLopez.Result.Result).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(EricksonLopez.Result.OpenTelemetry.ResultActivityExtensions).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Diagnostics.Activity).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(EricksonLopez.Result.AspNetCore.ResultHttpOptions).Assembly.Location)
        };

        var runtimeRef = MetadataReference.CreateFromFile(
            System.IO.Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.dll"));
        if (System.IO.File.Exists(runtimeRef.Display!))
            references.Add(runtimeRef);

        return references;
    }
}
