// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading.Tasks;
using EricksonLopez.Result.Grpc;
using Grpc.Core;
using Xunit;

namespace EricksonLopez.Result.Grpc.Tests;

public sealed class GrpcResultExtensionsTests
{
    [Fact]
    public async Task ToResultAsync_WhenTaskSucceeds_ReturnsSuccessResult()
    {
        var task = Task.FromResult("payload");

        var result = await task.ToResultAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal("payload", result.Value);
    }

    [Fact]
    public async Task ToResultAsync_WhenTaskThrowsRpcException_ReturnsMappedFailure()
    {
        var rpcException = new RpcException(new Status(StatusCode.NotFound, "Resource missing"));
        var task = Task.FromException<string>(rpcException);

        var result = await task.ToResultAsync();

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("Resource missing", result.Error.Description);
    }

    [Fact]
    public async Task ToResultAsync_WhenTaskIsNull_ThrowsArgumentNullException()
    {
        Task<string> nullTask = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() => nullTask.ToResultAsync());
    }

    [Fact]
    public async Task ToResultAsync_WhenCallIsNull_ThrowsArgumentNullException()
    {
        AsyncUnaryCall<string> nullCall = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() => nullCall.ToResultAsync());
    }

    [Fact]
    public async Task ToResultAsync_WhenCallSucceeds_ReturnsSuccessResult()
    {
        var responseTask = Task.FromResult("server-response");
        var headersTask = Task.FromResult(new Metadata());
        var trailersFunc = new Func<Metadata>(() => new Metadata());
        var statusFunc = new Func<Status>(() => Status.DefaultSuccess);
        var disposeAction = new Action(() => { });

        var call = new AsyncUnaryCall<string>(responseTask, headersTask, statusFunc, trailersFunc, disposeAction);

        var result = await call.ToResultAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal("server-response", result.Value);
    }

    [Fact]
    public async Task ToResultAsync_WhenCallThrowsRpcException_ReturnsMappedFailure()
    {
        var rpcException = new RpcException(new Status(StatusCode.PermissionDenied, "Access denied"));
        var responseTask = Task.FromException<string>(rpcException);
        var headersTask = Task.FromResult(new Metadata());
        var trailersFunc = new Func<Metadata>(() => new Metadata());
        var statusFunc = new Func<Status>(() => rpcException.Status);
        var disposeAction = new Action(() => { });

        var call = new AsyncUnaryCall<string>(responseTask, headersTask, statusFunc, trailersFunc, disposeAction);

        var result = await call.ToResultAsync();

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal("Access denied", result.Error.Description);
    }
}
