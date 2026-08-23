// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core.Performance;

/// <summary>
/// Regression and quality test suite ensuring that the core Result and Result{T} value-type operations
/// remain strictly allocation-free (zero bytes allocated on the heap) when used with value types and stateful delegates.
/// </summary>
public class ResultZeroAllocationTests
{
    private static readonly Error CachedError = Error.Failure("ZeroAlloc.Test", "Cached test error");

    private static void AssertZeroAllocations(Action action, int iterations = 100)
    {
        // 1. Warm-up JIT and any static delegates/types
        for (int i = 0; i < 10; i++)
        {
            action();
        }

        // 2. Measure strictly on the current thread
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < iterations; i++)
        {
            action();
        }
        long after = GC.GetAllocatedBytesForCurrentThread();

        long allocated = after - before;
        allocated.Should().Be(0, $"Expected 0 bytes allocated across {iterations} iterations, but got {allocated} bytes.");
    }

    [Fact]
    public void Construction_NonGenericSuccess_AllocatesZeroBytes()
    {
        AssertZeroAllocations(() =>
        {
            var r = Result.Success();
            _ = r.IsSuccess;
        });
    }

    [Fact]
    public void Construction_GenericSuccess_ValueType_AllocatesZeroBytes()
    {
        AssertZeroAllocations(() =>
        {
            var r = Result.Success(42);
            _ = r.Value;
        });
    }

    [Fact]
    public void Construction_NonGenericFailure_WithCachedError_AllocatesZeroBytes()
    {
        AssertZeroAllocations(() =>
        {
            var r = Result.Failure(CachedError);
            _ = r.IsFailure;
        });
    }

    [Fact]
    public void Construction_GenericFailure_WithCachedError_AllocatesZeroBytes()
    {
        AssertZeroAllocations(() =>
        {
            var r = Result.Failure<int>(CachedError);
            _ = r.IsFailure;
        });
    }

    [Fact]
    public void Conversions_ImplicitValueAndError_AllocatesZeroBytes()
    {
        AssertZeroAllocations(() =>
        {
            Result<int> rVal = 42;
            Result<int> rErr = CachedError;
            _ = rVal.IsSuccess;
            _ = rErr.IsFailure;
        });
    }

    [Fact]
    public void PropertiesAndDeconstruct_AllocatesZeroBytes()
    {
        var success = Result.Success(42);
        var failure = Result.Failure<int>(CachedError);

        AssertZeroAllocations(() =>
        {
            _ = success.IsSuccess;
            _ = success.IsFailure;
            _ = success.IsUninitialized;
            _ = failure.IsSuccess;
            _ = failure.IsFailure;
            _ = failure.IsUninitialized;

            success.Deconstruct(out bool sOk, out int sVal, out Error? sErr);
            failure.Deconstruct(out bool fOk, out int fVal, out Error? fErr);
            _ = sOk && sVal == 42 && sErr is null;
            _ = !fOk && fVal == 0 && fErr is not null;
        });
    }

    [Fact]
    public void TryGetError_BothOverloads_AllocatesZeroBytes()
    {
        var success = Result.Success();
        var failure = Result.Failure(CachedError);
        var uninit = default(Result);

        AssertZeroAllocations(() =>
        {
            _ = success.TryGetError(out _);
            _ = failure.TryGetError(out _);
            _ = uninit.TryGetError(out _, out _);
        });
    }

    [Fact]
    public void Equality_OperatorsAndEquals_AllocatesZeroBytes()
    {
        var r1 = Result.Success(42);
        var r2 = Result.Success(42);
        var r3 = Result.Failure<int>(CachedError);

        AssertZeroAllocations(() =>
        {
            _ = r1 == r2;
            _ = r1 != r3;
            _ = r1.Equals(r2);
        });
    }

    [Fact]
    public void Map_WithState_AllocatesZeroBytes()
    {
        var result = Result.Success(10);

        AssertZeroAllocations(() =>
        {
            var mapped = result.Map(5, static (state, val) => val + state);
            _ = mapped.Value;
        });
    }

    [Fact]
    public void Bind_WithState_AllocatesZeroBytes()
    {
        var result = Result.Success(10);

        AssertZeroAllocations(() =>
        {
            var bound = result.Bind(5, static (state, val) => Result.Success(val + state));
            _ = bound.Value;
        });
    }

    [Fact]
    public void Ensure_WithState_AllocatesZeroBytes()
    {
        var result = Result.Success(10);

        AssertZeroAllocations(() =>
        {
            var ensured = result.Ensure(5, static (state, val) => val > state, CachedError);
            _ = ensured.IsSuccess;
        });
    }

    [Fact]
    public void TapOnSuccess_WithState_AllocatesZeroBytes()
    {
        var result = Result.Success(10);

        AssertZeroAllocations(() =>
        {
            var tapped = result.TapOnSuccess(5, static (state, val) => { });
            _ = tapped.IsSuccess;
        });
    }

    [Fact]
    public void TapOnFailure_WithState_AllocatesZeroBytes()
    {
        var result = Result.Failure<int>(CachedError);

        AssertZeroAllocations(() =>
        {
            var tapped = result.TapOnFailure(5, static (state, err) => { });
            _ = tapped.IsFailure;
        });
    }

    [Fact]
    public void Match_WithStaticFunctions_AllocatesZeroBytes()
    {
        var result = Result.Success(10);

        AssertZeroAllocations(() =>
        {
            int matched = result.Match(
                static val => val * 2,
                static err => -1);
            _ = matched;
        });
    }

    [Fact]
    public void Execute_WithStaticFunctions_AllocatesZeroBytes()
    {
        var result = Result.Success(10);

        AssertZeroAllocations(() =>
        {
            result.Execute(
                static val => { },
                static err => { });
        });
    }
}
