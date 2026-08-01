#nullable enable
using System;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ErrorTests
{
    [Fact]
    public void FactoryMethods_CreateCorrectTypeAndSeverity()
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
    public void FluentBuilders_WorkImmutably()
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
        var e = Error.Create("C", "D").WithMetadata(new System.Collections.Generic.Dictionary<string, object>()).Build();
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
        var e = new Error("C", "M", innerErrors: [], metadata: new System.Collections.Generic.Dictionary<string, object>());
        Assert.Empty(e.InnerErrors);
    }

    [Fact]
    public void Builder_EmptyInnerErrors_UsesEmptyArray()
    {
        var builder = Error.Create("CODE", "DESC");
        var error = builder.Build();
        Assert.Empty(error.InnerErrors);
    }

    [Fact]
    public void Builder_WithInnerErrors_UsesArray()
    {
        var builder = Error.Create("CODE", "DESC").WithInnerError(Error.Failure("A", "B"));
        var error = builder.Build();
        Assert.Single(error.InnerErrors);
    }

    [Fact]
    public void CreateFromBuilder_EmptyMetadata_UsesNull()
    {
        var meta = System.Collections.Immutable.ImmutableDictionary<string, object>.Empty;
        var error = Error.CreateFromBuilder("C", "D", ErrorType.Failure, ErrorSeverity.Error, ErrorRetryability.NotApplicable, null, null, null, default, meta);
        Assert.False(error.HasMetadata);
    }
}
