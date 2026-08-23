// Copyright © Erickson Lopez. MIT License.
using System;
using System.Runtime.CompilerServices;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

#nullable enable
namespace EricksonLopez.Result.Tests.Monad;

public class ResultStateTests
{
    [Fact]
    public void GetHashCode_Failure_DiffersFromSuccess()
    {
        var success = Result.Success();
        var failure = Result.Failure(Error.Failure("err", "msg"));
        Assert.NotEqual(success.GetHashCode(), failure.GetHashCode());
    }

    [Fact]
    public void UninitializedError_HasExactCodeAndDescription()
    {
        Assert.Equal("Result.Uninitialized", WellKnownErrors.UninitializedError.Code);
        Assert.Equal("Cannot access an uninitialized default Result.", WellKnownErrors.UninitializedError.Description);
    }

    [Fact]
    public void SuccessResult_StateFlags_AreCorrect()
    {
        var r = Result.Success();
        Assert.True(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.False(r.IsUninitialized);
    }

    [Fact]
    public void FailureResult_StateFlags_AreCorrect()
    {
        var r = Result.Failure(Error.Failure("E", "M"));
        Assert.False(r.IsSuccess);
        Assert.True(r.IsFailure);
        Assert.False(r.IsUninitialized);
    }

    [Fact]
    public void UninitializedResult_StateFlags_AreCorrect()
    {
        Result r = default;
        Assert.False(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.True(r.IsUninitialized);
    }

    [Fact]
    public void SuccessResultOfT_StateFlags_AreCorrect()
    {
        var r = Result.Success(1);
        Assert.True(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.False(r.IsUninitialized);
    }

    [Fact]
    public void FailureResultOfT_StateFlags_AreCorrect()
    {
        var r = Result.Failure<int>(Error.Failure("E", "M"));
        Assert.False(r.IsSuccess);
        Assert.True(r.IsFailure);
        Assert.False(r.IsUninitialized);
    }

    [Fact]
    public void UninitializedResultOfT_StateFlags_AreCorrect()
    {
        Result<int> r = default;
        Assert.False(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.True(r.IsUninitialized);
    }
}

