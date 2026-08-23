// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result.Analyzers;
using Xunit;

namespace EricksonLopez.Result.Analyzers.Tests;

public class LargeResultValueAnalyzerTests
{
    [Fact]
    public async Task RESULT001_TriggersOn_LargeStruct_ReturnType()
    {
        const string source = @"
using EricksonLopez.Result;

public struct LargeStruct
{
    public long A, B, C, D, E; // 5 * 8 = 40 bytes
}

public class TestClass
{
    public Result<LargeStruct> Method() => default(LargeStruct);
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_DoesNotTrigger_On_SmallStruct_ReturnType()
    {
        const string source = @"

public struct SmallStruct
{
    public long A, B, C; // 3 * 8 = 24 bytes (<= 32)
}

public class TestClass
{
    public Result<SmallStruct> Method() => default(SmallStruct);
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_DoesNotTrigger_On_Class_ReturnType()
    {
        const string source = @"

public class LargeClass
{
    public long A, B, C, D, E, F, G;
}

public class TestClass
{
    public Result<LargeClass> Method() => new LargeClass();
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_TriggersOn_LargeStruct_Parameter()
    {
        const string source = @"

public struct LargeStruct
{
    public long A, B, C, D, E; // 40 bytes
}

public class TestClass
{
    public void Method(Result<LargeStruct> p) { }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_TriggersOn_LargeStruct_Property()
    {
        const string source = @"

public struct LargeStruct
{
    public long A, B, C, D, E; // 40 bytes
}

public class TestClass
{
    public Result<LargeStruct> Property { get; set; }
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        // Expecting 3 diagnostics: 1 for property symbol, 1 for getter return type, 1 for setter parameter
        var matching = diagnostics.Where(d => d.Id == "RESULT001").ToList();
        Assert.Equal(3, matching.Count);
        Assert.All(matching, d => Assert.True(d.Location.IsInSource));
    }

    [Fact]
    public async Task RESULT001_DoesNotTrigger_On_SameNamespace_DifferentTypeName()
    {
        const string source = @"
namespace EricksonLopez.Result
{
    public struct OtherType<T> { }
}

public struct LargeStruct
{
    public long A, B, C, D, E;
}

public class TestClass
{
    public EricksonLopez.Result.OtherType<LargeStruct> Method() => default;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_Estimates_Decimal_Alignment_Accurately()
    {
        const string source = @"

public struct DecimalStruct
{
    public decimal Dec1; // 16 bytes, alignment 8
    public decimal Dec2; // 16 bytes, alignment 8
    public int Int1;     // 4 bytes, alignment 4 -> total 36 -> padded to 40 bytes with maxAlignment 8
}

public class TestClass
{
    public Result<DecimalStruct> Method() => default;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT001");
        Assert.Contains("40+ bytes", diag.GetMessage());
        Assert.True(diag.Location.IsInSource);
    }

    [Fact]
    public async Task RESULT001_Estimates_MaxAlignment_Padding_Accurately()
    {
        const string source = @"

public struct MaxAlignStruct
{
    public byte B;              // 1 byte (aligned to 1)
    public long L1, L2, L3, L4; // 32 bytes (padded to 8, then 32 bytes = 40)
    public int I;               // 4 bytes (offset 44 -> padded to 48 bytes due to maxAlignment 8)
}

public class TestClass
{
    public Result<MaxAlignStruct> Method() => default;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT001");
        Assert.Contains("48+ bytes", diag.GetMessage());
        Assert.True(diag.Location.IsInSource);
    }

    [Fact]
    public async Task RESULT001_TriggersOn_LargeStruct_Field()
    {
        const string source = @"

public struct LargeStruct
{
    public long A, B, C, D, E; // 40 bytes
}

public class TestClass
{
    public Result<LargeStruct> Field;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT001");
        Assert.True(diag.Location.IsInSource);
    }

    [Fact]
    public async Task RESULT001_TriggersOn_AllFieldTypes_WhenExceeding32Bytes()
    {
        const string source = @"

public struct ComplexLargeStruct
{
    public bool BoolField;
    public byte ByteField;
    public sbyte SByteField;
    public char CharField;
    public short ShortField;
    public ushort UShortField;
    public int IntField;
    public uint UIntField;
    public float FloatField;
    public ulong ULongField;
    public double DoubleField;
    public decimal DecimalField;
    public DateTime DateTimeField;
    public string StringRefField;
    public object ObjectRefField;
    public static int StaticField = 42;
    public const int ConstField = 100;
}

public class TestClass
{
    public Result<ComplexLargeStruct> Method() => default;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_TriggersOn_NestedStruct()
    {
        const string source = @"

public struct InnerStruct
{
    public long A, B, C; // 24 bytes
}

public struct OuterStruct
{
    public InnerStruct Inner1; // 24 bytes
    public InnerStruct Inner2; // 24 bytes (total 48 > 32)
}

public class TestClass
{
    public Result<OuterStruct> Method() => default;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_DoesNotTrigger_On_PrimitiveSpecialType_Result()
    {
        const string source = @"

public class TestClass
{
    public Result<int> Method() => 1;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_DoesNotTrigger_On_NonResult_GenericType()
    {
        const string source = @"

public struct LargeStruct
{
    public long A, B, C, D, E;
}

public class TestClass
{
    public List<LargeStruct> Method() => null!;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_DoesNotTrigger_On_OtherNamespace_Result()
    {
        const string source = @"
namespace OtherNamespace
{
    public struct Result<T> { }
}

public struct LargeStruct
{
    public long A, B, C, D, E;
}

public class TestClass
{
    public OtherNamespace.Result<LargeStruct> Method() => default;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_DoesNotTrigger_On_NonGeneric_Result()
    {
        const string source = @"

public class TestClass
{
    public Result Method() => Result.Success();
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_DoesNotTrigger_On_Struct_Exactly32Bytes_Boundary()
    {
        // 4 long fields = 32 bytes exactly, should NOT trigger (threshold is > 32)
        const string source = @"

public struct Exactly32BytesStruct
{
    public long A;
    public long B;
    public long C;
    public long D;
}

public class TestClass
{
    public Result<Exactly32BytesStruct> Method() => default;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_Reports_Correct_Diagnostic_Message_And_Size()
    {
        const string source = @"

public struct FortyByteStruct
{
    public long A, B, C, D, E; // 40 bytes
}

public class TestClass
{
    public Result<FortyByteStruct> Method() => default;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        var diag = Assert.Single(diagnostics, d => d.Id == "RESULT001");
        var message = diag.GetMessage();
        Assert.Contains("Result<FortyByteStruct>", message);
        Assert.Contains("40+ bytes", message);
    }

    [Fact]
    public void RESULT001_Descriptor_Properties_Are_Correct()
    {
        var analyzer = new LargeResultValueAnalyzer();
        var descriptor = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("RESULT001", descriptor.Id);
        Assert.Equal("Result<T> value type is excessively large", descriptor.Title.ToString());
        Assert.Equal("Performance", descriptor.Category);
        Assert.Equal(Microsoft.CodeAnalysis.DiagnosticSeverity.Warning, descriptor.DefaultSeverity);
        Assert.True(descriptor.IsEnabledByDefault);
        Assert.Equal("https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/performance.md#large-struct-warning", descriptor.HelpLinkUri);
        Assert.NotNull(descriptor.Description.ToString());
        Assert.Contains("Result<T> is a readonly struct", descriptor.Description.ToString());
        Assert.Contains("Result<{0}> uses a struct value type estimated at {1}+ bytes", descriptor.MessageFormat.ToString());
    }

    [Fact]
    public async Task RESULT001_DoesNotTrigger_On_GlobalNamespace_Result()
    {
        const string source = @"
public struct Result<T> { }

public struct LargeStruct
{
    public long A, B, C, D, E;
}

public class TestClass
{
    public Result<LargeStruct> Method() => default;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_TriggersOn_StructWithPointerField()
    {
        const string source = @"

public struct LargeStructWithPointer
{
    public unsafe int* Ptr;
    public long A, B, C, D, E;
}

public class TestClass
{
    public Result<LargeStructWithPointer> Method() => default;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.Contains(diagnostics, d => d.Id == "RESULT001");
    }

    [Fact]
    public async Task RESULT001_DoesNotTrigger_On_ArrayType()
    {
        const string source = @"

public class TestClass
{
    public Result<byte[]> Method() => default;
}";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<LargeResultValueAnalyzer>(source);
        Assert.DoesNotContain(diagnostics, d => d.Id == "RESULT001");
    }
}




