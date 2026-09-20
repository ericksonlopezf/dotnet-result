// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;

namespace EricksonLopez.Result.Dapr;

/// <summary>
/// Provides extension methods on <see cref="DaprClient"/> for converting state store operations
/// into strongly-typed <see cref="Result"/> and <see cref="Result{TValue}"/> instances.
/// </summary>
public static class DaprResultStateExtensions
{
    /// <summary>
    /// Asynchronously retrieves state from a Dapr state store, mapping missing keys to <see cref="Error.NotFound"/>.
    /// </summary>
    /// <typeparam name="T">The type of state value.</typeparam>
    /// <param name="daprClient">The Dapr client instance.</param>
    /// <param name="storeName">The name of the state store component.</param>
    /// <param name="key">The key of the state to retrieve.</param>
    /// <param name="consistencyMode">The optional consistency mode.</param>
    /// <param name="metadata">Optional metadata for the state store operation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result containing the state value on success, or a mapped domain error on failure.</returns>
    public static async Task<Result<T>> GetStateWithResultAsync<T>(
        this DaprClient daprClient,
        string storeName,
        string key,
        ConsistencyMode? consistencyMode = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (daprClient is null)
        {
            throw new ArgumentNullException(nameof(daprClient));
        }

        if (string.IsNullOrEmpty(storeName))
        {
            throw new ArgumentException("Store name cannot be null or empty.", nameof(storeName));
        }

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        }

        try
        {
            var (value, etag) = await daprClient.GetStateAndETagAsync<T>(
                storeName, key, consistencyMode, metadata, cancellationToken).ConfigureAwait(false);

            if (value is null && string.IsNullOrEmpty(etag))
            {
                return Result<T>.Failure(
                    Error.NotFound(DaprErrorCodes.StateNotFound, $"State entry with key '{key}' was not found in store '{storeName}'."));
            }

            return Result<T>.Success(value!);
        }
        catch (DaprException ex)
        {
            return Result<T>.Failure(
                Error.Unavailable(DaprErrorCodes.SidecarUnavailable, ex.Message)
                     .ToBuilder().WithRetryability(ErrorRetryability.Transient).Build());
        }
        catch (HttpRequestException ex)
        {
            return Result<T>.Failure(
                Error.Unavailable(DaprErrorCodes.SidecarUnavailable, ex.Message)
                     .ToBuilder().WithRetryability(ErrorRetryability.Transient).Build());
        }
    }

    /// <summary>
    /// Asynchronously saves state to a Dapr state store with optimistic concurrency checking via ETag.
    /// </summary>
    /// <typeparam name="T">The type of state value.</typeparam>
    /// <param name="daprClient">The Dapr client instance.</param>
    /// <param name="storeName">The name of the state store component.</param>
    /// <param name="key">The key of the state to save.</param>
    /// <param name="value">The value of the state to save.</param>
    /// <param name="etag">The expected ETag for optimistic concurrency verification.</param>
    /// <param name="stateOptions">Optional state options controlling concurrency and consistency.</param>
    /// <param name="metadata">Optional metadata for the state store operation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result containing the saved value on success, or a concurrency conflict error on failure.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Mirrors DaprClient.TrySaveStateAsync signature with etag and options")]
    public static async Task<Result<T>> SaveStateWithResultAsync<T>(
        this DaprClient daprClient,
        string storeName,
        string key,
        T value,
        string etag,
        StateOptions? stateOptions = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (daprClient is null)
        {
            throw new ArgumentNullException(nameof(daprClient));
        }

        if (string.IsNullOrEmpty(storeName))
        {
            throw new ArgumentException("Store name cannot be null or empty.", nameof(storeName));
        }

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        }

        try
        {
            bool success = await daprClient.TrySaveStateAsync(
                storeName, key, value, etag, stateOptions, metadata, cancellationToken).ConfigureAwait(false);

            if (!success)
            {
                return Result<T>.Failure(
                    Error.Conflict(DaprErrorCodes.EtagMismatch, $"Concurrency conflict saving state for key '{key}' in store '{storeName}' with ETag '{etag}'.")
                         .ToBuilder().WithRetryability(ErrorRetryability.Transient).Build());
            }

            return Result<T>.Success(value);
        }
        catch (DaprException ex)
        {
            return Result<T>.Failure(
                Error.Unavailable(DaprErrorCodes.SidecarUnavailable, ex.Message)
                     .ToBuilder().WithRetryability(ErrorRetryability.Transient).Build());
        }
        catch (HttpRequestException ex)
        {
            return Result<T>.Failure(
                Error.Unavailable(DaprErrorCodes.SidecarUnavailable, ex.Message)
                     .ToBuilder().WithRetryability(ErrorRetryability.Transient).Build());
        }
    }

    /// <summary>
    /// Asynchronously deletes state from a Dapr state store with optimistic concurrency checking via ETag.
    /// </summary>
    /// <param name="daprClient">The Dapr client instance.</param>
    /// <param name="storeName">The name of the state store component.</param>
    /// <param name="key">The key of the state to delete.</param>
    /// <param name="etag">The expected ETag for optimistic concurrency verification.</param>
    /// <param name="stateOptions">Optional state options controlling concurrency and consistency.</param>
    /// <param name="metadata">Optional metadata for the state store operation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result indicating success or a concurrency conflict error on failure.</returns>
    public static async Task<Result> DeleteStateWithResultAsync(
        this DaprClient daprClient,
        string storeName,
        string key,
        string etag,
        StateOptions? stateOptions = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (daprClient is null)
        {
            throw new ArgumentNullException(nameof(daprClient));
        }

        if (string.IsNullOrEmpty(storeName))
        {
            throw new ArgumentException("Store name cannot be null or empty.", nameof(storeName));
        }

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        }

        try
        {
            bool success = await daprClient.TryDeleteStateAsync(
                storeName, key, etag, stateOptions, metadata, cancellationToken).ConfigureAwait(false);

            if (!success)
            {
                return Result.Failure(
                    Error.Conflict(DaprErrorCodes.EtagMismatch, $"Concurrency conflict deleting state for key '{key}' in store '{storeName}' with ETag '{etag}'.")
                         .ToBuilder().WithRetryability(ErrorRetryability.Transient).Build());
            }

            return Result.Success();
        }
        catch (DaprException ex)
        {
            return Result.Failure(
                Error.Unavailable(DaprErrorCodes.SidecarUnavailable, ex.Message)
                     .ToBuilder().WithRetryability(ErrorRetryability.Transient).Build());
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure(
                Error.Unavailable(DaprErrorCodes.SidecarUnavailable, ex.Message)
                     .ToBuilder().WithRetryability(ErrorRetryability.Transient).Build());
        }
    }
}
