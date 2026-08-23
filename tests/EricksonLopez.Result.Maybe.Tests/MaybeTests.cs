// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Maybe;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Maybe.Tests;

public class MaybeTests
{
    [Fact]
    public void None_HasNoValue_AndThrowsOnValueAccess()
    {
        var maybe = Maybe<int>.None;

        Assert.False(maybe.HasValue);
        Assert.True(maybe.HasNoValue);

        var ex = Assert.Throws<InvalidOperationException>(() => _ = maybe.Value);
        Assert.Equal("Cannot access Value when Maybe has no value.", ex.Message);
    }

    [Fact]
    public void From_WithValue_HasValue()
    {
        var maybe = Maybe<string>.From("hello");

        Assert.True(maybe.HasValue);
        Assert.False(maybe.HasNoValue);
        Assert.Equal("hello", maybe.Value);
    }

    [Fact]
    public void From_WithNull_ReturnsNone()
    {
        string? val = null;
        var maybe = Maybe<string>.From(val);

        Assert.False(maybe.HasValue);
        Assert.True(maybe.HasNoValue);
        Assert.Equal(Maybe<string>.None, maybe);
    }

    [Fact]
    public void TryGetValue_WhenPresent_ReturnsTrueAndSetsOutParam()
    {
        var maybe = Maybe<int>.From(42);

        Assert.True(maybe.TryGetValue(out var val));
        Assert.Equal(42, val);
    }

    [Fact]
    public void TryGetValue_WhenNone_ReturnsFalseAndSetsDefault()
    {
        var maybe = Maybe<int>.None;

        Assert.False(maybe.TryGetValue(out var val));
        Assert.Equal(0, val);
    }

    [Fact]
    public void GetValueOrDefault_ReturnsCorrectValue()
    {
        Assert.Equal(42, Maybe<int>.From(42).GetValueOrDefault(10));
        Assert.Equal(10, Maybe<int>.None.GetValueOrDefault(10));
    }

    [Fact]
    public void GetValueOrFallback_WhenPresent_ReturnsValueWithoutInvokingFallback()
    {
        bool invoked = false;
        var maybe = Maybe<string>.From("found");

        var result = maybe.GetValueOrFallback(() =>
        {
            invoked = true;
            return "fallback";
        });

        Assert.False(invoked);
        Assert.Equal("found", result);
    }

    [Fact]
    public void GetValueOrFallback_WhenNone_InvokesFallbackAndReturnsResult()
    {
        bool invoked = false;
        var maybe = Maybe<string>.None;

        var result = maybe.GetValueOrFallback(() =>
        {
            invoked = true;
            return "fallback";
        });

        Assert.True(invoked);
        Assert.Equal("fallback", result);
    }

    [Fact]
    public void GetValueOrFallback_WithNullFallback_ThrowsArgumentNullException()
    {
        var maybe = Maybe<string>.From("test");
        Assert.Throws<ArgumentNullException>("fallback", () => maybe.GetValueOrFallback(null!));
    }

    [Fact]
    public void Map_WhenPresent_TransformsValue()
    {
        var mapped = Maybe<int>.From(5).Map(x => x * 2);

        Assert.True(mapped.HasValue);
        Assert.Equal(10, mapped.Value);
    }

    [Fact]
    public void Map_WhenNone_ReturnsNoneWithoutInvokingMapper()
    {
        bool invoked = false;
        var noneMapped = Maybe<int>.None.Map(x =>
        {
            invoked = true;
            return x * 2;
        });

        Assert.False(invoked);
        Assert.False(noneMapped.HasValue);
    }

    [Fact]
    public void Map_WithNullMapper_ThrowsArgumentNullException()
    {
        var maybe = Maybe<int>.From(5);
        Assert.Throws<ArgumentNullException>("mapper", () => maybe.Map<string>(null!));
    }

    [Fact]
    public void Map_WithState_WhenPresent_TransformsValue()
    {
        var mapped = Maybe<int>.From(5).Map(10, (s, x) => x + s);

        Assert.True(mapped.HasValue);
        Assert.Equal(15, mapped.Value);
    }

    [Fact]
    public void Map_WithState_WhenNone_ReturnsNoneWithoutInvokingMapper()
    {
        bool invoked = false;
        var noneMapped = Maybe<int>.None.Map(10, (s, x) =>
        {
            invoked = true;
            return x + s;
        });

        Assert.False(invoked);
        Assert.False(noneMapped.HasValue);
    }

    [Fact]
    public void Map_WithState_WithNullMapper_ThrowsArgumentNullException()
    {
        var maybe = Maybe<int>.From(5);
        Assert.Throws<ArgumentNullException>("mapper", () => maybe.Map<int, string>(10, null!));
    }

