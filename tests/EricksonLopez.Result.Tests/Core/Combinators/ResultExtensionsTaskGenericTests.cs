// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Operators;

public class ResultExtensionsTaskGenericTests
{
    private static readonly Error SharedError = Error.Failure("E", "M");

    private static Task<Result<int>> S() => Task.FromResult(Result.Success(42));
    private static Task<Result<int>> F() => Task.FromResult(Result.Failure<int>(SharedError));
    private static async Task<Result<int>> IS() { await Task.Yield(); return Result.Success(42); }
    private static async Task<Result<int>> IF() { await Task.Yield(); return Result.Failure<int>(SharedError); }

    [Fact]
    public async Task Bind_GenericResult_WithFuncResultTNext()
    {
        (await S().Bind(v => Result.Success(v * 2))).ShouldBeSuccess().Should().Be(84);
        (await S().Bind(v => Result.Failure<int>(SharedError))).ShouldBeFailure().Code.Should().Be("E");
        (await F().Bind(v => Result.Success(v * 2))).ShouldBeFailure().Code.Should().Be("E");

        (await IS().Bind(v => Result.Success(v * 2))).ShouldBeSuccess().Should().Be(84);
        (await IS().Bind(v => Result.Failure<int>(SharedError))).ShouldBeFailure().Code.Should().Be("E");
        (await IF().Bind(v => Result.Success(v * 2))).ShouldBeFailure().Code.Should().Be("E");
    }

    [Fact]
    public async Task Bind_GenericResult_WithTState_FuncResultTNext()
    {
        (await S().Bind(10, (s, v) => Result.Success(v + s))).ShouldBeSuccess().Should().Be(52);
        (await IS().Bind(10, (s, v) => Result.Success(v + s))).ShouldBeSuccess().Should().Be(52);
        (await F().Bind(10, (s, v) => Result.Success(v + s))).ShouldBeFailure();
        (await IF().Bind(10, (s, v) => Result.Success(v + s))).ShouldBeFailure();
    }

    [Fact]
    public async Task Bind_GenericResult_WithFuncTaskResultTNext()
    {
        (await S().Bind(v => Task.FromResult(Result.Success(v * 2)))).ShouldBeSuccess().Should().Be(84);
        (await IS().Bind(v => Task.FromResult(Result.Success(v * 2)))).ShouldBeSuccess().Should().Be(84);
        (await F().Bind(v => Task.FromResult(Result.Success(v * 2)))).ShouldBeFailure();
        (await IF().Bind(v => Task.FromResult(Result.Success(v * 2)))).ShouldBeFailure();
    }

    [Fact]
    public async Task Bind_GenericResult_WithFuncTaskResult()
    {
        (await S().Bind(v => Task.FromResult(Result.Success()))).ShouldBeSuccess();
        (await IS().Bind(v => Task.FromResult(Result.Success()))).ShouldBeSuccess();
        (await F().Bind(v => Task.FromResult(Result.Success()))).ShouldBeFailure();
        (await IF().Bind(v => Task.FromResult(Result.Success()))).ShouldBeFailure();
    }

    [Fact]
    public async Task Bind_GenericResult_WithFuncResult()
    {
        (await S().Bind(v => Result.Success())).ShouldBeSuccess();
        (await IS().Bind(v => Result.Success())).ShouldBeSuccess();
        (await F().Bind(v => Result.Success())).ShouldBeFailure();
        (await IF().Bind(v => Result.Success())).ShouldBeFailure();
    }

    [Fact]
    public async Task Bind_GenericResult_WithTState_FuncResult()
    {
        (await S().Bind(10, (s, v) => Result.Success())).ShouldBeSuccess();
        (await IS().Bind(10, (s, v) => Result.Success())).ShouldBeSuccess();
        (await F().Bind(10, (s, v) => Result.Success())).ShouldBeFailure();
        (await IF().Bind(10, (s, v) => Result.Success())).ShouldBeFailure();
    }

