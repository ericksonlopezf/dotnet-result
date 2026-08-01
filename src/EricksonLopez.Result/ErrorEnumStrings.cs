namespace EricksonLopez.Result;

/// <summary>
/// Provides stable, AOT-safe string representations of error-related enums.
/// Used internally by Serialization, AspNetCore, and OpenTelemetry packages to ensure
/// consistent enum formatting without duplicating the switch expressions.
/// </summary>
/// <remarks>
/// These methods use switch expressions that produce compile-time constant string references.
/// They are AOT-safe: no <c>Enum.ToString()</c> or reflection is used.
/// The string values are stable across versions and safe for wire-format serialization.
/// </remarks>
internal static class ErrorEnumStrings
{
    /// <summary>
    /// Returns the PascalCase string for an <see cref="ErrorType"/> value.
    /// Used for JSON serialization and ProblemDetails extensions.
    /// </summary>
    internal static string ErrorTypeToString(ErrorType type) => type switch
    {
        ErrorType.Failure => "Failure",
        ErrorType.Validation => "Validation",
        ErrorType.NotFound => "NotFound",
        ErrorType.Conflict => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.Unavailable => "Unavailable",
        ErrorType.Unexpected => "Unexpected",
        ErrorType.Domain => "Domain",
        ErrorType.Infrastructure => "Infrastructure",
        ErrorType.Custom => "Custom",
        _ => "Failure"
    };

    /// <summary>
    /// Returns the PascalCase string for an <see cref="ErrorSeverity"/> value.
    /// Used for JSON serialization and ProblemDetails extensions.
    /// </summary>
    internal static string ErrorSeverityToString(ErrorSeverity severity) => severity switch
    {
        ErrorSeverity.Info => "Info",
        ErrorSeverity.Warning => "Warning",
        ErrorSeverity.Error => "Error",
        ErrorSeverity.Critical => "Critical",
        _ => "Error"
    };

    /// <summary>
    /// Returns the PascalCase string for an <see cref="ErrorRetryability"/> value.
    /// Used for JSON serialization and ProblemDetails extensions.
    /// </summary>
    internal static string ErrorRetryabilityToString(ErrorRetryability retryability) => retryability switch
    {
        ErrorRetryability.NotApplicable => "NotApplicable",
        ErrorRetryability.Transient => "Transient",
        ErrorRetryability.Permanent => "Permanent",
        _ => "NotApplicable"
    };

    /// <summary>
    /// Returns the lowercase OTel-convention string for an <see cref="ErrorType"/> value.
    /// Used for OpenTelemetry <c>error.type</c> attribute values.
    /// </summary>
    internal static string ErrorTypeToOTelString(ErrorType type) => type switch
    {
        ErrorType.Failure => "failure",
        ErrorType.Validation => "validation",
        ErrorType.NotFound => "not_found",
        ErrorType.Conflict => "conflict",
        ErrorType.Unauthorized => "unauthorized",
        ErrorType.Forbidden => "forbidden",
        ErrorType.Unavailable => "unavailable",
        ErrorType.Unexpected => "unexpected",
        ErrorType.Domain => "domain",
        ErrorType.Infrastructure => "infrastructure",
        ErrorType.Custom => "custom",
        _ => "_OTHER"  // OTel convention for unknown error types
    };

    /// <summary>
    /// Returns the lowercase OTel-convention string for an <see cref="ErrorSeverity"/> value.
    /// Used for OpenTelemetry activity tag values.
    /// </summary>
    internal static string ErrorSeverityToOTelString(ErrorSeverity severity) => severity switch
    {
        ErrorSeverity.Info => "info",
        ErrorSeverity.Warning => "warning",
        ErrorSeverity.Error => "error",
        ErrorSeverity.Critical => "critical",
        _ => "error"
    };
}
