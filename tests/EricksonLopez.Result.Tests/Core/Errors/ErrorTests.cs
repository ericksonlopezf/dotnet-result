// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

#nullable enable
namespace EricksonLopez.Result.Tests.Core;

public class ErrorTests
{
    [Fact]
    public void FactoryMethods_WhenInvoked_CreatesCorrectTypeAndSeverity()
    {

        var failure = Error.Failure("F1", "Failure description");
        Assert.Equal(ErrorType.Failure, failure.Type);
        Assert.Equal(ErrorSeverity.Error, failure.Severity);
        Assert.Equal(ErrorRetryability.NotApplicable, failure.Retryability);

        var validation = Error.Validation("V1", "Validation description");
        Assert.Equal(ErrorType.Validation, validation.Type);
        Assert.Equal(ErrorSeverity.Warning, validation.Severity);

        var unavailable = Error.Unavailable("U1", "Unavailable description");
        Assert.Equal(ErrorType.Unavailable, unavailable.Type);
        Assert.Equal(ErrorRetryability.Transient, unavailable.Retryability);

        var domain = Error.Domain("D1", "Domain description");
        Assert.Equal(ErrorType.Domain, domain.Type);

        var infra = Error.Infrastructure("I1", "Infra description");
        Assert.Equal(ErrorType.Infrastructure, infra.Type);
        Assert.Equal(ErrorRetryability.Transient, infra.Retryability);
    }

    [Theory]
    [InlineData(null, "Description")]
    [InlineData("", "Description")]
    [InlineData("   ", "Description")]
    public void FactoryMethods_NullOrEmptyCode_ThrowsArgumentException(string? code, string description)
    {
        Assert.ThrowsAny<ArgumentException>(() => Error.Failure(code!, description));
    }

    [Theory]
    [InlineData("Code", null)]
    [InlineData("Code", "")]
    [InlineData("Code", "   ")]
    public void FactoryMethods_NullOrEmptyDescription_ThrowsArgumentException(string code, string? description)
    {
        Assert.ThrowsAny<ArgumentException>(() => Error.Failure(code, description!));
    }

    [Fact]
    public void WithMethods_WhenInvoked_ReturnsNewInstancePreservingImmutability()
    {

        var error = Error.Failure("E1", "Message")
            .WithTraceId("trace-123")
            .WithCorrelationId("corr-456")
            .WithDescriptionKey("errors.e1")
            .WithMetadata("key1", "val1");

        Assert.Equal("trace-123", error.TraceId);
        Assert.Equal("corr-456", error.CorrelationId);
        Assert.Equal("errors.e1", error.DescriptionKey);
        Assert.True(error.HasMetadata);
        Assert.Equal("val1", error.Metadata["key1"]);
    }

    [Fact]
    public void StrictEquals_ComparesMetadataAndAttributes()
    {
        var e1 = Error.Failure("E1", "Message").WithTraceId("t1");
        var e2 = Error.Failure("E1", "Message").WithTraceId("t1");
        var e3 = Error.Failure("E1", "Message").WithTraceId("t2");

        Assert.True(e1.Equals(e2)); // Semantic equality
        Assert.True(e1.Equals(e3)); // Semantic equality ignores traceId

        Assert.True(e1.StrictEquals(e2));  // Strict equality checks traceId
        Assert.False(e1.StrictEquals(e3)); // Strict equality fails on different traceId
    }

    [Fact]
    public void Constructor_EmptyInnerErrors_SetsNull()
    {
        var error = Error.Create("C", "D").WithInnerErrors(Array.Empty<Error>()).Build();
        Assert.False(error.HasInnerErrors);
        Assert.Empty(error.InnerErrors);
    }

    [Fact]
    public void WithMetadata_IEnumerable_AddsEntries()
    {
        var error = Error.Failure("C", "D");
        var metadata = new List<KeyValuePair<string, object>>
        {
            new("key1", "val1"),
            new("key2", "val2")
        };
        var newError = error.WithMetadata(metadata);
        Assert.True(newError.HasMetadata);
        Assert.Equal("val1", newError.Metadata["key1"]);
        Assert.Equal("val2", newError.Metadata["key2"]);
    }

