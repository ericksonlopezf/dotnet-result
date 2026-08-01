#nullable enable
using System;
using System.Runtime.CompilerServices;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultTests
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
    public void ResultT_Switch_ExecutesCorrectBranch()
    {
        var success = Result.Success(42);
        var failure = Result.Failure<int>(Error.Unexpected("Err", "Msg"));

        int successVal = 0;
        success.Execute(v => successVal = v, _ => Assert.Fail("Should not call failure"));
        Assert.Equal(42, successVal);

        string? errorVal = null;
        failure.Execute(_ => Assert.Fail("Should not call success"), e => errorVal = e.Code);
        Assert.Equal("Err", errorVal);
    }

    [Fact]
    public void NonGenericResult_Bind_ChainsPipeline()
    {
        var result = Result.Success()
            .Bind(() => Result.Success(100));

        result.ShouldBeSuccess();
        Assert.Equal(100, result.Value);
    }

    [Fact]
    public void Result_Combine_ReadOnlySpan_AggregatesErrors()
    {
        var r1 = Result.Success();
        var r2 = Result.Failure(Error.NotFound("E1", "Error 1"));
        var r3 = Result.Failure(Error.Validation("E2", "Error 2"));

        var combined = Result.Combine([r1, r2, r3]);

        var error = combined.ShouldBeFailure();
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, error.Code);
        Assert.True(error.HasInnerErrors);
        Assert.Equal(2, error.InnerErrors.Length);
    }

    [Fact]
    public void Result_Merge_CombinesGuardAndTypedResult()
    {
        var guardSuccess = Result.Success();
        var guardFailure = Result.Failure(Error.Unauthorized("A1", "No access"));

        var typed = Result.Success("Data");

        var m1 = Result.Merge(guardSuccess, typed);
        Assert.Equal("Data", m1.ShouldBeSuccess());

        var m2 = Result.Merge(guardFailure, typed);
        m2.ShouldHaveErrorCode("A1");
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
    public void SwitchTState_Uninitialized_Throws()
    {
        Result result = default;
        Assert.Throws<InvalidOperationException>(() => result.Execute(1, _ => { }, (_, _) => { }));
    }

    [Fact]
    public void BindTState_Uninitialized_Throws()
    {
        Result result = default;
        Assert.Throws<InvalidOperationException>(() => result.Bind(1, _ => Result.Success()));
    }

    [Fact]
    public void IResultOutcome_Error_SuccessResult_ReturnsNull()
    {
        IResultOutcome outcome = Result.Success();
        Assert.Null(outcome.Error);
    }

#pragma warning disable CA2201, CS0162
    [Fact]
    public void Try_FatalException_Throws()
    {
        Assert.Throws<OutOfMemoryException>(() => Result.Try(() => throw new OutOfMemoryException(), ex => Error.Failure("err", "desc")));
    }

    [Fact]
    public async System.Threading.Tasks.Task TryAsync_FatalException_Throws()
    {
        await Assert.ThrowsAsync<OutOfMemoryException>(async () => await Result.TryAsync((Func<System.Threading.Tasks.Task>)(async () => { throw new OutOfMemoryException(); await System.Threading.Tasks.Task.Yield(); }), ex => Error.Failure("err", "desc")));
    }

    [Fact]
    public async System.Threading.Tasks.Task TryAsyncCancellation_FatalException_Throws()
    {
        await Assert.ThrowsAsync<OutOfMemoryException>(async () => await Result.TryAsync((Func<System.Threading.CancellationToken, System.Threading.Tasks.Task>)(async (c) => { throw new OutOfMemoryException(); await System.Threading.Tasks.Task.Yield(); }), ex => Error.Failure("err", "desc"), default));
    }

    [Fact]
    public void TryT_FatalException_Throws()
    {
        Assert.Throws<OutOfMemoryException>(() => Result.Try<int>(() => throw new OutOfMemoryException(), ex => Error.Failure("err", "desc")));
    }

    [Fact]
    public async System.Threading.Tasks.Task TryAsyncT_FatalException_Throws()
    {
        await Assert.ThrowsAsync<OutOfMemoryException>(async () => await Result.TryAsync<int>((Func<System.Threading.Tasks.Task<int>>)(async () => { throw new OutOfMemoryException(); await System.Threading.Tasks.Task.Yield(); return 1; }), ex => Error.Failure("err", "desc")));
    }

    [Fact]
    public async System.Threading.Tasks.Task TryAsyncTCancellation_FatalException_Throws()
    {
        await Assert.ThrowsAsync<OutOfMemoryException>(async () => await Result.TryAsync<int>((Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<int>>)(async (c) => { throw new OutOfMemoryException(); await System.Threading.Tasks.Task.Yield(); return 1; }), ex => Error.Failure("err", "desc"), default));
    }
#pragma warning restore CA2201, CS0162

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
