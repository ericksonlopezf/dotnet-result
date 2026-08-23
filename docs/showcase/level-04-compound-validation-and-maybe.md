# Level 04 — Compound Validation & Maybe Monad

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** Senior Engineers & Architects | **Language:** English

---

## 1. The Challenge: Fail-Fast vs Fail-All Validation

- **Fail-Fast (`Bind` / `Ensure`)**: Aborts computation at the first failing rule. Ideal for sequential business logic (e.g. don't charge credit card if stock reservation failed).
- **Fail-All (Compound Validation)**: Evaluates *all* validation rules across a complex payload and aggregates every error into a composite diagnostic report. Essential for user registration and form submissions.

---

## 2. Zero-Allocation `Result.ValidateAll`

`Result.ValidateAll` executes a collection or span of validation rules and collects all failures without generating GC heap pressure:

```csharp
using EricksonLopez.Result;

public Result ValidateUserRegistration(CreateUserRequest request)
{
    ReadOnlySpan<Func<Result>> rules = 
    [
        () => !string.IsNullOrWhiteSpace(request.Username)
            ? Result.Success()
            : Error.Validation("User.UsernameRequired", "Username cannot be empty."),
            
        () => request.Username?.Length >= 3
            ? Result.Success()
            : Error.Validation("User.UsernameTooShort", "Username must be at least 3 characters."),
            
        () => IsValidEmail(request.Email)
            ? Result.Success()
            : Error.Validation("User.InvalidEmail", "Email address format is invalid."),
            
        () => request.Age >= 18
            ? Result.Success()
            : Error.Validation("User.Underage", "User must be 18 years or older.")
    ];

    // Evaluates all rules; if any fail, combines errors into a single Error.Validation composite!
    return Result.ValidateAll(rules);
}
```

### Inner Errors Inspection:
When `ValidateAll` fails, the returned `Error` contains the aggregate list under `Error.InnerErrors`:

```csharp
var validation = ValidateUserRegistration(invalidRequest);
if (validation.IsFailure)
{
    foreach (var error in validation.Error.InnerErrors)
    {
        Console.WriteLine($"Field Rule Failed: {error.Code} -> {error.Description}");
    }
}
```

---

## 3. The `Maybe<T>` Option Monad

In domain modeling, returning `null` often leads to `NullReferenceException`. The `EricksonLopez.Result.Maybe` package provides a zero-allocation `readonly struct Maybe<T>` to explicitly represent optional values:

```bash
dotnet add package EricksonLopez.Result.Maybe
```

### Usage & Conversion to `Result<T>`:

```csharp
using EricksonLopez.Result;
using EricksonLopez.Result.Maybe;

public Maybe<Customer> FindByTaxId(string taxId)
{
    var customer = _dbContext.Customers.FirstOrDefault(c => c.TaxId == taxId);
    return Maybe<Customer>.From(customer); // Returns Some(customer) or None
}

// Convert Maybe<T> to Result<T> seamlessly:
Result<Customer> result = FindByTaxId("TX-12345")
    .ToResult(Error.NotFound("Customer.NotFound", "No customer found with the given Tax ID."));
```

---

## Next Steps
Proceed to [Level 05 — ASP.NET Core & RFC 9457 ProblemDetails](level-05-aspnetcore-problem-details.md) to discover seamless Minimal APIs endpoint integration and HTTP status code mappings.
