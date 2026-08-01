using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace EricksonLopez.Result.Serialization;

/// <summary>
/// JsonConverter for non-generic <see cref="Result"/>.
/// </summary>
/// <remarks>
/// Uses manual <see cref="Utf8JsonReader"/> traversal with case-insensitive property matching,
/// consistent with <see cref="ErrorJsonConverter"/>. This avoids heap-allocating a <see cref="JsonDocument"/>
/// and ensures interoperability with JSON produced by systems using PascalCase or camelCase naming.
/// The <c>isFailure</c> property is accepted during deserialization for backward compatibility but is not required.
/// </remarks>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class ResultJsonConverter : JsonConverter<Result>
{
    private static readonly ErrorJsonConverter DefaultErrorConverter = new();

    /// <inheritdoc/>
    public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token for Result.");
        }

        bool? isSuccess = null;
        Error? error = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                if (string.Equals(propertyName, "isSuccess", StringComparison.OrdinalIgnoreCase))
                {
                    isSuccess = reader.GetBoolean();
                }
                else if (string.Equals(propertyName, "isFailure", StringComparison.OrdinalIgnoreCase))
                {
                    // Accepted for backward compatibility; isSuccess takes precedence if both are present.
                    if (isSuccess is null)
                    {
                        isSuccess = !reader.GetBoolean();
                    }
                    else
                    {
                        reader.GetBoolean(); // consume the value
                    }
                }
                else if (string.Equals(propertyName, "error", StringComparison.OrdinalIgnoreCase))
                {
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        error = null;
                    }
                    else
                    {
                        error = DefaultErrorConverter.Read(ref reader, typeof(Error), options);
                    }
                }
                else
                {
                    reader.Skip();
                }
            }
        }

        if (isSuccess is null)
        {
            throw new JsonException("Missing required property 'isSuccess' in Result JSON.");
        }

        if (isSuccess.Value)
        {
            return Result.Success();
        }

        return Result.Failure(error ?? Error.Failure("Serialization.Error", "Invalid Result JSON structure: failure without error."));
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("isSuccess", value.IsSuccess);
        writer.WriteBoolean("isFailure", value.IsFailure);
        if (value.IsFailure)
        {
            writer.WritePropertyName("error");
            DefaultErrorConverter.Write(writer, value.Error, options);
        }
        writer.WriteEndObject();
    }
}

