// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsValueTaskSlowPathTests : ResultExtensionsTestsBase
{
    // --- ValueTask<Result<T>> Map Tests ---
    [Fact]
    public async Task Map_ValueTaskOfResultT_WhenSlowPath_ReturnsMappedValue()
    {
        var s1 = await IncompleteResultValueTask(Result.Success(10)).Map(x => x * 2);
        s1.IsSuccess.Should().BeTrue();
        s1.Value.Should().Be(20);

        var f1 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Map(x => x * 2);
        f1.IsFailure.Should().BeTrue();
        f1.Error.Should().Be(TestError);

        var s2 = await IncompleteResultValueTask(Result.Success(10)).Map(5, (s, x) => s + x);
        s2.IsSuccess.Should().BeTrue();
        s2.Value.Should().Be(15);

        var f2 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Map(5, (s, x) => s + x);
        f2.IsFailure.Should().BeTrue();
        f2.Error.Should().Be(TestError);

        var s3 = await IncompleteResultValueTask(Result.Success(10)).Map(async x => { await Task.Yield(); return x * 2; });
        s3.IsSuccess.Should().BeTrue();
        s3.Value.Should().Be(20);

        var f3 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Map(async x => { await Task.Yield(); return x * 2; });
        f3.IsFailure.Should().BeTrue();
        f3.Error.Should().Be(TestError);

        var s4 = await IncompleteResultValueTask(Result.Success(10)).Map(5, async (s, x) => { await Task.Yield(); return s + x; });
        s4.IsSuccess.Should().BeTrue();
        s4.Value.Should().Be(15);

        var f4 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Map(5, async (s, x) => { await Task.Yield(); return s + x; });
        f4.IsFailure.Should().BeTrue();
        f4.Error.Should().Be(TestError);
    }

    // --- ValueTask<Result<T>> Bind Tests ---
    [Fact]
    public async Task Bind_ValueTaskOfResultT_WhenSlowPath_ChainsResults()
    {
        var s1 = await IncompleteResultValueTask(Result.Success(10)).Bind(x => Result.Success(x * 2));
        s1.IsSuccess.Should().BeTrue();
        s1.Value.Should().Be(20);

        var f1 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Bind(x => Result.Success(x * 2));
        f1.IsFailure.Should().BeTrue();
        f1.Error.Should().Be(TestError);

        var s2 = await IncompleteResultValueTask(Result.Success(10)).Bind(5, (s, x) => Result.Success(s + x));
        s2.IsSuccess.Should().BeTrue();
        s2.Value.Should().Be(15);

        var f2 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Bind(5, (s, x) => Result.Success(s + x));
        f2.IsFailure.Should().BeTrue();
        f2.Error.Should().Be(TestError);

        var s3 = await IncompleteResultValueTask(Result.Success(10)).Bind(async x => { await Task.Yield(); return Result.Success(x * 2); });
        s3.IsSuccess.Should().BeTrue();
        s3.Value.Should().Be(20);

        var f3 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Bind(async x => { await Task.Yield(); return Result.Success(x * 2); });
        f3.IsFailure.Should().BeTrue();
        f3.Error.Should().Be(TestError);

        // Bind to non-generic Result
        var s5 = await IncompleteResultValueTask(Result.Success(10)).Bind(x => Result.Success());
        s5.IsSuccess.Should().BeTrue();

        var f5 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Bind(x => Result.Success());
        f5.IsFailure.Should().BeTrue();
        f5.Error.Should().Be(TestError);

        var s6 = await IncompleteResultValueTask(Result.Success(10)).Bind(5, (s, x) => Result.Success());
        s6.IsSuccess.Should().BeTrue();

        var f6 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Bind(5, (s, x) => Result.Success());
        f6.IsFailure.Should().BeTrue();
        f6.Error.Should().Be(TestError);

        Func<int, ValueTask<Result>> asyncBind = async x => { await Task.Yield(); return Result.Success(); };
        var s7 = await IncompleteResultValueTask(Result.Success(10)).Bind(asyncBind);
        s7.IsSuccess.Should().BeTrue();

        var f7 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Bind(asyncBind);
        f7.IsFailure.Should().BeTrue();
        f7.Error.Should().Be(TestError);
    }

    // --- ValueTask<Result<T>> Match and Execute Tests ---
    [Fact]
    public async Task MatchAndExecute_ValueTaskOfResultT_WhenSlowPath_EvaluatesAppropriateBranch()
    {
        var m1 = await IncompleteResultValueTask(Result.Success(10)).Match(x => x * 2, e => -1);
        m1.Should().Be(20);

        var m1f = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Match(x => x * 2, e => -1);
        m1f.Should().Be(-1);

        var m2 = await IncompleteResultValueTask(Result.Success(10)).Match(5, (s, x) => s + x, (s, e) => -1);
        m2.Should().Be(15);

        var m2f = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Match(5, (s, x) => s + x, (s, e) => -1);
        m2f.Should().Be(-1);

        int execVal = 0;
        await IncompleteResultValueTask(Result.Success(10)).Execute(x => execVal = x, e => execVal = -1);
        execVal.Should().Be(10);

        await IncompleteResultValueTask(Result.Failure<int>(TestError)).Execute(x => execVal = 10, e => execVal = -1);
        execVal.Should().Be(-1);

        execVal = 0;
        await IncompleteResultValueTask(Result.Success(10)).Execute(5, (s, x) => execVal = s + x, (s, e) => execVal = -1);
        execVal.Should().Be(15);

        await IncompleteResultValueTask(Result.Failure<int>(TestError)).Execute(5, (s, x) => execVal = 10, (s, e) => execVal = -1);
        execVal.Should().Be(-1);
    }

    // --- ValueTask<Result<T>> TapOnSuccess and TapOnFailure Tests ---
    [Fact]
    public async Task Tap_ValueTaskOfResultT_WhenSlowPath_InvokesMatchingOutcome()
    {
        int tapped = 0;
        var s1 = await IncompleteResultValueTask(Result.Success(10)).TapOnSuccess(x => tapped = x);
        s1.IsSuccess.Should().BeTrue();
        tapped.Should().Be(10);

        tapped = 0;
        var f1 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).TapOnSuccess(x => tapped = x);
        f1.IsFailure.Should().BeTrue();
        tapped.Should().Be(0);

        tapped = 0;
        var s2 = await IncompleteResultValueTask(Result.Success(10)).TapOnSuccess(5, (s, x) => tapped = s + x);
        s2.IsSuccess.Should().BeTrue();
        tapped.Should().Be(15);

        tapped = 0;
        var f2 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).TapOnSuccess(5, (s, x) => tapped = s + x);
        f2.IsFailure.Should().BeTrue();
        tapped.Should().Be(0);

        tapped = 0;
        var s3 = await IncompleteResultValueTask(Result.Success(10)).TapOnSuccess(async x => { await Task.Yield(); tapped = x; });
        s3.IsSuccess.Should().BeTrue();
        tapped.Should().Be(10);

        tapped = 0;
        var f3 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).TapOnSuccess(async x => { await Task.Yield(); tapped = x; });
        f3.IsFailure.Should().BeTrue();
        tapped.Should().Be(0);

        bool failTapped = false;
        var s4 = await IncompleteResultValueTask(Result.Success(10)).TapOnFailure(e => failTapped = true);
        s4.IsSuccess.Should().BeTrue();
        failTapped.Should().BeFalse();

        var f4 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).TapOnFailure(e => failTapped = true);
        f4.IsFailure.Should().BeTrue();
        failTapped.Should().BeTrue();

        failTapped = false;
        var s5 = await IncompleteResultValueTask(Result.Success(10)).TapOnFailure(5, (s, e) => failTapped = true);
        s5.IsSuccess.Should().BeTrue();
        failTapped.Should().BeFalse();

        var f5 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).TapOnFailure(5, (s, e) => failTapped = true);
        f5.IsFailure.Should().BeTrue();
        failTapped.Should().BeTrue();

        failTapped = false;
        var s6 = await IncompleteResultValueTask(Result.Success(10)).TapOnFailure(async e => { await Task.Yield(); failTapped = true; });
        s6.IsSuccess.Should().BeTrue();
        failTapped.Should().BeFalse();

        var f6 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).TapOnFailure(async e => { await Task.Yield(); failTapped = true; });
        f6.IsFailure.Should().BeTrue();
        failTapped.Should().BeTrue();
    }

    // --- ValueTask<Result<T>> Ensure Tests ---
    [Fact]
    public async Task Ensure_ValueTaskOfResultT_WhenSlowPath_ValidatesPredicate()
    {
        var s1 = await IncompleteResultValueTask(Result.Success(10)).Ensure(x => x > 5, TestError);
        s1.IsSuccess.Should().BeTrue();

        var s1FailPred = await IncompleteResultValueTask(Result.Success(10)).Ensure(x => x < 5, TestError);
        s1FailPred.IsFailure.Should().BeTrue();
        s1FailPred.Error.Should().Be(TestError);

        var s1AlreadyFailed = await IncompleteResultValueTask(Result.Failure<int>(TestError2)).Ensure(x => x > 5, TestError);
        s1AlreadyFailed.IsFailure.Should().BeTrue();
        s1AlreadyFailed.Error.Should().Be(TestError2);

        var s2 = await IncompleteResultValueTask(Result.Success(10)).Ensure(5, (s, x) => x > s, TestError);
        s2.IsSuccess.Should().BeTrue();

        var s2FailPred = await IncompleteResultValueTask(Result.Success(10)).Ensure(5, (s, x) => x < s, TestError);
        s2FailPred.IsFailure.Should().BeTrue();
        s2FailPred.Error.Should().Be(TestError);

        var s2AlreadyFailed = await IncompleteResultValueTask(Result.Failure<int>(TestError2)).Ensure(5, (s, x) => x > s, TestError);
        s2AlreadyFailed.IsFailure.Should().BeTrue();
        s2AlreadyFailed.Error.Should().Be(TestError2);

        var s3 = await IncompleteResultValueTask(Result.Success(10)).Ensure(async x => { await Task.Yield(); return x > 5; }, TestError);
        s3.IsSuccess.Should().BeTrue();

        var s3FailPred = await IncompleteResultValueTask(Result.Success(10)).Ensure(async x => { await Task.Yield(); return x < 5; }, TestError);
        s3FailPred.IsFailure.Should().BeTrue();
        s3FailPred.Error.Should().Be(TestError);

        var s3AlreadyFailed = await IncompleteResultValueTask(Result.Failure<int>(TestError2)).Ensure(async x => { await Task.Yield(); return x > 5; }, TestError);
        s3AlreadyFailed.IsFailure.Should().BeTrue();
        s3AlreadyFailed.Error.Should().Be(TestError2);

        var s4 = await IncompleteResultValueTask(Result.Success(10)).Ensure(5, async (s, x) => { await Task.Yield(); return x > s; }, TestError);
        s4.IsSuccess.Should().BeTrue();

        var s4FailPred = await IncompleteResultValueTask(Result.Success(10)).Ensure(5, async (s, x) => { await Task.Yield(); return x < s; }, TestError);
        s4FailPred.IsFailure.Should().BeTrue();
        s4FailPred.Error.Should().Be(TestError);

        var s4AlreadyFailed = await IncompleteResultValueTask(Result.Failure<int>(TestError2)).Ensure(5, async (s, x) => { await Task.Yield(); return x > s; }, TestError);
        s4AlreadyFailed.IsFailure.Should().BeTrue();
        s4AlreadyFailed.Error.Should().Be(TestError2);
    }

    // --- ValueTask<Result<T>> Recover Tests ---
    [Fact]
    public async Task Recover_ValueTaskOfResultT_WhenSlowPath_RecoversFromFailure()
    {
        var s1 = await IncompleteResultValueTask(Result.Success(10)).Recover(e => Result.Success(99));
        s1.IsSuccess.Should().BeTrue();
        s1.Value.Should().Be(10);

        var f1 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Recover(e => Result.Success(99));
        f1.IsSuccess.Should().BeTrue();
        f1.Value.Should().Be(99);

        var s2 = await IncompleteResultValueTask(Result.Success(10)).Recover(5, (s, e) => Result.Success(s));
        s2.IsSuccess.Should().BeTrue();
        s2.Value.Should().Be(10);

        var f2 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Recover(5, (s, e) => Result.Success(s));
        f2.IsSuccess.Should().BeTrue();
        f2.Value.Should().Be(5);

        var s3 = await IncompleteResultValueTask(Result.Success(10)).Recover(async e => { await Task.Yield(); return Result.Success(99); });
        s3.IsSuccess.Should().BeTrue();
        s3.Value.Should().Be(10);

        var f3 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Recover(async e => { await Task.Yield(); return Result.Success(99); });
        f3.IsSuccess.Should().BeTrue();
        f3.Value.Should().Be(99);

        var s4 = await IncompleteResultValueTask(Result.Success(10)).Recover(5, async (s, e) => { await Task.Yield(); return Result.Success(s); });
        s4.IsSuccess.Should().BeTrue();
        s4.Value.Should().Be(10);

        var f4 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Recover(5, async (s, e) => { await Task.Yield(); return Result.Success(s); });
        f4.IsSuccess.Should().BeTrue();
        f4.Value.Should().Be(5);
    }

    // --- ValueTask<Result<T>> MapError, Inspect Tests ---
    [Fact]
    public async Task MapErrorAndInspect_ValueTaskOfResultT_WhenSlowPath_TransformsOrInspects()
    {
        var s1 = await IncompleteResultValueTask(Result.Success(10)).MapError(e => TestError2);
        s1.IsSuccess.Should().BeTrue();
        s1.Value.Should().Be(10);

        var f1 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).MapError(e => TestError2);
        f1.IsFailure.Should().BeTrue();
        f1.Error.Should().Be(TestError2);

        var s2 = await IncompleteResultValueTask(Result.Success(10)).MapError("prefix", (s, e) => Error.Failure("P", $"{s}_{e.Description}"));
        s2.IsSuccess.Should().BeTrue();

        var f2 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).MapError("prefix", (s, e) => Error.Failure("P", $"{s}_{e.Description}"));
        f2.IsFailure.Should().BeTrue();
        f2.Error.Description.Should().Be($"prefix_{TestError.Description}");

        bool inspected = false;
        var s3 = await IncompleteResultValueTask(Result.Success(10)).Inspect(r => inspected = true);
        s3.IsSuccess.Should().BeTrue();
        inspected.Should().BeTrue();

        inspected = false;
        var f3 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Inspect(r => inspected = true);
        f3.IsFailure.Should().BeTrue();
        inspected.Should().BeTrue();

        int inspectedState = 0;
        var s4 = await IncompleteResultValueTask(Result.Success(10)).Inspect(7, (s, r) => inspectedState = s);
        s4.IsSuccess.Should().BeTrue();
        inspectedState.Should().Be(7);

        inspectedState = 0;
        var f4 = await IncompleteResultValueTask(Result.Failure<int>(TestError)).Inspect(7, (s, r) => inspectedState = s);
        f4.IsFailure.Should().BeTrue();
        inspectedState.Should().Be(7);
    }

    // --- ValueTask<Result> Non-generic Bind, Match, Execute, Tap, Ensure, Recover, MapError, Inspect Tests ---
    [Fact]
    public async Task MonadicCombinators_ValueTaskOfResult_WhenSlowPath_ExecutesCorrectly()
    {
        var s1 = await IncompleteResultValueTask(Result.Success()).Bind(() => Result.Success());
        s1.IsSuccess.Should().BeTrue();

        var f1 = await IncompleteResultValueTask(Result.Failure(TestError)).Bind(() => Result.Success());
        f1.IsFailure.Should().BeTrue();
        f1.Error.Should().Be(TestError);

        var s2 = await IncompleteResultValueTask(Result.Success()).Bind(5, s => Result.Success());
        s2.IsSuccess.Should().BeTrue();

        var f2 = await IncompleteResultValueTask(Result.Failure(TestError)).Bind(5, s => Result.Success());
        f2.IsFailure.Should().BeTrue();
        f2.Error.Should().Be(TestError);

        var s3 = await IncompleteResultValueTask(Result.Success()).Bind(async () => { await Task.Yield(); return Result.Success(); });
        s3.IsSuccess.Should().BeTrue();

        var f3 = await IncompleteResultValueTask(Result.Failure(TestError)).Bind(async () => { await Task.Yield(); return Result.Success(); });
        f3.IsFailure.Should().BeTrue();
        f3.Error.Should().Be(TestError);

        var s4 = await IncompleteResultValueTask(Result.Success()).Bind(() => Result.Success(42));
        s4.IsSuccess.Should().BeTrue();
        s4.Value.Should().Be(42);

        var f4 = await IncompleteResultValueTask(Result.Failure(TestError)).Bind(() => Result.Success(42));
        f4.IsFailure.Should().BeTrue();
        f4.Error.Should().Be(TestError);

        var s5 = await IncompleteResultValueTask(Result.Success()).Bind(5, s => Result.Success(s * 2));
        s5.IsSuccess.Should().BeTrue();
        s5.Value.Should().Be(10);

        var f5 = await IncompleteResultValueTask(Result.Failure(TestError)).Bind(5, s => Result.Success(s * 2));
        f5.IsFailure.Should().BeTrue();
        f5.Error.Should().Be(TestError);

        var s6 = await IncompleteResultValueTask(Result.Success()).Bind(async () => { await Task.Yield(); return Result.Success(42); });
        s6.IsSuccess.Should().BeTrue();
        s6.Value.Should().Be(42);

        var f6 = await IncompleteResultValueTask(Result.Failure(TestError)).Bind(async () => { await Task.Yield(); return Result.Success(42); });
        f6.IsFailure.Should().BeTrue();
        f6.Error.Should().Be(TestError);

        var m1 = await IncompleteResultValueTask(Result.Success()).Match(() => 10, e => -1);
        m1.Should().Be(10);

        var m1f = await IncompleteResultValueTask(Result.Failure(TestError)).Match(() => 10, e => -1);
        m1f.Should().Be(-1);

        var m2 = await IncompleteResultValueTask(Result.Success()).Match(5, s => s * 2, (s, e) => -1);
        m2.Should().Be(10);

        var m2f = await IncompleteResultValueTask(Result.Failure(TestError)).Match(5, s => s * 2, (s, e) => -1);
        m2f.Should().Be(-1);

        int execVal = 0;
        await IncompleteResultValueTask(Result.Success()).Execute(() => execVal = 10, e => execVal = -1);
        execVal.Should().Be(10);

        await IncompleteResultValueTask(Result.Failure(TestError)).Execute(() => execVal = 10, e => execVal = -1);
        execVal.Should().Be(-1);

        execVal = 0;
        await IncompleteResultValueTask(Result.Success()).Execute(5, s => execVal = s * 2, (s, e) => execVal = -1);
        execVal.Should().Be(10);

        await IncompleteResultValueTask(Result.Failure(TestError)).Execute(5, s => execVal = 10, (s, e) => execVal = -1);
        execVal.Should().Be(-1);

        bool tapped = false;
        var t1 = await IncompleteResultValueTask(Result.Success()).TapOnSuccess(() => tapped = true);
        t1.IsSuccess.Should().BeTrue();
        tapped.Should().Be(true);

        tapped = false;
        var t1f = await IncompleteResultValueTask(Result.Failure(TestError)).TapOnSuccess(() => tapped = true);
        t1f.IsFailure.Should().BeTrue();
        tapped.Should().Be(false);

        tapped = false;
        var t2 = await IncompleteResultValueTask(Result.Success()).TapOnSuccess(5, s => tapped = true);
        t2.IsSuccess.Should().BeTrue();
        tapped.Should().Be(true);

        tapped = false;
        var t2f = await IncompleteResultValueTask(Result.Failure(TestError)).TapOnSuccess(5, s => tapped = true);
        t2f.IsFailure.Should().BeTrue();
        tapped.Should().Be(false);

        tapped = false;
        var t3 = await IncompleteResultValueTask(Result.Success()).TapOnSuccess(async () => { await Task.Yield(); tapped = true; });
        t3.IsSuccess.Should().BeTrue();
        tapped.Should().Be(true);

        tapped = false;
        var t3f = await IncompleteResultValueTask(Result.Failure(TestError)).TapOnSuccess(async () => { await Task.Yield(); tapped = true; });
        t3f.IsFailure.Should().BeTrue();
        tapped.Should().Be(false);
    }

    [Fact]
    public async Task TapOnFailure_ValueTaskOfResult_WhenSlowPath_InvokesOnFailure()
    {
        bool called = false;
        var failure = await IncompleteResultValueTask(Result.Failure(TestError)).TapOnFailure(e => { called = true; e.Should().Be(TestError); });
        failure.IsFailure.Should().BeTrue();
        called.Should().BeTrue();

        called = false;
        var success = await IncompleteResultValueTask(Result.Success()).TapOnFailure(e => { called = true; });
        success.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();

        int stateCalled = 0;
        var failureState = await IncompleteResultValueTask(Result.Failure(TestError)).TapOnFailure(42, (s, e) => { stateCalled = s; e.Should().Be(TestError); });
        failureState.IsFailure.Should().BeTrue();
        stateCalled.Should().Be(42);

        stateCalled = 0;
        var successState = await IncompleteResultValueTask(Result.Success()).TapOnFailure(42, (s, e) => { stateCalled = s; });
        successState.IsSuccess.Should().BeTrue();
        stateCalled.Should().Be(0);

        int sideEffect = 0;
        var failureAsync = await IncompleteResultValueTask(Result.Failure(TestError)).TapOnFailure(async e =>
        {
            await Task.Yield();
            sideEffect = 42;
            e.Should().Be(TestError);
        });
        failureAsync.IsFailure.Should().BeTrue();
        sideEffect.Should().Be(42);

        sideEffect = 0;
        var successAsync = await IncompleteResultValueTask(Result.Success()).TapOnFailure(async e =>
        {
            await Task.Yield();
            sideEffect = 42;
        });
        successAsync.IsSuccess.Should().BeTrue();
        sideEffect.Should().Be(0);
    }

    [Fact]
    public async Task Ensure_ValueTaskOfResult_WhenSlowPath_ValidatesCondition()
    {
        var success = await IncompleteResultValueTask(Result.Success()).Ensure(() => true, TestError);
        success.IsSuccess.Should().BeTrue();

        var failedPredicate = await IncompleteResultValueTask(Result.Success()).Ensure(() => false, TestError);
        failedPredicate.IsFailure.Should().BeTrue();
        failedPredicate.Error.Should().Be(TestError);

        var alreadyFailed = await IncompleteResultValueTask(Result.Failure(TestError2)).Ensure(() => true, TestError);
        alreadyFailed.IsFailure.Should().BeTrue();
        alreadyFailed.Error.Should().Be(TestError2);

        var successState = await IncompleteResultValueTask(Result.Success()).Ensure(10, s => s > 5, TestError);
        successState.IsSuccess.Should().BeTrue();

        var failedPredicateState = await IncompleteResultValueTask(Result.Success()).Ensure(10, s => s < 5, TestError);
        failedPredicateState.IsFailure.Should().BeTrue();
        failedPredicateState.Error.Should().Be(TestError);

        var alreadyFailedState = await IncompleteResultValueTask(Result.Failure(TestError2)).Ensure(10, s => s > 5, TestError);
        alreadyFailedState.IsFailure.Should().BeTrue();
        alreadyFailedState.Error.Should().Be(TestError2);
    }

    [Fact]
    public async Task MapErrorAndInspect_ValueTaskOfResult_WhenSlowPath_TransformsOrInspects()
    {
        var failure = await IncompleteResultValueTask(Result.Failure(TestError)).MapError(e => TestError2);
        failure.IsFailure.Should().BeTrue();
        failure.Error.Should().Be(TestError2);

        var success = await IncompleteResultValueTask(Result.Success()).MapError(e => TestError2);
        success.IsSuccess.Should().BeTrue();

        var failureState = await IncompleteResultValueTask(Result.Failure(TestError)).MapError("prefix", (s, e) => Error.Failure("Prefixed", $"{s}_{e.Description}"));
        failureState.IsFailure.Should().BeTrue();
        failureState.Error.Code.Should().Be("Prefixed");
        failureState.Error.Description.Should().Be($"prefix_{TestError.Description}");

        var successState = await IncompleteResultValueTask(Result.Success()).MapError("prefix", (s, e) => Error.Failure("Prefixed", $"{s}_{e.Description}"));
        successState.IsSuccess.Should().BeTrue();

        bool inspected = false;
        var successIns = await IncompleteResultValueTask(Result.Success()).Inspect(r => { inspected = true; r.IsSuccess.Should().BeTrue(); });
        successIns.IsSuccess.Should().BeTrue();
        inspected.Should().BeTrue();

        inspected = false;
        var failureIns = await IncompleteResultValueTask(Result.Failure(TestError)).Inspect(r => { inspected = true; r.IsFailure.Should().BeTrue(); });
        failureIns.IsFailure.Should().BeTrue();
        inspected.Should().BeTrue();

        int inspectedState = 0;
        var successInsState = await IncompleteResultValueTask(Result.Success()).Inspect(99, (s, r) => { inspectedState = s; r.IsSuccess.Should().BeTrue(); });
        successInsState.IsSuccess.Should().BeTrue();
        inspectedState.Should().Be(99);

        inspectedState = 0;
        var failureInsState = await IncompleteResultValueTask(Result.Failure(TestError)).Inspect(99, (s, r) => { inspectedState = s; r.IsFailure.Should().BeTrue(); });
        failureInsState.IsFailure.Should().BeTrue();
        inspectedState.Should().Be(99);
    }

    [Fact]
    public async Task Map_ValueTaskOfResult_WhenMappingToResultOfTNextSlowPath_ReturnsSuccess()
    {
        var success = await IncompleteResultValueTask(Result.Success()).Map(() => 42);
        success.IsSuccess.Should().BeTrue();
        success.Value.Should().Be(42);

        var failure = await IncompleteResultValueTask(Result.Failure(TestError)).Map(() => 42);
        failure.IsFailure.Should().BeTrue();
        failure.Error.Should().Be(TestError);

        var successState = await IncompleteResultValueTask(Result.Success()).Map(10, s => s * 2);
        successState.IsSuccess.Should().BeTrue();
        successState.Value.Should().Be(20);

        var failureState = await IncompleteResultValueTask(Result.Failure(TestError)).Map(10, s => s * 2);
        failureState.IsFailure.Should().BeTrue();
        failureState.Error.Should().Be(TestError);
    }

    [Fact]
    public async Task Recover_ValueTaskOfResult_WhenSlowPath_RecoversToSuccess()
    {
        var recovered = await IncompleteResultValueTask(Result.Failure(TestError)).Recover(e => Result.Success());
        recovered.IsSuccess.Should().BeTrue();

        var remainedFailed = await IncompleteResultValueTask(Result.Failure(TestError)).Recover(e => Result.Failure(TestError2));
        remainedFailed.IsFailure.Should().BeTrue();
        remainedFailed.Error.Should().Be(TestError2);

        var success = await IncompleteResultValueTask(Result.Success()).Recover(e => Result.Failure(TestError2));
        success.IsSuccess.Should().BeTrue();

        var recoveredState = await IncompleteResultValueTask(Result.Failure(TestError)).Recover("fallback", (s, e) => Result.Success());
        recoveredState.IsSuccess.Should().BeTrue();

        var successState = await IncompleteResultValueTask(Result.Success()).Recover("fallback", (s, e) => Result.Failure(TestError2));
        successState.IsSuccess.Should().BeTrue();

        var recoveredAsync = await IncompleteResultValueTask(Result.Failure(TestError)).Recover(async e =>
        {
            await Task.Yield();
            return Result.Success();
        });
        recoveredAsync.IsSuccess.Should().BeTrue();

        var remainedFailedAsync = await IncompleteResultValueTask(Result.Failure(TestError)).Recover(async e =>
        {
            await Task.Yield();
            return Result.Failure(TestError2);
        });
        remainedFailedAsync.IsFailure.Should().BeTrue();
        remainedFailedAsync.Error.Should().Be(TestError2);

        var successAsync = await IncompleteResultValueTask(Result.Success()).Recover(async e =>
        {
            await Task.Yield();
            return Result.Failure(TestError2);
        });
        successAsync.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Cancellation_WhenCancellationTokenCancelledOnValueTaskSlowPath_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // ValueTask<Result<T>> Overloads
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Map(x => x, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Map(1, (s, x) => x, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Map(async x => { await Task.Yield(); return x; }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Map(1, async (s, x) => { await Task.Yield(); return x; }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Bind(x => Result.Success(x), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Bind(1, (s, x) => Result.Success(x), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Bind(async x => { await Task.Yield(); return Result.Success(x); }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Bind(x => Result.Success(), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Bind(1, (s, x) => Result.Success(), cts.Token));

        Func<int, ValueTask<Result>> asyncBind = async x => { await Task.Yield(); return Result.Success(); };
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Bind(asyncBind, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Execute(x => { }, e => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Execute(1, (s, x) => { }, (s, e) => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).TapOnSuccess(x => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).TapOnSuccess(1, (s, x) => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).TapOnSuccess(async x => await Task.Yield(), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).TapOnFailure(e => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).TapOnFailure(1, (s, e) => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).TapOnFailure(async e => await Task.Yield(), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Ensure(x => true, TestError, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Ensure(1, (s, x) => true, TestError, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Ensure(async x => { await Task.Yield(); return true; }, TestError, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Ensure(1, async (s, x) => { await Task.Yield(); return true; }, TestError, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Failure<int>(TestError)).Recover(e => Result.Success(10), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Failure<int>(TestError)).Recover(1, (s, e) => Result.Success(10), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Failure<int>(TestError)).Recover(async e => { await Task.Yield(); return Result.Success(10); }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Failure<int>(TestError)).Recover(1, async (s, e) => { await Task.Yield(); return Result.Success(10); }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).MapError(e => e, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).MapError(1, (s, e) => e, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Inspect(r => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success(10)).Inspect(1, (s, r) => { }, cts.Token));

        // ValueTask<Result> Overloads
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).Bind(() => Result.Success(), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).Bind(1, s => Result.Success(), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).Bind(async () => { await Task.Yield(); return Result.Success(); }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).Bind(() => Result.Success(10), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).Bind(1, s => Result.Success(10), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).Bind(async () => { await Task.Yield(); return Result.Success(10); }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).Execute(() => { }, e => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).Execute(1, s => { }, (s, e) => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).TapOnSuccess(() => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).TapOnSuccess(1, s => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).TapOnSuccess(async () => await Task.Yield(), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).TapOnFailure(e => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).TapOnFailure(1, (s, e) => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).TapOnFailure(async e => await Task.Yield(), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).Ensure(() => true, TestError, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).Ensure(1, s => true, TestError, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).MapError(e => e, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).MapError(1, (s, e) => e, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).Inspect(r => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).Inspect(1, (s, r) => { }, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).Map(() => 42, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Success()).Map(1, s => 42, cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Failure(TestError)).Recover(e => Result.Success(), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Failure(TestError)).Recover(1, (s, e) => Result.Success(), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await IncompleteResultValueTask(Result.Failure(TestError)).Recover(async e => { await Task.Yield(); return Result.Success(); }, cts.Token));
    }

    [Fact]
    public async Task AsyncCore_WhenCompletedValueTaskWithAsyncSideEffects_AwaitsProperly()
    {
        int sideEffect = 0;
        await ValueTask.FromResult(Result.Success(10)).TapOnSuccess(async x =>
        {
            await Task.Yield();
            sideEffect = x;
        });
        sideEffect.Should().Be(10);

        sideEffect = 0;
        await ValueTask.FromResult(Result.Failure<int>(TestError)).TapOnFailure(async e =>
        {
            await Task.Yield();
            sideEffect = 99;
        });
        sideEffect.Should().Be(99);

        sideEffect = 0;
        await ValueTask.FromResult(Result.Success()).TapOnSuccess(async () =>
        {
            await Task.Yield();
            sideEffect = 42;
        });
        sideEffect.Should().Be(42);

        sideEffect = 0;
        await ValueTask.FromResult(Result.Failure(TestError)).TapOnFailure(async e =>
        {
            await Task.Yield();
            sideEffect = 42;
        });
        sideEffect.Should().Be(42);

        var ensure1 = await ValueTask.FromResult(Result.Success(10)).Ensure(async x =>
        {
            await Task.Yield();
            return x > 5;
        }, TestError);
        ensure1.IsSuccess.Should().BeTrue();

        var ensure1Fail = await ValueTask.FromResult(Result.Success(10)).Ensure(async x =>
        {
            await Task.Yield();
            return x < 5;
        }, TestError);
        ensure1Fail.IsFailure.Should().BeTrue();
        ensure1Fail.Error.Should().Be(TestError);

        var ensure2 = await ValueTask.FromResult(Result.Success(10)).Ensure(5, async (s, x) =>
        {
            await Task.Yield();
            return x > s;
        }, TestError);
        ensure2.IsSuccess.Should().BeTrue();

        var ensure2Fail = await ValueTask.FromResult(Result.Success(10)).Ensure(5, async (s, x) =>
        {
            await Task.Yield();
            return x < s;
        }, TestError);
        ensure2Fail.IsFailure.Should().BeTrue();
        ensure2Fail.Error.Should().Be(TestError);

        // Synchronous fast path calling with failure
        var mapFail1 = await ValueTask.FromResult(Result.Failure<int>(TestError)).Map(async x => { await Task.Yield(); return x * 2; });
        mapFail1.IsFailure.Should().BeTrue();

        var mapFail2 = await ValueTask.FromResult(Result.Failure<int>(TestError)).Map(5, async (s, x) => { await Task.Yield(); return s + x; });
        mapFail2.IsFailure.Should().BeTrue();

        var bindFail1 = await ValueTask.FromResult(Result.Failure<int>(TestError)).Bind(async x => { await Task.Yield(); return Result.Success(x * 2); });
        bindFail1.IsFailure.Should().BeTrue();

        Func<int, ValueTask<Result>> asyncBindFail = async x => { await Task.Yield(); return Result.Success(); };
        var bindFail2 = await ValueTask.FromResult(Result.Failure<int>(TestError)).Bind(asyncBindFail);
        bindFail2.IsFailure.Should().BeTrue();

        var ensureFail1 = await ValueTask.FromResult(Result.Failure<int>(TestError)).Ensure(async x => { await Task.Yield(); return true; }, TestError2);
        ensureFail1.IsFailure.Should().BeTrue();
        ensureFail1.Error.Should().Be(TestError);

        var ensureFail2 = await ValueTask.FromResult(Result.Failure<int>(TestError)).Ensure(5, async (s, x) => { await Task.Yield(); return true; }, TestError2);
        ensureFail2.IsFailure.Should().BeTrue();
        ensureFail2.Error.Should().Be(TestError);

        var recoverSuccess1 = await ValueTask.FromResult(Result.Success(10)).Recover(async e => { await Task.Yield(); return Result.Success(99); });
        recoverSuccess1.IsSuccess.Should().BeTrue();
        recoverSuccess1.Value.Should().Be(10);

        var recoverSuccess2 = await ValueTask.FromResult(Result.Success(10)).Recover(5, async (s, e) => { await Task.Yield(); return Result.Success(s); });
        recoverSuccess2.IsSuccess.Should().BeTrue();
        recoverSuccess2.Value.Should().Be(10);

        var recoverNonGenSuccess = await ValueTask.FromResult(Result.Success()).Recover(async e => { await Task.Yield(); return Result.Success(); });
        recoverNonGenSuccess.IsSuccess.Should().BeTrue();

        var bindNonGenFail = await ValueTask.FromResult(Result.Failure(TestError)).Bind(async () => { await Task.Yield(); return Result.Success(); });
        bindNonGenFail.IsFailure.Should().BeTrue();

        var bindNonGenTNextFail = await ValueTask.FromResult(Result.Failure(TestError)).Bind(async () => { await Task.Yield(); return Result.Success(42); });
        bindNonGenTNextFail.IsFailure.Should().BeTrue();

        var tapNonGenFail = await ValueTask.FromResult(Result.Failure(TestError)).TapOnSuccess(async () => { await Task.Yield(); });
        tapNonGenFail.IsFailure.Should().BeTrue();

        var tapFailureNonGenSuccess = await ValueTask.FromResult(Result.Success()).TapOnFailure(async e => { await Task.Yield(); });
        tapFailureNonGenSuccess.IsSuccess.Should().BeTrue();
    }
}




