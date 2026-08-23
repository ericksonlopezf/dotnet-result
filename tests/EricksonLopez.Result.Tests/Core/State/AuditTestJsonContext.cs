// Copyright © Erickson Lopez. MIT License.
using System.Text.Json.Serialization;

namespace EricksonLopez.Result.Tests.Core;

[JsonSerializable(typeof(AuditTestDto))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class AuditTestJsonContext : JsonSerializerContext { }
