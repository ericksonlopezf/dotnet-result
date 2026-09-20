// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace EricksonLopez.Result.Grpc;

/// <summary>
/// Provides client-side extension methods for converting gRPC call invocations
/// into <see cref="Result{TValue}"/> envelopes without catching raw <see cref="RpcException"/>.
/// </summary>
public static class GrpcResultExtensions
{
    /// <summary>
    /// Asynchronously awaits a gRPC unary call, returning a successful <see cref="Result{TValue}"/>
    /// with the response on completion, or mapping an <see cref="RpcException"/> to a domain error failure.
    /// </summary>
    /// <typeparam name="TResponse">The gRPC response message type.</typeparam>
    /// <param name="call">The async unary call task.</param>
    /// <returns>A result containing the response or the mapped error.</returns>
    public static async Task<Result<TResponse>> ToResultAsync<TResponse>(this AsyncUnaryCall<TResponse> call)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        try
        {
            var response = await call.ResponseAsync.ConfigureAwait(false);
            return Result<TResponse>.Success(response);
        }
        catch (RpcException ex)
        {
            return Result<TResponse>.Failure(ex.ToError());
        }
    }

    /// <summary>
    /// Asynchronously awaits a gRPC response task, returning a successful <see cref="Result{TValue}"/>
    /// with the response on completion, or mapping an <see cref="RpcException"/> to a domain error failure.
    /// </summary>
    /// <typeparam name="TResponse">The gRPC response message type.</typeparam>
    /// <param name="responseTask">The async response task.</param>
    /// <returns>A result containing the response or the mapped error.</returns>
    public static async Task<Result<TResponse>> ToResultAsync<TResponse>(this Task<TResponse> responseTask)
    {
        if (responseTask is null)
        {
            throw new ArgumentNullException(nameof(responseTask));
        }

        try
        {
            var response = await responseTask.ConfigureAwait(false);
            return Result<TResponse>.Success(response);
        }
        catch (RpcException ex)
        {
            return Result<TResponse>.Failure(ex.ToError());
        }
    }
}
