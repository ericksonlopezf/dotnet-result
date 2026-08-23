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

public class ResultExtensionsMatchExecuteInspectTests
{
    private static readonly Error SharedError = Error.Failure("E", "M");

    private static ValueTask<Result<int>> S() => new(Result.Success(42));
    private static ValueTask<Result<int>> F() => new(Result.Failure<int>(SharedError));
    private static async ValueTask<Result<int>> IS() { await Task.Yield(); return Result.Success(42); }
    private static async ValueTask<Result<int>> IF() { await Task.Yield(); return Result.Failure<int>(SharedError); }

    [Fact]
    public async Task Match_TState_T_TOut()
    {
        Assert.Equal(52, await S().Match(10, (s, v) => v + s, (s, e) => s));
        Assert.Equal(52, await IS().Match(10, (s, v) => v + s, (s, e) => s));
        Assert.Equal(10, await F().Match(10, (s, v) => v + s, (s, e) => s));
        Assert.Equal(10, await IF().Match(10, (s, v) => v + s, (s, e) => s));
    }

    [Fact]
    public async Task Execute_TState_T()
    {
        int ok = 0; int fail = 0;
        await S().Execute(10, (s, v) => ok += s + v, (s, e) => fail += s);
        Assert.Equal(52, ok); Assert.Equal(0, fail);
        ok = 0; fail = 0;
        await IS().Execute(10, (s, v) => ok += s + v, (s, e) => fail += s);
        Assert.Equal(52, ok); Assert.Equal(0, fail);
        ok = 0; fail = 0;
        await F().Execute(10, (s, v) => ok += s + v, (s, e) => fail += s);
        Assert.Equal(0, ok); Assert.Equal(10, fail);
        ok = 0; fail = 0;
        await IF().Execute(10, (s, v) => ok += s + v, (s, e) => fail += s);
        Assert.Equal(0, ok); Assert.Equal(10, fail);
    }

    [Fact]
    public async Task Inspect_ActionResultOfT()
    {
        int count = 0;
        await S().Inspect(r => { if (r.IsSuccess) count += r.Value; });
        Assert.Equal(42, count); count = 0;
        await IS().Inspect(r => { if (r.IsSuccess) count += r.Value; });
        Assert.Equal(42, count); count = 0;

        await F().Inspect(r => { if (r.IsFailure) count++; });
        Assert.Equal(1, count); count = 0;
        await IF().Inspect(r => { if (r.IsFailure) count++; });
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Inspect_TState_ActionResultOfT()
    {
        int count = 0;
        await S().Inspect(10, (s, r) => { if (r.IsSuccess) count += s + r.Value; });
        Assert.Equal(52, count); count = 0;
        await IS().Inspect(10, (s, r) => { if (r.IsSuccess) count += s + r.Value; });
        Assert.Equal(52, count); count = 0;

        await F().Inspect(10, (s, r) => { if (r.IsFailure) count += s; });
        Assert.Equal(10, count); count = 0;
        await IF().Inspect(10, (s, r) => { if (r.IsFailure) count += s; });
        Assert.Equal(10, count);
    }
}




