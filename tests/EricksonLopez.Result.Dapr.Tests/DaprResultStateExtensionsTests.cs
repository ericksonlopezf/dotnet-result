// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using EricksonLopez.Result.Dapr;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace EricksonLopez.Result.Dapr.Tests;

public sealed class DaprResultStateExtensionsTests
{
    private readonly DaprClient _daprClient = Substitute.For<DaprClient>();

    [Fact]
    public async Task GetStateWithResultAsync_WhenClientIsNull_ThrowsArgumentNullException()
    {
        DaprClient nullClient = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullClient.GetStateWithResultAsync<string>("statestore", "key1"));
    }

    [Fact]
    public async Task GetStateWithResultAsync_WhenStoreNameIsNullOrEmpty_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _daprClient.GetStateWithResultAsync<string>("", "key1"));
    }

    [Fact]
    public async Task GetStateWithResultAsync_WhenKeyIsNullOrEmpty_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _daprClient.GetStateWithResultAsync<string>("statestore", ""));
    }

    [Fact]
    public async Task GetStateWithResultAsync_WhenStateExists_ReturnsSuccessResult()
    {
        _daprClient.GetStateAndETagAsync<string>("statestore", "order-1", Arg.Any<ConsistencyMode?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns(("order-data", "etag-1"));

        var result = await _daprClient.GetStateWithResultAsync<string>("statestore", "order-1");

        Assert.True(result.IsSuccess);
        Assert.Equal("order-data", result.Value);
    }

    [Fact]
    public async Task GetStateWithResultAsync_WhenStateDoesNotExist_ReturnsNotFoundError()
    {
        _daprClient.GetStateAndETagAsync<string>("statestore", "missing-key", Arg.Any<ConsistencyMode?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns((null!, ""));

        var result = await _daprClient.GetStateWithResultAsync<string>("statestore", "missing-key");

        Assert.True(result.IsFailure);
        Assert.Equal(DaprErrorCodes.StateNotFound, result.Error.Code);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task GetStateWithResultAsync_WhenDaprExceptionThrown_ReturnsUnavailableError()
    {
        _daprClient.GetStateAndETagAsync<string>("statestore", "key1", Arg.Any<ConsistencyMode?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new DaprException("Sidecar connection refused"));

        var result = await _daprClient.GetStateWithResultAsync<string>("statestore", "key1");

        Assert.True(result.IsFailure);
        Assert.Equal(DaprErrorCodes.SidecarUnavailable, result.Error.Code);
        Assert.Equal(ErrorRetryability.Transient, result.Error.Retryability);
    }

    [Fact]
    public async Task GetStateWithResultAsync_WhenHttpRequestExceptionThrown_ReturnsUnavailableError()
    {
        _daprClient.GetStateAndETagAsync<string>("statestore", "key1", Arg.Any<ConsistencyMode?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new System.Net.Http.HttpRequestException("HTTP error"));

        var result = await _daprClient.GetStateWithResultAsync<string>("statestore", "key1");

        Assert.True(result.IsFailure);
        Assert.Equal(DaprErrorCodes.SidecarUnavailable, result.Error.Code);
        Assert.Equal(ErrorRetryability.Transient, result.Error.Retryability);
    }

    [Fact]
    public async Task GetStateWithResultAsync_WhenOperationCancelled_ThrowsOperationCanceledException()
    {
        _daprClient.GetStateAndETagAsync<string>("statestore", "key1", Arg.Any<ConsistencyMode?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _daprClient.GetStateWithResultAsync<string>("statestore", "key1"));
    }

    [Fact]
    public async Task SaveStateWithResultAsync_WhenClientIsNull_ThrowsArgumentNullException()
    {
        DaprClient nullClient = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullClient.SaveStateWithResultAsync("statestore", "key1", "val", "etag1"));
    }

    [Fact]
    public async Task SaveStateWithResultAsync_WhenStoreNameIsNullOrEmpty_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _daprClient.SaveStateWithResultAsync("", "key1", "val", "etag1"));
    }

    [Fact]
    public async Task SaveStateWithResultAsync_WhenKeyIsNullOrEmpty_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _daprClient.SaveStateWithResultAsync("statestore", "", "val", "etag1"));
    }

    [Fact]
    public async Task SaveStateWithResultAsync_WhenSuccessful_ReturnsSuccessResult()
    {
        _daprClient.TrySaveStateAsync("statestore", "order-1", "order-payload", "etag-1", Arg.Any<StateOptions?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _daprClient.SaveStateWithResultAsync("statestore", "order-1", "order-payload", "etag-1");

        Assert.True(result.IsSuccess);
        Assert.Equal("order-payload", result.Value);
    }

    [Fact]
    public async Task SaveStateWithResultAsync_WhenEtagMismatchOccurs_ReturnsConflictError()
    {
        _daprClient.TrySaveStateAsync("statestore", "order-1", "order-payload", "stale-etag", Arg.Any<StateOptions?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _daprClient.SaveStateWithResultAsync("statestore", "order-1", "order-payload", "stale-etag");

        Assert.True(result.IsFailure);
        Assert.Equal(DaprErrorCodes.EtagMismatch, result.Error.Code);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(ErrorRetryability.Transient, result.Error.Retryability);
    }

    [Fact]
    public async Task SaveStateWithResultAsync_WhenDaprExceptionThrown_ReturnsUnavailableError()
    {
        _daprClient.TrySaveStateAsync("statestore", "order-1", "order-payload", "etag-1", Arg.Any<StateOptions?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new DaprException("Sidecar timeout"));

        var result = await _daprClient.SaveStateWithResultAsync("statestore", "order-1", "order-payload", "etag-1");

        Assert.True(result.IsFailure);
        Assert.Equal(DaprErrorCodes.SidecarUnavailable, result.Error.Code);
    }

    [Fact]
    public async Task SaveStateWithResultAsync_WhenHttpRequestExceptionThrown_ReturnsUnavailableError()
    {
        _daprClient.TrySaveStateAsync("statestore", "order-1", "order-payload", "etag-1", Arg.Any<StateOptions?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new System.Net.Http.HttpRequestException("Network fail"));

        var result = await _daprClient.SaveStateWithResultAsync("statestore", "order-1", "order-payload", "etag-1");

        Assert.True(result.IsFailure);
        Assert.Equal(DaprErrorCodes.SidecarUnavailable, result.Error.Code);
        Assert.Equal(ErrorRetryability.Transient, result.Error.Retryability);
    }

    [Fact]
    public async Task SaveStateWithResultAsync_WhenOperationCancelled_ThrowsOperationCanceledException()
    {
        _daprClient.TrySaveStateAsync("statestore", "order-1", "order-payload", "etag-1", Arg.Any<StateOptions?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _daprClient.SaveStateWithResultAsync("statestore", "order-1", "order-payload", "etag-1"));
    }

    [Fact]
    public async Task DeleteStateWithResultAsync_WhenClientIsNull_ThrowsArgumentNullException()
    {
        DaprClient nullClient = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullClient.DeleteStateWithResultAsync("statestore", "key1", "etag1"));
    }

    [Fact]
    public async Task DeleteStateWithResultAsync_WhenStoreNameIsNullOrEmpty_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _daprClient.DeleteStateWithResultAsync("", "key1", "etag1"));
    }

    [Fact]
    public async Task DeleteStateWithResultAsync_WhenKeyIsNullOrEmpty_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _daprClient.DeleteStateWithResultAsync("statestore", "", "etag1"));
    }

    [Fact]
    public async Task DeleteStateWithResultAsync_WhenSuccessful_ReturnsSuccessResult()
    {
        _daprClient.TryDeleteStateAsync("statestore", "order-1", "etag-1", Arg.Any<StateOptions?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _daprClient.DeleteStateWithResultAsync("statestore", "order-1", "etag-1");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteStateWithResultAsync_WhenEtagMismatchOccurs_ReturnsConflictError()
    {
        _daprClient.TryDeleteStateAsync("statestore", "order-1", "stale-etag", Arg.Any<StateOptions?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _daprClient.DeleteStateWithResultAsync("statestore", "order-1", "stale-etag");

        Assert.True(result.IsFailure);
        Assert.Equal(DaprErrorCodes.EtagMismatch, result.Error.Code);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task DeleteStateWithResultAsync_WhenDaprExceptionThrown_ReturnsUnavailableError()
    {
        _daprClient.TryDeleteStateAsync("statestore", "order-1", "etag-1", Arg.Any<StateOptions?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new DaprException("Sidecar error"));

        var result = await _daprClient.DeleteStateWithResultAsync("statestore", "order-1", "etag-1");

        Assert.True(result.IsFailure);
        Assert.Equal(DaprErrorCodes.SidecarUnavailable, result.Error.Code);
    }

    [Fact]
    public async Task DeleteStateWithResultAsync_WhenHttpRequestExceptionThrown_ReturnsUnavailableError()
    {
        _daprClient.TryDeleteStateAsync("statestore", "order-1", "etag-1", Arg.Any<StateOptions?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new System.Net.Http.HttpRequestException("Sidecar http connection drop"));

        var result = await _daprClient.DeleteStateWithResultAsync("statestore", "order-1", "etag-1");

        Assert.True(result.IsFailure);
        Assert.Equal(DaprErrorCodes.SidecarUnavailable, result.Error.Code);
        Assert.Equal(ErrorRetryability.Transient, result.Error.Retryability);
    }

    [Fact]
    public async Task DeleteStateWithResultAsync_WhenOperationCancelled_ThrowsOperationCanceledException()
    {
        _daprClient.TryDeleteStateAsync("statestore", "order-1", "etag-1", Arg.Any<StateOptions?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _daprClient.DeleteStateWithResultAsync("statestore", "order-1", "etag-1"));
    }
}
