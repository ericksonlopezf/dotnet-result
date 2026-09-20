// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result.Grpc;
using Grpc.Core;
using Xunit;

namespace EricksonLopez.Result.Grpc.Tests;

public sealed class ResultServerInterceptorTests
{
    private readonly ResultServerInterceptor _interceptor = new();
    private readonly ServerCallContext _context = TestServerCallContext.Create();

    [Fact]
    public async Task UnaryServerHandler_WhenContinuationSucceeds_ReturnsResponse()
    {
        var request = "hello";
        var expectedResponse = "world";

        var actualResponse = await _interceptor.UnaryServerHandler(
            request,
            _context,
            (req, ctx) => Task.FromResult(expectedResponse));

        Assert.Equal(expectedResponse, actualResponse);
    }

    [Fact]
    public async Task UnaryServerHandler_WhenContextIsNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _interceptor.UnaryServerHandler<string, string>("req", null!, (r, c) => Task.FromResult("res")));
    }

    [Fact]
    public async Task UnaryServerHandler_WhenContinuationIsNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _interceptor.UnaryServerHandler<string, string>("req", _context, null!));
    }

    [Fact]
    public async Task UnaryServerHandler_WhenContinuationReturnsFailedOutcome_ThrowsRpcExceptionWithTrailers()
    {
        var request = "user-123";
        var error = Error.NotFound("User.NotFound", "User does not exist");
        var failedResponse = new TestFailureResponse(error);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            _interceptor.UnaryServerHandler(
                request,
                _context,
                (req, ctx) => Task.FromResult(failedResponse)));

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
        Assert.Equal("User.NotFound", ex.Trailers.GetValue("error-code"));
    }

    [Fact]
    public async Task UnaryServerHandler_WhenContinuationReturnsSuccessfulOutcome_ReturnsResponse()
    {
        var request = "user-123";
        var successResponse = new TestSuccessResponse();

        var actual = await _interceptor.UnaryServerHandler(
            request,
            _context,
            (req, ctx) => Task.FromResult(successResponse));

        Assert.Same(successResponse, actual);
    }

    [Fact]
    public async Task UnaryServerHandler_WhenContinuationThrowsUnhandledException_ThrowsRpcExceptionWithInternalStatus()
    {
        var request = "user-123";

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            _interceptor.UnaryServerHandler<string, string>(
                request,
                _context,
                (req, ctx) => throw new InvalidOperationException("Database crashed")));

        Assert.Equal(StatusCode.Internal, ex.StatusCode);
        Assert.Equal("Rpc.UnhandledException", ex.Trailers.GetValue("error-code"));
    }

    [Fact]
    public async Task UnaryServerHandler_WhenContinuationThrowsRpcException_RethrowsDirectly()
    {
        var expected = new RpcException(new Status(StatusCode.PermissionDenied, "Forbidden"));

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            _interceptor.UnaryServerHandler<string, string>(
                "test",
                _context,
                (req, ctx) => throw expected));

        Assert.Same(expected, ex);
    }

    [Fact]
    public async Task UnaryServerHandler_WhenContinuationThrowsOperationCanceledException_RethrowsDirectly()
    {
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _interceptor.UnaryServerHandler<string, string>(
                "test",
                _context,
                (req, ctx) => throw new OperationCanceledException()));
    }

    private sealed class TestFailureResponse : IResultOutcome
    {
        public TestFailureResponse(Error error)
        {
            Error = error;
        }

        public bool IsSuccess => false;
        public bool IsFailure => true;
        public bool IsUninitialized => false;
        public Error? Error { get; }
        public object? RawValue => null;
    }

    private sealed class TestSuccessResponse : IResultOutcome
    {
        public bool IsSuccess => true;
        public bool IsFailure => false;
        public bool IsUninitialized => false;
        public Error? Error => null;
        public object? RawValue => "ok";
    }

    private sealed class TestServerCallContext : ServerCallContext
    {
        private readonly Metadata _responseTrailers = new();
        private readonly Metadata _requestHeaders = new();

        public static TestServerCallContext Create() => new();

        protected override string MethodCore => "/TestService/TestMethod";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "127.0.0.1";
        protected override DateTime DeadlineCore => DateTime.MaxValue;
        protected override Metadata RequestHeadersCore => _requestHeaders;
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override Metadata ResponseTrailersCore => _responseTrailers;
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore => new(null, new Dictionary<string, List<AuthProperty>>());
        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) => throw new NotImplementedException();
        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
    }
}
