using System;
using EricksonLopez.Result.Testing;

namespace EricksonLopez.Result.Testing.NUnit;

/// <summary>
/// Configuration entry point for integrating <see cref="ResultAssertions"/> with NUnit.
/// </summary>
/// <remarks>
/// <para>
/// By default, assertion failures thrown by <c>EricksonLopez.Result.Testing</c> derive from
/// <see cref="Exception"/> and appear as <b>Errors</b> in NUnit output. Calling
/// <see cref="UseNUnitExceptions"/> redirects exception creation to
/// <see cref="ResultAssertionNUnitException"/>, which derives from <c>AssertionException</c>
/// and displays as a clean <b>Failed</b> with only the assertion message.
/// </para>
/// </remarks>
public static class ResultNUnitAssertionConfig
{
    // Use int for Interlocked.CompareExchange compatibility.
    // 0 = not configured, 1 = configured.
    private static volatile int _configured;

    /// <summary>
    /// Automatically configures <see cref="ResultAssertions"/> to use NUnit-native exceptions
    /// when this assembly is loaded. This is called automatically via the
    /// <see cref="System.Runtime.CompilerServices.ModuleInitializerAttribute"/> — no manual
    /// setup is required.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage", "CA2255",
        Justification = "ModuleInitializer is intentionally used here to auto-register NUnit exception factory " +
                        "when the EricksonLopez.Result.Testing.NUnit assembly is loaded into a test project.")]
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void AutoConfigure() => UseNUnitExceptions();

    /// <summary>
    /// Configures <see cref="ResultAssertions"/> to throw <see cref="ResultAssertionNUnitException"/>
    /// instead of the default <see cref="ResultAssertionException"/>. Safe to call multiple times.
    /// </summary>
    public static void UseNUnitExceptions()
    {
        if (System.Threading.Interlocked.CompareExchange(ref _configured, 1, 0) != 0) return;
        ResultAssertionException.ExceptionFactory =
            static message => new ResultAssertionNUnitException(message);
    }

    /// <summary>
    /// Resets the exception factory to the default. Intended for testing only.
    /// </summary>
    internal static void Reset()
    {
        System.Threading.Interlocked.Exchange(ref _configured, 0);
        ResultAssertionException.ExceptionFactory = static msg => new ResultAssertionException(msg);
    }
}
