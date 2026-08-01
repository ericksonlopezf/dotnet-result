using System.Text.Json.Serialization;

namespace EricksonLopez.Result.Serialization;

/// <summary>
/// Source-generated JSON serialization context for Result types.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
// RESULT_GEN_001: [JsonSerializable(typeof(Result))] on this context is intentional.
// The non-generic Result is serialized as a standalone type (not just as the envelope for Result<T>)
// so that ResultJsonConverter can be source-generated into this context alongside ErrorJsonConverter.
// This is an internal library context; consumer-facing contexts should use [JsonSerializable(typeof(Result<T>))].
#pragma warning disable RESULT_GEN_001
[JsonSerializable(typeof(Result))]
#pragma warning restore RESULT_GEN_001
[JsonSerializable(typeof(Error))]
[JsonSerializable(typeof(ErrorType))]
[JsonSerializable(typeof(ErrorSeverity))]
[JsonSerializable(typeof(ErrorRetryability))]
[JsonSerializable(typeof(IReadOnlyList<Error>))]
[JsonSerializable(typeof(IReadOnlyDictionary<string, object>))]
public partial class ResultJsonSerializerContext : JsonSerializerContext
{
}

