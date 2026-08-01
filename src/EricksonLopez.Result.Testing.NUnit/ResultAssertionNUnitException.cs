using System;
using NUnit.Framework;

namespace EricksonLopez.Result.Testing.NUnit;

/// <summary>
/// Exception thrown when a Result assertion fails, using NUnit's <see cref="AssertionException"/>
/// as the base class so that failures appear as <b>Failed</b> in NUnit's
/// test output — producing clean assertion messages.
/// </summary>
public sealed class ResultAssertionNUnitException : AssertionException
{
    /// <summary>
    /// Initializes a new instance of <see cref="ResultAssertionNUnitException"/> with no message.
    /// </summary>
    public ResultAssertionNUnitException()
        : base("A Result assertion failed.")
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ResultAssertionNUnitException"/> with a message.
    /// </summary>
    /// <param name="message">The error message that explains the assertion failure.</param>
    public ResultAssertionNUnitException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ResultAssertionNUnitException"/> from an existing
    /// <see cref="ResultAssertionException"/>, preserving the original message.
    /// </summary>
    /// <param name="inner">The framework-agnostic assertion exception to wrap.</param>
    public ResultAssertionNUnitException(ResultAssertionException inner)
        : base(inner?.Message ?? "A Result assertion failed.", inner)
    {
    }
}
