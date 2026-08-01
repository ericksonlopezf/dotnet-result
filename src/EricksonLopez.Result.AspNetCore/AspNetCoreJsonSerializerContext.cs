using System.Text.Json.Serialization;

namespace EricksonLopez.Result.AspNetCore;

/// <summary>
/// Source generator context for JSON serialization.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[JsonSerializable(typeof(ErrorDetailDto))]
[JsonSerializable(typeof(List<ErrorDetailDto>))]
internal sealed partial class AspNetCoreJsonSerializerContext : JsonSerializerContext
{
}
