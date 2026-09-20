// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;
using EricksonLopez.Result.Dapr;
using Microsoft.AspNetCore.Http;

namespace EricksonLopez.Result.Sample.Examples;

public static class DaprShowcase
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 32. DAPR DISTRIBUTED STATE & PUBSUB SHOWCASE");
        Console.WriteLine("========================================================");

        // 1. Dapr Error Codes & DaprErrorResponse
        Console.WriteLine("\n[1] Dapr Error Response and Canonical Codes:");
        var domainError = Error.Conflict(DaprErrorCodes.EtagMismatch, "ETag mismatch while writing to state store 'statestore'.");
        var daprResponse = new DaprErrorResponse("RETRY", domainError.Code, domainError.Description);

        Console.WriteLine($"  Dapr Status:       {daprResponse.Status}");
        Console.WriteLine($"  Error Code:        {daprResponse.Code}");
        Console.WriteLine($"  Error Message:     {daprResponse.Description}");

        // 2. Pub/Sub Topic Result Translation (ACK, RETRY, DROP)
        Console.WriteLine("\n[2] Dapr Pub/Sub Topic Result Mapping (ToDaprTopicResult):");

        // Success -> 200 OK (ACK)
        var successResult = Result.Success();
        var ackHttpResult = successResult.ToDaprTopicResult();
        Console.WriteLine($"  Success Result -> HTTP {ackHttpResult.GetType().Name} (Dapr ACK)");

        // Transient failure -> 503 Service Unavailable (Dapr RETRY)
        var transientError = Error.Custom(
            code: DaprErrorCodes.SidecarUnavailable,
            description: "Dapr sidecar is temporarily unreachable.",
            type: ErrorType.Unavailable,
            severity: ErrorSeverity.Warning,
            retryability: ErrorRetryability.Transient);
        var retryResult = Result.Failure(transientError).ToDaprTopicResult();
        Console.WriteLine($"  Transient Error -> HTTP 503 Status Code (Dapr RETRY)");

        // Permanent failure -> 422 Unprocessable Entity (Dapr DROP / Dead-Letter)
        var permanentError = Error.Validation("Order.InvalidPayload", "Schema version is unsupported.");
        var dropResult = Result.Failure(permanentError).ToDaprTopicResult();
        Console.WriteLine($"  Permanent Error -> HTTP 422 Unprocessable Entity (Dapr DROP)");

        // 3. State Store Extension Architecture
        Console.WriteLine("\n[3] Dapr State Store Result Envelope Integration:");
        Console.WriteLine($"  Extensions Available on DaprClient:");
        Console.WriteLine($"  • GetStateWithResultAsync<T>(storeName, key)");
        Console.WriteLine($"  • SaveStateWithResultAsync<T>(storeName, key, value, etag)");
        Console.WriteLine($"  • ExecuteStateTransactionWithResultAsync(storeName, operations)");
        Console.WriteLine($"  • GetBulkStateWithResultAsync(storeName, keys)");
        Console.WriteLine($"  All operations return Result<T> wrapping DaprException or ETag concurrency conflicts safely.");
    }
}
