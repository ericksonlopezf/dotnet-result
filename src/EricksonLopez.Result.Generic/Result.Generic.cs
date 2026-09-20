// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Generic;

/// <summary>
/// Represents a result of an operation that can succeed with a typed value <typeparamref name="TValue"/>
/// or fail with a strongly-typed domain error <typeparamref name="TError"/>.
/// </summary>
/// <typeparam name="TValue">The success value type.</typeparam>
/// <typeparam name="TError">The strongly-typed error type.</typeparam>
[StructLayout(LayoutKind.Auto)]
public readonly struct Result<TValue, TError> : IEquatable<Result<TValue, TError>>
    where TError : class
{
    private enum State : byte
    {
        Uninitialized = 0,
        Success = 1,
        Failure = 2
    }

    private readonly State _state;
    private readonly TValue _value;
    private readonly TError? _error;

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess => _state == State.Success;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => _state == State.Failure;

    /// <summary>
    /// Gets a value indicating whether the struct is an uninitialized default value.
    /// </summary>
    public bool IsUninitialized => _state == State.Uninitialized;

    /// <summary>
    /// Gets the success value.
    /// </summary>
    /// <exception cref="InvalidOperationException">The result is a failure or uninitialized</exception>
    public TValue Value
    {
        get
        {
            if (_state == State.Success)
                return _value!;
            if (_state == State.Failure)
                throw new InvalidOperationException($"Cannot access Value on a failure result. Error: {_error}");
            throw new InvalidOperationException("Cannot access Value on an uninitialized default Result<TValue, TError>.");
        }
    }

    /// <summary>
    /// Gets the strongly-typed failure error.
    /// </summary>
    /// <exception cref="InvalidOperationException">The result is a success or uninitialized</exception>
    public TError Error
    {
        get
        {
            if (_state == State.Failure)
                return _error!;
            if (_state == State.Success)
                throw new InvalidOperationException("Cannot access Error on a success result.");
            throw new InvalidOperationException("Cannot access Error on an uninitialized default Result<TValue, TError>.");
        }
    }

    private Result(TValue value)
    {
        _state = State.Success;
        _value = value;
        _error = null;
    }

    private Result(TError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _state = State.Failure;
        _value = default!;
        _error = error;
    }

    /// <summary>
    /// Creates a successful result containing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The success value to wrap in the result.</param>
    /// <returns>A successful <see cref="Result{TValue, TError}"/> instance containing <paramref name="value"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TValue, TError> Success(TValue value) => new(value);

    /// <summary>
    /// Creates a failure result containing <paramref name="error"/>.
    /// </summary>
    /// <param name="error">The strongly-typed error to wrap in the failure result.</param>
    /// <returns>A failed <see cref="Result{TValue, TError}"/> instance containing <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/></exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TValue, TError> Failure(TError error) => new(error);

    /// <summary>
    /// Attempts to retrieve the success value.
    /// </summary>
    /// <param name="value">When this method returns, contains the success value if the operation succeeded; otherwise, the default value.</param>
    /// <returns><see langword="true"/> if the operation succeeded and <paramref name="value"/> is populated; otherwise, <see langword="false"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
    {
        value = _value;
        return _state == State.Success;
    }

    /// <summary>
    /// Attempts to retrieve the failure error.
    /// </summary>
    /// <param name="error">When this method returns, contains the error if the operation failed; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the operation failed and <paramref name="error"/> is populated; otherwise, <see langword="false"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetError([MaybeNullWhen(false)] out TError error)
    {
        error = _error;
        return _state == State.Failure;
    }

    /// <summary>
    /// Projects the success value using <paramref name="mapper"/>.
    /// </summary>
    /// <typeparam name="TNext">The return type of the mapper function.</typeparam>
    /// <param name="mapper">The projection function applied to the success value.</param>
    /// <returns>A new <see cref="Result{TNext, TError}"/> containing the projected value on success, or the original error on failure.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mapper"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">The result is uninitialized</exception>
    [Pure]
    public Result<TNext, TError> Map<TNext>(Func<TValue, TNext> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        if (_state == State.Uninitialized)
            throw new InvalidOperationException("Cannot map on an uninitialized default Result<TValue, TError>.");
        return _state == State.Success ? Result<TNext, TError>.Success(mapper(_value!)) : Result<TNext, TError>.Failure(_error!);
    }

    /// <summary>
    /// Projects the error type using <paramref name="errorMapper"/>.
    /// </summary>
    /// <typeparam name="TNextError">The target strongly-typed error type.</typeparam>
    /// <param name="errorMapper">The projection function applied to the failure error.</param>
    /// <returns>A new <see cref="Result{TValue, TNextError}"/> containing the original value on success, or the projected error on failure.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errorMapper"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">The result is uninitialized</exception>
    [Pure]
    public Result<TValue, TNextError> MapError<TNextError>(Func<TError, TNextError> errorMapper)
        where TNextError : class
    {
        ArgumentNullException.ThrowIfNull(errorMapper);
        if (_state == State.Uninitialized)
            throw new InvalidOperationException("Cannot map error on an uninitialized default Result<TValue, TError>.");
        return _state == State.Success ? Result<TValue, TNextError>.Success(_value!) : Result<TValue, TNextError>.Failure(errorMapper(_error!));
    }

    /// <summary>
    /// Binds the success value to a new <see cref="Result{TNext, TError}"/>.
    /// </summary>
    /// <typeparam name="TNext">The value type of the chained result.</typeparam>
    /// <param name="bind">The operation to execute with the success value.</param>
    /// <returns>The result of executing <paramref name="bind"/> on success; otherwise, a failure with the original error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bind"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">The result is uninitialized</exception>
    [Pure]
    public Result<TNext, TError> Bind<TNext>(Func<TValue, Result<TNext, TError>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        if (_state == State.Uninitialized)
            throw new InvalidOperationException("Cannot bind on an uninitialized default Result<TValue, TError>.");
        return _state == State.Success ? bind(_value!) : Result<TNext, TError>.Failure(_error!);
    }

    /// <summary>
    /// Matches either branch depending on success or failure.
    /// </summary>
    /// <typeparam name="TOut">The return type produced by the matching functions.</typeparam>
    /// <param name="onSuccess">The function to evaluate with the value if the result is successful.</param>
    /// <param name="onFailure">The function to evaluate with the error if the result is a failure.</param>
    /// <returns>The value produced by either <paramref name="onSuccess"/> or <paramref name="onFailure"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="onSuccess"/> or <paramref name="onFailure"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">The result is uninitialized</exception>
    [Pure]
    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<TError, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        if (_state == State.Uninitialized)
            throw new InvalidOperationException("Cannot match on an uninitialized default Result<TValue, TError>.");
        return _state == State.Success ? onSuccess(_value!) : onFailure(_error!);
    }

    /// <summary>
    /// Converts this strongly-typed result to the standard <see cref="EricksonLopez.Result.Result{TValue}"/>
    /// by mapping the generic <typeparamref name="TError"/> to <see cref="EricksonLopez.Result.Error"/>.
    /// </summary>
    /// <param name="errorMapper">The mapping function that converts <typeparamref name="TError"/> to <see cref="EricksonLopez.Result.Error"/>.</param>
    /// <returns>A standard <see cref="EricksonLopez.Result.Result{TValue}"/> representing the outcome.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errorMapper"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">The result is uninitialized</exception>
    [Pure]
    public EricksonLopez.Result.Result<TValue> ToResult(Func<TError, EricksonLopez.Result.Error> errorMapper)
    {
        ArgumentNullException.ThrowIfNull(errorMapper);
        if (_state == State.Uninitialized)
            throw new InvalidOperationException("Cannot convert an uninitialized default Result<TValue, TError>.");
        return _state == State.Success
            ? EricksonLopez.Result.Result.Success(_value!)
            : EricksonLopez.Result.Result.Failure<TValue>(errorMapper(_error!));
    }

    /// <summary>Converts a value implicitly into a successful <see cref="Result{TValue, TError}"/>.</summary>
    /// <param name="value">The value to wrap in a successful result.</param>
    public static implicit operator Result<TValue, TError>(TValue value) => Success(value);

    /// <summary>Converts a strongly-typed error implicitly into a failed <see cref="Result{TValue, TError}"/>.</summary>
    /// <param name="error">The strongly-typed error to wrap in a failed result.</param>
    public static implicit operator Result<TValue, TError>(TError error) => Failure(error);

    /// <summary>Determines whether this result is equal to another result.</summary>
    /// <param name="other">The other result to compare against.</param>
    /// <returns><see langword="true"/> if both results represent the same outcome; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Result<TValue, TError> other)
    {
        if (_state != other._state) return false;
        return _state switch
        {
            State.Success => EqualityComparer<TValue>.Default.Equals(_value!, other._value!),
            State.Failure => EqualityComparer<TError>.Default.Equals(_error!, other._error!),
            _ => true
        };
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Result<TValue, TError> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _state switch
        {
            State.Success => HashCode.Combine(true, _value is null ? 0 : EqualityComparer<TValue>.Default.GetHashCode(_value)),
            State.Failure => HashCode.Combine(false, _error is null ? 0 : EqualityComparer<TError>.Default.GetHashCode(_error)),
            _ => 0
        };
    }

    /// <summary>Determines whether two <see cref="Result{TValue, TError}"/> instances are equal.</summary>
    /// <param name="left">The first result to compare.</param>
    /// <param name="right">The second result to compare.</param>
    /// <returns><see langword="true"/> if both instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Result<TValue, TError> left, Result<TValue, TError> right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Result{TValue, TError}"/> instances are not equal.</summary>
    /// <param name="left">The first result to compare.</param>
    /// <param name="right">The second result to compare.</param>
    /// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Result<TValue, TError> left, Result<TValue, TError> right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => _state switch
    {
        State.Success => $"Success({_value})",
        State.Failure => $"Failure({_error})",
        _ => "Uninitialized"
    };
}
