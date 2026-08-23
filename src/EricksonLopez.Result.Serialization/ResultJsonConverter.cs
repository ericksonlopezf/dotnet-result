// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Result;

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
                }
                else if (string.Equals(propertyName, "error", StringComparison.OrdinalIgnoreCase))
                {
                    if (reader.TokenType != JsonTokenType.Null)
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
