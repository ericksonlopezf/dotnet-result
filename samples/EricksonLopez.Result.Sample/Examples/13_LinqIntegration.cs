// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class LinqIntegration
{
    public static void Run()
    {
        Console.WriteLine("\n--- 13. LINQ INTEGRATION ---");

        // The library provides extensions that enable C# Query Syntax (LINQ) over Result<T>
        Result<int> GetUserId() => Result.Success(10);
        Result<string> GetUserName(int id) => Result.Success($"User_{id}");
        Result<bool> IsActive(string userName) => Result.Success(true);

        // 1. SelectMany and Select
        // This allows chaining multiple Result-returning functions cleanly using `from ... in ...`
        Result<string> combinedResult =
            from id in GetUserId()
            from name in GetUserName(id)
            from active in IsActive(name)
            select $"{name} is active: {active}";

        Console.WriteLine($"LINQ Query Syntax Result: {combinedResult.GetValueOrDefault("")}");

        // 2. Where (Filtering)
        // You can use `where` to apply a predicate. If it fails, it returns a validation error.
        Result<int> filteredResult =
            from id in GetUserId()
            where id > 5
            select id;

        Console.WriteLine($"LINQ Where (Success): {filteredResult.IsSuccess}");

        Result<int> failedFilter =
            from id in GetUserId()
            where id > 100
            select id;

        Console.WriteLine($"LINQ Where (Failed): {failedFilter.IsFailure}, Error: {failedFilter.Error?.Code}");
    }
}
