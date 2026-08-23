// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsValueTaskNonGenericSlowPathTests : ResultExtensionsTestsBase
{

    [Fact]
    public async Task Bind_WhenExecutedOnSlowPath_ReturnsBoundResult()
    {
        Func<int, Result> b3 = s => Result.Success();
        var bindSuccess = await Result.Success().AsAsyncValueTaskResult().Bind(1, b3);
        bindSuccess.ShouldBeSuccess();

        var bindFailure = await Result.Failure(TestError).AsAsyncValueTaskResult().Bind(1, b3);
        bindFailure.ShouldBeFailure();
    }

    [Fact]
    public async Task TapOnSuccess_WhenExecutedOnSlowPath_ExecutesSideEffectOnlyOnSuccess()
    {
        bool tapSuccessCalled = false;
        Action<int> t3 = s => { tapSuccessCalled = true; };
        var tapSuccess = await Result.Success().AsAsyncValueTaskResult().TapOnSuccess(1, t3);
        tapSuccess.ShouldBeSuccess();
        Assert.True(tapSuccessCalled);

        var tapFailure = await Result.Failure(TestError).AsAsyncValueTaskResult().TapOnSuccess(1, t3);
        tapFailure.ShouldBeFailure();
    }

    [Fact]
    public async Task Recover_WhenExecutedOnSlowPath_RecoversFromFailure()
    {
        Func<int, Error, Result> r3 = (s, e) => Result.Success();
        var recoverSuccess = await Result.Success().AsAsyncValueTaskResult().Recover(1, r3);
        recoverSuccess.ShouldBeSuccess();

        var recoverFailure = await Result.Failure(TestError).AsAsyncValueTaskResult().Recover(1, r3);
        recoverFailure.ShouldBeSuccess();
    }

    [Fact]
    public async Task Match_WhenExecutedOnSlowPath_EvaluatesAppropriateBranch()
    {
        Func<int, int> ms1 = s => s * 2;
        Func<int, Error, int> me1 = (s, e) => -1;
        var matchSuccess = await Result.Success().AsAsyncValueTaskResult().Match(5, ms1, me1);
        Assert.Equal(10, matchSuccess);

        var matchFailure = await Result.Failure(TestError).AsAsyncValueTaskResult().Match(5, ms1, me1);
        Assert.Equal(-1, matchFailure);
    }

    [Fact]
    public async Task MapError_WhenExecutedOnSlowPath_TransformsError()
    {
        Func<int, Error, Error> mapErr1 = (s, e) => Error.Validation("V", "Mapped");
        var mapErrSuccess = await Result.Success().AsAsyncValueTaskResult().MapError(1, mapErr1);
        mapErrSuccess.ShouldBeSuccess();

        var mapErrFailure = await Result.Failure(TestError).AsAsyncValueTaskResult().MapError(1, mapErr1);
        mapErrFailure.ShouldBeFailure();
        Assert.Equal("V", mapErrFailure.Error.Code);
    }

    [Fact]
    public async Task TapOnFailure_WhenExecutedOnSlowPath_ExecutesSideEffectOnlyOnFailure()
    {
        bool tapFailureCalled = false;
        Action<int, Error> tapErr1 = (s, e) => { tapFailureCalled = true; };
        var tapErrSuccess = await Result.Success().AsAsyncValueTaskResult().TapOnFailure(1, tapErr1);
        tapErrSuccess.ShouldBeSuccess();
        Assert.False(tapFailureCalled);

        var tapErrFailure = await Result.Failure(TestError).AsAsyncValueTaskResult().TapOnFailure(1, tapErr1);
        tapErrFailure.ShouldBeFailure();
        Assert.True(tapFailureCalled);
    }

    [Fact]
    public async Task Ensure_WhenExecutedOnSlowPath_ValidatesCondition()
    {
        Func<int, bool> ens1 = s => true;
        var ensureSuccess = await Result.Success().AsAsyncValueTaskResult().Ensure(1, ens1, TestError);
        ensureSuccess.ShouldBeSuccess();

        var ensureFailure = await Result.Failure(TestError).AsAsyncValueTaskResult().Ensure(1, ens1, TestError);
        ensureFailure.ShouldBeFailure();
    }

    [Fact]
    public async Task Inspect_WhenExecutedOnSlowPath_InspectsResultState()
    {
        bool inspectCalled = false;
        Action<int, Result> ins1 = (s, r) => { inspectCalled = true; };
        var inspectSuccess = await Result.Success().AsAsyncValueTaskResult().Inspect(1, ins1);
        inspectSuccess.ShouldBeSuccess();
        Assert.True(inspectCalled);

        inspectCalled = false;
        var inspectFailure = await Result.Failure(TestError).AsAsyncValueTaskResult().Inspect(1, ins1);
        inspectFailure.ShouldBeFailure();
        Assert.True(inspectCalled);
    }
}
