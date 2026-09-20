// Copyright © Erickson Lopez. MIT License.
using System.Collections.Generic;
using System.Text.Json.Serialization;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Serialization.Tests;

[JsonSerializable(typeof(Result<string>))]
[JsonSerializable(typeof(Result<List<int>>))]
[JsonSerializable(typeof(Result<int[]>))]
public partial class IntegrationTestContext : JsonSerializerContext
{
}
