// Copyright © Erickson Lopez. MIT License.
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultNonGenericComprehensiveTests
{
    private static readonly Error SampleError1 = Error.Failure("ERR_01", "First error");
    private static readonly Error SampleError2 = Error.Validation("ERR_02", "Second error");

    #region Factory & Basic State

    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.IsUninitialized.Should().BeFalse();

        var act = () => result.Error;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access the Error of a successful result.");
    }

    [Fact]
    public void Failure_WithValidError_CreatesFailedResult()
    {
        var result = Result.Failure(SampleError1);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.IsUninitialized.Should().BeFalse();
        result.Error.Should().Be(SampleError1);
    }

    [Fact]
    public void Failure_WithNullError_ThrowsArgumentNullException()
    {
        var act = () => Result.Failure(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Default_IsUninitialized()
    {
        Result result = default;

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeFalse();
        result.IsUninitialized.Should().BeTrue();
        result.Error.Should().Be(WellKnownErrors.UninitializedError);
    }

    [Fact]
    public void GenericFactoryMethods_OnNonGenericResultClass_WorkProperly()
    {
        var success = Result.Success(42);
        success.IsSuccess.Should().BeTrue();
        success.Value.Should().Be(42);

        var failure = Result.Failure<int>(SampleError1);
        failure.IsFailure.Should().BeTrue();
        failure.Error.Should().Be(SampleError1);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailureResult()
    {
        Result result = SampleError1;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SampleError1);
    }

    [Fact]
    public void OpTrue_WhenSuccess_ReturnsTrue()
    {
        var success = Result.Success();
        (success ? true : false).Should().BeTrue();
        var opTrueMethod = typeof(Result).GetMethod("op_True", BindingFlags.Public | BindingFlags.Static)!;
        ((bool)opTrueMethod.Invoke(null, new object[] { success })!).Should().BeTrue();
    }

    [Fact]
    public void OpTrue_WhenFailure_ReturnsFalse()
    {
        var failure = Result.Failure(SampleError1);
        (failure ? true : false).Should().BeFalse();
        var opTrueMethod = typeof(Result).GetMethod("op_True", BindingFlags.Public | BindingFlags.Static)!;
        ((bool)opTrueMethod.Invoke(null, new object[] { failure })!).Should().BeFalse();
    }

    [Fact]
    public void OpTrue_WhenUninitialized_ReturnsFalse()
    {
        Result uninitialized = default;
        (uninitialized ? true : false).Should().BeFalse();
        var opTrueMethod = typeof(Result).GetMethod("op_True", BindingFlags.Public | BindingFlags.Static)!;
        ((bool)opTrueMethod.Invoke(null, new object[] { uninitialized })!).Should().BeFalse();
    }

    [Fact]
    public void OpFalse_WhenSuccess_ReturnsFalse()
    {
        var success = Result.Success();
        var opFalseMethod = typeof(Result).GetMethod("op_False", BindingFlags.Public | BindingFlags.Static)!;
        ((bool)opFalseMethod.Invoke(null, new object[] { success })!).Should().BeFalse();
    }

    [Fact]
    public void OpFalse_WhenFailure_ReturnsTrue()
    {
        var failure = Result.Failure(SampleError1);
        var opFalseMethod = typeof(Result).GetMethod("op_False", BindingFlags.Public | BindingFlags.Static)!;
        ((bool)opFalseMethod.Invoke(null, new object[] { failure })!).Should().BeTrue();
    }

    [Fact]
    public void OpFalse_WhenUninitialized_ReturnsFalse()
    {
        Result uninitialized = default;
        var opFalseMethod = typeof(Result).GetMethod("op_False", BindingFlags.Public | BindingFlags.Static)!;
        ((bool)opFalseMethod.Invoke(null, new object[] { uninitialized })!).Should().BeFalse();
    }

    #endregion

    #region Match & Execute

    [Fact]
    public void Match_WhenSuccess_InvokesOnSuccess()
    {
        var result = Result.Success();
        var evaluated = result.Match(() => "ok", _ => "err");
        evaluated.Should().Be("ok");
    }

    [Fact]
    public void Match_WhenFailure_InvokesOnFailure()
    {
        var result = Result.Failure(SampleError1);
        var evaluated = result.Match(() => "ok", e => e.Code);
        evaluated.Should().Be("ERR_01");
    }

    [Fact]
    public void Match_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result result = default;
        var act = () => result.Match(() => 1, _ => 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Match_WithState_InvokesCorrectBranch()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        success.Match(10, s => s * 2, (s, _) => s * 3).Should().Be(20);
        failure.Match(10, s => s * 2, (s, _) => s * 3).Should().Be(30);

        var act = () => uninitialized.Match(10, s => s, (s, _) => s);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Execute_WhenSuccess_InvokesOnSuccessOnly()
    {
        var result = Result.Success();
        bool successCalled = false;
        bool failureCalled = false;

        result.Execute(() => successCalled = true, _ => failureCalled = true);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public void Execute_WhenFailure_InvokesOnFailureOnly()
    {
        var result = Result.Failure(SampleError1);
        bool successCalled = false;
        Error? caughtError = null;

        result.Execute(() => successCalled = true, e => caughtError = e);

        successCalled.Should().BeFalse();
        caughtError.Should().Be(SampleError1);
    }

    [Fact]
    public void Execute_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result result = default;
        var act = () => result.Execute(() => { }, _ => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Execute_WithState_InvokesCorrectBranch()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        int stateValue = 0;
        success.Execute(5, s => stateValue += s, (s, _) => stateValue -= s);
        stateValue.Should().Be(5);

        failure.Execute(3, s => stateValue += s, (s, _) => stateValue -= s);
        stateValue.Should().Be(2);

        var act = () => uninitialized.Execute(1, _ => { }, (_, _) => { });
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region MapFailure

    [Fact]
    public void MapFailure_WhenSuccess_ReturnsDefault()
    {
        var result = Result.Success();
        result.MapFailure(e => e.Code, "default_val").Should().Be("default_val");
    }

    [Fact]
    public void MapFailure_WhenFailure_ReturnsMappedError()
    {
        var result = Result.Failure(SampleError1);
        result.MapFailure(e => e.Code, "default_val").Should().Be("ERR_01");
    }

    [Fact]
    public void MapFailure_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result result = default;
        var act = () => result.MapFailure(e => e.Code, "default");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MapFailure_WithState_BehavesCorrectly()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        int successCalls = 0;
        var sRes = success.MapFailure("prefix", (s, e) => { successCalls++; return $"{s}:{e.Code}"; }, "default");
        successCalls.Should().Be(0);
        sRes.Should().Be("default");

        int failCalls = 0;
        var fRes = failure.MapFailure("prefix", (s, e) => { failCalls++; return $"{s}:{e.Code}"; }, "default");
        failCalls.Should().Be(1);
        fRes.Should().Be("prefix:ERR_01");

        var act = () => uninitialized.MapFailure("prefix", (s, e) => $"{s}:{e.Code}", "default");
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Map & Bind

    [Fact]
    public void Map_WhenSuccess_ReturnsTypedSuccess()
    {
        var result = Result.Success();
        var mapped = result.Map(() => "mapped_value");

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("mapped_value");
    }

    [Fact]
    public void Map_WhenFailure_ReturnsTypedFailureWithSameError()
    {
        var result = Result.Failure(SampleError1);
        var mapped = result.Map(() => "mapped_value");

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(SampleError1);
    }

    [Fact]
    public void Map_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result result = default;
        var act = () => result.Map(() => 42);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Map_WithState_BehavesCorrectly()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        success.Map(100, s => s * 2).Value.Should().Be(200);
        failure.Map(100, s => s * 2).Error.Should().Be(SampleError1);

        var act = () => uninitialized.Map(100, s => s);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Bind_NonGeneric_WhenSuccess_ReturnsBoundResult()
    {
        var success = Result.Success();
        var boundSuccess = success.Bind(() => Result.Success());
        boundSuccess.IsSuccess.Should().BeTrue();

        var boundFailure = success.Bind(() => Result.Failure(SampleError2));
        boundFailure.IsFailure.Should().BeTrue();
        boundFailure.Error.Should().Be(SampleError2);
    }

    [Fact]
    public void Bind_NonGeneric_WhenFailure_ReturnsExistingFailure()
    {
        var failure = Result.Failure(SampleError1);
        bool called = false;

        var result = failure.Bind(() => { called = true; return Result.Success(); });

        called.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SampleError1);
    }

    [Fact]
    public void Bind_NonGeneric_WhenUninitialized_Throws()
    {
        Result uninitialized = default;
        var act = () => uninitialized.Bind(() => Result.Success());
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Bind_NonGeneric_WithState_BehavesCorrectly()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        success.Bind(10, s => s > 5 ? Result.Success() : Result.Failure(SampleError2)).IsSuccess.Should().BeTrue();
        failure.Bind(10, s => Result.Success()).Error.Should().Be(SampleError1);

        var act = () => uninitialized.Bind(10, _ => Result.Success());
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Bind_Generic_WhenSuccess_ReturnsBoundGenericResult()
    {
        var success = Result.Success();
        var bound = success.Bind(() => Result.Success("hello"));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("hello");
    }

    [Fact]
    public void Bind_Generic_WhenFailure_ReturnsTypedFailure()
    {
        var failure = Result.Failure(SampleError1);
        var bound = failure.Bind(() => Result.Success("hello"));

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be(SampleError1);
    }

    [Fact]
    public void Bind_Generic_WhenUninitialized_Throws()
    {
        Result uninitialized = default;
        var act = () => uninitialized.Bind(() => Result.Success("hello"));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Bind_Generic_WithState_BehavesCorrectly()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        success.Bind("world", s => Result.Success($"hello {s}")).Value.Should().Be("hello world");
        failure.Bind("world", s => Result.Success($"hello {s}")).Error.Should().Be(SampleError1);

        var act = () => uninitialized.Bind("world", s => Result.Success(s));
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Side Effects: TapOnSuccess & TapOnFailure & Inspect

    [Fact]
    public void TapOnSuccess_ExecutesOnlyOnSuccess()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        bool successRun = false;
        var retSuccess = success.TapOnSuccess(() => successRun = true);
        successRun.Should().BeTrue();
        retSuccess.IsSuccess.Should().BeTrue();

        bool failureRun = false;
        var retFailure = failure.TapOnSuccess(() => failureRun = true);
        failureRun.Should().BeFalse();
        retFailure.IsFailure.Should().BeTrue();

        var act = () => uninitialized.TapOnSuccess(() => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TapOnSuccess_WithState_BehavesCorrectly()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        int counter = 0;
        success.TapOnSuccess(5, s => counter += s);
        counter.Should().Be(5);

        failure.TapOnSuccess(5, s => counter += s);
        counter.Should().Be(5);

        var act = () => uninitialized.TapOnSuccess(5, _ => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TapOnFailure_ExecutesOnlyOnFailure()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        bool successRun = false;
        success.TapOnFailure(_ => successRun = true);
        successRun.Should().BeFalse();

        Error? caught = null;
        failure.TapOnFailure(e => caught = e);
        caught.Should().Be(SampleError1);

        var act = () => uninitialized.TapOnFailure(_ => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TapOnFailure_WithState_BehavesCorrectly()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        string val = "";
        success.TapOnFailure("test", (s, _) => val = s);
        val.Should().Be("");

        failure.TapOnFailure("test", (s, e) => val = $"{s}:{e.Code}");
        val.Should().Be("test:ERR_01");

        var act = () => uninitialized.TapOnFailure("test", (_, _) => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Inspect_ExecutesUnconditionallyOnSuccessAndFailure()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        bool successSeen = false;
        success.Inspect(r => successSeen = r.IsSuccess);
        successSeen.Should().BeTrue();

        bool failureSeen = false;
        failure.Inspect(r => failureSeen = r.IsFailure);
        failureSeen.Should().BeTrue();

        var act = () => uninitialized.Inspect(_ => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Inspect_WithState_BehavesCorrectly()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        int seen = 0;
        success.Inspect(10, (s, r) => { if (r.IsSuccess) seen += s; });
        seen.Should().Be(10);

        failure.Inspect(20, (s, r) => { if (r.IsFailure) seen += s; });
        seen.Should().Be(30);

        var act = () => uninitialized.Inspect(1, (_, _) => { });
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Ensure

    [Fact]
    public void Ensure_WithPredicateAndError_BehavesCorrectly()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        success.Ensure(() => true, SampleError2).IsSuccess.Should().BeTrue();
        var failed = success.Ensure(() => false, SampleError2);
        failed.IsFailure.Should().BeTrue();
        failed.Error.Should().Be(SampleError2);

        // Failure short-circuits
        failure.Ensure(() => false, SampleError2).Error.Should().Be(SampleError1);

        var act = () => uninitialized.Ensure(() => true, SampleError2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Ensure_WithState_BehavesCorrectly()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        success.Ensure(10, s => s == 10, SampleError2).IsSuccess.Should().BeTrue();
        success.Ensure(10, s => s != 10, SampleError2).Error.Should().Be(SampleError2);

        failure.Ensure(10, s => false, SampleError2).Error.Should().Be(SampleError1);

        var act = () => uninitialized.Ensure(10, _ => true, SampleError2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Ensure_WithErrorFactory_ConstructsErrorLazilyOnlyOnPredicateFailure()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        bool factoryCalled = false;
        success.Ensure(() => true, () => { factoryCalled = true; return SampleError2; }).IsSuccess.Should().BeTrue();
        factoryCalled.Should().BeFalse();

        var failed = success.Ensure(() => false, () => { factoryCalled = true; return SampleError2; });
        factoryCalled.Should().BeTrue();
        failed.Error.Should().Be(SampleError2);

        // Failure short-circuits
        factoryCalled = false;
        failure.Ensure(() => false, () => { factoryCalled = true; return SampleError2; }).Error.Should().Be(SampleError1);
        factoryCalled.Should().BeFalse();

        var act = () => uninitialized.Ensure(() => true, () => SampleError2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Ensure_WithStateAndErrorFactory_BehavesCorrectly()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        bool factoryCalled = false;
        var sRes1 = success.Ensure(5, s => s == 5, () => { factoryCalled = true; return SampleError2; });
        sRes1.IsSuccess.Should().BeTrue();
        factoryCalled.Should().BeFalse();

        var sRes2 = success.Ensure(5, s => s != 5, () => { factoryCalled = true; return SampleError2; });
        sRes2.Error.Should().Be(SampleError2);
        factoryCalled.Should().BeTrue();

        factoryCalled = false;
        var fRes = failure.Ensure(5, _ => false, () => { factoryCalled = true; return SampleError2; });
        fRes.Error.Should().Be(SampleError1);
        factoryCalled.Should().BeFalse();

        var act = () => uninitialized.Ensure(5, _ => true, () => SampleError2);
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Recover & MapError

    [Fact]
    public void Recover_WhenFailure_InvokesRecoveryFunction()
    {
        var failure = Result.Failure(SampleError1);
        var recovered = failure.Recover(e => e.Code == "ERR_01" ? Result.Success() : Result.Failure(SampleError2));
        recovered.IsSuccess.Should().BeTrue();

        var notRecovered = failure.Recover(e => Result.Failure(SampleError2));
        notRecovered.IsFailure.Should().BeTrue();
        notRecovered.Error.Should().Be(SampleError2);
    }

    [Fact]
    public void Recover_WhenSuccess_ReturnsThisUnchanged()
    {
        var success = Result.Success();
        bool called = false;
        var result = success.Recover(_ => { called = true; return Result.Success(); });

        called.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Recover_WhenUninitialized_Throws()
    {
        Result uninitialized = default;
        var act = () => uninitialized.Recover(_ => Result.Success());
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Recover_WithState_BehavesCorrectly()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        success.Recover("fallback", (s, _) => Result.Failure(Error.Custom(s, s, ErrorType.Failure))).IsSuccess.Should().BeTrue();
        failure.Recover("fallback", (s, _) => Result.Failure(Error.Custom(s, s, ErrorType.Failure))).Error.Code.Should().Be("fallback");

        var act = () => uninitialized.Recover("fallback", (_, _) => Result.Success());
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MapError_WhenFailure_MapsErrorProperly()
    {
        var failure = Result.Failure(SampleError1);
        var mapped = failure.MapError(e => Error.Validation("NEW_CODE", e.Description));

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Code.Should().Be("NEW_CODE");
        mapped.Error.Description.Should().Be("First error");
    }

    [Fact]
    public void MapError_WhenSuccess_ReturnsThisUnchanged()
    {
        var success = Result.Success();
        bool called = false;
        var result = success.MapError(e => { called = true; return e; });

        called.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void MapError_WhenUninitialized_Throws()
    {
        Result uninitialized = default;
        var act = () => uninitialized.MapError(e => e);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MapError_WithState_BehavesCorrectly()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        success.MapError("P", (s, e) => Error.Custom(s, e.Description, ErrorType.Failure)).IsSuccess.Should().BeTrue();
        failure.MapError("P", (s, e) => Error.Custom(s, e.Description, ErrorType.Failure)).Error.Code.Should().Be("P");

        var act = () => uninitialized.MapError("P", (_, e) => e);
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Deconstruct & TryGetError

    [Fact]
    public void Deconstruct_BehavesCorrectlyAcrossStates()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        var (sOk, sErr) = success;
        sOk.Should().BeTrue();
        sErr.Should().BeNull();

        var (fOk, fErr) = failure;
        fOk.Should().BeFalse();
        fErr.Should().Be(SampleError1);

        var (uOk, uErr) = uninitialized;
        uOk.Should().BeFalse();
        uErr.Should().Be(WellKnownErrors.UninitializedError);
    }

    [Fact]
    public void TryGetError_SingleOut_BehavesCorrectly()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        success.TryGetError(out var err1).Should().BeFalse();
        err1.Should().BeNull();

        failure.TryGetError(out var err2).Should().BeTrue();
        err2.Should().Be(SampleError1);

        uninitialized.TryGetError(out var err3).Should().BeFalse();
        err3.Should().BeNull();
    }

    [Fact]
    public void TryGetError_WithIsUninitializedOut_DistinguishesUninitialized()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        success.TryGetError(out var err1, out var isUninit1).Should().BeFalse();
        err1.Should().BeNull();
        isUninit1.Should().BeFalse();

        failure.TryGetError(out var err2, out var isUninit2).Should().BeTrue();
        err2.Should().Be(SampleError1);
        isUninit2.Should().BeFalse();

        uninitialized.TryGetError(out var err3, out var isUninit3).Should().BeFalse();
        err3.Should().BeNull();
        isUninit3.Should().BeTrue();
    }

    #endregion

    #region Try / TryAsync / TryAsyncValue Exception Bridges

    [Fact]
    public void Try_Action_Success_ReturnsSuccessResult()
    {
        var result = Result.Try(() => { }, ex => Error.Failure("EX", ex.Message));
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Try_Action_NonFatalException_ReturnsFailure()
    {
        var result = Result.Try(
            () => throw new InvalidOperationException("boom"),
            ex => Error.Failure("OP_ERR", ex.Message));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OP_ERR");
        result.Error.Description.Should().Be("boom");
    }

#pragma warning disable CA2201
    [Fact]
    public void Try_Action_FatalException_ReThrows()
    {
        var act = () => Result.Try(
            () => throw new OutOfMemoryException(),
            ex => Error.Failure("OP_ERR", ex.Message));

        act.Should().Throw<OutOfMemoryException>();
    }
#pragma warning restore CA2201

    [Fact]
    public void Try_Action_WithState_BehavesCorrectly()
    {
        var result = Result.Try(
            "context_info",
            () => throw new InvalidOperationException("boom"),
            (ctx, ex) => Error.Failure(ctx, ex.Message));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("context_info");
        result.Error.Description.Should().Be("boom");
    }

    [Fact]
    public void Try_FuncT_Success_ReturnsSuccessResult()
    {
        var result = Result.Try(() => 123, ex => Error.Failure("EX", ex.Message));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(123);
    }

    [Fact]
    public void Try_FuncT_NonFatalException_ReturnsFailure()
    {
        var result = Result.Try<int>(
            () => throw new ArgumentException("bad arg"),
            ex => Error.Failure("ARG_ERR", ex.Message));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ARG_ERR");
    }

#pragma warning disable CA2201
    [Fact]
    public void Try_FuncT_FatalException_ReThrows()
    {
        var act = () => Result.Try<int>(
            () => throw new AccessViolationException(),
            ex => Error.Failure("EX", ex.Message));

        act.Should().Throw<AccessViolationException>();
    }

    [Fact]
    public void Try_FuncT_WithState_BehavesCorrectly()
    {
        var success = Result.Try(50, () => 50 * 2, (_, ex) => Error.Failure("EX", ex.Message));
        success.Value.Should().Be(100);

        var failure = Result.Try<int, string>(50, () => throw new InvalidOperationException("err"), (s, ex) => Error.Failure(s.ToString(), ex.Message));
        failure.Error.Code.Should().Be("50");
    }
#pragma warning restore CA2201

    [Fact]
    public async Task TryAsync_Action_SuccessAndFailure()
    {
        var success = await Result.TryAsync(async () => await Task.Yield(), ex => Error.Failure("EX", ex.Message));
        success.IsSuccess.Should().BeTrue();

        var failure = await Result.TryAsync(
            async () => { await Task.Yield(); throw new InvalidOperationException("async boom"); },
            ex => Error.Failure("ASYNC_ERR", ex.Message));
        failure.IsFailure.Should().BeTrue();
        failure.Error.Description.Should().Be("async boom");
    }

    [Fact]
    public async Task TryAsync_ActionWithCancellationToken_SuccessAndFailure()
    {
        using var cts = new CancellationTokenSource();
        var success = await Result.TryAsync(
            async ct => { await Task.Yield(); ct.ThrowIfCancellationRequested(); },
            ex => Error.Failure("EX", ex.Message),
            cts.Token);
        success.IsSuccess.Should().BeTrue();

        await cts.CancelAsync();
        var cancelled = await Result.TryAsync(
            async ct => { await Task.Yield(); ct.ThrowIfCancellationRequested(); },
            ex => Error.Unavailable("CANCELLED", ex.Message),
            cts.Token);
        cancelled.IsFailure.Should().BeTrue();
        cancelled.Error.Code.Should().Be("CANCELLED");
    }

    [Fact]
    public async Task TryAsync_FuncT_SuccessAndFailure()
    {
        var success = await Result.TryAsync(async () => { await Task.Yield(); return 777; }, ex => Error.Failure("EX", ex.Message));
        success.IsSuccess.Should().BeTrue();
        success.Value.Should().Be(777);

        var failure = await Result.TryAsync<int>(
            async () => { await Task.Yield(); throw new InvalidOperationException("async boom"); },
            ex => Error.Failure("ASYNC_ERR", ex.Message));
        failure.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsync_FuncTWithCancellationToken_SuccessAndFailure()
    {
        using var cts = new CancellationTokenSource();
        var success = await Result.TryAsync(
            async ct => { await Task.Yield(); return 888; },
            ex => Error.Failure("EX", ex.Message),
            cts.Token);
        success.Value.Should().Be(888);

        await cts.CancelAsync();
        var cancelled = await Result.TryAsync<int>(
            async ct => { await Task.Yield(); ct.ThrowIfCancellationRequested(); return 0; },
            ex => Error.Unavailable("CANCELLED", ex.Message),
            cts.Token);
        cancelled.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsyncValue_Action_SuccessAndFailure()
    {
        var success = await Result.TryAsyncValue(() => ValueTask.CompletedTask, ex => Error.Failure("EX", ex.Message));
        success.IsSuccess.Should().BeTrue();

        var failure = await Result.TryAsyncValue(
            () => ValueTask.FromException(new InvalidOperationException("vt boom")),
            ex => Error.Failure("VT_ERR", ex.Message));
        failure.IsFailure.Should().BeTrue();
        failure.Error.Description.Should().Be("vt boom");
    }

    [Fact]
    public async Task TryAsyncValue_ActionWithCancellationToken_SuccessAndFailure()
    {
        using var cts = new CancellationTokenSource();
        var success = await Result.TryAsyncValue(
            ct => ValueTask.CompletedTask,
            ex => Error.Failure("EX", ex.Message),
            cts.Token);
        success.IsSuccess.Should().BeTrue();

        await cts.CancelAsync();
        var failure = await Result.TryAsyncValue(
            ct => ValueTask.FromException(new OperationCanceledException(ct)),
            ex => Error.Failure("CANCEL", ex.Message),
            cts.Token);
        failure.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsyncValue_FuncT_SuccessAndFailure()
    {
        var success = await Result.TryAsyncValue(() => ValueTask.FromResult(999), ex => Error.Failure("EX", ex.Message));
        success.IsSuccess.Should().BeTrue();
        success.Value.Should().Be(999);

        var failure = await Result.TryAsyncValue<int>(
            () => ValueTask.FromException<int>(new InvalidOperationException("vt boom")),
            ex => Error.Failure("VT_ERR", ex.Message));
        failure.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsyncValue_FuncTWithCancellationToken_SuccessAndFailure()
    {
        using var cts = new CancellationTokenSource();
        var success = await Result.TryAsyncValue(
            ct => ValueTask.FromResult(1234),
            ex => Error.Failure("EX", ex.Message),
            cts.Token);
        success.Value.Should().Be(1234);

        await cts.CancelAsync();
        var failure = await Result.TryAsyncValue<int>(
            ct => ValueTask.FromException<int>(new OperationCanceledException(ct)),
            ex => Error.Failure("CANCEL", ex.Message),
            cts.Token);
        failure.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsyncValue_FuncTWithState_SuccessAndFailure()
    {
        var success = await Result.TryAsyncValue(
            "context",
            () => ValueTask.FromResult(555),
            (s, ex) => Error.Failure(s, ex.Message));
        success.Value.Should().Be(555);

        var failure = await Result.TryAsyncValue<string, int>(
            "context",
            () => ValueTask.FromException<int>(new InvalidOperationException("boom")),
            (s, ex) => Error.Failure(s, ex.Message));
        failure.IsFailure.Should().BeTrue();
        failure.Error.Code.Should().Be("context");
    }

    [Fact]
    public async Task TryAsyncValue_FuncTWithStateAndCancellationToken_SuccessAndFailure()
    {
        using var cts = new CancellationTokenSource();
        var success = await Result.TryAsyncValue(
            "context",
            ct => ValueTask.FromResult(666),
            (s, ex) => Error.Failure(s, ex.Message),
            cts.Token);
        success.Value.Should().Be(666);

        await cts.CancelAsync();
        var failure = await Result.TryAsyncValue<string, int>(
            "context",
            ct => ValueTask.FromException<int>(new OperationCanceledException(ct)),
            (s, ex) => Error.Failure(s, ex.Message),
            cts.Token);
        failure.IsFailure.Should().BeTrue();
        failure.Error.Code.Should().Be("context");
    }

    #endregion

    #region Equality & HashCode & Operators

    [Fact]
    public void Equals_SameStateAndError_ReturnsTrue()
    {
        var s1 = Result.Success();
        var s2 = Result.Success();
        var f1 = Result.Failure(SampleError1);
        var f2 = Result.Failure(SampleError1);
        Result u1 = default;
        Result u2 = default;

        s1.Equals(s2).Should().BeTrue();
        s1.Equals((object)s2).Should().BeTrue();
        (s1 == s2).Should().BeTrue();
        (s1 != s2).Should().BeFalse();
        s1.GetHashCode().Should().Be(s2.GetHashCode());

        f1.Equals(f2).Should().BeTrue();
        f1.Equals((object)f2).Should().BeTrue();
        (f1 == f2).Should().BeTrue();
        (f1 != f2).Should().BeFalse();
        f1.GetHashCode().Should().Be(f2.GetHashCode());

        u1.Equals(u2).Should().BeTrue();
        u1.Equals((object)u2).Should().BeTrue();
        (u1 == u2).Should().BeTrue();
        (u1 != u2).Should().BeFalse();
        u1.GetHashCode().Should().Be(u2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentStatesOrErrors_ReturnsFalse()
    {
        var s = Result.Success();
        var f1 = Result.Failure(SampleError1);
        var f2 = Result.Failure(SampleError2);
        Result u = default;

        s.Equals(f1).Should().BeFalse();
        (s == f1).Should().BeFalse();
        (s != f1).Should().BeTrue();

        s.Equals(u).Should().BeFalse();
        (s == u).Should().BeFalse();

        f1.Equals(f2).Should().BeFalse();
        (f1 == f2).Should().BeFalse();
        (f1 != f2).Should().BeTrue();

        f1.Equals(u).Should().BeFalse();
        (f1 == u).Should().BeFalse();

        s.Equals((object?)null).Should().BeFalse();
        s.Equals("not a result").Should().BeFalse();
    }

    #endregion

    #region DebuggerDisplay & IResultOutcome

    [Fact]
    public void GetDebuggerDisplay_ReturnsExpectedStrings()
    {
        var method = typeof(Result).GetMethod("GetDebuggerDisplay", BindingFlags.Instance | BindingFlags.NonPublic);
        method.Should().NotBeNull();

        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        method!.Invoke(success, null).Should().Be("Success");
        method.Invoke(failure, null).Should().Be("Failure (ERR_01)");
        method.Invoke(uninitialized, null).Should().Be("Uninitialized");
    }

    [Fact]
    public void IResultOutcome_Properties_BehaveExpectedly()
    {
        var success = Result.Success();
        var failure = Result.Failure(SampleError1);
        Result uninitialized = default;

        IResultOutcome ioSuccess = success;
        ioSuccess.IsSuccess.Should().BeTrue();
        ioSuccess.IsFailure.Should().BeFalse();
        ioSuccess.IsUninitialized.Should().BeFalse();
        ioSuccess.Error.Should().BeNull();
        ioSuccess.RawValue.Should().BeNull();

        IResultOutcome ioFailure = failure;
        ioFailure.IsSuccess.Should().BeFalse();
        ioFailure.IsFailure.Should().BeTrue();
        ioFailure.IsUninitialized.Should().BeFalse();
        ioFailure.Error.Should().Be(SampleError1);
        ioFailure.RawValue.Should().BeNull();

        IResultOutcome ioUninit = uninitialized;
        ioUninit.IsSuccess.Should().BeFalse();
        ioUninit.IsFailure.Should().BeFalse();
        ioUninit.IsUninitialized.Should().BeTrue();
        ioUninit.Error.Should().BeNull();
        ioUninit.RawValue.Should().BeNull();
    }

    #endregion
}




