using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ErrorComprehensiveTests
{
    [Fact]
    public void AllFactoryMethods_WorkAsExpected()
    {

        var eNotFound = Error.NotFound("E1", "Not found");
        Assert.Equal(ErrorType.NotFound, eNotFound.Type);
        Assert.Equal(ErrorSeverity.Warning, eNotFound.Severity);

        var eConflict = Error.Conflict("E1", "Conflict");
        Assert.Equal(ErrorType.Conflict, eConflict.Type);
        Assert.Equal(ErrorSeverity.Warning, eConflict.Severity);

        var eUnauthorized = Error.Unauthorized("E1", "Unauthorized");
        Assert.Equal(ErrorType.Unauthorized, eUnauthorized.Type);
        Assert.Equal(ErrorSeverity.Error, eUnauthorized.Severity);

        var eForbidden = Error.Forbidden("E1", "Forbidden");
        Assert.Equal(ErrorType.Forbidden, eForbidden.Type);
        Assert.Equal(ErrorSeverity.Error, eForbidden.Severity);

        var eUnexpected = Error.Unexpected("E1", "Unexpected");
        Assert.Equal(ErrorType.Unexpected, eUnexpected.Type);
        Assert.Equal(ErrorSeverity.Critical, eUnexpected.Severity);

        var eFailureInner = Error.Failure("E1", "msg", Error.Failure("E2", "msg2"));
        Assert.Single(eFailureInner.InnerErrors);

        var eValidationInner = Error.Validation("E1", "msg", Error.Validation("E2", "msg2"));
        Assert.Single(eValidationInner.InnerErrors);
        
        var custom1 = Error.Custom("C1", "M", ErrorType.Custom, ErrorSeverity.Info, ErrorRetryability.Transient, "desc", "trace", "corr", [eFailureInner], new Dictionary<string, object>{{"k","v"}});
        Assert.Equal(ErrorType.Custom, custom1.Type);
        Assert.Equal(ErrorSeverity.Info, custom1.Severity);
        Assert.Equal(ErrorRetryability.Transient, custom1.Retryability);
        Assert.Equal("desc", custom1.DescriptionKey);
        Assert.Equal("trace", custom1.TraceId);
        Assert.Equal("corr", custom1.CorrelationId);
        Assert.Single(custom1.InnerErrors);
        Assert.True(custom1.HasMetadata);
        Assert.Equal("v", custom1.Metadata["k"]);

        var custom2 = Error.Custom("C2", "M", ErrorType.Domain, ErrorSeverity.Warning, ErrorRetryability.Permanent, new Dictionary<string, object>{{"k2","v2"}}, eFailureInner);
        Assert.Equal(ErrorType.Domain, custom2.Type);
        Assert.Single(custom2.InnerErrors);
        Assert.Equal("v2", custom2.Metadata["k2"]);
    }

    [Fact]
    public void FluentMethods_WorkAsExpected()
    {
        var e1 = Error.Failure("E1", "M");
        
        var e2 = e1.WithCorrelationId("corr");
        Assert.Equal("corr", e2.CorrelationId);

        var e3 = e2.WithDescriptionKey("key");
        Assert.Equal("key", e3.DescriptionKey);

        var e4 = e3.WithRetryability(ErrorRetryability.Permanent);
        Assert.Equal(ErrorRetryability.Permanent, e4.Retryability);

        var dic = new Dictionary<string, object> { { "k3", "v3" } };
        var e5 = e4.WithMetadata(dic);
        Assert.Equal("v3", e5.Metadata["k3"]);
        
        var e6 = e5.WithMetadata(new Dictionary<string, object>());
        Assert.Same(e6, e5); // Should return this if count is 0

        var e7 = e6.WithMetadata("k4", "v4");
        Assert.Equal("v4", e7.Metadata["k4"]);

        Assert.Equal("[Failure] E1: M", e1.ToString());
    }

    [Fact]
    public void StrictEquals_AllBranches()
    {
        var e1 = Error.Failure("E1", "M").WithRetryability(ErrorRetryability.Permanent);
        var e2 = Error.Failure("E1", "M").WithRetryability(ErrorRetryability.Transient);
        Assert.False(e1.StrictEquals(e2));

        var e3 = Error.Failure("E1", "M").WithDescriptionKey("a");
        var e4 = Error.Failure("E1", "M").WithDescriptionKey("b");
        Assert.False(e3.StrictEquals(e4));

        var e5 = Error.Failure("E1", "M").WithCorrelationId("a");
        var e6 = Error.Failure("E1", "M").WithCorrelationId("b");
        Assert.False(e5.StrictEquals(e6));

        var e7 = Error.Failure("E1", "M", Error.Failure("E2", "M2"));
        var e8 = Error.Failure("E1", "M");
        Assert.False(e7.StrictEquals(e8));
        Assert.False(e8.StrictEquals(e7));

        var e9 = Error.Failure("E1", "M").WithMetadata("k", "v");
        var e10 = Error.Failure("E1", "M").WithMetadata("k", "v2");
        Assert.False(e9.StrictEquals(e10));
        Assert.False(e9.StrictEquals(e8));

        var e11 = Error.Failure("E1", "M", Error.Failure("E2", "M3"));
        Assert.False(e7.StrictEquals(e11));

        Assert.True(e7.StrictEquals(Error.Failure("E1", "M", Error.Failure("E2", "M2"))));
        Assert.False(e1.StrictEquals(null));

        Assert.True(e1 == Error.Failure("E1", "M").WithRetryability(ErrorRetryability.Permanent));
        Assert.False(e1 != Error.Failure("E1", "M").WithRetryability(ErrorRetryability.Permanent));
        
        Error? n1 = null;
        Error? n2 = null;
        Assert.True(n1 == n2);
        Assert.False(n1 != n2);
        Assert.False(e1 == n2);

        Assert.False(e1.Equals(null));
        Assert.False(e1.Equals(new object()));
    }

    [Fact]
    public void ErrorBuilder_Comprehensive()
    {
        var builder = Error.Create("E1", "M");
        var e = builder.WithType(ErrorType.Validation)
                       .WithSeverity(ErrorSeverity.Critical)
                       .WithRetryability(ErrorRetryability.Permanent)
                       .WithDescriptionKey("key")
                       .WithTraceId("t1")
                       .WithCorrelationId("c1")
                       .WithInnerError(Error.Failure("Inner", "M"))
                       .WithInnerError(Error.Failure("Inner2", "M"))
                       .WithMetadata("k", "v")
                       .WithMetadata(new Dictionary<string, object> { { "k2", "v2" } })
                       .Build();

        Assert.Equal(ErrorType.Validation, e.Type);
        Assert.Equal(ErrorSeverity.Critical, e.Severity);
        Assert.Equal(ErrorRetryability.Permanent, e.Retryability);
        Assert.Equal("key", e.DescriptionKey);
        Assert.Equal("t1", e.TraceId);
        Assert.Equal("c1", e.CorrelationId);
        Assert.Equal(2, e.InnerErrors.Length);
        Assert.Equal("v", e.Metadata["k"]);
        Assert.Equal("v2", e.Metadata["k2"]);

        var e2 = e.ToBuilder().Build();
        Assert.True(e.StrictEquals(e2));
    }

    [Fact]
    public void Coverage_MoreCoreEdgeCases()
    {
        // StrictEquals Coverage
        var eBase = Error.Failure("A", "B");
        var e1 = eBase.WithCorrelationId("c1");
        var e2 = eBase.WithCorrelationId("c2");
        Assert.False(e1.StrictEquals(e2));

        var e3 = eBase.WithTraceId(System.Diagnostics.ActivityTraceId.CreateRandom());
        var e4 = eBase.WithTraceId(System.Diagnostics.ActivityTraceId.CreateRandom());
        Assert.False(e3.StrictEquals(e4));

        var e5 = eBase.WithDescriptionKey("dk1");
        var e6 = eBase.WithDescriptionKey("dk2");
        Assert.False(e5.StrictEquals(e6));

        // ErrorEqualityComparer null metadata value hash code
        var eNullMeta = eBase.WithMetadata("key", null!);
        var hash = ErrorEqualityComparer.Strict.GetHashCode(eNullMeta);
        Assert.NotEqual(0, hash); // Should not throw

        // ErrorBuilder Build traceId override
        var e7 = eBase.WithTraceId("custom_string");
        var e8 = e7.ToBuilder().Build();
        Assert.Equal("custom_string", e8.TraceId);

        // ErrorBuilder Build with ActivityTraceId (covers _traceId is null && _traceIdValue.HasValue)
        var actTraceId = System.Diagnostics.ActivityTraceId.CreateRandom();
        var eTraceIdStruct = Error.Failure("A", "B").WithTraceId(actTraceId);
        var eTraceIdStructBuilt = eTraceIdStruct.ToBuilder().Build();
        Assert.Equal(actTraceId.ToString(), eTraceIdStructBuilt.TraceId);

        var eAct1Custom = eBase.WithTraceId("custom");
        var eAct2Custom = eBase.WithTraceId("custom2");
        Assert.False(eAct1Custom.StrictEquals(eAct2Custom)); // Differs on _traceIdOverride
    }
}
