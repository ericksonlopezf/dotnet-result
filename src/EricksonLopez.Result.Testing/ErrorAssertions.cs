// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics.CodeAnalysis;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Testing;

/// <summary>
/// Fluent assertion extensions directly on <see cref="Error"/> instances.
/// Enables chained assertions after <see cref="ResultAssertions.ShouldBeFailure"/>:
/// <code>
/// result.ShouldBeFailure()
///       .ShouldHaveErrorCode("Order.Expired")
///       .ShouldHaveErrorType(ErrorType.Domain)
///       .ShouldHaveSeverity(ErrorSeverity.Warning)
///       .ShouldHaveMetadata("orderId", expectedId);
/// </code>
/// </summary>
public static class ErrorAssertions
{
    /// <summary>
    /// Asserts that the Error has the expected error code.
    /// </summary>
    /// <param name="error">The error to assert on.</param>
    /// <param name="expectedErrorCode">The expected error code string.</param>
    /// <returns>The same <paramref name="error"/> instance for method chaining.</returns>
    /// <exception cref="ResultAssertionException">The error code does not match <paramref name="expectedErrorCode"/></exception>
    public static Error ShouldHaveErrorCode(this Error error, string expectedErrorCode)
    {
        if (error.Code != expectedErrorCode)
            throw new ResultAssertionException($"Expected error code '{expectedErrorCode}', but got '{error.Code}'.");

        return error;
    }

    /// <summary>
    /// Asserts that the Error has the expected <see cref="ErrorType"/>.
    /// </summary>
    /// <param name="error">The error to assert on.</param>
    /// <param name="expectedType">The expected error type.</param>
    /// <returns>The same <paramref name="error"/> instance for method chaining.</returns>
    /// <exception cref="ResultAssertionException">The error type does not match <paramref name="expectedType"/></exception>
    public static Error ShouldHaveErrorType(this Error error, ErrorType expectedType)
    {
        if (error.Type != expectedType)
            throw new ResultAssertionException($"Expected ErrorType '{expectedType}', but got '{error.Type}'.");

        return error;
    }

    /// <summary>
    /// Asserts that the Error has the expected <see cref="ErrorSeverity"/>.
    /// </summary>
    /// <param name="error">The error to assert on.</param>
    /// <param name="expectedSeverity">The expected error severity.</param>
    /// <returns>The same <paramref name="error"/> instance for method chaining.</returns>
    /// <exception cref="ResultAssertionException">The severity does not match <paramref name="expectedSeverity"/></exception>
    public static Error ShouldHaveSeverity(this Error error, ErrorSeverity expectedSeverity)
    {
        if (error.Severity != expectedSeverity)
            throw new ResultAssertionException($"Expected ErrorSeverity '{expectedSeverity}', but got '{error.Severity}'.");

        return error;
    }

    /// <summary>
    /// Asserts that the Error has the expected <see cref="ErrorRetryability"/>.
    /// </summary>
    /// <param name="error">The error to assert on.</param>
    /// <param name="expectedRetryability">The expected error retryability.</param>
    /// <returns>The same <paramref name="error"/> instance for method chaining.</returns>
    /// <exception cref="ResultAssertionException">The retryability does not match <paramref name="expectedRetryability"/></exception>
    public static Error ShouldHaveRetryability(this Error error, ErrorRetryability expectedRetryability)
    {
        if (error.Retryability != expectedRetryability)
            throw new ResultAssertionException($"Expected ErrorRetryability '{expectedRetryability}', but got '{error.Retryability}'.");

        return error;
    }

    /// <summary>
    /// Asserts that the Error has the expected description.
    /// </summary>
    /// <param name="error">The error to assert on.</param>
    /// <param name="expectedDescription">The expected error description string.</param>
    /// <returns>The same <paramref name="error"/> instance for method chaining.</returns>
    /// <exception cref="ResultAssertionException">The description does not match <paramref name="expectedDescription"/></exception>
    public static Error ShouldHaveDescription(this Error error, string expectedDescription)
    {
        if (!string.Equals(error.Description, expectedDescription, StringComparison.Ordinal))
            throw new ResultAssertionException($"Expected error Description to be '{expectedDescription}', but got '{error.Description}'.");

        return error;
    }

    /// <summary>
    /// Asserts that the Error contains a metadata entry with the specified key and value.
    /// </summary>
    /// <param name="error">The error to assert on.</param>
    /// <param name="key">The metadata key to look up.</param>
    /// <param name="expectedValue">The expected value corresponding to <paramref name="key"/>.</param>
    /// <returns>The same <paramref name="error"/> instance for method chaining.</returns>
    /// <exception cref="ResultAssertionException">The metadata key was not found or its value does not match <paramref name="expectedValue"/></exception>
    public static Error ShouldHaveMetadata(this Error error, string key, object expectedValue)
    {
        if (!error.Metadata.TryGetValue(key, out var val) || !Equals(val, expectedValue))
            throw new ResultAssertionException($"Expected metadata key '{key}' with value '{expectedValue}', but got '{val}'.");

        return error;
    }