/// <summary>
/// JsonConverter for generic <see cref="Result{T}"/>.
/// </summary>
/// <typeparam name="T">The result value type.</typeparam>
/// <remarks>
/// <para>
/// Uses manual <see cref="Utf8JsonReader"/> traversal with case-insensitive property matching,
/// consistent with <see cref="ErrorJsonConverter"/>. The <c>isFailure</c> property is accepted
/// during deserialization for backward compatibility but is not required.
/// </para>
/// <para>
/// <b>NativeAOT / Trimming — two construction modes:</b>
/// <list type="number">
/// <item>
///   <b>Reflection-based (default):</b> <c>new ResultOfTJsonConverter&lt;MyDto&gt;()</c> — uses
///   <see cref="JsonSerializer.Deserialize{TValue}(ref Utf8JsonReader, JsonSerializerOptions)"/>.
///   Requires <c>T</c> to be preserved by trimming and registered in a <see cref="System.Text.Json.Serialization.JsonSerializerContext"/>
///   for NativeAOT. Suppresses <c>IL2026</c>/<c>IL3050</c> warnings; callers are responsible.
/// </item>
/// <item>
///   <b>AOT-safe:</b> <c>new ResultOfTJsonConverter&lt;MyDto&gt;(MyContext.Default.MyDto)</c> — uses
///   <see cref="JsonSerializer.Deserialize{TValue}(ref Utf8JsonReader, JsonTypeInfo{TValue})"/>.
///   No reflection, fully compatible with NativeAOT and aggressive trimming. Recommended for
///   all NativeAOT applications.
/// </item>
/// </list>
/// </para>
/// </remarks>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class ResultOfTJsonConverter<T> : JsonConverter<Result<T>>
{
    private static readonly ErrorJsonConverter DefaultErrorConverter = new();
    private readonly JsonTypeInfo<T>? _typeInfo;

    /// <summary>
    /// Initializes the converter using <see cref="JsonSerializerOptions"/> for value serialization
    /// (reflection-based path). Requires <typeparamref name="T"/> to be preserved by trimming.
    /// </summary>
    /// <remarks>
    /// For NativeAOT applications, prefer
    /// <see cref="ResultOfTJsonConverter{T}(System.Text.Json.Serialization.Metadata.JsonTypeInfo{T})"/>
    /// which uses source-generated metadata and requires no reflection.
    /// </remarks>
    // ─── Design rationale: Obsolete(error: false) vs Obsolete(error: true) ─────────────────────
    // This constructor is intentionally marked [Obsolete(error: false)] rather than [Obsolete(error: true)].
    //
    // [Obsolete(error: true)] would generate CS0619 — a compilation error, not a warning.
    // CS0619 is a static compilation gate with no awareness of the deployment context:
    //   • It cannot be suppressed by #pragma warning disable in call sites.
    //   • It blocks legitimate reflection-based test paths that are safe in non-trimmed runtimes.
    //   • It provides no additional safety at publish time (where trimming actually occurs).
    //
    // [RequiresUnreferencedCode] + [RequiresDynamicCode] operate at a fundamentally different level:
    //   • They generate IL2026 / IL3050 diagnostics propagated by the ILC linker and trimmer.
    //   • Risk propagates transitively up the call graph — a caller of this constructor in a
    //     non-annotated method will itself be flagged, pushing the warning to the correct boundary.
    //   • They surface at dotnet publish time under /p:PublishAot=true or TrimmerRootAssembly,
    //     which is the actual deployment gate where the risk materializes.
    //   • They integrate with the PublishReadyToRun / SuppressTrimAnalysisWarnings pipeline.
    //
    // Architectural conclusion: [Obsolete(error: false)] + [RequiresUnreferencedCode/DynamicCode]
    // provides semantically precise risk communication aligned with the NativeAOT/trimming toolchain,
    // while preserving internal testability of the reflection path. Converting to error: true would
    // introduce artificial compilation rigidity without improving safety in trimmed deployments.
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    [Obsolete(
        "This constructor uses reflection-based JSON serialization which requires T and all reachable types to be " +
        "preserved by the trimmer. In NativeAOT or trimmed applications, use the overload " +
        "ResultOfTJsonConverter<T>(JsonTypeInfo<T>) instead. " +
        "See the package documentation for AOT-safe registration via source generators.",
        error: false)]
    [RequiresUnreferencedCode(
        "ResultOfTJsonConverter<T>() uses reflection-based JSON serialization. " +
        "T and all its reachable types must be preserved by the trimmer. " +
        "For NativeAOT or trimmed apps, use ResultOfTJsonConverter<T>(JsonTypeInfo<T>) instead.")]
    [RequiresDynamicCode(
        "ResultOfTJsonConverter<T>() uses reflection-based JSON serialization which may require " +
        "runtime code generation. For NativeAOT, use ResultOfTJsonConverter<T>(JsonTypeInfo<T>) instead.")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public ResultOfTJsonConverter() { }


    /// <summary>
    /// Initializes the converter using a <see cref="System.Text.Json.Serialization.Metadata.JsonTypeInfo{T}"/>
    /// for fully AOT-safe and trim-safe value serialization.
    /// </summary>
    /// <param name="typeInfo">
    /// The compile-time generated type info for <typeparamref name="T"/>, typically obtained from
    /// a <see cref="System.Text.Json.Serialization.JsonSerializerContext"/> (e.g.,
    /// <c>MyAppJsonContext.Default.MyDto</c>).
    /// This overload requires no reflection and is safe for NativeAOT and aggressive trimming.
    /// </param>
    /// <example>
    /// <code>
    /// // Preferred AOT-safe registration:
    /// options.Converters.Add(new ResultOfTJsonConverter&lt;MyDto&gt;(MyAppJsonContext.Default.MyDto));
    /// </code>
    /// </example>
    public ResultOfTJsonConverter(JsonTypeInfo<T> typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        _typeInfo = typeInfo;
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Reflection path: T value deserialization requires the caller to ensure T is preserved. " +
                        "Use the ResultOfTJsonConverter<T>(JsonTypeInfo<T>) constructor for AOT-safe serialization.")]
    [UnconditionalSuppressMessage("AOT", "IL3050",
        Justification = "Reflection path: T value deserialization requires the caller to register T in their JsonSerializerContext for NativeAOT. " +
                        "Use the ResultOfTJsonConverter<T>(JsonTypeInfo<T>) constructor for AOT-safe serialization.")]
    public override Result<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token for Result<T>.");
        }

        bool? isSuccess = null;
        T? value = default;
        bool hasValue = false;
        Error? error = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                if (string.Equals(propertyName, "isSuccess", StringComparison.OrdinalIgnoreCase))
                {
                    isSuccess = reader.GetBoolean();
                }
                else if (string.Equals(propertyName, "isFailure", StringComparison.OrdinalIgnoreCase))
                {
                    // Accepted for backward compatibility; isSuccess takes precedence if both are present.
                    if (isSuccess is null)
                    {
                        isSuccess = !reader.GetBoolean();
                    }
                    else
                    {
                        reader.GetBoolean(); // consume the value
                    }
                }
                else if (string.Equals(propertyName, "value", StringComparison.OrdinalIgnoreCase))
                {
                    // Use AOT-safe path when JsonTypeInfo<T> was provided; fall back to options-based
                    // reflection path when constructed via the parameterless constructor.
                    value = _typeInfo is not null
                        ? JsonSerializer.Deserialize<T>(ref reader, _typeInfo)
                        : JsonSerializer.Deserialize<T>(ref reader, options);
                    hasValue = true;
                }
                else if (string.Equals(propertyName, "error", StringComparison.OrdinalIgnoreCase))
                {
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        error = null;
                    }
                    else
                    {
                        error = DefaultErrorConverter.Read(ref reader, typeof(Error), options);
                    }
                }
                else
                {
                    reader.Skip();
                }
            }
        }

        if (isSuccess is null)
        {
            throw new JsonException("Missing required property 'isSuccess' in Result<T> JSON.");
        }

        if (isSuccess.Value)
        {
            if (!hasValue)
            {
                throw new JsonException("Missing required property 'value' in successful Result<T> JSON.");
            }
            return Result.Success(value!);
        }

        return Result.Failure<T>(error ?? Error.Failure("Serialization.Error", "Invalid Result<T> JSON structure: failure without error."));
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Reflection path: T value serialization requires the caller to ensure T is preserved or registered in their JsonSerializerContext. " +
                        "Use the ResultOfTJsonConverter<T>(JsonTypeInfo<T>) constructor for AOT-safe serialization.")]
    [UnconditionalSuppressMessage("AOT", "IL3050",
        Justification = "Reflection path: T value serialization requires the caller to register T in their JsonSerializerContext for NativeAOT. " +
                        "Use the ResultOfTJsonConverter<T>(JsonTypeInfo<T>) constructor for AOT-safe serialization.")]
    public override void Write(Utf8JsonWriter writer, Result<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("isSuccess", value.IsSuccess);
        writer.WriteBoolean("isFailure", value.IsFailure);
        if (value.IsSuccess)
        {
            writer.WritePropertyName("value");
            // Use AOT-safe path when JsonTypeInfo<T> was provided; fall back to options-based reflection path.
            if (_typeInfo is not null)
                JsonSerializer.Serialize(writer, value.Value, _typeInfo);
            else
                JsonSerializer.Serialize(writer, value.Value, options);
        }
        else if (value.IsFailure)
        {
            writer.WritePropertyName("error");
            DefaultErrorConverter.Write(writer, value.Error, options);
        }
        writer.WriteEndObject();
    }
} // Stryker restore all



