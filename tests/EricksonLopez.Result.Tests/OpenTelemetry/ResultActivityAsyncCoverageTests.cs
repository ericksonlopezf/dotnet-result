using System.Threading.Tasks;
using Xunit;
using EricksonLopez.Result.OpenTelemetry;

namespace EricksonLopez.Result.Tests.OpenTelemetry;

[Collection("Metrics")]
public class ResultActivityAsyncCoverageTests
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
    public async Task Cover_ResultActivity_ValueTask()
    {
        await GetUncompletedValueTask(Result.Success()).TraceOutcome("A");
        await GetUncompletedValueTask(Result.Success()).TraceOnSuccess("A");
        await GetUncompletedValueTask(Result.Success()).TraceOnFailure("A");
        
        await GetUncompletedValueTask(Result.Failure(Error.Failure("A", "B"))).TraceOutcome("A");
        await GetUncompletedValueTask(Result.Failure(Error.Failure("A", "B"))).TraceOnSuccess("A");
        await GetUncompletedValueTask(Result.Failure(Error.Failure("A", "B"))).TraceOnFailure("A");
        
        await GetUncompletedValueTask(Result.Success(1)).TraceOutcome("A");
        await GetUncompletedValueTask(Result.Success(1)).TraceOnSuccess("A");
        await GetUncompletedValueTask(Result.Success(1)).TraceOnFailure("A");
        
        await GetUncompletedValueTask(Result.Failure<int>(Error.Failure("A", "B"))).TraceOutcome("A");
        await GetUncompletedValueTask(Result.Failure<int>(Error.Failure("A", "B"))).TraceOnSuccess("A");
        await GetUncompletedValueTask(Result.Failure<int>(Error.Failure("A", "B"))).TraceOnFailure("A");
    }

    [Fact]
    public async Task Cover_ResultActivity_Task()
    {
        await GetUncompletedTask(Result.Success()).TraceOutcome("A");
        await GetUncompletedTask(Result.Success()).TraceOnSuccess("A");
        await GetUncompletedTask(Result.Success()).TraceOnFailure("A");
        
        await GetUncompletedTask(Result.Failure(Error.Failure("A", "B"))).TraceOutcome("A");
        await GetUncompletedTask(Result.Failure(Error.Failure("A", "B"))).TraceOnSuccess("A");
        await GetUncompletedTask(Result.Failure(Error.Failure("A", "B"))).TraceOnFailure("A");
        
        await GetUncompletedTask(Result.Success(1)).TraceOutcome("A");
        await GetUncompletedTask(Result.Success(1)).TraceOnSuccess("A");
        await GetUncompletedTask(Result.Success(1)).TraceOnFailure("A");
        
        await GetUncompletedTask(Result.Failure<int>(Error.Failure("A", "B"))).TraceOutcome("A");
        await GetUncompletedTask(Result.Failure<int>(Error.Failure("A", "B"))).TraceOnSuccess("A");
        await GetUncompletedTask(Result.Failure<int>(Error.Failure("A", "B"))).TraceOnFailure("A");
    }
}