    [Fact]
    public void WithMetadata_IEnumerable_Null_Throws()
    {
        var error = Error.Failure("C", "D");
        Assert.Throws<ArgumentNullException>(() => error.WithMetadata((IEnumerable<KeyValuePair<string, object>>)null!));
    }

    [Fact]
    public void WithMetadata_IEnumerable_Empty_ReturnsSame()
    {
        var error = Error.Failure("C", "D");
        var metadata = new List<KeyValuePair<string, object>>();
        var newError = error.WithMetadata(metadata);
        Assert.Same(error, newError);
    }

    [Fact]
    public void WithMetadata_IEnumerable_Existing_AddsEntries()
    {
        var error = Error.Failure("C", "D").WithMetadata("key0", "val0");
        var metadata = new List<KeyValuePair<string, object>>
        {
            new("key1", "val1")
        };
        var newError = error.WithMetadata(metadata);
        Assert.Equal("val0", newError.Metadata["key0"]);
        Assert.Equal("val1", newError.Metadata["key1"]);
    }

    [Fact]
    public void WithMetadata_IReadOnlyDictionary_Null_Throws()
    {
        var error = Error.Failure("C", "D");
        Assert.Throws<ArgumentNullException>(() => error.WithMetadata((IReadOnlyDictionary<string, object>)null!));
    }

    [Fact]
    public void WithMetadata_IReadOnlyDictionary_Existing_AddsEntries()
    {
        var error = Error.Failure("C", "D").WithMetadata("key0", "val0");
        var metadata = new Dictionary<string, object>
        {
            { "key1", "val1" }
        };
        var newError = error.WithMetadata((IReadOnlyDictionary<string, object>)metadata);
        Assert.Equal("val0", newError.Metadata["key0"]);
        Assert.Equal("val1", newError.Metadata["key1"]);
    }


    // Note: DerivedError test class removed — Error is sealed. Subclassing is no longer possible.
    // Extensibility is achieved via Error.Create(...).WithMetadata(...).Build() or domain factory methods.

    [Fact]
    public void Equals_NullOrSelf_HandledCorrectly()
    {
        var err = Error.Failure("C", "D");
        // Two errors with same code/description/type/severity are equal
        var sameErr = Error.Failure("C", "D");
        Assert.True(err.Equals(sameErr));
        Assert.False(err.Equals(null));
        Assert.True(err.Equals(err));
        var differentCode = Error.Failure("X", "D");
        Assert.False(err.Equals(differentCode));
    }

    [Fact]
    public void GetHashCode_DifferentProperties_DifferentHash()
    {
        var e1 = Error.Failure("C1", "D1");
        var e2 = Error.Failure("C2", "D1");
        var e3 = Error.Failure("C1", "D2");
        var e4 = Error.Validation("C1", "D1");
        var e5 = Error.Create("C1", "D1").WithSeverity(ErrorSeverity.Warning).Build();

        var hashes = new HashSet<int>
        {
            e1.GetHashCode(),
            e2.GetHashCode(),
            e3.GetHashCode(),
            e4.GetHashCode(),
            e5.GetHashCode()
        };
        Assert.Equal(5, hashes.Count);
    }

    [Fact]
    public void StrictEquals_NotEquals_ReturnsFalse()
    {
        var e1 = Error.Failure("C1", "D1");
        var e2 = Error.Failure("C2", "D2");
        Assert.False(e1.StrictEquals(e2));
    }

    [Fact]
    public void StrictEquals_DifferentInnerErrorsCount_ReturnsFalse()
    {
        var e1 = Error.Failure("C", "D").WithMetadata("a", "1");
        var e2 = Error.Failure("C", "D", Error.Failure("A", "B")).WithMetadata("a", "1");
        Assert.False(e1.StrictEquals(e2));
        Assert.False(e2.StrictEquals(e1));

        var e3 = Error.Failure("C", "D", Error.Failure("A", "B"));
        var e4 = Error.Failure("C", "D", Error.Failure("X", "Y"));
        Assert.False(e3.StrictEquals(e4));

        var e5 = Error.Failure("C", "D", Error.Failure("A", "B"), Error.Failure("A", "B"));
        Assert.False(e3.StrictEquals(e5));
    }

