// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.Result;

/// <summary>Specifies whether an operation associated with an error can be retried.</summary>
public enum ErrorRetryability : byte
{
    /// <summary>Specifies that retry semantics do not apply to this error.</summary>
    NotApplicable = 0,
    /// <summary>Specifies a temporary failure that may succeed if retried.</summary>
    Transient = 1,
    /// <summary>Specifies a deterministic failure that will not succeed upon retry.</summary>
    Permanent = 2
}
