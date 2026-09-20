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

        // 8. Sentinel Errors: Error.None and Error.NullValue
        // ----------------------------------------------------------
        // Error.None is a sentinel representing the absence of an error (used when a
        // field must hold an Error instance but no actual error has occurred).
        // Error.NullValue is a sentinel for null-value guard scenarios.
        // Both avoid allocating a new Error object each time.
        Console.WriteLine("\n--- 8. Sentinel Errors: Error.None and Error.NullValue ---");

        Error noError = Error.None;
        Console.WriteLine($"Error.None  Code='{noError.Code}', Type={noError.Type}, Severity={noError.Severity}");

        Error nullError = Error.NullValue;
        Console.WriteLine($"Error.NullValue Code='{nullError.Code}', Type={nullError.Type}");

        // Idiomatic: use Error.None to represent 'no error' in optional fields.
        static Error GetLastError(bool hasError) => hasError
            ? Error.Validation("Input.Invalid", "Input was invalid.")
            : Error.None;

        Console.WriteLine($"GetLastError(false).Code == Error.None.Code: {GetLastError(false).Code == Error.None.Code}");

        // 9. Error.Message property (alias for Description)
        // ----------------------------------------------------------
        // Error.Message is an alias for Error.Description introduced for compatibility
        // with common exception patterns where callers expect a .Message property.
        Console.WriteLine("\n--- 9. Error.Message — alias for Description ---");

        Error errorWithMessage = Error.Validation("Order.Expired", "The order has expired and cannot be processed.");
        Console.WriteLine($"Description: \"{errorWithMessage.Description}\"");
        Console.WriteLine($"Message:     \"{errorWithMessage.Message}\"");
        Console.WriteLine($"Description == Message: {errorWithMessage.Description == errorWithMessage.Message}");

        // 10. Error.NotFound<TId> — strongly-typed entity + id factory
        // ----------------------------------------------------------
        // Generates a canonical NotFound error in the format:
        //   Code:        "{Entity}.NotFound"
        //   Description: "{Entity} with Id '{id}' was not found."
        // Avoids string interpolation boilerplate in repositories and handlers.
        Console.WriteLine("\n--- 10. Error.NotFound<TId> — strongly-typed entity lookup ---");

        // With Guid (struct TId)
        var orderId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000001");
        Error orderNotFound = Error.NotFound<Guid>("Order", orderId);
        Console.WriteLine($"  Code:        {orderNotFound.Code}");
        Console.WriteLine($"  Description: {orderNotFound.Description}");
        Console.WriteLine($"  Type:        {orderNotFound.Type}");

        // With int (struct TId)
        Error productNotFound = Error.NotFound<int>("Product", 42);
        Console.WriteLine($"  Product: Code='{productNotFound.Code}'");

        // 11. Error.Validation with IReadOnlyDictionary<string, object> metadata
        // ----------------------------------------------------------
        // Creates a Validation error with structured metadata in a single call —
        // no need for the fluent ErrorBuilder chain when the metadata is already available.
        Console.WriteLine("\n--- 11. Error.Validation(code, desc, metadata overload) ---");

        IReadOnlyDictionary<string, object> validationMeta = new Dictionary<string, object>
        {
            { "Field",    "Email" },
            { "Received", "not-an-email" },
            { "Rule",     "EmailFormat" }
        };

        Error validationWithMeta = Error.Validation(
            "User.EmailInvalid",
            "The provided email address is not valid.",
            validationMeta);

        Console.WriteLine($"  Code:     {validationWithMeta.Code}");
        Console.WriteLine($"  Type:     {validationWithMeta.Type}");
        Console.WriteLine($"  Metadata: Field={validationWithMeta.Metadata["Field"]}, Rule={validationWithMeta.Metadata["Rule"]}");

        // 12. Error.Conflict with IReadOnlyDictionary<string, object> metadata
        // ----------------------------------------------------------
        // Creates a Conflict error with structured metadata in a single call.
        // Typical scenario: optimistic concurrency — record the conflicting version.
        Console.WriteLine("\n--- 12. Error.Conflict(code, desc, metadata overload) ---");

        IReadOnlyDictionary<string, object> conflictMeta = new Dictionary<string, object>
        {
            { "ExistingId",      "usr-001"    },
            { "ConflictingField","Email"       },
            { "ExistingValue",   "user@domain.com" }
        };

        Error conflictWithMeta = Error.Conflict(
            "User.EmailConflict",
            "A user with this email already exists.",
            conflictMeta);

        Console.WriteLine($"  Code:     {conflictWithMeta.Code}");
        Console.WriteLine($"  Type:     {conflictWithMeta.Type}");
        Console.WriteLine($"  ExistingId: {conflictWithMeta.Metadata["ExistingId"]}");
    }
}
