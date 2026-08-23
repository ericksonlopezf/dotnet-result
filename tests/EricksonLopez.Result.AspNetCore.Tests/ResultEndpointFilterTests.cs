// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Xunit;

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
    public void Constructor_WhenWithoutOptions_CreatesDefaultOptions()
    {
        var filter = new ResultEndpointFilter(options: (ResultHttpOptions?)null);
        filter.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WhenWithIOptions_UsesValue()
    {
        var options = new ResultHttpOptions();
        var filter = new ResultEndpointFilter(Options.Create(options));
        filter.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WhenWithNullIOptions_CreatesDefaultOptions()
    {
        var filter = new ResultEndpointFilter(options: (IOptions<ResultHttpOptions>?)null);
        filter.Should().NotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_WhenNonResult_ReturnsOriginalValue()
    {
        var filter = new ResultEndpointFilter();
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());

        var originalResult = new object();
        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(originalResult));

        result.Should().BeSameAs(originalResult);
    }

    [Fact]
    public async Task InvokeAsync_WhenResultSuccess_ReturnsNoContent()
    {
        var filter = new ResultEndpointFilter();
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());

        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(Result.Success()));

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task InvokeAsync_WhenResultFailure_ReturnsProblemDetails()
    {
        // Pass IncludeDescription=true to test description forwarding; the secure
        // default is false (description replaced with "An error occurred.").
        var options = new ResultHttpOptions { IncludeDescription = true };
        var filter = new ResultEndpointFilter(options: options);
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());

        var error = Error.Validation("V", "M");
        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(Result.Failure(error)));

        var problem = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problem.StatusCode.Should().Be(400);
        problem.ProblemDetails.Title.Should().Be("Bad Request");
        problem.ProblemDetails.Detail.Should().Be("M");
        problem.ProblemDetails.Extensions["errorCode"].Should().Be("V");
    }

    [Fact]
    public async Task InvokeAsync_WhenResultTSuccess_ReturnsOkWithValue()
    {
        var filter = new ResultEndpointFilter();
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());

        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(Result.Success("value")));

        var ok = result.Should().BeOfType<Ok<object>>().Subject;
        ok.Value.Should().Be("value");
    }

    [Fact]
    public async Task InvokeAsync_WhenResultTFailure_ReturnsProblemDetails()
    {
        // Pass IncludeDescription=true to test description forwarding; the secure
        // default is false (description replaced with "An error occurred.").
        var options = new ResultHttpOptions { IncludeDescription = true };
        var filter = new ResultEndpointFilter(options: options);
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());

        var error = Error.NotFound("N", "M");
        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(Result.Failure<string>(error)));

        var problem = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problem.StatusCode.Should().Be(404);
        problem.ProblemDetails.Title.Should().Be("Not Found");
        problem.ProblemDetails.Detail.Should().Be("M");
        problem.ProblemDetails.Extensions["errorCode"].Should().Be("N");
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
    public async Task InvokeAsync_WhenIResultOutcomeIsFailureWithoutError_ThrowsInvalidOperationException()
    {
        // This state is impossible through public APIs (Result.Failure requires a non-null Error),
        // but could theoretically occur via reflection or an external IResultOutcome implementation.
        // The filter must throw rather than silently return the raw object (which would produce
        // a 200 OK with the struct serialized as JSON body — a catastrophic failure mode).
        var filter = new ResultEndpointFilter(options: (ResultHttpOptions?)null);
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());
        var outcome = new MockOutcome { IsFailure = true, Error = null };

        Func<Task> action = async () => await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(outcome));
        var ex = await action.Should().ThrowAsync<InvalidOperationException>();

        ex.WithMessage("*ResultEndpointFilter encountered a result in an invalid state:*");
        ex.WithMessage("*IsFailure=true but Error is null*");
        ex.WithMessage("*MockOutcome*");
        ex.WithMessage("*This indicates a corrupted Result instance.*");
        ex.WithMessage("*Ensure all Result instances are created via Result.Success() or Result.Failure(Error).*");
    }

    // ─── ARB Audit Regression Tests (Blocking Condition 1) ───────────────────────────────────────
    // These tests confirm that the bug where default(Result<T>) produced a "200 OK null" response
    // is resolved. The filter must throw InvalidOperationException for any Uninitialized result.

    [Fact]
    public async Task InvokeAsync_WhenDefaultResultOfTUninitialized_ThrowsInvalidOperationException()
    {
        // Arrange
        // default(Result<string>) has IsSuccess=false, IsFailure=false, IsUninitialized=true.
        // Before the fix, this silently returned TypedResults.Ok(null) — a 200 OK with null body.
        // After the fix, this must throw InvalidOperationException to surface the programming error.
        var filter = new ResultEndpointFilter(options: (ResultHttpOptions?)null);
        var context = new TestEndpointFilterInvocationContext(new DefaultHttpContext());
        var uninitializedResult = default(Result<string>);

        // Act
        Func<Task> action = async () => await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(uninitializedResult));
        var ex = await action.Should().ThrowAsync<InvalidOperationException>();

        // Assert — message must identify the type and the uninitialized state
        ex.WithMessage("*ResultEndpointFilter encountered an uninitialized Result:*");
        ex.WithMessage("*Uninitialized*");
        ex.WithMessage("*IsUninitialized=True*");
        ex.WithMessage("*This typically means a handler returned default(Result<T>) instead of a properly constructed result.*");
        ex.WithMessage("*Ensure all handler return paths use Result.Success(), Result.Success<T>(value), or Result.Failure(error).*");
    }

    [Fact]
    public async Task InvokeAsync_WhenDefaultResultNonGenericUninitialized_ThrowsInvalidOperationException()
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
        Func<Task> action = async () => await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(uninitializedResult));
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task InvokeAsync_WhenIResultOutcomeUninitialized_ThrowsInvalidOperationException()
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
        Func<Task> action = async () => await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(outcome));
        var ex = await action.Should().ThrowAsync<InvalidOperationException>();

        // Assert
        ex.WithMessage("*ResultEndpointFilter encountered an uninitialized Result:*");
        ex.WithMessage("*Uninitialized*");
        ex.WithMessage("*MockOutcome*");
        ex.WithMessage("*This typically means a handler returned default(Result<T>) instead of a properly constructed result.*");
        ex.WithMessage("*Ensure all handler return paths use Result.Success(), Result.Success<T>(value), or Result.Failure(error).*");
    }
}






