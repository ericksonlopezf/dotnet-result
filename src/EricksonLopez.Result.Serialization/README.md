# EricksonLopez.Result.Serialization

System.Text.Json serialization support for `EricksonLopez.Result` types.

## Installation

```bash
dotnet add package EricksonLopez.Result.Serialization
```

## Quick Start

```csharp
var options = new JsonSerializerOptions();
options.Converters.Add(new ResultJsonConverterFactory());
```

## ⚠️ NativeAOT / Trimming Compatibility

> **IMPORTANT**: `ResultJsonConverterFactory` uses `Type.MakeGenericType` and `Activator.CreateInstance`
> internally, which are **NOT compatible with NativeAOT** (`PublishAot=true`) or aggressive trimming.

### For NativeAOT / Trimming Scenarios

Do **NOT** use `ResultJsonConverterFactory`. Instead, register converters explicitly for each
concrete `Result<T>` type you need to serialize:

```csharp
var options = new JsonSerializerOptions();

// Register the non-generic Result and Error converters (AOT-safe)
options.Converters.Add(new ResultJsonConverter());
options.Converters.Add(new ErrorJsonConverter());

// Register a typed converter for EACH Result<T> you use
options.Converters.Add(new ResultOfTJsonConverter<MyDto>());
options.Converters.Add(new ResultOfTJsonConverter<OrderResponse>());
```

Additionally, ensure each `T` type is registered in a `JsonSerializerContext` for source generation:

```csharp
[JsonSerializable(typeof(MyDto))]
[JsonSerializable(typeof(OrderResponse))]
public partial class AppJsonSerializerContext : JsonSerializerContext { }
```

### Provided AOT-Safe Converters

| Converter | Type | AOT-Safe |
|-----------|------|----------|
| `ResultJsonConverter` | `Result` (non-generic) | ✅ Yes |
| `ErrorJsonConverter` | `Error` | ✅ Yes |
| `ResultOfTJsonConverter<T>` | `Result<T>` (concrete T) | ✅ Yes |
| `ResultJsonConverterFactory` | `Result` + `Result<T>` (auto) | ❌ No |

### Metadata Round-Trip Behavior

> [!WARNING]
> Metadata values in `Error.Metadata` are **type-lossy after JSON round-trip**. This can cause
> silent `InvalidCastException` errors at runtime if you cast metadata values to their original CLR types.

**Numeric types change silently:**

| Written as | Deserialized as |
|------------|----------------|
| `int` (e.g. `42`) | `long` |
| `float` | `double` |
| `decimal` | `string` |
| `DateTime`, `Guid`, custom objects | `string` |
| `bool` | `bool` ✅ |

**Example of the bug:**

```csharp
// Service A: writes metadata
var error = Error.Failure("Order.Invalid", "Order count exceeds limit")
    .ToBuilder()
    .WithMetadata("count", 42)  // stored as int
    .Build();

// Serialized to JSON: { "count": 42 }

// Service B: reads metadata after JSON round-trip
var count = (int)error.Metadata["count"];  // ❌ InvalidCastException: cannot cast Int64 to Int32
//           ^^^^ int, but the actual type is long after JSON deserialization
```

**Safe patterns:**

```csharp
// Option 1: use Convert.ToInt32 instead of direct cast
var count = Convert.ToInt32(error.Metadata["count"]);  // ✅ safe for numeric round-trip

// Option 2: use pattern matching
if (error.Metadata["count"] is long l) { var count = (int)l; }

// Option 3 (recommended): use a typed DTO in your domain model instead of Error.Metadata
//           for data that must round-trip faithfully across service boundaries.
```

> [!NOTE]
> This limitation is inherent to JSON's type system (numbers have no int/long distinction).
> `Error.Metadata` is designed for low-cardinality diagnostic context (user IDs, request IDs),
> not as a general-purpose data carrier across service boundaries.