    [Fact]
    public void Bind_WhenPresent_ChainsOperation()
    {
        var bound = Maybe<int>.From(5).Bind(x => Maybe<string>.From(x.ToString()));

        Assert.True(bound.HasValue);
        Assert.Equal("5", bound.Value);
    }

    [Fact]
    public void Bind_WhenNone_ReturnsNoneWithoutInvokingBind()
    {
        bool invoked = false;
        var noneBound = Maybe<int>.None.Bind(x =>
        {
            invoked = true;
            return Maybe<string>.From(x.ToString());
        });

        Assert.False(invoked);
        Assert.False(noneBound.HasValue);
    }

    [Fact]
    public void Bind_WithNullBindFunc_ThrowsArgumentNullException()
    {
        var maybe = Maybe<int>.From(5);
        Assert.Throws<ArgumentNullException>("bind", () => maybe.Bind<string>(null!));
    }

    [Fact]
    public void Bind_WithState_WhenPresent_ChainsOperation()
    {
        var bound = Maybe<int>.From(5).Bind("prefix:", (s, x) => Maybe<string>.From($"{s}{x}"));

        Assert.True(bound.HasValue);
        Assert.Equal("prefix:5", bound.Value);
    }

    [Fact]
    public void Bind_WithState_WhenNone_ReturnsNoneWithoutInvokingBind()
    {
        bool invoked = false;
        var noneBound = Maybe<int>.None.Bind("prefix:", (s, x) =>
        {
            invoked = true;
            return Maybe<string>.From($"{s}{x}");
        });

        Assert.False(invoked);
        Assert.False(noneBound.HasValue);
    }

    [Fact]
    public void Bind_WithState_WithNullBindFunc_ThrowsArgumentNullException()
    {
        var maybe = Maybe<int>.From(5);
        Assert.Throws<ArgumentNullException>("bind", () => maybe.Bind<string, int>("s", null!));
    }

    [Fact]
    public void Match_WhenPresent_ExecutesOnValue()
    {
        var res = Maybe<int>.From(42).Match(v => $"Value: {v}", () => "None");
        Assert.Equal("Value: 42", res);
    }

    [Fact]
    public void Match_WhenNone_ExecutesOnNone()
    {
        var res = Maybe<int>.None.Match(v => $"Value: {v}", () => "None");
        Assert.Equal("None", res);
    }

    [Fact]
    public void Match_WithNullDelegates_ThrowsArgumentNullException()
    {
        var maybe = Maybe<int>.From(42);

        Assert.Throws<ArgumentNullException>("onValue", () => maybe.Match<string>(null!, () => "None"));
        Assert.Throws<ArgumentNullException>("onNone", () => maybe.Match<string>(v => v.ToString(), null!));
    }

    [Fact]
    public void Ensure_WhenPredicateTrue_KeepsValue()
    {
        var kept = Maybe<int>.From(42).Ensure(x => x > 40);

        Assert.True(kept.HasValue);
        Assert.Equal(42, kept.Value);
    }

    [Fact]
    public void Ensure_WhenPredicateFalse_ReturnsNone()
    {
        var filtered = Maybe<int>.From(42).Ensure(x => x > 50);

        Assert.False(filtered.HasValue);
        Assert.True(filtered.HasNoValue);
    }

    [Fact]
    public void Ensure_WhenNone_ReturnsNoneWithoutInvokingPredicate()
    {
        bool invoked = false;
        var result = Maybe<int>.None.Ensure(x =>
        {
            invoked = true;
            return true;
        });

        Assert.False(invoked);
        Assert.False(result.HasValue);
    }

    [Fact]
    public void Ensure_WithNullPredicate_ThrowsArgumentNullException()
    {
        var maybe = Maybe<int>.From(42);
        Assert.Throws<ArgumentNullException>("predicate", () => maybe.Ensure(null!));
    }

    [Fact]
    public void ToResult_WithError_WhenPresent_ReturnsSuccess()
    {
        var notFound = Error.NotFound("Item.NotFound", "Item not found.");
        var res = Maybe<string>.From("data").ToResult(notFound);

        res.ShouldBeSuccess();
        Assert.Equal("data", res.Value);
    }

    [Fact]
    public void ToResult_WithError_WhenNone_ReturnsFailure()
    {
        var notFound = Error.NotFound("Item.NotFound", "Item not found.");
        var res = Maybe<string>.None.ToResult(notFound);

        res.ShouldBeFailure();
        Assert.Equal(notFound.Code, res.Error.Code);
    }

    [Fact]
    public void ToResult_WithError_WithNullError_ThrowsArgumentNullException()
    {
        var maybe = Maybe<string>.From("data");
        Assert.Throws<ArgumentNullException>("notFoundError", () => maybe.ToResult((Error)null!));
    }

