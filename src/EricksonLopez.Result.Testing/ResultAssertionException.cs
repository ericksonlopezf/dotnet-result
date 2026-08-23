// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Testing;

/// <summary>
/// Exception thrown when a Result assertion fails.
/// </summary>
/// <remarks>
/// <para>
/// This exception inherits from <see cref="Exception"/> rather than a test framework-specific
/// assertion exception (e.g., xUnit's <c>XunitException</c>) to avoid coupling to a specific
/// test framework. Test runners such as xUnit, NUnit, and MSTest will display this as a test
/// failure in the output, though it may appear in the "Error" column rather than "Failure" for
/// some runners.
/// </para>
/// <para>
/// <b>Framework integration:</b> To use xUnit-native exceptions (which show as "Failure" instead
/// of "Error" in xUnit output), install the <c>EricksonLopez.Result.Testing.XUnit</c> package
/// and call <c>ResultXUnitAssertionConfig.UseXUnitExceptions()</c> in your test assembly setup.
/// </para>
/// <para>
/// CA1032: All standard Exception constructors are implemented.
/// </para>
/// </remarks>
public class ResultAssertionException : Exception
{
    // ─── Configurable exception factory ───────────────────────────────────────
    // This factory is used by all ResultAssertions methods to create assertion exceptions.
    // It can be replaced by test framework adapters (e.g., ResultXUnitAssertionConfig)
    // to produce framework-native assertion exceptions that display as "Failure" rather than
    // "Error" in test output. Default: creates a plain ResultAssertionException.
    //
    // Thread safety: accessed via Interlocked.Exchange/CompareExchange to ensure atomic
    // read/write semantics. In parallel test suites that configure different exception
    // factories concurrently, each thread sees a consistent factory reference.
    private static Func<string, Exception> _factory = static message => new ResultAssertionException(message);

    /// <summary>
    /// Gets or sets the factory function used to create assertion exceptions.
    /// Override this to produce framework-native exceptions (e.g., xUnit's XunitException).
    /// Thread-safe: uses <see cref="Interlocked"/> for atomic access.
    /// </summary>
    /// <exception cref="ArgumentNullException">The assigned value is <see langword="null"/></exception>
    /// <remarks>
    /// Do not call this directly. Use <c>ResultXUnitAssertionConfig.UseXUnitExceptions()</c> from
    /// the <c>EricksonLopez.Result.Testing.XUnit</c> package, or the equivalent for your test framework.
    /// </remarks>
    public static Func<string, Exception> ExceptionFactory
    {
        get => Volatile.Read(ref _factory);
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            // Stryker disable once all : Framework thread-safety implementation
            Interlocked.Exchange(ref _factory, value);
        }
    }

    /// <summary>
    /// Creates and returns an assertion exception using the configured <see cref="ExceptionFactory"/>.
    /// All ResultAssertions methods call this instead of <see langword="new"/> so that
    /// framework adapters (xUnit, NUnit) can intercept exception creation.
    /// </summary>
    /// <remarks>
    /// If the factory produces a non-<see cref="ResultAssertionException"/> (e.g., an XunitException),
    /// this method throws it directly since it cannot be returned as a <see cref="ResultAssertionException"/>.
    /// Call sites must use the returned exception in a <c>throw</c> statement.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    internal static Exception Throw(string message) => throw _factory(message);

    /// <summary>
    /// Initializes a new instance of <see cref="ResultAssertionException"/> with no message.
    /// </summary>
    public ResultAssertionException()
        : base("A Result assertion failed.")
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ResultAssertionException"/> with a message.
    /// </summary>
    /// <param name="message">The error message that explains the assertion failure.</param>
    public ResultAssertionException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ResultAssertionException"/> with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the assertion failure.</param>
    /// <param name="innerException">The exception that caused this assertion failure.</param>
    public ResultAssertionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}





