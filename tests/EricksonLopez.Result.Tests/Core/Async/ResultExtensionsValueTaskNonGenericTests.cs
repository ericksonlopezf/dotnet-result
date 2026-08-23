// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

#pragma warning disable CA2012

public class ResultExtensionsValueTaskNonGenericTests
{
    private static readonly Error SharedError = Error.Failure("E", "M");

    private static ValueTask<Result> S() => new(Result.Success());
    private static ValueTask<Result> F() => new(Result.Failure(SharedError));
    private static async ValueTask<Result> IS() { await Task.Yield(); return Result.Success(); }
    private static async ValueTask<Result> IF() { await Task.Yield(); return Result.Failure(SharedError); }

    [Fact]
    public async Task Bind_WithSyncDelegate_ReturnsExpectedResult()
    {
        (await S().Bind(() => Result.Success())).ShouldBeSuccess();
        (await S().Bind(() => Result.Failure(SharedError))).ShouldBeFailure().Code.Should().Be("E");
        (await F().Bind(() => Result.Success())).ShouldBeFailure().Code.Should().Be("E");

        (await IS().Bind(() => Result.Success())).ShouldBeSuccess();
        (await IS().Bind(() => Result.Failure(SharedError))).ShouldBeFailure().Code.Should().Be("E");
        (await IF().Bind(() => Result.Success())).ShouldBeFailure().Code.Should().Be("E");
    }

    [Fact]
    public async Task Bind_WithState_SyncDelegate_ReturnsExpectedResult()
    {
        (await S().Bind(42, (s) => Result.Success())).ShouldBeSuccess();
        (await IS().Bind(42, (s) => Result.Success())).ShouldBeSuccess();
        (await F().Bind(42, (s) => Result.Success())).ShouldBeFailure();
        (await IF().Bind(42, (s) => Result.Success())).ShouldBeFailure();
    }

    [Fact]
    public async Task Bind_WithAsyncDelegate_ReturnsExpectedResult()
    {
        (await S().Bind(() => new ValueTask<Result>(Result.Success()))).ShouldBeSuccess();
        (await IS().Bind(() => new ValueTask<Result>(Result.Success()))).ShouldBeSuccess();
        (await F().Bind(() => new ValueTask<Result>(Result.Success()))).ShouldBeFailure();
        (await IF().Bind(() => new ValueTask<Result>(Result.Success()))).ShouldBeFailure();
    }

    [Fact]
    public async Task Bind_WithGeneric_SyncDelegateGeneric_ReturnsExpectedResult()
    {
        (await S().Bind(() => Result.Success(42))).ShouldBeSuccess().Should().Be(42);
        (await IS().Bind(() => Result.Success(42))).ShouldBeSuccess().Should().Be(42);
        (await F().Bind(() => Result.Success(42))).ShouldBeFailure();
        (await IF().Bind(() => Result.Success(42))).ShouldBeFailure();
    }

    [Fact]
    public async Task Bind_WithState_Generic_SyncDelegateGeneric_ReturnsExpectedResult()
    {
        (await S().Bind("state", (s) => Result.Success(42))).ShouldBeSuccess().Should().Be(42);
        (await IS().Bind("state", (s) => Result.Success(42))).ShouldBeSuccess().Should().Be(42);
        (await F().Bind("state", (s) => Result.Success(42))).ShouldBeFailure();
        (await IF().Bind("state", (s) => Result.Success(42))).ShouldBeFailure();
    }

    [Fact]
    public async Task Bind_WithGeneric_AsyncDelegateGeneric_ReturnsExpectedResult()
    {
        (await S().Bind(() => new ValueTask<Result<int>>(Result.Success(42)))).ShouldBeSuccess().Should().Be(42);
        (await IS().Bind(() => new ValueTask<Result<int>>(Result.Success(42)))).ShouldBeSuccess().Should().Be(42);
        (await F().Bind(() => new ValueTask<Result<int>>(Result.Success(42)))).ShouldBeFailure();
        (await IF().Bind(() => new ValueTask<Result<int>>(Result.Success(42)))).ShouldBeFailure();
    }

