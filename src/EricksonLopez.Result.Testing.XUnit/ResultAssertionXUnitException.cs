// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;
using Xunit.Sdk;

namespace EricksonLopez.Result.Testing.XUnit;

/// <summary>
/// Exception thrown when a Result assertion fails, using xUnit's <see cref="XunitException"/>
/// as the base class so that failures appear as <b>Failure</b> (not <b>Error</b>) in xUnit's
/// test output — producing clean assertion messages without unnecessary stack trace noise.
/// </summary>
/// <remarks>
/// <para>
/// xUnit v2 and v3 differentiate between two kinds of test-ending exceptions:
/// <list type="bullet">
///   <item><b>Failures</b>: Exceptions that derive from <see cref="XunitException"/>. These display
///   with only the assertion message — no stack trace from the assertion library internals.
///   This is the expected behavior for assertion failures (e.g., from FluentAssertions, Shouldly).</item>
///   <item><b>Errors</b>: Any other exception. These display with the full stack trace, making it
///   harder to see what went wrong in the test. The base <see cref="EricksonLopez.Result.Testing.ResultAssertionException"/>
///   inherits from <see cref="Exception"/> and therefore shows as an Error in xUnit.</item>
/// </list>
/// </para>
/// <para>
/// Register this exception type in your xUnit test project by calling
/// <c>ResultXUnitAssertionConfig.UseXUnitExceptions()</c> in your test assembly setup,
/// or by using the <c>EricksonLopez.Result.Testing.XUnit</c> package and directly throwing this
/// exception in custom assertion helpers.
/// </para>
/// <para>
/// <b>Framework independence</b>: This class is in a separate package so that
/// <c>EricksonLopez.Result.Testing</c> remains framework-agnostic (no xUnit dependency).
/// NUnit and MSTest users should use <c>EricksonLopez.Result.Testing</c> directly.
/// </para>
/// </remarks>
public sealed class ResultAssertionXUnitException : XunitException
{
    /// <summary>
    /// Initializes a new instance of <see cref="ResultAssertionXUnitException"/> with no message.
    /// </summary>
    public ResultAssertionXUnitException()
        : base("A Result assertion failed.")
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ResultAssertionXUnitException"/> with a message.
    /// </summary>
    /// <param name="message">The error message that explains the assertion failure.</param>
    public ResultAssertionXUnitException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ResultAssertionXUnitException"/> from an existing
    /// <see cref="ResultAssertionException"/>, preserving the original message.
    /// </summary>
    /// <param name="inner">The framework-agnostic assertion exception to wrap.</param>
    public ResultAssertionXUnitException(ResultAssertionException inner)
        : base(inner?.Message ?? "A Result assertion failed.", inner)
    {
    }
}