    /// <summary>
    /// Asserts that the Error contains a metadata entry with the specified key.
    /// </summary>
    /// <param name="error">The error to assert on.</param>
    /// <param name="key">The metadata key to check for presence.</param>
    /// <returns>The same <paramref name="error"/> instance for method chaining.</returns>
    /// <exception cref="ResultAssertionException">The metadata key was not found</exception>
    public static Error ShouldHaveMetadataKey(this Error error, string key)
    {
        if (!error.Metadata.ContainsKey(key))
            throw new ResultAssertionException($"Expected metadata to contain key '{key}', but it was not found.");

        return error;
    }

    /// <summary>
    /// Asserts that the Error has the expected OpenTelemetry TraceId.
    /// </summary>
    /// <param name="error">The error to assert on.</param>
    /// <param name="expectedTraceId">The expected trace ID string.</param>
    /// <returns>The same <paramref name="error"/> instance for method chaining.</returns>
    /// <exception cref="ResultAssertionException">The trace ID does not match <paramref name="expectedTraceId"/></exception>
    public static Error ShouldHaveTraceId(this Error error, string expectedTraceId)
    {
        if (!string.Equals(error.TraceId, expectedTraceId, StringComparison.Ordinal))
            throw new ResultAssertionException($"Expected error TraceId to be '{expectedTraceId}', but got '{error.TraceId ?? "<null>"}'.");

        return error;
    }

    /// <summary>
    /// Asserts that the Error has the expected CorrelationId.
    /// </summary>
    /// <param name="error">The error to assert on.</param>
    /// <param name="expectedCorrelationId">The expected correlation ID string.</param>
    /// <returns>The same <paramref name="error"/> instance for method chaining.</returns>
    /// <exception cref="ResultAssertionException">The correlation ID does not match <paramref name="expectedCorrelationId"/></exception>
    public static Error ShouldHaveCorrelationId(this Error error, string expectedCorrelationId)
    {
        if (!string.Equals(error.CorrelationId, expectedCorrelationId, StringComparison.Ordinal))
            throw new ResultAssertionException($"Expected error CorrelationId to be '{expectedCorrelationId}', but got '{error.CorrelationId ?? "<null>"}'.");

        return error;
    }

    /// <summary>
    /// Asserts that the Error is retryable (Transient).
    /// </summary>
    /// <param name="error">The error to assert on.</param>
    /// <returns>The same <paramref name="error"/> instance for method chaining.</returns>
    /// <exception cref="ResultAssertionException">The error retryability is not Transient</exception>
    public static Error ShouldBeRetryable(this Error error)
    {
        if (error.Retryability != ErrorRetryability.Transient)
            throw new ResultAssertionException($"Expected error to be Transient retryable, but got '{error.Retryability}'.");

        return error;
    }

    /// <summary>
    /// Asserts that the Error is permanent (not retryable).
    /// </summary>
    /// <param name="error">The error to assert on.</param>
    /// <returns>The same <paramref name="error"/> instance for method chaining.</returns>
    /// <exception cref="ResultAssertionException">The error retryability is not Permanent</exception>
    public static Error ShouldBePermanent(this Error error)
    {
        if (error.Retryability != ErrorRetryability.Permanent)
            throw new ResultAssertionException($"Expected error to be Permanent (not retryable), but got '{error.Retryability}'.");

        return error;
    }

    /// <summary>
    /// Asserts that the Error has inner errors of the expected count.
    /// </summary>
    /// <param name="error">The error to assert on.</param>
    /// <param name="expectedCount">The expected number of inner errors.</param>
    /// <returns>The same <paramref name="error"/> instance for method chaining.</returns>
    /// <exception cref="ResultAssertionException">The inner error count does not match <paramref name="expectedCount"/></exception>
    public static Error ShouldHaveInnerErrors(this Error error, int expectedCount)
    {
        if (error.InnerErrors.Length != expectedCount)
            throw new ResultAssertionException($"Expected {expectedCount} inner errors, but got {error.InnerErrors.Length}.");

        return error;
    }

    /// <summary>
    /// Asserts that the Error has no inner errors.
    /// </summary>
    /// <param name="error">The error to assert on.</param>
    /// <returns>The same <paramref name="error"/> instance for method chaining.</returns>
    /// <exception cref="ResultAssertionException">The error contains inner errors</exception>
    public static Error ShouldHaveNoInnerErrors(this Error error)
    {
        if (error.HasInnerErrors)
            throw new ResultAssertionException($"Expected no inner errors, but found {error.InnerErrors.Length}.");

        return error;
    }

    /// <summary>
    /// Asserts that the Error has at least one inner error with the specified error code.
    /// </summary>
    /// <param name="error">The error to assert on.</param>
    /// <param name="errorCode">The error code to search for among inner errors.</param>
    /// <returns>The same <paramref name="error"/> instance for method chaining.</returns>
    /// <exception cref="ResultAssertionException">No inner error with <paramref name="errorCode"/> was found</exception>
    public static Error ShouldContainInnerError(this Error error, string errorCode)
    {
        if (!error.HasInnerErrors)
            throw new ResultAssertionException($"Expected at least one inner error with code '{errorCode}', but error has no inner errors.");


        var innerErrors = error.InnerErrors;
        for (int i = 0; i < innerErrors.Length; i++)
        {
            if (string.Equals(innerErrors[i].Code, errorCode, StringComparison.Ordinal))
                return error;
        }

        throw new ResultAssertionException($"Expected at least one inner error with code '{errorCode}', but none was found.");
    }
}






