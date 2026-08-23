// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class RecoverAndErrorTransformation
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 16. RECOVER & ERROR TRANSFORMATION");
        Console.WriteLine("========================================================");

        // -------------------------------------------------------------
        // 1. Recover — attempt to correct a failure
        // -------------------------------------------------------------
        // Recover is called ONLY if the result is a failure.
        // It can return either a new success or another failure.
        Console.WriteLine("\n[1] Recover — corrective action on failure:");

        Result<int> inventoryResult = Result.Failure<int>(
            Error.NotFound("Inventory.OutOfStock", "Item is out of stock."));

        // Recover with a fallback value (retry from cache)
        Result<int> recovered = inventoryResult.Recover(
            err =>
            {
                Console.WriteLine($"  Recovering from [{err.Code}]: {err.Description}");
                Console.WriteLine("  Falling back to cached inventory count: 5");
                return Result.Success(5);
            }
        );

        Console.WriteLine($"  Recovered IsSuccess: {recovered.IsSuccess}, Value: {recovered.GetValueOrDefault(0)}");

        // Recover with state (allocation-free closure substitute)
        int fallbackStock = 10;
        Result<int> recoveredWithState = inventoryResult.Recover(
            state: fallbackStock,
            recovery: (state, err) => err.Type == ErrorType.NotFound
                ? Result.Success(state)
                : Result.Failure<int>(err) // propagate non-NotFound errors
        );
        Console.WriteLine($"  Recovered with state IsSuccess: {recoveredWithState.IsSuccess}, Value: {recoveredWithState.GetValueOrDefault(0)}");

        // Recover on a SUCCESS — does nothing, returns same result
        Result<int> alreadySuccess = Result.Success(42);
        Result<int> notRecovered = alreadySuccess.Recover(err => Result.Success(-1));
        Console.WriteLine($"  Recover on success (no-op): IsSuccess={notRecovered.IsSuccess}, Value={notRecovered.GetValueOrDefault(0)}");

        // -------------------------------------------------------------
        // 2. MapError — transform the error without changing the value type
        // -------------------------------------------------------------
        // Useful for normalizing errors at API boundaries,
        // or enriching an error with additional context before returning it.
        Console.WriteLine("\n[2] MapError — transform the Error on failure:");

        Result<string> dbResult = Result.Failure<string>(
            Error.Unexpected("Db.Timeout", "Connection timed out after 30s."));

        // Map the error: enrich with retry context
        Result<string> enriched = dbResult.MapError(err =>
            err.ToBuilder()
                .WithRetryability(ErrorRetryability.Transient)
                .WithMetadata("RetryAfterSeconds", 5)
                .Build()
        );

        Console.WriteLine($"  Original Type: {dbResult.Error.Type}");
        Console.WriteLine($"  Enriched Retryability: {enriched.Error.Retryability}");

        // MapError with state (allocation-free)
        string serviceTag = "PaymentGateway";
        Result<string> taggedError = dbResult.MapError(
            state: serviceTag,
            mapper: (tag, err) => err.ToBuilder()
                .WithMetadata("ServiceTag", tag)
                .Build()
        );
        Console.WriteLine($"  Tagged Error ServiceTag: {taggedError.Error.Metadata?["ServiceTag"]}");

        // MapError on success — no-op
        Result<string> successResult = Result.Success("data");
        Result<string> mapErrorOnSuccess = successResult.MapError(err => Error.Unexpected("X", "Y"));
        Console.WriteLine($"  MapError on success (no-op): IsSuccess={mapErrorOnSuccess.IsSuccess}");

        // -------------------------------------------------------------
        // 3. MapFailure — extract a value from a failure, bypassing Result entirely
        // -------------------------------------------------------------
        // Useful for HTTP response mapping: turn a failed Result<T> into
        // a plain TOut (e.g., HTTP status code, DTO, or a default value).
        Console.WriteLine("\n[3] MapFailure — extract a plain value from failure:");

        Result<string> failedOperation = Result.Failure<string>(
            Error.Validation("Input.Invalid", "The input payload failed validation."));

        // On failure: transform the Error to string. On success: return the default.
        string friendlyMessage = failedOperation.MapFailure(
            err => $"Error [{err.Code}]: {err.Description}",
            successDefault: string.Empty
        );
        Console.WriteLine($"  MapFailure message: \"{friendlyMessage}\"");

        // On a success — successDefault is returned
        Result<string> okOperation = Result.Success("All good");
        string fromSuccess = okOperation.MapFailure(
            err => $"Unexpected: {err.Code}",
            successDefault: "OK"
        );
        Console.WriteLine($"  MapFailure on success: \"{fromSuccess}\"");

        // -------------------------------------------------------------
        // 4. DiscardValue — convert Result<T> to non-generic Result
        // -------------------------------------------------------------
        // Useful when you need to combine typed results with non-generic ones,
        // or when downstream methods only accept Result (no value).
        Console.WriteLine("\n[4] DiscardValue — drop the value type:");

        Result<int> typedSuccess = Result.Success(99);
        Result discarded = typedSuccess.DiscardValue();
        Console.WriteLine($"  DiscardValue on success: IsSuccess={discarded.IsSuccess}");

        Result<int> typedFailure = Result.Failure<int>(Error.Validation("X.Invalid", "Bad input."));
        Result discardedFailure = typedFailure.DiscardValue();
        Console.WriteLine($"  DiscardValue on failure: IsFailure={discardedFailure.IsFailure}, Code={discardedFailure.Error.Code}");

        // Real-world use: combine a guard (Result) with a typed result (Result<T>)
        // via Result.Merge — see example 18 for Result.Merge.
        Result guard = Result.Failure(Error.Unauthorized("Auth.Required", "Must be authenticated."));
        Result guardFromTyped = typedFailure.DiscardValue(); // drop T, use as guard
        Console.WriteLine($"  Guard from typed failure: IsFailure={guardFromTyped.IsFailure}");
    }
}
