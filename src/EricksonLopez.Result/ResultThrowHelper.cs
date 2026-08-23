// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace EricksonLopez.Result;

/// <summary>
/// Internal throw helpers used by <see cref="Result"/> and <see cref="Result{TValue}"/>
/// to keep the hot <c>ThrowIfUninitialized</c> check as small as possible when inlined.
/// </summary>
/// <remarks>
/// By isolating the <c>throw</c> statement in a separate <c>[DoesNotReturn]</c> method,
/// the JIT avoids inlining the string allocation and exception construction into every
/// call site, reducing native code size without sacrificing the fast-path branch.
/// </remarks>
internal static class ResultThrowHelper
{
    [DoesNotReturn]
    [StackTraceHidden]
    internal static void ThrowUninitialized()
        => throw new InvalidOperationException(
            "Cannot operate on an uninitialized default Result. " +
            "Always construct Result via Result.Success() or Result.Failure(error).");

    [DoesNotReturn]
    [StackTraceHidden]
    internal static void ThrowUninitializedOfT()
        => throw new InvalidOperationException(
            "Cannot operate on an uninitialized default Result<TValue>. " +
            "Always construct Result<TValue> via Result.Success(value) or Result.Failure(error).");
}

