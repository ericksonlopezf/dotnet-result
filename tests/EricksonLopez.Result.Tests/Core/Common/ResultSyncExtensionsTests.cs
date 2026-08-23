// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

/// <summary>
/// Tests for the ResultSyncExtensions class (R-02 ARB audit correction).
/// These extension methods accept Result&lt;TValue&gt; by reference (in) to avoid struct copies
/// for large value types like decimal and Guid.
/// </summary>
public class ResultSyncExtensionsTests
{
    // ─── Map ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var result = Result.Success(42m); // decimal is a large value type — good test case
        var mapped = ResultSyncExtensions.Map(result, v => v * 2);

        mapped.ShouldBeSuccess();
        Assert.Equal(84m, mapped.Value);
    }

    [Fact]
    public void Map_OnFailure_PropagatesError()
    {
        var error = Error.Failure("ERR", "fail");
        Result<decimal> result = error;
        var mapped = ResultSyncExtensions.Map(result, v => v * 2);

        mapped.ShouldBeFailure();
        Assert.Equal("ERR", mapped.Error.Code);
    }

    [Fact]
    public void Map_WithState_OnSuccess_TransformsValue()
    {
        var result = Result.Success(10m);
        var mapped = ResultSyncExtensions.Map(result, 5m, (state, v) => v + state);

        mapped.ShouldBeSuccess();
        Assert.Equal(15m, mapped.Value);
    }

    [Fact]
    public void Map_WithState_OnFailure_PropagatesError()
    {
        var error = Error.Failure("ERR", "fail");
        Result<decimal> result = error;
        var mapped = ResultSyncExtensions.Map(result, 5m, (state, v) => v + state);

        mapped.ShouldBeFailure();
        Assert.Equal("ERR", mapped.Error.Code);
    }

    // ─── Bind ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Bind_OnSuccess_ChainsToNextResult()
    {
        var result = Result.Success(Guid.NewGuid()); // Guid is 16 bytes
        var bound = ResultSyncExtensions.Bind(result, g => Result.Success(g.ToString()));

        bound.ShouldBeSuccess();
        Assert.NotEmpty(bound.Value);
    }

    [Fact]
    public void Bind_OnFailure_ShortCircuits()
    {
        var error = Error.Failure("ERR", "fail");
        Result<Guid> result = error;
        bool bindCalled = false;
        var bound = ResultSyncExtensions.Bind(result, g =>
        {
            bindCalled = true;
            return Result.Success(g.ToString());
        });

        Assert.False(bindCalled);
        bound.ShouldBeFailure();
        Assert.Equal("ERR", bound.Error.Code);
    }

    [Fact]
    public void Bind_WithState_OnSuccess_ChainsToNextResult()
    {
        var result = Result.Success(10m);
        var bound = ResultSyncExtensions.Bind(result, "prefix-", (state, v) => Result.Success(state + v.ToString()));

        bound.ShouldBeSuccess();
        Assert.StartsWith("prefix-", bound.Value);
    }

    // ─── Ensure ───────────────────────────────────────────────────────────────

    [Fact]
    public void Ensure_OnSuccess_PredicateTrue_ReturnsSuccess()
    {
        var result = Result.Success(42m);
        var ensured = ResultSyncExtensions.Ensure(result, v => v > 0, Error.Validation("V", "must be positive"));

        ensured.ShouldBeSuccess();
        Assert.Equal(42m, ensured.Value);
    }

    [Fact]
    public void Ensure_OnSuccess_PredicateFalse_ReturnsFailure()
    {
        var result = Result.Success(-1m);
        var validationError = Error.Validation("V", "must be positive");
        var ensured = ResultSyncExtensions.Ensure(result, v => v > 0, validationError);

        ensured.ShouldBeFailure();
        Assert.Equal("V", ensured.Error.Code);
    }

    [Fact]
    public void Ensure_OnFailure_ShortCircuits()
    {
        var error = Error.Failure("ERR", "fail");
        Result<decimal> result = error;
        bool predicateCalled = false;
        var ensured = ResultSyncExtensions.Ensure(result, v => { predicateCalled = true; return v > 0; }, Error.Validation("V", "v"));

        Assert.False(predicateCalled);
        ensured.ShouldBeFailure();
        Assert.Equal("ERR", ensured.Error.Code);
    }

    [Fact]
    public void Ensure_WithState_OnSuccess_PredicateTrue_ReturnsSuccess()
    {
        var result = Result.Success(42m);
        var ensured = ResultSyncExtensions.Ensure(result, 10m, (min, v) => v > min, Error.Validation("V", "too small"));

        ensured.ShouldBeSuccess();
    }

    // ─── Ensure with Func<Error> factory ─────────────────────────────────────

    [Fact]
    public void Ensure_WithErrorFactory_OnSuccess_PredicateTrue_DoesNotInvokeFactory()
    {
        var result = Result.Success(42m);
        bool factoryCalled = false;
        var ensured = ResultSyncExtensions.Ensure(result, v => v > 0, () => { factoryCalled = true; return Error.Validation("V", "neg"); });

        ensured.ShouldBeSuccess();
        Assert.False(factoryCalled, "Error factory must not be invoked when predicate passes");
    }

    [Fact]
    public void Ensure_WithErrorFactory_OnSuccess_PredicateFalse_InvokesFactory()
    {
        var result = Result.Success(-5m);
        bool factoryCalled = false;
        var ensured = ResultSyncExtensions.Ensure(result, v => v > 0, () => { factoryCalled = true; return Error.Validation("NEG", "negative"); });

        ensured.ShouldBeFailure();
        Assert.True(factoryCalled, "Error factory must be invoked when predicate fails");
        Assert.Equal("NEG", ensured.Error.Code);
    }

    [Fact]
    public void Ensure_WithErrorFactory_OnFailure_ShortCircuits_FactoryNotCalled()
    {
        var error = Error.Failure("ERR", "fail");
        Result<decimal> result = error;
        bool factoryCalled = false;
        var ensured = ResultSyncExtensions.Ensure(result, v => v > 0, () => { factoryCalled = true; return Error.Validation("V", "v"); });

        Assert.False(factoryCalled, "Error factory must not be invoked when already failed");
        ensured.ShouldBeFailure();
        Assert.Equal("ERR", ensured.Error.Code);
    }

    // ─── Ensure with Func<TValue, Error> value-contextual factory ─────────────

    [Fact]
    public void Ensure_WithValueContextualFactory_OnSuccess_PredicateFalse_ReceivesValue()
    {
        var result = Result.Success(-7m);
        decimal capturedValue = 0m;
        var ensured = ResultSyncExtensions.Ensure(result, v => v > 0, v => { capturedValue = v; return Error.Validation("NEG", $"value {v} is negative"); });

        ensured.ShouldBeFailure();
        Assert.Equal(-7m, capturedValue);
        Assert.Contains("-7", ensured.Error.Description);
    }

    [Fact]
    public void Ensure_WithValueContextualFactory_OnSuccess_PredicateTrue_FactoryNotCalled()
    {
        var result = Result.Success(5m);
        bool factoryCalled = false;
        var ensured = ResultSyncExtensions.Ensure(result, v => v > 0, v => { factoryCalled = true; return Error.Validation("V", "neg"); });

        ensured.ShouldBeSuccess();
        Assert.False(factoryCalled);
    }

    // ─── Ensure TState with Func<TState, Error> factory ──────────────────────

    [Fact]
    public void Ensure_WithState_WithStateFactory_OnSuccess_PredicateFalse_ReceivesState()
    {
        var result = Result.Success(3m);
        string capturedState = string.Empty;
        var ensured = ResultSyncExtensions.Ensure(result, "min10",
            (state, v) => v >= 10m,
            state => { capturedState = state; return Error.Validation("TOO_SMALL", $"need at least 10, state={state}"); });

        ensured.ShouldBeFailure();
        Assert.Equal("min10", capturedState);
        Assert.Contains("min10", ensured.Error.Description);
    }

    [Fact]
    public void Ensure_WithState_WithStateFactory_OnSuccess_PredicateTrue_FactoryNotCalled()
    {
        var result = Result.Success(15m);
        bool factoryCalled = false;
        var ensured = ResultSyncExtensions.Ensure(result, "min10",
            (state, v) => v >= 10m,
            state => { factoryCalled = true; return Error.Validation("V", "v"); });

        ensured.ShouldBeSuccess();
        Assert.False(factoryCalled);
    }

    // ─── Ensure TState with Func<TState, TValue, Error> factory ─────────────

    [Fact]
    public void Ensure_WithState_WithStateValueFactory_OnSuccess_PredicateFalse_ReceivesBoth()
    {
        var result = Result.Success(2m);
        string capturedState = string.Empty;
        decimal capturedValue = 0m;
        var ensured = ResultSyncExtensions.Ensure(result, "threshold=10",
            (state, v) => v >= 10m,
            (state, v) => { capturedState = state; capturedValue = v; return Error.Validation("TOO_SMALL", $"{v}<10"); });

        ensured.ShouldBeFailure();
        Assert.Equal("threshold=10", capturedState);
        Assert.Equal(2m, capturedValue);
    }

    [Fact]
    public void Ensure_WithState_WithStateValueFactory_OnFailure_ShortCircuits()
    {
        Result<decimal> result = Error.Failure("ERR", "already failed");
        bool factoryCalled = false;
        var ensured = ResultSyncExtensions.Ensure(result, "state",
            (state, v) => v >= 10m,
            (state, v) => { factoryCalled = true; return Error.Validation("V", "v"); });

        Assert.False(factoryCalled);
        ensured.ShouldBeFailure();
        Assert.Equal("ERR", ensured.Error.Code);
    }


    [Fact]
    public void Match_OnSuccess_InvokesOnSuccess()
    {
        var result = Result.Success(42m);
        var output = ResultSyncExtensions.Match(result, v => $"ok:{v}", e => $"err:{e.Code}");

        Assert.Equal("ok:42", output);
    }

    [Fact]
    public void Match_OnFailure_InvokesOnFailure()
    {
        Result<decimal> result = Error.Failure("ERR", "fail");
        var output = ResultSyncExtensions.Match(result, v => $"ok:{v}", e => $"err:{e.Code}");

        Assert.Equal("err:ERR", output);
    }

    [Fact]
    public void Match_WithState_OnSuccess_PassesState()
    {
        var result = Result.Success(10m);
        var output = ResultSyncExtensions.Match(result, "prefix-",
            (s, v) => s + v.ToString(),
            (s, e) => s + e.Code);

        Assert.Equal("prefix-10", output);
    }

    // ─── TryGetValue ──────────────────────────────────────────────────────────

    [Fact]
    public void TryGetValue_OnSuccess_ReturnsTrueAndValue()
    {
        var result = Result.Success(99m);
        var success = ResultSyncExtensions.TryGetValue(result, out var value);

        Assert.True(success);
        Assert.Equal(99m, value);
    }

    [Fact]
    public void TryGetValue_OnFailure_ReturnsFalseAndDefault()
    {
        Result<decimal> result = Error.Failure("ERR", "fail");
        var success = ResultSyncExtensions.TryGetValue(result, out var value);

        Assert.False(success);
        Assert.Equal(0m, value);
    }

    // ─── GetValueOrDefault ────────────────────────────────────────────────────

    [Fact]
    public void GetValueOrDefault_OnSuccess_ReturnsValue()
    {
        var result = Result.Success(42m);
        var value = ResultSyncExtensions.GetValueOrDefault(result, -1m);

        Assert.Equal(42m, value);
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_ReturnsDefault()
    {
        Result<decimal> result = Error.Failure("ERR", "fail");
        var value = ResultSyncExtensions.GetValueOrDefault(result, -1m);

        Assert.Equal(-1m, value);
    }

    [Fact]
    public void GetValueOrDefault_OnUninitialized_ReturnsDefault()
    {
        Result<decimal> result = default;
        var value = ResultSyncExtensions.GetValueOrDefault(result, -1m);

        Assert.Equal(-1m, value);
    }

    // ─── Large value type scenario ────────────────────────────────────────────

    [Fact]
    public void MapBindEnsureChain_WithDecimal_NoStructCopyPenalty()
    {
        // Smoke test: demonstrates the intended usage pattern for large value types.
        // This chain would incur 5+ struct copies if using instance methods directly.
        // With in-parameter extensions, the struct is passed by readonly reference.
        var result = Result.Success(100m)
            .Map(v => v + 50m)
            .Bind(v => v > 100m
                ? Result.Success(v)
                : Result.Failure<decimal>(Error.Validation("TOO_SMALL", "value too small")))
            .Ensure(v => v > 0m, Error.Validation("NEG", "must be positive"))
            .Map(v => v.ToString("C"));

        result.ShouldBeSuccess();
        Assert.NotEmpty(result.Value);
    }

    [Fact]
    public void Bind_WithState_OnFailure_ShortCircuits_Missing()
    {
        var error = Error.Failure("ERR", "fail");
        Result<decimal> result = error;
        var bound = ResultSyncExtensions.Bind(result, 5m, (state, v) => Result.Success(v + state));

        bound.ShouldBeFailure();
        Assert.Equal("ERR", bound.Error.Code);
    }

    [Fact]
    public void Ensure_WithValueContextualFactory_OnFailure_ShortCircuits_Missing()
    {
        var error = Error.Failure("ERR", "fail");
        Result<decimal> result = error;
        var ensured = ResultSyncExtensions.Ensure(result, v => v > 0, v => Error.Validation("V", "fail"));

        ensured.ShouldBeFailure();
        Assert.Equal("ERR", ensured.Error.Code);
    }

    [Fact]
    public void Ensure_WithState_OnSuccess_PredicateFalse_Missing()
    {
        var result = Result.Success(5m);
        var ensured = ResultSyncExtensions.Ensure(result, 10m, (min, v) => v > min, Error.Validation("V", "too small"));

        ensured.ShouldBeFailure();
        Assert.Equal("V", ensured.Error.Code);
    }

    [Fact]
    public void Ensure_WithState_OnFailure_ShortCircuits_Missing()
    {
        var error = Error.Failure("ERR", "fail");
        Result<decimal> result = error;
        var ensured = ResultSyncExtensions.Ensure(result, 10m, (min, v) => v > min, Error.Validation("V", "too small"));

        ensured.ShouldBeFailure();
        Assert.Equal("ERR", ensured.Error.Code);
    }

    [Fact]
    public void Ensure_WithState_WithErrorFactory_OnFailure_ShortCircuits_Missing()
    {
        var error = Error.Failure("ERR", "fail");
        Result<decimal> result = error;
        var ensured = ResultSyncExtensions.Ensure(result, 10m, (min, v) => v > min, state => Error.Validation("V", "too small"));

        ensured.ShouldBeFailure();
        Assert.Equal("ERR", ensured.Error.Code);
    }

    [Fact]
    public void Ensure_WithState_WithStateValueFactory_OnFailure_ShortCircuits_Missing()
    {
        var error = Error.Failure("ERR", "fail");
        Result<decimal> result = error;
        var ensured = ResultSyncExtensions.Ensure(result, 10m, (min, v) => v > min, (state, v) => Error.Validation("V", "too small"));

        ensured.ShouldBeFailure();
        Assert.Equal("ERR", ensured.Error.Code);
    }

    [Fact]
    public void Match_WithState_OnFailure_PassesState_Missing()
    {
        var error = Error.Failure("ERR", "fail");
        Result<decimal> result = error;
        var matched = ResultSyncExtensions.Match(result, 5m, (state, v) => state + v, (state, e) => state - 1m);

        Assert.Equal(4m, matched);
    }

    [Fact]
    public void Ensure_WithState_WithStateValueFactory_OnSuccess_PredicateTrue()
    {
        var result = Result.Success(5m);
        var ensured = ResultSyncExtensions.Ensure(result, 1m, (min, v) => v > min, (state, v) => Error.Validation("V", "too small"));

        ensured.ShouldBeSuccess();
        Assert.Equal(5m, ensured.Value);
    }

    // ─── Uninitialized guard tests (B-01 ARB audit fix) ──────────────────────
    // Each monadic method must throw InvalidOperationException for default(Result<T>).
    // TryGetValue and GetValueOrDefault are exempt (BCL *OrDefault / Try* convention).

    [Fact]
    public void Map_OnUninitialized_ThrowsInvalidOperationException()
    {
        var uninitialized = default(Result<decimal>);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResultSyncExtensions.Map(uninitialized, v => v * 2));
        Assert.Contains("Cannot operate on an uninitialized default Result<TValue>", ex.Message);
    }

    [Fact]
    public void Map_WithState_OnUninitialized_ThrowsInvalidOperationException()
    {
        var uninitialized = default(Result<decimal>);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResultSyncExtensions.Map(uninitialized, 2m, (state, v) => v * state));
        Assert.Contains("Cannot operate on an uninitialized default Result<TValue>", ex.Message);
    }

    [Fact]
    public void Bind_OnUninitialized_ThrowsInvalidOperationException()
    {
        var uninitialized = default(Result<Guid>);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResultSyncExtensions.Bind(uninitialized, g => Result.Success(g.ToString())));
        Assert.Contains("Cannot operate on an uninitialized default Result<TValue>", ex.Message);
    }

    [Fact]
    public void Bind_WithState_OnUninitialized_ThrowsInvalidOperationException()
    {
        var uninitialized = default(Result<Guid>);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResultSyncExtensions.Bind(uninitialized, "prefix", (state, g) => Result.Success(state + g.ToString())));
        Assert.Contains("Cannot operate on an uninitialized default Result<TValue>", ex.Message);
    }

    [Fact]
    public void Ensure_WithError_OnUninitialized_ThrowsInvalidOperationException()
    {
        var uninitialized = default(Result<decimal>);
        var error = Error.Failure("V", "invalid");
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResultSyncExtensions.Ensure(uninitialized, v => v > 0, error));
        Assert.Contains("Cannot operate on an uninitialized default Result<TValue>", ex.Message);
    }

    [Fact]
    public void Ensure_WithErrorFactory_OnUninitialized_ThrowsInvalidOperationException()
    {
        var uninitialized = default(Result<decimal>);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResultSyncExtensions.Ensure(uninitialized, v => v > 0, () => Error.Failure("V", "invalid")));
        Assert.Contains("Cannot operate on an uninitialized default Result<TValue>", ex.Message);
    }

    [Fact]
    public void Ensure_WithValueErrorFactory_OnUninitialized_ThrowsInvalidOperationException()
    {
        var uninitialized = default(Result<decimal>);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResultSyncExtensions.Ensure(uninitialized, v => v > 0, v => Error.Failure("V", $"{v} is invalid")));
        Assert.Contains("Cannot operate on an uninitialized default Result<TValue>", ex.Message);
    }

    [Fact]
    public void Ensure_WithState_WithError_OnUninitialized_ThrowsInvalidOperationException()
    {
        var uninitialized = default(Result<decimal>);
        var error = Error.Failure("V", "invalid");
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResultSyncExtensions.Ensure(uninitialized, 0m, (min, v) => v > min, error));
        Assert.Contains("Cannot operate on an uninitialized default Result<TValue>", ex.Message);
    }

    [Fact]
    public void Ensure_WithState_WithStateErrorFactory_OnUninitialized_ThrowsInvalidOperationException()
    {
        var uninitialized = default(Result<decimal>);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResultSyncExtensions.Ensure(uninitialized, 0m, (min, v) => v > min, min => Error.Failure("V", $"must be > {min}")));
        Assert.Contains("Cannot operate on an uninitialized default Result<TValue>", ex.Message);
    }

    [Fact]
    public void Ensure_WithState_WithStateValueErrorFactory_OnUninitialized_ThrowsInvalidOperationException()
    {
        var uninitialized = default(Result<decimal>);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResultSyncExtensions.Ensure(uninitialized, 0m, (min, v) => v > min, (min, v) => Error.Failure("V", $"{v} must be > {min}")));
        Assert.Contains("Cannot operate on an uninitialized default Result<TValue>", ex.Message);
    }

    [Fact]
    public void Match_OnUninitialized_ThrowsInvalidOperationException()
    {
        var uninitialized = default(Result<decimal>);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResultSyncExtensions.Match(uninitialized, v => v.ToString(), e => "error"));
        Assert.Contains("Cannot operate on an uninitialized default Result<TValue>", ex.Message);
    }

    [Fact]
    public void Match_WithState_OnUninitialized_ThrowsInvalidOperationException()
    {
        var uninitialized = default(Result<decimal>);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResultSyncExtensions.Match(uninitialized, "state", (state, v) => state + v.ToString(), (state, e) => state));
        Assert.Contains("Cannot operate on an uninitialized default Result<TValue>", ex.Message);
    }

    [Fact]
    public void TryGetValue_OnUninitialized_ReturnsFalseWithoutThrowing()
    {
        // BCL Try* convention: never throws — returns false for non-Success states
        var uninitialized = default(Result<decimal>);
        var result = ResultSyncExtensions.TryGetValue(uninitialized, out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void GetValueOrDefault_OnUninitialized_ReturnsDefaultWithoutThrowing()
    {
        // BCL *OrDefault convention: never throws — returns fallback for non-Success states
        var uninitialized = default(Result<decimal>);
        const decimal fallback = -1m;
        var result = ResultSyncExtensions.GetValueOrDefault(uninitialized, fallback);

        Assert.Equal(fallback, result);
    }
}



