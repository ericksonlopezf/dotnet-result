// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class WellKnownErrorsAndTry
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 19. WELL-KNOWN ERRORS, RESULT.TRY & INSPECT");
        Console.WriteLine("========================================================");

        // -------------------------------------------------------------
        // 1. WellKnownErrors — system-level constants
        // -------------------------------------------------------------
        // WellKnownErrors provides two public members:
        //   • CombinedFailuresCode — the error code used by Combine/ValidateAll
        //     when aggregating multiple failures into a compound error.
        //   • UninitializedError  — a sentinel Error returned when accessing
        //     an uninitialized default(Result).
        Console.WriteLine("\n[1] WellKnownErrors — system error constants:");

        Console.WriteLine($"  CombinedFailuresCode: \"{WellKnownErrors.CombinedFailuresCode}\"");
        Console.WriteLine($"  UninitializedError.Code: \"{WellKnownErrors.UninitializedError.Code}\"");
        Console.WriteLine($"  UninitializedError.Type: {WellKnownErrors.UninitializedError.Type}");
        Console.WriteLine($"  UninitializedError.Severity: {WellKnownErrors.UninitializedError.Severity}");

        // Use CombinedFailuresCode to detect aggregated compound failures:
        Result combined = Result.Combine(
            Result.Failure(Error.Validation("A.Missing", "Field A required.")),
            Result.Failure(Error.Validation("B.Missing", "Field B required."))
        );
        bool isAggregated = combined.Error.Code == WellKnownErrors.CombinedFailuresCode;
        Console.WriteLine($"\n  Combined failure code matches sentinel: {isAggregated}");
        Console.WriteLine($"  Inner error count: {combined.Error.InnerErrors.Length}");

        // -------------------------------------------------------------
        // 2. Result.Try<T> — wrap unsafe synchronous code (typed)
        // -------------------------------------------------------------
        // Signature: Result.Try<T>(Func<T> func, Func<Exception, Error> errorHandler)
        // Executes the delegate; any non-fatal exception is caught and converted.
        // Note: OutOfMemoryException, StackOverflowException, AccessViolationException
        // are "fatal" and are always re-thrown.
        Console.WriteLine("\n[2] Result.Try<T> — safe exception boundary (typed):");

        // 2a. Success path
        Result<int> parsed = Result.Try(
            () => int.Parse("42"),
            ex => Error.Unexpected("Parse.Failed", ex.Message)
        );
        Console.WriteLine($"  Try parse '42': IsSuccess={parsed.IsSuccess}, Value={parsed.GetValueOrDefault(-1)}");

        // 2b. Failure path (exception caught and converted)
        Result<int> failedParse = Result.Try(
            () => int.Parse("not-a-number"),
            ex => Error.Validation("Parse.InvalidFormat", $"Cannot parse as int: {ex.Message}")
        );
        Console.WriteLine($"  Try parse 'not-a-number': IsFailure={failedParse.IsFailure}");
        Console.WriteLine($"  Error code: [{failedParse.Error.Code}]");

        // 2c. With state — allocation-free error handler (no closure)
        // Type arguments must be explicit because C# cannot infer T from a throwing lambda.
        string operationTag = "OrderLoad";
        Result<string> withState = Result.Try<string, string>(
            operationTag,
            () => throw new InvalidOperationException("DB offline"),
            (tag, ex) => Error.Infrastructure($"{tag}.Failed", ex.Message)
        );
        Console.WriteLine($"  Try with state: [{withState.Error.Code}]");

        // -------------------------------------------------------------
        // 3. Result.Try (non-generic, void operations)
        // -------------------------------------------------------------
        // Signature: Result.Try(Action action, Func<Exception, Error> errorHandler)
        Console.WriteLine("\n[3] Result.Try — safe exception boundary (non-generic):");

        bool sideEffectRan = false;
        Result actionResult = Result.Try(
            () => { sideEffectRan = true; },
            ex => Error.Unexpected("Action.Failed", ex.Message)
        );
        Console.WriteLine($"  Try Action success: IsSuccess={actionResult.IsSuccess}, sideEffect={sideEffectRan}");

        Result actionFailure = Result.Try(
            () => throw new InvalidOperationException("boom"),
            ex => Error.Unexpected("Op.Failed", ex.Message)
        );
        Console.WriteLine($"  Try Action with throw: IsFailure={actionFailure.IsFailure}, Code={actionFailure.Error.Code}");

        // With state overload
        string context = "InitDb";
        Result tryWithState = Result.Try(
            state: context,
            action: () => throw new TimeoutException("connection timeout"),
            errorHandler: (ctx, ex) => Error.Infrastructure($"{ctx}.Timeout", ex.Message)
        );
        Console.WriteLine($"  Try non-generic with state: [{tryWithState.Error.Code}]");

        // -------------------------------------------------------------
        // 4. Inspect — unconditional side-effect, returns same result
        // -------------------------------------------------------------
        // Inspect executes an action on the current Result<T>, then returns
        // it unchanged. Perfect for structured logging inside a pipeline.
        Console.WriteLine("\n[4] Inspect — unconditional side-effect pipeline step:");

        Result<int> resultToInspect = Result.Success(7);

        Result<int> afterInspect = resultToInspect
            .Inspect(r =>
            {
                string state = r.IsSuccess ? $"SUCCESS({r.Value})" : $"FAILURE({r.Error.Code})";
                Console.WriteLine($"  Inspect callback: result is {state}");
            });

        Console.WriteLine($"  Result unchanged after Inspect: {afterInspect.IsSuccess}");

        // Inspect with state (allocation-free)
        string logContext = "PaymentFlow";
        Result<int> withStateInspect = resultToInspect
            .Inspect(
                state: logContext,
                action: (ctx, r) => Console.WriteLine(
                    $"  [{ctx}] Inspect with state: IsSuccess={r.IsSuccess}")
            );
        Console.WriteLine($"  Result unchanged after stateful Inspect: {withStateInspect.IsSuccess}");
    }
}
