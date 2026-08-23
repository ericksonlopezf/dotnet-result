// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.Result;

/// <summary>Specifies the classification category and functional scope of an error.</summary>
public enum ErrorType : byte
{
    /// <summary>Specifies a general unclassified failure.</summary>
    Failure = 0,
    /// <summary>Specifies an input validation or contract violation failure.</summary>
    Validation = 1,
    /// <summary>Specifies a missing or nonexistent resource failure.</summary>
    NotFound = 2,
    /// <summary>Specifies a state conflict with the current state of a resource.</summary>
    Conflict = 3,
    /// <summary>Specifies an authentication requirement or failure.</summary>
    Unauthorized = 4,
    /// <summary>Specifies an authorization or permission denial failure.</summary>
    Forbidden = 5,
    /// <summary>Specifies a transient external service unavailability failure.</summary>
    Unavailable = 6,
    /// <summary>Specifies an unexpected internal or critical system failure.</summary>
    Unexpected = 7,
    /// <summary>Specifies a business rule or domain invariant violation.</summary>
    Domain = 8,
    /// <summary>Specifies an infrastructure, network, or database connectivity failure.</summary>
    Infrastructure = 9,
    /// <summary>Specifies an application-specific custom error category.</summary>
    Custom = 10
}