    [Fact]
    public async Task Match_WithTOut_ReturnsExpectedResult()
    {
        Assert.Equal(1, await S().Match(() => 1, e => 0));
        Assert.Equal(1, await IS().Match(() => 1, e => 0));
        Assert.Equal(0, await F().Match(() => 1, e => 0));
        Assert.Equal(0, await IF().Match(() => 1, e => 0));
    }

    [Fact]
    public async Task Match_WithState_TOut_ReturnsExpectedResult()
    {
        Assert.Equal(10, await S().Match(10, s => s, (s, e) => 0));
        Assert.Equal(10, await IS().Match(10, s => s, (s, e) => 0));
        Assert.Equal(0, await F().Match(10, s => s, (s, e) => 0));
        Assert.Equal(0, await IF().Match(10, s => s, (s, e) => 0));
    }

    [Fact]
    public async Task Execute()
    {
        int ok = 0; int fail = 0;
        await S().Execute(() => ok++, e => fail++);
        Assert.Equal(1, ok); Assert.Equal(0, fail);
        ok = 0; fail = 0;
        await IS().Execute(() => ok++, e => fail++);
        Assert.Equal(1, ok); Assert.Equal(0, fail);
        ok = 0; fail = 0;
        await F().Execute(() => ok++, e => fail++);
        Assert.Equal(0, ok); Assert.Equal(1, fail);
        ok = 0; fail = 0;
        await IF().Execute(() => ok++, e => fail++);
        Assert.Equal(0, ok); Assert.Equal(1, fail);
    }

    [Fact]
    public async Task Execute_WithState_ReturnsExpectedResult()
    {
        int ok = 0; int fail = 0;
        await S().Execute(5, s => ok += s, (s, e) => fail += s);
        Assert.Equal(5, ok); Assert.Equal(0, fail);
        ok = 0; fail = 0;
        await IS().Execute(5, s => ok += s, (s, e) => fail += s);
        Assert.Equal(5, ok); Assert.Equal(0, fail);
        ok = 0; fail = 0;
        await F().Execute(5, s => ok += s, (s, e) => fail += s);
        Assert.Equal(0, ok); Assert.Equal(5, fail);
        ok = 0; fail = 0;
        await IF().Execute(5, s => ok += s, (s, e) => fail += s);
        Assert.Equal(0, ok); Assert.Equal(5, fail);
    }

