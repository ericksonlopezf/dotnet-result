using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using FluentValidation.Results;

namespace EricksonLopez.Result.FluentValidation;

/// <summary>
/// Extension methods for converting FluentValidation <see cref="ValidationResult"/>
/// to <see cref="Result"/> and <see cref="Result{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Converts validation failures into structured <see cref="Error"/> instances with:
/// <list type="bullet">
///   <item><see cref="ErrorType.Validation"/> type</item>
///   <item>Error code from <see cref="ValidationFailure.ErrorCode"/> (or property name fallback)</item>
///   <item>Description from <see cref="ValidationFailure.ErrorMessage"/></item>
///   <item>Inner errors for each individual validation failure</item>
///   <item>Metadata with property name, attempted value, and severity</item>
/// </list>
/// </para>
/// <para>
/// <b>? ErrorCode behavior:</b> FluentValidation sets <see cref="ValidationFailure.ErrorCode"/>
/// to the <i>validator type name</i> by default (e.g., <c>"NotEmptyValidator"</c>,
/// <c>"GreaterThanValidator"</c>). This does NOT follow the <c>"Domain.Property"</c>
/// naming convention used by the rest of this library. When using default FluentValidation
/// validators, the generated error codes will be validator type names, not domain codes.
/// </para>
/// <para>
/// To produce domain-consistent error codes (e.g., <c>"Order.Customer.Required"</c>),
/// use FluentValidation's <c>.WithErrorCode()</c> extension on each rule:
/// <code>
/// RuleFor(x => x.Customer)
///     .NotEmpty()
///     .WithErrorCode("Order.Customer.Required");
/// </code>
/// Without <c>.WithErrorCode()</c>, the fallback is <c>"Validation.{PropertyName}"</c>
/// (e.g., <c>"Validation.Customer"</c>), which is better than a validator type name
/// but still not a fully qualified domain code.
/// </para>
/// <para>
/// <b>Note:</b> <see cref="ValidationFailure.CustomState"/> is not propagated to error
/// metadata. Use <c>ValidationFailure.CustomState</c> if you need structured data from
/// validators and access it before conversion, or set metadata via <c>.WithState()</c>
/// with a <see cref="System.Collections.Generic.Dictionary{TKey,TValue}"/> and
/// convert via a custom overload.
/// </para>
/// </remarks>
public static class FluentValidationResultExtensions
{
    /// <summary>
    /// Converts a <see cref="ValidationResult"/> to a non-generic <see cref="Result"/>.
    /// Returns <see cref="Result.Success()"/> if valid, or a structured validation failure if not.
    /// </summary>
    /// <param name="validationResult">The FluentValidation result to convert.</param>
    /// <returns>A <see cref="Result"/> representing the validation outcome.</returns>
    /// <example>
    /// <code>
    /// var validator = new OrderValidator();
    /// var result = validator.Validate(order).ToValidationResult();
    ///
    /// return result.Match(
    ///     onSuccess: () => "Valid",
    ///     onFailure: error => $"Invalid: {error.Description}"
    /// );
    /// </code>
    /// </example>
    public static Result ToValidationResult(this ValidationResult validationResult)
    {
        if (validationResult.IsValid)
            return Result.Success();

        return Result.Failure(CreateValidationError(validationResult));
    }

    /// <summary>
    /// Converts a <see cref="ValidationResult"/> to a <see cref="Result{T}"/>.
    /// Returns <see cref="Result.Success{T}(T)"/> with the provided value if valid,
    /// or a structured validation failure if not.
    /// </summary>
    /// <typeparam name="T">The type of the value to wrap on success.</typeparam>
    /// <param name="validationResult">The FluentValidation result to convert.</param>
    /// <param name="value">The value to wrap in the result if validation succeeds.</param>
    /// <returns>A <see cref="Result{T}"/> representing the validation outcome.</returns>
    /// <example>
    /// <code>
    /// var validator = new OrderValidator();
    /// var result = validator.Validate(order).ToValidationResult(order);
    ///
    /// return result.Map(o => ProcessOrder(o));
    /// </code>
    /// </example>
    public static Result<T> ToValidationResult<T>(this ValidationResult validationResult, T value)
    {
        if (validationResult.IsValid)
            return Result.Success(value);

        return Result.Failure<T>(CreateValidationError(validationResult));
    }

    /// <summary>
    /// Validates an object using the provided validator and returns a <see cref="Result"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to validate.</typeparam>
    /// <param name="validator">The FluentValidation validator.</param>
    /// <param name="instance">The object to validate.</param>
    /// <returns>A <see cref="Result"/> representing the validation outcome.</returns>
    /// <remarks>
    /// Named <c>ValidateToResult</c> instead of <c>Validate</c> to avoid method resolution
    /// ambiguity with FluentValidation's own <c>IValidator&lt;T&gt;.Validate(T)</c> when both
    /// the <c>FluentValidation</c> and <c>EricksonLopez.Result.FluentValidation</c> namespaces
    /// are imported.
    /// </remarks>
    public static Result ValidateToResult<T>(this global::FluentValidation.IValidator<T> validator, T instance)
    {
        var validationResult = validator.Validate(instance);
        return validationResult.ToValidationResult();
    }

