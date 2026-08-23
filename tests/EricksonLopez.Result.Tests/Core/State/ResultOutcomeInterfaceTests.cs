// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.OpenTelemetry;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultOutcomeInterfaceTests
{
    [Fact]
    public void IResultOutcome_Properties_NonReflectiveAccess()
    {

        IResultOutcome successResult = Result.Success("Hello");
        Assert.True(successResult.IsSuccess);
        Assert.False(successResult.IsFailure);
        Assert.Null(successResult.Error);
        Assert.Equal("Hello", successResult.RawValue);

        var err = Error.NotFound("404", "Not found");
        IResultOutcome failureResult = Result.Failure<string>(err);
        Assert.False(failureResult.IsSuccess);
        Assert.True(failureResult.IsFailure);
        Assert.Same(err, failureResult.Error);
        Assert.Null(failureResult.RawValue);
    }

    [Fact]
    public void Inspect_ExecutesActionAndReturnsSelf()
    {
        bool inspected = false;
        var result = Result.Success(42).Inspect(r => inspected = r.IsSuccess);

        Assert.True(inspected);
        Assert.Equal(42, result.ShouldBeSuccess());
    }

    [Fact]
    public void Recover_ExecutesOnFailure()
    {
        var failed = Result.Failure(Error.Validation("V1", "Invalid"));
        var recovered = failed.Recover(e => Result.Success());

        recovered.ShouldBeSuccess();
    }

    [Fact]
    public async Task TryAsync_HandlesTaskExceptions()
    {
        var success = await Result.TryAsync((Func<Task<int>>)(async () =>
        {

            await Task.Yield();
            return 10;
        }), ex => Error.Unexpected("EX", ex.Message));

        Assert.Equal(10, success.ShouldBeSuccess());

        var failure = await Result.TryAsync<int>((Func<Task<int>>)(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("Boom");
        }), ex => Error.Unexpected("EX", ex.Message));

        var err = failure.ShouldBeFailure();
        Assert.Equal("EX", err.Code);
        Assert.Equal("Boom", err.Description);
    }

    [Fact]
    public void Deconstruct_NonGenericResult()
    {
        var err = Error.Conflict("C1", "Conflict");
        var result = Result.Failure(err);

        var (isSuccess, error) = result;
        Assert.False(isSuccess);
        Assert.Same(err, error);
    }

    [Fact]
    public void OpenTelemetry_TraceOutcome_RecordsActivityAndMetrics()
    {
        var activity = new Activity("TestOperation").Start();
        var failure = Result.Failure(Error.Unavailable("S1", "Service Unavailable"));

        failure.TraceOutcome("TestOperation", activity);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("Service Unavailable", activity.StatusDescription);
    }

    [Fact]
    public void TestingPackage_NewAssertionsWork()
    {
        var err = Error.Validation("V1", "Value invalid")
            .WithMetadata("field", "email");

        var result = Result.Failure<string>(err);

        result.ShouldHaveErrorType(ErrorType.Validation);
        result.ShouldHaveSeverity(ErrorSeverity.Warning);
        result.ShouldHaveMetadata("field", "email");
        Assert.Throws<ResultAssertionException>(() => result.ShouldHaveErrorType(ErrorType.NotFound));
    }


    [Fact]
    public void IResultOutcome_Error_SuccessResult_ReturnsNull()
    {
        IResultOutcome outcome = Result.Success();
        Assert.Null(outcome.Error);
    }

    [Fact]
    public void IResultOutcome_WhenFailure_ExposesErrorAndNullRawValue()
    {
        var f = Result.Failure(Error.Failure("X", "X"));
        var ioF = (IResultOutcome)f;
        Assert.NotNull(ioF.Error);
        Assert.Null(ioF.RawValue);
    }

    [Fact]
    public void IResultOutcome_WhenSuccess_ExposesNullErrorAndNullRawValue()
    {
        var s = Result.Success();
        var ioS = (IResultOutcome)s;
        Assert.Null(ioS.Error);
        Assert.Null(ioS.RawValue);
    }

    [Fact]
    public void IResultOutcome_UninitializedStates_ExposeCorrectContractProperties()
    {
        IResultOutcome uninitNonGeneric = default(Result);
        Assert.True(uninitNonGeneric.IsUninitialized);
        Assert.False(uninitNonGeneric.IsSuccess);
        Assert.False(uninitNonGeneric.IsFailure);
        Assert.Null(uninitNonGeneric.Error);
        Assert.Null(uninitNonGeneric.RawValue);

        IResultOutcome uninitGeneric = default(Result<int>);
        Assert.True(uninitGeneric.IsUninitialized);
        Assert.False(uninitGeneric.IsSuccess);
        Assert.False(uninitGeneric.IsFailure);
        Assert.Null(uninitGeneric.Error);
        Assert.Null(uninitGeneric.RawValue);
    }

    [Fact]
    public void IResultOutcome_GenericSuccess_ExposesRawValueBoxed()
    {
        IResultOutcome successGeneric = Result.Success(12345);
        Assert.False(successGeneric.IsUninitialized);
        Assert.True(successGeneric.IsSuccess);
        Assert.False(successGeneric.IsFailure);
        Assert.Null(successGeneric.Error);
        Assert.Equal(12345, successGeneric.RawValue);
    }
}



