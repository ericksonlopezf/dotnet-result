// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Serialization;

/// <summary>
/// Represents a <see cref="JsonConverter{T}"/> for non-generic <see cref="Result"/> instances.
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
        bool? isFailure = null;
        bool isUninitialized = false;
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

                if (string.Equals(propertyName, "isUninitialized", StringComparison.OrdinalIgnoreCase))
                {
                    isUninitialized = reader.GetBoolean();
                }
                else if (string.Equals(propertyName, "isSuccess", StringComparison.OrdinalIgnoreCase))
                {
                    isSuccess = reader.GetBoolean();
                }
                else if (string.Equals(propertyName, "isFailure", StringComparison.OrdinalIgnoreCase))
                {
                    isFailure = reader.GetBoolean();
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

        if (isUninitialized || (isSuccess == false && isFailure == false && error is null))
        {
            return default;
        }

        if (isSuccess is null)
        {
            if (isFailure.HasValue)
            {
                isSuccess = !isFailure.Value;
            }
            else
            {
                throw new JsonException("Missing required property 'isSuccess' in Result JSON.");
            }
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
        if (value.IsUninitialized)
        {
            writer.WriteBoolean("isUninitialized", true);
            writer.WriteBoolean("isSuccess", false);
            writer.WriteBoolean("isFailure", false);
            writer.WriteEndObject();
            return;
        }

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
