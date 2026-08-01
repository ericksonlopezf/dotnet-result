using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using EricksonLopez.Result.Serialization.Generators;
using Xunit;
using System.Reflection;

namespace EricksonLopez.Result.Tests.Serialization.Generators;

public class ResultMetricsVersionGeneratorTests
{
    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(AssemblyInformationalVersionAttribute).Assembly.Location)
        };
        return CSharpCompilation.Create("TestAssembly", new[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    [Fact]
    public void Generator_Creates_VersionConstantsClass()
    {
        var source = @"
[assembly: System.Reflection.AssemblyInformationalVersionAttribute(""2.0.0-beta+12345"")]
namespace TestNamespace;
public class MyClass { }
";
        var compilation = CreateCompilation(source);
        var generator = new ResultMetricsVersionGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        Assert.Single(result.GeneratedTrees);
        var generatedText = result.GeneratedTrees[0].GetText().ToString();
        Assert.Contains("internal static class ResultMetricsVersionConstants", generatedText);
        Assert.Contains("internal const string Version = \"2.0.0-beta\";", generatedText);
    }

    [Fact]
    public void Generator_Uses_VersionWithoutPlus()
    {
        var source = @"
[assembly: System.Reflection.AssemblyInformationalVersionAttribute(""3.1.4"")]
namespace TestNamespace;
public class MyClass { }
";
        var compilation = CreateCompilation(source);
        var generator = new ResultMetricsVersionGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        var generatedText = result.GeneratedTrees[0].GetText().ToString();
        Assert.Contains("internal const string Version = \"3.1.4\";", generatedText);
    }

    [Fact]
    public void Generator_FallsBack_To_AssemblyVersion_If_InfoVersion_Is_Missing()
    {
        var source = @"
[assembly: System.Reflection.AssemblyVersionAttribute(""4.5.6.0"")]
namespace TestNamespace;
public class MyClass { }
";
        var compilation = CreateCompilation(source);
        var generator = new ResultMetricsVersionGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        var generatedText = result.GeneratedTrees[0].GetText().ToString();
        Assert.Contains("internal const string Version = \"4.5.6\";", generatedText);
    }

    [Fact]
    public void Generator_FallsBack_To_AssemblyVersion_If_InfoVersion_Is_Empty()
    {
        var source = @"
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("""")]
namespace TestNamespace;
public class MyClass { }
";
        var compilation = CreateCompilation(source);
        var generator = new ResultMetricsVersionGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        var generatedText = result.GeneratedTrees[0].GetText().ToString();
        Assert.Contains("internal const string Version = \"0.0.0\";", generatedText);
    }
}
