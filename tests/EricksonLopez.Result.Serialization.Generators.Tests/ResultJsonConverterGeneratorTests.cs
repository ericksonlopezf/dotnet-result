// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Result.Serialization.Generators.Tests;

public class ResultJsonConverterGeneratorTests
{
    [Fact]
    public void Ignores_NonPartial_Classes()
    {
        var source = @"
using System.Text.Json.Serialization;
using EricksonLopez.Result;

[JsonSerializable(typeof(Result<int>))]
public class NonPartialContext : JsonSerializerContext
{
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultJsonConverterGenerator>(source);
        diagnostics.Should().BeEmpty();
        sources.Should().BeEmpty();
    }

    [Fact]
    public void Ignores_Classes_NotInheriting_JsonSerializerContext()
    {
        var source = @"

[JsonSerializable(typeof(Result<int>))]
public partial class OtherClass
{
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultJsonConverterGenerator>(source);
        diagnostics.Should().BeEmpty();
        sources.Should().BeEmpty();
    }

    [Fact]
    public void Emits_Warning_RESULT_GEN_001_For_NonGeneric_Result()
    {
        var source = @"

namespace MyApp;

[JsonSerializable(typeof(Result))]
public partial class AppJsonContext : JsonSerializerContext
{
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultJsonConverterGenerator>(source);
        diagnostics.Should().HaveCount(1);
        diagnostics[0].Id.Should().Be("RESULT_GEN_001");
        diagnostics[0].Severity.Should().Be(DiagnosticSeverity.Warning);
        diagnostics[0].Location.Should().NotBe(Location.None);
        sources.Should().BeEmpty();
    }

    [Fact]
    public void Emits_Extension_For_Valid_ResultOfT_In_Namespace()
    {
        var source = @"

namespace MyNamespace;

public class UserDto { public int Id { get; set; } }

[JsonSerializable(typeof(Result<UserDto>))]
public partial class AppContext : JsonSerializerContext
{
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultJsonConverterGenerator>(source);
        diagnostics.Should().BeEmpty();
        sources.Should().HaveCount(1);
        sources[0].HintName.Should().Be("AppContextResultConverters.g.cs");

        var text = sources[0].SourceText.ToString();
        text.Should().Contain("namespace MyNamespace;");
        text.Should().Contain("public static class AppContextResultExtensions");
        text.Should().Contain("public static System.Text.Json.JsonSerializerOptions AddResultConverters(this System.Text.Json.JsonSerializerOptions options)");
        text.Should().Contain("options.Converters.Add(new EricksonLopez.Result.Serialization.ResultJsonConverter());");
        text.Should().Contain("options.Converters.Add(new EricksonLopez.Result.Serialization.ErrorJsonConverter());");
        text.Should().Contain("options.Converters.Add(new EricksonLopez.Result.Serialization.ResultOfTJsonConverter<global::MyNamespace.UserDto>(global::MyNamespace.AppContext.Default.UserDto));");
    }

    [Fact]
    public void Emits_Extension_For_GlobalNamespace_Context()
    {
        var source = @"

public class GlobalDto { }

[JsonSerializable(typeof(Result<GlobalDto>))]
public partial class GlobalContext : JsonSerializerContext
{
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultJsonConverterGenerator>(source);
        diagnostics.Should().BeEmpty();
        sources.Should().HaveCount(1);
        sources[0].HintName.Should().Be("GlobalContextResultConverters.g.cs");

        var text = sources[0].SourceText.ToString();
        text.Should().NotContain("namespace ");
        text.Should().Contain("public static class GlobalContextResultExtensions");
        text.Should().Contain("options.Converters.Add(new EricksonLopez.Result.Serialization.ResultOfTJsonConverter<global::GlobalDto>(global::GlobalContext.Default.GlobalDto));");
    }

    [Fact]
    public void Handles_Complex_Generic_And_Array_Types()
    {
        var source = @"

namespace MyComplexApp;

public class Item { }

[JsonSerializable(typeof(Result<List<Item>>))]
[JsonSerializable(typeof(Result<Item[]>))]
[JsonSerializable(typeof(Result<Dictionary<string, int>>))]
[JsonSerializable(typeof(Result<int?>))]
public partial class ComplexContext : JsonSerializerContext
{
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultJsonConverterGenerator>(source);
        diagnostics.Should().BeEmpty();
        sources.Should().HaveCount(1);

        var text = sources[0].SourceText.ToString();
        text.Should().Contain("ListItem");
        text.Should().Contain("ItemArray");
        text.Should().Contain("DictionaryStringInt32");
        text.Should().Contain("NullableInt32");
    }

    [Fact]
    public void Deduplicates_PropertyNames_When_Namespaces_Collide()
    {
        var source = @"

namespace NsA { public class CollisionDto { } }
namespace NsB { public class CollisionDto { } }

namespace MyApp;

[JsonSerializable(typeof(Result<NsA.CollisionDto>))]
[JsonSerializable(typeof(Result<NsB.CollisionDto>))]
public partial class CollisionContext : JsonSerializerContext
{
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultJsonConverterGenerator>(source);
        diagnostics.Should().BeEmpty();
        sources.Should().HaveCount(1);

        var text = sources[0].SourceText.ToString();
        text.Should().Contain("global::MyApp.CollisionContext.Default.NsA_CollisionDto");
        text.Should().Contain("global::MyApp.CollisionContext.Default.NsB_CollisionDto");
        text.Should().NotContain("global__");
    }

    [Fact]
    public void Ignores_NonResult_Standard_Types_And_Foreign_Result_Types()
    {
        var source = @"

namespace OtherNs { public class Result<T> { } }

namespace MyApp;

public class ValidDto { }

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(OtherNs.Result<int>))]
[JsonSerializable(typeof(EricksonLopez.Result.Error))]
[JsonSerializable(typeof(Result<ValidDto>))]
public partial class MixedTypesContext : JsonSerializerContext
{
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultJsonConverterGenerator>(source);
        diagnostics.Should().BeEmpty();
        sources.Should().HaveCount(1);

        var text = sources[0].SourceText.ToString();
        text.Should().Contain("ResultOfTJsonConverter<global::MyApp.ValidDto>");
        text.Should().NotContain("ResultOfTJsonConverter<global::OtherNs.Result");
        text.Should().NotContain("ResultOfTJsonConverter<global::System.String");
        text.Should().NotContain("ResultOfTJsonConverter<global::EricksonLopez.Result.Error>");
    }

    [Fact]
    public void Ignores_Unrelated_Attributes_And_Handles_Combined_Collision_With_NonGeneric_Result()
    {
        var source = @"

namespace NsA { public class Dto { } }
namespace NsB { public class Dto { } }

namespace MyApp;

[System.ComponentModel.Description(""Test"")]
[Serializable]
[JsonSerializable(typeof(Result))]
[JsonSerializable(typeof(Result<NsA.Dto>))]
[JsonSerializable(typeof(Result<NsB.Dto>))]
public partial class MixedContext : JsonSerializerContext
{
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultJsonConverterGenerator>(source);
        diagnostics.Should().HaveCount(1);
        diagnostics[0].Id.Should().Be("RESULT_GEN_001");
        sources.Should().HaveCount(1);

        var text = sources[0].SourceText.ToString();
        text.Should().Contain("NsA_Dto");
        text.Should().Contain("NsB_Dto");
    }

    [Fact]
    public void ResultTypeInfo_Equality_And_Operators()
    {
        var t1 = new ResultTypeInfo("global::Foo", "Foo");
        var t2 = new ResultTypeInfo("global::Foo", "Foo");
        var t3 = new ResultTypeInfo("global::Bar", "Bar");
        var t4 = new ResultTypeInfo("global::Foo", "Bar");
        var t5 = new ResultTypeInfo("global::Bar", "Foo");

        t1.Equals(t2).Should().BeTrue();
        t1.Equals((object)t2).Should().BeTrue();
        t1.Equals(t3).Should().BeFalse();
        t1.Equals((object)t3).Should().BeFalse();
        t1.Equals(t4).Should().BeFalse();
        t1.Equals(t5).Should().BeFalse();
        t1.Equals((object?)null).Should().BeFalse();
        t1.Equals(new object()).Should().BeFalse();

        (t1 == t2).Should().BeTrue();
        (t1 != t3).Should().BeTrue();
        (t1 == t3).Should().BeFalse();
        (t1 != t2).Should().BeFalse();

        t1.GetHashCode().Should().Be(t2.GetHashCode());
    }

    [Fact]
    public void ContextInfo_Equality_And_Operators()
    {
        var t1 = new ResultTypeInfo("global::Foo", "Foo");
        var t2 = new ResultTypeInfo("global::Bar", "Bar");

        var c1 = new ContextInfo("C", "global::C", "Ns", ImmutableArray.Create(t1));
        var c2 = new ContextInfo("C", "global::C", "Ns", ImmutableArray.Create(t1));
        var c3 = new ContextInfo("Diff", "global::Diff", "Ns", ImmutableArray.Create(t1));
        var c4 = new ContextInfo("C", "global::C", "OtherNs", ImmutableArray.Create(t1));
        var c5 = new ContextInfo("C", "global::C", "Ns", ImmutableArray.Create(t1, t2));
        var c6 = new ContextInfo("C", "global::C", "Ns", ImmutableArray.Create(t2));
        var c7 = new ContextInfo("C", "global::C", null, ImmutableArray.Create(t1));

        c1.Equals(c2).Should().BeTrue();
        c1.Equals(c3).Should().BeFalse();
        c1.Equals(c4).Should().BeFalse();
        c1.Equals(c5).Should().BeFalse();
        c1.Equals(c6).Should().BeFalse();
        c1.Equals(c7).Should().BeFalse();

        c1.GetHashCode().Should().Be(c2.GetHashCode());
        c1.GetHashCode().Should().NotBe(c3.GetHashCode());
        c1.GetHashCode().Should().NotBe(c4.GetHashCode());
        c1.GetHashCode().Should().NotBe(c5.GetHashCode());
        c1.GetHashCode().Should().NotBe(c6.GetHashCode());
        c1.GetHashCode().Should().NotBe(c7.GetHashCode());

        unchecked
        {
            var expected = 17;
            expected = expected * 31 + "C".GetHashCode();
            expected = expected * 31 + "Ns".GetHashCode();
            expected = expected * 31 + "global::Foo".GetHashCode();
            c1.GetHashCode().Should().Be(expected);
        }
    }

    [Fact]
    public void Handles_Indirect_Inheritance_From_JsonSerializerContext()
    {
        var source = @"

public class BaseContext : JsonSerializerContext { }

public class MyDto { }

[JsonSerializable(typeof(Result<MyDto>))]
public partial class DerivedContext : BaseContext
{
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultJsonConverterGenerator>(source);
        diagnostics.Should().BeEmpty();
        sources.Should().HaveCount(1);
    }

    [Fact]
    public void Ignores_JsonSerializable_Attribute_With_Empty_ConstructorArguments()
    {
        var source = @"

public class MyDto { }

[JsonSerializable]
[JsonSerializable(typeof(Result<MyDto>))]
public partial class EmptyArgContext : JsonSerializerContext
{
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultJsonConverterGenerator>(source);
        diagnostics.Should().BeEmpty();
        sources.Should().HaveCount(1);
    }

    [Fact]
    public void Ignores_JsonSerializerContext_With_Only_NonResult_Attributes()
    {
        var source = @"

[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
public partial class OnlyNonResultContext : JsonSerializerContext
{
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultJsonConverterGenerator>(source);
        diagnostics.Should().BeEmpty();
        sources.Should().BeEmpty();
    }

    [Fact]
    public void Ignores_Class_Without_Attributes()
    {
        var source = @"

public partial class NoAttrsContext : JsonSerializerContext
{
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultJsonConverterGenerator>(source);
        diagnostics.Should().BeEmpty();
        sources.Should().BeEmpty();
    }

    [Fact]
    public void Ignores_NonJsonSerializable_Attributes_Containing_Result_Type()
    {
        var source = @"

[AttributeUsage(AttributeTargets.Class)]
public class MyCustomAttribute : Attribute
{
    public MyCustomAttribute(Type t) { }
}

[MyCustom(typeof(Result<int>))]
public partial class CustomAttrContext : JsonSerializerContext
{
}
";
        var (sources, diagnostics) = GeneratorTestHelper.RunGenerator<ResultJsonConverterGenerator>(source);
        diagnostics.Should().BeEmpty();
        sources.Should().BeEmpty();
    }
}