    [Fact]
    public void ToResult_WithErrorFactory_WhenPresent_ReturnsSuccessWithoutInvokingFactory()
    {
        bool invoked = false;
        var res = Maybe<string>.From("data").ToResult(() =>
        {
            invoked = true;
            return Error.NotFound("Code", "Msg");
        });

        Assert.False(invoked);
        res.ShouldBeSuccess();
        Assert.Equal("data", res.Value);
    }

    [Fact]
    public void ToResult_WithErrorFactory_WhenNone_InvokesFactoryAndReturnsFailure()
    {
        bool invoked = false;
        var res = Maybe<string>.None.ToResult(() =>
        {
            invoked = true;
            return Error.NotFound("Item.NotFound", "Item not found");
        });

        Assert.True(invoked);
        res.ShouldBeFailure();
        Assert.Equal("Item.NotFound", res.Error.Code);
    }

    [Fact]
    public void ToResult_WithErrorFactory_WithNullFactory_ThrowsArgumentNullException()
    {
        var maybe = Maybe<string>.From("data");
        Assert.Throws<ArgumentNullException>("errorFactory", () => maybe.ToResult((Func<Error>)null!));
    }

    [Fact]
    public void ImplicitOperator_CreatesExpectedMaybe()
    {
        Maybe<int> someInt = 42;
        Assert.True(someInt.HasValue);
        Assert.Equal(42, someInt.Value);

        string? nullStr = null;
        Maybe<string> noneStr = nullStr;
        Assert.False(noneStr.HasValue);
    }

    [Fact]
    public void Equality_AndOperators_WorkCorrectly()
    {
        var a1 = Maybe<int>.From(10);
        var a2 = Maybe<int>.From(10);
        var b = Maybe<int>.From(20);
        var n1 = Maybe<int>.None;
        var n2 = Maybe<int>.None;

        Assert.True(a1.Equals(a2));
        Assert.True(a1 == a2);
        Assert.False(a1 != a2);
        Assert.True(a1.Equals((object)a2));

        Assert.False(a1.Equals(b));
        Assert.False(a1 == b);
        Assert.True(a1 != b);

        Assert.True(n1.Equals(n2));
        Assert.True(n1 == n2);
        Assert.False(n1 != n2);

        Assert.False(a1.Equals(n1));
        Assert.False(a1 == n1);
        Assert.True(a1 != n1);

        Assert.False(n1.Equals(a1));
        Assert.False(n1 == a1);
        Assert.True(n1 != a1);

        Assert.False(a1.Equals((object?)null));
        Assert.False(a1.Equals("not-a-maybe"));
        Assert.False(n1.Equals((object?)null));
    }

