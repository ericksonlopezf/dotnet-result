// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;
using Xunit;

namespace EricksonLopez.Result.Tests.Audit;

public class PropertyAndFuzzTests
{
    private static readonly Random Rng = new(42); // Deterministic seed

    [Fact]
    public void Property_Map_On_Success_Always_Produces_Success()
    {
        for (int i = 0; i < 5000; i++)
        {
            int val = Rng.Next(-100000, 100000);
            int multiplier = Rng.Next(-100, 100);

            var res = Result.Success(val).Map(x => x * multiplier);

            Assert.True(res.IsSuccess);
            Assert.False(res.IsFailure);
            Assert.Equal(val * multiplier, res.Value);
        }
    }

    [Fact]
    public void Property_Map_On_Failure_Always_Preserves_Original_Error()
    {
        for (int i = 0; i < 5000; i++)
        {
            string code = "ERR_" + Rng.Next(1, 10000);
            string desc = "Desc_" + Guid.NewGuid();
            var err = Error.Failure(code, desc);

            bool mapperInvoked = false;
            var res = Result.Failure<int>(err).Map(x =>
            {
                mapperInvoked = true;
                return x * 2;
            });

            Assert.False(mapperInvoked, "Mapper was illegally invoked on failure!");
            Assert.True(res.IsFailure);
            Assert.False(res.IsSuccess);
            Assert.Same(err, res.Error);
        }
    }

    [Fact]
    public void Property_Match_Always_Executes_Exactly_One_Branch()
    {
        for (int i = 0; i < 5000; i++)
        {
            bool isSuccess = Rng.Next(2) == 0;
            var res = isSuccess ? Result.Success(Rng.Next()) : Result.Failure<int>(Error.Failure("CODE", "DESC"));

            int successBranches = 0;
            int failureBranches = 0;

            int output = res.Match(
                s => { successBranches++; return 1; },
                f => { failureBranches++; return 2; }
            );

            if (isSuccess)
            {
                Assert.Equal(1, successBranches);
                Assert.Equal(0, failureBranches);
                Assert.Equal(1, output);
            }
            else
            {
                Assert.Equal(0, successBranches);
                Assert.Equal(1, failureBranches);
                Assert.Equal(2, output);
            }
        }
    }

    [Fact]
    public void Fuzzing_Extreme_Unicode_And_Control_Chars_In_Error()
    {
        string[] maliciousStrings = new[]
        {
            "\0\0\0",
            "\uD83D\uDE00\uD83D\uDE01\uD83D\uDE02", // Emojis
            new string('A', 50000), // 50K chars
            "<script>alert('xss')</script>",
            "' OR '1'='1",
            "\r\n\t\b\f\v",
            "한국어, 日本語, Русский, العربية, עברית",
            "\uFEFF\u200B\u200C\u200D" // Zero-width spaces
        };

        foreach (var text in maliciousStrings)
        {
            var err = Error.Failure(text, text);
            Assert.Equal(text, err.Code);
            Assert.Equal(text, err.Description);

            var res = Result.Failure(err);
            Assert.True(res.IsFailure);
            Assert.Equal(text, res.Error.Code);
        }
    }

    [Fact]
    public void Fuzzing_Malformed_And_Adversarial_Json()
    {
        string[] malformedPayloads = new[]
        {
            "",
            "{",
            "{\"isSuccess\":",
            "{\"isSuccess\": null}",
            "{\"isSuccess\": \"true\"}",
            "{\"isSuccess\": false}", // failure without error object
            "{\"isSuccess\": false, \"error\": \"not-an-object\"}",
            "{\"isSuccess\": false, \"error\": { \"code\": null }}",
            "[]",
            "12345",
            "true",
            "null"
        };

        var options = new JsonSerializerOptions();
        options.Converters.Add(new ResultJsonConverter());

        foreach (var payload in malformedPayloads)
        {
            try
            {
                var res = JsonSerializer.Deserialize<Result>(payload, options);
                // If it deserializes without throwing, it must be in a consistent state (never corrupt)
                Assert.True(res.IsSuccess || res.IsFailure || res.IsUninitialized);
            }
            catch (JsonException)
            {
                // Expected and safe defense against malformed inputs
            }
        }
    }

    [Fact]
    public void Fuzzing_Deeply_Nested_Error_Hierarchy()
    {
        var current = Error.Failure("ROOT", "Root Error");
        for (int depth = 1; depth <= 25; depth++)
        {
            current = Error.Create($"LVL_{depth}", $"Depth {depth}")
                .WithInnerError(current)
                .Build();
        }

        Assert.Equal("LVL_25", current.Code);
        Assert.Single(current.InnerErrors);

        // Serialize and ensure no stack overflow
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ErrorJsonConverter());
        var json = JsonSerializer.Serialize(current, options);
        var restored = JsonSerializer.Deserialize<Error>(json, options);

        Assert.NotNull(restored);
        Assert.Equal("LVL_25", restored!.Code);
    }
}
