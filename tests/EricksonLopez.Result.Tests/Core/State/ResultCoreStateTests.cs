// Copyright © Erickson Lopez. MIT License.
using System;
using System.Runtime.CompilerServices;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultCoreStateTests
{
    [Fact]
    public void DefaultResult_IsUninitialized_ThrowsOnOperations()
    {
        Result result = default;

        Assert.False(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.True(result.IsUninitialized);

        Assert.Equal(WellKnownErrors.UninitializedError, result.Error);

        // Regression (Pipeline Validation): Pipeline methods now throw instead of silently propagating
        Assert.Throws<InvalidOperationException>(() => result.Match(() => 1, _ => 0));
        Assert.Throws<InvalidOperationException>(() => result.Execute(() => { }, _ => { }));
        Assert.Throws<InvalidOperationException>(() => result.Ensure(() => true, Error.Failure("E", "M")));
        Assert.Throws<InvalidOperationException>(() => result.Bind(() => Result.Success()));
    }

    [Fact]
    public void DefaultResultT_IsUninitialized_ThrowsOnOperations()
    {
        Result<string> result = default;

        Assert.False(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.True(result.IsUninitialized);

        Assert.Throws<InvalidOperationException>(() => { _ = result.Value; });
        Assert.Equal(WellKnownErrors.UninitializedError, result.Error);

        // Regression (Pipeline Validation): Pipeline methods now throw instead of silently propagating
        Assert.Throws<InvalidOperationException>(() => result.Match(v => v, e => e.Code));
        Assert.Throws<InvalidOperationException>(() => result.Map(v => v.Length));
        Assert.Throws<InvalidOperationException>(() => result.Bind(v => Result.Success(v)));
        Assert.Throws<InvalidOperationException>(() => result.Ensure(v => v.Length > 0, Error.Failure("E", "M")));
    }

    [Fact]
    public void Success_ReturnsSuccessResult()
    {
        var result = Result.Success();

        result.ShouldBeSuccess();
        Assert.False(result.IsFailure);
        Assert.False(result.IsUninitialized);
    }

    [Fact]
    public void Failure_ReturnsFailureResult()
    {
        var error = Error.Failure("Test.Code", "Test description");
        var result = Result.Failure(error);

        var returnedError = result.ShouldBeFailure();
        Assert.Equal(error, returnedError);
    }

    [Fact]
    public void ResultT_IsZeroAllocation_SizeMatchesOptimizedStruct()
    {
        var sizeOfResult = Unsafe.SizeOf<Result<int>>();
        Assert.True(sizeOfResult <= 32, $"Size of Result<int> was {sizeOfResult}, expected <= 32");
    }

    [Fact]
    public void Result_IsZeroAllocation_SizeMatchesOptimizedStruct()
    {
        var sizeOfResult = Unsafe.SizeOf<Result>();
        Assert.True(sizeOfResult <= 24, $"Size of Result was {sizeOfResult}, expected <= 24");
    }

    [Fact]
    public void ErrorProperty_SuccessResult_ThrowsWithMessage()
    {
        var result = Result.Success();
        var ex = Assert.Throws<InvalidOperationException>(() => result.Error);
        Assert.Equal("Cannot access the Error of a successful result.", ex.Message);
    }

    [Fact]
    public void TryGetError_WithState_Failure_ReturnsError()
    {
        var result = Result.Failure(Error.Failure("err", "msg"));
        var b = result.TryGetError(out var error, out var isUninitialized);
        Assert.True(b);
        Assert.False(isUninitialized);
        Assert.NotNull(error);
        Assert.Equal("err", error!.Code);
    }

    [Fact]
    public void TryGetError_WhenFailure_ReturnsTrueAndError()
    {
        var f = Result.Failure(Error.Failure("X", "X"));
        Assert.True(f.TryGetError(out var nErr1));
        Assert.Equal("X", nErr1?.Code);
        Assert.True(f.TryGetError(out var nErr2, out var nUn1));
        Assert.False(nUn1);
    }

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

    [Fact]
    public void DefaultResult_IsUninitialized()
    {
        Result r = default;
        Assert.True(r.IsUninitialized);
        Assert.False(r.IsSuccess);
        Assert.False(r.IsFailure);

        // operator true/false
        bool isTrue = r ? true : false;
        Assert.False(isTrue);

        if (r) { Assert.Fail("operator true on default returned true"); }
        else { Assert.True(true); }
    }

    [Fact]
    public void DefaultResultT_IsUninitialized()
    {
        Result<int> r = default;
        Assert.True(r.IsUninitialized);
        Assert.False(r.IsSuccess);
        Assert.False(r.IsFailure);
    }

    [Fact]
    public void DefaultResult_ThrowsOnMethods()
    {
        Result r = default;
        Assert.Equal(WellKnownErrors.UninitializedError, r.Error);

        Assert.Throws<InvalidOperationException>(() => r.Match(() => 1, e => 2));
        Assert.Throws<InvalidOperationException>(() => r.Match(0, (s) => 1, (s, e) => 2));

        Assert.Throws<InvalidOperationException>(() => r.Execute(() => { }, e => { }));
        Assert.Throws<InvalidOperationException>(() => r.Execute(0, (s) => { }, (s, e) => { }));

        Assert.Throws<InvalidOperationException>(() => r.MapFailure(e => 1, 0));
        Assert.Throws<InvalidOperationException>(() => r.MapFailure(0, (s, e) => 1, 0));
    }

    [Fact]
    public void DefaultResultT_ThrowsOnMethods()
    {
        Result<int> r = default;
        Assert.Equal(WellKnownErrors.UninitializedError, r.Error);

        Assert.Throws<InvalidOperationException>(() => { _ = r.Value; });

        Assert.Throws<InvalidOperationException>(() => r.Match(v => 1, e => 2));
        Assert.Throws<InvalidOperationException>(() => r.Match(0, (s, v) => 1, (s, e) => 2));

        Assert.Throws<InvalidOperationException>(() => r.Execute(v => { }, e => { }));
        Assert.Throws<InvalidOperationException>(() => r.Execute(0, (s, v) => { }, (s, e) => { }));

        Assert.Throws<InvalidOperationException>(() => r.MapFailure(e => 1, 0));
        Assert.Throws<InvalidOperationException>(() => r.MapFailure(0, (s, e) => 1, 0));
    }

    [Fact]
    public void ImplicitOperator_ErrorToResult()
    {
        Error e = Error.Failure("A", "B");
        Result r = e;
        Assert.True(r.IsFailure);
        Assert.Equal(e, r.Error);

        Result<int> rt = e;
        Assert.True(rt.IsFailure);
        Assert.Equal(e, rt.Error);
    }

    [Fact]
    public void SuccessResult_Error_Throws()
    {
        Result r = Result.Success();
        Assert.Throws<InvalidOperationException>(() => r.Error);

        Result<int> rt = Result.Success(42);
        Assert.Throws<InvalidOperationException>(() => rt.Error);
    }

    [Fact]
    public void Failure_WhenNullErrorPassed_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Failure(null!));
    }
}

