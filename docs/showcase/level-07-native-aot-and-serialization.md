# Level 07 — Native AOT & Zero-Reflection Serialization

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** Performance Engineers & Cloud Architects | **Language:** English

---

## 1. The Native AOT Mandate

In cloud-native serverless (AWS Lambda, Azure Container Apps) and Kubernetes environments, Native Ahead-Of-Time (AOT) compilation provides:
- **Instant Cold Starts**: Execution starts in under ~10 milliseconds (no JIT warmup).
- **Minimal Working Set**: Memory consumption is reduced by up to 70%.
- **Zero Reflection & Trimming**: Unused code is stripped by IL trimmer (`PublishTrimmed=true`).

---

## 2. JSON Serialization with `System.Text.Json`

Install the serialization package:

```bash
dotnet add package EricksonLopez.Result.Serialization
```

### Configuring Native AOT JSON Serialization:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;

var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

// Register custom AOT-compatible Result converters
options.Converters.Add(new ResultJsonConverter());
options.Converters.Add(new ErrorJsonConverter());
options.Converters.Add(new ResultOfTJsonConverter<OrderDto>());

// Serialize a success result:
Result<OrderDto> orderResult = new OrderDto(Guid.NewGuid(), 150.00m);
string jsonSuccess = JsonSerializer.Serialize(orderResult, options);

// Serialize a failure result:
Result<OrderDto> errorResult = Error.NotFound("Order.NotFound", "Order does not exist.");
string jsonError = JsonSerializer.Serialize(errorResult, options);
```

---

## 3. Roslyn Source Generator (`EricksonLopez.Result.Serialization.Generators`)

To satisfy strict trimming (`EnableTrimAnalyzer=true`) and Native AOT without any runtime reflection overhead, define a `JsonSerializerContext`:

```csharp
[JsonSerializable(typeof(Result))]
[JsonSerializable(typeof(Result<OrderDto>))]
[JsonSerializable(typeof(Error))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
```

Then serialize using the generated type information:

```csharp
byte[] utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(
    orderResult, 
    AppJsonSerializerContext.Default.ResultOrderDto);
```

---

## 4. Verification via `AotSmokeTest`

The repository includes a dedicated Native AOT smoke test project (`tests/EricksonLopez.Result.AotSmokeTest`) that compiles with `PublishAot=true` in CI/CD, guaranteeing that:
1. No dynamic code generation or unsupported reflection warnings occur.
2. Value type envelopes and combinators execute deterministically across all Linux/Windows AOT runtimes.

---

## Next Steps
Proceed to [Level 08 — Observability & Fluent Testing](level-08-telemetry-and-testing.md) to explore OpenTelemetry distributed tracing, runtime metrics, and the fluent assertion framework.
