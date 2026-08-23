// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace EricksonLopez.Result.Analyzers.Tests;

public class ClosureCaptureAnalyzerTests
{
    [Fact]
    public async Task RESULT004_TriggersOn_Map_Capturing_Local()
    {
        const string source = @"
using EricksonLopez.Result;

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        int myLocal = 5;
        return result.Map(x => x + myLocal);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT004");
        Assert.Contains("Map", diag.GetMessage());
        Assert.Contains("myLocal", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT004_TriggersOn_AllTrackedMethods()
    {
        const string source = @"

public class TestClass
{
    public void Test(Result<int> r, Result nonGeneric, int local)
    {
        _ = r.Bind(x => Result.Success(x + local));
        _ = r.TapOnSuccess(x => { int a = x + local; });
        _ = r.TapOnFailure(err => { string s = err.Code + local; });
        _ = r.Ensure(x => x > local, Error.Create(""c"", ""d"").Build());
        _ = r.MapError(err => Error.Create(err.Code + local, ""d"").Build());
        _ = r.Inspect(res => { int a = local; });
        _ = r.Match(x => x + local, err => local);
        r.Execute(x => { int a = x + local; }, err => { int b = local; });
        _ = r.MapFailure(err => local, 0);
        _ = r.Recover(err => Result.Success(local));
        
        _ = nonGeneric.Bind(() => Result.Success());
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        var ids = diagnostics.Where(d => d.Id == "RESULT004").ToList();
        Assert.True(ids.Count >= 10);
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_NoCapture()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        return result.Map(x => x + 1);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_StaticLambda()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        return result.Map(static x => x + 1);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_UsingStateOverload()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        int myLocal = 5;
        return result.Map(myLocal, static (state, x) => x + state);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_MethodGroup()
    {
        const string source = @"

public class TestClass
{
    private static int Transform(int x) => x * 2;

    public Result<int> Method()
    {
        var result = Result.Success(1);
        return result.Map(Transform);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_NotResultType()
    {
        const string source = @"
public class CustomCollection
{
    public int Map(System.Func<int, int> func) => func(10);
}

public class TestClass
{
    public int Method()
    {
        var coll = new CustomCollection();
        int local = 5;
        return coll.Map(x => x + local);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_SameNamedType_InDifferentNamespace()
    {
        const string source = @"
namespace OtherNamespace
{
    public class Result
    {
        public int Map(System.Func<int, int> func) => func(10);
    }
}

public class TestClass
{
    public int Method()
    {
        var res = new OtherNamespace.Result();
        int local = 5;
        return res.Map(x => x + local);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_TriggersOn_MethodParameterCapture()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method(int multiplier)
    {
        var result = Result.Success(1);
        return result.Map(x => x * multiplier);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT004");
        Assert.Contains("multiplier", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT004_TriggersOn_MultipleCaptures()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method(int a, int b)
    {
        var result = Result.Success(1);
        int c = 10;
        return result.Map(x => x + a + b + c);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT004");
        var msg = diag.GetMessage();
        Assert.Contains("3 local variable(s)", msg);
        Assert.Contains("a", msg);
        Assert.Contains("b", msg);
        Assert.Contains("c", msg);
    }

    [Fact]
    public async Task RESULT004_TriggersOn_ParenthesizedLambda_And_AnonymousMethod()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var result = Result.Success(1);
        int local = 5;
        _ = result.Map((x) => x + local);
        _ = result.Map(delegate(int x) { return x + local; });
        _ = result.Map(delegate { return local; });
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        var count = diagnostics.Count(d => d.Id == "RESULT004");
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task RESULT004_TriggersOn_ExplicitThisCapture()
    {
        const string source = @"

public class TestClass
{
    private int _factor = 2;

    public Result<int> Method()
    {
        var result = Result.Success(1);
        return result.Map(x => x * this._factor);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT004");
        Assert.Contains("this", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT004_TriggersOn_ImplicitThisCapture_Field_Property_Method()
    {
        const string source = @"

public class TestClass
{
    private int _field = 2;
    public int Prop => 3;
    private int Compute(int x) => x * 4;

    public void Method()
    {
        var result = Result.Success(1);
        _ = result.Map(x => x * _field);
        _ = result.Map(x => x * Prop);
        _ = result.Map(x => Compute(x));
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        var diags = diagnostics.Where(d => d.Id == "RESULT004").ToList();
        Assert.Equal(3, diags.Count);
        Assert.All(diags, d => Assert.Contains("this", d.GetMessage()));
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_StaticMemberAccess()
    {
        const string source = @"

public class TestClass
{
    private static int StaticField = 2;
    public static int StaticProp => 3;
    private static int StaticCompute(int x) => x * 4;

    public void Method()
    {
        var result = Result.Success(1);
        _ = result.Map(x => x * StaticField);
        _ = result.Map(x => x * StaticProp);
        _ = result.Map(x => StaticCompute(x));
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public void RESULT004_Descriptor_Properties_Are_Correct()
    {
        var analyzer = new ClosureCaptureAnalyzer();
        var descriptor = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("RESULT004", descriptor.Id);
        Assert.Equal("Lambda captures locals — use the TState overload to avoid closure allocation", descriptor.Title.ToString());
        Assert.Equal("Performance", descriptor.Category);
        Assert.Equal(DiagnosticSeverity.Warning, descriptor.DefaultSeverity);
        Assert.True(descriptor.IsEnabledByDefault);
        Assert.Equal("https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/performance.md#closure-free-pipelines", descriptor.HelpLinkUri);
        Assert.Equal("Lambda in {0}() captures {1} local variable(s) ({2}); use the {0}(TState, ...) overload to avoid the allocation", descriptor.MessageFormat.ToString());
        Assert.Equal("Result<T> methods (Map, Bind, TapOnSuccess, TapOnFailure, Ensure, MapError, Inspect) provide TState overloads that accept an additional state parameter, eliminating the need for a closure delegate. When a lambda captures local variables or 'this' from the enclosing scope, the JIT allocates a closure object on every invocation. Pass captured values as the TState parameter instead. Example: change 'result.Map(x => Process(id, x))' to 'result.Map(id, (i, x) => Process(i, x))'. For 'this' captures, pass the relevant member or 'this' as state: 'result.Map(this, (self, x) => self.Process(x))'.", descriptor.Description.ToString());
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_On_NonTrackedMethod_On_Result()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var result = Result.Success(1);
        _ = result.ToString();
        _ = result.IsSuccess;
        _ = result.GetValueOrDefault(0);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_On_OtherType_In_EricksonLopezResult()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var builder = Error.Create(""code"", ""desc"");
        int local = 5;
        _ = builder.WithType(ErrorType.Domain);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_Diagnostic_Message_Format_And_Capture_List_Separation()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method(int varA, int varB)
    {
        var result = Result.Success(1);
        return result.Map(x => x + varA + varB);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT004");
        Assert.Contains("2 local variable(s) (varA, varB)", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_LambdaParameter_Shadows_OuterVariable()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method()
    {
        int x = 10;
        var result = Result.Success(1);
        return result.Map((int x) => x + 1);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_TriggersOn_AnonymousMethod_And_ParenthesizedLambda_WithMultipleParameters()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        int local = 5;
        var result = Result.Success(1);
        _ = result.Map((a) => a + local);
        _ = result.Map(delegate(int a) { return a + local; });
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        var diags = diagnostics.Where(d => d.Id == "RESULT004").ToList();
        Assert.Equal(2, diags.Count);
        foreach (var diag in diags)
        {
            var msg = diag.GetMessage();
            Assert.Equal("Lambda in Map() captures 1 local variable(s) (local); use the Map(TState, ...) overload to avoid the allocation", msg);
        }
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_MemberAccessOnAnotherObject()
    {
        const string source = @"

public class OtherClass
{
    public int Value => 42;
}

public class TestClass
{
    public Result<int> Method()
    {
        var other = new OtherClass();
        var result = Result.Success(1);
        return result.Map(x => x + other.Value);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT004");
        Assert.Contains("other", diag.GetMessage());
        Assert.DoesNotContain("this", diag.GetMessage());
    }

    [Fact]
    public void RESULT004_CodeFix_Properties_Are_Correct()
    {
        var provider = new ClosureCaptureCodeFix();
        var id = Assert.Single(provider.FixableDiagnosticIds);
        Assert.Equal("RESULT004", id);
        Assert.NotNull(provider.GetFixAllProvider());
    }

    [Fact]
    public async Task RESULT004_CodeFix_Registers_Actions_WithExpectedMetadata()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        int myLocal = 5;
        return result.Map(x => x + myLocal);
    }
}";
        var actions = await AnalyzerTestHelper.GetCodeActionsAsync<ClosureCaptureAnalyzer, ClosureCaptureCodeFix>(source, "RESULT004");
        Assert.Equal(2, actions.Length);

        var fix1 = actions[0];
        Assert.Equal("[RESULT004] Reveal captures: make lambda 'static' (then switch to TState overload)", fix1.Title);
        Assert.Equal("ClosureCaptureCodeFix_static", fix1.EquivalenceKey);

        var fix2 = actions[1];
        Assert.Equal("[RESULT004] Insert TState rewrite guidance comment (shows zero-allocation pattern)", fix2.Title);
        Assert.Equal("ClosureCaptureCodeFix_guidance", fix2.EquivalenceKey);
    }

    [Fact]
    public async Task RESULT004_CodeFix_Applies_StaticLambdaFix_SimpleLambda()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        int myLocal = 5;
        return result.Map(x => x + myLocal);
    }
}";
        var fixedSource = await AnalyzerTestHelper.ApplyCodeFixAsync<ClosureCaptureAnalyzer, ClosureCaptureCodeFix>(source, "RESULT004");
        Assert.Contains("result.Map(static x => x + myLocal)", fixedSource);
    }

    [Fact]
    public async Task RESULT004_CodeFix_Applies_StaticLambdaFix_ParenthesizedLambda()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        int myLocal = 5;
        return result.Map((x) => x + myLocal);
    }
}";
        var fixedSource = await AnalyzerTestHelper.ApplyCodeFixAsync<ClosureCaptureAnalyzer, ClosureCaptureCodeFix>(source, "RESULT004");
        Assert.Contains("result.Map(static (x) => x + myLocal)", fixedSource);
    }

    [Fact]
    public async Task RESULT004_CodeFix_Applies_GuidanceCommentFix()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        int myLocal = 5;
        return result.Map(x => x + myLocal);
    }
}";
        var actions = await AnalyzerTestHelper.GetCodeActionsAsync<ClosureCaptureAnalyzer, ClosureCaptureCodeFix>(source, "RESULT004");
        var guidanceAction = actions.First(a => a.EquivalenceKey == "ClosureCaptureCodeFix_guidance");
        var operations = await guidanceAction.GetOperationsAsync(CancellationToken.None);
        var applyDocOp = operations.OfType<ApplyChangesOperation>().Single();
        var doc = applyDocOp.ChangedSolution.Projects.SelectMany(p => p.Documents).Single();
        var text = (await doc.GetTextAsync()).ToString();
        var normalized = text.Replace("\r\n", "\n");
        Assert.Contains("        // RESULT004: Eliminate closure \u2014 use the TState overload to pass captured variables as a parameter.\n", normalized);
        Assert.Contains("\n        // Before (allocates closure):   result.Map(x => DoWork(captured, x))\n", normalized);
        Assert.Contains("\n        // After  (zero-allocation):     result.Map(captured, static (state, x) => DoWork(state, x))\n", normalized);
        Assert.Contains("\n        // For 'this' captures:          result.Map(this, static (self, x) => self.DoWork(x))\n", normalized);
        Assert.Contains("\n        // Remove this comment after applying the TState rewrite.\n", normalized);
        Assert.DoesNotContain("\n// Before", normalized);
        Assert.DoesNotContain("\n // Before", normalized);
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_UntrackedMethod_On_Result_Has_Capturing_Lambda()
    {
        const string source = @"

namespace EricksonLopez.Result
{
    public partial class Result
    {
        public static void UntrackedMethod(Func<int, int> func) {}
    }
}

public class TestClass
{
    public void Method()
    {
        int local = 5;
        EricksonLopez.Result.Result.UntrackedMethod(x => x + local);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_OtherType_In_EricksonLopezResult_Has_TrackedMethodName()
    {
        const string source = @"

namespace EricksonLopez.Result
{
    public class OtherResultType
    {
        public int Map(Func<int, int> func) => func(1);
    }
}

public class TestClass
{
    public void Method()
    {
        var helper = new EricksonLopez.Result.OtherResultType();
        int local = 5;
        _ = helper.Map(x => x + local);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_TriggersOn_NonGenericResult_Map()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var result = Result.Success();
        int local = 5;
        _ = result.Map(() => local);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT004");
        Assert.Contains("local", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_StateOverload_Has_Capturing_Lambda()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var result = Result.Success(1);
        int state = 10;
        int local = 5;
        _ = result.Map(state, (s, x) => s + x + local);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT004");
    }

    [Fact]
    public async Task RESULT004_CodeFix_Applies_GuidanceCommentFix_Indentation_Verification()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        int myLocal = 5;
        return result.Map(x => x + myLocal);
    }
}";
        var actions = await AnalyzerTestHelper.GetCodeActionsAsync<ClosureCaptureAnalyzer, ClosureCaptureCodeFix>(source, "RESULT004");
        var guidanceAction = actions.First(a => a.EquivalenceKey == "ClosureCaptureCodeFix_guidance");
        var operations = await guidanceAction.GetOperationsAsync(CancellationToken.None);
        var applyDocOp = operations.OfType<ApplyChangesOperation>().Single();
        var doc = applyDocOp.ChangedSolution.Projects.SelectMany(p => p.Documents).Single();
        var text = (await doc.GetTextAsync()).ToString();

        var normalized = text.Replace("\r\n", "\n");
        Assert.Contains("\n        // Before (allocates closure):   result.Map(x => DoWork(captured, x))\n", normalized);
        Assert.Contains("\n        // After  (zero-allocation):     result.Map(captured, static (state, x) => DoWork(state, x))\n", normalized);
        Assert.Contains("\n        // For 'this' captures:          result.Map(this, static (self, x) => self.DoWork(x))\n", normalized);
        Assert.Contains("\n        // Remove this comment after applying the TState rewrite.\n", normalized);
        Assert.DoesNotContain("\n// Before", normalized);
        Assert.DoesNotContain("\n // Before", normalized);
        Assert.Contains("return result.Map(x => x + myLocal);", text);
        Assert.True(text.IndexOf("// RESULT004", StringComparison.Ordinal) < text.IndexOf("return result.Map", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RESULT004_CodeFix_Applies_When_DiagnosticSpan_IsOnReturnStatementNode()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        int local = 5;
        return result.Map(x => x + local);
    }
}";
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var solution = workspace.CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .AddDocument(documentId, "Test.cs", SourceText.From(source));
        var document = solution.GetDocument(documentId)!;
        var tree = await document.GetSyntaxTreeAsync();
        var root = await tree!.GetRootAsync();
        var returnStatement = root.DescendantNodes().OfType<ReturnStatementSyntax>().First();
        var diagnostic = Diagnostic.Create(
            new ClosureCaptureAnalyzer().SupportedDiagnostics[0],
            returnStatement.GetLocation(),
            "Map",
            1,
            "local");

        var actions = new List<CodeAction>();
        var provider = new ClosureCaptureCodeFix();
        var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
        await provider.RegisterCodeFixesAsync(context);

        Assert.Equal(2, actions.Count);
    }

    [Fact]
    public async Task RESULT004_CodeFix_Applies_GuidanceCommentFix_WithFallbackMethodName()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        int local = 5;
        Func<Func<int, int>, Result<int>> fn = result.Map;
        return (true ? fn : fn)(x => x + local);
    }
}";
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var solution = workspace.CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .AddDocument(documentId, "Test.cs", SourceText.From(source));
        var document = solution.GetDocument(documentId)!;
        var tree = await document.GetSyntaxTreeAsync();
        var root = await tree!.GetRootAsync();
        var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().Last();
        var diagnostic = Diagnostic.Create(
            new ClosureCaptureAnalyzer().SupportedDiagnostics[0],
            invocation.GetLocation(),
            "Method",
            1,
            "local");

        var actions = new List<CodeAction>();
        var provider = new ClosureCaptureCodeFix();
        var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
        await provider.RegisterCodeFixesAsync(context);

        var guidanceAction = Assert.Single(actions, a => a.EquivalenceKey == "ClosureCaptureCodeFix_guidance");
        var operations = await guidanceAction.GetOperationsAsync(CancellationToken.None);
        var applyDocOp = operations.OfType<ApplyChangesOperation>().Single();
        var doc = applyDocOp.ChangedSolution.Projects.SelectMany(p => p.Documents).Single();
        var text = (await doc.GetTextAsync()).ToString();
        var normalized = text.Replace("\r\n", "\n");
        Assert.Contains("        // Before (allocates closure):   result.Method(x => DoWork(captured, x))\n", normalized);
    }

    [Fact]
    public async Task RESULT004_CodeFix_Applies_GuidanceCommentFix_WithIdentifierNameSyntaxMethod()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        int local = 5;
        Func<Func<int, int>, Result<int>> Map = result.Map;
        return Map(x => x + local);
    }
}";
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var solution = workspace.CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .AddDocument(documentId, "Test.cs", SourceText.From(source));
        var document = solution.GetDocument(documentId)!;
        var tree = await document.GetSyntaxTreeAsync();
        var root = await tree!.GetRootAsync();
        var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().Last();
        var diagnostic = Diagnostic.Create(
            new ClosureCaptureAnalyzer().SupportedDiagnostics[0],
            invocation.GetLocation(),
            "Map",
            1,
            "local");

        var actions = new List<CodeAction>();
        var provider = new ClosureCaptureCodeFix();
        var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
        await provider.RegisterCodeFixesAsync(context);

        var guidanceAction = Assert.Single(actions, a => a.EquivalenceKey == "ClosureCaptureCodeFix_guidance");
        var operations = await guidanceAction.GetOperationsAsync(CancellationToken.None);
        var applyDocOp = operations.OfType<ApplyChangesOperation>().Single();
        var doc = applyDocOp.ChangedSolution.Projects.SelectMany(p => p.Documents).Single();
        var text = (await doc.GetTextAsync()).ToString();
        var normalized = text.Replace("\r\n", "\n");
        Assert.Contains("        // Before (allocates closure):   result.Map(x => DoWork(captured, x))\n", normalized);
    }

    [Fact]
    public async Task RESULT004_CodeFix_Applies_GuidanceCommentFix_On_ExpressionBodiedMember()
    {
        const string source = @"

public class TestClass
{
    private int myLocal = 5;
    public Result<int> Method() => Result.Success(1).Map(x => x + myLocal);
}";
        var actions = await AnalyzerTestHelper.GetCodeActionsAsync<ClosureCaptureAnalyzer, ClosureCaptureCodeFix>(source, "RESULT004");
        var guidanceAction = actions.First(a => a.EquivalenceKey == "ClosureCaptureCodeFix_guidance");
        var operations = await guidanceAction.GetOperationsAsync(CancellationToken.None);
        var applyDocOp = operations.OfType<ApplyChangesOperation>().Single();
        var doc = applyDocOp.ChangedSolution.Projects.SelectMany(p => p.Documents).Single();
        var text = (await doc.GetTextAsync()).ToString();

        Assert.Contains("// RESULT004: Eliminate closure", text);
        Assert.Contains("// Before (allocates closure):   result.Map(x => DoWork(captured, x))", text);
        Assert.DoesNotContain("Stryker", text);
    }

    [Fact]
    public async Task RESULT004_CodeFix_RegistersNoAction_When_InvocationNotFound()
    {
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var solution = workspace.CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .AddDocument(documentId, "Test.cs", SourceText.From("public class Test {}"));
        var document = solution.GetDocument(documentId)!;
        var tree = await document.GetSyntaxTreeAsync();
        var diagnostic = Diagnostic.Create(
            new ClosureCaptureAnalyzer().SupportedDiagnostics[0],
            Location.Create(tree!, new TextSpan(0, 0)),
            "Map",
            1,
            "local");

        var actions = new List<CodeAction>();
        var provider = new ClosureCaptureCodeFix();
        var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
        await provider.RegisterCodeFixesAsync(context);

        Assert.Empty(actions);
    }

    [Fact]
    public async Task RESULT004_CodeFix_RegistersNoAction_When_NoNonStaticLambdas()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method()
    {
        var result = Result.Success(1);
        return result.Map(static x => x + 1);
    }
}";
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var solution = workspace.CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .AddDocument(documentId, "Test.cs", SourceText.From(source));
        var document = solution.GetDocument(documentId)!;
        var tree = await document.GetSyntaxTreeAsync();
        var root = await tree!.GetRootAsync();
        var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
        var diagnostic = Diagnostic.Create(
            new ClosureCaptureAnalyzer().SupportedDiagnostics[0],
            invocation.GetLocation(),
            "Map",
            1,
            "local");

        var actions = new List<CodeAction>();
        var provider = new ClosureCaptureCodeFix();
        var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
        await provider.RegisterCodeFixesAsync(context);

        Assert.Empty(actions);
    }

    [Fact]
    public async Task RESULT004_TriggersOn_AnonymousMethod_WithParameterList()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var result = Result.Success(1);
        int local = 5;
        _ = result.Map(delegate (int x) { return x + local; });
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT004");
        Assert.Contains("local", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT004_TriggersOn_AnonymousMethod_WithoutParameterList()
    {
        const string source = @"

public class TestClass
{
    public void Method()
    {
        var result = Result.Success();
        int local = 5;
        _ = result.Map(delegate { return local; });
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT004");
        Assert.Contains("local", diag.GetMessage());
    }

    [Fact]
    public async Task RESULT004_DoesNotTrigger_When_TypeInGlobalNamespace_HasTrackedMethodName()
    {
        const string source = @"

public class GlobalResult
{
    public void Map(Func<int, int> fn) => fn(1);
    public void Run()
    {
        int local = 5;
        Map(x => x + local);
    }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ClosureCaptureAnalyzer>(source);
        Assert.Empty(diagnostics);
    }
}





