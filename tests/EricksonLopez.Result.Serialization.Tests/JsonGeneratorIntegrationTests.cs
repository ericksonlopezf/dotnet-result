// Copyright © Erickson Lopez. MIT License.
#pragma warning disable CA1861
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Serialization.Tests;

[JsonSerializable(typeof(Result<string>))]
[JsonSerializable(typeof(Result<List<int>>))]
[JsonSerializable(typeof(Result<int[]>))]
public partial class IntegrationTestContext : JsonSerializerContext
{
}

[Trait("Category", "Integration")]
public class JsonGeneratorIntegrationTests
{
    [Fact]
    public void GeneratedContext_WhenGenericResults_SerializesAndDeserializes()
    {
        // This test ensures that the Source Generator correctly wires up the JsonConverter
        // and matches the STJ TypeInfoPropertyName correctly across TFMs (.NET 8/9/10).
        // If GetStjTypeInfoPropertyName fails to match the STJ property name, 
        // options.Converters.Add() won't register the correct context properties,
        // and serialization will either fallback to reflection (if enabled) or fail.

        var options = new JsonSerializerOptions();
        options.TypeInfoResolverChain.Insert(0, IntegrationTestContext.Default);
        options.AddResultConverters();

        System.Console.WriteLine("CONVERTERS:");
        foreach (var c in options.Converters)
        {
            System.Console.WriteLine(c.GetType().FullName);
        }

        options.Converters.Should().Contain(c => c.GetType().Name.Contains("ResultOfTJsonConverter"));

        var resultString = Result.Success("Test");
        var jsonString = JsonSerializer.Serialize(resultString, options);
        jsonString.Should().Be("{\"isSuccess\":true,\"isFailure\":false,\"value\":\"Test\"}");
        var deserializedString = JsonSerializer.Deserialize<Result<string>>(jsonString, options);
        deserializedString.IsSuccess.Should().BeTrue();
        deserializedString.Value.Should().Be("Test");

        var resultList = Result.Success(new List<int> { 1, 2, 3 });
        var jsonList = JsonSerializer.Serialize(resultList, options);
        jsonList.Should().Be("{\"isSuccess\":true,\"isFailure\":false,\"value\":[1,2,3]}");
        var deserializedList = JsonSerializer.Deserialize<Result<List<int>>>(jsonList, options);
        deserializedList.IsSuccess.Should().BeTrue();
        deserializedList.Value.Should().BeEquivalentTo(new[] { 1, 2, 3 });

        var resultArray = Result.Success(new[] { 1, 2, 3 });
        var jsonArray = JsonSerializer.Serialize(resultArray, options);
        jsonArray.Should().Be("{\"isSuccess\":true,\"isFailure\":false,\"value\":[1,2,3]}");
        var deserializedArray = JsonSerializer.Deserialize<Result<int[]>>(jsonArray, options);
        deserializedArray.IsSuccess.Should().BeTrue();
        deserializedArray.Value.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }
}


