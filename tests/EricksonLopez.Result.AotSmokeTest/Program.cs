// ============================================================================
// NativeAOT Smoke Test — EricksonLopez.Result
// ============================================================================
// This program is compiled and published with PublishAot=true in CI to validate
// that the Result core and AspNetCore packages are genuinely NativeAOT-compatible.
//
// The test is intentionally minimal: it exercises every top-level API path
// (success, failure, error construction, monadic operations, HTTP options) in a
// way that forces the NativeAOT compiler (ILC) to analyze every referenced code
// path. If any API secretly relies on reflection or dynamic code, the ILC will
// emit IL2026 / IL3050 warnings that are treated as errors in the workflow.
//
// Exit codes:
//   0  — all validations passed
//   1  — a validation assertion failed (prints the failure message to stderr)
// ============================================================================

using System;
using System.Collections.Immutable;
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using Microsoft.AspNetCore.Http;

Console.WriteLine("=== EricksonLopez.Result NativeAOT Smoke Test ===");
Console.WriteLine($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier}");
Console.WriteLine($"Framework: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
Console.WriteLine();

int failureCount = 0;

void Assert(bool condition, string testName, string? failureMessage = null)
{
    if (condition)
    {
        Console.WriteLine($"  [PASS] {testName}");
    }
    else
    {
        Console.Error.WriteLine($"  [FAIL] {testName}: {failureMessage ?? "assertion failed"}");
        failureCount++;
    }
}

// ─── 1. Result (non-generic) ──────────────────────────────────────────────

Console.WriteLine("--- 1. Result (non-generic) ---");
var success = Result.Success();
Assert(success.IsSuccess, "Result.Success().IsSuccess == true");
Assert(!success.IsFailure, "Result.Success().IsFailure == false");
Assert(!success.IsUninitialized, "Result.Success().IsUninitialized == false");

var error = Error.Failure("Smoke.Failure", "Smoke test failure");
var failure = Result.Failure(error);
Assert(!failure.IsSuccess, "Result.Failure().IsSuccess == false");
Assert(failure.IsFailure, "Result.Failure().IsFailure == true");
Assert(failure.Error.Code == "Smoke.Failure", $"Result.Failure().Error.Code == 'Smoke.Failure', got '{failure.Error.Code}'");

var defaultResult = default(Result);
Assert(defaultResult.IsUninitialized, "default(Result).IsUninitialized == true");
Assert(!defaultResult.IsSuccess, "default(Result).IsSuccess == false");

// ─── 2. Result<T> (generic) ───────────────────────────────────────────────

Console.WriteLine("--- 2. Result<T> (generic) ---");
var successT = Result.Success(42);
Assert(successT.IsSuccess, "Result<int>.Success(42).IsSuccess == true");
Assert(successT.Value == 42, $"Result<int>.Success(42).Value == 42, got {successT.Value}");

var failureT = Result.Failure<int>(error);
Assert(failureT.IsFailure, "Result<int>.Failure().IsFailure == true");
Assert(failureT.Error.Code == "Smoke.Failure", "Result<int>.Failure().Error.Code matches");

var defaultT = default(Result<string>);
Assert(defaultT.IsUninitialized, "default(Result<string>).IsUninitialized == true");

// ─── 3. Monadic operations ────────────────────────────────────────────────

Console.WriteLine("--- 3. Monadic operations ---");
var mapped = successT.Map(x => x * 2);
Assert(mapped.IsSuccess && mapped.Value == 84, $"Map(x=>x*2): expected 84, got {(mapped.IsSuccess ? mapped.Value : "failure")}");

var bound = successT.Bind(x => x > 0 ? Result.Success(x.ToString(System.Globalization.CultureInfo.InvariantCulture)) : Result.Failure<string>(error));
Assert(bound.IsSuccess && bound.Value == "42", $"Bind: expected '42', got {(bound.IsSuccess ? bound.Value : "failure")}");

var matched = successT.Match(onSuccess: v => $"ok:{v}", onFailure: e => $"err:{e.Code}");
Assert(matched == "ok:42", $"Match on success: expected 'ok:42', got '{matched}'");

var matchedFailure = failureT.Match(onSuccess: v => $"ok:{v.ToString(System.Globalization.CultureInfo.InvariantCulture)}", onFailure: e => $"err:{e.Code}");
Assert(matchedFailure == "err:Smoke.Failure", $"Match on failure: expected 'err:Smoke.Failure', got '{matchedFailure}'");

var ensured = successT.Ensure(x => x > 0, Error.Validation("Smoke.Range", "Must be positive"));
Assert(ensured.IsSuccess, "Ensure(true predicate) keeps success");

var ensuredFail = successT.Ensure(x => x < 0, Error.Validation("Smoke.Range", "Must be negative"));
Assert(ensuredFail.IsFailure, "Ensure(false predicate) converts to failure");

var recovered = failureT.Recover(_ => Result.Success(0));
Assert(recovered.IsSuccess && recovered.Value == 0, $"Recover: expected 0, got {(recovered.IsSuccess ? recovered.Value : "failure")}");

// ─── 4. TryGetValue / TryGetError ─────────────────────────────────────────

Console.WriteLine("--- 4. TryGetValue / TryGetError ---");
bool gotValue = successT.TryGetValue(out var value);
Assert(gotValue && value == 42, $"TryGetValue on success: {gotValue}, value={value}");

bool gotError = failureT.TryGetError(out var err);
Assert(gotError && err?.Code == "Smoke.Failure", $"TryGetError on failure: {gotError}");

// ─── 5. Error construction ────────────────────────────────────────────────

Console.WriteLine("--- 5. Error construction ---");
var richError = Error.Create("Order.Expired", "Order has expired")
    .WithType(ErrorType.Domain)
    .WithSeverity(ErrorSeverity.Warning)
    .WithRetryability(ErrorRetryability.Permanent)
    .WithCorrelationId("corr-123")
    .WithMetadata("orderId", "ORD-001")
    .Build();

Assert(richError.Code == "Order.Expired", $"ErrorBuilder.Code: '{richError.Code}'");
Assert(richError.Type == ErrorType.Domain, $"ErrorBuilder.Type: {richError.Type}");
Assert(richError.Severity == ErrorSeverity.Warning, $"ErrorBuilder.Severity: {richError.Severity}");
Assert(richError.Retryability == ErrorRetryability.Permanent, $"ErrorBuilder.Retryability: {richError.Retryability}");
Assert(richError.CorrelationId == "corr-123", $"ErrorBuilder.CorrelationId: '{richError.CorrelationId}'");
Assert(richError.HasMetadata, "ErrorBuilder.HasMetadata == true");

// Use ErrorBuilder (via Error.Create()) to access WithInnerError —
// Error.Validation() returns a built Error; only ErrorBuilder has With*() methods.
var withInner = Error.Create("Parent.Error", "Parent")
    .WithType(ErrorType.Validation)
    .WithInnerError(Error.Validation("Child.Error", "Child 1"))
    .WithInnerError(Error.Validation("Child.Error2", "Child 2"))
    .Build();
Assert(withInner.InnerErrors.Length == 2, $"WithInnerError count: expected 2, got {withInner.InnerErrors.Length}");

// ─── 6. Well-known error factories ───────────────────────────────────────

Console.WriteLine("--- 6. Well-known error factories ---");
var errFailure = Error.Failure("F", "Failure");
var errValidation = Error.Validation("V", "Validation");
var errNotFound = Error.NotFound("NF", "Not Found");
var errConflict = Error.Conflict("C", "Conflict");
var errUnauthorized = Error.Unauthorized("U", "Unauthorized");
var errForbidden = Error.Forbidden("FB", "Forbidden");
var errUnavailable = Error.Unavailable("UA", "Unavailable");
var errUnexpected = Error.Unexpected("UX", "Unexpected");
Assert(errFailure.Type == ErrorType.Failure, "Error.Failure().Type");
Assert(errValidation.Type == ErrorType.Validation, "Error.Validation().Type");
Assert(errNotFound.Type == ErrorType.NotFound, "Error.NotFound().Type");
Assert(errConflict.Type == ErrorType.Conflict, "Error.Conflict().Type");
Assert(errUnauthorized.Type == ErrorType.Unauthorized, "Error.Unauthorized().Type");
Assert(errForbidden.Type == ErrorType.Forbidden, "Error.Forbidden().Type");
Assert(errUnavailable.Type == ErrorType.Unavailable, "Error.Unavailable().Type");
Assert(errUnexpected.Type == ErrorType.Unexpected, "Error.Unexpected().Type");

// ─── 7. Result.Combine ────────────────────────────────────────────────────

Console.WriteLine("--- 7. Result.Combine ---");
var r1 = Result.Success();
var r2 = Result.Success();
var r3 = Result.Failure(Error.Validation("V1", "Error 1"));
var r4 = Result.Failure(Error.Validation("V2", "Error 2"));

var allSuccess = Result.Combine(r1, r2);
Assert(allSuccess.IsSuccess, "Combine(all success) is success");

var mixed = Result.Combine(r1, r3, r4);
Assert(mixed.IsFailure, "Combine(with failures) is failure");
Assert(mixed.Error.InnerErrors.Length == 2, $"Combine errors count: expected 2, got {mixed.Error.InnerErrors.Length}");

// ─── 8. AspNetCore — ResultHttpOptions (reflection-free) ──────────────────

Console.WriteLine("--- 8. AspNetCore — ResultHttpOptions ---");
var options = new ResultHttpOptions();
Assert(options.DefaultSuccessStatusCode == StatusCodes.Status204NoContent, "DefaultSuccessStatusCode == 204");
Assert(!options.IncludeTraceId, "IncludeTraceId defaults to false (secure-by-default)");
Assert(!options.IncludeDescription, "IncludeDescription defaults to false (secure-by-default)");
Assert(options.TypeUriBase == "about:blank", "TypeUriBase defaults to 'about:blank'");

// Verify public StatusCodeMap property reflects the expected defaults.
// GetFrozenStatusCodeMap() and IsFrozen are internal; test the observable public behavior.
var statusCodeMap = options.StatusCodeMap;
Assert(statusCodeMap.ContainsKey(ErrorType.Failure), "StatusCodeMap has ErrorType.Failure");
Assert(statusCodeMap[ErrorType.Validation] == StatusCodes.Status400BadRequest, $"Validation→400, got {statusCodeMap[ErrorType.Validation]}");
Assert(statusCodeMap[ErrorType.NotFound] == StatusCodes.Status404NotFound, $"NotFound→404");
Assert(statusCodeMap[ErrorType.Unauthorized] == StatusCodes.Status401Unauthorized, "Unauthorized→401");
Assert(statusCodeMap[ErrorType.Custom] == StatusCodes.Status500InternalServerError, $"Custom→500, got {statusCodeMap[ErrorType.Custom]}");

// Verify mutations are guarded: configure a custom mapping then verify it is reflected
options.ConfigureStatusCode(ErrorType.Domain, StatusCodes.Status422UnprocessableEntity);
Assert(options.StatusCodeMap[ErrorType.Domain] == StatusCodes.Status422UnprocessableEntity, "ConfigureStatusCode sets Domain→422");

// ─── 9. Error equality and ToString ──────────────────────────────────────

Console.WriteLine("--- 9. Error equality and ToString ---");
var e1 = Error.Failure("Code.A", "Desc A");
var e2 = Error.Failure("Code.A", "Desc A");
var e3 = Error.Failure("Code.B", "Desc B");
Assert(e1.Equals(e2), "Same-code errors are equal");
Assert(!e1.Equals(e3), "Different-code errors are not equal");
Assert(e1 == e2, "== operator works");
Assert(e1 != e3, "!= operator works");
Assert(e1.GetHashCode() == e2.GetHashCode(), "Equal errors have same hash code");

var str = e1.ToString();
Assert(str.Contains("Code.A"), $"Error.ToString() contains code: '{str}'");

// ─── 10. Implicit conversions ─────────────────────────────────────────────

Console.WriteLine("--- 10. Implicit conversions ---");
Result<int> fromValue = 99;
Assert(fromValue.IsSuccess && fromValue.Value == 99, "implicit (int → Result<int>) from value");

Result<int> fromError = Error.Failure("Impl.Error", "Implicit");
Assert(fromError.IsFailure, "implicit (Error → Result<int>) from error");

// ─── 11. Deconstruct ─────────────────────────────────────────────────────

Console.WriteLine("--- 11. Deconstruct ---");
var (isOk, val, decError) = successT;
Assert(isOk && val == 42 && decError == null, $"Deconstruct success: isOk={isOk}, val={val}, error={decError}");

var (isFail, failVal, failDecError) = failureT;
Assert(!isFail && failDecError != null, $"Deconstruct failure: isFail={isFail}, error={failDecError?.Code}");

// ─── Final Summary ────────────────────────────────────────────────────────

Console.WriteLine();
if (failureCount == 0)
{
    Console.WriteLine($"=== ALL VALIDATIONS PASSED ===");
    return 0;
}
else
{
    Console.Error.WriteLine($"=== {failureCount} VALIDATION(S) FAILED ===");
    return 1;
}
