// Copyright © Erickson Lopez. MIT License.
namespace EricksonLopez.Result;

/// <summary>
/// Defines the foundational contract for all Result outcomes.
/// Named IResultOutcome to avoid name collision with Microsoft.AspNetCore.Http.IResult.
/// </summary>
public interface IResultOutcome
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    bool IsFailure { get; }

    /// <summary>
    /// Gets a value indicating whether this result is an uninitialized default value
    /// (i.e., constructed via <c>default(Result)</c> or <c>default(Result&lt;T&gt;)</c>
    /// instead of through <c>Result.Success()</c> or <c>Result.Failure(Error)</c>).
    /// </summary>
    /// <remarks>
    /// An uninitialized result is neither <see cref="IsSuccess"/> nor <see cref="IsFailure"/>.
    /// Code that receives an <see cref="IResultOutcome"/> should check this property when
    /// handling all possible states, particularly in middleware or endpoint filters.
    /// </remarks>
    bool IsUninitialized { get; }

    /// <summary>Gets the error associated with a failed result, or <see langword="null"/> if successful.</summary>
    Error? Error { get; }

    /// <summary>Gets the raw underlying value of a successful generic result, or <see langword="null"/> if non-generic or failed.</summary>
    object? RawValue { get; }
}

