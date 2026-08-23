// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using EricksonLopez.Result;

namespace EricksonLopez.Result.AspNetCore;

/// <summary>
/// Source generator context for JSON serialization.
/// </summary>
[JsonSerializable(typeof(ErrorDetailDto))]
[JsonSerializable(typeof(List<ErrorDetailDto>))]
[JsonSerializable(typeof(string))]
[ExcludeFromCodeCoverage(Justification = "Source-generated code by System.Text.Json")]
internal sealed partial class AspNetCoreJsonSerializerContext : JsonSerializerContext
{
}
