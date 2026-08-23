// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ErrorBuilderMetadataTests
{
    [Fact]
    public void ErrorBuilder_FromError_MetadataOnly()
    {
        var source = Error.Failure("E", "M").WithMetadata("k", "v");
        var builder = source.ToBuilder();
        var copy = builder.Build();
        Assert.True(copy.HasMetadata);
        Assert.False(copy.HasInnerErrors);
    }

    [Fact]
    public void ErrorBuilder_FromError_InnerErrorsOnly()
    {
        var source = Error.Failure("E", "M", Error.Failure("I", "M2"));
        var builder = source.ToBuilder();
        var copy = builder.Build();
        Assert.False(copy.HasMetadata);
        Assert.True(copy.HasInnerErrors);
    }

    [Fact]
    public void ErrorBuilder_WithEnumerableCollections()
    {

        // You MUST assign the returned value, not discard it.
        var meta = new List<KeyValuePair<string, object>> { new("k1", "v1") };
        var meta2 = new List<KeyValuePair<string, object>> { new("k2", "v2") };
        var inner1 = new List<Error> { Error.Failure("I1", "M") };
        var inner2 = new List<Error> { Error.Failure("I2", "M") };

        var e = Error.Create("E", "M")
            .WithMetadata(meta)
            .WithMetadata(meta2)
            .WithInnerErrors(inner1)
            .WithInnerErrors(inner2)
            .Build();

        Assert.Equal(2, e.Metadata.Count);
        Assert.Equal(2, e.InnerErrors.Length);
    }

    [Theory]
    [InlineData(null, "DESC")]
    [InlineData("", "DESC")]
    [InlineData("   ", "DESC")]
    public void ErrorBuilder_Constructor_InvalidCode_Throws(string? code, string desc)
    {
        Assert.ThrowsAny<ArgumentException>(() => Error.Create(code!, desc));
    }

    [Theory]
    [InlineData("CODE", null)]
    [InlineData("CODE", "")]
    [InlineData("CODE", "   ")]
    public void ErrorBuilder_Constructor_InvalidDesc_Throws(string code, string? desc)
    {
        Assert.ThrowsAny<ArgumentException>(() => Error.Create(code, desc!));
    }

    [Fact]
    public void ErrorBuilder_AllProperties_AndChaining()
    {
        var builder = Error.Create("CODE", "DESC")
            .WithType(ErrorType.Domain)
            .WithSeverity(ErrorSeverity.Critical)
            .WithRetryability(ErrorRetryability.Transient)
            .WithDescriptionKey("key.desc")
            .WithCorrelationId("corr-123")
            .WithMetadata("k1", "v1")
            .WithMetadata("k2", 42);

        var err = builder.Build();

        Assert.Equal("CODE", err.Code);
        Assert.Equal("DESC", err.Description);
        Assert.Equal(ErrorType.Domain, err.Type);
        Assert.Equal(ErrorSeverity.Critical, err.Severity);
        Assert.Equal(ErrorRetryability.Transient, err.Retryability);
        Assert.Equal("key.desc", err.DescriptionKey);
        Assert.Equal("corr-123", err.CorrelationId);
        Assert.True(err.HasMetadata);
        Assert.Equal("v1", err.Metadata["k1"]);
        Assert.Equal(42, err.Metadata["k2"]);
        Assert.False(err.HasInnerErrors);
        Assert.Null(err.TraceId);
    }

    [Fact]
    public void ErrorBuilder_WithTraceId_StructAndOverridePrecedence()
    {
        var traceStruct = System.Diagnostics.ActivityTraceId.CreateRandom();

        // Struct only
        var err1 = Error.Create("C", "D").WithTraceId(traceStruct).Build();
        Assert.Equal(traceStruct.ToString(), err1.TraceId);

        // String override only
        var err2 = Error.Create("C", "D").WithTraceId("custom-trace").Build();
        Assert.Equal("custom-trace", err2.TraceId);

        // String override after struct clears struct
        var err3 = Error.Create("C", "D").WithTraceId(traceStruct).WithTraceId("override-trace").Build();
        Assert.Equal("override-trace", err3.TraceId);

        // Struct after string clears string
        var err4 = Error.Create("C", "D").WithTraceId("first").WithTraceId(traceStruct).Build();
        Assert.Equal(traceStruct.ToString(), err4.TraceId);

        // Null traceId
        var err5 = Error.Create("C", "D").WithTraceId((string?)null).Build();
        Assert.Null(err5.TraceId);
    }

    [Fact]
    public void ErrorBuilder_WithInnerError_ChainingAndSingle()
    {
        var i1 = Error.Failure("I1", "D1");
        var i2 = Error.Validation("I2", "D2");

        // Single call (creates array from unset)
        var errSingle = Error.Create("C", "D").WithInnerError(i1).Build();
        Assert.True(errSingle.HasInnerErrors);
        Assert.Single(errSingle.InnerErrors);
        Assert.Equal("I1", errSingle.InnerErrors[0].Code);

        // Chained calls (Add to existing array)
        var errChained = Error.Create("C", "D").WithInnerError(i1).WithInnerError(i2).Build();
        Assert.True(errChained.HasInnerErrors);
        Assert.Equal(2, errChained.InnerErrors.Length);
        Assert.Equal("I1", errChained.InnerErrors[0].Code);
        Assert.Equal("I2", errChained.InnerErrors[1].Code);
    }

    [Fact]
    public void ErrorBuilder_WithInnerErrors_ChainingAndEmpty()
    {
        var list1 = new List<Error> { Error.Failure("I1", "D1") };
        var list2 = new List<Error> { Error.Failure("I2", "D2") };

        // CreateRange from unset
        var err1 = Error.Create("C", "D").WithInnerErrors(list1).Build();
        Assert.Single(err1.InnerErrors);

        // AddRange to existing
        var err2 = Error.Create("C", "D").WithInnerErrors(list1).WithInnerErrors(list2).Build();
        Assert.Equal(2, err2.InnerErrors.Length);

        // Empty list
        var errEmpty = Error.Create("C", "D").WithInnerErrors(Array.Empty<Error>()).Build();
        Assert.False(errEmpty.HasInnerErrors);
        Assert.Empty(errEmpty.InnerErrors);
    }

    [Fact]
    public void ErrorBuilder_FromError_FullCopy()
    {
        var traceStruct = System.Diagnostics.ActivityTraceId.CreateRandom();
        var orig = Error.Failure("ORIG", "DESC")
            .WithTraceId(traceStruct)
            .WithCorrelationId("corr")
            .WithDescriptionKey("key")
            .WithMetadata("k", "v");

        var builder = orig.ToBuilder();
        var copy = builder.Build();

        Assert.Equal(orig.TraceId, copy.TraceId);
        Assert.True(orig.Equals(copy));

        var origWithOverride = Error.Failure("ORIG", "DESC")
            .WithTraceId("custom-trace")
            .WithCorrelationId("corr")
            .WithDescriptionKey("key")
            .WithMetadata("k", "v");
        var copyWithOverride = origWithOverride.ToBuilder().Build();
        Assert.True(origWithOverride.StrictEquals(copyWithOverride));
    }

    [Fact]
    public void Result_TryGetError_ReturnsError()
    {

        var r = Result.Failure(Error.Failure("E", "M"));
        Assert.True(r.TryGetError(out var error, out var isUninitialized));
        Assert.NotNull(error);
        Assert.False(isUninitialized);

        Result u = default;
        Assert.False(u.TryGetError(out _, out var u2));
        Assert.True(u2);
    }

    [Fact]
    public void ResultOfT_TryGetError_ReturnsError()
    {

        var r = Result.Failure<int>(Error.Failure("E", "M"));
        Assert.True(r.TryGetError(out var error, out var isUninitialized));
        Assert.NotNull(error);
        Assert.False(isUninitialized);

        Result<int> u = default;
        Assert.False(u.TryGetError(out _, out var u2));
        Assert.True(u2);
    }

    [Fact]
    public async Task Result_TryAsync_NonGeneric()
    {

        var r1 = await Result.TryAsync(() => Task.CompletedTask, e => Error.Failure("E", "M"));
        r1.ShouldBeSuccess();

        var r2 = await Result.TryAsync((Func<Task>)(() => throw new InvalidOperationException()), e => Error.Failure("E", "M"));
        r2.ShouldBeFailure();

        var r3 = await Result.TryAsync((Func<CancellationToken, Task>)(async ct => { await Task.Yield(); }), e => Error.Failure("E", "M"), default);
        r3.ShouldBeSuccess();

        var r4 = await Result.TryAsync((Func<CancellationToken, Task>)(async ct => { await Task.Yield(); throw new InvalidOperationException(); }), e => Error.Failure("E", "M"), default);
        r4.ShouldBeFailure();
    }

    [Fact]
    public async Task ResultOfT_TryAsync_Generic()
    {

        var r1 = await Result.TryAsync(() => Task.FromResult(42), e => Error.Failure("E", "M"));
        r1.ShouldBeSuccess();

        var r2 = await Result.TryAsync<int>((Func<Task<int>>)(() => throw new InvalidOperationException()), e => Error.Failure("E", "M"));
        r2.ShouldBeFailure();

        var r3 = await Result.TryAsync((Func<CancellationToken, Task<int>>)(async ct => { await Task.Yield(); return 42; }), e => Error.Failure("E", "M"), default);
        r3.ShouldBeSuccess();

        var r4 = await Result.TryAsync<int>((Func<CancellationToken, Task<int>>)(async ct => { await Task.Yield(); throw new InvalidOperationException(); }), e => Error.Failure("E", "M"), default);
        r4.ShouldBeFailure();
    }

    [Fact]
    public void Result_Deconstruct_Uninitialized()
    {
        Result r = default;
        var (s, e) = r;
        Assert.False(s);
        Assert.Equal(WellKnownErrors.UninitializedError, e);
    }

    [Fact]
    public void ResultOfT_Deconstruct_Uninitialized()
    {
        Result<int> r = default;
        var (s, v, e) = r;
        Assert.False(s);
        Assert.Equal(0, v);
        Assert.Equal(WellKnownErrors.UninitializedError, e);
    }
}





