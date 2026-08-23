// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

/// <summary>
/// Demonstrates Error equality, comparison, and mutation APIs:
/// - Error.Equals (shallow — Code, Description, Type, Severity, Retryability)
/// - Error.StrictEquals (deep — all fields including TraceId, CorrelationId, Metadata, InnerErrors)
/// - Error.operator == / !=
/// - ErrorEqualityComparer.Default (shallow)
/// - ErrorEqualityComparer.Strict (deep structural)
/// - Error.With* instance methods (copy-on-write fluent mutations)
///   - Error.WithTraceId(string?)
///   - Error.WithTraceId(ActivityTraceId)
///   - Error.ClearTraceId()
///   - Error.WithCorrelationId(string?)
///   - Error.WithDescriptionKey(string?)
///   - Error.WithRetryability(ErrorRetryability)
/// - Error.ToString()
/// - Error.ToBuilder() (convert to ErrorBuilder for heavy modifications)
/// </summary>
public static class ErrorEqualityAndMutation
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 12. ERROR EQUALITY, COMPARERS & COPY-ON-WRITE MUTATION");
        Console.WriteLine("========================================================");

        // ------------------------------------------------------------------
        // 1. Error.Equals — shallow semantic equality
        // ------------------------------------------------------------------
        // Two errors are SHALLOW-equal if they share the same:
        //   Code, Description, Type, Severity, Retryability
        // Excluded from shallow equality: TraceId, CorrelationId,
        // DescriptionKey, InnerErrors, and Metadata.
        Console.WriteLine("\n[1] Error.Equals — shallow semantic equality:");

        Error e1 = Error.NotFound("User.NotFound", "User was not found.");
        Error e2 = Error.NotFound("User.NotFound", "User was not found.").WithTraceId("trace-abc");

        Console.WriteLine($"  e1 == e2 (different TraceId): {e1 == e2}"); // true — TraceId excluded
        Console.WriteLine($"  e1.Equals(e2): {e1.Equals(e2)}");           // true

        Error e3 = Error.NotFound("User.NotFound", "User was not found.")
            .WithRetryability(ErrorRetryability.Permanent);
        Console.WriteLine($"  e1 == e3 (different Retryability): {e1 == e3}"); // false — Retryability IS included

        // operator != 
        Error e4 = Error.Validation("X.Invalid", "X is invalid.");
        Console.WriteLine($"  e1 != e4 (different Code): {e1 != e4}"); // true

        // ------------------------------------------------------------------
        // 2. Error.StrictEquals — deep structural equality
        // ------------------------------------------------------------------
        // StrictEquals additionally compares TraceId, CorrelationId,
        // DescriptionKey, InnerErrors, and Metadata.
        Console.WriteLine("\n[2] Error.StrictEquals — deep structural equality:");

        Error base1 = Error.Validation("Form.Invalid", "Form validation failed.")
            .WithTraceId("trace-001")
            .WithCorrelationId("sess-001");

        Error base2 = Error.Validation("Form.Invalid", "Form validation failed.")
            .WithTraceId("trace-001")
            .WithCorrelationId("sess-001");

        Error base3 = Error.Validation("Form.Invalid", "Form validation failed.")
            .WithTraceId("trace-DIFFERENT");

        Console.WriteLine($"  base1 == base2 (shallow): {base1 == base2}");                  // true
        Console.WriteLine($"  base1.StrictEquals(base2): {base1.StrictEquals(base2)}");      // true
        Console.WriteLine($"  base1.StrictEquals(base3): {base1.StrictEquals(base3)}");      // false (TraceId differs)

        // ------------------------------------------------------------------
        // 3. ErrorEqualityComparer.Default and ErrorEqualityComparer.Strict
        // ------------------------------------------------------------------
        // Use these with collections (HashSet<Error>, Dictionary<Error,T>)
        // to explicitly control equality semantics.
        Console.WriteLine("\n[3] ErrorEqualityComparer — collection usage:");

        Error errA = Error.NotFound("X.NotFound", "X not found.").WithTraceId("trace-A");
        Error errB = Error.NotFound("X.NotFound", "X not found.").WithTraceId("trace-B");

        // Default (shallow) — errA and errB are EQUAL (deduplicated in set)
        var shallowSet = new HashSet<Error>(ErrorEqualityComparer.Default) { errA, errB };
        Console.WriteLine($"  Shallow set count (should be 1): {shallowSet.Count}");

        // Strict (deep) — errA and errB are DISTINCT (different TraceIds)
        var strictSet = new HashSet<Error>(ErrorEqualityComparer.Strict) { errA, errB };
        Console.WriteLine($"  Strict set count (should be 2): {strictSet.Count}");

        // Dictionary with Default comparer — deduplication on key equality
        var errorCounter = new Dictionary<Error, int>(ErrorEqualityComparer.Default);
        errorCounter[errA] = 1;
        errorCounter[errB] += 1; // errB == errA under Default → same key
        Console.WriteLine($"  Default dictionary count (should be 1): {errorCounter.Count}");

        // ------------------------------------------------------------------
        // 4. Error.With* — copy-on-write instance mutations
        // ------------------------------------------------------------------
        // Error is immutable. Each With* method returns a NEW Error with
        // the specified field updated, leaving the original unchanged.
        // These are single-field updates — prefer ErrorBuilder (Error.Create / ToBuilder)
        // for building errors with multiple simultaneous field changes.
        Console.WriteLine("\n[4] Error.With* — copy-on-write instance mutations:");

        Error original = Error.Unexpected("Op.Failed", "Operation failed.");

        // WithTraceId — string override
        Error withTrace = original.WithTraceId("trace-xyz-123");
        Console.WriteLine($"  Original TraceId: {original.TraceId ?? "null"}");
        Console.WriteLine($"  WithTraceId: {withTrace.TraceId}");
        Console.WriteLine($"  Original unchanged: {original.TraceId is null}");

        // WithCorrelationId
        Error withCorr = original.WithCorrelationId("corr-session-456");
        Console.WriteLine($"  WithCorrelationId: {withCorr.CorrelationId}");

        // WithDescriptionKey (i18n resource key)
        Error withKey = original.WithDescriptionKey("errors.op.failed");
        Console.WriteLine($"  WithDescriptionKey: {withKey.DescriptionKey}");

        // WithRetryability — change the retry classification on an existing error
        Error transientVersion = original.WithRetryability(ErrorRetryability.Transient);
        Console.WriteLine($"  Original Retryability: {original.Retryability}");
        Console.WriteLine($"  WithRetryability(Transient): {transientVersion.Retryability}");

        // ClearTraceId — removes any trace ID override
        Error errWithTrace = original.WithTraceId("to-be-cleared");
        Error cleared = errWithTrace.ClearTraceId();
        Console.WriteLine($"  ClearTraceId: TraceId is null = {cleared.TraceId is null}");

        // ------------------------------------------------------------------
        // 5. Chaining With* — multi-step copy pattern
        // ------------------------------------------------------------------
        // Chaining With* is valid but copies the Error struct on each call.
        // For more than 2 properties, prefer ToBuilder() + Build().
        Console.WriteLine("\n[5] With* chaining vs ErrorBuilder:");

        // Short chain — 2 fields, acceptable
        Error tagged = original
            .WithRetryability(ErrorRetryability.Transient)
            .WithCorrelationId("corr-abc");
        Console.WriteLine($"  Chained: Retry={tagged.Retryability}, Corr={tagged.CorrelationId}");

        // Longer — prefer ToBuilder for 3+ fields (single struct copy)
        Error enriched = original.ToBuilder()
            .WithRetryability(ErrorRetryability.Transient)
            .WithCorrelationId("corr-abc")
            .WithDescriptionKey("errors.op.failed")
            .WithMetadata("component", "PaymentGateway")
            .Build();
        Console.WriteLine($"  Builder: Retry={enriched.Retryability}, Key={enriched.DescriptionKey}");

        // ------------------------------------------------------------------
        // 6. Error.ToString()
        // ------------------------------------------------------------------
        Console.WriteLine("\n[6] Error.ToString() — diagnostic representation:");
        Console.WriteLine($"  {Error.Validation("User.Age", "User must be at least 18.")}");
        Console.WriteLine($"  {Error.Unexpected("System.OutOfMemory", "Process ran out of memory.")}");
    }
}
