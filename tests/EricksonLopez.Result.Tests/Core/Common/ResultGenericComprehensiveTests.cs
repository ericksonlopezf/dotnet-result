// Copyright © Erickson Lopez. MIT License.
using System;
using System.Reflection;
using AwesomeAssertions;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultGenericComprehensiveTests
{
    private static readonly Error SampleError1 = Error.Failure("ERR_GEN_01", "First generic error");
    private static readonly Error SampleError2 = Error.Validation("ERR_GEN_02", "Second generic error");

    #region Factory & Basic State

    [Fact]
    public void Success_WithValidValue_CreatesSuccessfulResult()
    {
        var result = Result<string>.Success("hello");

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.IsUninitialized.Should().BeFalse();
        result.Value.Should().Be("hello");

        var act = () => result.Error;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access the Error of a successful result.");
    }

    [Fact]
    public void Success_WithNullValue_AllowedForNullableTypes()
    {
        var result = Result<string?>.Success(null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void Failure_WithValidError_CreatesFailedResult()
    {
        var result = Result<string>.Failure(SampleError1);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.IsUninitialized.Should().BeFalse();
        result.Error.Should().Be(SampleError1);

        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Failure_WithNullError_ThrowsArgumentNullException()
    {
        var act = () => Result<string>.Failure(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Default_IsUninitialized()
    {
        Result<int> result = default;

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeFalse();
        result.IsUninitialized.Should().BeTrue();
        result.Error.Should().Be(WellKnownErrors.UninitializedError);

        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access Value on an uninitialized default Result<T>.");
    }

    [Fact]
    public void ImplicitConversions_FromValueAndFromError_WorkProperly()
    {
        Result<int> fromValue = 100;
        fromValue.IsSuccess.Should().BeTrue();
        fromValue.Value.Should().Be(100);

        Result<int> fromError = SampleError1;
        fromError.IsFailure.Should().BeTrue();
        fromError.Error.Should().Be(SampleError1);
    }

    #endregion

    #region Map & Bind

    [Fact]
    public void Map_WhenSuccess_ProjectsValue()
    {
        var success = Result<int>.Success(10);
        var mapped = success.Map(v => $"Value is {v}");

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("Value is 10");
    }

    [Fact]
    public void Map_WhenFailure_ReturnsFailureWithSameError()
    {
        var failure = Result<int>.Failure(SampleError1);
        var mapped = failure.Map(v => $"Value is {v}");

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(SampleError1);
    }

    [Fact]
    public void Map_WhenUninitialized_Throws()
    {
        Result<int> uninitialized = default;
        var act = () => uninitialized.Map(v => v * 2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Map_WithState_BehavesCorrectly()
    {
        var success = Result<int>.Success(10);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.Map(5, (s, v) => s + v).Value.Should().Be(15);
        failure.Map(5, (s, v) => s + v).Error.Should().Be(SampleError1);

        var act = () => uninitialized.Map(5, (s, v) => s + v);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Bind_Generic_WhenSuccess_ReturnsBoundResult()
    {
        var success = Result<int>.Success(10);
        var bound = success.Bind(v => Result<string>.Success($"Number: {v}"));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("Number: 10");
    }

    [Fact]
    public void Bind_Generic_WhenFailure_ReturnsFailure()
    {
        var failure = Result<int>.Failure(SampleError1);
        var bound = failure.Bind(v => Result<string>.Success($"Number: {v}"));

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be(SampleError1);
    }

    [Fact]
    public void Bind_Generic_WhenUninitialized_Throws()
    {
        Result<int> uninitialized = default;
        var act = () => uninitialized.Bind(v => Result<string>.Success(v.ToString()));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Bind_Generic_WithState_BehavesCorrectly()
    {
        var success = Result<int>.Success(10);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.Bind("prefix", (s, v) => Result<string>.Success($"{s}:{v}")).Value.Should().Be("prefix:10");
        failure.Bind("prefix", (s, v) => Result<string>.Success($"{s}:{v}")).Error.Should().Be(SampleError1);

        var act = () => uninitialized.Bind("prefix", (s, v) => Result<string>.Success(s));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Bind_NonGeneric_WhenSuccess_ReturnsBoundNonGenericResult()
    {
        var success = Result<int>.Success(10);
        var bound = success.Bind(v => v > 5 ? Result.Success() : Result.Failure(SampleError2));

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Bind_NonGeneric_WhenFailure_ReturnsFailure()
    {
        var failure = Result<int>.Failure(SampleError1);
        var bound = failure.Bind(v => Result.Success());

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be(SampleError1);
    }

    [Fact]
    public void Bind_NonGeneric_WhenUninitialized_Throws()
    {
        Result<int> uninitialized = default;
        var act = () => uninitialized.Bind(v => Result.Success());
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Bind_NonGeneric_WithState_BehavesCorrectly()
    {
        var success = Result<int>.Success(10);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.Bind(5, (s, v) => v > s ? Result.Success() : Result.Failure(SampleError2)).IsSuccess.Should().BeTrue();
        failure.Bind(5, (s, v) => Result.Success()).Error.Should().Be(SampleError1);

        var act = () => uninitialized.Bind(5, (_, _) => Result.Success());
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Match & Execute

    [Fact]
    public void Match_WhenSuccess_InvokesOnSuccess()
    {
        var result = Result<int>.Success(42);
        result.Match(v => $"val_{v}", e => e.Code).Should().Be("val_42");
    }

    [Fact]
    public void Match_WhenFailure_InvokesOnFailure()
    {
        var result = Result<int>.Failure(SampleError1);
        result.Match(v => $"val_{v}", e => e.Code).Should().Be("ERR_GEN_01");
    }

    [Fact]
    public void Match_WhenUninitialized_Throws()
    {
        Result<int> uninitialized = default;
        var act = () => uninitialized.Match(v => v, _ => 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Match_WithState_BehavesCorrectly()
    {
        var success = Result<int>.Success(10);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.Match(100, (s, v) => s + v, (s, e) => s).Should().Be(110);
        failure.Match(100, (s, v) => s + v, (s, e) => s).Should().Be(100);

        var act = () => uninitialized.Match(100, (s, v) => s, (s, e) => s);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Execute_WhenSuccess_InvokesOnSuccessOnly()
    {
        var success = Result<int>.Success(42);
        int? seen = null;
        Error? caught = null;

        success.Execute(v => seen = v, e => caught = e);

        seen.Should().Be(42);
        caught.Should().BeNull();
    }

    [Fact]
    public void Execute_WhenFailure_InvokesOnFailureOnly()
    {
        var failure = Result<int>.Failure(SampleError1);
        int? seen = null;
        Error? caught = null;

        failure.Execute(v => seen = v, e => caught = e);

        seen.Should().BeNull();
        caught.Should().Be(SampleError1);
    }

    [Fact]
    public void Execute_WhenUninitialized_Throws()
    {
        Result<int> uninitialized = default;
        var act = () => uninitialized.Execute(_ => { }, _ => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Execute_WithState_BehavesCorrectly()
    {
        var success = Result<int>.Success(10);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        int total = 0;
        success.Execute(5, (s, v) => total += s + v, (s, e) => total -= s);
        total.Should().Be(15);

        failure.Execute(5, (s, v) => total += s + v, (s, e) => total -= s);
        total.Should().Be(10);

        var act = () => uninitialized.Execute(5, (_, _) => { }, (_, _) => { });
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region MapFailure

    [Fact]
    public void MapFailure_WhenSuccess_ReturnsDefault()
    {
        var success = Result<int>.Success(42);
        success.MapFailure(e => e.Code, "default_str").Should().Be("default_str");
    }

    [Fact]
    public void MapFailure_WhenFailure_MapsError()
    {
        var failure = Result<int>.Failure(SampleError1);
        failure.MapFailure(e => e.Code, "default_str").Should().Be("ERR_GEN_01");
    }

    [Fact]
    public void MapFailure_WhenUninitialized_Throws()
    {
        Result<int> uninitialized = default;
        var act = () => uninitialized.MapFailure(e => e.Code, "default");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MapFailure_WithState_BehavesCorrectly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        int successCalls = 0;
        var sRes = success.MapFailure("prefix", (s, e) => { successCalls++; return $"{s}:{e.Code}"; }, "default");
        successCalls.Should().Be(0);
        sRes.Should().Be("default");

        int failCalls = 0;
        var fRes = failure.MapFailure("prefix", (s, e) => { failCalls++; return $"{s}:{e.Code}"; }, "default");
        failCalls.Should().Be(1);
        fRes.Should().Be("prefix:ERR_GEN_01");

        var act = () => uninitialized.MapFailure("prefix", (s, e) => $"{s}:{e.Code}", "default");
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region TapOnSuccess & TapOnFailure & Inspect

    [Fact]
    public void TapOnSuccess_ExecutesOnlyOnSuccess()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        int? seen = null;
        success.TapOnSuccess(v => seen = v).Should().Be(success);
        seen.Should().Be(42);

        seen = null;
        failure.TapOnSuccess(v => seen = v).Should().Be(failure);
        seen.Should().BeNull();

        var act = () => uninitialized.TapOnSuccess(_ => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TapOnSuccess_WithState_BehavesCorrectly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        int resultVal = 0;
        success.TapOnSuccess(10, (s, v) => resultVal = s + v);
        resultVal.Should().Be(52);

        failure.TapOnSuccess(10, (s, v) => resultVal = 0);
        resultVal.Should().Be(52);

        var act = () => uninitialized.TapOnSuccess(10, (_, _) => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TapOnFailure_ExecutesOnlyOnFailure()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        Error? caught = null;
        success.TapOnFailure(e => caught = e).Should().Be(success);
        caught.Should().BeNull();

        failure.TapOnFailure(e => caught = e).Should().Be(failure);
        caught.Should().Be(SampleError1);

        var act = () => uninitialized.TapOnFailure(_ => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TapOnFailure_WithState_BehavesCorrectly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        string val = "";
        success.TapOnFailure("test", (s, e) => val = s);
        val.Should().Be("");

        failure.TapOnFailure("test", (s, e) => val = $"{s}:{e.Code}");
        val.Should().Be("test:ERR_GEN_01");

        var act = () => uninitialized.TapOnFailure("test", (_, _) => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Inspect_ExecutesUnconditionally()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        int count = 0;
        success.Inspect(r => { if (r.IsSuccess) count++; });
        count.Should().Be(1);

        failure.Inspect(r => { if (r.IsFailure) count++; });
        count.Should().Be(2);

        var act = () => uninitialized.Inspect(_ => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Inspect_WithState_BehavesCorrectly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        int score = 0;
        success.Inspect(10, (s, r) => score += s);
        score.Should().Be(10);

        failure.Inspect(20, (s, r) => score += s);
        score.Should().Be(30);

        var act = () => uninitialized.Inspect(1, (_, _) => { });
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Ensure

    [Fact]
    public void Ensure_WithPredicateAndError_BehavesCorrectly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.Ensure(v => v == 42, SampleError2).Value.Should().Be(42);
        success.Ensure(v => v != 42, SampleError2).Error.Should().Be(SampleError2);

        // Failure short-circuits
        failure.Ensure(v => false, SampleError2).Error.Should().Be(SampleError1);

        var act = () => uninitialized.Ensure(v => true, SampleError2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Ensure_WithState_BehavesCorrectly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.Ensure(10, (s, v) => v == 42, SampleError2).Value.Should().Be(42);
        success.Ensure(42, (s, v) => s != v, SampleError2).Error.Should().Be(SampleError2);

        failure.Ensure(42, (s, v) => false, SampleError2).Error.Should().Be(SampleError1);

        var act = () => uninitialized.Ensure(42, (s, v) => true, SampleError2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Ensure_WithErrorFactory_ConstructsErrorLazily()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        bool factoryCalled = false;
        success.Ensure(v => v == 42, () => { factoryCalled = true; return SampleError2; }).Value.Should().Be(42);
        factoryCalled.Should().BeFalse();

        success.Ensure(v => v != 42, () => { factoryCalled = true; return SampleError2; }).Error.Should().Be(SampleError2);
        factoryCalled.Should().BeTrue();

        factoryCalled = false;
        failure.Ensure(v => false, () => { factoryCalled = true; return SampleError2; }).Error.Should().Be(SampleError1);
        factoryCalled.Should().BeFalse();

        var act = () => uninitialized.Ensure(v => true, () => SampleError2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Ensure_WithStateAndErrorFactory_BehavesCorrectly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        bool factoryCalled = false;
        var sRes1 = success.Ensure(5, (s, v) => v == 42, () => { factoryCalled = true; return SampleError2; });
        sRes1.Value.Should().Be(42);
        factoryCalled.Should().BeFalse();

        var sRes2 = success.Ensure(42, (s, v) => s != v, () => { factoryCalled = true; return SampleError2; });
        sRes2.Error.Should().Be(SampleError2);
        factoryCalled.Should().BeTrue();

        factoryCalled = false;
        var fRes = failure.Ensure(42, (s, v) => false, () => { factoryCalled = true; return SampleError2; });
        fRes.Error.Should().Be(SampleError1);
        factoryCalled.Should().BeFalse();

        var act = () => uninitialized.Ensure(42, (s, v) => true, () => SampleError2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Ensure_WithValueErrorFactory_PassesValueToFactory()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.Ensure(v => v == 42, v => Error.Failure("CODE", $"val {v}")).Value.Should().Be(42);
        var failed = success.Ensure(v => v != 42, v => Error.Failure("CODE", $"val {v}"));
        failed.Error.Description.Should().Be("val 42");

        failure.Ensure(v => false, v => Error.Failure("CODE", $"val {v}")).Error.Should().Be(SampleError1);

        var act = () => uninitialized.Ensure(v => true, v => Error.Failure("CODE", $"val {v}"));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Ensure_WithStateAndValueErrorFactory_PassesBothToFactory()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        var sRes = success.Ensure("ctx", (s, v) => v == 42, (s, v) => Error.Failure(s, $"val {v}"));
        sRes.Value.Should().Be(42);
        var failed = success.Ensure("ctx", (s, v) => v != 42, (s, v) => Error.Failure(s, $"val {v}"));
        failed.Error.Code.Should().Be("ctx");
        failed.Error.Description.Should().Be("val 42");

        failure.Ensure("ctx", (s, v) => false, (s, v) => Error.Failure(s, $"val {v}")).Error.Should().Be(SampleError1);

        var act = () => uninitialized.Ensure("ctx", (s, v) => true, (s, v) => Error.Failure(s, $"val {v}"));
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Recover & MapError

    [Fact]
    public void Recover_WhenFailure_InvokesRecoveryFunction()
    {
        var failure = Result<int>.Failure(SampleError1);
        var recovered = failure.Recover(e => Result<int>.Success(100));
        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(100);

        var notRecovered = failure.Recover(e => Result<int>.Failure(SampleError2));
        notRecovered.IsFailure.Should().BeTrue();
        notRecovered.Error.Should().Be(SampleError2);
    }

    [Fact]
    public void Recover_WhenSuccess_ReturnsThisUnchanged()
    {
        var success = Result<int>.Success(42);
        bool called = false;
        var result = success.Recover(_ => { called = true; return Result<int>.Success(100); });

        called.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Recover_WhenUninitialized_Throws()
    {
        Result<int> uninitialized = default;
        var act = () => uninitialized.Recover(_ => Result<int>.Success(100));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Recover_WithState_BehavesCorrectly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.Recover(999, (s, e) => Result<int>.Success(s)).Value.Should().Be(42);
        failure.Recover(999, (s, e) => Result<int>.Success(s)).Value.Should().Be(999);

        var act = () => uninitialized.Recover(999, (s, e) => Result<int>.Success(s));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MapError_WhenFailure_TransformsError()
    {
        var failure = Result<int>.Failure(SampleError1);
        var mapped = failure.MapError(e => Error.Validation("TRANSFORMED", e.Description));

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Code.Should().Be("TRANSFORMED");
        mapped.Error.Description.Should().Be("First generic error");
    }

    [Fact]
    public void MapError_WhenSuccess_ReturnsThisUnchanged()
    {
        var success = Result<int>.Success(42);
        bool called = false;
        var result = success.MapError(e => { called = true; return e; });

        called.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void MapError_WhenUninitialized_Throws()
    {
        Result<int> uninitialized = default;
        var act = () => uninitialized.MapError(e => e);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MapError_WithState_BehavesCorrectly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.MapError("P", (s, e) => Error.Failure(s, e.Description)).IsSuccess.Should().BeTrue();
        failure.MapError("P", (s, e) => Error.Failure(s, e.Description)).Error.Code.Should().Be("P");

        var act = () => uninitialized.MapError("P", (s, e) => e);
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region TryGetValue & TryGetError & Safe Access

    [Fact]
    public void TryGetValue_SingleOut_BehavesCorrectly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.TryGetValue(out var v1).Should().BeTrue();
        v1.Should().Be(42);

        failure.TryGetValue(out var v2).Should().BeFalse();
        v2.Should().Be(0);

        uninitialized.TryGetValue(out var v3).Should().BeFalse();
        v3.Should().Be(0);
    }

    [Fact]
    public void TryGetValue_WithIsUninitializedOut_DistinguishesUninitialized()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.TryGetValue(out var v1, out var isUninit1).Should().BeTrue();
        v1.Should().Be(42);
        isUninit1.Should().BeFalse();

        failure.TryGetValue(out var v2, out var isUninit2).Should().BeFalse();
        v2.Should().Be(0);
        isUninit2.Should().BeFalse();

        uninitialized.TryGetValue(out var v3, out var isUninit3).Should().BeFalse();
        v3.Should().Be(0);
        isUninit3.Should().BeTrue();
    }

    [Fact]
    public void TryGetError_SingleOut_BehavesCorrectly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.TryGetError(out var e1).Should().BeFalse();
        e1.Should().BeNull();

        failure.TryGetError(out var e2).Should().BeTrue();
        e2.Should().Be(SampleError1);

        uninitialized.TryGetError(out var e3).Should().BeFalse();
        e3.Should().BeNull();
    }

    [Fact]
    public void TryGetError_WithIsUninitializedOut_DistinguishesUninitialized()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.TryGetError(out var e1, out var isUninit1).Should().BeFalse();
        e1.Should().BeNull();
        isUninit1.Should().BeFalse();

        failure.TryGetError(out var e2, out var isUninit2).Should().BeTrue();
        e2.Should().Be(SampleError1);
        isUninit2.Should().BeFalse();

        uninitialized.TryGetError(out var e3, out var isUninit3).Should().BeFalse();
        e3.Should().BeNull();
        isUninit3.Should().BeTrue();
    }

    [Fact]
    public void GetValueOrDefault_NeverThrows_ReturnsDefaultOnFailureAndUninitialized()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.GetValueOrDefault(99).Should().Be(42);
        failure.GetValueOrDefault(99).Should().Be(99);
        uninitialized.GetValueOrDefault(99).Should().Be(99);
    }

    [Fact]
    public void GetValueOrFallback_InvokesFallbackOnlyOnFailure_ThrowsOnUninitialized()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.GetValueOrFallback(e => 99).Should().Be(42);
        failure.GetValueOrFallback(e => 99).Should().Be(99);

        var act = () => uninitialized.GetValueOrFallback(e => 99);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetValueOrFallback_WithState_BehavesCorrectly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        success.GetValueOrFallback(100, (s, e) => s).Should().Be(42);
        failure.GetValueOrFallback(100, (s, e) => s).Should().Be(100);

        var act = () => uninitialized.GetValueOrFallback(100, (s, e) => s);
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region DiscardValue & Deconstruct

    [Fact]
    public void DiscardValue_ConvertsToNonGenericResult()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        var sNonGen = success.DiscardValue();
        sNonGen.IsSuccess.Should().BeTrue();

        var fNonGen = failure.DiscardValue();
        fNonGen.IsFailure.Should().BeTrue();
        fNonGen.Error.Should().Be(SampleError1);

        var act = () => uninitialized.DiscardValue();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Deconstruct_ThreeOutputs_BehavesCorrectly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        var (sOk, sVal, sErr) = success;
        sOk.Should().BeTrue();
        sVal.Should().Be(42);
        sErr.Should().BeNull();

        var (fOk, fVal, fErr) = failure;
        fOk.Should().BeFalse();
        fVal.Should().Be(0);
        fErr.Should().Be(SampleError1);

        var (uOk, uVal, uErr) = uninitialized;
        uOk.Should().BeFalse();
        uVal.Should().Be(0);
        uErr.Should().Be(WellKnownErrors.UninitializedError);
    }

    [Fact]
    public void Deconstruct_TwoOutputs_BehavesCorrectly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

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

    #endregion

    #region Equality & HashCode & Operators

    [Fact]
    public void Equals_SameStateValueAndError_ReturnsTrue()
    {
        var s1 = Result<int>.Success(42);
        var s2 = Result<int>.Success(42);
        var f1 = Result<int>.Failure(SampleError1);
        var f2 = Result<int>.Failure(SampleError1);
        Result<int> u1 = default;
        Result<int> u2 = default;

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
    public void Equals_DifferentStatesValuesOrErrors_ReturnsFalse()
    {
        var s1 = Result<int>.Success(42);
        var s2 = Result<int>.Success(99);
        var f1 = Result<int>.Failure(SampleError1);
        var f2 = Result<int>.Failure(SampleError2);
        Result<int> u = default;

        s1.Equals(s2).Should().BeFalse();
        (s1 == s2).Should().BeFalse();
        (s1 != s2).Should().BeTrue();

        s1.Equals(f1).Should().BeFalse();
        (s1 == f1).Should().BeFalse();
        (s1 != f1).Should().BeTrue();

        s1.Equals(u).Should().BeFalse();
        (s1 == u).Should().BeFalse();

        f1.Equals(f2).Should().BeFalse();
        (f1 == f2).Should().BeFalse();
        (f1 != f2).Should().BeTrue();

        f1.Equals(u).Should().BeFalse();
        (f1 == u).Should().BeFalse();

        s1.Equals((object?)null).Should().BeFalse();
        s1.Equals("not a generic result").Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_NullableReferenceTypeWithNull_ComputesDeterministicHash()
    {
        var sNull1 = Result<string?>.Success(null);
        var sNull2 = Result<string?>.Success(null);

        sNull1.GetHashCode().Should().Be(sNull2.GetHashCode());
    }

    #endregion

    #region DebuggerDisplay & IResultOutcome

    [Fact]
    public void GetDebuggerDisplay_ReturnsExpectedStrings()
    {
        var method = typeof(Result<int>).GetMethod("GetDebuggerDisplay", BindingFlags.Instance | BindingFlags.NonPublic);
        method.Should().NotBeNull();

        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        method!.Invoke(success, null).Should().Be("Success (42)");
        method.Invoke(failure, null).Should().Be("Failure (ERR_GEN_01)");
        method.Invoke(uninitialized, null).Should().Be("Uninitialized");
    }

    [Fact]
    public void IResultOutcome_Properties_BehaveExpectedly()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(SampleError1);
        Result<int> uninitialized = default;

        IResultOutcome ioSuccess = success;
        ioSuccess.IsSuccess.Should().BeTrue();
        ioSuccess.IsFailure.Should().BeFalse();
        ioSuccess.IsUninitialized.Should().BeFalse();
        ioSuccess.Error.Should().BeNull();
        ioSuccess.RawValue.Should().Be(42);

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