    [Fact]
    public void GetHashCode_ReturnsConsistentHashCode()
    {
        var a1 = Maybe<int>.From(10);
        var a2 = Maybe<int>.From(10);
        var b = Maybe<int>.From(20);
        var n = Maybe<int>.None;

        Assert.Equal(a1.GetHashCode(), a2.GetHashCode());
        Assert.NotEqual(a1.GetHashCode(), b.GetHashCode());
        Assert.Equal(0, n.GetHashCode());
        Assert.Equal(HashCode.Combine(true, 10.GetHashCode()), a1.GetHashCode());
        Assert.NotEqual(Maybe<int>.From(0).GetHashCode(), n.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsExpectedRepresentation()
    {
        Assert.Equal("Some(42)", Maybe<int>.From(42).ToString());
        Assert.Equal("None", Maybe<int>.None.ToString());
    }

    // ─── MaybeExtensions Tests ──────────────────────────────────────────────────

    [Fact]
    public async Task MaybeExtensions_Map_WhenPresent_TransformsValue()
    {
        var task = Task.FromResult(Maybe<int>.From(5));
        var mapped = await task.Map(x => x * 3);

        Assert.True(mapped.HasValue);
        Assert.Equal(15, mapped.Value);
    }

    [Fact]
    public async Task MaybeExtensions_Map_WhenNone_ReturnsNone()
    {
        var task = Task.FromResult(Maybe<int>.None);
        var mapped = await task.Map(x => x * 3);

        Assert.False(mapped.HasValue);
    }

    [Fact]
    public async Task MaybeExtensions_Map_WithDelayedTask_WorksCorrectly()
    {
        var task = Task.Run(async () =>
        {
            await Task.Yield();
            return Maybe<int>.From(7);
        });

        var mapped = await task.Map(x => x + 3);
        Assert.Equal(10, mapped.Value);
    }

    [Fact]
    public async Task MaybeExtensions_Map_WithNullArguments_ThrowsArgumentNullException_Eagerly()
    {
        Task<Maybe<int>> nullTask = null!;
        await Assert.ThrowsAsync<ArgumentNullException>("maybeTask", () => nullTask.Map(x => x * 2));

        var faultedTask = Task.FromException<Maybe<int>>(new InvalidOperationException("Must not be awaited"));
        await Assert.ThrowsAsync<ArgumentNullException>("mapper", () => faultedTask.Map<int, int>(null!));
    }

    [Fact]
    public async Task MaybeExtensions_Map_WithCanceledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var task = Task.FromResult(Maybe<int>.From(5));
        await Assert.ThrowsAsync<OperationCanceledException>(() => task.Map(x => x * 2, cts.Token));
    }

    [Fact]
    public async Task MaybeExtensions_Bind_WhenPresent_ChainsTask()
    {
        var task = Task.FromResult(Maybe<int>.From(5));
        var bound = await task.Bind(x => Task.FromResult(Maybe<string>.From($"val:{x}")));

        Assert.True(bound.HasValue);
        Assert.Equal("val:5", bound.Value);
    }

    [Fact]
    public async Task MaybeExtensions_Bind_WhenNone_ReturnsNoneWithoutInvokingBind()
    {
        bool invoked = false;
        var task = Task.FromResult(Maybe<int>.None);
        var bound = await task.Bind(x =>
        {
            invoked = true;
            return Task.FromResult(Maybe<string>.From($"val:{x}"));
        });

        Assert.False(invoked);
        Assert.False(bound.HasValue);
    }

    [Fact]
    public async Task MaybeExtensions_Bind_WithDelayedTask_WorksCorrectly()
    {
        var task = Task.Run(async () =>
        {
            await Task.Yield();
            return Maybe<int>.From(8);
        });

        var bound = await task.Bind(x => Task.Run(async () =>
        {
            await Task.Yield();
            return Maybe<int>.From(x * 2);
        }));

        Assert.Equal(16, bound.Value);
    }

    [Fact]
    public async Task MaybeExtensions_Bind_WithNullArguments_ThrowsArgumentNullException_Eagerly()
    {
        Task<Maybe<int>> nullTask = null!;
        await Assert.ThrowsAsync<ArgumentNullException>("maybeTask", () => nullTask.Bind(x => Task.FromResult(Maybe<int>.From(x))));

        var faultedTask = Task.FromException<Maybe<int>>(new InvalidOperationException("Must not be awaited"));
        await Assert.ThrowsAsync<ArgumentNullException>("bind", () => faultedTask.Bind<int, int>(null!));
    }

    [Fact]
    public async Task MaybeExtensions_Bind_WithCanceledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var task = Task.FromResult(Maybe<int>.From(5));
        await Assert.ThrowsAsync<OperationCanceledException>(() => task.Bind(x => Task.FromResult(Maybe<int>.From(x)), cts.Token));
    }

    [Fact]
    public async Task MaybeExtensions_ToResult_WhenPresent_ReturnsSuccessResult()
    {
        var notFound = Error.NotFound("Not.Found", "Not found");
        var task = Task.FromResult(Maybe<int>.From(5));

        var result = await task.ToResult(notFound);

        result.ShouldBeSuccess();
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public async Task MaybeExtensions_ToResult_WhenNone_ReturnsFailureResult()
    {
        var notFound = Error.NotFound("Not.Found", "Not found");
        var task = Task.FromResult(Maybe<int>.None);

        var result = await task.ToResult(notFound);

        result.ShouldBeFailure();
        Assert.Equal(notFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task MaybeExtensions_ToResult_WithDelayedTask_WorksCorrectly()
    {
        var notFound = Error.NotFound("Not.Found", "Not found");
        var task = Task.Run(async () =>
        {
            await Task.Yield();
            return Maybe<string>.From("async-val");
        });

        var result = await task.ToResult(notFound);

        result.ShouldBeSuccess();
        Assert.Equal("async-val", result.Value);
    }

    [Fact]
    public async Task MaybeExtensions_ToResult_WithNullArguments_ThrowsArgumentNullException_Eagerly()
    {
        Task<Maybe<int>> nullTask = null!;
        var notFound = Error.NotFound("Not.Found", "Not found");

        await Assert.ThrowsAsync<ArgumentNullException>("maybeTask", () => nullTask.ToResult(notFound));

        var faultedTask = Task.FromException<Maybe<int>>(new InvalidOperationException("Must not be awaited"));
        await Assert.ThrowsAsync<ArgumentNullException>("notFoundError", () => faultedTask.ToResult(null!));
    }

    [Fact]
    public async Task MaybeExtensions_ToResult_WithCanceledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var notFound = Error.NotFound("Not.Found", "Not found");
        var task = Task.FromResult(Maybe<int>.From(5));

        await Assert.ThrowsAsync<OperationCanceledException>(() => task.ToResult(notFound, cts.Token));
    }
}

