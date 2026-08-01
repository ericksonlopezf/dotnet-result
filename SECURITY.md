# Security Policy

## Supported Versions

The following versions of `EricksonLopez.Result` are currently supported with security updates:

| Version | Supported |
|---|---|
| 1.0.x | ✅ Yes |
| < 1.0.0 | ❌ No |

## Reporting a Vulnerability

We take the security of `EricksonLopez.Result` seriously. If you believe you have found a security vulnerability in any of our packages, please report it to us as soon as possible.

**Do NOT report security vulnerabilities through public GitHub issues.**

Instead, please send an email to:
📧 **[ericksonlopez.dev@gmail.com](mailto:ericksonlopez.dev@gmail.com)**

Please include:
- A description of the vulnerability.
- Steps to reproduce or proof-of-concept code.
- Impact assessment.

### Disclosure Policy

- We will acknowledge receipt of your vulnerability report within 48 hours.
- We will provide an estimated timeline for remediation.
- Once a fix is released, we will publicly credit you in the release notes (unless you prefer to remain anonymous).

## Supply Chain Security

`EricksonLopez.Result` implements multiple supply chain security measures:

### Sigstore Provenance Attestation

All NuGet packages are built with [Sigstore](https://www.sigstore.dev/) provenance attestations via `actions/attest-build-provenance@v2` (SLSA v1.0 predicate format), providing cryptographic proof that packages were built from this repository's CI pipeline. Consumers can verify via:

```bash
gh attestation verify <package.nupkg> --repo ericksonlopez/dotnet-result
```

### NuGet Trusted Publishing (OIDC)

Package publishing to NuGet.org uses OpenID Connect (OIDC) authentication via `NuGet/login@v1` instead of static API keys, eliminating the risk of long-lived credential exposure.

### Strong Name Signing

All assemblies are signed with a strong name key (`EricksonLopez.Result.snk`). The public key (`public.snk`) is committed to the repository. The private key is stored as a GitHub secret and restored at build time.

### NuGet Dependency Auditing

All projects run NuGet security audits during restore (`NuGetAudit=true`, `NuGetAuditLevel=low`) to detect vulnerable transitive dependencies.

## Information Disclosure Risks

### `ResultExceptionBehavior` + `IncludeDescription = true` (PII / Secret Exposure)

**Severity: Medium — configuration-dependent**

`ResultExceptionBehavior<TRequest, TResponse>` (in `EricksonLopez.Result.MediatR`) catches unhandled exceptions and maps them to `Result.Failure` using the caught exception's message as the `Error.Description`:

```csharp
// Internally, the behavior does roughly:
Error.Create(code: "Unexpected", description: ex.Message)
```

`Exception.Message` may contain **sensitive information** including:

- Connection strings (e.g., `SqlException`: `"Login failed for user 'sa' at server 'prod-db-01'"`)
- File system paths (e.g., `FileNotFoundException`: `"Could not find file 'C:\secrets\appsettings.prod.json'"`)
- PII (e.g., validation errors with user data embedded in the message)
- Internal infrastructure names (hostnames, service names, queue names)

**If `IncludeDescription = true` is set in `ResultHttpOptions`**, this description will be included in the HTTP response body (via ProblemDetails `detail` field), potentially exposing the above information to API clients.

#### Mitigations

1. **Default is safe**: `IncludeDescription = false` by default. No description is sent over the wire unless explicitly opted in.

2. **`RESULT009` analyzer** (`IncludeDescriptionSecurityAnalyzer`) warns at compile time when `IncludeDescription = true` is set as a literal without an environment guard.

3. **Use environment guards** when enabling description in non-production environments:
   ```csharp
   services.AddResultAspNetCore(options =>
   {
       // Only include description in Development — never in Production
       options.IncludeDescription = builder.Environment.IsDevelopment();
   });
   ```

4. **Custom error factory in `ResultExceptionBehavior`**: Use the `errorFactory` parameter to suppress or sanitize `ex.Message`:
   ```csharp
   services.AddResultExceptionBehavior(errorFactory: (ex, ct) =>
       Error.Create("Unexpected", "An unexpected error occurred.") // no ex.Message
   );
   ```

### `Error.Description` as a General PII Surface

Any `Error.Description` set by the application developer can contain user data or system internals. The risk applies to all `Error` objects, not just those created by `ResultExceptionBehavior`. Apply the same `IncludeDescription` and environment guard discipline to all result failure paths that may carry sensitive descriptions.

---

### `ResultExceptionBehavior` — `Error.Code` with Exception Type Name (Infrastructure Disclosure)

**Severity: Low-Medium — always active when using the default errorFactory**

The default `errorFactory` of `ResultExceptionBehavior<TRequest, TResponse>` generates an `Error.Code` using the exception's runtime type name:

```csharp
// Default error factory (ResultExceptionBehavior.cs ~line 107):
Error.Unexpected(
    $"Handler.{ex.GetType().Name}",     // ← always included in errorCode
    "An unexpected handler error occurred.");
```

This produces codes like `"Handler.SqlException"`, `"Handler.HttpRequestException"`, `"Handler.SocketException"` which are **always serialized as `errorCode` in ProblemDetails responses**, regardless of the `IncludeDescription` setting.

**Risk**: An observer can enumerate exception types from your backend by monitoring `errorCode` values in API responses, inferring:
- Database technology (`SqlException` → SQL Server; `NpgsqlException` → PostgreSQL)
- Network dependencies (`HttpRequestException` → downstream HTTP calls)
- Queue technology (`RabbitMqClientException`, `ServiceBusException`)

This is a **reconnaissance vector** in threat models that require full backend opacity.

**Unlike `ex.Message` (controlled by `IncludeDescription`)**, `ex.GetType().Name` is not suppressed by any existing configuration option.

#### Mitigations

1. **Provide a custom `errorFactory`** that uses a fixed, generic `Error.Code`:
   ```csharp
   services.AddResultExceptionBehavior(errorFactory: (ex, ct) =>
       Error.Unexpected("Handler.Unexpected", "An unexpected error occurred."));
   ```

2. **Category-based codes** (reveals less than type name, still useful for monitoring):
   ```csharp
   services.AddResultExceptionBehavior(errorFactory: (ex, ct) =>
       Error.Unexpected(
           ex is OperationCanceledException ? "Handler.Cancelled" : "Handler.Unexpected",
           "An unexpected error occurred."));
   ```

3. **Accept the default** if your API is internal-only or your threat model does not require backend opacity.

#### Why the default is not changed

The default `"Handler.{ex.GetType().Name}"` pattern is retained because it provides valuable signal for internal APM dashboards and error monitoring (e.g., grouping alerts by exception type) without requiring a custom factory. The trade-off between observability and opacity is explicitly left to the developer via the `errorFactory` parameter.


### `ErrorType` and `Severity` Implementation Detail Exposure

**Severity: Low**

When `ErrorDetailDto` is serialized to a `ProblemDetails` response (typically via `ResultEndpointFilter`), the `ErrorType` and `Severity` properties are included by default. Depending on how your application models errors, exposing these properties may reveal internal implementation details to external clients.

For example, returning an `ErrorType.Infrastructure` or `ErrorType.Domain` gives clients insight into the backend architecture and where the failure occurred. This is a deliberate tradeoff between usability (providing structured, typed errors to clients) and opacity. If your security requirements mandate complete opacity for internal failures, you should map internal errors to a generic `ErrorType.Unexpected` before returning them to the HTTP layer.
