using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class MissingCoverageTests
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
        Assert.True(r1.IsSuccess);

        var r2 = await Result.TryAsync((Func<Task>)(() => throw new InvalidOperationException()), e => Error.Failure("E", "M"));
        Assert.True(r2.IsFailure);
        
        var r3 = await Result.TryAsync((Func<CancellationToken, Task>)(async ct => { await Task.Yield(); }), e => Error.Failure("E", "M"), default);
        Assert.True(r3.IsSuccess);

        var r4 = await Result.TryAsync((Func<CancellationToken, Task>)(async ct => { await Task.Yield(); throw new InvalidOperationException(); }), e => Error.Failure("E", "M"), default);
        Assert.True(r4.IsFailure);
    }

    [Fact]
    public async Task ResultOfT_TryAsync_Generic()
    {

        var r1 = await Result.TryAsync(() => Task.FromResult(42), e => Error.Failure("E", "M"));
        Assert.True(r1.IsSuccess);

        var r2 = await Result.TryAsync<int>((Func<Task<int>>)(() => throw new InvalidOperationException()), e => Error.Failure("E", "M"));
        Assert.True(r2.IsFailure);
        
        var r3 = await Result.TryAsync((Func<CancellationToken, Task<int>>)(async ct => { await Task.Yield(); return 42; }), e => Error.Failure("E", "M"), default);
        Assert.True(r3.IsSuccess);

        var r4 = await Result.TryAsync<int>((Func<CancellationToken, Task<int>>)(async ct => { await Task.Yield(); throw new InvalidOperationException(); }), e => Error.Failure("E", "M"), default);
        Assert.True(r4.IsFailure);
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
