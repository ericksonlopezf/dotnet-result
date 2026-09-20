// Copyright © Erickson Lopez. MIT License.
using EricksonLopez.Result.Dapr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace EricksonLopez.Result.Dapr.Tests;

public sealed class DaprResultPubSubExtensionsTests
{
    [Fact]
    public void ToDaprTopicResult_WhenResultIsSuccess_ReturnsOkResult()
    {
        var result = Result.Success();

        var httpResult = result.ToDaprTopicResult();

        Assert.IsType<Ok>(httpResult);
    }

    [Fact]
    public void ToDaprTopicResult_WhenErrorIsTransient_ReturnsStatusCode503()
    {
        var error = Error.Unavailable("Dapr.NetworkFlake", "Connection lost")
                         .ToBuilder().WithRetryability(ErrorRetryability.Transient).Build();
        var result = Result.Failure(error);

        var httpResult = result.ToDaprTopicResult();

        var statusCodeResult = Assert.IsType<StatusCodeHttpResult>(httpResult);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCodeResult.StatusCode);
    }

    [Fact]
    public void ToDaprTopicResult_WhenErrorIsPermanent_ReturnsUnprocessableEntity()
    {
        var error = Error.Validation("Dapr.PoisonPill", "Invalid payload")
                         .ToBuilder().WithRetryability(ErrorRetryability.Permanent).Build();
        var result = Result.Failure(error);

        var httpResult = result.ToDaprTopicResult();

        var unprocessableResult = Assert.IsType<UnprocessableEntity<DaprErrorResponse>>(httpResult);
        Assert.Equal("DROP", unprocessableResult.Value?.Status);
        Assert.Equal("Dapr.PoisonPill", unprocessableResult.Value?.Code);
    }

    [Fact]
    public void ToDaprTopicResultGeneric_WhenResultIsSuccess_ReturnsOkWithValue()
    {
        var result = Result<string>.Success("processed-order");

        var httpResult = result.ToDaprTopicResult();

        var okResult = Assert.IsType<Ok<string>>(httpResult);
        Assert.Equal("processed-order", okResult.Value);
    }

    [Fact]
    public void ToDaprTopicResultGeneric_WhenErrorIsTransient_ReturnsStatusCode503()
    {
        var error = Error.Unavailable("Dapr.Timeout", "Service busy")
                         .ToBuilder().WithRetryability(ErrorRetryability.Transient).Build();
        var result = Result<string>.Failure(error);

        var httpResult = result.ToDaprTopicResult();

        var statusCodeResult = Assert.IsType<StatusCodeHttpResult>(httpResult);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCodeResult.StatusCode);
    }

    [Fact]
    public void ToDaprTopicResultGeneric_WhenErrorIsPermanent_ReturnsUnprocessableEntity()
    {
        var error = Error.Conflict("Dapr.DuplicateOrder", "Order already exists")
                         .ToBuilder().WithRetryability(ErrorRetryability.Permanent).Build();
        var result = Result<string>.Failure(error);

        var httpResult = result.ToDaprTopicResult();

        var unprocessableResult = Assert.IsType<UnprocessableEntity<DaprErrorResponse>>(httpResult);
        Assert.Equal("DROP", unprocessableResult.Value?.Status);
        Assert.Equal("Dapr.DuplicateOrder", unprocessableResult.Value?.Code);
    }
}
