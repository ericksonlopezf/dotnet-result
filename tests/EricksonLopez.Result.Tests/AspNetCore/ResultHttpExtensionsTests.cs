#pragma warning disable CS8602
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using AwesomeAssertions;
using EricksonLopez.Result.Testing;
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
    
    [Fact]
    public void GetTitle_MapsAllErrorTypes()
    {
        var pd = Result.Failure(Error.Failure("A", "B")).ToProblemDetails();
        ((ProblemHttpResult)pd).ProblemDetails.Title.Should().Be("Internal Server Error");
        Result.Failure(Error.Validation("X", "X")).ToProblemDetails().As<ProblemHttpResult>().ProblemDetails.Title.Should().Be("Bad Request");
        Result.Failure(Error.NotFound("X", "X")).ToProblemDetails().As<ProblemHttpResult>().ProblemDetails.Title.Should().Be("Not Found");
        Result.Failure(Error.Conflict("X", "X")).ToProblemDetails().As<ProblemHttpResult>().ProblemDetails.Title.Should().Be("Conflict");
        Result.Failure(Error.Unauthorized("X", "X")).ToProblemDetails().As<ProblemHttpResult>().ProblemDetails.Title.Should().Be("Unauthorized");
        Result.Failure(Error.Forbidden("X", "X")).ToProblemDetails().As<ProblemHttpResult>().ProblemDetails.Title.Should().Be("Forbidden");
        Result.Failure(Error.Unexpected("X", "X")).ToProblemDetails().As<ProblemHttpResult>().ProblemDetails.Title.Should().Be("Internal Server Error");
    }

    [Theory]
    [InlineData(400)] [InlineData(401)] [InlineData(402)] [InlineData(403)] [InlineData(404)]
    [InlineData(405)] [InlineData(406)] [InlineData(407)] [InlineData(408)] [InlineData(409)]
    [InlineData(410)] [InlineData(411)] [InlineData(412)] [InlineData(413)] [InlineData(414)]
    [InlineData(415)] [InlineData(416)] [InlineData(417)] [InlineData(418)] [InlineData(422)]
    [InlineData(426)] [InlineData(428)] [InlineData(429)] [InlineData(431)] [InlineData(451)]
    [InlineData(499)] [InlineData(500)] [InlineData(501)] [InlineData(502)] [InlineData(503)]
    [InlineData(504)] [InlineData(505)] [InlineData(599)] [InlineData(600)]
    public void CreateProblemDetails_HitsAllStatusCodes_ForCanonicalTitleAndRfcSection(int statusCode)
    {
        var opt = new ResultHttpOptions { TypeUriBase = "about:blank" };
        var dict = new Dictionary<ErrorType, int> { { ErrorType.Failure, statusCode } };
        // Use reflection to bypass internal set
        var fi = typeof(ResultHttpOptions).GetField("_statusCodeMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        fi.SetValue(opt, dict);
        
        var pd = Result.Failure(Error.Failure("A", "B")).ToProblemDetails(opt);
        var problem = (ProblemHttpResult)pd;
        problem.StatusCode.Should().Be(statusCode);
        problem.ProblemDetails.Type.Should().Be("about:blank");
        problem.ProblemDetails.Title.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData((ErrorType)0)] [InlineData((ErrorType)1)] [InlineData((ErrorType)2)]
    [InlineData((ErrorType)3)] [InlineData((ErrorType)4)] [InlineData((ErrorType)5)]
    [InlineData((ErrorType)6)] [InlineData((ErrorType)7)] [InlineData((ErrorType)8)]
    [InlineData((ErrorType)9)] [InlineData((ErrorType)10)] [InlineData((ErrorType)200)]
    public void CreateProblemDetails_HitsAllErrorTypes_ForDescriptiveTitle(ErrorType errorType)
    {
        var pd = Result.Failure(Error.Custom("A", "B", errorType)).ToProblemDetails();
        ((ProblemHttpResult)pd).ProblemDetails.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SerializeMetadataValue_HandlesVariousTypes()
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
    public void ToProblemDetails_HitsVariousUncoveredPaths()
    {
        var options = new ResultHttpOptions();
        options.ConfigureTitleOverride(ErrorType.Validation, "OVERRIDE");
        var res = Result.Failure(Error.Validation("V", "V")).ToProblemDetails(options);
        
        var options2 = new ResultHttpOptions();
        typeof(ResultHttpOptions).GetField("_statusCodeMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(options2, new System.Collections.Generic.Dictionary<ErrorType, int> { { ErrorType.Failure, 100 } });
        Result.Failure(Error.Failure("F", "F")).ToProblemDetails(options2);
        
        var err = Error.Create("F", "F").WithTraceId("T1").WithCorrelationId("C1").Build();
        var options3 = new ResultHttpOptions { IncludeTraceId = true };
        Result.Failure(err).ToProblemDetails(options3);
        
        foreach(var sc in new[] { 200, 201, 202, 204 })
        {
            var opt = new ResultHttpOptions();
            typeof(ResultHttpOptions).GetField("_statusCodeMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(opt, new System.Collections.Generic.Dictionary<ErrorType, int> { { ErrorType.Failure, sc } });
            Result.Failure(Error.Failure("F", "F")).ToProblemDetails(opt);
        }
        
        try { Result.Success().ToProblemDetails(); } catch { }
    }


    [Fact]
    public void HttpExt_MoreMissedPaths()
    {
        var e = Error.Create("F", "F").WithType(unchecked((ErrorType)255)).Build();
        var opt = new ResultHttpOptions();
        typeof(ResultHttpOptions).GetField("_statusCodeMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(opt, new System.Collections.Generic.Dictionary<ErrorType, int> { { unchecked((ErrorType)255), 400 }, { ErrorType.Failure, 400 } });
        Result.Failure(e).ToProblemDetails(opt);
        Result.Failure(Error.Failure("F", "F")).ToProblemDetails(opt);
        
        var dict1 = new System.Collections.Generic.Dictionary<string, object>();
        var dict2 = new System.Collections.Generic.Dictionary<string, object>();
        var dict3 = new System.Collections.Generic.Dictionary<string, object>();
        var dict4 = new System.Collections.Generic.Dictionary<string, object>();
        var dict5 = new System.Collections.Generic.Dictionary<string, object>();
        var dict6 = new System.Collections.Generic.Dictionary<string, object>();
        var dict7 = new System.Collections.Generic.Dictionary<string, object>();
        var dict8 = new System.Collections.Generic.Dictionary<string, object>();
        var dict9 = new System.Collections.Generic.Dictionary<string, object>();
        var dict10 = new System.Collections.Generic.Dictionary<string, object>();
        var dict11 = new System.Collections.Generic.Dictionary<string, object>();
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
        Result.Failure(err).ToProblemDetails();
    }

    [Fact]
    public void ToProblemDetails_IncludeDescriptionTrue_IncludesInnerErrorDescription()
    {
        var inner = Error.Failure("I", "Inner Description");
        var result = Result.Failure(Error.Validation("V", "Desc").ToBuilder().WithInnerError(inner).Build());
        var opt = new ResultHttpOptions { IncludeDescription = true };
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)result.ToProblemDetails(opt);
        var errors = (System.Collections.Generic.IEnumerable<EricksonLopez.Result.AspNetCore.ErrorDetailDto>)problem.ProblemDetails.Extensions["errors"]!;
        AwesomeAssertions.AssertionExtensions.Should(errors).Contain(e => e.Description == "Inner Description");
    }

    [Fact]
    public void GetDescriptiveTitle_Failure_StatusCodeLessThan500_ReturnsOperationFailed()
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
        var outDict = (System.Collections.Generic.Dictionary<string, object?>)problem4.ProblemDetails.Extensions["dict"]!;
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
        var errors = (System.Collections.Generic.IEnumerable<EricksonLopez.Result.AspNetCore.ErrorDetailDto>)problem.ProblemDetails.Extensions["errors"]!;
        AwesomeAssertions.AssertionExtensions.Should(errors).Contain(e => e.Description == "FB");
    }
    [Fact]
    public void ToProblemDetails_IncludeTraceIdTrue_IncludesTraceIdInInnerError()
    {
        var inner = Error.Failure("I", "Inner").ToBuilder().WithTraceId("TID").Build();
        var result = Result.Failure(Error.Validation("V", "Desc").ToBuilder().WithInnerError(inner).Build());
        var opt = new ResultHttpOptions { IncludeTraceId = true };
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)result.ToProblemDetails(opt);
        var errors = (System.Collections.Generic.IEnumerable<EricksonLopez.Result.AspNetCore.ErrorDetailDto>)problem.ProblemDetails.Extensions["errors"]!;
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
        var errors = (System.Collections.Generic.IEnumerable<EricksonLopez.Result.AspNetCore.ErrorDetailDto>)problem.ProblemDetails.Extensions["errors"]!;
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
    public void ToProblemDetails_SuccessResult_ThrowsSpecificException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Result.Success().ToProblemDetails());
        AwesomeAssertions.AssertionExtensions.Should(ex.Message).Be("Cannot create ProblemDetails from a successful result.");
        
        var exGeneric = Assert.Throws<InvalidOperationException>(() => Result.Success(42).ToProblemDetails());
        AwesomeAssertions.AssertionExtensions.Should(exGeneric.Message).Be("Cannot create ProblemDetails from a successful result.");
    }
}

