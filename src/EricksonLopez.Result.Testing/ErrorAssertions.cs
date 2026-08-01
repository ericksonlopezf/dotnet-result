using System;
using System.Diagnostics.CodeAnalysis;

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
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class ErrorAssertions
{
    /// <summary>
    /// Asserts that the Error has the expected error code.
    /// </summary>
    public static Error ShouldHaveErrorCode(this Error error, string expectedErrorCode)
    {
        if (error.Code != expectedErrorCode)
        throw new ResultAssertionException($"Expected error code '{expectedErrorCode}', but got '{error.Code}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Error has the expected <see cref="ErrorType"/>.
    /// </summary>
    public static Error ShouldHaveErrorType(this Error error, ErrorType expectedType)
    {
        if (error.Type != expectedType)
        throw new ResultAssertionException($"Expected ErrorType '{expectedType}', but got '{error.Type}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Error has the expected <see cref="ErrorSeverity"/>.
    /// </summary>
    public static Error ShouldHaveSeverity(this Error error, ErrorSeverity expectedSeverity)
    {
        if (error.Severity != expectedSeverity)
        throw new ResultAssertionException($"Expected ErrorSeverity '{expectedSeverity}', but got '{error.Severity}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Error has the expected <see cref="ErrorRetryability"/>.
    /// </summary>
    public static Error ShouldHaveRetryability(this Error error, ErrorRetryability expectedRetryability)
    {
        if (error.Retryability != expectedRetryability)
        throw new ResultAssertionException($"Expected ErrorRetryability '{expectedRetryability}', but got '{error.Retryability}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Error has the expected description.
    /// </summary>
    public static Error ShouldHaveDescription(this Error error, string expectedDescription)
    {
        if (!string.Equals(error.Description, expectedDescription, StringComparison.Ordinal))
        throw new ResultAssertionException($"Expected error Description to be '{expectedDescription}', but got '{error.Description}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Error contains a metadata entry with the specified key and value.
    /// </summary>
    public static Error ShouldHaveMetadata(this Error error, string key, object expectedValue)
    {
        if (!error.Metadata.TryGetValue(key, out var val) || !Equals(val, expectedValue))
        throw new ResultAssertionException($"Expected metadata key '{key}' with value '{expectedValue}', but got '{val}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Error contains a metadata entry with the specified key.
    /// </summary>
    public static Error ShouldHaveMetadataKey(this Error error, string key)
    {
        if (!error.Metadata.ContainsKey(key))
        throw new ResultAssertionException($"Expected metadata to contain key '{key}', but it was not found.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Error has the expected OpenTelemetry TraceId.
    /// </summary>
    public static Error ShouldHaveTraceId(this Error error, string expectedTraceId)
    {
        if (!string.Equals(error.TraceId, expectedTraceId, StringComparison.Ordinal))
        throw new ResultAssertionException($"Expected error TraceId to be '{expectedTraceId}', but got '{error.TraceId ?? "<null>"}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Error has the expected CorrelationId.
    /// </summary>
    public static Error ShouldHaveCorrelationId(this Error error, string expectedCorrelationId)
    {
        if (!string.Equals(error.CorrelationId, expectedCorrelationId, StringComparison.Ordinal))
        throw new ResultAssertionException($"Expected error CorrelationId to be '{expectedCorrelationId}', but got '{error.CorrelationId ?? "<null>"}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Error is retryable (Transient).
    /// </summary>
    public static Error ShouldBeRetryable(this Error error)
    {
        if (error.Retryability != ErrorRetryability.Transient)
        throw new ResultAssertionException($"Expected error to be Transient retryable, but got '{error.Retryability}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Error is permanent (not retryable).
    /// </summary>
    public static Error ShouldBePermanent(this Error error)
    {
        if (error.Retryability != ErrorRetryability.Permanent)
        throw new ResultAssertionException($"Expected error to be Permanent (not retryable), but got '{error.Retryability}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Error has inner errors of the expected count.
    /// </summary>
    public static Error ShouldHaveInnerErrors(this Error error, int expectedCount)
    {
        if (error.InnerErrors.Length != expectedCount)
        throw new ResultAssertionException($"Expected {expectedCount} inner errors, but got {error.InnerErrors.Length}.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Error has no inner errors.
    /// </summary>
    public static Error ShouldHaveNoInnerErrors(this Error error)
    {
        if (error.HasInnerErrors)
        throw new ResultAssertionException($"Expected no inner errors, but found {error.InnerErrors.Length}.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Error has at least one inner error with the specified error code.
    /// </summary>
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




