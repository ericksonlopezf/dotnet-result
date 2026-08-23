// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultValidateAllCoverageTests
{
    [Fact]
    public void ValidateAll_Span_WhenContainsUninitialized_ThrowsInvalidOperationException()
    {
        Result uninit = default;
        var result = Assert.Throws<InvalidOperationException>(() => Result.ValidateAll(
            () => Result.Success(),
            () => uninit
        ));
        
    }

    [Fact]
    public void ValidateAll_WithValue_WhenMultipleFail_ChecksInnerErrors()
    {
        var input = "";
        var result = Result.ValidateAll(input,
            v => v.Length > 0 ? Result.Success() : Error.Validation("Empty", "Empty"),
            v => v.Contains('-') ? Result.Success() : Error.Validation("NoDash", "No dash"));

        Assert.True(result.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, result.Error.Code);
        Assert.Equal(2, result.Error.InnerErrors.Length);
        Assert.Equal("Empty", result.Error.InnerErrors[0].Code);
        Assert.Equal("NoDash", result.Error.InnerErrors[1].Code);
    }

    [Fact]
    public void ValidateAll_WithValue_WhenContainsUninitialized_ThrowsInvalidOperationException()
    {
        Result uninit = default;
        var result = Assert.Throws<InvalidOperationException>(() => Result.ValidateAll("test",
            v => Result.Success(),
            v => uninit
        ));
        
    }

    [Fact]
    public async Task ValidateAllAsync_Task_WhenValidatorsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<Func<CancellationToken, Task<Result>>> validators = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await Result.ValidateAllAsync(validators));
    }

    [Fact]
    public async Task ValidateAllAsync_Task_WhenCanceled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await Result.ValidateAllAsync([_ => Task.FromResult(Result.Success())], cts.Token));
    }

    [Fact]
    public async Task ValidateAllAsync_Task_WhenContainsUninitialized_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await Result.ValidateAllAsync([
            _ => Task.FromResult(Result.Success()),
            _ => Task.FromResult(default(Result))
        ]));
    }

    [Fact]
    public async Task ValidateAllAsync_ValueTask_WhenValidatorsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<Func<CancellationToken, ValueTask<Result>>> validators = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await Result.ValidateAllAsync(validators));
    }

    [Fact]
    public async Task ValidateAllAsync_ValueTask_WhenCanceled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await Result.ValidateAllAsync([_ => ValueTask.FromResult(Result.Success())], cts.Token));
    }

    [Fact]
    public async Task ValidateAllAsync_ValueTask_WhenContainsUninitialized_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await Result.ValidateAllAsync([_ => ValueTask.FromResult(Result.Success()), _ => ValueTask.FromResult(default(Result))]));
    }

    [Fact]
    public async Task ValidateAllAsync_Value_Task_WhenValidatorsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<Func<string, CancellationToken, Task<Result>>> validators = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await Result.ValidateAllAsync("val", validators));
    }

    [Fact]
    public async Task ValidateAllAsync_Value_Task_WhenCanceled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await Result.ValidateAllAsync("val", [(_, _) => Task.FromResult(Result.Success())], cts.Token));
    }

    [Fact]
    public async Task ValidateAllAsync_Value_Task_WhenContainsUninitialized_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await Result.ValidateAllAsync("val", [
            (_, _) => Task.FromResult(Result.Success()),
            (_, _) => Task.FromResult(default(Result))
        ]));
    }

    [Fact]
    public async Task ValidateAllAsync_Value_ValueTask_WhenValidatorsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<Func<string, CancellationToken, ValueTask<Result>>> validators = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await Result.ValidateAllAsync("val", validators));
    }

    [Fact]
    public async Task ValidateAllAsync_Value_ValueTask_WhenCanceled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await Result.ValidateAllAsync("val", [(_, _) => ValueTask.FromResult(Result.Success())], cts.Token));
    }

    [Fact]
    public async Task ValidateAllAsync_Value_ValueTask_WhenContainsUninitialized_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await Result.ValidateAllAsync("val", [
            (_, _) => ValueTask.FromResult(Result.Success()),
            (_, _) => ValueTask.FromResult(default(Result))
        ]));
    }
}



