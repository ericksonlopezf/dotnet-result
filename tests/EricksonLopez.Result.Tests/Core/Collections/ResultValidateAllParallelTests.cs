// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultValidateAllParallelTests
{
    [Fact]
    public async Task ValidateAllParallelAsync_WhenAllSucceed_ReturnsSuccess()
    {
        var validators = new List<Func<CancellationToken, Task<Result>>>
        {
            _ => Task.FromResult(Result.Success()),
            _ => Task.FromResult(Result.Success()),
            _ => Task.FromResult(Result.Success())
        };

        var result = await Result.ValidateAllParallelAsync(validators);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public async Task ValidateAllParallelAsync_WhenEmpty_ReturnsSuccess()
    {
        var validators = Array.Empty<Func<CancellationToken, Task<Result>>>();

        var result = await Result.ValidateAllParallelAsync(validators);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateAllParallelAsync_WhenNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            Result.ValidateAllParallelAsync(null!));
    }

    [Fact]
    public async Task ValidateAllParallelAsync_WhenSingleValidatorFails_ReturnsDirectFailure()
    {
        var error = Error.Validation("Name.Required", "Name is required.");
        var validators = new List<Func<CancellationToken, Task<Result>>>
        {
            _ => Task.FromResult(Result.Success()),
            _ => Task.FromResult(Result.Failure(error)),
            _ => Task.FromResult(Result.Success())
        };

        var result = await Result.ValidateAllParallelAsync(validators);

        Assert.True(result.IsFailure);
        Assert.Equal(error.Code, result.Error.Code);
        Assert.Equal(error.Description, result.Error.Description);
        Assert.False(result.Error.HasInnerErrors);
    }

    [Fact]
    public async Task ValidateAllParallelAsync_WhenMultipleValidatorsFail_ReturnsCompoundFailure()
    {
        var err1 = Error.Validation("Field.A", "Field A invalid");
        var err2 = Error.Validation("Field.B", "Field B invalid");
        var err3 = Error.Validation("Field.C", "Field C invalid");

        var validators = new List<Func<CancellationToken, Task<Result>>>
        {
            _ => Task.FromResult(Result.Failure(err1)),
            _ => Task.FromResult(Result.Success()),
            _ => Task.FromResult(Result.Failure(err2)),
            _ => Task.FromResult(Result.Failure(err3))
        };

        var result = await Result.ValidateAllParallelAsync(validators);

        Assert.True(result.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, result.Error.Code);
        Assert.Equal("3 validation errors occurred", result.Error.Description);
        Assert.True(result.Error.HasInnerErrors);
        Assert.Equal(3, result.Error.InnerErrors.Length);
        Assert.Equal(err1.Code, result.Error.InnerErrors[0].Code);
        Assert.Equal(err2.Code, result.Error.InnerErrors[1].Code);
        Assert.Equal(err3.Code, result.Error.InnerErrors[2].Code);
    }

    [Fact]
    public async Task ValidateAllParallelAsync_ExecutesConcurrently_NotSequentially()
    {
        const int delayMs = 100;
        var activeCount = 0;
        var maxActiveCount = 0;
        var syncLock = new object();

        var validators = new List<Func<CancellationToken, Task<Result>>>
        {
            async ct =>
            {
                lock (syncLock)
                {
                    activeCount++;
                    if (activeCount > maxActiveCount) maxActiveCount = activeCount;
                }
                await Task.Delay(delayMs, ct);
                lock (syncLock) { activeCount--; }
                return Result.Success();
            },
            async ct =>
            {
                lock (syncLock)
                {
                    activeCount++;
                    if (activeCount > maxActiveCount) maxActiveCount = activeCount;
                }
                await Task.Delay(delayMs, ct);
                lock (syncLock) { activeCount--; }
                return Result.Success();
            },
            async ct =>
            {
                lock (syncLock)
                {
                    activeCount++;
                    if (activeCount > maxActiveCount) maxActiveCount = activeCount;
                }
                await Task.Delay(delayMs, ct);
                lock (syncLock) { activeCount--; }
                return Result.Success();
            }
        };

        var result = await Result.ValidateAllParallelAsync(validators);

        Assert.True(result.IsSuccess);
        // Deterministically verify that validators executed concurrently (multiple active simultaneously)
        Assert.True(maxActiveCount >= 2, $"Expected concurrent execution (maxActiveCount >= 2), but observed: {maxActiveCount}");
    }

    [Fact]
    public async Task ValidateAllParallelAsync_TargetValue_WhenAllSucceed_ReturnsValue()
    {
        var validators = new List<Func<string, CancellationToken, Task<Result>>>
        {
            (val, _) => Task.FromResult(val.Length > 0 ? Result.Success() : Result.Failure(Error.Validation("Empty", "Empty"))),
            (val, _) => Task.FromResult(val.Contains('@') ? Result.Success() : Result.Failure(Error.Validation("NoAt", "No @")))
        };

        var result = await Result.ValidateAllParallelAsync("test@example.com", validators);

        Assert.True(result.IsSuccess);
        Assert.Equal("test@example.com", result.Value);
    }

    [Fact]
    public async Task ValidateAllParallelAsync_TargetValue_WhenNullValidators_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            Result.ValidateAllParallelAsync("test", null!));
    }

    [Fact]
    public async Task ValidateAllParallelAsync_TargetValue_WhenMultipleFail_ReturnsCompoundFailure()
    {
        var validators = new List<Func<int, CancellationToken, Task<Result>>>
        {
            (val, _) => Task.FromResult(val > 0 ? Result.Success() : Result.Failure(Error.Validation("Negative", "Negative"))),
            (val, _) => Task.FromResult(val > 10 ? Result.Success() : Result.Failure(Error.Validation("TooSmall", "Too small")))
        };

        var result = await Result.ValidateAllParallelAsync(-5, validators);

        Assert.True(result.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, result.Error.Code);
        Assert.Equal(2, result.Error.InnerErrors.Length);
    }

    [Fact]
    public async Task ValidateAllParallelAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var validators = new List<Func<CancellationToken, Task<Result>>>
        {
            ct => Task.Delay(100, ct).ContinueWith(_ => Result.Success(), ct)
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Result.ValidateAllParallelAsync(validators, cts.Token));
    }

    [Fact]
    public async Task ValidateAllParallelAsync_WhenValidatorYieldsUninitializedResult_ThrowsInvalidOperationException()
    {
        var validators = new List<Func<CancellationToken, Task<Result>>>
        {
            _ => Task.FromResult(default(Result))
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Result.ValidateAllParallelAsync(validators));
    }
}
