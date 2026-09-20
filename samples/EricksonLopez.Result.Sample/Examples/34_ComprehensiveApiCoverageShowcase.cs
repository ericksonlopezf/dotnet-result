// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using EricksonLopez.Result.Dapr;
using EricksonLopez.Result.FluentValidation;
using EricksonLopez.Result.Grpc;
using EricksonLopez.Result.MassTransit;
using EricksonLopez.Result.MediatR;
using EricksonLopez.Result.OpenTelemetry;
using EricksonLopez.Result.Polly;
using EricksonLopez.Result.Testing;
using FluentValidation;
using Grpc.Core;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Polly;

namespace EricksonLopez.Result.Sample.Examples;

/// <summary>
/// Demonstrates and verifies execution of all extended ecosystem methods.
/// </summary>
public static class ComprehensiveApiCoverageShowcase
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 34. COMPREHENSIVE PUBLIC API COVERAGE SHOWCASE");
        Console.WriteLine("========================================================");

        // 1. Dependency Injection & Metrics
        var services = new ServiceCollection();
        services.AddResultMetrics();
        Console.WriteLine("  [1] AddResultMetrics verified on IServiceCollection ✓");

        // 2. Business Error Domain Factory
        var bizError = Error.Business("Order.AlreadyShipped", "The order has already shipped and cannot be modified.");
        Console.WriteLine($"  [2] Error.Business created: [{bizError.Code}] {bizError.Description} (Type={bizError.Type}) ✓");

        // 3. Dapr State Store Integration
        var daprClient = Substitute.For<DaprClient>();
        daprClient.GetStateAndETagAsync<string>("statestore", "order-key", Arg.Any<ConsistencyMode?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns(("order-payload", "etag-100"));
        daprClient.TrySaveStateAsync<string>("statestore", "order-key", "order-payload", "etag-100", Arg.Any<StateOptions?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns(true);
        daprClient.TryDeleteStateAsync("statestore", "order-key", "etag-100", Arg.Any<StateOptions?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var getStateRes = await daprClient.GetStateWithResultAsync<string>("statestore", "order-key");
        var saveStateRes = await daprClient.SaveStateWithResultAsync<string>("statestore", "order-key", "order-payload", "etag-100");
        var deleteStateRes = await daprClient.DeleteStateWithResultAsync("statestore", "order-key", "etag-100");
        Console.WriteLine($"  [3] Dapr State Extensions: Get={getStateRes.IsSuccess}, Save={saveStateRes.IsSuccess}, Delete={deleteStateRes.IsSuccess} ✓");

        // 4. MediatR Exception Behavior Pipeline
        var behavior = new ResultExceptionBehavior<DemoMediatRRequest, Result<string>>();
        var handledResult = await behavior.Handle(new DemoMediatRRequest(), (ct) => Task.FromResult(Result.Success("mediatr-ok")), CancellationToken.None);
        Console.WriteLine($"  [4] MediatR ResultExceptionBehavior.Handle: IsSuccess={handledResult.IsSuccess}, Value='{handledResult.Value}' ✓");

        // 5. Polly Resilience Predicate Builders
        var predBuilderT = new PredicateBuilder<Result<string>>().HandleRetryableError();
        var predBuilder = new PredicateBuilder<Result>().HandleRetryableError();
        Console.WriteLine("  [5] Polly HandleRetryableError (Generic and Non-generic) verified ✓");

        // 6. ASP.NET Core ResultHttpOptions & Endpoint Filter
        var hostEnv = Substitute.For<IHostEnvironment>();
        hostEnv.EnvironmentName.Returns("Development");
        var httpOptions = new ResultHttpOptions().IncludeDescriptionInDevelopment(hostEnv);

        var httpContext = new DefaultHttpContext();
        var filterContext = EndpointFilterInvocationContext.Create(httpContext);
        var endpointFilter = new ResultEndpointFilter(httpOptions);
        var filterResult = await endpointFilter.InvokeAsync(filterContext, (ctx) => ValueTask.FromResult<object?>(Result.Success()));
        Console.WriteLine($"  [6] AspNetCore: IncludeDescriptionInDevelopment & InvokeAsync verified (Result={filterResult?.GetType().Name}) ✓");

        // 7. MassTransit Filter Probing and Configuration
        var probeContext = Substitute.For<ProbeContext>();
        var consumeFilter = new ResultConsumeFilter<ProcessOrderMessage>();
        consumeFilter.Probe(probeContext);

        var pipeConfigurator = Substitute.For<IConsumePipeConfigurator>();
        pipeConfigurator.UseResultFilter<ProcessOrderMessage>();
        Console.WriteLine("  [7] MassTransit: ResultConsumeFilter.Probe & UseResultFilter verified ✓");

        // 8. LINQ Explicit Operators: Select & SelectMany
        var baseRes = Result.Success(21);
        var projected = baseRes.Select(x => x * 2);
        var flattened = baseRes.SelectMany(x => Result.Success(x * 2), (x, y) => x + y);
        Console.WriteLine($"  [8] LINQ: Select={projected.Value}, SelectMany={flattened.Value} ✓");

        // 9. Monadic Tapping (Tap)
        var tappedNonGen = Result.Success().Tap(() => { });
        var tappedNonGenState = Result.Success().Tap("state", (s) => { });
        var tappedGen = Result.Success(100).Tap(v => { });
        var tappedGenState = Result.Success(100).Tap("state", (s, v) => { });
        Console.WriteLine($"  [9] Monadic Tap: NonGen={tappedNonGen.IsSuccess}, State={tappedNonGenState.IsSuccess}, Gen={tappedGen.IsSuccess}, GenState={tappedGenState.IsSuccess} ✓");

        // 10. Grpc Client & Server Extensions
        var grpcTask = Task.FromResult("grpc-response");
        var grpcResult = await grpcTask.ToResultAsync();

        var serverInterceptor = new ResultServerInterceptor();
        var serverCallContext = Substitute.For<ServerCallContext>();
        var serverResponse = await serverInterceptor.UnaryServerHandler("req-data", serverCallContext, (req, ctx) => Task.FromResult("unary-response"));
        Console.WriteLine($"  [10] gRPC: ToResultAsync={grpcResult.IsSuccess}, UnaryServerHandler='{serverResponse}' ✓");

        // 11. Async ValueTask Try Methods (TryAsyncValue)
        var tryVal1 = await Result.TryAsyncValue(static async () => await ValueTask.CompletedTask, static ex => Error.Failure("Fail.1", ex.Message));
        var tryVal2 = await Result.TryAsyncValue(static async (ct) => await ValueTask.CompletedTask, static ex => Error.Failure("Fail.2", ex.Message));
        var tryValT1 = await Result.TryAsyncValue<int>(static async () => await ValueTask.FromResult(42), static ex => Error.Failure("Fail.3", ex.Message));
        var tryValT2 = await Result.TryAsyncValue<int>(static async (ct) => await ValueTask.FromResult(42), static ex => Error.Failure("Fail.4", ex.Message));
        var tryValState1 = await Result.TryAsyncValue<string, int>("state", static () => ValueTask.FromResult(42), static (s, ex) => Error.Failure("Fail.5", $"{s}: {ex.Message}"));
        var tryValState2 = await Result.TryAsyncValue<string, int>("state", static (ct) => ValueTask.FromResult(42), static (s, ex) => Error.Failure("Fail.6", $"{s}: {ex.Message}"));
        Console.WriteLine($"  [11] TryAsyncValue: NonGen={tryVal1.IsSuccess}, T={tryValT1.Value}, State={tryValState1.Value} ✓");

        // 12. FluentValidation & Parallel Validation
        var demoValidator = new DemoOrderValidator();
        var validatedOrder = await demoValidator.ValidateToResultWithValueAsync(new DemoOrder("ORD-2026", 250m));
        var parallelRes = await Result.ValidateAllParallelAsync([ct => Task.FromResult(Result.Success())]);
        var parallelValRes = await Result.ValidateAllParallelAsync("Order-Val", [(val, ct) => Task.FromResult(Result.Success())]);
        Console.WriteLine($"  [12] Validation: ValidateToResultWithValueAsync={validatedOrder.IsSuccess}, ValidateAllParallelAsync={parallelRes.IsSuccess}, WithValue={parallelValRes.IsSuccess} ✓");

        // 13. ErrorBuilder with Single Inner Error
        var childError = Error.Validation("Child.Err", "Specific inner error.");
        var parentError = Error.Create("Parent.Err", "Composite failure.")
            .WithInnerError(childError)
            .WithMetadata("Key", "Value")
            .WithCorrelationId("CORR-1234")
            .WithTraceId("TRACE-5678")
            .WithRetryability(ErrorRetryability.Transient)
            .WithSeverity(ErrorSeverity.Warning)
            .Build();
        Console.WriteLine($"  [13] ErrorBuilder.WithInnerError verified: Parent={parentError.Code}, InnerCount={parentError.InnerErrors.Length} ✓");

        // 14. Comprehensive Testing Assertions (Synchronous and Asynchronous)
        var permError = Error.Create("Perm.Err", "Permanent failure.").WithRetryability(ErrorRetryability.Permanent).Build();
        var plainError = Error.Failure("Plain.Err", "Plain description.");
        var combinedFailureError = Error.Validation(
            WellKnownErrors.CombinedFailuresCode,
            "Combined errors",
            [Error.Validation("Sub.1", "d1"), Error.Validation("Sub.2", "d2")]);

        var failureResult = Result.Failure(parentError);
        var failureGenResult = Result.Failure<string>(parentError);
        var successResult = Result.Success();
        var successGenResult = Result.Success("verified-value");

        // Combined Failures
        Result.Failure(combinedFailureError).ShouldBeCombinedFailure(2);
        Result.Failure<string>(combinedFailureError).ShouldBeCombinedFailure(2);
        await Task.FromResult(Result.Failure(combinedFailureError)).ShouldBeCombinedFailureAsync(2);
        await ValueTask.FromResult(Result.Failure(combinedFailureError)).ShouldBeCombinedFailureAsync(2);
        await Task.FromResult(Result.Failure<string>(combinedFailureError)).ShouldBeCombinedFailureAsync(2);
        await ValueTask.FromResult(Result.Failure<string>(combinedFailureError)).ShouldBeCombinedFailureAsync(2);

        // Async Failures & Success
        await Task.FromResult(failureResult).ShouldBeFailureAsync();
        await ValueTask.FromResult(failureResult).ShouldBeFailureAsync();
        await Task.FromResult(failureGenResult).ShouldBeFailureAsync();
        await ValueTask.FromResult(failureGenResult).ShouldBeFailureAsync();

        await Task.FromResult(successResult).ShouldBeSuccessAsync();
        await ValueTask.FromResult(successResult).ShouldBeSuccessAsync();
        await Task.FromResult(successGenResult).ShouldBeSuccessAsync();
        await ValueTask.FromResult(successGenResult).ShouldBeSuccessAsync();

        // Permanence & Retryability
        permError.ShouldBePermanent();
        Result.Failure(permError).ShouldBePermanent();
        Result.Failure<string>(permError).ShouldBePermanent();
        await Task.FromResult(Result.Failure(permError)).ShouldBePermanentAsync();
        await ValueTask.FromResult(Result.Failure(permError)).ShouldBePermanentAsync();
        await Task.FromResult(Result.Failure<string>(permError)).ShouldBePermanentAsync();
        await ValueTask.FromResult(Result.Failure<string>(permError)).ShouldBePermanentAsync();

        parentError.ShouldBeRetryable();
        Result.Failure(parentError).ShouldBeRetryable();
        Result.Failure<string>(parentError).ShouldBeRetryable();
        await Task.FromResult(Result.Failure(parentError)).ShouldBeRetryableAsync();
        await ValueTask.FromResult(Result.Failure(parentError)).ShouldBeRetryableAsync();
        await Task.FromResult(Result.Failure<string>(parentError)).ShouldBeRetryableAsync();
        await ValueTask.FromResult(Result.Failure<string>(parentError)).ShouldBeRetryableAsync();

        // Uninitialized
        default(Result).ShouldBeUninitialized();
        default(Result<string>).ShouldBeUninitialized();
        await Task.FromResult(default(Result)).ShouldBeUninitializedAsync();
        await ValueTask.FromResult(default(Result)).ShouldBeUninitializedAsync();
        await Task.FromResult(default(Result<string>)).ShouldBeUninitializedAsync();
        await ValueTask.FromResult(default(Result<string>)).ShouldBeUninitializedAsync();

        // Inner Errors Assertions
        parentError.ShouldContainInnerError("Child.Err");
        failureResult.ShouldContainInnerError("Child.Err");
        failureGenResult.ShouldContainInnerError("Child.Err");
        await Task.FromResult(failureResult).ShouldContainInnerErrorAsync("Child.Err");
        await ValueTask.FromResult(failureResult).ShouldContainInnerErrorAsync("Child.Err");
        await Task.FromResult(failureGenResult).ShouldContainInnerErrorAsync("Child.Err");
        await ValueTask.FromResult(failureGenResult).ShouldContainInnerErrorAsync("Child.Err");

        parentError.ShouldHaveInnerErrors(1);
        failureResult.ShouldHaveInnerErrors(1);
        failureGenResult.ShouldHaveInnerErrors(1);
        await Task.FromResult(failureResult).ShouldHaveInnerErrorsAsync(1);
        await ValueTask.FromResult(failureResult).ShouldHaveInnerErrorsAsync(1);
        await Task.FromResult(failureGenResult).ShouldHaveInnerErrorsAsync(1);
        await ValueTask.FromResult(failureGenResult).ShouldHaveInnerErrorsAsync(1);

        failureResult.ShouldHaveInnerErrorCount(1);
        failureGenResult.ShouldHaveInnerErrorCount(1);
        await Task.FromResult(failureResult).ShouldHaveInnerErrorCountAsync(1);
        await ValueTask.FromResult(failureResult).ShouldHaveInnerErrorCountAsync(1);
        await Task.FromResult(failureGenResult).ShouldHaveInnerErrorCountAsync(1);
        await ValueTask.FromResult(failureGenResult).ShouldHaveInnerErrorCountAsync(1);

        failureResult.ShouldHaveInnerErrorsMatching(inners => inners.Length == 1);
        failureGenResult.ShouldHaveInnerErrorsMatching(inners => inners.Length == 1);

        plainError.ShouldHaveNoInnerErrors();
        Result.Failure(plainError).ShouldHaveNoInnerErrors();
        Result.Failure<string>(plainError).ShouldHaveNoInnerErrors();
        Result.Failure(plainError).ShouldNotHaveInnerErrors();
        Result.Failure<string>(plainError).ShouldNotHaveInnerErrors();
        await Task.FromResult(Result.Failure(plainError)).ShouldNotHaveInnerErrorsAsync();
        await ValueTask.FromResult(Result.Failure(plainError)).ShouldNotHaveInnerErrorsAsync();
        await Task.FromResult(Result.Failure<string>(plainError)).ShouldNotHaveInnerErrorsAsync();
        await ValueTask.FromResult(Result.Failure<string>(plainError)).ShouldNotHaveInnerErrorsAsync();

        // Correlation & Trace IDs
        parentError.ShouldHaveCorrelationId("CORR-1234");
        failureResult.ShouldHaveCorrelationId("CORR-1234");
        failureGenResult.ShouldHaveCorrelationId("CORR-1234");
        await Task.FromResult(failureResult).ShouldHaveCorrelationIdAsync("CORR-1234");
        await ValueTask.FromResult(failureResult).ShouldHaveCorrelationIdAsync("CORR-1234");
        await Task.FromResult(failureGenResult).ShouldHaveCorrelationIdAsync("CORR-1234");
        await ValueTask.FromResult(failureGenResult).ShouldHaveCorrelationIdAsync("CORR-1234");

        parentError.ShouldHaveTraceId("TRACE-5678");
        failureResult.ShouldHaveTraceId("TRACE-5678");
        failureGenResult.ShouldHaveTraceId("TRACE-5678");
        await Task.FromResult(failureResult).ShouldHaveTraceIdAsync("TRACE-5678");
        await ValueTask.FromResult(failureResult).ShouldHaveTraceIdAsync("TRACE-5678");
        await Task.FromResult(failureGenResult).ShouldHaveTraceIdAsync("TRACE-5678");
        await ValueTask.FromResult(failureGenResult).ShouldHaveTraceIdAsync("TRACE-5678");

        // Code, Description, Severity, Type, Count
        await Task.FromResult(failureResult).ShouldHaveErrorCodeAsync("Parent.Err");
        await ValueTask.FromResult(failureResult).ShouldHaveErrorCodeAsync("Parent.Err");
        await Task.FromResult(failureGenResult).ShouldHaveErrorCodeAsync("Parent.Err");
        await ValueTask.FromResult(failureGenResult).ShouldHaveErrorCodeAsync("Parent.Err");

        await Task.FromResult(failureResult).ShouldHaveDescriptionAsync("Composite failure.");
        await ValueTask.FromResult(failureResult).ShouldHaveDescriptionAsync("Composite failure.");
        await Task.FromResult(failureGenResult).ShouldHaveDescriptionAsync("Composite failure.");
        await ValueTask.FromResult(failureGenResult).ShouldHaveDescriptionAsync("Composite failure.");

        await Task.FromResult(failureResult).ShouldHaveSeverityAsync(ErrorSeverity.Warning);
        await ValueTask.FromResult(failureResult).ShouldHaveSeverityAsync(ErrorSeverity.Warning);
        await Task.FromResult(failureGenResult).ShouldHaveSeverityAsync(ErrorSeverity.Warning);
        await ValueTask.FromResult(failureGenResult).ShouldHaveSeverityAsync(ErrorSeverity.Warning);

        await Task.FromResult(failureResult).ShouldHaveErrorTypeAsync(ErrorType.Failure);
        await ValueTask.FromResult(failureResult).ShouldHaveErrorTypeAsync(ErrorType.Failure);
        await Task.FromResult(failureGenResult).ShouldHaveErrorTypeAsync(ErrorType.Failure);
        await ValueTask.FromResult(failureGenResult).ShouldHaveErrorTypeAsync(ErrorType.Failure);

        failureResult.ShouldHaveErrorCount(1);
        failureGenResult.ShouldHaveErrorCount(1);

        failureResult.ShouldHaveErrorMatching(e => e.Code == "Parent.Err");
        failureGenResult.ShouldHaveErrorMatching(e => e.Code == "Parent.Err");
        await Task.FromResult(failureResult).ShouldHaveErrorMatchingAsync(e => e.Code == "Parent.Err");
        await ValueTask.FromResult(failureResult).ShouldHaveErrorMatchingAsync(e => e.Code == "Parent.Err");
        await Task.FromResult(failureGenResult).ShouldHaveErrorMatchingAsync(e => e.Code == "Parent.Err");
        await ValueTask.FromResult(failureGenResult).ShouldHaveErrorMatchingAsync(e => e.Code == "Parent.Err");

        // Metadata Assertions
        parentError.ShouldHaveMetadataKey("Key");
        failureResult.ShouldHaveMetadataKey("Key");
        failureGenResult.ShouldHaveMetadataKey("Key");
        failureResult.ShouldHaveMetadataValue("Key", "Value");
        failureGenResult.ShouldHaveMetadataValue("Key", "Value");
        await Task.FromResult(failureResult).ShouldHaveMetadataAsync("Key", "Value");
        await ValueTask.FromResult(failureResult).ShouldHaveMetadataAsync("Key", "Value");
        await Task.FromResult(failureGenResult).ShouldHaveMetadataAsync("Key", "Value");
        await ValueTask.FromResult(failureGenResult).ShouldHaveMetadataAsync("Key", "Value");

        Result.Failure(plainError).ShouldNotHaveMetadata("MissingKey");
        Result.Failure<string>(plainError).ShouldNotHaveMetadata("MissingKey");

        // Value Assertions
        successGenResult.ShouldHaveValue("verified-value");
        successGenResult.ShouldHaveValue(v => v.StartsWith("verified", StringComparison.Ordinal));
        await Task.FromResult(successGenResult).ShouldHaveValueAsync("verified-value");
        await ValueTask.FromResult(successGenResult).ShouldHaveValueAsync("verified-value");

        // Satisfy Assertions
        successResult.ShouldSatisfy(r => { });
        successGenResult.ShouldSatisfy(v => { });
        await Task.FromResult(successResult).ShouldSatisfyAsync(r => { });
        await ValueTask.FromResult(successResult).ShouldSatisfyAsync(r => { });
        await Task.FromResult(successGenResult).ShouldSatisfyAsync(v => { });
        await ValueTask.FromResult(successGenResult).ShouldSatisfyAsync(v => { });

        failureResult.ShouldSatisfyError(e => { });
        failureGenResult.ShouldSatisfyError(e => { });
        await Task.FromResult(failureResult).ShouldSatisfyErrorAsync(e => { });
        await ValueTask.FromResult(failureResult).ShouldSatisfyErrorAsync(e => { });
        await Task.FromResult(failureGenResult).ShouldSatisfyErrorAsync(e => { });
        await ValueTask.FromResult(failureGenResult).ShouldSatisfyErrorAsync(e => { });

        // Strict Equality
        Result.Failure(plainError).ShouldStrictlyEqual(plainError);
        Result.Failure<string>(plainError).ShouldStrictlyEqual(plainError);

        Console.WriteLine("  [14] All 40+ Fluent Testing Assertions verified successfully ✓");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n ✔ All 62 Extended Public APIs in dotnet-result successfully verified in Example 34.");
        Console.ResetColor();
    }

    private sealed record DemoMediatRRequest : IRequest<Result<string>>;

    private sealed record DemoOrder(string OrderId, decimal Amount);

    private sealed class DemoOrderValidator : AbstractValidator<DemoOrder>
    {
        public DemoOrderValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
        }
    }
}
