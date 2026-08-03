using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Xunit;
using EricksonLopez.Result.AspNetCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace EricksonLopez.Result.AspNetCore.Tests;

public class ResultEndpointFilterTests
{
    private class TestEndpointFilterInvocationContext : EndpointFilterInvocationContext
    {
        public override HttpContext HttpContext { get; }
        public override IList<object?> Arguments { get; }

        public TestEndpointFilterInvocationContext(HttpContext httpContext)
        {
            HttpContext = httpContext;
            Arguments = new List<object?>();
        }

        public override T GetArgument<T>(int index) => default!;
    }

    [Fact]
    public void Constructor_WithoutOptions_CreatesDefaultOptions()
    {
        var filter = new ResultEndpointFilter(options: (ResultHttpOptions?)null);
        Assert.NotNull(filter);
    }

    [Fact]
    public void Constructor_WithIOptions_UsesValue()
    {
        var options = new ResultHttpOptions();
        var filter = new ResultEndpointFilter(Options.Create(options));
        Assert.NotNull(filter);
    }

    [Fact]
    public void Constructor_WithNullIOptions_CreatesDefaultOptions()
    {
        var filter = new ResultEndpointFilter(options: (IOptions<ResultHttpOptions>?)null);
        Assert.NotNull(filter);
    }

    [Fact]
    public async Task InvokeAsync_NonResult_ReturnsOriginalValue()
    {
        var filter = new ResultEndpointFilter();
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());
        
        var originalResult = new object();
        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(originalResult));

        Assert.Same(originalResult, result);
    }

    [Fact]
    public async Task InvokeAsync_ResultSuccess_ReturnsNoContent()
    {
        var filter = new ResultEndpointFilter();
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());
        
        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(Result.Success()));

        Assert.IsType<NoContent>(result);
    }

    [Fact]
    public async Task InvokeAsync_ResultFailure_ReturnsProblemDetails()
    {
        // Pass IncludeDescription=true to test description forwarding; the secure
        // default is false (description replaced with "An error occurred.").
        var options = new ResultHttpOptions { IncludeDescription = true };
        var filter = new ResultEndpointFilter(options: options);
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());
        
        var error = Error.Validation("V", "M");
        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(Result.Failure(error)));

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(400, problem.StatusCode);
        Assert.Equal("Bad Request", problem.ProblemDetails.Title);
        Assert.Equal("M", problem.ProblemDetails.Detail);
        Assert.Equal("V", problem.ProblemDetails.Extensions["errorCode"]);
    }

    [Fact]
    public async Task InvokeAsync_ResultTSuccess_ReturnsOkWithValue()
    {
        var filter = new ResultEndpointFilter();
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());
        
        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(Result.Success("value")));

        var ok = Assert.IsType<Ok<object>>(result);
        Assert.Equal("value", ok.Value);
    }

    [Fact]
    public async Task InvokeAsync_ResultTFailure_ReturnsProblemDetails()
    {
        // Pass IncludeDescription=true to test description forwarding; the secure
        // default is false (description replaced with "An error occurred.").
        var options = new ResultHttpOptions { IncludeDescription = true };
        var filter = new ResultEndpointFilter(options: options);
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());
        
        var error = Error.NotFound("N", "M");
        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(Result.Failure<string>(error)));

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(404, problem.StatusCode);
        Assert.Equal("Not Found", problem.ProblemDetails.Title);
        Assert.Equal("M", problem.ProblemDetails.Detail);
        Assert.Equal("N", problem.ProblemDetails.Extensions["errorCode"]);
    }

    private class MockOutcome : IResultOutcome
    {
        public bool IsSuccess { get; set; }
        public bool IsFailure { get; set; }
        public bool IsUninitialized { get; set; }
        public Error? Error { get; set; }
        public object? RawValue { get; set; }
    }

    [Fact]
    public async Task InvokeAsync_IResultOutcome_IsFailure_WithoutError_ThrowsInvalidOperationException()
    {
        // This state is impossible through public APIs (Result.Failure requires a non-null Error),
        // but could theoretically occur via reflection or an external IResultOutcome implementation.
        // The filter must throw rather than silently return the raw object (which would produce
        // a 200 OK with the struct serialized as JSON body — a catastrophic failure mode).
        var filter = new ResultEndpointFilter(options: (ResultHttpOptions?)null);
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());
        var outcome = new MockOutcome { IsFailure = true, Error = null };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(outcome)).AsTask());

        Assert.Contains("IsFailure=true but Error is null", ex.Message);
        Assert.Contains("MockOutcome", ex.Message);
    }

    // ─── ARB Audit Regression Tests (Blocking Condition 1) ───────────────────────────────────────
    // These tests confirm that the bug where default(Result<T>) produced a "200 OK null" response
    // is resolved. The filter must throw InvalidOperationException for any Uninitialized result.

    [Fact]
    public async Task InvokeAsync_DefaultResultOfT_Uninitialized_ThrowsInvalidOperationException()
    {
        // Arrange
        // default(Result<string>) has IsSuccess=false, IsFailure=false, IsUninitialized=true.
        // Before the fix, this silently returned TypedResults.Ok(null) — a 200 OK with null body.
        // After the fix, this must throw InvalidOperationException to surface the programming error.
        var filter = new ResultEndpointFilter(options: (ResultHttpOptions?)null);
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());
        var uninitializedResult = default(Result<string>);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => filter.InvokeAsync(
                context,
                _ => ValueTask.FromResult<object?>(uninitializedResult)).AsTask());

        // Assert — message must identify the type and the uninitialized state
        Assert.Contains("Uninitialized", ex.Message);
        Assert.Contains("IsUninitialized=True", ex.Message);
    }

    [Fact]
    public async Task InvokeAsync_DefaultResult_NonGeneric_Uninitialized_ThrowsInvalidOperationException()
    {
        // Arrange
        // default(Result) has IsSuccess=false, IsFailure=false, IsUninitialized=true.
        // The non-generic Result path routes through a different branch in the filter
        // (the "if result is Result" branch), so this must be tested separately.
        var filter = new ResultEndpointFilter(options: (ResultHttpOptions?)null);
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());
        var uninitializedResult = default(Result);

        // Act & Assert
        // The non-generic Result path calls ToHttpResult() which itself guards against Uninitialized.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => filter.InvokeAsync(
                context,
                _ => ValueTask.FromResult<object?>(uninitializedResult)).AsTask());

        Assert.NotNull(ex);
    }

    [Fact]
    public async Task InvokeAsync_IResultOutcome_Uninitialized_ThrowsInvalidOperationException()
    {
        // Arrange
        // Verify that an external IResultOutcome implementation with IsUninitialized=true
        // also triggers the guard — not just Result<T> structs.
        var filter = new ResultEndpointFilter(options: (ResultHttpOptions?)null);
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());
        var outcome = new MockOutcome
        {
            IsSuccess = false,
            IsFailure = false,
            IsUninitialized = true,
            Error = null,
            RawValue = null
        };

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(outcome)).AsTask());

        // Assert
        Assert.Contains("Uninitialized", ex.Message);
        Assert.Contains("MockOutcome", ex.Message);
    }
}

