// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultValidateAllTests
{
    [Fact]
    public void ValidateAll_WhenAllValidatorsSucceed_ReturnsSuccess()
    {
        var result = Result.ValidateAll(
            () => Result.Success(),
            () => Result.Success(),
            () => Result.Success());

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void ValidateAll_WhenEmpty_ReturnsSuccess()
    {
        var result = Result.ValidateAll(ReadOnlySpan<Func<Result>>.Empty);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateAll_WhenSingleValidatorFails_ReturnsDirectFailure()
    {
        var error = Error.Validation("Name.Required", "Name is required.");
        var result = Result.ValidateAll(
            () => Result.Success(),
            () => error,
            () => Result.Success());

        Assert.True(result.IsFailure);
        Assert.Equal(error.Code, result.Error.Code);
        Assert.Equal(error.Description, result.Error.Description);
        Assert.False(result.Error.HasInnerErrors);
    }

    [Fact]
    public void ValidateAll_WhenMultipleValidatorsFail_ReturnsCompoundFailure()
    {
        var err1 = Error.Validation("Field.A", "Field A invalid");
        var err2 = Error.Validation("Field.B", "Field B invalid");
        var err3 = Error.Validation("Field.C", "Field C invalid");

        var result = Result.ValidateAll(
            () => err1,
            () => Result.Success(),
            () => err2,
            () => err3);

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
    public void ValidateAll_WithValue_WhenAllSucceed_ReturnsSuccessWithValue()
    {
        var input = "valid-string";
        var result = Result.ValidateAll(input,
            v => v.Length > 0 ? Result.Success() : Error.Validation("Empty", "Empty string"),
            v => v.Contains('-') ? Result.Success() : Error.Validation("NoDash", "No dash"));

        Assert.True(result.IsSuccess);
        Assert.Equal("valid-string", result.Value);
    }

    [Fact]
    public void ValidateAll_WithValue_WhenSingleFails_ReturnsFailure()
    {
        var input = "invalid";
        var result = Result.ValidateAll(input,
            v => v.Length > 0 ? Result.Success() : Error.Validation("Empty", "Empty"),
            v => v.Contains('-') ? Result.Success() : Error.Validation("NoDash", "No dash"));

        Assert.True(result.IsFailure);
        Assert.Equal("NoDash", result.Error.Code);
    }

    [Fact]
    public void ValidateAll_WithValue_WhenMultipleFail_ReturnsCompoundFailure()
    {
        var input = "";
        var result = Result.ValidateAll(input,
            v => v.Length > 0 ? Result.Success() : Error.Validation("Empty", "Empty"),
            v => v.Contains('-') ? Result.Success() : Error.Validation("NoDash", "No dash"));

        Assert.True(result.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, result.Error.Code);
        Assert.Equal(2, result.Error.InnerErrors.Length);
    }

    [Fact]
    public async Task ValidateAllAsync_Task_WhenAllSucceed_ReturnsSuccess()
    {
        var validators = new List<Func<CancellationToken, Task<Result>>>
        {
            _ => Task.FromResult(Result.Success()),
            _ => Task.FromResult(Result.Success())
        };

        var result = await Result.ValidateAllAsync(validators);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateAllAsync_Task_WhenSingleFail_ReturnsDirectFailure()
    {
        var validators = new List<Func<CancellationToken, Task<Result>>>
        {
            _ => Task.FromResult(Result.Success()),
            _ => Task.FromResult<Result>(Error.Validation("E1", "Error 1"))
        };

        var result = await Result.ValidateAllAsync(validators);

        Assert.True(result.IsFailure);
        Assert.Equal("E1", result.Error.Code);
        Assert.False(result.Error.HasInnerErrors);
    }

    [Fact]
    public async Task ValidateAllAsync_Task_WhenMultipleFail_ReturnsCompoundFailure()
    {
        var validators = new List<Func<CancellationToken, Task<Result>>>
        {
            _ => Task.FromResult<Result>(Error.Validation("E1", "Error 1")),
            _ => Task.FromResult(Result.Success()),
            _ => Task.FromResult<Result>(Error.Validation("E2", "Error 2"))
        };

        var result = await Result.ValidateAllAsync(validators);

        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Error.InnerErrors.Length);
    }

    [Fact]
    public async Task ValidateAllAsync_TaskWithValue_WhenSucceed_ReturnsValue()
    {
        var validators = new List<Func<int, CancellationToken, Task<Result>>>
        {
            (val, _) => Task.FromResult(val > 0 ? Result.Success() : Result.Failure(Error.Validation("Negative", "Negative"))),
            (val, _) => Task.FromResult(val < 100 ? Result.Success() : Result.Failure(Error.Validation("TooHigh", "Too high")))
        };

        var result = await Result.ValidateAllAsync(42, validators);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task ValidateAllAsync_TaskWithValue_WhenSingleFail_ReturnsFailure()
    {
        var validators = new List<Func<int, CancellationToken, Task<Result>>>
        {
            (val, _) => Task.FromResult(val > 0 ? Result.Success() : Result.Failure(Error.Validation("Negative", "Negative"))),
            (val, _) => Task.FromResult(val < 100 ? Result.Success() : Result.Failure(Error.Validation("TooHigh", "Too high")))
        };

        var result = await Result.ValidateAllAsync(-5, validators);

        Assert.True(result.IsFailure);
        Assert.Equal("Negative", result.Error.Code);
    }

    [Fact]
    public async Task ValidateAllAsync_TaskWithValue_WhenMultipleFail_ReturnsCompoundFailure()
    {
        var validators = new List<Func<int, CancellationToken, Task<Result>>>
        {
            (val, _) => Task.FromResult(val > 0 ? Result.Success() : Result.Failure(Error.Validation("Negative", "Negative"))),
            (val, _) => Task.FromResult(val > 10 ? Result.Success() : Result.Failure(Error.Validation("TooLow", "Too low")))
        };

        var result = await Result.ValidateAllAsync(-5, validators);

        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Error.InnerErrors.Length);
    }

    [Fact]
    public async Task ValidateAllAsync_ValueTask_WhenAllSucceed_ReturnsSuccess()
    {
        var validators = new List<Func<CancellationToken, ValueTask<Result>>>
        {
            _ => ValueTask.FromResult(Result.Success()),
            _ => ValueTask.FromResult(Result.Success())
        };

        var result = await Result.ValidateAllAsync(validators);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateAllAsync_ValueTask_WhenSingleFail_ReturnsDirectFailure()
    {
        var validators = new List<Func<CancellationToken, ValueTask<Result>>>
        {
            _ => ValueTask.FromResult(Result.Success()),
            _ => ValueTask.FromResult<Result>(Error.Validation("E1", "Error 1"))
        };

        var result = await Result.ValidateAllAsync(validators);

        Assert.True(result.IsFailure);
        Assert.Equal("E1", result.Error.Code);
    }

    [Fact]
    public async Task ValidateAllAsync_ValueTask_WhenMultipleFail_ReturnsCompound()
    {
        var validators = new List<Func<CancellationToken, ValueTask<Result>>>
        {
            _ => ValueTask.FromResult<Result>(Error.Validation("E1", "Error 1")),
            _ => ValueTask.FromResult<Result>(Error.Validation("E2", "Error 2"))
        };

        var result = await Result.ValidateAllAsync(validators);

        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Error.InnerErrors.Length);
    }

    [Fact]
    public async Task ValidateAllAsync_ValueTaskWithValue_WhenSucceed_ReturnsValue()
    {
        var validators = new List<Func<string, CancellationToken, ValueTask<Result>>>
        {
            (val, _) => ValueTask.FromResult(Result.Success())
        };

        var result = await Result.ValidateAllAsync("test", validators);

        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value);
    }

    [Fact]
    public async Task ValidateAllAsync_ValueTaskWithValue_WhenSingleFail_ReturnsFailure()
    {
        var validators = new List<Func<string, CancellationToken, ValueTask<Result>>>
        {
            (val, _) => ValueTask.FromResult(Result.Success()),
            (val, _) => ValueTask.FromResult<Result>(Error.Validation("E1", "Error 1"))
        };

        var result = await Result.ValidateAllAsync("test", validators);

        Assert.True(result.IsFailure);
        Assert.Equal("E1", result.Error.Code);
    }

    [Fact]
    public async Task ValidateAllAsync_ValueTaskWithValue_WhenMultipleFail_ReturnsCompoundFailure()
    {
        var validators = new List<Func<string, CancellationToken, ValueTask<Result>>>
        {
            (val, _) => ValueTask.FromResult<Result>(Error.Validation("E1", "Error 1")),
            (val, _) => ValueTask.FromResult<Result>(Error.Validation("E2", "Error 2"))
        };

        var result = await Result.ValidateAllAsync("test", validators);

        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Error.InnerErrors.Length);
    }
}
