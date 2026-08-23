// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultTryAsyncTests
{
    [Fact]
    public async Task Result_TryAsync_Coverage()
    {
        var e = new InvalidOperationException("X");
        var r1 = await Result.TryAsync(() => Task.FromException(e), ex => Error.Failure("X", "X"));
        r1.ShouldBeFailure();
        var r2 = await Result.TryAsync<int>(() => Task.FromException<int>(e), ex => Error.Failure("X", "X"));
        r2.ShouldBeFailure();

        var failure = Result.Failure(Error.Failure("A", "B"));
        Assert.Equal(10, failure.MapFailure(5, (s, e) => s * 2, 0));
        var success = Result.Success();
        Assert.Equal(0, success.MapFailure(5, (s, e) => s * 2, 0));
    }

    [Fact]
    public void ResultOfT_Ensure_Coverage()
    {
        var r = Result.Success(5);
        Assert.True(r.Ensure(v => true, v => Error.Failure("X", "X")).IsSuccess);
        Assert.True(r.Ensure(v => false, v => Error.Failure("X", "X")).IsFailure);
        Assert.True(r.Ensure(10, (s, v) => true, (s, v) => Error.Failure("X", "X")).IsSuccess);
        Assert.True(r.Ensure(10, (s, v) => false, (s, v) => Error.Failure("X", "X")).IsFailure);

        var u = default(Result<int>);
        try { u.Ensure(v => true, v => Error.Failure("X", "X")); } catch { }
        try { u.Ensure(10, (s, v) => true, (s, v) => Error.Failure("X", "X")); } catch { }

        var f = Result.Failure<int>(Error.Failure("X", "X"));
        Assert.True(f.Ensure(v => true, v => Error.Failure("X", "X")).IsFailure);
        Assert.True(f.Ensure(10, (s, v) => true, (s, v) => Error.Failure("X", "X")).IsFailure);

        Assert.True(r.TryGetValue(out var val1, out var u1));
        Assert.False(u.TryGetValue(out var val2, out var u2));
        Assert.False(f.TryGetValue(out var val3, out var u3));

        var ioR = (IResultOutcome)r;
        Assert.Null(ioR.Error);
        Assert.Equal(5, ioR.RawValue);
        var ioF2 = (IResultOutcome)f;
        Assert.NotNull(ioF2.Error);
        Assert.Null(ioF2.RawValue);

        var rNull = Result.Success<string>(null!);
        Assert.Equal(HashCode.Combine((byte)1, 0), rNull.GetHashCode());
    }

    [Fact]
    public async Task Result_Map_Cancellation_Coverage()
    {
        var ct = new CancellationToken(true);
        var t = Task.FromResult(Result.Success());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => t.Map(c => Task.FromResult(5), ct));
    }

    [Fact]
    public void Result_Coverage_EdgeCases()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Failure(null!));

        var s = Result.Success();
        Assert.True(s.Ensure(() => true, () => Error.Failure("X", "X")).IsSuccess);
        Assert.True(s.Ensure(() => false, () => Error.Failure("X", "X")).IsFailure);
        Assert.True(s.Ensure(10, (st) => true, () => Error.Failure("X", "X")).IsSuccess);
        Assert.True(s.Ensure(10, (st) => false, () => Error.Failure("X", "X")).IsFailure);

        var f = Result.Failure(Error.Failure("X", "X"));
        Assert.True(f.Ensure(() => true, () => Error.Failure("X", "X")).IsFailure);
        Assert.True(f.Ensure(10, (st) => true, () => Error.Failure("X", "X")).IsFailure);
        Assert.True(f.TryGetError(out var nErr1));
        Assert.True(f.TryGetError(out var nErr2, out var nUn1));

        var ioF = (IResultOutcome)f;
        Assert.NotNull(ioF.Error);
        Assert.Null(ioF.RawValue);

        var ioS = (IResultOutcome)s;
        Assert.Null(ioS.Error);
        Assert.Null(ioS.RawValue);

        var type = typeof(Result);
        var getDebuggerDisplay = type.GetMethod("GetDebuggerDisplay", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        getDebuggerDisplay!.Invoke(f, null);

        // Reflection for branches 804, 821, 879 in Result.cs
        object boxed = new Result();
        // WARNING: Fragile coupling to internal field '_state' for defensive branch coverage.
        type.GetField("_state", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(boxed, (byte)2); // ResultState.Failure is 2
        var badResult = (Result)boxed;

        Assert.True(badResult.TryGetError(out var err1));
        Assert.True(badResult.TryGetError(out var err2, out var un1));

        var getDebuggerDisplay2 = type.GetMethod("GetDebuggerDisplay", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var disp1 = getDebuggerDisplay2!.Invoke(badResult, null);
        Assert.Equal("Failure ()", disp1);

        // Reflection for branches 660, 679 in ResultOfT.cs
        object boxedT = new Result<int>();
        var typeT = typeof(Result<int>);
        // WARNING: Fragile coupling to internal field '_state' for defensive branch coverage.
        typeT.GetField("_state", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(boxedT, (byte)2); // ResultState.Failure
        var badResultT = (Result<int>)boxedT;

        badResultT.GetHashCode();
        var getDebuggerDisplayT = typeT.GetMethod("GetDebuggerDisplay", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var dispT = getDebuggerDisplayT!.Invoke(badResultT, null);
        Assert.Equal("Failure ()", dispT);
    }
}




