// Copyright © Erickson Lopez. MIT License.
using System.Collections.Immutable;
using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace EricksonLopez.Result.DomainErrors.Generators.Tests;

public class DomainErrorsGeneratorTests
{
    [Fact]
    public void Generates_StronglyTyped_Error_Factory_Methods_From_ErrorsJson()
    {
        string json = @"
{
  ""namespace"": ""TestDomain.Errors"",
  ""className"": ""OrderErrors"",
  ""errors"": [
    {
      ""name"": ""NotFound"",
      ""code"": ""Order.NotFound"",
      ""description"": ""Order '{0}' was not found."",
      ""type"": ""NotFound"",
      ""severity"": ""Info"",
      ""retryability"": ""Permanent"",
      ""descriptionKey"": ""order.not_found"",
      ""parameters"": [
        {
          ""name"": ""orderId"",
          ""type"": ""string""
        }
      ]
    },
    {
      ""name"": ""PaymentFailed"",
      ""code"": ""Order.PaymentFailed"",
      ""description"": ""Payment failed for order."",
      ""type"": ""Failure"",
      ""severity"": ""Error"",
      ""retryability"": ""Transient""
    }
  ]
}";

        var compilation = CSharpCompilation.Create("TestCompilation");
        var generator = new DomainErrorsGenerator();
        var additionalText = new TestAdditionalText("order.errors.json", json);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(additionalText));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.Should().BeEmpty();
        var runResult = driver.GetRunResult();
        runResult.GeneratedTrees.Length.Should().Be(1);

