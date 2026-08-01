using EricksonLopez.Result;

namespace EricksonLopez.Result.AspNetCore;

/// <summary>
/// DTO representing an error detail for ProblemDetails payload (NativeAOT compliant).
/// Enum values are serialized as stable string constants to avoid naming instability across versions.
/// </summary>
/// <remarks>
/// <para>
/// This struct is registered in <see cref="AspNetCoreJsonSerializerContext"/> for AOT-safe serialization.
/// It is used internally to populate the <c>errors</c> array in RFC 9457 ProblemDetails extensions.
/// </para>
/// <para>
/// Properties:
/// <list type="bullet">
///   <item><description><c>Code</c> — application error code (e.g., <c>"ORDER.EXPIRED"</c>).</description></item>
///   <item><description><c>Description</c> — human-readable error message.</description></item>
///   <item><description><c>Type</c> — <see cref="ErrorType"/> as a PascalCase string (e.g., <c>"Validation"</c>).</description></item>
///   <item><description><c>Severity</c> — <see cref="ErrorSeverity"/> as a PascalCase string (e.g., <c>"Error"</c>).</description></item>
///   <item><description><c>Retryability</c> — <see cref="ErrorRetryability"/> as a PascalCase string (e.g., <c>"Transient"</c>).</description></item>
///   <item><description><c>DescriptionKey</c> — optional localization key, or <see langword="null"/> if not set.</description></item>
///   <item><description><c>TraceId</c> — optional OpenTelemetry trace ID, or <see langword="null"/> if not set.</description></item>
/// </list>
/// </para>
/// </remarks>
public readonly record struct ErrorDetailDto(
    string Code,
    string Description,
    string Type,
    string Severity,
    string Retryability,
    string? DescriptionKey,
    string? TraceId);
