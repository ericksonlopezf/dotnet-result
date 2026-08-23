// Copyright © Erickson Lopez. MIT License.
#pragma warning disable CS8602
using System;
using System.Collections.Generic;
using System.Threading;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace EricksonLopez.Result.AspNetCore.Tests;

public class ResultHttpExtensionsTests
{
    [Fact]
    public void ToProblemDetails_Success_ThrowsInvalidOperationException()
    {
        var result = Result.Success();
        Action act = () => result.ToProblemDetails();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToProblemDetailsOfT_Success_ThrowsInvalidOperationException()
    {
        var result = Result.Success(10);
        Action act = () => result.ToProblemDetails();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToProblemDetails_Failure_ReturnsProblemHttpResult()
    {
        var result = Result.Failure(Error.Validation("V", "Desc"));
        var httpResult = result.ToProblemDetails();
        httpResult.Should().BeOfType<ProblemHttpResult>();

        var problem = (ProblemHttpResult)httpResult;
        problem.StatusCode.Should().Be(400);
        problem.ProblemDetails.Title.Should().Be("Bad Request");
        problem.ProblemDetails.Extensions["errorCode"].Should().Be("V");
    }

    [Fact]
    public void ToProblemDetailsOfT_Failure_ReturnsProblemHttpResult()
    {
        var result = Result.Failure<int>(Error.NotFound("N", "Desc"));
        var httpResult = result.ToProblemDetails();
        httpResult.Should().BeOfType<ProblemHttpResult>();

        var problem = (ProblemHttpResult)httpResult;
        problem.StatusCode.Should().Be(404);
        problem.ProblemDetails.Title.Should().Be("Not Found");
        problem.ProblemDetails.Extensions["errorCode"].Should().Be("N");
    }

    [Fact]
    public void ToHttpResult_Success_ReturnsNoContent()
    {
        var result = Result.Success();
        var httpResult = result.ToHttpResult();
        httpResult.Should().BeOfType<NoContent>();

        var opt = new ResultHttpOptions { DefaultSuccessStatusCode = 200 };
        var okResult = result.ToHttpResult(opt);
        okResult.Should().BeOfType<Ok>();

        opt = new ResultHttpOptions { DefaultSuccessStatusCode = 201 };
        var createdResult = result.ToHttpResult(opt);
        createdResult.Should().BeOfType<Created>();
        ((Created)createdResult).StatusCode.Should().Be(201);

        opt = new ResultHttpOptions { DefaultSuccessStatusCode = 202 };
        var accResult = result.ToHttpResult(opt);
        accResult.Should().BeOfType<Accepted>();
        ((Accepted)accResult).StatusCode.Should().Be(202);

        opt = new ResultHttpOptions { DefaultSuccessStatusCode = 205 };
        var statResult = result.ToHttpResult(opt);
        statResult.Should().BeOfType<StatusCodeHttpResult>();
        ((StatusCodeHttpResult)statResult).StatusCode.Should().Be(205);
    }

    [Fact]
    public void ToHttpResultT_Success_ReturnsOkWithValue()
    {
        var result = Result.Success(42);
        var httpResult = result.ToHttpResult();
        httpResult.Should().BeOfType<Ok<int>>();
        ((Ok<int>)httpResult).Value.Should().Be(42);
    }

    [Fact]
    public void ToHttpResultT_Failure_ReturnsProblemDetails()
    {
        var result = Result.Failure<int>(Error.Conflict("C", "Desc"));
        var httpResult = result.ToHttpResult();
        httpResult.Should().BeOfType<ProblemHttpResult>();
    }

    [Fact]
    public void ToProblemDetails_Success_Throws()
    {
        Action act1 = () => Result.Success().ToProblemDetails();
        act1.Should().Throw<InvalidOperationException>();

        Action act2 = () => Result.Success(42).ToProblemDetails();
        act2.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToHttpResult_Failure_ReturnsProblemDetails()
    {
        var result = Result.Failure(Error.Conflict("C", "Desc"));
        var httpResult = result.ToHttpResult();
        httpResult.Should().BeOfType<ProblemHttpResult>();
        var problem = (ProblemHttpResult)httpResult;
        problem.StatusCode.Should().Be(409);
    }

    [Fact]
    public void ToHttpResultOfT_Success_ReturnsOkWithValue()
    {
        var result = Result.Success("value");
        var httpResult = result.ToHttpResult();
        httpResult.Should().BeOfType<Ok<string>>();
        var ok = (Ok<string>)httpResult;
        ok.Value.Should().Be("value");
    }

    [Fact]
    public void ToHttpResultOfT_Failure_ReturnsProblemDetails()
    {
        var result = Result.Failure<int>(Error.Conflict("C", "Desc"));
        var httpResult = result.ToHttpResult();
        httpResult.Should().BeOfType<ProblemHttpResult>();
        var problem = (ProblemHttpResult)httpResult;
        problem.StatusCode.Should().Be(409);
    }

    [Fact]
    public void CreateProblemDetails_WithInnerErrors_MapsToExtensions()
    {
        var result = Result.Failure(Error.Validation("code", "msg").ToBuilder().WithInnerError(Error.Failure("i", "i")).Build());

        var problem = (ProblemHttpResult)result.ToProblemDetails();

        problem.ProblemDetails.Extensions.Should().ContainKey("errors");
        var errors = (IEnumerable<ErrorDetailDto>)problem.ProblemDetails.Extensions["errors"]!;

    }

    [Fact]
    public void CreateProblemDetails_WithMetadata_MapsToExtensions()
    {
        var result = Result.Failure(Error.Validation("V", "Desc")
            .WithMetadata("errorCode", "override")
            .WithMetadata("custom", "value")
            .WithMetadata("dict", new Dictionary<string, object> { { "k", "v" } })
            .WithMetadata("list", new List<int> { 1, 2 })
            .WithMetadata("formattable", new Version(1, 0)));

        var problem = (ProblemHttpResult)result.ToProblemDetails();

        problem.ProblemDetails.Extensions.Should().ContainKey("meta.errorCode");
        problem.ProblemDetails.Extensions["meta.errorCode"].Should().Be("override");

        problem.ProblemDetails.Extensions.Should().ContainKey("custom");
        problem.ProblemDetails.Extensions["custom"].Should().Be("value");

        problem.ProblemDetails.Extensions.Should().ContainKey("dict");
        var dict = (Dictionary<string, object?>)problem.ProblemDetails.Extensions["dict"]!;
        dict["k"].Should().Be("v");

        problem.ProblemDetails.Extensions.Should().ContainKey("list");
        var list = (List<object?>)problem.ProblemDetails.Extensions["list"]!;
        list[0].Should().Be(1);
        list[1].Should().Be(2);

        problem.ProblemDetails.Extensions.Should().ContainKey("formattable");
        problem.ProblemDetails.Extensions["formattable"].Should().Be("1.0");
    }

    [Fact]
    public void CreateProblemDetails_UnknownErrorType_FallsBackTo500()
    {
        var result = Result.Failure(Error.Custom("x", "x", unchecked((ErrorType)255)));
        var problem = (ProblemHttpResult)result.ToProblemDetails();
        problem.StatusCode.Should().Be(500);
        problem.ProblemDetails.Type.Should().Be("about:blank");
    }

    [Theory]
    [InlineData((int)ErrorType.Failure, "Internal Server Error")]
    [InlineData((int)ErrorType.Validation, "Bad Request")]
    [InlineData((int)ErrorType.NotFound, "Not Found")]
    [InlineData((int)ErrorType.Conflict, "Conflict")]
    [InlineData((int)ErrorType.Unauthorized, "Unauthorized")]
    [InlineData((int)ErrorType.Forbidden, "Forbidden")]
    [InlineData((int)ErrorType.Unexpected, "Internal Server Error")]
    public void GetTitle_MapsAllErrorTypes(int errorTypeInt, string expectedTitle)
    {
        var pd = Result.Failure(Error.Custom("A", "B", (ErrorType)errorTypeInt)).ToProblemDetails();
        ((ProblemHttpResult)pd).ProblemDetails.Title.Should().Be(expectedTitle);
    }

    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(402)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(405)]
    [InlineData(406)]
    [InlineData(407)]
    [InlineData(408)]
    [InlineData(409)]
    [InlineData(410)]
    [InlineData(411)]
    [InlineData(412)]
    [InlineData(413)]
    [InlineData(414)]
    [InlineData(415)]
    [InlineData(416)]
    [InlineData(417)]
    [InlineData(418)]
    [InlineData(422)]
    [InlineData(426)]
    [InlineData(428)]
    [InlineData(429)]
    [InlineData(431)]
    [InlineData(451)]
    [InlineData(499)]
    [InlineData(500)]
    [InlineData(501)]
    [InlineData(502)]
    [InlineData(503)]
    [InlineData(504)]
    [InlineData(505)]
    [InlineData(599)]
    [InlineData(600)]
    public void CreateProblemDetails_HitsAllStatusCodes_ForCanonicalTitleAndRfcSection(int statusCode)
    {
        var opt = new ResultHttpOptions { TypeUriBase = "about:blank" };
        opt.ConfigureStatusCode(ErrorType.Failure, statusCode);

        var pd = Result.Failure(Error.Failure("A", "B")).ToProblemDetails(opt);
        var problem = (ProblemHttpResult)pd;
        problem.StatusCode.Should().Be(statusCode);
        problem.ProblemDetails.Type.Should().Be("about:blank");
        problem.ProblemDetails.Title.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData((ErrorType)0)]
    [InlineData((ErrorType)1)]
    [InlineData((ErrorType)2)]
    [InlineData((ErrorType)3)]
    [InlineData((ErrorType)4)]
    [InlineData((ErrorType)5)]
    [InlineData((ErrorType)6)]
    [InlineData((ErrorType)7)]
    [InlineData((ErrorType)8)]
    [InlineData((ErrorType)9)]
    [InlineData((ErrorType)10)]
    [InlineData((ErrorType)200)]
    public void CreateProblemDetails_HitsAllErrorTypes_ForDescriptiveTitle(ErrorType errorType)
    {
        var pd = Result.Failure(Error.Custom("A", "B", errorType)).ToProblemDetails();
        ((ProblemHttpResult)pd).ProblemDetails.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SerializeMetadataValue_WhenVariousTypes_HandlesProperly()
    {
        var error = Error.Failure("E", "M")
            .WithMetadata("null", null!)
            .WithMetadata("int", 1)
            .WithMetadata("guid", Guid.Empty)
            .WithMetadata("date", DateTime.MinValue)
            .WithMetadata("ts", TimeSpan.Zero)
            .WithMetadata("obj", new object());
        var problem = (ProblemHttpResult)Result.Failure(error).ToProblemDetails();
        problem.ProblemDetails.Extensions.Should().ContainKey("null");
        problem.ProblemDetails.Extensions["null"].Should().BeNull();
        problem.ProblemDetails.Extensions["int"].Should().Be(1);
        problem.ProblemDetails.Extensions["guid"].Should().Be(Guid.Empty);
        problem.ProblemDetails.Extensions["date"].Should().Be(DateTime.MinValue);
        problem.ProblemDetails.Extensions["ts"].Should().Be(TimeSpan.Zero);
        problem.ProblemDetails.Extensions["obj"].Should().Be(new object().ToString());
    }

    [Fact]
    public void ToProblemDetails_WhenVariousUncoveredPaths_HitsProperly()
    {
        var options = new ResultHttpOptions();
        options.ConfigureTitleOverride(ErrorType.Validation, "OVERRIDE");
        var res = (ProblemHttpResult)Result.Failure(Error.Validation("V", "V")).ToProblemDetails(options);
        res.ProblemDetails.Title.Should().Be("OVERRIDE");

        var options2 = new ResultHttpOptions();
        options2.ConfigureStatusCode(ErrorType.Failure, 100);
        var res2 = (ProblemHttpResult)Result.Failure(Error.Failure("F", "F")).ToProblemDetails(options2);
        res2.StatusCode.Should().Be(100);

        var err = Error.Create("F", "F").WithTraceId("T1").WithCorrelationId("C1").Build();
        var options3 = new ResultHttpOptions { IncludeTraceId = true };
        var res3 = (ProblemHttpResult)Result.Failure(err).ToProblemDetails(options3);
        res3.ProblemDetails.Extensions["traceId"].Should().Be("T1");
        res3.ProblemDetails.Extensions["correlationId"].Should().Be("C1");

        foreach (var sc in new[] { 200, 201, 202, 204 })
        {
            var opt = new ResultHttpOptions();
            opt.ConfigureStatusCode(ErrorType.Failure, sc);
            var resSc = (ProblemHttpResult)Result.Failure(Error.Failure("F", "F")).ToProblemDetails(opt);
            resSc.StatusCode.Should().Be(sc);
        }

        Action action = () => Result.Success().ToProblemDetails();
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void HttpExt_WhenMoreMissedPaths_HitsProperly()
    {
        var e = Error.Create("F", "F").WithType(unchecked((ErrorType)255)).Build();
        var opt = new ResultHttpOptions();
        opt.ConfigureStatusCode(unchecked((ErrorType)255), 400);
        opt.ConfigureStatusCode(ErrorType.Failure, 400);
        var pd1 = (ProblemHttpResult)Result.Failure(e).ToProblemDetails(opt);
        pd1.StatusCode.Should().Be(400);
        var pd2 = (ProblemHttpResult)Result.Failure(Error.Failure("F", "F")).ToProblemDetails(opt);
        pd2.StatusCode.Should().Be(400);

        var dict1 = new Dictionary<string, object>();
        var dict2 = new Dictionary<string, object>();
        var dict3 = new Dictionary<string, object>();
        var dict4 = new Dictionary<string, object>();
        var dict5 = new Dictionary<string, object>();
        var dict6 = new Dictionary<string, object>();
        var dict7 = new Dictionary<string, object>();
        var dict8 = new Dictionary<string, object>();
        var dict9 = new Dictionary<string, object>();
        var dict10 = new Dictionary<string, object>();
        var dict11 = new Dictionary<string, object>();
        dict1.Add("2", dict2);
        dict2.Add("3", dict3);
        dict3.Add("4", dict4);
        dict4.Add("5", dict5);
        dict5.Add("6", dict6);
        dict6.Add("7", dict7);
        dict7.Add("8", dict8);
        dict8.Add("9", dict9);
        dict9.Add("10", dict10);
        dict10.Add("11", dict11);
        dict11.Add("12", "too deep");
        var err = Error.Create("F", "F").WithMetadata("deep", dict1).Build();
        var pd3 = (ProblemHttpResult)Result.Failure(err).ToProblemDetails();
        pd3.ProblemDetails.Extensions.Should().ContainKey("deep");
    }

    [Fact]
    public void ToProblemDetails_IncludeDescriptionTrue_IncludesInnerErrorDescription()
    {
        var inner = Error.Failure("I", "Inner Description");
        var result = Result.Failure(Error.Validation("V", "Desc").ToBuilder().WithInnerError(inner).Build());
        var opt = new ResultHttpOptions { IncludeDescription = true };
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)result.ToProblemDetails(opt);
        var errors = (IEnumerable<EricksonLopez.Result.AspNetCore.ErrorDetailDto>)problem.ProblemDetails.Extensions["errors"]!;
        AwesomeAssertions.AssertionExtensions.Should(errors).Contain(e => e.Description == "Inner Description");
    }

    [Fact]
    public void GetDescriptiveTitle_WhenFailureStatusCodeLessThan500_ReturnsOperationFailed()
    {
        var opt = new ResultHttpOptions { TypeUriBase = "https://test.com/" };
        opt.ConfigureStatusCode(ErrorType.Failure, 400);
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)Result.Failure(Error.Failure("F", "Desc")).ToProblemDetails(opt);
        AwesomeAssertions.AssertionExtensions.Should(problem.ProblemDetails.Title).Be("Operation Failed");
    }

    [Fact]
    public void SerializeMetadata_WithNullKey_SkipsOrHandlesNullKey()
    {
        var listDict = new MockNullKeyDict();
        var e3 = Error.Create("F", "F").WithMetadata("dict", listDict).Build();
        var problem4 = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)Result.Failure(e3).ToProblemDetails();
        var outDict = (Dictionary<string, object?>)problem4.ProblemDetails.Extensions["dict"]!;
        AwesomeAssertions.AssertionExtensions.Should(outDict).ContainKey(""); // ?? string.Empty is used for null key
    }

    private class MockNullKeyDict : System.Collections.Hashtable
    {
        public MockNullKeyDict()
        {
        }

        public override System.Collections.IDictionaryEnumerator GetEnumerator()
        {
            return new MockEnumerator();
        }

        private class MockEnumerator : System.Collections.IDictionaryEnumerator
        {
            private int _index = -1;
            public object Current => Entry;
            public System.Collections.DictionaryEntry Entry => _index switch
            {
                0 => new System.Collections.DictionaryEntry(null!, "null value"),
                1 => new System.Collections.DictionaryEntry("", "empty key value"),
                _ => new System.Collections.DictionaryEntry("  ", "whitespace key value")
            };
            public object Key => Entry.Key;
            public object? Value => Entry.Value;
            public bool MoveNext() => ++_index < 3;
            public void Reset() => _index = -1;
        }
    }

    [Fact]
    public void ToProblemDetails_IncludeDescriptionFalse_UsesFallbackDescription()
    {
        var inner = Error.Failure("I", "Inner");
        var result = Result.Failure(Error.Validation("V", "Desc").ToBuilder().WithInnerError(inner).Build());
        var opt = new ResultHttpOptions { IncludeDescription = false, DefaultFallbackDescription = "FB" };
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)result.ToProblemDetails(opt);
        var errors = (IEnumerable<EricksonLopez.Result.AspNetCore.ErrorDetailDto>)problem.ProblemDetails.Extensions["errors"]!;
        AwesomeAssertions.AssertionExtensions.Should(errors).Contain(e => e.Description == "FB");
    }
    [Fact]
    public void ToProblemDetails_IncludeTraceIdTrue_IncludesTraceIdInInnerError()
    {
        var inner = Error.Failure("I", "Inner").ToBuilder().WithTraceId("TID").Build();
        var result = Result.Failure(Error.Validation("V", "Desc").ToBuilder().WithInnerError(inner).Build());
        var opt = new ResultHttpOptions { IncludeTraceId = true };
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)result.ToProblemDetails(opt);
        var errors = (IEnumerable<EricksonLopez.Result.AspNetCore.ErrorDetailDto>)problem.ProblemDetails.Extensions["errors"]!;
        AwesomeAssertions.AssertionExtensions.Should(errors).Contain(e => e.TraceId == "TID");
    }

    [Theory]
    [InlineData((int)ErrorType.Validation, "Validation Error")]
    [InlineData((int)ErrorType.Conflict, "Conflict")]
    [InlineData((int)ErrorType.NotFound, "Not Found")]
    [InlineData((int)ErrorType.Unauthorized, "Unauthorized")]
    [InlineData((int)ErrorType.Forbidden, "Forbidden")]
    [InlineData((int)ErrorType.Unexpected, "Internal Server Error")]
    [InlineData(999, "Operation Failed")]
    public void GetDescriptiveTitle_AllTypes_ReturnsCorrectTitle(int typeInt, string expectedTitle)
    {
        var type = (ErrorType)typeInt;
        var error = Error.Validation("F", "Desc").ToBuilder().WithType(type).Build();
        var opt = new ResultHttpOptions { TypeUriBase = "https://test.com/" };
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)Result.Failure(error).ToProblemDetails(opt);
        AwesomeAssertions.AssertionExtensions.Should(problem.ProblemDetails.Title).Be(expectedTitle);
    }

    [Fact]
    public void ToProblemDetails_IncludeDescriptionFalse_MainErrorUsesFallback()
    {
        var result = Result.Failure(Error.Validation("V", "Desc"));
        var opt = new ResultHttpOptions { IncludeDescription = false, DefaultFallbackDescription = "FB2" };
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)result.ToProblemDetails(opt);
        AwesomeAssertions.AssertionExtensions.Should(problem.ProblemDetails.Detail).Be("FB2");
    }

    [Fact]
    public void ToProblemDetails_IncludeTraceIdFalse_MainErrorOmitsTraceId()
    {
        var result = Result.Failure(Error.Validation("V", "Desc").ToBuilder().WithTraceId("TID").Build());
        var opt = new ResultHttpOptions { IncludeTraceId = false };
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)result.ToProblemDetails(opt);
        AwesomeAssertions.AssertionExtensions.Should(problem.ProblemDetails.Extensions).NotContainKey("traceId");
    }

    [Fact]
    public void ToProblemDetails_TraceIdNull_OmitsTraceId()
    {
        var result = Result.Failure(Error.Validation("V", "Desc").ToBuilder().WithTraceId(null).Build());
        var opt = new ResultHttpOptions { IncludeTraceId = true };
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)result.ToProblemDetails(opt);
        AwesomeAssertions.AssertionExtensions.Should(problem.ProblemDetails.Extensions).NotContainKey("traceId");
    }

    [Fact]
    public void ToProblemDetails_IncludeTraceIdFalse_InnerErrorOmitsTraceId()
    {
        var inner = Error.Failure("I", "Inner").ToBuilder().WithTraceId("TID").Build();
        var result = Result.Failure(Error.Validation("V", "Desc").ToBuilder().WithInnerError(inner).Build());
        var opt = new ResultHttpOptions { IncludeTraceId = false };
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)result.ToProblemDetails(opt);
        var errors = (IEnumerable<EricksonLopez.Result.AspNetCore.ErrorDetailDto>)problem.ProblemDetails.Extensions["errors"]!;
        AwesomeAssertions.AssertionExtensions.Should(errors).Contain(e => e.TraceId == null);
    }

    [Fact]
    public void ToProblemDetails_CorrelationIdNull_OmitsCorrelationId()
    {
        var result = Result.Failure(Error.Validation("V", "Desc").ToBuilder().WithCorrelationId(null).Build());
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)result.ToProblemDetails(new ResultHttpOptions());
        AwesomeAssertions.AssertionExtensions.Should(problem.ProblemDetails.Extensions).NotContainKey("correlationId");
    }

    [Fact]
    public void GetDescriptiveTitle_StatusCodeExactly500_ReturnsInternalServerError()
    {
        var opt = new ResultHttpOptions { TypeUriBase = "https://test.com/" };
        opt.ConfigureStatusCode(ErrorType.Failure, 500);
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)Result.Failure(Error.Failure("F", "Desc")).ToProblemDetails(opt);
        AwesomeAssertions.AssertionExtensions.Should(problem.ProblemDetails.Title).Be("Internal Server Error");
    }

    [Fact]
    public void GetDescriptiveTitle_StatusCodeExactly400_ReturnsBadRequest()
    {
        var opt = new ResultHttpOptions { TypeUriBase = "https://test.com/" };
        opt.ConfigureStatusCode(ErrorType.Failure, 400);
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)Result.Failure(Error.Failure("F", "Desc")).ToProblemDetails(opt);
        AwesomeAssertions.AssertionExtensions.Should(problem.ProblemDetails.Title).Be("Operation Failed");
    }

    [Fact]
    public void GetDescriptiveTitle_StatusCodeLessThen400_ReturnsOperationFailed()
    {
        var opt = new ResultHttpOptions { TypeUriBase = "https://test.com/" };
        opt.ConfigureStatusCode(ErrorType.Failure, 200);
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)Result.Failure(Error.Failure("F", "Desc")).ToProblemDetails(opt);
        AwesomeAssertions.AssertionExtensions.Should(problem.ProblemDetails.Title).Be("Operation Failed");
    }

    [Fact]
    public void ToProblemDetails_WhenSuccessResult_ThrowsSpecificException()
    {
        Action action1 = () => Result.Success().ToProblemDetails();
        var ex = action1.Should().Throw<InvalidOperationException>().Which;
        AwesomeAssertions.AssertionExtensions.Should(ex.Message).Be("Cannot create ProblemDetails from a successful result.");

        Action action2 = () => Result.Success(42).ToProblemDetails();
        var exGeneric = action2.Should().Throw<InvalidOperationException>().Which;
        AwesomeAssertions.AssertionExtensions.Should(exGeneric.Message).Be("Cannot create ProblemDetails from a successful result.");
    }

    [Fact]
    public void Extensions_WhenChecked_ContainSeverityAndRetryability()
    {
        var result = Result.Failure(Error.Failure("F", "Desc"));
        var problem = (ProblemHttpResult)result.ToProblemDetails();
        problem.ProblemDetails.Extensions["severity"].Should().Be("Error");
        problem.ProblemDetails.Extensions["retryability"].Should().Be("NotApplicable");
    }

    [Fact]
    public void ToProblemDetailsOfT_WithCustomOptions_AppliesOptions()
    {
        var opt = new ResultHttpOptions { IncludeDescription = false, DefaultFallbackDescription = "Custom Fallback" };
        var result = Result.Failure<int>(Error.Validation("V", "Desc"));
        var problem = (ProblemHttpResult)result.ToProblemDetails(opt);
        problem.ProblemDetails.Detail.Should().Be("Custom Fallback");
    }

    [Fact]
    public void ToHttpResultOfT_WithCustomOptions_AppliesOptions()
    {
        var opt = new ResultHttpOptions { IncludeDescription = false, DefaultFallbackDescription = "Custom Fallback" };
        var failureResult = Result.Failure<int>(Error.Validation("V", "Desc"));
        var problem = (ProblemHttpResult)failureResult.ToHttpResult(opt);
        problem.ProblemDetails.Detail.Should().Be("Custom Fallback");

        var successResult = Result.Success(100);
        var ok = (Ok<int>)successResult.ToHttpResult(opt);
        ok.Value.Should().Be(100);
    }

    [Theory]
    [InlineData("errorCode", "meta.errorCode")]
    [InlineData("severity", "meta.severity")]
    [InlineData("retryability", "meta.retryability")]
    [InlineData("errors", "meta.errors")]
    [InlineData("traceId", "meta.traceId")]
    [InlineData("correlationId", "meta.correlationId")]
    public void MetadataKeyCollisions_ArePrefixedWithMeta(string reservedKey, string expectedPrefixedKey)
    {
        var error = Error.Validation("V", "Desc").ToBuilder()
            .WithMetadata(reservedKey, "custom-value")
            .Build();
        var result = Result.Failure(error);

        var problem = (ProblemHttpResult)result.ToProblemDetails();
        problem.ProblemDetails.Extensions.Should().ContainKey(expectedPrefixedKey);
        problem.ProblemDetails.Extensions[expectedPrefixedKey].Should().Be("custom-value");
    }

    [Theory]
    [InlineData(400, "15.5.1")]
    [InlineData(401, "15.5.2")]
    [InlineData(402, "15.5.3")]
    [InlineData(403, "15.5.4")]
    [InlineData(404, "15.5.5")]
    [InlineData(405, "15.5.6")]
    [InlineData(406, "15.5.7")]
    [InlineData(407, "15.5.8")]
    [InlineData(408, "15.5.9")]
    [InlineData(409, "15.5.10")]
    [InlineData(410, "15.5.11")]
    [InlineData(411, "15.5.12")]
    [InlineData(412, "15.5.13")]
    [InlineData(413, "15.5.14")]
    [InlineData(414, "15.5.15")]
    [InlineData(415, "15.5.16")]
    [InlineData(416, "15.5.17")]
    [InlineData(417, "15.5.18")]
    [InlineData(418, "15.5")]
    [InlineData(422, "15.5.21")]
    [InlineData(426, "15.5.22")]
    [InlineData(428, "15.5.25")]
    [InlineData(429, "15.5.26")]
    [InlineData(431, "15.5.28")]
    [InlineData(451, "15.5.30")]
    [InlineData(500, "15.6.1")]
    [InlineData(501, "15.6.2")]
    [InlineData(502, "15.6.3")]
    [InlineData(503, "15.6.4")]
    [InlineData(504, "15.6.5")]
    [InlineData(505, "15.6.6")]
    [InlineData(599, "15.6")]
    [InlineData(499, "15.5")]
    [InlineData(399, "15")]
    public void GetRfcSection_MapsAllStatusCodesCorrectly(int statusCode, string expectedSection)
    {
        var opt = new ResultHttpOptions { TypeUriBase = "https://rfc-editor.org/rfc/rfc9110#" };
        opt.ConfigureStatusCode(ErrorType.Custom, statusCode);

        var result = Result.Failure(Error.Create("C", "Desc").WithType(ErrorType.Custom).Build());
        var problem = (ProblemHttpResult)result.ToProblemDetails(opt);
        problem.ProblemDetails.Type.Should().Be($"https://rfc-editor.org/rfc/rfc9110#{expectedSection}");
    }

    [Theory]
    [InlineData(200, "OK")]
    [InlineData(201, "Created")]
    [InlineData(202, "Accepted")]
    [InlineData(204, "No Content")]
    [InlineData(400, "Bad Request")]
    [InlineData(401, "Unauthorized")]
    [InlineData(402, "Payment Required")]
    [InlineData(403, "Forbidden")]
    [InlineData(404, "Not Found")]
    [InlineData(405, "Method Not Allowed")]
    [InlineData(406, "Not Acceptable")]
    [InlineData(408, "Request Timeout")]
    [InlineData(409, "Conflict")]
    [InlineData(410, "Gone")]
    [InlineData(412, "Precondition Failed")]
    [InlineData(413, "Content Too Large")]
    [InlineData(415, "Unsupported Media Type")]
    [InlineData(422, "Unprocessable Content")]
    [InlineData(429, "Too Many Requests")]
    [InlineData(500, "Internal Server Error")]
    [InlineData(501, "Not Implemented")]
    [InlineData(502, "Bad Gateway")]
    [InlineData(503, "Service Unavailable")]
    [InlineData(504, "Gateway Timeout")]
    [InlineData(599, "Internal Server Error")]
    [InlineData(499, "Client Error")]
    [InlineData(399, "Error")]
    public void GetCanonicalHttpTitle_MapsAllStatusCodesCorrectly(int statusCode, string expectedTitle)
    {
        var opt = new ResultHttpOptions { TypeUriBase = "about:blank" };
        opt.ConfigureStatusCode(ErrorType.Custom, statusCode);

        var result = Result.Failure(Error.Create("C", "Desc").WithType(ErrorType.Custom).Build());
        var problem = (ProblemHttpResult)result.ToProblemDetails(opt);
        problem.ProblemDetails.Title.Should().Be(expectedTitle);
    }

    [Theory]
    [InlineData(ErrorType.Failure, 500, "Internal Server Error")]
    [InlineData(ErrorType.Failure, 400, "Operation Failed")]
    [InlineData(ErrorType.Validation, 400, "Validation Error")]
    [InlineData(ErrorType.NotFound, 404, "Not Found")]
    [InlineData(ErrorType.Conflict, 409, "Conflict")]
    [InlineData(ErrorType.Unauthorized, 401, "Unauthorized")]
    [InlineData(ErrorType.Forbidden, 403, "Forbidden")]
    [InlineData(ErrorType.Unavailable, 503, "Service Unavailable")]
    [InlineData(ErrorType.Unexpected, 500, "Internal Server Error")]
    [InlineData(ErrorType.Domain, 422, "Domain Rule Violation")]
    [InlineData(ErrorType.Infrastructure, 500, "Infrastructure Error")]
    [InlineData(ErrorType.Custom, 500, "Application Error")]
    [InlineData((ErrorType)255, 400, "Operation Failed")]
    public void GetDescriptiveTitle_MapsAllErrorTypesCorrectly(ErrorType errorType, int statusCode, string expectedTitle)
    {
        var opt = new ResultHttpOptions { TypeUriBase = "https://example.com/errors#" };
        opt.ConfigureStatusCode(errorType, statusCode);

        var result = Result.Failure(Error.Create("CODE", "Desc").WithType(errorType).Build());
        var problem = (ProblemHttpResult)result.ToProblemDetails(opt);
        problem.ProblemDetails.Title.Should().Be(expectedTitle);
    }

    private class CustomFormattable : IFormattable
    {
        public string ToString(string? format, System.IFormatProvider? formatProvider) => "FormattedCustomValue";
    }

    private class NullKeyCustomDictionary : System.Collections.IDictionary
    {
        public object? this[object key] { get => "nullKeyVal"; set { } }
        public System.Collections.ICollection Keys => new[] { (object?)null };
        public System.Collections.ICollection Values => new[] { "nullKeyVal" };
        public bool IsReadOnly => true;
        public bool IsFixedSize => true;
        public int Count => 1;
        public object SyncRoot => this;
        public bool IsSynchronized => false;
        public void Add(object key, object? value) { }
        public void Clear() { }
        public bool Contains(object key) => true;
        public System.Collections.IDictionaryEnumerator GetEnumerator() => new NullKeyDictEnum();
        public void Remove(object key) { }
        public void CopyTo(Array array, int index) { }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        private class NullKeyDictEnum : System.Collections.IDictionaryEnumerator
        {
            private int _index = -1;
            public System.Collections.DictionaryEntry Entry => new(null!, "nullKeyVal");
            public object Key => null!;
            public object? Value => "nullKeyVal";
            public object Current => Entry;
            public bool MoveNext() => ++_index == 0;
            public void Reset() => _index = -1;
        }
    }

    [Fact]
    public void SerializeMetadataValue_WhenAllTypesAndRecursionLimit_HandlesProperly()
    {
        var now = DateTime.UtcNow;
        var nowOffset = System.DateTimeOffset.UtcNow;
        var guid = Guid.NewGuid();
        var span = TimeSpan.FromSeconds(30);

        var innerDict = new Dictionary<string, object?>
        {
            { "k1", "v1" }
        };

        // Create 6-level deeply nested dictionary
        var d5 = new Dictionary<string, object?> { { "leaf", "value" } };
        var d4 = new Dictionary<string, object?> { { "d5", d5 } };
        var d3 = new Dictionary<string, object?> { { "d4", d4 } };
        var d2 = new Dictionary<string, object?> { { "d3", d3 } };
        var d1 = new Dictionary<string, object?> { { "d2", d2 } };
        var d0 = new Dictionary<string, object?> { { "d1", d1 } };

        // Deep list
        var l5 = new List<object> { "deepItem" };
        var l4 = new List<object> { l5 };
        var l3 = new List<object> { l4 };
        var l2 = new List<object> { l3 };
        var l1 = new List<object> { l2 };
        var l0 = new List<object> { l1 };

        var error = Error.Validation("V", "Desc").ToBuilder()
            .WithMetadata("str", "stringVal")
            .WithMetadata("b", true)
            .WithMetadata("i32", 123)
            .WithMetadata("i64", 123456789L)
            .WithMetadata("i16", (short)12)
            .WithMetadata("u8", (byte)1)
            .WithMetadata("u32", 123u)
            .WithMetadata("u64", 123456789UL)
            .WithMetadata("u16", (ushort)12)
            .WithMetadata("s8", (sbyte)1)
            .WithMetadata("f32", 1.5f)
            .WithMetadata("f64", 2.5d)
            .WithMetadata("m", 3.5m)
            .WithMetadata("guid", guid)
            .WithMetadata("dt", now)
            .WithMetadata("dto", nowOffset)
            .WithMetadata("ts", span)
            .WithMetadata("customFormat", new CustomFormattable())
            .WithMetadata("dict", innerDict)
            .WithMetadata("nullKeyDict", new NullKeyCustomDictionary())
            .WithMetadata("deepDict", d0)
            .WithMetadata("deepList", l0)
            .WithMetadata("unknownObj", new System.Text.StringBuilder("BuilderString"))
            .Build();

        var result = Result.Failure(error);
        var problem = (ProblemHttpResult)result.ToProblemDetails();

        problem.ProblemDetails.Extensions["str"].Should().Be("stringVal");
        problem.ProblemDetails.Extensions["b"].Should().Be(true);
        problem.ProblemDetails.Extensions["i32"].Should().Be(123);
        problem.ProblemDetails.Extensions["customFormat"].Should().Be("FormattedCustomValue");
        problem.ProblemDetails.Extensions["unknownObj"].Should().Be("BuilderString");

        var nullKeyDict = (Dictionary<string, object?>)problem.ProblemDetails.Extensions["nullKeyDict"]!;
        nullKeyDict[""].Should().Be("nullKeyVal");

        // Verify recursion depth limit was hit
        var deepDictResult = (Dictionary<string, object?>)problem.ProblemDetails.Extensions["deepDict"]!;
        var level1 = (Dictionary<string, object?>)deepDictResult["d1"]!;
        var level2 = (Dictionary<string, object?>)level1["d2"]!;
        var level3 = (Dictionary<string, object?>)level2["d3"]!;
        var level4 = (Dictionary<string, object?>)level3["d4"]!;
        level4["d5"].Should().Be("[Depth Limit Exceeded]");

        var deepListResult = (List<object?>)problem.ProblemDetails.Extensions["deepList"]!;
        var list1 = (List<object?>)deepListResult[0]!;
        var list2 = (List<object?>)list1[0]!;
        var list3 = (List<object?>)list2[0]!;
        var list4 = (List<object?>)list3[0]!;
        list4[0].Should().Be("[Depth Limit Exceeded]");
    }
}






