using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Testing.Tests;

public class ResultAssertionsAsyncCoverageTests
{
    private static async ValueTask<Result> GetUncompletedValueTask(Result r)
    {
        await Task.Delay(1);
        return r;
    }

    private static async ValueTask<Result<T>> GetUncompletedValueTask<T>(Result<T> r)
    {
        await Task.Delay(1);
        return r;
    }

    private static async Task<Result> GetUncompletedTask(Result r)
    {
        await Task.Delay(1);
        return r;
    }

    private static async Task<Result<T>> GetUncompletedTask<T>(Result<T> r)
    {
        await Task.Delay(1);
        return r;
    }

    [Fact]
    public async Task Cover_ResultAssertions_ValueTask()
    {
        var e = Error.Create("A", "B").WithSeverity(ErrorSeverity.Critical).WithRetryability(ErrorRetryability.Permanent).WithTraceId("T1").WithCorrelationId("C1").WithMetadata("K", "V").Build();
        var e2 = Error.Create("A", "B").WithSeverity(ErrorSeverity.Critical).WithRetryability(ErrorRetryability.Transient).Build();
        var innerError = Error.Validation("X", "Y");
        var nestedResult = Result.Failure(Error.Failure("M", "N", innerError));

        await GetUncompletedValueTask(Result.Success()).ShouldBeSuccessAsync();
        await GetUncompletedValueTask(Result.Failure(e)).ShouldBeFailureAsync();
        await GetUncompletedValueTask(Result.Failure(e)).ShouldHaveErrorCodeAsync("A");
        await GetUncompletedValueTask(Result.Failure(e)).ShouldHaveErrorTypeAsync(ErrorType.Failure);
        await GetUncompletedValueTask(Result.Failure(e)).ShouldHaveMetadataAsync("K", "V");
        await GetUncompletedValueTask(Result.Failure(e)).ShouldHaveSeverityAsync(ErrorSeverity.Critical);
        await GetUncompletedValueTask(Result.Failure(e)).ShouldBePermanentAsync();
        await GetUncompletedValueTask(Result.Failure(e2)).ShouldBeRetryableAsync();
        await GetUncompletedValueTask(nestedResult).ShouldHaveInnerErrorsAsync(1); await GetUncompletedValueTask(Result.Failure<int>(Error.Failure("M", "N", innerError))).ShouldHaveInnerErrorsAsync(1);
        await GetUncompletedValueTask(nestedResult).ShouldContainInnerErrorAsync("X"); await GetUncompletedValueTask(Result.Failure<int>(Error.Failure("M", "N", innerError))).ShouldContainInnerErrorAsync("X");
        await GetUncompletedValueTask(Result.Failure(e)).ShouldHaveDescriptionAsync("B");
        await GetUncompletedValueTask(Result.Failure(e)).ShouldHaveTraceIdAsync("T1");
        await GetUncompletedValueTask(Result.Failure(e)).ShouldHaveCorrelationIdAsync("C1");
        
        await GetUncompletedValueTask(Result.Success(1)).ShouldBeSuccessAsync();
        await GetUncompletedValueTask(Result.Success(1)).ShouldHaveValueAsync(1);
        await GetUncompletedValueTask(Result.Failure<int>(e)).ShouldBeFailureAsync();
        await GetUncompletedValueTask(Result.Failure<int>(e)).ShouldHaveErrorCodeAsync("A");
        await GetUncompletedValueTask(Result.Failure<int>(e)).ShouldHaveErrorTypeAsync(ErrorType.Failure);
        await GetUncompletedValueTask(Result.Failure<int>(e)).ShouldHaveMetadataAsync("K", "V");
        await GetUncompletedValueTask(Result.Failure<int>(e)).ShouldHaveSeverityAsync(ErrorSeverity.Critical);
        await GetUncompletedValueTask(Result.Failure<int>(e)).ShouldBePermanentAsync();
        await GetUncompletedValueTask(Result.Failure<int>(e2)).ShouldBeRetryableAsync();
        await GetUncompletedValueTask(nestedResult).ShouldHaveInnerErrorsAsync(1); await GetUncompletedValueTask(Result.Failure<int>(Error.Failure("M", "N", innerError))).ShouldHaveInnerErrorsAsync(1);
        await GetUncompletedValueTask(nestedResult).ShouldContainInnerErrorAsync("X"); await GetUncompletedValueTask(Result.Failure<int>(Error.Failure("M", "N", innerError))).ShouldContainInnerErrorAsync("X");
        await GetUncompletedValueTask(Result.Failure<int>(e)).ShouldHaveDescriptionAsync("B");
        await GetUncompletedValueTask(Result.Failure<int>(e)).ShouldHaveTraceIdAsync("T1");
        await GetUncompletedValueTask(Result.Failure<int>(e)).ShouldHaveCorrelationIdAsync("C1");
    }