    [Fact]
    public void StrictEquals_DifferentMetadataCount_ReturnsFalse()
    {
        var e1 = Error.Failure("C", "D").WithMetadata("k1", "v1");
        var e2 = Error.Failure("C", "D").WithMetadata("k1", "v1").WithMetadata("k2", "v2");
        Assert.False(e1.StrictEquals(e2));

        var e3 = Error.Failure("C", "D").WithMetadata("kx", "v1");
        Assert.False(e1.StrictEquals(e3));
    }

    [Fact]
    public void Constructor_EmptyInnerErrors_InnerErrorsIsEmpty()
    {
        var e = Error.Create("C", "D").WithInnerErrors(Array.Empty<Error>()).Build();
        Assert.False(e.HasInnerErrors);
        Assert.Empty(e.InnerErrors);
    }

    [Fact]
    public void Constructor_EmptyMetadata_MetadataIsEmpty()
    {
        var e = Error.Create("C", "D").WithMetadata(new Dictionary<string, object>()).Build();
        Assert.False(e.HasMetadata);
        Assert.Empty(e.Metadata);
    }

    [Fact]
    public void EqualityOperators_Behavior()
    {
        var e1 = Error.Failure("C", "D");
        var e2 = Error.Failure("C", "D");
        var e3 = Error.Failure("X", "Y");
        Error? nullError1 = null;
        Error? nullError2 = null;

        Assert.True(e1 == e2);
        Assert.False(e1 != e2);

        Assert.True(e1 != e3);
        Assert.False(e1 == e3);

        Assert.True(nullError1 == nullError2);
        Assert.False(nullError1 != nullError2);

        Assert.True(e1 != nullError1);
        Assert.False(e1 == nullError1);

        Assert.True(nullError1 != e1);
        Assert.False(nullError1 == e1);
    }

    [Fact]
    public void WithMetadata_EmptyKey_ThrowsArgumentException()
    {
        var e = Error.Failure("C", "D");
        Assert.Throws<ArgumentException>(() => e.WithMetadata("", "value"));
    }

    [Fact]
    public void WithMetadata_ExistingMetadata_CreatesNewCopy()
    {
        var e1 = Error.Failure("C", "D").WithMetadata("A", 1);
        var e2 = e1.WithMetadata("B", 2);

        Assert.True(e1.HasMetadata);
        Assert.Single(e1.Metadata);

        Assert.True(e2.HasMetadata);
        Assert.Equal(2, e2.Metadata.Count);
    }

    [Fact]
    public void TraceId_NotCapturedIfOverriden()
    {
        var e = Error.Create("C", "D").WithTraceId("custom-trace").Build();
        Assert.Equal("custom-trace", e.TraceId);
    }

    [Fact]
    public void TraceId_ReturnsNullIfNoOverrideAndNoActivity()
    {
        var e = Error.Failure("C", "D");
        Assert.Null(e.TraceId);
    }

    [Fact]
    public void TraceId_ReturnsActivityTraceIdIfNoOverride()
    {
        using var activity = new System.Diagnostics.Activity("TestActivity");
        activity.Start();

        var e = Error.Failure("C", "D");

        Assert.NotNull(e.TraceId);
        Assert.Equal(activity.TraceId.ToString(), e.TraceId);
    }

    [Theory]
    [InlineData(null, "Description")]
    [InlineData("", "Description")]
    [InlineData("   ", "Description")]
    public void ErrorCreate_NullOrEmptyCode_ThrowsArgumentException(string? code, string description)
    {
        Assert.ThrowsAny<ArgumentException>(() => Error.Create(code!, description));
    }

    [Theory]
    [InlineData("Code", null)]
    [InlineData("Code", "")]
    [InlineData("Code", "   ")]
    public void ErrorCreate_NullOrEmptyDescription_ThrowsArgumentException(string code, string? description)
    {
        Assert.ThrowsAny<ArgumentException>(() => Error.Create(code, description!));
    }

