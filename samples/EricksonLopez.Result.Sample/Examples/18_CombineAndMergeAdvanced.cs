// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class CombineAndMergeAdvanced
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 18. COMBINE (ADVANCED) & MERGE");
        Console.WriteLine("========================================================");

        // -------------------------------------------------------------
        // 1. Combine(params Result[]) — non-generic, multiple guard results
        // -------------------------------------------------------------
        // Returns Result.Success() only if ALL inputs succeed.
        // Returns a compound failure (with inner errors) if ANY fail.
        Console.WriteLine("\n[1] Combine(params Result[]) — aggregate non-generic results:");

        Result authCheck   = Result.Success();
        Result rateLimit   = Result.Success();
        Result featureFlag = Result.Failure(Error.Forbidden("Feature.Disabled", "This feature is not enabled."));

        Result combined = Result.Combine(authCheck, rateLimit, featureFlag);
        Console.WriteLine($"  Combined IsSuccess: {combined.IsSuccess}, IsFailure: {combined.IsFailure}");
        if (combined.IsFailure)
            Console.WriteLine($"  Failure Code: {combined.Error.Code}");

        // All succeed
        Result allOk = Result.Combine(
            Result.Success(),
            Result.Success(),
            Result.Success());
        Console.WriteLine($"  All succeed: IsSuccess={allOk.IsSuccess}");

        // Multiple failures are aggregated into inner errors
        Result multiFailure = Result.Combine(
            Result.Failure(Error.Validation("A.Missing", "Field A is required.")),
            Result.Success(),
            Result.Failure(Error.Validation("B.Missing", "Field B is required."))
        );
        Console.WriteLine($"\n  Multiple failures aggregated: IsFailure={multiFailure.IsFailure}");
        Console.WriteLine($"  Aggregate Code: {multiFailure.Error.Code}");
        if (multiFailure.Error.HasInnerErrors)
        {
            foreach (var inner in multiFailure.Error.InnerErrors)
                Console.WriteLine($"    -> [{inner.Code}]: {inner.Description}");
        }

        // -------------------------------------------------------------
        // 2. Combine<T>(params Result<T>[]) — homogeneous typed list → IReadOnlyList<T>
        // -------------------------------------------------------------
        // When ALL inputs succeed, returns Result<IReadOnlyList<T>> containing all values.
        // When ANY fail, returns a compound failure.
        Console.WriteLine("\n[2] Combine<T>(params Result<T>[]) — homogeneous collection:");

        Result<int> score1 = Result.Success(85);
        Result<int> score2 = Result.Success(92);
        Result<int> score3 = Result.Success(78);

        // Pass an explicit array to select the IReadOnlyList<T> overload,
        // avoiding ambiguity with the tuple overload (Combine<T1,T2,T3>) when
        // all elements have the same type.
        Result<IReadOnlyList<int>> allScores = Result.Combine<int>(new[] { score1, score2, score3 });
        if (allScores.IsSuccess)
        {
            Console.WriteLine($"  All scores: [{string.Join(", ", allScores.Value)}]");
            Console.WriteLine($"  Average: {ComputeAverage(allScores.Value):F1}");
        }

        // With a failure mixed in
        Result<int> invalidScore = Result.Failure<int>(Error.Validation("Score.Range", "Score must be 0-100."));
        Result<IReadOnlyList<int>> partialFailure = Result.Combine<int>(new[] { score1, invalidScore, score3 });
        Console.WriteLine($"  Partial failure: IsFailure={partialFailure.IsFailure}");

        // -------------------------------------------------------------
        // 3. Combine<T1, T2> — heterogeneous typed tuple (2-5 args)
        //    (already covered in example 06 with the 3-arg form, shown here for 2 and 4)
        // -------------------------------------------------------------
        Console.WriteLine("\n[3] Combine<T1,T2> (heterogeneous 2-arg tuple):");

        Result<string> userId   = Result.Success("usr-123");
        Result<string> userName = Result.Success("erickson");

        Result<(string Id, string Name)> user = Result.Combine(userId, userName);
        if (user.IsSuccess)
        {
            var (id, name) = user.Value;
            Console.WriteLine($"  User: Id={id}, Name={name}");
        }

        // 4-arg overload
        Console.WriteLine("\n[4] Combine<T1,T2,T3,T4> (4-arg tuple):");

        Result<string>   firstName = Result.Success("Erickson");
        Result<string>   lastName  = Result.Success("Lopez");
        Result<int>      age       = Result.Success(30);
        Result<string>   email     = Result.Success("dev@ericksonlopez.dev");

        Result<(string, string, int, string)> profile = Result.Combine(firstName, lastName, age, email);
        if (profile.IsSuccess)
        {
            var (fn, ln, a, em) = profile.Value;
            Console.WriteLine($"  Profile: {fn} {ln}, Age={a}, Email={em}");
        }

        // When one of the four fails — all failures aggregated
        Result<string> badEmail = Result.Failure<string>(Error.Validation("Email.Invalid", "Not a valid email."));
        Result<(string, string, int, string)> badProfile = Result.Combine(firstName, lastName, age, badEmail);
        Console.WriteLine($"  4-arg with failure: IsFailure={badProfile.IsFailure}, Code={badProfile.Error.Code}");

        // -------------------------------------------------------------
        // 5. Result.Merge — guard check before a typed result
        // -------------------------------------------------------------
        // Merge(guard: Result, next: Result<T>) returns:
        //   - The failure from `guard` if it failed
        //   - The `next` result if guard succeeded
        // This is the idiomatic way to apply a non-generic guard
        // before propagating a typed result.
        Console.WriteLine("\n[5] Result.Merge — guard + typed result:");

        Result authorizationGuard = Result.Success();  // e.g., user is authenticated
        Result<string> payload    = Result.Success("Sensitive data");

        // Guard passes → payload flows through
        Result<string> merged = Result.Merge(authorizationGuard, payload);
        Console.WriteLine($"  Guard OK: merged.IsSuccess={merged.IsSuccess}, Value=\"{merged.GetValueOrDefault("")}\"");

        // Guard fails → failure propagated, payload ignored
        Result failedGuard = Result.Failure(Error.Unauthorized("Auth.Required", "Token expired."));
        Result<string> blockedByGuard = Result.Merge(failedGuard, payload);
        Console.WriteLine($"  Guard FAIL: merged.IsFailure={blockedByGuard.IsFailure}, Code={blockedByGuard.Error.Code}");

        // Common pattern: validate with DiscardValue + Merge
        Result<int> validatedInput = Result.Success(42)
            .Ensure(v => v > 0, Error.Validation("Val.NonPositive", "Must be positive."));

        Result nonGenericGuard = validatedInput.DiscardValue(); // drop value type
        Result<string> downstream = Result.Success("downstream data");
        Result<string> guardedDownstream = Result.Merge(nonGenericGuard, downstream);
        Console.WriteLine($"  DiscardValue+Merge chain: IsSuccess={guardedDownstream.IsSuccess}");
    }

    private static double ComputeAverage(IReadOnlyList<int> scores)
    {
        double sum = 0;
        for (int i = 0; i < scores.Count; i++) sum += scores[i];
        return scores.Count == 0 ? 0 : sum / scores.Count;
    }
}