    [Fact]
    public async Task Cover_ResultAssertions_Task()
    {
        var e = Error.Create("A", "B").WithSeverity(ErrorSeverity.Critical).WithRetryability(ErrorRetryability.Permanent).WithTraceId("T1").WithCorrelationId("C1").WithMetadata("K", "V").Build();
        var e2 = Error.Create("A", "B").WithSeverity(ErrorSeverity.Critical).WithRetryability(ErrorRetryability.Transient).Build();
        var innerError = Error.Validation("X", "Y");
        var nestedResult = Result.Failure(Error.Failure("M", "N", innerError));

        await GetUncompletedTask(Result.Success()).ShouldBeSuccessAsync();
        await GetUncompletedTask(Result.Failure(e)).ShouldBeFailureAsync();
        await GetUncompletedTask(Result.Failure(e)).ShouldHaveErrorCodeAsync("A");
        await GetUncompletedTask(Result.Failure(e)).ShouldHaveErrorTypeAsync(ErrorType.Failure);
        await GetUncompletedTask(Result.Failure(e)).ShouldHaveMetadataAsync("K", "V");
        await GetUncompletedTask(Result.Failure(e)).ShouldHaveSeverityAsync(ErrorSeverity.Critical);
        await GetUncompletedTask(Result.Failure(e)).ShouldBePermanentAsync();
        await GetUncompletedTask(Result.Failure(e2)).ShouldBeRetryableAsync();
        await GetUncompletedTask(nestedResult).ShouldHaveInnerErrorsAsync(1); await GetUncompletedTask(Result.Failure<int>(Error.Failure("M", "N", innerError))).ShouldHaveInnerErrorsAsync(1);
        await GetUncompletedTask(nestedResult).ShouldContainInnerErrorAsync("X"); await GetUncompletedTask(Result.Failure<int>(Error.Failure("M", "N", innerError))).ShouldContainInnerErrorAsync("X");
        await GetUncompletedTask(Result.Failure(e)).ShouldHaveDescriptionAsync("B");
        await GetUncompletedTask(Result.Failure(e)).ShouldHaveTraceIdAsync("T1");
        await GetUncompletedTask(Result.Failure(e)).ShouldHaveCorrelationIdAsync("C1");
        
        await GetUncompletedTask(Result.Success(1)).ShouldBeSuccessAsync();
        await GetUncompletedTask(Result.Success(1)).ShouldHaveValueAsync(1);
        await GetUncompletedTask(Result.Failure<int>(e)).ShouldBeFailureAsync();
        await GetUncompletedTask(Result.Failure<int>(e)).ShouldHaveErrorCodeAsync("A");
        await GetUncompletedTask(Result.Failure<int>(e)).ShouldHaveErrorTypeAsync(ErrorType.Failure);
        await GetUncompletedTask(Result.Failure<int>(e)).ShouldHaveMetadataAsync("K", "V");
        await GetUncompletedTask(Result.Failure<int>(e)).ShouldHaveSeverityAsync(ErrorSeverity.Critical);
        await GetUncompletedTask(Result.Failure<int>(e)).ShouldBePermanentAsync();
        await GetUncompletedTask(Result.Failure<int>(e2)).ShouldBeRetryableAsync();
        await GetUncompletedTask(nestedResult).ShouldHaveInnerErrorsAsync(1); await GetUncompletedTask(Result.Failure<int>(Error.Failure("M", "N", innerError))).ShouldHaveInnerErrorsAsync(1);
        await GetUncompletedTask(nestedResult).ShouldContainInnerErrorAsync("X"); await GetUncompletedTask(Result.Failure<int>(Error.Failure("M", "N", innerError))).ShouldContainInnerErrorAsync("X");
        await GetUncompletedTask(Result.Failure<int>(e)).ShouldHaveDescriptionAsync("B");
        await GetUncompletedTask(Result.Failure<int>(e)).ShouldHaveTraceIdAsync("T1");
        await GetUncompletedTask(Result.Failure<int>(e)).ShouldHaveCorrelationIdAsync("C1");
    }
}


