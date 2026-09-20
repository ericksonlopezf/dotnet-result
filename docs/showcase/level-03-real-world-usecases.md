# Level 03 — Real-World Use Cases: Domain Pipelines & Business Logic

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** Software Engineers, Backend Developers | **Complexity:** Level 3 (Intermediate) | **Code Reference:** `04_MappingAndChaining.cs`, `11_GenericResult.cs` | **Language:** English

---

## 1. Scenario 1: User Registration & Activation Pipeline

Demonstrates an end-to-end Railway-Oriented Programming pipeline where each step depends on the success of the previous one. If any invariant fails, the pipeline short-circuits immediately without throwing an exception:

```csharp
public async Task<Result<UserDto>> RegisterUserAsync(RegisterUserCommand cmd, CancellationToken ct = default)
{
    return await ValidateCommand(cmd)
        .Ensure(c => !string.IsNullOrWhiteSpace(c.Email), Error.Validation("Email.Empty", "Email is required."))
        .Ensure(c => c.Email.Contains('@'), Error.Validation("Email.Format", "Email format is invalid."))
        .Bind(c => CheckEmailAvailableAsync(c.Email, ct))
        .Bind(c => HashPasswordAsync(c, ct))
        .Bind(c => SaveUserToRepositoryAsync(c, ct))
        .TapOnSuccess(user => _emailService.SendWelcomeEmail(user.Email))
        .Map(user => new UserDto(user.Id, user.Email, user.FullName));
}
```

---

## 2. Scenario 2: Transactional Payment Processing

Handles financial business rules by distinguishing semantic domain violations (insufficient funds) from transient infrastructure failures:

```csharp
public async Task<Result<TransactionReceipt>> ProcessPaymentAsync(PaymentRequest req, CancellationToken ct = default)
{
    return await CheckAccountActiveAsync(req.AccountId, ct)
        .Ensure(acc => acc.Balance >= req.Amount, 
            Error.Conflict("Account.InsufficientFunds", "Current balance is lower than requested amount."))
        .Bind(acc => ChargeExternalGatewayAsync(acc, req.Amount, ct))
        .TapOnFailure(err =>
        {
            if (err.Retryability == ErrorRetryability.Transient)
            {
                _logger.LogWarning("Payment gateway timeout for account {Id}. Enqueuing for background retry.", req.AccountId);
            }
        })
        .Map(chargeReceipt => new TransactionReceipt(chargeReceipt.TxnId, req.Amount, DateTime.UtcNow));
}
```

---

## 3. Scenario 3: Strict Domain Modeling with `Result<TValue, TError>`

For Domain-Driven Design (DDD) aggregates and microservices where errors must be modeled as strict compile-time class or record hierarchies rather than generalized strings, the `EricksonLopez.Result.Generic` package provides `Result<TValue, TError>`:

```csharp
using EricksonLopez.Result.Generic;

public abstract record DomainError(string Message);
public sealed record InsufficientFundsError(decimal CurrentBalance, decimal Requested) 
    : DomainError($"Balance {CurrentBalance:C} is less than {Requested:C}");
public sealed record AccountLockedError(string Reason) 
    : DomainError($"Account locked: {Reason}");

public static class BankAccountService
{
    public static Result<decimal, DomainError> Withdraw(decimal balance, decimal amount, bool isLocked)
    {
        if (isLocked)
            return Result<decimal, DomainError>.Failure(new AccountLockedError("Fraud review in progress"));

        if (balance < amount)
            return Result<decimal, DomainError>.Failure(new InsufficientFundsError(balance, amount));

        return Result<decimal, DomainError>.Success(balance - amount);
    }
}
```

### Projecting to Standard `Result<T>`
Any `Result<TValue, TError>` can be projected back to the standard ecosystem envelope using `.ToResult()`:
```csharp
EricksonLopez.Result.Result<decimal> standardResult = bankResult.ToResult(err => err switch
{
    InsufficientFundsError ife => Error.Conflict("Account.InsufficientFunds", ife.Message),
    AccountLockedError ale => Error.Forbidden("Account.Locked", ale.Message),
    _ => Error.Failure("Account.Error", err.Message)
});
```

---

## Next Level
Proceed to **[Level 04 — Advanced Integration](level-04-advanced-integration.md)** to master heterogeneous tuple composition (`Combine`), batch error aggregations (`Merge`), cumulative validation (`ValidateAll`), and LINQ query comprehension syntax.