    [Fact]
    public void ToBuilder_WithNullTraceId_BuildsCorrectly()
    {
        var source = Error.Failure("C", "D");
        var builder = source.ToBuilder();
        var result = builder.Build();

        Assert.Null(result.TraceId);
    }

    [Fact]
    public void ToBuilder_WithActivityTraceId_BuildsCorrectly()
    {
        using var activity = new System.Diagnostics.Activity("TestActivity");
        activity.Start();

        var source = Error.Failure("C", "D");
        var builder = source.ToBuilder();
        var result = builder.Build();

        Assert.Equal(activity.TraceId.ToString(), result.TraceId);
    }

    [Fact]
    public void TryGetMetadata_ReturnsFalse_WhenMetadataIsNotNullButKeyNotFound()
    {
        var e = Error.Failure("A", "B").WithMetadata("key1", "val1");
        var result = e.TryGetMetadata<string>("key2", out var val);
        Assert.False(result);
        Assert.Null(val);
    }

    [Fact]
    public void TryGetMetadata_ThrowsArgumentException_WhenKeyIsNullOrWhitespace()
    {
        var e = Error.Failure("A", "B").WithMetadata("key", "val");
        Assert.Throws<ArgumentException>(() => e.TryGetMetadata<string>("", out _));
        Assert.Throws<ArgumentNullException>(() => e.TryGetMetadata<string>(null!, out _));
    }

    [Fact]
    public void TryGetMetadata_ReturnsFalse_WhenMetadataIsMissing()
    {
        var e = Error.Failure("A", "B");
        var result = e.TryGetMetadata<string>("key", out var val);
        Assert.False(result);
        Assert.Null(val);
    }

    [Fact]
    public void TryGetMetadata_ReturnsTrue_AndSetsValue_WhenTypeMatches()
    {
        var e = Error.Failure("A", "B").WithMetadata("key", 123);
        var result = e.TryGetMetadata<int>("key", out var val);
        Assert.True(result);
        Assert.Equal(123, val);
    }

    [Fact]
    public void TryGetMetadata_ThrowsInvalidCastException_WhenTypeDoesNotMatch()
    {
        var e = Error.Failure("A", "B").WithMetadata("key", 123L);
        var ex = Assert.Throws<InvalidCastException>(() => e.TryGetMetadata<int>("key", out _));
        Assert.Contains("long", ex.Message);
        Assert.Contains("key", ex.Message);
        Assert.Contains("If this Error was deserialized from JSON, numeric types are narrowed to 'long'/'double'", ex.Message);
        Assert.Contains("complex types (Guid, DateTime) are narrowed to 'string'", ex.Message);
        Assert.Contains("Use the narrowed type in TryGetMetadata<T>, or avoid type-specific casts after deserialization.", ex.Message);
    }

    [Fact]
    public void GetMetadata_ReturnsValue_WhenTypeMatches()
    {
        var e = Error.Failure("A", "B").WithMetadata("key", 123);
        var val = e.GetMetadata<int>("key");
        Assert.Equal(123, val);
    }

    [Fact]
    public void GetMetadata_ThrowsKeyNotFoundException_WhenKeyMissing()
    {
        var e = Error.Failure("A", "B").WithMetadata("key", 123);
        Assert.Throws<KeyNotFoundException>(() => e.GetMetadata<int>("key2"));
    }

    [Fact]
    public void GetMetadata_ThrowsArgumentException_WhenKeyIsNullOrWhitespace()
    {
        var e = Error.Failure("A", "B").WithMetadata("key", 123);
        Assert.Throws<ArgumentException>(() => e.GetMetadata<int>(""));
        Assert.Throws<ArgumentNullException>(() => e.GetMetadata<int>(null!));
    }