    /// <summary>
    /// Validates an object using the provided validator and returns a <see cref="Result{T}"/>
    /// wrapping the validated instance on success.
    /// </summary>
    /// <typeparam name="T">The type of the object to validate.</typeparam>
    /// <param name="validator">The FluentValidation validator.</param>
    /// <param name="instance">The object to validate.</param>
    public static Result<T> ValidateToResultWithValue<T>(this global::FluentValidation.IValidator<T> validator, T instance)
    {
        var validationResult = validator.Validate(instance);
        return validationResult.ToValidationResult(instance);
    }

    /// <summary>
    /// Validates an object asynchronously and returns a <see cref="Result"/>.
    /// </summary>
    /// <remarks>
    /// Named <c>ValidateToResultAsync</c> instead of <c>ValidateAsync</c> to avoid method resolution
    /// ambiguity with FluentValidation's own <c>IValidator&lt;T&gt;.ValidateAsync(T)</c>.
    /// </remarks>
    public static async Task<Result> ValidateToResultAsync<T>(
        this global::FluentValidation.IValidator<T> validator,
        T instance,
        System.Threading.CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(instance, cancellationToken).ConfigureAwait(false);
        return validationResult.ToValidationResult();
    }

    /// <summary>
    /// Validates an object asynchronously and returns a <see cref="Result{T}"/>
    /// wrapping the validated instance on success.
    /// </summary>
    public static async Task<Result<T>> ValidateToResultWithValueAsync<T>(
        this global::FluentValidation.IValidator<T> validator,
        T instance,
        System.Threading.CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(instance, cancellationToken).ConfigureAwait(false);
        return validationResult.ToValidationResult(instance);
    }

    /// <summary>
    /// Integrates validation into a Result pipeline. Validates the value of a successful
    /// <see cref="Result{T}"/> and returns a validation failure if invalid.
    /// </summary>
    /// <typeparam name="T">The type of the value to validate.</typeparam>
    /// <param name="result">The result containing the value to validate.</param>
    /// <param name="validator">The FluentValidation validator.</param>
    /// <returns>The original result if valid, or a validation failure result.</returns>
    /// <example>
    /// <code>
    /// var result = await GetOrderAsync()
    ///     .EnsureValid(new OrderValidator())
    ///     .Map(order => ProcessOrder(order));
    /// </code>
    /// </example>
    public static Result<T> EnsureValid<T>(this in Result<T> result, global::FluentValidation.IValidator<T> validator)
    {
        if (result.IsUninitialized) throw new System.InvalidOperationException("Cannot operate on an uninitialized default Result<TValue>. Always construct Result<TValue> via Result.Success(value) or Result.Failure(error).");
        if (result.IsFailure) return result;

        var validationResult = validator.Validate(result.Value);
        if (validationResult.IsValid) return result;

        return Result.Failure<T>(CreateValidationError(validationResult));
    }

    /// <summary>
    /// Async version: Integrates validation into a Result pipeline.
    /// </summary>
    public static async Task<Result<T>> EnsureValidAsync<T>(
        this Task<Result<T>> resultTask,
        global::FluentValidation.IValidator<T> validator,
        System.Threading.CancellationToken cancellationToken = default)
    {
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsUninitialized) throw new System.InvalidOperationException("Cannot operate on an uninitialized default Result<TValue>. Always construct Result<TValue> via Result.Success(value) or Result.Failure(error).");
        if (result.IsFailure) return result;

        var validationResult = await validator.ValidateAsync(result.Value, cancellationToken).ConfigureAwait(false);
        if (validationResult.IsValid) return result;

        return Result.Failure<T>(CreateValidationError(validationResult));
    }

    // --- Internal helpers ------------------------------------------------------

    private static Error CreateValidationError(ValidationResult validationResult)
    {
        var failures = validationResult.Errors;
        var innerErrors = new Error[failures.Count];

        for (int i = 0; i < failures.Count; i++)
        {
            var failure = failures[i];
            var errorCode = !string.IsNullOrWhiteSpace(failure.ErrorCode)
                ? failure.ErrorCode
                : $"Validation.{failure.PropertyName}";

            var builder = Error.Create(errorCode, failure.ErrorMessage)
                .WithType(ErrorType.Validation)
                .WithSeverity(MapSeverity(failure.Severity))
                .WithMetadata("propertyName", failure.PropertyName);

            if (failure.AttemptedValue is not null)
            {
                builder = builder.WithMetadata("attemptedValue", failure.AttemptedValue);
            }

            if (failure.FormattedMessagePlaceholderValues != null)
            {
                foreach (var kvp in failure.FormattedMessagePlaceholderValues)
                {
                    if (kvp.Value is not null && kvp.Key is not ("PropertyName" or "PropertyValue"))
                    {
                        builder = builder.WithMetadata($"placeholder.{kvp.Key}", kvp.Value);
                    }
                }
            }

            innerErrors[i] = builder.Build();
        }

        return Error.Create(
                "Validation.Failed",
                $"{failures.Count} validation error(s) occurred.")
            .WithType(ErrorType.Validation)
            .WithSeverity(ErrorSeverity.Warning)
            .WithInnerErrors(innerErrors)
            .Build();
    }

    private static ErrorSeverity MapSeverity(global::FluentValidation.Severity severity) => severity switch
    {
        global::FluentValidation.Severity.Error => ErrorSeverity.Error,
        global::FluentValidation.Severity.Warning => ErrorSeverity.Warning,
        global::FluentValidation.Severity.Info => ErrorSeverity.Info,
        _ => ErrorSeverity.Error
    };
}
