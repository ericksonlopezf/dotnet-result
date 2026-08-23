// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class ErrorsAndBuilders
{
    public static void Run()
    {
        Console.WriteLine("\n--- 02. ERRORS AND BUILDERS ---");

        // The Error object in this library is very rich. You use Error.Create(...) to start an ErrorBuilder.

        // 1. Basic Error
        Error simpleError = Error.Create("Validation.MissingField", "The field 'Email' is required.").Build();

        // 2. Rich Error using the Builder
        Error richError = Error.Create("Database.Timeout", "The connection to the database timed out.")
            .WithType(ErrorType.Unexpected)
            .WithSeverity(ErrorSeverity.Critical)
            .WithRetryability(ErrorRetryability.Transient)
            .WithCorrelationId(Guid.NewGuid().ToString())
            .WithMetadata("DbServer", "prod-db-01")
            .WithMetadata("RetryCount", 3)
            .Build();

        Console.WriteLine($"Rich Error Code: {richError.Code}");
        Console.WriteLine($"Type: {richError.Type}, Severity: {richError.Severity}, Retryability: {richError.Retryability}");
        Console.WriteLine($"Correlation ID: {richError.CorrelationId}");

        if (richError.Metadata != null)
        {
            Console.WriteLine("Metadata:");
            foreach (var kvp in richError.Metadata)
            {
                Console.WriteLine($" - {kvp.Key}: {kvp.Value}");
            }
        }

        // 3. Inner Errors
        // Errors can form a tree, useful for aggregate exceptions or multi-field validation failures.
        Error validationError1 = Error.Create("Validation.Email", "Invalid email format.").Build();
        Error validationError2 = Error.Create("Validation.Age", "Age must be over 18.").Build();

        Error aggregateError = Error.Create("Validation.Failed", "Multiple validation errors occurred.")
            .WithType(ErrorType.Validation)
            .WithInnerErrors(new[] { validationError1, validationError2 })
            .Build();

        Console.WriteLine($"Aggregate Error Code: {aggregateError.Code}");
        if (aggregateError.InnerErrors != null)
        {
            Console.WriteLine("Inner Errors:");
            foreach (var inner in aggregateError.InnerErrors)
            {
                Console.WriteLine($" - {inner.Code}: {inner.Description}");
            }
        }

        // 4. Built-in Error Factories
        // The library provides quick factories for common error types instead of manually setting .WithType(...)
        Error notFound = Error.NotFound("User.NotFound", "User not found.");
        Error conflict = Error.Conflict("User.Conflict", "User already exists.");
        Error forbidden = Error.Forbidden("User.Forbidden", "Not allowed.");
        Error infrastructure = Error.Infrastructure("Db.Offline", "Database is offline.");
        Error unauthorized = Error.Unauthorized("Auth.Failed", "Invalid token.");
        Error unavailable = Error.Unavailable("Service.Down", "Service is unavailable.");
        Error unexpected = Error.Unexpected("System.Crash", "Unknown error.");
        Error domain = Error.Domain("Business.Rule", "Violated a domain rule.");
        Error validation = Error.Validation("Input.Invalid", "Invalid input.");

        // 5. Trace IDs
        // You can attach string trace IDs or native System.Diagnostics.ActivityTraceId
        var traceId = System.Diagnostics.ActivityTraceId.CreateRandom();
        Error errorWithTrace = Error.Create("Trace.Test", "Testing trace ID")
            .WithTraceId(traceId)
            .Build();

        Error clearedTrace = errorWithTrace.ClearTraceId();

        // 6. Modifying existing Errors (ToBuilder)
        // Errors are immutable. You can use ToBuilder() to create a new Error based on an existing one.
        Error updatedError = simpleError.ToBuilder()
            .WithSeverity(ErrorSeverity.Warning)
            .Build();

        Console.WriteLine($"Updated Error Severity: {updatedError.Severity} (Original was: {simpleError.Severity})");

        // 7. Equality
        // Equals() only compares Code and Description (and maybe Type/Severity depending on library semantics).
        // StrictEquals() compares absolutely every field including Metadata and InnerErrors.
        bool isStrictlyEqual = simpleError.StrictEquals(updatedError);
        Console.WriteLine($"Are simpleError and updatedError strictly equal? {isStrictlyEqual}");
    }
}


