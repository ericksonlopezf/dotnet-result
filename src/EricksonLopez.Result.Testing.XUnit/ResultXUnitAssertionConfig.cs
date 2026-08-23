// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;

namespace EricksonLopez.Result.Testing.XUnit;

/// <summary>
/// Configuration entry point for integrating <see cref="ResultAssertions"/> with xUnit v2 and v3.
/// </summary>
/// <remarks>
/// <para>
/// By default, assertion failures thrown by <c>EricksonLopez.Result.Testing</c> derive from
/// <see cref="Exception"/> and appear as <b>Errors</b> in xUnit output. Calling
/// <see cref="UseXUnitExceptions"/> redirects exception creation to
/// <see cref="ResultAssertionXUnitException"/>, which derives from <c>XunitException</c>
/// and displays as a clean <b>Failure</b> with only the assertion message.
/// </para>
/// <para>
/// <b>Usage</b> — call once during test setup, typically via a module initializer:
/// <code>
/// internal static class TestAssemblySetup
/// {
///     [System.Runtime.CompilerServices.ModuleInitializer]
///     internal static void Initialize() => ResultXUnitAssertionConfig.UseXUnitExceptions();
/// }
/// </code>
/// </para>
/// </remarks>
public static class ResultXUnitAssertionConfig
{
    // Use int for Interlocked.CompareExchange compatibility.
    // 0 = not configured, 1 = configured.
    private static volatile int _configured;

    /// <summary>
    /// Automatically configures <see cref="ResultAssertions"/> to use xUnit-native exceptions
    /// when this assembly is loaded. This is called automatically via the
    /// <see cref="System.Runtime.CompilerServices.ModuleInitializerAttribute"/> — no manual
    /// setup is required.
    /// </summary>
    /// <remarks>
    /// The <see cref="UseXUnitExceptions"/> method is still available for explicit configuration
    /// (e.g., to re-configure after a <see cref="Reset"/> in tests of this library itself).
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage", "CA2255",
        Justification = "ModuleInitializer is intentionally used here to auto-register xUnit exception factory " +
                        "when the EricksonLopez.Result.Testing.XUnit assembly is loaded into a test project. " +
                        "This is a well-known library pattern (also used by MSTest, TestContainers, etc.) that " +
                        "eliminates the need for consumer setup code. CA2255 is advisory for application code.")]
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void AutoConfigure() => UseXUnitExceptions();

    /// <summary>
    /// Configures <see cref="ResultAssertions"/> to throw <see cref="ResultAssertionXUnitException"/>
    /// instead of the default <see cref="ResultAssertionException"/>. Safe to call multiple times.
    /// </summary>
    /// <remarks>
    /// This method is called automatically via <see cref="AutoConfigure"/> when the assembly loads.
    /// You only need to call this explicitly if you have called <see cref="Reset"/> and want to
    /// re-enable xUnit exception mode.
    /// <para>
    /// <b>Thread safety:</b> Uses <see cref="Interlocked.CompareExchange(ref int, int, int)"/>
    /// to atomically check-and-set the configured flag, eliminating the TOCTOU race condition that
    /// would exist with a plain read-then-write pattern.
    /// </para>
    /// </remarks>
    public static void UseXUnitExceptions()
    {
        // Atomically transition from 0 (not configured) to 1 (configured).
        // If CompareExchange returns 0, we were the first to configure — proceed.
        // If it returns 1, another thread already configured — return immediately.
        if (Interlocked.CompareExchange(ref _configured, 1, 0) != 0) return;
        ResultAssertionException.ExceptionFactory =
            static message => new ResultAssertionXUnitException(message);
    }

    /// <summary>
    /// Resets the exception factory to the default. Intended for testing only.
    /// </summary>
    internal static void Reset()
    {
        Interlocked.Exchange(ref _configured, 0);
        ResultAssertionException.ExceptionFactory = static msg => new ResultAssertionException(msg);
    }
}



