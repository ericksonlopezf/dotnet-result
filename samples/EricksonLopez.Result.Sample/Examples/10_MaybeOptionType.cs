// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Maybe;

namespace EricksonLopez.Result.Sample.Examples;

public static class MaybeOptionType
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 10. MAYBE<T> OPTION TYPE & REPOSITORY INTEROP");
        Console.WriteLine("========================================================");

        // -------------------------------------------------------------
        // 1. Creation and State Inspection
        // -------------------------------------------------------------
        Console.WriteLine("\n[1] Creating Maybe<T> instances:");
        Maybe<string> present = Maybe<string>.From("john.doe@company.com");
        Maybe<string> missing = Maybe<string>.None;
        Maybe<string> fromNull = Maybe<string>.From((string?)null); // Automatically becomes Maybe.None

        Console.WriteLine($"Present: HasValue={present.HasValue}, Value={present.Value}");
        Console.WriteLine($"Missing: HasNoValue={missing.HasNoValue}, FromNull.HasValue={fromNull.HasValue}");

        // -------------------------------------------------------------
        // 2. Monadic Operations (Map, Bind, Ensure)
        // -------------------------------------------------------------
        Console.WriteLine("\n[2] Monadic chaining over Maybe<T>:");

        Maybe<string> domain = present
            .Ensure(email => email.Contains('@'))
            .Map(email => email.Split('@')[1].ToUpperInvariant());

        Console.WriteLine($"Mapped Domain: {domain.GetValueOrDefault("UNKNOWN")}");

        // -------------------------------------------------------------
        // 3. Pattern Matching & Safe Value Extraction
        // -------------------------------------------------------------
        Console.WriteLine("\n[3] Pattern matching and fallbacks:");

        string message = missing.Match(
            onValue: v => $"Found: {v}",
            onNone: () => "Value was absent."
        );
        Console.WriteLine($"Match on missing: {message}");

        string fallback = missing.GetValueOrFallback(() => "default@company.com");
        Console.WriteLine($"GetValueOrFallback: {fallback}");

        // -------------------------------------------------------------
        // 4. Interoperability with Result<T>
        // -------------------------------------------------------------
        Console.WriteLine("\n[4] Converting Maybe<T> to Result<T>:");

        // When absent, converts to Failure with the specified Domain Error
        Result<string> userResult = missing.ToResult(Error.NotFound("User.NotFound", "User could not be found in repository."));
        Console.WriteLine($"Converted to Result: IsFailure={userResult.IsFailure}, ErrorCode={userResult.Error.Code}");

        Result<string> presentResult = present.ToResult(Error.NotFound("User.NotFound", "User not found."));
        Console.WriteLine($"Converted present to Result: IsSuccess={presentResult.IsSuccess}, Value={presentResult.Value}");

        // -------------------------------------------------------------
        // 5. Asynchronous Maybe Extensions
        // -------------------------------------------------------------
        Console.WriteLine("\n[5] Asynchronous Task<Maybe<T>> Pipeline:");

        Task<Maybe<string>> FetchUserEmailAsync(int id) => Task.FromResult(id == 1 ? Maybe<string>.From("admin@system.local") : Maybe<string>.None);

        Result<string> asyncResult = await FetchUserEmailAsync(1)
            .Map(email => email.ToUpperInvariant())
            .ToResult(Error.NotFound("User.NotFound", "User was not found"));

        Console.WriteLine($"Async Maybe pipeline result: {asyncResult.GetValueOrDefault("None")}");
    }
}