        string generatedCode = runResult.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public static partial class OrderErrors");
        generatedCode.Should().Contain("public static global::EricksonLopez.Result.Error NotFound(string orderId)");
        generatedCode.Should().Contain("global::EricksonLopez.Result.Error.Create(\"Order.NotFound\"");
        generatedCode.Should().Contain("WithDescriptionKey(\"order.not_found\")");
        generatedCode.Should().Contain("public static global::EricksonLopez.Result.Error PaymentFailed()");
        generatedCode.Should().Contain("global::EricksonLopez.Result.Error.Create(\"Order.PaymentFailed\"");
        generatedCode.Should().Contain("WithRetryability(global::EricksonLopez.Result.ErrorRetryability.Transient)");
    }

    [Fact]
    public void Ignores_NonMatching_Files()
    {
        string json = "{ \"test\": 123 }";
        var compilation = CSharpCompilation.Create("TestCompilation");
        var generator = new DomainErrorsGenerator();
        var additionalText = new TestAdditionalText("other.json", json);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(additionalText));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        diagnostics.Should().BeEmpty();
        var runResult = driver.GetRunResult();
        runResult.GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public void Generates_All_ErrorTypes_And_Escapes_Strings()
    {
        string json = @"
{
  ""namespace"": ""EscapedDomain.Errors"",
  ""className"": ""SpecialErrors"",
  ""errors"": [
    {
      ""name"": ""ValidationErr"",
      ""code"": ""Special.ValSlash\\path"",
      ""description"": ""Line 1\r\nLine 2 with \\slash"",
      ""type"": ""Validation"",
      ""severity"": ""Warning""
    },
    {
      ""name"": ""ConflictErr"",
      ""code"": ""Special.Conflict"",
      ""description"": ""Conflict occurred"",
      ""type"": ""Conflict"",
      ""retryability"": ""NonRetryable""
    },
    {
      ""name"": ""UnauthorizedErr"",
      ""code"": ""Special.Unauthorized"",
      ""description"": ""Unauthorized"",
      ""type"": ""Unauthorized""
    },
    {
      ""name"": ""ForbiddenErr"",
      ""code"": ""Special.Forbidden"",
      ""description"": ""Forbidden"",
      ""type"": ""Forbidden""
    },
    {
      ""name"": ""UnavailableErr"",
      ""code"": ""Special.Unavailable"",
      ""description"": ""Unavailable"",
      ""type"": ""Unavailable""
    },
    {
      ""name"": ""UnexpectedErr"",
      ""code"": ""Special.Unexpected"",
      ""description"": ""Unexpected"",
      ""type"": ""Unexpected""
    },
    {
      ""name"": ""CustomUnknownTypeErr"",
      ""code"": ""Special.Custom"",
      ""description"": ""Custom fallback"",
      ""type"": ""UnknownType""
    }
  ]
}";

        var compilation = CSharpCompilation.Create("TestCompilation");
        var generator = new DomainErrorsGenerator();
        var additionalText = new TestAdditionalText("special.errors.json", json);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(additionalText));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        diagnostics.Should().BeEmpty();
        var runResult = driver.GetRunResult();
        runResult.GeneratedTrees.Length.Should().Be(1);

        string generatedCode = runResult.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("ErrorType.Validation");
        generatedCode.Should().Contain("ErrorType.Conflict");
        generatedCode.Should().Contain("ErrorType.Unauthorized");
        generatedCode.Should().Contain("ErrorType.Forbidden");
        generatedCode.Should().Contain("ErrorType.Unavailable");
        generatedCode.Should().Contain("ErrorType.Unexpected");
        generatedCode.Should().Contain("Special.ValSlash");
        generatedCode.Should().Contain("Line 1");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DomainErrorJsonParser_TryParse_WhenNullOrWhitespace_ReturnsNull(string? json)
    {
        var result = DomainErrorJsonParser.TryParse(json!);
        result.Should().BeNull();
    }

    [Fact]
    public void DomainErrorJsonParser_TryParse_WhenNoErrorsKey_ReturnsEmptyModel()
    {
        var result = DomainErrorJsonParser.TryParse("{\"namespace\": \"App.NS\", \"className\": \"AppErrors\"}");
        result.Should().NotBeNull();
        result!.NamespaceName.Should().Be("App.NS");
        result.ClassName.Should().Be("AppErrors");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void DomainErrorJsonParser_TryParse_WhenMalformedErrorsArray_ReturnsEmptyModel()
    {
        var result = DomainErrorJsonParser.TryParse("{\"errors\": \"not-array\"}");
        result.Should().NotBeNull();
        result!.NamespaceName.Should().Be("Domain.Errors");
        result.ClassName.Should().Be("DomainErrors");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void DomainErrorJsonParser_TryParse_WhenErrorMissingNameOrCode_SkipsItem()
    {
        string json = @"
{
  ""errors"": [
    { ""name"": """", ""code"": ""Valid.Code"" },
    { ""name"": ""ValidName"", ""code"": """" },
    { ""name"": ""ValidName"", ""code"": ""Valid.Code"" }
  ]
}";
        var result = DomainErrorJsonParser.TryParse(json);
        result.Should().NotBeNull();
        result!.Errors.Length.Should().Be(1);
        result.Errors[0].Name.Should().Be("ValidName");
        result.Errors[0].Code.Should().Be("Valid.Code");
    }

    [Fact]
    public void DomainErrorJsonParser_TryParse_Parameters_EdgeCases()
    {
        string json = @"
{
  ""errors"": [
    {
      ""name"": ""ParamTest"",
      ""code"": ""Test.Code"",
      ""parameters"": [
        { ""name"": """", ""type"": ""string"" },
        { ""name"": ""p1"" },
        { ""name"": ""p2"", ""type"": ""int"" }
      ]
    },
    {
      ""name"": ""NoParams"",
      ""code"": ""Test.NoParams"",
      ""parameters"": ""malformed""
    }
  ]
}";
        var result = DomainErrorJsonParser.TryParse(json);
        result.Should().NotBeNull();
        result!.Errors.Length.Should().Be(2);

        var p1Error = result.Errors[0];
        p1Error.Parameters.Length.Should().Be(2);
        p1Error.Parameters[0].Name.Should().Be("p1");
        p1Error.Parameters[0].Type.Should().Be("string");
        p1Error.Parameters[1].Name.Should().Be("p2");
        p1Error.Parameters[1].Type.Should().Be("int");

        var noParamsError = result.Errors[1];
        noParamsError.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void DomainErrorParameterModel_EqualityAndHashCode()
    {
        var p1 = new DomainErrorParameterModel("param1", "string");
        var p2 = new DomainErrorParameterModel("param1", "string");
        var p3 = new DomainErrorParameterModel("param2", "string");
        var p4 = new DomainErrorParameterModel("param1", "int");
        var pDefault = new DomainErrorParameterModel(null!, null!);

        p1.Equals(p2).Should().BeTrue();
        (p1 == p2).Should().BeFalse(); // reference equality
        p1.Equals((object)p2).Should().BeTrue();
        p1.GetHashCode().Should().Be(p2.GetHashCode());

        p1.Equals(p3).Should().BeFalse();
        p1.Equals(p4).Should().BeFalse();
        DomainErrorParameterModel? nullParam = null;
        p1.Equals(nullParam).Should().BeFalse();
        object? nullObj = null;
        p1.Equals(nullObj).Should().BeFalse();
        p1.Equals(new object()).Should().BeFalse();

        pDefault.Name.Should().Be(string.Empty);
        pDefault.Type.Should().Be("string");
    }

    [Fact]
    public void DomainErrorModel_EqualityAndHashCode()
    {
        var paramA = new DomainErrorParameterModel("id", "int");
        var m1 = new DomainErrorModel("Test", "Code.1", "Desc", "Validation", "Warning", "Transient", "key.1", ImmutableArray.Create(paramA));
        var m2 = new DomainErrorModel("Test", "Code.1", "Desc", "Validation", "Warning", "Transient", "key.1", ImmutableArray.Create(paramA));
        var mDifferentCode = new DomainErrorModel("Test", "Code.2", "Desc", "Validation", "Warning", "Transient", "key.1", ImmutableArray.Create(paramA));
        var mDifferentParams = new DomainErrorModel("Test", "Code.1", "Desc", "Validation", "Warning", "Transient", "key.1", ImmutableArray<DomainErrorParameterModel>.Empty);

        m1.Equals(m2).Should().BeTrue();
        m1.Equals((object)m2).Should().BeTrue();
        m1.GetHashCode().Should().Be(m2.GetHashCode());

        m1.Equals(mDifferentCode).Should().BeFalse();
        m1.Equals(mDifferentParams).Should().BeFalse();
        DomainErrorModel? nullModel = null;
        m1.Equals(nullModel).Should().BeFalse();
        object? nullObj = null;
        m1.Equals(nullObj).Should().BeFalse();
        m1.Equals(new object()).Should().BeFalse();

        var mNullDefaults = new DomainErrorModel(null!, null!, null!, null, null, null, null, default);
        mNullDefaults.Name.Should().Be(string.Empty);
        mNullDefaults.Code.Should().Be(string.Empty);
        mNullDefaults.Description.Should().Be(string.Empty);
        mNullDefaults.Type.Should().Be("Failure");
        mNullDefaults.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void DomainErrorClassModel_EqualityAndHashCode()
    {
        var err = new DomainErrorModel("E1", "C1", "D1", null, null, null, null, ImmutableArray<DomainErrorParameterModel>.Empty);
        var c1 = new DomainErrorClassModel("NS", "Class1", ImmutableArray.Create(err));
        var c2 = new DomainErrorClassModel("NS", "Class1", ImmutableArray.Create(err));
        var cDiffNs = new DomainErrorClassModel("DiffNS", "Class1", ImmutableArray.Create(err));
        var cDiffName = new DomainErrorClassModel("NS", "Class2", ImmutableArray.Create(err));
        var cDiffErrors = new DomainErrorClassModel("NS", "Class1", ImmutableArray<DomainErrorModel>.Empty);

        c1.Equals(c2).Should().BeTrue();
        c1.Equals((object)c2).Should().BeTrue();
        c1.GetHashCode().Should().Be(c2.GetHashCode());

        c1.Equals(cDiffNs).Should().BeFalse();
        c1.Equals(cDiffName).Should().BeFalse();
        c1.Equals(cDiffErrors).Should().BeFalse();
        DomainErrorClassModel? nullClass = null;
        c1.Equals(nullClass).Should().BeFalse();
        object? nullObj = null;
        c1.Equals(nullObj).Should().BeFalse();
        c1.Equals(new object()).Should().BeFalse();

        var cDefault = new DomainErrorClassModel(null!, null!, default);
        cDefault.NamespaceName.Should().Be("Domain.Errors");
        cDefault.ClassName.Should().Be("DomainErrors");
        cDefault.Errors.Should().BeEmpty();
    }
}
