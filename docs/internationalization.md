# Internationalization (i18n)

This guide describes how to localize error messages in `EricksonLopez.Result` using the `DescriptionKey` property and standard .NET localization primitives.

---

## 1. Design Philosophy

In modern distributed systems, domain errors should separate **semantic error identifiers** from **localized presentation messages**:

- **`Code`**: Stable machine-readable identifier (e.g., `"User.NotFound"`).
- **`Description`**: Default English developer-facing explanation.
- **`DescriptionKey`**: Resource key used to look up localized strings via `IStringLocalizer` or client-side translation catalogs.

```csharp
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User with ID '{id}' was not found.")
             .WithDescriptionKey("Errors_User_NotFound");
}
```

---

## 2. Resource File (`.resx`) Structure

Define error messages in standard `.resx` resource files within your application or localization project.

### `Errors.resx` (Default - English)
```xml
<data name="Errors_User_NotFound" xml:space="preserve">
  <value>The requested user was not found in the system.</value>
</data>
<data name="Errors_Payment_Failed" xml:space="preserve">
  <value>Payment processing failed. Please try a different payment method.</value>
</data>
```

### `Errors.es-ES.resx` (Spanish)
```xml
<data name="Errors_User_NotFound" xml:space="preserve">
  <value>El usuario especificado no fue encontrado en el sistema.</value>
</data>
<data name="Errors_Payment_Failed" xml:space="preserve">
  <value>El pago no pudo procesarse. Por favor intente con otro método.</value>
</data>
```

---

## 3. ASP.NET Core Integration with `IStringLocalizer`

You can translate `Error` instances automatically when mapping to HTTP responses or in API filter pipelines:

```csharp
public class LocalizedProblemDetailsFactory
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public LocalizedProblemDetailsFactory(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public string GetLocalizedDescription(Error error)
    {
        if (!string.IsNullOrEmpty(error.DescriptionKey))
        {
            var localized = _localizer[error.DescriptionKey];
            if (!localized.ResourceNotFound)
                return localized.Value;
        }

        return error.Description;
    }
}
```

---

## 4. Client-Side Localization with `DescriptionKey`

When returning errors in JSON responses via RFC 9457 `ProblemDetails`, `descriptionKey` can be included in the `extensions` dictionary:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "User with ID '123' was not found.",
  "errorCode": "User.NotFound",
  "descriptionKey": "Errors_User_NotFound"
}
```

Frontend single-page applications (React, Angular, Vue) using i18n libraries (e.g., `i18next`) can match `descriptionKey` directly against their client-side translation catalogs.
