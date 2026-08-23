// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.Result;

/// <summary>Specifies the severity level of an error.</summary>
public enum ErrorSeverity : byte
{
    /// <summary>Specifies an informational event that does not disrupt execution.</summary>
    Info = 0,
    /// <summary>Specifies a non-critical condition or expected failure.</summary>
    Warning = 1,
    /// <summary>Specifies a standard operational error or business failure.</summary>
    Error = 2,
    /// <summary>Specifies a catastrophic failure requiring urgent intervention.</summary>
    Critical = 3
}
