# Serialization & Native AOT Guide

This document describes JSON serialization with `System.Text.Json`, Native Ahead-of-Time (AOT) compilation, trimming compliance, and source generators in `EricksonLopez.Result`.

---

## 1. System.Text.Json Converters Overview

The `EricksonLopez.Result.Serialization` package provides high-performance JSON converters:
- **`ResultJsonConverter`**: Handles non-generic `Result` serialization (`{ "isSuccess": true }` or `{ "isSuccess": false, "error": { ... } }`).
- **`ResultOfTJsonConverter<T>`**: Handles generic `Result<T>` serialization for a specific type `T`.
- **`ErrorJsonConverter`**: Handles multi-dimensional `Error` serialization and RFC 9457 compatibility.

---

## 2. Native AOT & Source Generation

Under Native AOT (`PublishAot=true`), dynamic reflection (such as `MakeGenericType`) is not supported by the .NET runtime. 

To achieve full trimming and AOT compatibility, the `EricksonLopez.Result.Serialization.Generators` Roslyn Source Generator generates compile-time converter registrations when decorating your `JsonSerializerContext`:

```csharp
using System.Text.Json.Serialization;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;

[JsonSerializable(typeof(Result<OrderDto>))]
[JsonSerializable(typeof(Error))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
```

Then register the source-generated converters with `JsonSerializerOptions`:

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

// Generated AOT-safe extension method (recommended):
options.AddResultConverters();

// Or explicit AOT converter registration with JsonTypeInfo<T>:
options.Converters.Add(new ResultJsonConverter());
options.Converters.Add(new ErrorJsonConverter());
options.Converters.Add(new ResultOfTJsonConverter<OrderDto>(AppJsonSerializerContext.Default.OrderDto));
```

---

## 3. Diagnostic Rules

### `RESULT_GEN_001` — `[JsonSerializable(typeof(Result))]` Has No Effect for Converter Generation

#### Cause
A `[JsonSerializable(typeof(Result))]` attribute is placed on a `JsonSerializerContext` to generate generic Result converters.

#### Severity
⚠️ **`Warning`**

#### Rationale
Non-generic `Result` is handled out-of-the-box by `ResultJsonConverter` and does not require type-parameterized source generation. The source generator specifically inspects generic types `Result<T>` to generate typed `ResultOfTJsonConverter<T>` bindings.

#### How to Fix
Replace the non-generic `Result` attribute with the specific typed `Result<T>` payload, or remove the attribute if only non-generic `Result` is needed:

```csharp
// ❌ WRONG (Triggers RESULT_GEN_001):
[JsonSerializable(typeof(Result))]
public partial class AppJsonSerializerContext : JsonSerializerContext { }

// ✅ CORRECT:
[JsonSerializable(typeof(Result<OrderDto>))]
public partial class AppJsonSerializerContext : JsonSerializerContext { }
```
