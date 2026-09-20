// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace EricksonLopez.Result.Grpc;

/// <summary>
/// Represents a gRPC server interceptor that inspects handler results and unhandled exceptions,
/// converting domain <see cref="Error"/> failures and exceptions into standardized <see cref="RpcException"/> instances
/// with status codes and metadata trailers.
/// </summary>
public sealed class ResultServerInterceptor : Interceptor
{
    /// <inheritdoc/>
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (continuation is null)
        {
            throw new ArgumentNullException(nameof(continuation));
        }

        try
        {
            var response = await continuation(request, context).ConfigureAwait(false);

            if (response is IResultOutcome { IsFailure: true, Error: not null } outcome)
            {
                throw outcome.Error.ToRpcException();
            }

            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var error = Error.Unexpected("Rpc.UnhandledException", ex.Message);
            throw error.ToRpcException();
        }
    }
}