    [Fact]
    public void Constructor_WithEmptyInnerErrorsAndMetadata_SetsEmptyAndNull()
    {
        var e = new Error("C", "M", innerErrors: [], metadata: new Dictionary<string, object>());
        Assert.False(e.HasInnerErrors);
        Assert.Empty(e.InnerErrors);
        Assert.False(e.HasMetadata);
        Assert.Empty(e.Metadata);
        Assert.Null(e.RawMetadata);
    }

    [Fact]
    public void Builder_EmptyInnerErrors_UsesEmptyArray()
    {
        var builder = Error.Create("CODE", "DESC");
        var error = builder.Build();
        Assert.False(error.HasInnerErrors);
        Assert.Empty(error.InnerErrors);
    }

    [Fact]
    public void Builder_WithInnerErrors_UsesArray()
    {
        var builder = Error.Create("CODE", "DESC").WithInnerError(Error.Failure("A", "B"));
        var error = builder.Build();
        Assert.True(error.HasInnerErrors);
        Assert.Single(error.InnerErrors);
    }

    [Fact]
    public void CreateFromBuilder_EmptyMetadata_UsesNull()
    {
        var meta = System.Collections.Immutable.ImmutableDictionary<string, object>.Empty;
        var error = Error.CreateFromBuilder("C", "D", ErrorType.Failure, ErrorSeverity.Error, ErrorRetryability.NotApplicable, null, null, null, default, meta);
        Assert.False(error.HasMetadata);
        Assert.Empty(error.Metadata);
        Assert.Null(error.RawMetadata);
    }

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
    public void TryGetMetadata_NullValue_ReturnsFalse()
    {
        var error = Error.Failure("A", "B").WithMetadata("key", null!);

        Assert.False(error.TryGetMetadata<string>("key", out var val));
        Assert.Null(val);
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
    public void ErrorBuilder_WithTraceIdActivity_SetsCorrectly()
    {
        var traceId = System.Diagnostics.ActivityTraceId.CreateRandom();
        var b = Error.Create("C", "D").WithTraceId(traceId).Build();
        b.TraceId.Should().Be(traceId.ToString());
    }

    [Fact]
    public void FactoryMethods_AllStandardVariants_ProduceExpectedContract()
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

        var custom1 = Error.Custom("C1", "M", ErrorType.Custom, ErrorSeverity.Info, ErrorRetryability.Transient, "desc", "trace", "corr", [eFailureInner], new Dictionary<string, object> { { "k", "v" } });
        Assert.Equal(ErrorType.Custom, custom1.Type);
        Assert.Equal(ErrorSeverity.Info, custom1.Severity);
        Assert.Equal(ErrorRetryability.Transient, custom1.Retryability);
        Assert.Equal("desc", custom1.DescriptionKey);
        Assert.Equal("trace", custom1.TraceId);
        Assert.Equal("corr", custom1.CorrelationId);
        Assert.Single(custom1.InnerErrors);
        Assert.True(custom1.HasMetadata);
        Assert.Equal("v", custom1.Metadata["k"]);

        var custom2 = Error.Custom("C2", "M", ErrorType.Domain, ErrorSeverity.Warning, ErrorRetryability.Permanent, new Dictionary<string, object> { { "k2", "v2" } }, eFailureInner);
        Assert.Equal(ErrorType.Domain, custom2.Type);
        Assert.Single(custom2.InnerErrors);
        Assert.Equal("v2", custom2.Metadata["k2"]);
    }

    [Fact]
    public void FluentMethods_WhenInvoked_ReturnsExpectedMutatedInstance()
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
    public void StrictEquals_WhenDifferentProperties_ReturnsFalse()
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
    public void ErrorBuilder_WhenAllPropertiesSet_BuildsCorrectly()
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
    public void StrictEquals_AndBuilder_EdgeCases_BehaveCorrectly()
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

    [Fact]
    public void TraceId_WhenConfiguredViaBuilder_InitializesCorrectly()
    {
        var e1 = Error.Failure("code", "msg");
        Assert.Null(e1.TraceId);

        var e2 = Error.Create("code", "msg").WithTraceId(Guid.NewGuid().ToString()).Build();
        Assert.NotNull(e2.TraceId);
    }

