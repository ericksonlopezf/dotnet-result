// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Serialization;

/// <summary>
/// AOT-compliant JsonConverter for <see cref="Error"/>.
/// Uses manual property matching and switch-based enum parsing to avoid reflection in NativeAOT scenarios.
/// </summary>
/// <remarks>
/// <para>
/// <b>Metadata round-trip is partially lossy.</b>
/// Values stored in <see cref="Error.Metadata"/> are serialized preserving native JSON types:
/// numbers (<c>int</c>, <c>long</c>, <c>double</c>, etc.) are written as JSON numbers,
/// booleans as JSON booleans, strings as JSON strings, and collections as JSON arrays.
/// On deserialization, numbers are recovered as <c>long</c> or <c>double</c>, booleans as <c>bool</c>,
/// and all other values (including <see cref="DateTime"/>, <see cref="Guid"/>, custom objects)
/// are deserialized as <c>string</c>. The original CLR numeric type (e.g., <c>int</c> vs <c>long</c>) may differ.
/// </para>
/// <para>
/// If you require type-faithful metadata round-tripping, store metadata as a typed DTO and use a separate
/// property on your domain object rather than relying on <see cref="Error.Metadata"/>.
/// </para>
/// </remarks>
public sealed class ErrorJsonConverter : JsonConverter<Error>
{
    /// <inheritdoc/>
    // Stryker disable all : Serialization boilerplate
    public override Error Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token.");
        }

        string? code = null;
        string? description = null;
        string? descriptionKey = null;
        string? traceId = null;
        string? correlationId = null;
        ErrorType type = ErrorType.Failure;
        ErrorSeverity severity = ErrorSeverity.Error;
        ErrorRetryability retryability = ErrorRetryability.NotApplicable;
        List<Error>? innerErrors = null;
        Dictionary<string, object>? metadata = null;

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

                if (string.Equals(propertyName, "code", StringComparison.OrdinalIgnoreCase))
                {
                    code = reader.GetString();
                }
                else if (string.Equals(propertyName, "description", StringComparison.OrdinalIgnoreCase))
                {
                    description = reader.GetString();
                }
                else if (string.Equals(propertyName, "descriptionKey", StringComparison.OrdinalIgnoreCase))
                {
                    descriptionKey = reader.GetString();
                }
                else if (string.Equals(propertyName, "type", StringComparison.OrdinalIgnoreCase))
                {
                    // Use switch-based parsing instead of Enum.TryParse for NativeAOT safety
                    type = ParseErrorType(reader.GetString());
                }
                else if (string.Equals(propertyName, "severity", StringComparison.OrdinalIgnoreCase))
                {
                    // Use switch-based parsing instead of Enum.TryParse for NativeAOT safety
                    severity = ParseErrorSeverity(reader.GetString());
                }
                else if (string.Equals(propertyName, "retryability", StringComparison.OrdinalIgnoreCase))
                {
                    // Use switch-based parsing instead of Enum.TryParse for NativeAOT safety
                    retryability = ParseErrorRetryability(reader.GetString());
                }
                else if (string.Equals(propertyName, "traceId", StringComparison.OrdinalIgnoreCase))
                {
                    traceId = reader.GetString();
                }
                else if (string.Equals(propertyName, "correlationId", StringComparison.OrdinalIgnoreCase))
                {
                    correlationId = reader.GetString();
                }
                else if (string.Equals(propertyName, "innerErrors", StringComparison.OrdinalIgnoreCase))
                {
                    if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        innerErrors = new List<Error>();
                        while (true)
                        {
                            reader.Read();
                            if (reader.TokenType == JsonTokenType.EndArray) break;
                            var innerError = Read(ref reader, typeof(Error), options);
                            if (innerError is not null)
                            {
                                innerErrors.Add(innerError);
                            }
                        }
                    }
                }
                else if (string.Equals(propertyName, "metadata", StringComparison.OrdinalIgnoreCase))
                {
                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        // Read metadata directly from the Utf8JsonReader without
                        // allocating an intermediate JsonDocument on the heap.
                        // Each property value is deserialized inline using the reader's
                        // current token type, matching the same semantics as the previous
                        // JsonDocument.ParseValue approach but with zero extra allocation.
                        metadata = ReadMetadataObject(ref reader);
                    }
                }
                else
                {
                    reader.Skip();
                }
            }
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(description))
        {
            throw new JsonException("Missing required fields 'code' or 'description'.");
        }

        // Use ErrorBuilder to construct the deserialized Error without calling the protected constructor.
        // WithTraceId() ensures the stored traceId string is used as-is (no Activity.Current capture).
        // WithType/Severity/Retryability use the parsed enum values (defaulting to Failure/Error/NotApplicable
        // if not present in the JSON).
        var builder = Error.Create(code!, description!)
            .WithType(type)
            .WithSeverity(severity)
            .WithRetryability(retryability)
            .WithTraceId(traceId)
            .WithCorrelationId(correlationId);

        if (descriptionKey is not null)
            builder = builder.WithDescriptionKey(descriptionKey);

        if (innerErrors is { Count: > 0 })
            builder = builder.WithInnerErrors(innerErrors);

        if (metadata is { Count: > 0 })
            builder = builder.WithMetadata(metadata);

        return builder.Build();
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Error value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("code", value.Code);
        writer.WriteString("description", value.Description);
        writer.WriteString("type", ErrorTypeToString(value.Type));
        writer.WriteString("severity", ErrorSeverityToString(value.Severity));
        writer.WriteString("retryability", ErrorRetryabilityToString(value.Retryability));
        if (value.DescriptionKey is not null) writer.WriteString("descriptionKey", value.DescriptionKey);
        if (value.TraceId is not null) writer.WriteString("traceId", value.TraceId);
        if (value.CorrelationId is not null) writer.WriteString("correlationId", value.CorrelationId);

        if (value.HasInnerErrors)
        {
            writer.WritePropertyName("innerErrors");
            writer.WriteStartArray();
            foreach (var inner in value.InnerErrors)
            {
                Write(writer, inner, options);
            }
            writer.WriteEndArray();
        }

        if (value.HasMetadata)
        {
            writer.WritePropertyName("metadata");
            writer.WriteStartObject();
            foreach (var kvp in value.Metadata)
            {
                writer.WritePropertyName(kvp.Key);
                WriteMetadataValue(writer, kvp.Value);
            }
            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }
    // ─── AOT-safe metadata value reading (mirrors WriteMetadataValue) ────────────

    /// <summary>
    /// Recursively deserializes a JSON element into its corresponding CLR type.
    /// Mirrors <see cref="WriteMetadataValue"/> to ensure full round-trip fidelity.
    /// Reads a JSON object directly from the <see cref="Utf8JsonReader"/> as a metadata dictionary.
    /// The reader must be positioned at <see cref="JsonTokenType.StartObject"/>.
    /// </summary>
    /// <remarks>
    /// This replaces the previous <c>JsonDocument.ParseValue(ref reader)</c> approach,
    /// avoiding the intermediate <c>JsonDocument</c> heap allocation. Values are deserialized
    /// using the same type-mapping rules as the previous <c>DeserializeMetadataValue</c> method.
    /// </remarks>
    private static Dictionary<string, object> ReadMetadataObject(ref Utf8JsonReader reader)
    {
        // reader is at StartObject
        var dict = new Dictionary<string, object>(StringComparer.Ordinal);
        // Stryker disable once Logical : Stream reader loop termination
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var key = reader.GetString()!;
                reader.Read(); // advance to value token

                var value = ReadMetadataValue(ref reader);
                if (value is not null)
                    dict[key] = value;
            }
        }
        return dict;
    }

    /// <summary>
    /// Reads a single metadata value from the current position of the <see cref="Utf8JsonReader"/>.
    /// Supports string, number (long/double), boolean, null, array, and nested object values.
    /// </summary>
    private static object? ReadMetadataValue(ref Utf8JsonReader reader) => reader.TokenType switch
    {
        JsonTokenType.String => reader.GetString(),
        JsonTokenType.Number => reader.TryGetInt64(out var l) ? (object)l : reader.GetDouble(),
        JsonTokenType.True => true,
        JsonTokenType.False => false,
        JsonTokenType.Null => null,
        JsonTokenType.StartArray => ReadMetadataArray(ref reader),
        JsonTokenType.StartObject => ReadMetadataNestedObject(ref reader),
        _ => null // Skip unknown token types
    };

    private static List<object?> ReadMetadataArray(ref Utf8JsonReader reader)
    {
        // reader is at StartArray
        var list = new List<object?>();
        // Stryker disable once Logical : Stream reader loop termination
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            list.Add(ReadMetadataValue(ref reader));
        }
        return list;
    }

    private static Dictionary<string, object?> ReadMetadataNestedObject(ref Utf8JsonReader reader)
    {
        // reader is at StartObject
        var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
        // Stryker disable once Logical : Stream reader loop termination
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;

            var key = reader.GetString()!;
            reader.Read(); // advance to value token
            dict[key] = ReadMetadataValue(ref reader);
        }
        return dict;
    }

    // ─── AOT-safe metadata value writing (type-switch, no reflection) ────────────

    /// <summary>
    /// Writes a metadata value preserving its native JSON type when possible.
    /// Primitives (numbers, booleans, strings) are written as native JSON values.
    /// Collections implementing <see cref="System.Collections.IEnumerable"/> are written as JSON arrays.
    /// Unknown types fall back to <see cref="object.ToString"/>.
    /// </summary>
    /// <remarks>
    /// This method is AOT-safe: it uses a type-switch on known CLR types and does not
    /// use <c>JsonSerializer.Serialize</c> or reflection-based serialization.
    /// <para>
    /// <b>Round-trip note:</b> On deserialization, numbers are recovered as <c>long</c> or <c>double</c>,
    /// booleans as <c>bool</c>, and strings as <c>string</c>. The original CLR numeric type
    /// (e.g., <c>int</c> vs <c>long</c>) may differ after round-tripping.
    /// </para>
    /// </remarks>
    private static void WriteMetadataValue(Utf8JsonWriter writer, object? value)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case string s:
                writer.WriteStringValue(s);
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            case int i:
                writer.WriteNumberValue(i);
                break;
            case long l:
                writer.WriteNumberValue(l);
                break;
            case double d:
                writer.WriteNumberValue(d);
                break;
            case float f:
                writer.WriteNumberValue(f);
                break;
            case decimal m:
                writer.WriteNumberValue(m);
                break;
            case short sh:
                writer.WriteNumberValue(sh);
                break;
            case byte by:
                writer.WriteNumberValue(by);
                break;
            case uint ui:
                writer.WriteNumberValue(ui);
                break;
            case ulong ul:
                writer.WriteNumberValue(ul);
                break;
            case ushort us:
                writer.WriteNumberValue(us);
                break;
            case sbyte sb:
                writer.WriteNumberValue(sb);
                break;
            // Well-known types that have standard JSON string representations
            case DateTime dt:
                writer.WriteStringValue(dt.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
                break;
            case DateTimeOffset dto:
                writer.WriteStringValue(dto.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
                break;
            case Guid g:
                writer.WriteStringValue(g.ToString());
                break;
            case TimeSpan ts:
                // Stryker disable once String : In .NET TimeSpan.ToString("") defaults to "c" invariant format
                writer.WriteStringValue(ts.ToString("c", System.Globalization.CultureInfo.InvariantCulture));
                break;
            // Collections: write as JSON array with recursive element handling
            case System.Collections.IEnumerable enumerable:
                writer.WriteStartArray();
                foreach (var item in enumerable)
                {
                    WriteMetadataValue(writer, item);
                }
                writer.WriteEndArray();
                break;
            // Fallback: IFormattable types get invariant formatting, others get ToString()
            case IFormattable formattable:
                writer.WriteStringValue(formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture));
                break;
            default:
                writer.WriteStringValue(value.ToString() ?? string.Empty);
                break;
        }
    }

    // ─── AOT-safe enum parsing (switch-based, no Enum.TryParse reflection) ─────

    private static ErrorType ParseErrorType(string? value)
    {
        if (string.Equals(value, "Validation", StringComparison.OrdinalIgnoreCase)) return ErrorType.Validation;
        if (string.Equals(value, "NotFound", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "not_found", StringComparison.OrdinalIgnoreCase)) return ErrorType.NotFound;
        if (string.Equals(value, "Conflict", StringComparison.OrdinalIgnoreCase)) return ErrorType.Conflict;
        if (string.Equals(value, "Unauthorized", StringComparison.OrdinalIgnoreCase)) return ErrorType.Unauthorized;
        if (string.Equals(value, "Forbidden", StringComparison.OrdinalIgnoreCase)) return ErrorType.Forbidden;
        if (string.Equals(value, "Unavailable", StringComparison.OrdinalIgnoreCase)) return ErrorType.Unavailable;
        if (string.Equals(value, "Unexpected", StringComparison.OrdinalIgnoreCase)) return ErrorType.Unexpected;
        if (string.Equals(value, "Domain", StringComparison.OrdinalIgnoreCase)) return ErrorType.Domain;
        if (string.Equals(value, "Infrastructure", StringComparison.OrdinalIgnoreCase)) return ErrorType.Infrastructure;
        if (string.Equals(value, "Custom", StringComparison.OrdinalIgnoreCase)) return ErrorType.Custom;
        return ErrorType.Failure;
    }

    private static ErrorSeverity ParseErrorSeverity(string? value)
    {
        if (string.Equals(value, "Info", StringComparison.OrdinalIgnoreCase)) return ErrorSeverity.Info;
        if (string.Equals(value, "Warning", StringComparison.OrdinalIgnoreCase)) return ErrorSeverity.Warning;
        if (string.Equals(value, "Critical", StringComparison.OrdinalIgnoreCase)) return ErrorSeverity.Critical;
        return ErrorSeverity.Error;
    }

    private static ErrorRetryability ParseErrorRetryability(string? value)
    {
        if (string.Equals(value, "Transient", StringComparison.OrdinalIgnoreCase)) return ErrorRetryability.Transient;
        if (string.Equals(value, "Permanent", StringComparison.OrdinalIgnoreCase)) return ErrorRetryability.Permanent;
        return ErrorRetryability.NotApplicable;
    }

    // ─── AOT-safe enum serialization — delegates to shared ErrorEnumStrings ─────

    private static string ErrorTypeToString(ErrorType type)
        => ErrorEnumStrings.ErrorTypeToString(type);

    private static string ErrorSeverityToString(ErrorSeverity severity)
        => ErrorEnumStrings.ErrorSeverityToString(severity);

    private static string ErrorRetryabilityToString(ErrorRetryability retryability)
        => ErrorEnumStrings.ErrorRetryabilityToString(retryability);
} // Stryker restore all





