// Copyright © Erickson Lopez. MIT License.

using System.Collections.Generic;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Tests.Core.Common;

/// <summary>
/// Verifies <see cref="object.GetHashCode"/> contract compliance across Result types.
/// </summary>
public class GetHashCodeContractTests
{
    // ─── Result (non-generic) ────────────────────────────────────────────────

    [Fact]
    public void GetHashCode_TwoSuccessResults_HaveSameHashCode()
    {
        var r1 = Result.Success();
        var r2 = Result.Success();
        Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_TwoFailureResultsSameError_HaveSameHashCode()
    {
        var error = Error.Failure("Test.Hash", "Hash test error");
        var r1 = Result.Failure(error);
        var r2 = Result.Failure(error);
        Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_SuccessAndFailure_HaveDifferentHashCodes()
    {
        var success = Result.Success();
        var failure = Result.Failure(Error.Failure("X", "Y"));
        Assert.NotEqual(success.GetHashCode(), failure.GetHashCode());
    }

    [Fact]
    public void GetHashCode_UninitializedAndSuccess_HaveDifferentHashCodes()
    {
        var uninitialized = default(Result);
        var success = Result.Success();
        Assert.NotEqual(uninitialized.GetHashCode(), success.GetHashCode());
    }

    [Fact]
    public void GetHashCode_UninitializedAndFailure_HaveDifferentHashCodes()
    {
        var uninitialized = default(Result);
        var failure = Result.Failure(Error.Failure("X", "Y"));
        Assert.NotEqual(uninitialized.GetHashCode(), failure.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ResultUsedAsHashSetKey_FindsEqualResult()
    {
        var error = Error.Failure("Hash.Key", "test");
        var r1 = Result.Failure(error);
        var r2 = Result.Failure(error);
        var set = new HashSet<Result> { r1 };
        Assert.Contains(r2, set);
    }

    [Fact]
    public void GetHashCode_SuccessUsedAsHashSetKey_FindsEqualSuccess()
    {
        var r1 = Result.Success();
        var r2 = Result.Success();
        var set = new HashSet<Result> { r1 };
        Assert.Contains(r2, set);
    }

    [Fact]
    public void GetHashCode_ResultUsedAsDictionaryKey_LooksUpCorrectly()
    {
        var error = Error.Failure("Dict.Key", "test");
        var r1 = Result.Failure(error);
        var r2 = Result.Failure(error);
        var dict = new Dictionary<Result, string> { [r1] = "value" };
        Assert.True(dict.TryGetValue(r2, out var v));
        Assert.Equal("value", v);
    }

    // ─── Result<T> (generic) ─────────────────────────────────────────────────

    [Fact]
    public void ResultOfT_GetHashCode_TwoSuccessWithSameValue_HaveSameHashCode()
    {
        var r1 = Result.Success(42);
        var r2 = Result.Success(42);
        Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
    }

    [Fact]
    public void ResultOfT_GetHashCode_TwoSuccessWithDifferentValues_HaveDifferentHashCodes()
    {
        var r1 = Result.Success(42);
        var r2 = Result.Success(99);
        Assert.NotEqual(r1.GetHashCode(), r2.GetHashCode());
    }

    [Fact]
    public void ResultOfT_GetHashCode_SuccessAndFailure_HaveDifferentHashCodes()
    {
        var success = Result.Success(42);
        var failure = Result.Failure<int>(Error.Failure("X", "Y"));
        Assert.NotEqual(success.GetHashCode(), failure.GetHashCode());
    }

    [Fact]
    public void ResultOfT_GetHashCode_TwoFailuresWithSameError_HaveSameHashCode()
    {
        var error = Error.Failure("Hash.T", "test");
        var r1 = Result.Failure<int>(error);
        var r2 = Result.Failure<int>(error);
        Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
    }

    [Fact]
    public void ResultOfT_GetHashCode_ResultUsedAsHashSetKey_FindsEqualResult()
    {
        var r1 = Result.Success("hello");
        var r2 = Result.Success("hello");
        var set = new HashSet<Result<string>> { r1 };
        Assert.Contains(r2, set);
    }
}
