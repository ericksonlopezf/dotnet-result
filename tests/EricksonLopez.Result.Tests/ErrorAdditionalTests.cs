using System;
using System.Collections.Generic;
using Xunit;
using AwesomeAssertions;

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
        
        Assert.Equal("CODE", error.Code);
        Assert.Equal("DESC", error.Description);
        Assert.Equal(ErrorType.Domain, error.Type);
        Assert.Equal(ErrorSeverity.Critical, error.Severity);
        Assert.Equal(ErrorRetryability.Transient, error.Retryability);
        Assert.Equal("value", error.Metadata["key"]);
        Assert.Equal(2, error.InnerErrors.Length);
        Assert.Equal(inner1, error.InnerErrors[0]);
    }

    [Fact]
    public void Custom_WithIReadOnlyListInnerErrors_SetsProperties()
    {
        var inner1 = Error.Failure("1", "1");
        var list = new List<Error> { inner1 };
        var dict = new Dictionary<string, object> { { "key", "value" } };
        
        var error = Error.Custom("CODE", "DESC", ErrorType.Domain, ErrorSeverity.Critical, ErrorRetryability.Transient, "descKey", "traceId", "corrId", list, dict);
        
        Assert.Equal("CODE", error.Code);
        Assert.Equal("descKey", error.DescriptionKey);
        Assert.Equal("traceId", error.TraceId);
        Assert.Equal("corrId", error.CorrelationId);
        Assert.Equal("value", error.Metadata["key"]);
        Assert.Single(error.InnerErrors);
        Assert.Equal(inner1, error.InnerErrors[0]);
    }

    [Fact]
    public void WithMetadata_IReadOnlyDictionary_AddsMetadata()
    {
        var error = Error.Failure("A", "B");
        var dict = new Dictionary<string, object> { { "k1", "v1" }, { "k2", "v2" } };
        
        var error2 = error.WithMetadata(dict);
        
        Assert.Equal("v1", error2.Metadata["k1"]);
        Assert.Equal("v2", error2.Metadata["k2"]);
    }
    
    [Fact]
    public void WithMetadata_IEnumerable_AddsMetadata()
    {
        var error = Error.Failure("A", "B");
        IEnumerable<KeyValuePair<string, object>> entries = new[] { new KeyValuePair<string, object>("k1", "v1") };
        
        var error2 = error.WithMetadata(entries);
        
        Assert.Equal("v1", error2.Metadata["k1"]);
    }

    [Fact]
    public void TryGetMetadata_NullValue_ReturnsFalse()
    {
        var error = Error.Failure("A", "B").WithMetadata("key", null!);
        
        Assert.False(error.TryGetMetadata<string>("key", out var val));
        Assert.Null(val);
    }

    [Fact]
    public void TryGetMetadata_WrongType_ThrowsInvalidCastException()
    {
        var error = Error.Failure("A", "B").WithMetadata("key", 42);
        
        var ex = Assert.Throws<InvalidCastException>(() => error.TryGetMetadata<string>("key", out _));
        Assert.Contains("cannot be cast to", ex.Message);
    }

    [Fact]
    public void GetMetadata_KeyNotFound_ThrowsKeyNotFoundException()
    {
        var error = Error.Failure("A", "B");
        Assert.Throws<KeyNotFoundException>(() => error.GetMetadata<string>("missing"));
    }

    [Fact]
    public void GetMetadata_NullValue_ThrowsInvalidCastException()
    {
        var error = Error.Failure("A", "B").WithMetadata("key", null!);
        var ex = Assert.Throws<InvalidCastException>(() => error.GetMetadata<string>("key"));
        Assert.Contains("null value which cannot be cast", ex.Message);
    }

    [Fact]
    public void GetMetadata_WrongType_ThrowsInvalidCastException()
    {
        var error = Error.Failure("A", "B").WithMetadata("key", 42);
        var ex = Assert.Throws<InvalidCastException>(() => error.GetMetadata<string>("key"));
        Assert.Contains("cannot be cast to", ex.Message);
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
        Action act = () => e.GetMetadata<string>("k");
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