    [Fact]
    public async Task TapOnSuccess_GenericResult_WithAction()
    {
        int count = 0;
        (await S().TapOnSuccess(v => count += v)).ShouldBeSuccess();
        Assert.Equal(42, count); count = 0;
        (await IS().TapOnSuccess(v => count += v)).ShouldBeSuccess();
        Assert.Equal(42, count); count = 0;
        (await F().TapOnSuccess(v => count += v)).ShouldBeFailure();
        Assert.Equal(0, count); count = 0;
        (await IF().TapOnSuccess(v => count += v)).ShouldBeFailure();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task TapOnSuccess_GenericResult_WithTState_Action()
    {
        int count = 0;
        (await S().TapOnSuccess(5, (s, v) => count += s + v)).ShouldBeSuccess();
        Assert.Equal(47, count); count = 0;
        (await IS().TapOnSuccess(5, (s, v) => count += s + v)).ShouldBeSuccess();
        Assert.Equal(47, count); count = 0;
        (await F().TapOnSuccess(5, (s, v) => count += s + v)).ShouldBeFailure();
        Assert.Equal(0, count); count = 0;
        (await IF().TapOnSuccess(5, (s, v) => count += s + v)).ShouldBeFailure();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task TapOnSuccess_GenericResult_WithFuncTask()
    {
        int count = 0;
        (await S().TapOnSuccess(async v => { await Task.Yield(); count += v; })).ShouldBeSuccess();
        Assert.Equal(42, count); count = 0;
        (await IS().TapOnSuccess(async v => { await Task.Yield(); count += v; })).ShouldBeSuccess();
        Assert.Equal(42, count); count = 0;
        (await F().TapOnSuccess(async v => { await Task.Yield(); count += v; })).ShouldBeFailure();
        Assert.Equal(0, count); count = 0;
        (await IF().TapOnSuccess(async v => { await Task.Yield(); count += v; })).ShouldBeFailure();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task TapOnFailure_GenericResult_WithAction()
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
    public async Task TapOnFailure_GenericResult_WithTState_Action()
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
    public async Task TapOnFailure_GenericResult_WithFuncTask()
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
    public async Task Ensure_GenericResult_WithFuncBool()
    {
        var e2 = Error.Failure("2", "2");
        (await S().Ensure(v => v == 42, e2)).ShouldBeSuccess();
        (await IS().Ensure(v => v == 42, e2)).ShouldBeSuccess();
        (await S().Ensure(v => v != 42, e2)).ShouldBeFailure().Code.Should().Be("2");
        (await IS().Ensure(v => v != 42, e2)).ShouldBeFailure().Code.Should().Be("2");
        (await F().Ensure(v => v == 42, e2)).ShouldBeFailure().Code.Should().Be("E");
        (await IF().Ensure(v => v == 42, e2)).ShouldBeFailure().Code.Should().Be("E");
    }

    [Fact]
    public async Task Ensure_GenericResult_WithTState_FuncBool()
    {
        var e2 = Error.Failure("2", "2");
        (await S().Ensure(42, (s, v) => s == v, e2)).ShouldBeSuccess();
        (await IS().Ensure(42, (s, v) => s == v, e2)).ShouldBeSuccess();
        (await S().Ensure(42, (s, v) => s != v, e2)).ShouldBeFailure().Code.Should().Be("2");
        (await IS().Ensure(42, (s, v) => s != v, e2)).ShouldBeFailure().Code.Should().Be("2");
        (await F().Ensure(42, (s, v) => s == v, e2)).ShouldBeFailure().Code.Should().Be("E");
        (await IF().Ensure(42, (s, v) => s == v, e2)).ShouldBeFailure().Code.Should().Be("E");
    }

    [Fact]
    public async Task MapError_GenericResult_WithFuncError()
    {
        var e2 = Error.Failure("2", "2");
        (await F().MapError(e => e2)).ShouldBeFailure().Code.Should().Be("2");
        (await IF().MapError(e => e2)).ShouldBeFailure().Code.Should().Be("2");
        (await S().MapError(e => e2)).ShouldBeSuccess();
        (await IS().MapError(e => e2)).ShouldBeSuccess();
    }

    [Fact]
    public async Task MapError_GenericResult_WithTState_FuncError()
    {
        var e2 = Error.Failure("2", "2");
        (await F().MapError(e2, (s, e) => s)).ShouldBeFailure().Code.Should().Be("2");
        (await IF().MapError(e2, (s, e) => s)).ShouldBeFailure().Code.Should().Be("2");
        (await S().MapError(e2, (s, e) => s)).ShouldBeSuccess();
        (await IS().MapError(e2, (s, e) => s)).ShouldBeSuccess();
    }

    [Fact]
    public async Task Map_GenericResult_WithTNext()
    {
        (await S().Map(v => v.ToString())).ShouldBeSuccess().Should().Be("42");
        (await IS().Map(v => v.ToString())).ShouldBeSuccess().Should().Be("42");
        (await F().Map(v => v.ToString())).ShouldBeFailure();
        (await IF().Map(v => v.ToString())).ShouldBeFailure();
    }

    [Fact]
    public async Task Map_GenericResult_WithTState_TNext()
    {
        (await S().Map(10, (s, v) => (s + v).ToString())).ShouldBeSuccess().Should().Be("52");
        (await IS().Map(10, (s, v) => (s + v).ToString())).ShouldBeSuccess().Should().Be("52");
        (await F().Map(10, (s, v) => (s + v).ToString())).ShouldBeFailure();
        (await IF().Map(10, (s, v) => (s + v).ToString())).ShouldBeFailure();
    }

    [Fact]
    public async Task Recover_GenericResult_WithFuncResult()
    {
        (await F().Recover(e => Result.Success(100))).ShouldBeSuccess().Should().Be(100);
        (await IF().Recover(e => Result.Success(100))).ShouldBeSuccess().Should().Be(100);
        (await F().Recover(e => Result.Failure<int>(Error.Failure("2", "2")))).ShouldBeFailure().Code.Should().Be("2");
        (await IF().Recover(e => Result.Failure<int>(Error.Failure("2", "2")))).ShouldBeFailure().Code.Should().Be("2");

        (await S().Recover(e => Result.Failure<int>(Error.Failure("2", "2")))).ShouldBeSuccess().Should().Be(42);
        (await IS().Recover(e => Result.Failure<int>(Error.Failure("2", "2")))).ShouldBeSuccess().Should().Be(42);
    }

    [Fact]
    public async Task Recover_GenericResult_WithTState_FuncResult()
    {
        (await F().Recover(100, (s, e) => Result.Success(s))).ShouldBeSuccess().Should().Be(100);
        (await IF().Recover(100, (s, e) => Result.Success(s))).ShouldBeSuccess().Should().Be(100);
        (await S().Recover(100, (s, e) => Result.Failure<int>(Error.Failure("2", "2")))).ShouldBeSuccess().Should().Be(42);
        (await IS().Recover(100, (s, e) => Result.Failure<int>(Error.Failure("2", "2")))).ShouldBeSuccess().Should().Be(42);
    }

    [Fact]
    public async Task Recover_GenericResult_WithFuncTaskResult()
    {
        (await F().Recover(e => Task.FromResult(Result.Success(100)))).ShouldBeSuccess().Should().Be(100);
        (await IF().Recover(e => Task.FromResult(Result.Success(100)))).ShouldBeSuccess().Should().Be(100);
        (await S().Recover(e => Task.FromResult(Result.Failure<int>(Error.Failure("2", "2"))))).ShouldBeSuccess().Should().Be(42);
        (await IS().Recover(e => Task.FromResult(Result.Failure<int>(Error.Failure("2", "2"))))).ShouldBeSuccess().Should().Be(42);
    }
}