    [Fact]
    public async Task TapOnSuccess_WithAction_ReturnsExpectedResult()
    {
        int count = 0;
        (await S().TapOnSuccess(() => count++)).ShouldBeSuccess();
        Assert.Equal(1, count); count = 0;
        (await IS().TapOnSuccess(() => count++)).ShouldBeSuccess();
        Assert.Equal(1, count); count = 0;
        (await F().TapOnSuccess(() => count++)).ShouldBeFailure();
        Assert.Equal(0, count); count = 0;
        (await IF().TapOnSuccess(() => count++)).ShouldBeFailure();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task TapOnSuccess_WithState_Action_ReturnsExpectedResult()
    {
        int count = 0;
        (await S().TapOnSuccess(5, s => count += s)).ShouldBeSuccess();
        Assert.Equal(5, count); count = 0;
        (await IS().TapOnSuccess(5, s => count += s)).ShouldBeSuccess();
        Assert.Equal(5, count); count = 0;
        (await F().TapOnSuccess(5, s => count += s)).ShouldBeFailure();
        Assert.Equal(0, count); count = 0;
        (await IF().TapOnSuccess(5, s => count += s)).ShouldBeFailure();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task TapOnSuccess_WithFuncValueTask_ReturnsExpectedResult()
    {
        int count = 0;
        (await S().TapOnSuccess(async () => { await Task.Yield(); count++; })).ShouldBeSuccess();
        Assert.Equal(1, count); count = 0;
        (await IS().TapOnSuccess(async () => { await Task.Yield(); count++; })).ShouldBeSuccess();
        Assert.Equal(1, count); count = 0;
        (await F().TapOnSuccess(async () => { await Task.Yield(); count++; })).ShouldBeFailure();
        Assert.Equal(0, count); count = 0;
        (await IF().TapOnSuccess(async () => { await Task.Yield(); count++; })).ShouldBeFailure();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task TapOnFailure_WithAction_ReturnsExpectedResult()
    {
        int count = 0;
        (await F().TapOnFailure(e => count++)).ShouldBeFailure();
        Assert.Equal(1, count); count = 0;
        (await IF().TapOnFailure(e => count++)).ShouldBeFailure();
        Assert.Equal(1, count); count = 0;
        (await S().TapOnFailure(e => count++)).ShouldBeSuccess();
        Assert.Equal(0, count); count = 0;
        (await IS().TapOnFailure(e => count++)).ShouldBeSuccess();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task TapOnFailure_WithState_Action_ReturnsExpectedResult()
    {
        int count = 0;
        (await F().TapOnFailure(5, (s, e) => count += s)).ShouldBeFailure();
        Assert.Equal(5, count); count = 0;
        (await IF().TapOnFailure(5, (s, e) => count += s)).ShouldBeFailure();
        Assert.Equal(5, count); count = 0;
        (await S().TapOnFailure(5, (s, e) => count += s)).ShouldBeSuccess();
        Assert.Equal(0, count); count = 0;
        (await IS().TapOnFailure(5, (s, e) => count += s)).ShouldBeSuccess();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task TapOnFailure_WithFuncValueTask_ReturnsExpectedResult()
    {
        int count = 0;
        (await F().TapOnFailure(async e => { await Task.Yield(); count++; })).ShouldBeFailure();
        Assert.Equal(1, count); count = 0;
        (await IF().TapOnFailure(async e => { await Task.Yield(); count++; })).ShouldBeFailure();
        Assert.Equal(1, count); count = 0;
        (await S().TapOnFailure(async e => { await Task.Yield(); count++; })).ShouldBeSuccess();
        Assert.Equal(0, count); count = 0;
        (await IS().TapOnFailure(async e => { await Task.Yield(); count++; })).ShouldBeSuccess();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Ensure_WithFuncBool_ReturnsExpectedResult()
    {
        var e2 = Error.Failure("2", "2");
        (await S().Ensure(() => true, e2)).ShouldBeSuccess();
        (await IS().Ensure(() => true, e2)).ShouldBeSuccess();
        (await S().Ensure(() => false, e2)).ShouldBeFailure().Code.Should().Be("2");
        (await IS().Ensure(() => false, e2)).ShouldBeFailure().Code.Should().Be("2");
        (await F().Ensure(() => true, e2)).ShouldBeFailure().Code.Should().Be("E");
        (await IF().Ensure(() => true, e2)).ShouldBeFailure().Code.Should().Be("E");
    }

    [Fact]
    public async Task Ensure_WithState_FuncBool_ReturnsExpectedResult()
    {
        var e2 = Error.Failure("2", "2");
        (await S().Ensure(true, s => s, e2)).ShouldBeSuccess();
        (await IS().Ensure(true, s => s, e2)).ShouldBeSuccess();
        (await S().Ensure(false, s => s, e2)).ShouldBeFailure().Code.Should().Be("2");
        (await IS().Ensure(false, s => s, e2)).ShouldBeFailure().Code.Should().Be("2");
        (await F().Ensure(true, s => s, e2)).ShouldBeFailure().Code.Should().Be("E");
        (await IF().Ensure(true, s => s, e2)).ShouldBeFailure().Code.Should().Be("E");
    }

    [Fact]
    public async Task MapError_WithFuncError_ReturnsExpectedResult()
    {
        var e2 = Error.Failure("2", "2");
        (await F().MapError(e => e2)).ShouldBeFailure().Code.Should().Be("2");
        (await IF().MapError(e => e2)).ShouldBeFailure().Code.Should().Be("2");
        (await S().MapError(e => e2)).ShouldBeSuccess();
        (await IS().MapError(e => e2)).ShouldBeSuccess();
    }

    [Fact]
    public async Task MapError_WithState_FuncError_ReturnsExpectedResult()
    {
        var e2 = Error.Failure("2", "2");
        (await F().MapError(e2, (s, e) => s)).ShouldBeFailure().Code.Should().Be("2");
        (await IF().MapError(e2, (s, e) => s)).ShouldBeFailure().Code.Should().Be("2");
        (await S().MapError(e2, (s, e) => s)).ShouldBeSuccess();
        (await IS().MapError(e2, (s, e) => s)).ShouldBeSuccess();
    }

    [Fact]
    public async Task Inspect_WithAction_ReturnsExpectedResult()
    {
        int count = 0;
        await S().Inspect(r => count++);
        Assert.Equal(1, count); count = 0;
        await IS().Inspect(r => count++);
        Assert.Equal(1, count); count = 0;
        await F().Inspect(r => count++);
        Assert.Equal(1, count); count = 0;
        await IF().Inspect(r => count++);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Inspect_WithState_Action_ReturnsExpectedResult()
    {
        int count = 0;
        await S().Inspect(5, (s, r) => count += s);
        Assert.Equal(5, count); count = 0;
        await IS().Inspect(5, (s, r) => count += s);
        Assert.Equal(5, count); count = 0;
        await F().Inspect(5, (s, r) => count += s);
        Assert.Equal(5, count); count = 0;
        await IF().Inspect(5, (s, r) => count += s);
        Assert.Equal(5, count);
    }

    [Fact]
    public async Task Map_WithGeneric_ReturnsExpectedResult()
    {
        (await S().Map(() => 42)).ShouldBeSuccess().Should().Be(42);
        (await IS().Map(() => 42)).ShouldBeSuccess().Should().Be(42);
        (await F().Map(() => 42)).ShouldBeFailure();
        (await IF().Map(() => 42)).ShouldBeFailure();
    }

    [Fact]
    public async Task Map_WithState_Generic_ReturnsExpectedResult()
    {
        (await S().Map(42, s => s)).ShouldBeSuccess().Should().Be(42);
        (await IS().Map(42, s => s)).ShouldBeSuccess().Should().Be(42);
        (await F().Map(42, s => s)).ShouldBeFailure();
        (await IF().Map(42, s => s)).ShouldBeFailure();
    }

    [Fact]
    public async Task Recover_WithSyncDelegate_ReturnsExpectedResult()
    {
        (await F().Recover(e => Result.Success())).ShouldBeSuccess();
        (await IF().Recover(e => Result.Success())).ShouldBeSuccess();
        (await F().Recover(e => Result.Failure(Error.Failure("2", "2")))).ShouldBeFailure().Code.Should().Be("2");
        (await IF().Recover(e => Result.Failure(Error.Failure("2", "2")))).ShouldBeFailure().Code.Should().Be("2");

        (await S().Recover(e => Result.Failure(Error.Failure("2", "2")))).ShouldBeSuccess();
        (await IS().Recover(e => Result.Failure(Error.Failure("2", "2")))).ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_WithState_SyncDelegate_ReturnsExpectedResult()
    {
        (await F().Recover(42, (s, e) => Result.Success())).ShouldBeSuccess();
        (await IF().Recover(42, (s, e) => Result.Success())).ShouldBeSuccess();
        (await S().Recover(42, (s, e) => Result.Failure(Error.Failure("2", "2")))).ShouldBeSuccess();
        (await IS().Recover(42, (s, e) => Result.Failure(Error.Failure("2", "2")))).ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_WithAsyncDelegate_ReturnsExpectedResult()
    {
        (await F().Recover(e => new ValueTask<Result>(Result.Success()))).ShouldBeSuccess();
        (await IF().Recover(e => new ValueTask<Result>(Result.Success()))).ShouldBeSuccess();
        (await S().Recover(e => new ValueTask<Result>(Result.Failure(Error.Failure("2", "2"))))).ShouldBeSuccess();
        (await IS().Recover(e => new ValueTask<Result>(Result.Failure(Error.Failure("2", "2"))))).ShouldBeSuccess();
    }
}




