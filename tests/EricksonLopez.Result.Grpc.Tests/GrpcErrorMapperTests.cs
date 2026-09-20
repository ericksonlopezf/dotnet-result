// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Result.Grpc;
using Grpc.Core;
using Xunit;

namespace EricksonLopez.Result.Grpc.Tests;

public sealed class GrpcErrorMapperTests
{
    [Theory]
    [InlineData(ErrorType.Validation, StatusCode.InvalidArgument)]
    [InlineData(ErrorType.NotFound, StatusCode.NotFound)]
    [InlineData(ErrorType.Conflict, StatusCode.AlreadyExists)]
    [InlineData(ErrorType.Unauthorized, StatusCode.Unauthenticated)]
    [InlineData(ErrorType.Forbidden, StatusCode.PermissionDenied)]
    [InlineData(ErrorType.Unavailable, StatusCode.Unavailable)]
    [InlineData(ErrorType.Infrastructure, StatusCode.DataLoss)]
    [InlineData(ErrorType.Unexpected, StatusCode.Internal)]
    [InlineData(ErrorType.Failure, StatusCode.Internal)]
    public void ToStatusCode_WhenErrorTypeGiven_ReturnsExpectedStatusCode(ErrorType errorType, StatusCode expectedStatus)
    {
        var statusCode = errorType.ToStatusCode();

        Assert.Equal(expectedStatus, statusCode);
    }

    [Fact]
    public void ToRpcException_WhenErrorIsNull_ThrowsArgumentNullException()
    {
        Error nullError = null!;

        Assert.Throws<ArgumentNullException>(() => nullError.ToRpcException());
    }

    [Fact]
    public void ToRpcException_WhenErrorProvided_PopulatesStatusAndTrailers()
    {
        var metadata = new Dictionary<string, object>
        {
            { "orderid", "12345" }
        };
        var error = Error.Custom(
            code: "Order.InvalidStatus",
            description: "Cannot cancel order in shipped status.",
            type: ErrorType.Validation,
            severity: ErrorSeverity.Warning,
            retryability: ErrorRetryability.Permanent,
            metadata: metadata);

        var rpcException = error.ToRpcException();

        Assert.Equal(StatusCode.InvalidArgument, rpcException.StatusCode);
        Assert.Equal(error.Description, rpcException.Status.Detail);
        Assert.NotNull(rpcException.Trailers);
        Assert.Equal("Order.InvalidStatus", rpcException.Trailers.GetValue("error-code"));
        Assert.Equal("Validation", rpcException.Trailers.GetValue("error-type"));
        Assert.Equal("Warning", rpcException.Trailers.GetValue("error-severity"));
        Assert.Equal("Permanent", rpcException.Trailers.GetValue("error-retryability"));
        Assert.Equal("12345", rpcException.Trailers.GetValue("error-meta-orderid"));
    }

    [Fact]
    public void ToError_WhenRpcExceptionIsNull_ThrowsArgumentNullException()
    {
        RpcException nullException = null!;

        Assert.Throws<ArgumentNullException>(() => nullException.ToError());
    }

    [Fact]
    public void ToError_WhenRpcExceptionProvided_ReconstitutesErrorWithCodeAndType()
    {
        var trailers = new Metadata
        {
            { "error-code", "Catalog.ItemNotFound" },
            { "error-traceid", "0af7651916cd43dd8448eb211c80319c" }
        };
        var rpcException = new RpcException(new Status(StatusCode.NotFound, "Item 42 not found"), trailers);

        var error = rpcException.ToError();

        Assert.Equal("Catalog.ItemNotFound", error.Code);
        Assert.Equal("Item 42 not found", error.Description);
        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Equal("0af7651916cd43dd8448eb211c80319c", error.Metadata?["traceId"]);
    }

    [Fact]
    public void ToError_WhenRpcExceptionHasNoTrailers_FallsBackToStatusCode()
    {
        var rpcException = new RpcException(new Status(StatusCode.Unavailable, "Downstream timeout"));

        var error = rpcException.ToError();

        Assert.Equal("Unavailable", error.Code);
        Assert.Equal("Downstream timeout", error.Description);
        Assert.Equal(ErrorType.Unavailable, error.Type);
        Assert.Equal(ErrorRetryability.Transient, error.Retryability);
    }

    [Theory]
    [InlineData(StatusCode.InvalidArgument, ErrorType.Validation)]
    [InlineData(StatusCode.NotFound, ErrorType.NotFound)]
    [InlineData(StatusCode.AlreadyExists, ErrorType.Conflict)]
    [InlineData(StatusCode.Aborted, ErrorType.Conflict)]
    [InlineData(StatusCode.Unauthenticated, ErrorType.Unauthorized)]
    [InlineData(StatusCode.PermissionDenied, ErrorType.Forbidden)]
    [InlineData(StatusCode.Unavailable, ErrorType.Unavailable)]
    [InlineData(StatusCode.DataLoss, ErrorType.Infrastructure)]
    [InlineData(StatusCode.Internal, ErrorType.Unexpected)]
    [InlineData(StatusCode.Unknown, ErrorType.Unexpected)]
    public void ToError_MapsStatusCodesToExpectedErrorTypes(StatusCode statusCode, ErrorType expectedType)
    {
        var rpcException = new RpcException(new Status(statusCode, "Detail"));
        var error = rpcException.ToError();

        Assert.Equal(expectedType, error.Type);
    }

    [Fact]
    public void ToRpcException_HandlesTraceId_CorrelationId_AndFiltersInvalidMetadataKeys()
    {
        var error = Error.Custom("Test.Code", "Test desc", ErrorType.Conflict)
            .ToBuilder()
            .WithTraceId("trace-xyz-123")
            .WithCorrelationId("corr-abc-456")
            .WithMetadata("valid_key-1", "value1")
            .WithMetadata("INVALID KEY WITH SPACES!", "ignoredValue")
            .Build();

        var rpcException = error.ToRpcException();

        Assert.NotNull(rpcException.Trailers);
        Assert.Equal("trace-xyz-123", rpcException.Trailers.GetValue("error-traceid"));
        Assert.Equal("corr-abc-456", rpcException.Trailers.GetValue("error-correlationid"));
        Assert.Equal("value1", rpcException.Trailers.GetValue("error-meta-valid_key-1"));
        Assert.Null(rpcException.Trailers.GetValue("error-meta-invalid key with spaces!"));

        // Reconstitute back with ToError
        var roundTripped = rpcException.ToError();
        Assert.Equal("Test.Code", roundTripped.Code);
        Assert.Equal("Test desc", roundTripped.Description);
        Assert.Equal(ErrorType.Conflict, roundTripped.Type);
        Assert.Equal("trace-xyz-123", roundTripped.Metadata?["traceId"]);
        Assert.Equal("value1", roundTripped.Metadata?["valid_key-1"]);
    }
}
