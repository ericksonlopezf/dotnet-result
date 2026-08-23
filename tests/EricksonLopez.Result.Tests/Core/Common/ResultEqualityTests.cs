// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultEqualityTests
{
    [Fact]
    public void Result_EqualityAndInequality_Operators()
    {
        Result r1 = Result.Success();
        Result r2 = Result.Failure(Error.Failure("X", "X"));
        Result r3 = Result.Success();

        Assert.False(r1 == r2);
        Assert.True(r1 != r2);
        Assert.True(r1 == r3);
        Assert.False(r1 != r3);
        Assert.False(((IEquatable<Result>)r1).Equals(r2));
        Assert.True(((IEquatable<Result>)r1).Equals(r3));

        // Boolean evaluation (true/false operators)
        Assert.True(r1 ? true : false);
        Assert.False(r2 ? true : false);
        Result uninit = default;
        Assert.False(uninit ? true : false);
    }

    [Fact]
    public async Task Result_TryAsync_ExceptionHandlingPaths()
    {
        var e = new InvalidOperationException("X");
        var r1 = await Result.TryAsync(() => throw e, ex => Error.Failure("X", ex.Message));
        r1.ShouldBeFailure();
        Assert.Equal("X", r1.Error.Code);

        var r2 = await Result.TryAsync(async () => { await Task.Yield(); throw e; }, ex => Error.Failure("X", ex.Message));
        r2.ShouldBeFailure();
        Assert.Equal("X", r2.Error.Code);

        var r3 = await Result.TryAsync<int>(() => throw e, ex => Error.Failure("X", ex.Message));
        r3.ShouldBeFailure();
        Assert.Equal("X", r3.Error.Code);

        var r4 = await Result.TryAsync<int>(async () => { await Task.Yield(); throw e; }, ex => Error.Failure("X", ex.Message));
        r4.ShouldBeFailure();
        Assert.Equal("X", r4.Error.Code);

        var error = Error.Create("A", "A").WithMetadata("k1", 5).WithMetadata("k2", (object?)null!).Build();
        Assert.True(error.TryGetMetadata<int>("k1", out var k1Val));
        Assert.Equal(5, k1Val);
        Assert.Equal(5, error.GetMetadata<int>("k1"));

        Assert.Throws<InvalidCastException>(() => error.TryGetMetadata<string>("k1", out _));
        Assert.Throws<InvalidCastException>(() => error.GetMetadata<string>("k1"));
    }
}




