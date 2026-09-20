// Copyright © Erickson Lopez. MIT License.
using EricksonLopez.Result;

namespace EricksonLopez.Result.Tests.Core;

/// <summary>
/// Provides centralized canonical error fixtures for the test suite.
/// Eliminates redundant error declarations across test classes while maintaining clear semantic intent.
/// </summary>
public static class TestErrors
{
    /// <summary>Gets a canonical generic test failure error.</summary>
    public static readonly Error Default = Error.Failure("Test.Error", "Test error message");

    /// <summary>Gets a secondary distinct test failure error for branching or sequence assertions.</summary>
    public static readonly Error Second = Error.Failure("Test.Error2", "Test error message 2");

    /// <summary>Gets a canonical validation error for filtering or predicate assertions.</summary>
    public static readonly Error Validation = Error.Validation("Test.Validation", "Test validation error");

    /// <summary>Gets a canonical transient infrastructure error for retryability assertions.</summary>
    public static readonly Error Transient = Error.Infrastructure("Test.Transient", "Test transient error");
}
