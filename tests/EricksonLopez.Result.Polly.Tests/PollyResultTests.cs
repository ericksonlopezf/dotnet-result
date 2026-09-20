// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using Polly;
using Xunit;

namespace EricksonLopez.Result.Polly.Tests;

public class PollyResultTests
{
    [Fact]
    public void ExecuteResult_Returns_Success()
    {
        var pipeline = new ResiliencePipelineBuilder().Build();

        var result = pipeline.ExecuteResult(() => Result<string>.Success("hello"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void ExecuteResult_With_State_Avoids_Closure()
    {
        var pipeline = new ResiliencePipelineBuilder().Build();

        var result = pipeline.ExecuteResult(42, static state => Result<int>.Success(state * 2));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(84);
    }

    [Fact]
    public async Task ExecuteResultAsync_Returns_Success()
    {
        var pipeline = new ResiliencePipelineBuilder().Build();

        var result = await pipeline.ExecuteResultAsync(static async ct =>
        {
            await Task.Yield();
            return Result<int>.Success(100);
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(100);
    }

    [Fact]
    public async Task AddResultRetry_Retries_On_Retryable_Error()
    {
        int attempts = 0;
        var pipeline = new ResiliencePipelineBuilder<Result<int>>()
            .AddResultRetry(maxRetryAttempts: 2, delay: TimeSpan.FromMilliseconds(10))
            .Build();

        var result = await pipeline.ExecuteResultAsync(async ct =>
        {
            await Task.Yield();
            attempts++;
            if (attempts < 2)
            {
                return Result<int>.Failure(
                    Error.Unavailable("Server.Busy", "Temporary overload")
                         .ToBuilder().WithRetryability(ErrorRetryability.Transient).Build());
            }

            return Result<int>.Success(42);
        });

        attempts.Should().Be(2);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task AddResultRetry_Does_Not_Retry_On_NonRetryable_Error()
    {
        int attempts = 0;
        var pipeline = new ResiliencePipelineBuilder<Result<int>>()
            .AddResultRetry(maxRetryAttempts: 3, delay: TimeSpan.FromMilliseconds(10))
            .Build();

        var result = await pipeline.ExecuteResultAsync(async ct =>
        {
            await Task.Yield();
            attempts++;
            return Result<int>.Failure(
                Error.NotFound("User.NotFound", "User does not exist")
                     .ToBuilder().WithRetryability(ErrorRetryability.Permanent).Build());
        });

        attempts.Should().Be(1);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.NotFound");
    }

    [Fact]
    public void NonGeneric_ExecuteResult_Returns_Success()
    {
        var pipeline = new ResiliencePipelineBuilder().Build();

        var result = pipeline.ExecuteResult(static () => Result.Success());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void NonGeneric_ExecuteResult_With_State_Returns_Success()
    {
        var pipeline = new ResiliencePipelineBuilder().Build();
        var result = pipeline.ExecuteResult(42, static state => Result.Success());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task NonGeneric_ExecuteResultAsync_Returns_Success()
    {
        var pipeline = new ResiliencePipelineBuilder().Build();
        var result = await pipeline.ExecuteResultAsync(static async ct =>
        {
            await Task.Yield();
            return Result.Success();
        });
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddResultRetry_NonGeneric_Retries_On_Retryable_Error()
    {
        int attempts = 0;
        var pipeline = new ResiliencePipelineBuilder<Result>()
            .AddResultRetry(maxRetryAttempts: 2, delay: TimeSpan.FromMilliseconds(10))
            .Build();

        var result = await pipeline.ExecuteAsync(async ct =>
        {
            await Task.Yield();
            attempts++;
            if (attempts < 2)
            {
                return Result.Failure(
                    Error.Unavailable("Server.Busy", "Temporary overload")
                         .ToBuilder().WithRetryability(ErrorRetryability.Transient).Build());
            }

            return Result.Success();
        });

        attempts.Should().Be(2);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void PollyResultExtensions_Validate_Null_Arguments()
    {
        ResiliencePipeline nullPipeline = null!;
        Assert.Throws<ArgumentNullException>(() => nullPipeline.ExecuteResult(() => Result.Success()));
        Assert.Throws<ArgumentNullException>(() => nullPipeline.ExecuteResult(1, _ => Result.Success()));
        Assert.Throws<ArgumentNullException>(() => nullPipeline.ExecuteResult(() => Result<int>.Success(1)));
        Assert.Throws<ArgumentNullException>(() => nullPipeline.ExecuteResult(1, _ => Result<int>.Success(1)));

        ResiliencePipelineBuilder<Result> nullBuilder = null!;
        Assert.Throws<ArgumentNullException>(() => nullBuilder.AddResultRetry());

        ResiliencePipelineBuilder<Result<int>> nullGenericBuilder = null!;
        Assert.Throws<ArgumentNullException>(() => nullGenericBuilder.AddResultRetry());

        PredicateBuilder<Result> nullPredicate = null!;
        Assert.Throws<ArgumentNullException>(() => nullPredicate.HandleRetryableError());

        PredicateBuilder<Result<int>> nullGenericPredicate = null!;
        Assert.Throws<ArgumentNullException>(() => nullGenericPredicate.HandleRetryableError());
    }
}
