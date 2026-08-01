using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using EricksonLopez.Result.Serialization.Generators;
using Xunit;

namespace EricksonLopez.Result.Tests.Serialization.Generators;

public class ResultJsonConverterGeneratorTests
{
    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(JsonSerializerContext).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(EricksonLopez.Result.Result).Assembly.Location)
        };
        var runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
        references.Add(MetadataReference.CreateFromFile(System.IO.Path.Combine(runtimeDir, "System.Runtime.dll")));
        references.Add(MetadataReference.CreateFromFile(System.IO.Path.Combine(runtimeDir, "System.Collections.dll")));

        return CSharpCompilation.Create("TestAssembly", new[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    [Fact]
    public void Generator_Creates_ExtensionsClass_For_Context()
    {
        var source = @"
using System.Text.Json.Serialization;
using EricksonLopez.Result;

namespace TestNamespace;

[JsonSerializable(typeof(Result<string>))]
public partial class MyContext : JsonSerializerContext
{
}
";
        var compilation = CreateCompilation(source);
        var generator = new ResultJsonConverterGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        Assert.Single(result.GeneratedTrees);
        var generatedText = result.GeneratedTrees[0].GetText().ToString();
        Assert.Contains("public static class MyContextResultExtensions", generatedText);
        Assert.Contains("options.Converters.Add(new EricksonLopez.Result.Serialization.ResultOfTJsonConverter<string>(global::TestNamespace.MyContext.Default.String));", generatedText);
    }

    [Fact]
    public void Generator_Handles_ComplexTypes_And_Arrays()
    {
        var source = @"
using System.Text.Json.Serialization;
using System.Collections.Generic;
using EricksonLopez.Result;

[JsonSerializable(typeof(Result<List<string>>))]
[JsonSerializable(typeof(Result<int[]>))]
public partial class GlobalContext : JsonSerializerContext
{
}
";
        var compilation = CreateCompilation(source);
        var generator = new ResultJsonConverterGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        Assert.Single(result.GeneratedTrees);
        var generatedText = result.GeneratedTrees[0].GetText().ToString();
        Assert.Contains("public static class GlobalContextResultExtensions", generatedText);
        Assert.Contains("ResultOfTJsonConverter<global::System.Collections.Generic.List<string>>(global::GlobalContext.Default.ListString)", generatedText);
        Assert.Contains("ResultOfTJsonConverter<int[]>(global::GlobalContext.Default.Int32Array)", generatedText);
    }

    [Fact]
    public void Generator_Handles_Collisions()
    {
        var source = @"
using System.Text.Json.Serialization;
using EricksonLopez.Result;
namespace Ns1 { public class Dto {} }
namespace Ns2 { public class Dto {} }

namespace TestNamespace;
[JsonSerializable(typeof(Result<Ns1.Dto>))]
[JsonSerializable(typeof(Result<Ns2.Dto>))]
[JsonSerializable(typeof(Result))]
public partial class CollisionContext : JsonSerializerContext
{
}
";
        var compilation = CreateCompilation(source);
        var generator = new ResultJsonConverterGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        Assert.Single(result.GeneratedTrees);
        var generatedText = result.GeneratedTrees[0].GetText().ToString();
        Assert.Contains("ResultOfTJsonConverter<global::Ns1.Dto>(global::TestNamespace.CollisionContext.Default.Ns1_Dto)", generatedText);
        Assert.Contains("ResultOfTJsonConverter<global::Ns2.Dto>(global::TestNamespace.CollisionContext.Default.Ns2_Dto)", generatedText);
    }

    [Fact]
    public void Generator_Warns_On_NonGenericResult()
    {
        var source = @"
using System.Text.Json.Serialization;
using EricksonLopez.Result;

namespace TestNamespace;

[JsonSerializable(typeof(Result))]
public partial class WarningContext : JsonSerializerContext
{
}
";
        var compilation = CreateCompilation(source);
        var generator = new ResultJsonConverterGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // The generator shouldn't produce a tree for just the non-generic Result
        Assert.Empty(result.GeneratedTrees);
        
        // It should produce a diagnostic
        Assert.Single(result.Diagnostics);
        Assert.Equal("RESULT_GEN_001", result.Diagnostics[0].Id);
    }

    [Fact]
    public void Generator_Warns_On_NonGenericResult_And_Generates_For_Generic()
    {
        var source = @"
using System.Text.Json.Serialization;
using EricksonLopez.Result;

namespace TestNamespace;

[JsonSerializable(typeof(Result))]
[JsonSerializable(typeof(Result<string>))]
public partial class MixedContext : JsonSerializerContext
{
}
";
        var compilation = CreateCompilation(source);
        var generator = new ResultJsonConverterGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        Assert.Single(result.GeneratedTrees);
        Assert.Single(result.Diagnostics);
        Assert.Equal("RESULT_GEN_001", result.Diagnostics[0].Id);
    }

    [Fact]
    public void Generator_Ignores_NonContextClasses()
    {
        var source = @"
using System.Text.Json.Serialization;
using EricksonLopez.Result;

namespace TestNamespace;

[JsonSerializable(typeof(Result<string>))]
public partial class MyClass // Doesn't inherit from JsonSerializerContext
{
}
";
        var compilation = CreateCompilation(source);
        var generator = new ResultJsonConverterGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        Assert.Empty(result.GeneratedTrees);
    }
}
