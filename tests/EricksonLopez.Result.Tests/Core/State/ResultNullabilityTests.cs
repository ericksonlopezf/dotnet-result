// Copyright © Erickson Lopez. MIT License.
#pragma warning disable CS0619 // Intentionally testing the reflection-based constructor (Obsolete error:true)
#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultNullabilityTests
{

    [Fact]
    public void Success_AcceptsNull_ForNullableReferenceType()
    {
        // Regression (Null Success Values): Previously, Result<string?>.Success(null) threw ArgumentNullException.
        // For nullable reference types, null is a valid success value.
        var result = Result.Success<string?>(null);

        result.ShouldBeSuccess();
        Assert.Null(result.Value);
    }

    [Fact]
    public void Success_AcceptsNull_ForNullableObjectType()
    {
        // Regression (Null Success Values): object? should also accept null as a valid success.
        var result = Result.Success<object?>(null);

        result.ShouldBeSuccess();
        Assert.Null(result.Value);
    }

    [Fact]
    public void Success_WithNonNullValue_StillWorks()
    {
        // Regression (Null Success Values): non-null values still produce a success result.
        var result = Result.Success<string?>("hello");

        result.ShouldBeSuccess();
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void Success_WithValueType_StillWorks()
    {
        // Regression (Null Success Values): value types (non-nullable by nature) are unaffected.
        var result = Result.Success(42);

        result.ShouldBeSuccess();
        Assert.Equal(42, result.Value);
    }

    // Tested via AspNetCore integration tests. Static analysis fix — no unit test needed here.


    [Fact]
    public void ResultOfTJsonConverter_WithJsonTypeInfo_CanSerializeSuccess()
    {
        // Regression (NativeAOT Compatibility): The new JsonTypeInfo<T> constructor provides an AOT-safe serialization path.
        // We simulate this using a source-generated context.
        var typeInfo = AuditTestJsonContext.Default.AuditTestDto;
        var converter = new ResultOfTJsonConverter<AuditTestDto>(typeInfo);

        var options = new JsonSerializerOptions();
        options.Converters.Add(new ErrorJsonConverter());
        options.Converters.Add(converter);

        var dto = new AuditTestDto { Name = "TestName", Value = 42 };
        var result = Result.Success(dto);

        var json = JsonSerializer.Serialize(result, options);
        var deserialized = JsonSerializer.Deserialize<Result<AuditTestDto>>(json, options);

        deserialized.ShouldBeSuccess();
        Assert.Equal("TestName", deserialized.Value.Name);
        Assert.Equal(42, deserialized.Value.Value);
    }

    [Fact]
    public void ResultOfTJsonConverter_WithJsonTypeInfo_CanSerializeFailure()
    {
        // Regression (NativeAOT Compatibility): Failure serialization must still work with the AOT-safe constructor.
        var typeInfo = AuditTestJsonContext.Default.AuditTestDto;
        var converter = new ResultOfTJsonConverter<AuditTestDto>(typeInfo);

        var options = new JsonSerializerOptions();
        options.Converters.Add(new ErrorJsonConverter());
        options.Converters.Add(converter);

        var error = Error.Validation("AUDIT.TEST", "Test validation error");
        var result = Result.Failure<AuditTestDto>(error);

        var json = JsonSerializer.Serialize(result, options);
        var deserialized = JsonSerializer.Deserialize<Result<AuditTestDto>>(json, options);

        deserialized.ShouldBeFailure();
        Assert.Equal("AUDIT.TEST", deserialized.Error.Code);
    }

    [Fact]
    public void ResultOfTJsonConverter_WithJsonTypeInfo_ThrowsOnNullTypeInfo()
    {
        // Regression (NativeAOT Compatibility): Null typeInfo should throw ArgumentNullException immediately.
        Assert.Throws<ArgumentNullException>(() =>
            new ResultOfTJsonConverter<AuditTestDto>(null!));
    }

    [Fact]
    public void ResultOfTJsonConverter_DefaultConstructor_StillWorks()
    {
        // Regression (Parameterless Constructor): the original parameterless constructor must still function.
        var converter = new ResultOfTJsonConverter<AuditTestDto>();
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ErrorJsonConverter());
        options.Converters.Add(converter);

        var dto = new AuditTestDto { Name = "Legacy", Value = 99 };
        var result = Result.Success(dto);
        var json = JsonSerializer.Serialize(result, options);
        var deserialized = JsonSerializer.Deserialize<Result<AuditTestDto>>(json, options);

        deserialized.ShouldBeSuccess();
        Assert.Equal("Legacy", deserialized.Value.Name);
    }


    [Fact]
    public void TryGetError_ReturnsFalse_ForUninitializedResult()
    {
        // Intentional (Uninitialized State API): Documents and verifies the known behavior: TryGetError(out error) returns false
        // for both Success and Uninitialized states. The two-out overload distinguishes them.
        var uninit = default(Result);

        // Single-overload: returns false (does NOT throw, but also does not indicate uninitialized)
        var singleOverloadResult = uninit.TryGetError(out var error);
        Assert.False(singleOverloadResult);
        Assert.Null(error);

        // Two-out overload: correctly identifies uninitialized state
        var twoOverloadResult = uninit.TryGetError(out var error2, out bool isUninitialized);
        Assert.False(twoOverloadResult);
        Assert.Null(error2);
        Assert.True(isUninitialized);
    }

    [Fact]
    public void TryGetErrorOfT_ReturnsFalse_ForUninitializedResult()
    {
        // Intentional (Uninitialized State API): Same behavior for Result<T>
        var uninit = default(Result<string>);

        var singleOverloadResult = uninit.TryGetError(out var error);
        Assert.False(singleOverloadResult);
        Assert.Null(error);

        var twoOverloadResult = uninit.TryGetError(out var error2, out bool isUninitialized);
        Assert.False(twoOverloadResult);
        Assert.Null(error2);
        Assert.True(isUninitialized);
    }


    [Fact]
    public void CombineOfT_WithFailures_DoesNotLeakMemory_Functionally()
    {
        // Regression (Combine Method): Verify that Combine<T> with partial successes and failures
        // still produces the correct result (ensures pooling refactor didn't break correctness).
        var r1 = Result.Success(1);
        var r2 = Result.Failure<int>(Error.Failure("F1", "Failure 1"));
        var r3 = Result.Success(3);
        var r4 = Result.Failure<int>(Error.Failure("F2", "Failure 2"));

        var combined = Result.Combine(r1, r2, r3, r4);

        combined.ShouldBeFailure();
        // Should have a compound error with 2 inner errors
        Assert.Equal(2, combined.Error.InnerErrors.Length);
    }

    [Fact]
    public void CombineOfT_AllSuccess_ReturnsAllValues()
    {
        // Regression (Object Pooling): success path must still return correct values after pooling refactor.
        // Use explicit type parameter to force the params ReadOnlySpan<Result<T>> overload,
        // not the Combine<T1,T2,T3> tuple overload.
        var r1 = Result.Success(10);
        var r2 = Result.Success(20);
        var r3 = Result.Success(30);

        var combined = Result.Combine<int>(r1, r2, r3);

        combined.ShouldBeSuccess();
        Assert.Equal(3, combined.Value.Count);
        Assert.Contains(10, (IEnumerable<int>)combined.Value);
        Assert.Contains(20, (IEnumerable<int>)combined.Value);
        Assert.Contains(30, (IEnumerable<int>)combined.Value);
    }
}
