# Level 04 — Advanced Integration: Composition, Combine & LINQ

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** Senior Software Engineers, Solution Architects | **Complexity:** Level 4 (Advanced) | **Code Reference:** `06_Combining.cs`, `09_CumulativeValidation.cs`, `13_LinqIntegration.cs`, `19_CombineAndMergeAdvanced.cs` | **Language:** English

---

## 1. Heterogeneous Result Composition (`Result.Combine`)

Merges multiple independent or concurrent operations with differing return types into a single, strongly-typed tuple result:

```csharp
Result<User> userResult = GetUser(userId);
Result<Account> accountResult = GetAccount(accountId);
Result<Permissions> permResult = GetPermissions(userId);

// Combines heterogeneous results into a strongly-typed 3-tuple
Result<(User, Account, Permissions)> combined = Result.Combine(userResult, accountResult, permResult);

if (combined.TryGetValue(out var data))
{
    var (user, account, permissions) = data;
    Console.WriteLine($"Profile ready for {user.Name}, balance {account.Balance}");
}
else
{
    // If any operand failed, combined contains the failure diagnostics of the first failing step
    Console.WriteLine($"Failed to assemble profile: {combined.Error.Code}");
}
```

---

## 2. Guard Conditions with `Result.Merge`

`Result.Merge` conditions a value-carrying result (`Result<T>`) upon the success of a non-generic guard result (`Result`):

```csharp
Result authCheck = VerifyUserSession(token);
Result<Document> docResult = LoadDocument(docId);

// If authCheck is a failure, merged returns a failed Result<Document> propagating authCheck.Error
Result<Document> merged = Result.Merge(authCheck, docResult);
```

---

## 3. High-Performance Cumulative Validation (`Result.ValidateAll`)

Unlike short-circuiting combinators (`Ensure`, `Bind`), user registration, DTO ingestion, and domain rule engines often require evaluating all rules and collecting every failure before returning:

```csharp
public static Result<RegistrationForm> Validate(RegistrationForm form)
{
    return Result.ValidateAll(form,
        f => string.IsNullOrWhiteSpace(f.Username) 
            ? Result.Failure(Error.Validation("User.Required", "Username is required.")) 
            : Result.Success(),
        f => !f.Email.Contains('@') 
            ? Result.Failure(Error.Validation("Email.Invalid", "Invalid email format.")) 
            : Result.Success(),
        f => f.Age < 18 
            ? Result.Failure(Error.Validation("Age.Underage", "User must be 18 or older.")) 
            : Result.Success()
    );
}
```

### Performance Characteristics of `ValidateAll`:
- **Zero GC Heap Allocations on Happy Path**: Leases temporary buffer arrays from `ArrayPool<Error>.Shared` to collect errors without allocating arrays on the garbage collector heap.
- If no rules fail, it immediately returns `Result.Success(instance)`.
- If exactly one rule fails, it returns that individual `Error` directly without creating synthetic wrappers.
- If two or more rules fail, it returns an aggregated `Error.Validation` with code `WellKnownErrors.CombinedFailuresCode` ("Result.CombinedErrors"), attaching all individual failures inside `InnerErrors`.

---

## 4. LINQ Query Comprehension Syntax (`ResultLinqExtensions`)

`EricksonLopez.Result` implements `Select`, `SelectMany`, and `Where` operator patterns, enabling natural C# LINQ query syntax over results:

```csharp
Result<int> r1 = Result.Success(10);
Result<int> r2 = Result.Success(20);

Result<int> query = 
    from a in r1
    from b in r2
    where a + b > 15
    select a * b;

// query.Value == 200
```

If any step in the query fails or a `where` predicate evaluates to `false`, the pipeline short-circuits automatically and returns `WellKnownErrors.FilteredOut`.

---

## Next Level
Proceed to **[Level 05 — Processing](level-05-processing.md)** to explore asynchronous workflows, `ValueTask` deadlock avoidance, cooperative cancellation via `CancellationToken`, and exception translation boundaries.
