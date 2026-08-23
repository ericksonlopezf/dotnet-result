// Copyright © Erickson Lopez. MIT License.
using EricksonLopez.Result;

namespace EricksonLopez.Result.Tests.Core;

/// <summary>
/// Centralized canonical error fixtures for the test suite.
/// Eliminates redundant error declarations across test classes while maintaining clear semantic intent.
/// </summary>
public static class TestErrors
{
    /// <summary>Canonical generic test failure error.</summary>
    public static readonly Error Default = Error.Failure("Test.Error", "Test error message");

    /// <summary>Secondary distinct test failure error for branching / sequence assertions.</summary>
    public static readonly Error Second = Error.Failure("Test.Error2", "Test error message 2");

    /// <summary>Canonical validation error for filtering / predicate assertions.</summary>
    public static readonly Error Validation = Error.Validation("Test.Validation", "Test validation error");

    /// <summary>Canonical transient infrastructure error for retryability assertions.</summary>
    public static readonly Error Transient = Error.Infrastructure("Test.Transient", "Test transient error");
}
