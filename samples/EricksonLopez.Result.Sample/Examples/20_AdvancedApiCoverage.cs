// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

/// <summary>
/// Demonstrates advanced APIs not covered in previous examples:
/// - Implicit operator conversions (value → Result&lt;T&gt;, Error → Result&lt;T&gt;)
/// - operator true/false on non-generic Result
/// - Result (non-generic) Deconstruct, Match&lt;TState&gt;, Execute&lt;TState&gt;
/// - Bind&lt;TState,TNext&gt; (sync, allocation-free)
/// - Ensure with lazy error factories (Func&lt;Error&gt; and Func&lt;TValue, Error&gt;)
/// - Error.Failure(params Error[]) and Error.Validation(params Error[]) with inner errors
/// - Error.DescriptionKey, CorrelationId (via ErrorBuilder)
/// - Error.TryGetMetadata&lt;T&gt;, Error.GetMetadata&lt;T&gt;
/// - Error.HasMetadata
/// - Error.Custom (advanced factory)
/// - IResultOutcome polymorphic interface
/// </summary>
public static class AdvancedApiCoverage
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 20. ADVANCED API COVERAGE");
        Console.WriteLine("========================================================");

        // ------------------------------------------------------------------
        // 1. Implicit operator — TValue → Result<T> and Error → Result<T>
        // ------------------------------------------------------------------
        // Result<TValue> has implicit conversions from both TValue and Error.
        // This is the idiomatic return style in application handlers:
        // just return the value or error directly from a method returning Result<T>.
        Console.WriteLine("\n[1] Implicit operators — TValue → Result<T> and Error → Result<T>:");

        Result<string> fromValue = "implicit success"; // no Result.Success() needed
        Console.WriteLine($"  From value: IsSuccess={fromValue.IsSuccess}, Value={fromValue.GetValueOrDefault("")}");

        Result<string> fromError = Error.NotFound("Item.Missing", "Item not found."); // no Result.Failure needed
        Console.WriteLine($"  From Error: IsFailure={fromError.IsFailure}, Code={fromError.Error.Code}");

        // Idiomatic usage in a method:
        static Result<int> GetScore(bool pass) =>
            pass ? 100 : Error.Validation("Score.Failed", "Did not pass.");

        Console.WriteLine($"  GetScore(true):  {GetScore(true).GetValueOrDefault(-1)}");
        Console.WriteLine($"  GetScore(false): {GetScore(false).Error.Code}");

        // ------------------------------------------------------------------
        // 2. operator true/false on non-generic Result
        // ------------------------------------------------------------------
        // Non-generic Result supports `if (result)` and `if (!result)` directly,
        // via operator true and operator false. Convenient for guard checks.
        // ⚠ IMPORTANT: default(Result) returns FALSE for BOTH operator true AND
        // operator false. Always construct via Result.Success() or Result.Failure().
        Console.WriteLine("\n[2] operator true/false on non-generic Result:");

        Result success = Result.Success();
        Result failure = Result.Failure(Error.Validation("X.Failed", "X failed."));

        if (success) Console.WriteLine("  success is truthy ✓");
        // operator false returns false for failure — so `if (failure)` would NOT execute
        if (!failure.IsSuccess) Console.WriteLine("  failure.IsSuccess is false ✓");
        // Note: C# does NOT derive operator! automatically from operator true/false;
        // !result is a compile error. Use result.IsFailure or .IsSuccess instead.

        static Result Authorize(string role) =>
            role == "admin" ? Result.Success() : Error.Forbidden("Auth.Forbidden", "Not authorized.");

        if (Authorize("admin"))         Console.WriteLine("  Admin authorized ✓");
        if (Authorize("guest").IsFailure) Console.WriteLine("  Guest blocked ✓");

        // ------------------------------------------------------------------
        // 3. non-generic Result — Deconstruct, Match<TState>, Execute<TState>
        // ------------------------------------------------------------------
        Console.WriteLine("\n[3] Non-generic Result — Deconstruct, Match<TState>, Execute<TState>:");

        // Deconstruct (2 values: isSuccess + error)
        var (isOk, errCode) = success;
        Console.WriteLine($"  Deconstruct success: isOk={isOk}, err={errCode?.Code ?? "null"}");

        var (isFail, fErr) = failure;
        Console.WriteLine($"  Deconstruct failure: isFail={!isFail}, err=[{fErr?.Code}]");

        // Match<TState> — returns a value, allocation-free
        string tag = "OrderFlow";
        string matchResult = failure.Match(
            state: tag,
            onSuccess: ctx => $"[{ctx}] Succeeded",
            onFailure: (ctx, e) => $"[{ctx}] Failed: {e.Code}"
        );
        Console.WriteLine($"  Match<TState>: {matchResult}");

        // Execute<TState> — void side-effects, allocation-free
        success.Execute(
            state: tag,
            onSuccess: ctx => Console.WriteLine($"  Execute<TState> onSuccess: tag={ctx}"),
            onFailure: (ctx, e) => Console.WriteLine($"  Execute<TState> onFailure: [{ctx}] {e.Code}")
        );

        // ------------------------------------------------------------------
        // 4. Bind<TState,TNext> — allocation-free sync binding
        // ------------------------------------------------------------------
        Console.WriteLine("\n[4] Bind<TState,TNext> — allocation-free monadic chaining:");

        Result<int> score = Result.Success(85);
        string label = "Score";

        // Without TState: compiler would create a closure capturing `label`
        // With TState: zero closure allocation
        Result<string> labeled = score.Bind(
            state: label,
            bind: (prefix, val) =>
                val >= 50
                    ? Result.Success($"{prefix}: {val} (PASS)")
                    : Result.Failure<string>(Error.Validation("Score.Low", $"Score {val} < 50"))
        );
        Console.WriteLine($"  Bind<TState>: {labeled.GetValueOrDefault("n/a")}");

        // ------------------------------------------------------------------
        // 5. Ensure with lazy error factories
        // ------------------------------------------------------------------
        Console.WriteLine("\n[5] Ensure — lazy error factory overloads:");

        Result<int> num = Result.Success(42);

        // 5a. Ensure(predicate, Func<Error>) — error constructed only on failure
        Result<int> lazyOk = num.Ensure(
            n => n > 0,
            () => Error.Validation("Num.NonPositive", "Must be positive.") // never called here
        );
        Console.WriteLine($"  Lazy Ensure (success): IsSuccess={lazyOk.IsSuccess}");

        Result<int> lazyFail = Result.Success(-5).Ensure(
            n => n > 0,
            () => Error.Validation("Num.NonPositive", "Must be positive.")
        );
        Console.WriteLine($"  Lazy Ensure (failure): Code={lazyFail.Error.Code}");

        // 5b. Ensure(predicate, Func<TValue, Error>) — error receives the value
        Result<int> valueCtxFail = Result.Success(-3).Ensure(
            n => n >= 0,
            n => Error.Validation("Num.Negative", $"Value {n} must not be negative.")
        );
        Console.WriteLine($"  Value-contextual Ensure: Code={valueCtxFail.Error.Code}");

        // ------------------------------------------------------------------
        // 6. Error.Failure(params Error[]) and Error.Validation(params Error[])
        // ------------------------------------------------------------------
        // Directly create compound errors with inner errors without the ErrorBuilder.
        // This is the idiomatic way to create pre-built compound errors.
        Console.WriteLine("\n[6] Error.Failure and Error.Validation with params inner errors:");

        Error inner1 = Error.Validation("Field.A", "Field A is required.");
        Error inner2 = Error.Validation("Field.B", "Field B must be a number.");
        Error inner3 = Error.Validation("Field.C", "Field C exceeds max length.");

        // params Error[] overload — direct compound construction
        Error compound = Error.Validation("Form.Invalid", "3 form validation errors.", inner1, inner2, inner3);

        Console.WriteLine($"  Compound Code: {compound.Code}");
        Console.WriteLine($"  HasInnerErrors: {compound.HasInnerErrors}");
        Console.WriteLine($"  Inner count: {compound.InnerErrors.Length}");
        foreach (var ie in compound.InnerErrors)
            Console.WriteLine($"    -> [{ie.Code}]: {ie.Description}");

        // Error.Failure with inner errors
        Error failureWithInners = Error.Failure("Pipeline.Failed", "Pipeline failed on 2 steps.",
            Error.Unexpected("Step1.Failed", "Step 1 timed out."),
            Error.Unexpected("Step2.Failed", "Step 2 returned null.")
        );
        Console.WriteLine($"  Failure with inners: Code={failureWithInners.Code}, InnerCount={failureWithInners.InnerErrors.Length}");

        // ------------------------------------------------------------------
        // 7. Error.DescriptionKey and CorrelationId (via ErrorBuilder)
        // ------------------------------------------------------------------
        Console.WriteLine("\n[7] DescriptionKey, CorrelationId, TraceId (via ErrorBuilder):");

        Error localizedError = Error.Create("User.NotFound", "User was not found.")
            .WithDescriptionKey("errors.user.not_found")  // i18n resource key
            .WithCorrelationId("sess-abc-123")
            .WithTraceId("trace-xyz-456")
            .Build();

        Console.WriteLine($"  DescriptionKey: \"{localizedError.DescriptionKey}\"");
        Console.WriteLine($"  CorrelationId:  \"{localizedError.CorrelationId}\"");
        Console.WriteLine($"  TraceId:        \"{localizedError.TraceId}\"");

        // ------------------------------------------------------------------
        // 8. Error.TryGetMetadata<T> and Error.GetMetadata<T>
        // ------------------------------------------------------------------
        Console.WriteLine("\n[8] TryGetMetadata<T> and GetMetadata<T>:");

        Error metaError = Error.Create("Payment.Failed", "Payment gateway failed.")
            .WithType(ErrorType.Unexpected)
            .WithMetadata("RequestId", "req-abc-789")
            .WithMetadata("GatewayCode", 503)
            .WithMetadata("Retryable", true)
            .Build();

        // TryGetMetadata<T> — safe pattern-match on typed metadata
        if (metaError.TryGetMetadata<string>("RequestId", out var reqId))
            Console.WriteLine($"  RequestId: {reqId}");

        if (metaError.TryGetMetadata<int>("GatewayCode", out var code))
            Console.WriteLine($"  GatewayCode: {code}");

        // GetMetadata<T> — throws KeyNotFoundException if missing
        bool retryable = metaError.GetMetadata<bool>("Retryable");
        Console.WriteLine($"  Retryable: {retryable}");

        // HasMetadata check
        Console.WriteLine($"  HasMetadata: {metaError.HasMetadata}");

        // ------------------------------------------------------------------
        // 9. Error.Custom — advanced factory
        // ------------------------------------------------------------------
        Console.WriteLine("\n[9] Error.Custom — full-control factory:");

        Error customError = Error.Custom(
            code: "Payment.RateLimited",
            description: "Payment gateway rate limit exceeded.",
            type: ErrorType.Unavailable,
            severity: ErrorSeverity.Warning,
            retryability: ErrorRetryability.Transient,
            descriptionKey: "errors.payment.rate_limited",
            correlationId: "corr-999"
        );
        Console.WriteLine($"  Custom: [{customError.Code}] Type={customError.Type} Retry={customError.Retryability}");

        // ------------------------------------------------------------------
        // 10. IResultOutcome — polymorphic interface
        // ------------------------------------------------------------------
        // IResultOutcome is the shared interface between Result and Result<T>.
        // Use it in middleware, filters, or telemetry that handles both types uniformly.
        Console.WriteLine("\n[10] IResultOutcome — polymorphic interface:");

        IResultOutcome nonGenericSuccess = Result.Success();
        IResultOutcome genericSuccess = Result.Success(42);
        IResultOutcome typedFailure = Result.Failure<string>(Error.NotFound("X", "Not found."));

        PrintOutcome("non-generic success", nonGenericSuccess);
        PrintOutcome("generic success", genericSuccess);
        PrintOutcome("typed failure", typedFailure);
    }

    private static void PrintOutcome(string label, IResultOutcome outcome)
    {
        string state = outcome.IsSuccess ? $"Success (Value={outcome.RawValue})"
            : outcome.IsFailure ? $"Failure({outcome.Error?.Code})"
            : "Uninitialized";
        Console.WriteLine($"  [{label}]: {state}");
    }
}
