// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ErrorAdditionalTests
{
    [Fact]
    public void Custom_WithParamsInnerErrors_SetsProperties()
    {
        var inner1 = Error.Failure("1", "1");
        var inner2 = Error.Failure("2", "2");
        var dict = new Dictionary<string, object> { { "key", "value" } };

        var error = Error.Custom("CODE", "DESC", ErrorType.Domain, ErrorSeverity.Critical, ErrorRetryability.Transient, dict, inner1, inner2);

        error.Code.Should().Be("CODE");
        error.Description.Should().Be("DESC");
        error.Type.Should().Be(ErrorType.Domain);
        error.Severity.Should().Be(ErrorSeverity.Critical);
        error.Retryability.Should().Be(ErrorRetryability.Transient);
        error.Metadata["key"].Should().Be("value");
        error.InnerErrors.Length.Should().Be(2);
        error.InnerErrors[0].Should().Be(inner1);
    }

    [Fact]
    public void Custom_WithIReadOnlyListInnerErrors_SetsProperties()
    {
        var inner1 = Error.Failure("1", "1");
        var list = new List<Error> { inner1 };
        var dict = new Dictionary<string, object> { { "key", "value" } };

        var error = Error.Custom("CODE", "DESC", ErrorType.Domain, ErrorSeverity.Critical, ErrorRetryability.Transient, "descKey", "traceId", "corrId", list, dict);

        error.Code.Should().Be("CODE");
        error.DescriptionKey.Should().Be("descKey");
        error.TraceId.Should().Be("traceId");
        error.CorrelationId.Should().Be("corrId");
        error.Metadata["key"].Should().Be("value");
        error.InnerErrors.Should().ContainSingle();
        error.InnerErrors[0].Should().Be(inner1);
    }

    [Fact]
    public void WithMetadata_IReadOnlyDictionary_AddsMetadata()
    {
        var error = Error.Failure("A", "B");
        var dict = new Dictionary<string, object> { { "k1", "v1" }, { "k2", "v2" } };

        var error2 = error.WithMetadata(dict);

        error2.Metadata["k1"].Should().Be("v1");
        error2.Metadata["k2"].Should().Be("v2");
    }

    [Fact]
    public void WithMetadata_IEnumerable_AddsMetadata()
    {
        var error = Error.Failure("A", "B");
        IEnumerable<KeyValuePair<string, object>> entries = new[] { new KeyValuePair<string, object>("k1", "v1") };

        var error2 = error.WithMetadata(entries);

        error2.Metadata["k1"].Should().Be("v1");
    }

    [Fact]
    public void TryGetMetadata_NullValue_ReturnsFalse()
    {
        var error = Error.Failure("A", "B").WithMetadata("key", null!);

        Assert.False(error.TryGetMetadata<string>("key", out var val));
        val.Should().BeNull();
    }

    [Fact]
    public void TryGetMetadata_WrongType_ThrowsInvalidCastException()
    {
        var error = Error.Failure("A", "B").WithMetadata("key", 42);

        Action act = () => error.TryGetMetadata<string>("key", out _);
        var ex = act.Should().Throw<InvalidCastException>().Which;
        ex.Message.Should().Contain("cannot be cast to");
    }

    [Fact]
    public void GetMetadata_KeyNotFound_ThrowsKeyNotFoundException()
    {
        var error = Error.Failure("A", "B");
        Action act = () => _ = error.GetMetadata<string>("missing");
        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void GetMetadata_NullValue_ThrowsInvalidCastException()
    {
        var error = Error.Failure("A", "B").WithMetadata("key", null!);
        Action act = () => _ = error.GetMetadata<string>("key");
        var ex = act.Should().Throw<InvalidCastException>().Which;
        ex.Message.Should().Contain("null value which cannot be cast");
    }

    [Fact]
    public void GetMetadata_WrongType_ThrowsInvalidCastException()
    {
        var error = Error.Failure("A", "B").WithMetadata("key", 42);
        Action act = () => _ = error.GetMetadata<string>("key");
        var ex = act.Should().Throw<InvalidCastException>().Which;
        ex.Message.Should().Contain("cannot be cast to");
    }

    [Fact]
    public void WithTraceId_WithActivityTraceId_CreatesCopy()
    {
        var traceId = System.Diagnostics.ActivityTraceId.CreateRandom();
        var e = Error.Failure("code", "desc").WithTraceId(traceId);
        e.TraceId.Should().Be(traceId.ToString());
    }

    [Fact]
    public void ClearTraceId_CreatesCopy()
    {
        var e = Error.Failure("code", "desc").WithTraceId("test").ClearTraceId();
        e.TraceId.Should().BeNull();
    }

    [Fact]
    public void TryGetMetadata_InvalidCast_Throws()
    {
        var e = Error.Failure("C", "D").WithMetadata("k", 123);
        Action act = () => e.TryGetMetadata<string>("k", out _);
        act.Should().Throw<InvalidCastException>();
    }

    [Fact]
    public void TryGetMetadata_NotFound_ReturnsFalse()
    {
        var e = Error.Failure("C", "D");
        e.TryGetMetadata<string>("k", out var v).Should().BeFalse();
        v.Should().BeNull();
    }

    [Fact]
    public void GetMetadata_NullValue_Throws()
    {
        var e = Error.Failure("C", "D").WithMetadata("k", null!);
        Action act = () => _ = e.GetMetadata<string>("k");
        act.Should().Throw<InvalidCastException>();
    }

    [Fact]
    public void ErrorBuilder_WithTraceIdActivity_SetsCorrectly()
    {
        var traceId = System.Diagnostics.ActivityTraceId.CreateRandom();
        var b = Error.Create("C", "D").WithTraceId(traceId).Build();
        b.TraceId.Should().Be(traceId.ToString());
    }
}
