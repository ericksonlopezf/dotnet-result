// Copyright © Erickson Lopez. MIT License.
// Stryker disable Boolean : ConfigureAwait(false) equivalent mutation
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using MediatR;

namespace EricksonLopez.Result.MediatR;

/// <summary>
/// MediatR pipeline behavior that catches unhandled exceptions in request handlers
/// and wraps them as <see cref="Result"/> failures instead of propagating the exception.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type (must be <see cref="Result"/> or <see cref="Result{T}"/>).</typeparam>
/// <remarks>
/// <para>
/// This behavior intercepts exceptions thrown by inner handlers and converts them into
/// <see cref="Result.Failure(Error)"/> using a configurable error factory. By default,
/// it creates an <see cref="ErrorType.Unexpected"/> error with the exception message.
/// </para>
/// <para>
/// <b>Registration:</b>
/// <code>
/// services.AddTransient(typeof(IPipelineBehavior&lt;,&gt;), typeof(ResultExceptionBehavior&lt;,&gt;));
/// </code>
/// </para>
/// <para>
/// <b>Note:</b> This behavior only activates when the response type is <see cref="Result"/>
/// or <see cref="Result{T}"/>. Non-Result responses pass through unmodified.
/// </para>
/// <para>
/// <b>Performance:</b> The failure factory delegate is compiled once per closed generic
/// <typeparamref name="TResponse"/> type and cached statically. Subsequent invocations
/// use the cached delegate with zero reflection overhead.
/// </para>
/// <para>
/// <b>⚠ NativeAOT / Trimming:</b> This class uses <c>Expression.Lambda.Compile()</c> and
/// <c>MakeGenericMethod()</c> at static initialization to build a cached failure delegate.
/// It requires dynamic code generation and is not compatible with NativeAOT.
/// MediatR itself (<c>IsAotCompatible=false</c>) already requires dynamic code, so this
/// package is only intended for reflection-based runtimes.
/// </para>
/// </remarks>
[RequiresDynamicCode(
    "ResultExceptionBehavior uses Expression.Lambda.Compile() and MakeGenericMethod at static initialization " +
    "to build a cached Result failure delegate. This requires dynamic code generation and is not compatible " +
    "with NativeAOT. MediatR itself (IsAotCompatible=false) already requires dynamic code.")]
[RequiresUnreferencedCode(
    "ResultExceptionBehavior uses MakeGenericMethod to call Result.Failure<T>(Error) at static initialization. " +
    "The generic method Result.Failure<TValue> must be preserved by the linker. " +
    "MediatR itself (IsAotCompatible=false) is not compatible with trimming.")]
public sealed class ResultExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // Cache the failure factory delegate per closed generic TResponse type.
    // This is a static field on the closed generic type, so each ResultExceptionBehavior<TReq, Result<T>>
    // gets its own cache slot — no dictionary lookup needed at runtime.
    private static readonly Func<Error, TResponse>? CachedFailureFactory = BuildFailureFactory();

    private readonly Func<Exception, Error>? _errorFactory;

    /// <summary>
    /// Creates a new instance using the default error factory.
    /// </summary>
    public ResultExceptionBehavior() : this(null) { }

    /// <summary>
    /// Creates a new instance with a custom error factory.
    /// </summary>
    /// <param name="errorFactory">
    /// Optional factory to create <see cref="Error"/> from an <see cref="Exception"/>.
    /// When null, uses <see cref="Error.Unexpected(string, string)"/> with the exception type and message.
    /// </param>
    public ResultExceptionBehavior(Func<Exception, Error>? errorFactory)
    {
        _errorFactory = errorFactory;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Only intercept if the response is a Result type (cached factory is non-null)
        if (CachedFailureFactory is null)
            return await next(cancellationToken).ConfigureAwait(false);

        try
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw; // Don't catch cancellation — let it propagate
        }
        catch (Exception ex)
        {
            // ⚠ Security note: ex.Message may contain sensitive data (connection strings, internal paths,
            // file system paths, or PII) in exceptions like SqlException, HttpRequestException, or SocketException.
            // If the ASP.NET Core integration is configured with IncludeDescription = true, this data can
            // be exposed in the HTTP response ProblemDetails body.
            // To prevent sensitive data leakage, provide a custom errorFactory that sanitizes the exception:
            //
            //   services.AddTransient<IPipelineBehavior<,>>(provider =>
            //       new ResultExceptionBehavior<,>(ex => Error.Unexpected("Handler.Error", "An unexpected error occurred.")));
            //
            // See the ResultExceptionBehavior constructor overload for custom errorFactory registration.
            var error = _errorFactory?.Invoke(ex)
                ?? Error.Unexpected(
                    $"Handler.{ex.GetType().Name}",
                    "An unexpected handler error occurred.");

            return CachedFailureFactory(error);
        }
    }

    /// <summary>
    /// Builds a compiled delegate that creates a failure TResponse from an Error.
    /// Returns null if TResponse is not a Result type — used to skip interception entirely.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For <c>TResponse == Result</c>: returns <c>error => (TResponse)(object)Result.Failure(error)</c>.
    /// </para>
    /// <para>
    /// For <c>TResponse == Result&lt;T&gt;</c>: compiles an Expression that calls
    /// <c>Result.Failure&lt;T&gt;(error)</c> directly — no <c>MakeGenericMethod</c>,
    /// no <c>MethodInfo.Invoke</c>, no <c>object[]</c> boxing at call time.
    /// The Expression is compiled once and reused for all subsequent invocations.
    /// </para>
    /// </remarks>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2060",
        Justification = "MakeGenericMethod is called once at static init to build a cached delegate. " +
                        "The MediatR package is not AOT-compatible (IsAotCompatible=false). " +
                        "The generic argument is extracted from the known Result<T> type parameter.")]
    [RequiresDynamicCode(
        "ResultExceptionBehavior uses Expression.Lambda.Compile() and MakeGenericMethod at static initialization " +
        "to build a cached Result failure delegate. This requires dynamic code generation and is not compatible " +
        "with NativeAOT. MediatR itself (IsAotCompatible=false) already requires dynamic code, so this " +
        "package is only intended for use in reflection-based runtimes.")]
    private static Func<Error, TResponse>? BuildFailureFactory()
    {
        var responseType = typeof(TResponse);

        // Non-generic Result
        if (responseType == typeof(Result))
        {
            return static error => (TResponse)(object)Result.Failure(error);
        }

        // Generic Result<T>
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];

            // Build: (Error error) => Result.Failure<T>(error)
            // Using Expression trees to compile a direct call without per-invocation reflection.
            var errorParam = Expression.Parameter(typeof(Error));
            var failureMethod = typeof(Result)
                .GetMethod(nameof(Result.Failure), 1, new[] { typeof(Error) })!
                .MakeGenericMethod(valueType);

            var callExpr = Expression.Call(failureMethod, errorParam);
            // Result.Failure<T> returns Result<T> which IS TResponse, so we can cast directly.
            var castExpr = Expression.Convert(callExpr, typeof(TResponse));
            var lambda = Expression.Lambda<Func<Error, TResponse>>(castExpr, errorParam);

            return lambda.Compile();
        }

        // Not a Result type — return null to signal "don't intercept"
        return null;
    }
}




