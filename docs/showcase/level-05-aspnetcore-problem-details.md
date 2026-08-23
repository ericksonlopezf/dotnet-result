# Level 05 — ASP.NET Core & RFC 9457 ProblemDetails

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** API Architects & Web Developers | **Language:** English

---

## 1. Package Installation

Install the ASP.NET Core integration package:

```bash
dotnet add package EricksonLopez.Result.AspNetCore
```

---

## 2. Minimal APIs Integration via `.ToHttpResult()`

Transform any domain `Result<T>` directly into an idiomatic ASP.NET Core `IResult`:

```csharp
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api/users/{id:guid}", async (Guid id, IUserService userService, CancellationToken ct) =>
{
    Result<UserDto> result = await userService.GetUserByIdAsync(id, ct);
    
    // Maps Success -> 200 OK with UserDto body
    // Maps Failure -> 404/400/409/500 ProblemDetails payload matching ErrorType!
    return result.ToHttpResult();
});

app.MapPost("/api/users", async (CreateUserCommand command, IUserService userService, CancellationToken ct) =>
{
    Result<UserDto> result = await userService.CreateUserAsync(command, ct);
    
    // Custom success status: 201 Created with Location header
    return result.Match(
        user => Results.Created($"/api/users/{user.Id}", user),
        error => error.ToHttpResult());
});

app.Run();
```

---

## 3. RFC 9457 Automatic Status Code Mapping

When an error occurs, `EricksonLopez.Result.AspNetCore` maps the `ErrorType` to the RFC 9457 compliant status code and JSON structure:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "The user with ID 'c6b245e1-8844-47f2-90cf-847285a86d22' does not exist.",
  "instance": "/api/users/c6b245e1-8844-47f2-90cf-847285a86d22",
  "errorCode": "User.NotFound",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
  "metadata": {
    "RequestedUserId": "c6b245e1-8844-47f2-90cf-847285a86d22"
  }
}
```

---

## 4. Transparent Endpoint Filters

Eliminate repetitive `.ToHttpResult()` calls across all endpoints using an `IEndpointFilter`:

```csharp
// Program.cs
var apiGroup = app.MapGroup("/api")
    .AddEndpointFilter<ResultEndpointFilter>();

// Endpoints can now return Result<T> directly!
apiGroup.MapGet("/orders/{id:guid}", async (Guid id, IOrderService service) =>
{
    return await service.GetOrderAsync(id); // Automatically unwrapped & mapped!
});
```

---

## Next Steps
Proceed to [Level 06 — Integrations: FluentValidation, MediatR & Analyzers](level-06-integrations.md) to explore ecosystem companion adapters and compile-time Roslyn diagnostic rules.
