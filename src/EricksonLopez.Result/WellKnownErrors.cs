namespace EricksonLopez.Result;

/// <summary>
/// Contains well-known error codes and system error constants used by the Result framework.
/// </summary>
public static class WellKnownErrors
{
    /// <summary>
    /// Error code used when multiple results are combined and one or more failures occur.
    /// </summary>
    public const string CombinedFailuresCode = "Result.CombinedErrors";

    /// <summary>
    /// Error instance used when an uninitialized result is accessed.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="Error.CreateSentinel"/> to avoid capturing <see cref="System.Diagnostics.Activity.Current"/>
    /// at static initialization time, and to ensure <see cref="Error.TraceId"/> serializes as
    /// <see langword="null"/> rather than an empty string.
    /// </remarks>
    public static readonly Error UninitializedError = Error.CreateSentinel(
        "Result.Uninitialized",
        "Cannot access an uninitialized default Result.",
        ErrorType.Unexpected,
        ErrorSeverity.Critical,
        ErrorRetryability.NotApplicable);
}