    [Fact]
    public void Failure_WithEmptyCode_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Error.Failure("", "msg"));
    }

    [Fact]
    public void Constructor_ImmutableArrayFastPath_ReusesArrayInstance()
    {
        var inners = System.Collections.Immutable.ImmutableArray.Create(Error.Failure("I1", "D1"));
        var err = new Error("C", "D", innerErrors: inners);
        Assert.True(err.HasInnerErrors);
        Assert.Single(err.InnerErrors);
        Assert.Equal("I1", err.InnerErrors[0].Code);
    }

    [Fact]
    public void Constructor_ListInnerErrors_CreatesImmutableCopy()
    {
        var list = new List<Error> { Error.Failure("I1", "D1"), Error.Failure("I2", "D2") };
        var err = new Error("C", "D", innerErrors: list);
        Assert.Equal(2, err.InnerErrors.Length);
        Assert.Equal("I1", err.InnerErrors[0].Code);
        Assert.Equal("I2", err.InnerErrors[1].Code);
    }

    [Fact]
    public void CreateSentinel_ConstructsValidSentinelWithoutTraceId()
    {
        using var act = new System.Diagnostics.Activity("Ambient");
        act.Start();

        var sentinel = Error.CreateSentinel("SENTINEL", "Sentinel description", ErrorType.Failure, ErrorSeverity.Error, ErrorRetryability.Permanent);

        Assert.Equal("SENTINEL", sentinel.Code);
        Assert.Equal("Sentinel description", sentinel.Description);
        Assert.Equal(ErrorType.Failure, sentinel.Type);
        Assert.Equal(ErrorSeverity.Error, sentinel.Severity);
        Assert.Equal(ErrorRetryability.Permanent, sentinel.Retryability);
        Assert.Null(sentinel.TraceId);
        Assert.Null(sentinel.CorrelationId);
        Assert.Null(sentinel.DescriptionKey);
        Assert.False(sentinel.HasInnerErrors);
        Assert.False(sentinel.HasMetadata);
    }

    [Fact]
    public void CreateFromBuilder_DefaultInnerErrors_SetsEmpty()
    {
        var err = Error.CreateFromBuilder(
            "C", "D", ErrorType.Failure, ErrorSeverity.Error, ErrorRetryability.NotApplicable,
            null, null, null,
            default(System.Collections.Immutable.ImmutableArray<Error>),
            null);

        Assert.False(err.HasInnerErrors);
        Assert.Empty(err.InnerErrors);
    }

    [Fact]
    public void TraceId_WhenAccessedMultipleTimes_CachesMaterializedValue()
    {
        using var act = new System.Diagnostics.Activity("TraceTest");
        act.Start();

        var err = Error.Failure("C", "D");

        // First access: materializes and caches via Interlocked.CompareExchange
        var traceId1 = err.TraceId;
        Assert.NotNull(traceId1);
        Assert.Equal(act.TraceId.ToString(), traceId1);

        // Second access: hits the fast path cache (_cachedTraceIdString is not null)
        var traceId2 = err.TraceId;
        Assert.Same(traceId1, traceId2);
    }

    [Fact]
    public void Equals_WhenSemanticPropertiesVary_DistinguishesCorrectly()
    {
        var baseErr = Error.Custom("CODE", "DESC", ErrorType.Validation, ErrorSeverity.Warning, ErrorRetryability.Transient);

        // Matching
        var match = Error.Custom("CODE", "DESC", ErrorType.Validation, ErrorSeverity.Warning, ErrorRetryability.Transient);
        Assert.True(baseErr.Equals(match));

        // Different Code
        var diffCode = Error.Custom("DIFF", "DESC", ErrorType.Validation, ErrorSeverity.Warning, ErrorRetryability.Transient);
        Assert.False(baseErr.Equals(diffCode));

        // Different Description
        var diffDesc = Error.Custom("CODE", "DIFF", ErrorType.Validation, ErrorSeverity.Warning, ErrorRetryability.Transient);
        Assert.False(baseErr.Equals(diffDesc));

        // Different Type
        var diffType = Error.Custom("CODE", "DESC", ErrorType.Domain, ErrorSeverity.Warning, ErrorRetryability.Transient);
        Assert.False(baseErr.Equals(diffType));

        // Different Severity
        var diffSev = Error.Custom("CODE", "DESC", ErrorType.Validation, ErrorSeverity.Critical, ErrorRetryability.Transient);
        Assert.False(baseErr.Equals(diffSev));

        // Different Retryability
        var diffRetry = Error.Custom("CODE", "DESC", ErrorType.Validation, ErrorSeverity.Warning, ErrorRetryability.Permanent);
        Assert.False(baseErr.Equals(diffRetry));
    }

    [Fact]
    public void StrictEquals_WhenContextPropertiesVary_DistinguishesCorrectly()
    {
        var trace1 = System.Diagnostics.ActivityTraceId.CreateRandom();
        var trace2 = System.Diagnostics.ActivityTraceId.CreateRandom();

        var baseErr = Error.Custom("CODE", "DESC", ErrorType.Failure, ErrorSeverity.Error, ErrorRetryability.NotApplicable,
            descriptionKey: "KEY1", traceId: "TRACE_OVERRIDE_1", correlationId: "CORR1",
            innerErrors: [Error.Failure("I1", "D1")],
            metadata: new Dictionary<string, object> { { "k1", "v1" } });

        // Identical
        var same = Error.Custom("CODE", "DESC", ErrorType.Failure, ErrorSeverity.Error, ErrorRetryability.NotApplicable,
            descriptionKey: "KEY1", traceId: "TRACE_OVERRIDE_1", correlationId: "CORR1",
            innerErrors: [Error.Failure("I1", "D1")],
            metadata: new Dictionary<string, object> { { "k1", "v1" } });
        Assert.True(baseErr.StrictEquals(same));

        // Different DescriptionKey
        var diffKey = baseErr.WithDescriptionKey("KEY2");
        Assert.False(baseErr.StrictEquals(diffKey));

        // Different TraceIdOverride
        var diffTrace = baseErr.WithTraceId("TRACE_OVERRIDE_2");
        Assert.False(baseErr.StrictEquals(diffTrace));

        // Different CorrelationId
        var diffCorr = baseErr.WithCorrelationId("CORR2");
        Assert.False(baseErr.StrictEquals(diffCorr));

        // Struct TraceId difference (both have struct, but different)
        var structTraceErr1 = Error.Failure("A", "B").WithTraceId(trace1);
        var structTraceErr2 = Error.Failure("A", "B").WithTraceId(trace2);
        Assert.False(structTraceErr1.StrictEquals(structTraceErr2));

        // Struct TraceId difference (one has struct, other has null struct, both have null override)
        var noTraceErr = Error.Failure("A", "B").ClearTraceId();
        Assert.False(structTraceErr1.StrictEquals(noTraceErr));
        Assert.False(noTraceErr.StrictEquals(structTraceErr1));

        // Struct vs Override TraceId difference
        var mixedTraceErr = Error.Failure("A", "B").WithTraceId("custom");
        Assert.False(structTraceErr1.StrictEquals(mixedTraceErr));
        Assert.False(mixedTraceErr.StrictEquals(structTraceErr1));

        // Different Metadata value
        var diffMetaVal = baseErr.WithMetadata("k1", "diff_val");
        Assert.False(baseErr.StrictEquals(diffMetaVal));

        // Different Metadata key
        var diffMetaKey = Error.Failure("CODE", "DESC").WithMetadata("k1", "v1");
        var diffMetaKey2 = Error.Failure("CODE", "DESC").WithMetadata("k2", "v1");
        Assert.False(diffMetaKey.StrictEquals(diffMetaKey2));

        // Inner errors item difference
        var diffInnerItem = Error.Failure("CODE", "DESC", Error.Failure("I1", "D1"));
        var diffInnerItem2 = Error.Failure("CODE", "DESC", Error.Failure("I1", "DIFFERENT"));
        Assert.False(diffInnerItem.StrictEquals(diffInnerItem2));
    }
}


