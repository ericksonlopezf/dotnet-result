// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Testing.Tests;

public class ResultAssertionsTests
{
    private static readonly Error TestError = Error.Create("ValCode", "ValDesc").WithType(ErrorType.Validation).WithMetadata("k", "v").WithCorrelationId("corr").WithTraceId("trace").Build();


    [Fact]
    public void ShouldBeSuccess_Result_ReturnsResult_WhenSuccess()
    {
        var result = Result.Success();

        var returned = result.ShouldBeSuccess();

        returned.ShouldBeSuccess();
    }

    [Fact]
    public void ShouldBeSuccess_Result_Throws_WhenFailure()
    {
        var result = Result.Failure(TestError);

        var ex = Assert.Throws<ResultAssertionException>(() => result.ShouldBeSuccess());

        Assert.Contains(TestError.Code, ex.Message);
        Assert.Contains(TestError.Description, ex.Message);
    }

    [Fact]
    public void ShouldBeSuccess_Result_Throws_WhenUninitialized()
    {
        Result result = default;

        var ex = Assert.Throws<ResultAssertionException>(() => result.ShouldBeSuccess());

        Assert.Contains("uninitialized", ex.Message);
    }

    [Fact]
    public void ShouldBeSuccess_ResultT_ReturnsValue_WhenSuccess()
    {
        var result = Result<int>.Success(42);

        var value = result.ShouldBeSuccess();

        Assert.Equal(42, value);
    }

    [Fact]
    public void ShouldBeSuccess_ResultT_Throws_WhenFailure()
    {
        var result = Result<int>.Failure(TestError);

        var ex = Assert.Throws<ResultAssertionException>(() => result.ShouldBeSuccess());

        Assert.Contains("int", ex.Message);
    }

    [Fact]
    public void ShouldBeSuccess_ResultT_Throws_WhenUninitialized()
    {
        Result<int> result = default;

        var ex = Assert.Throws<ResultAssertionException>(() => result.ShouldBeSuccess());

        Assert.Contains("uninitialized", ex.Message);
    }


    [Fact]
    public void ShouldHaveValue_ReturnsValue_WhenMatches()
    {
        var result = Result<string>.Success("test");

        var value = result.ShouldHaveValue("test");

        Assert.Equal("test", value);
    }

    [Fact]
    public void ShouldHaveValue_Throws_WhenValueDiffers()
    {
        var result = Result<string>.Success("test");

        var ex = Assert.Throws<ResultAssertionException>(() => result.ShouldHaveValue("other"));

        Assert.Contains("Expected Result to have value", ex.Message);
    }


    [Fact]
    public void ShouldBeFailure_Result_ReturnsError_WhenFailure()
    {
        var result = Result.Failure(TestError);

        var error = result.ShouldBeFailure();

        Assert.Equal(TestError, error);
    }

    [Fact]
    public void ShouldBeFailure_Result_Throws_WhenSuccess()
    {
        var result = Result.Success();

        Assert.Throws<ResultAssertionException>(() => result.ShouldBeFailure());

    }

    [Fact]
    public void ShouldBeFailure_Result_Throws_WhenUninitialized()
    {
        Result result = default;

        Assert.Throws<ResultAssertionException>(() => result.ShouldBeFailure());

    }

    [Fact]
    public void ShouldBeFailure_ResultT_ReturnsError_WhenFailure()
    {
        var result = Result<int>.Failure(TestError);

        var error = result.ShouldBeFailure();

        Assert.Equal(TestError, error);
    }

    [Fact]
    public void ShouldBeFailure_ResultT_Throws_WhenSuccess()
    {
        var result = Result<int>.Success(1);

        Assert.Throws<ResultAssertionException>(() => result.ShouldBeFailure());

    }

    [Fact]
    public void ShouldBeFailure_ResultT_Throws_WhenUninitialized()
    {
        Result<int> result = default;

        Assert.Throws<ResultAssertionException>(() => result.ShouldBeFailure());

    }


    [Fact]
    public void ShouldHaveErrorCode_Result_Passes()
    {
        Result.Failure(TestError).ShouldHaveErrorCode("ValCode");
    }

    [Fact]
    public void ShouldHaveErrorCode_Result_ThrowsWhenDifferent()
    {
        Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveErrorCode("Other"));
    }

    [Fact]
    public void ShouldHaveErrorCode_ResultT_Passes()
    {
        Result<int>.Failure(TestError).ShouldHaveErrorCode("ValCode");
    }

    [Fact]
    public void ShouldHaveErrorCode_ResultT_ThrowsWhenDifferent()
    {
        Assert.Throws<ResultAssertionException>(() => Result<int>.Failure(TestError).ShouldHaveErrorCode("Other"));
    }

    [Fact]
    public void ShouldHaveErrorType_Result_Passes()
    {
        Result.Failure(TestError).ShouldHaveErrorType(ErrorType.Validation);
    }

    [Fact]
    public void ShouldHaveErrorType_Result_ThrowsWhenDifferent()
    {
        Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveErrorType(ErrorType.Failure));
    }

    [Fact]
    public void ShouldHaveErrorType_ResultT_Passes()
    {
        Result<int>.Failure(TestError).ShouldHaveErrorType(ErrorType.Validation);
    }

    [Fact]
    public void ShouldHaveErrorType_ResultT_ThrowsWhenDifferent()
    {
        Assert.Throws<ResultAssertionException>(() => Result<int>.Failure(TestError).ShouldHaveErrorType(ErrorType.Failure));
    }

    [Fact]
    public void ShouldHaveSeverity_Result_Passes()
    {
        var err = Error.Create("C", "D").WithSeverity(ErrorSeverity.Critical).Build();
        Result.Failure(err).ShouldHaveSeverity(ErrorSeverity.Critical);
    }

    [Fact]
    public void ShouldHaveSeverity_Result_ThrowsWhenDifferent()
    {
        var err = Error.Create("C", "D").WithSeverity(ErrorSeverity.Critical).Build();
        Assert.Throws<ResultAssertionException>(() => Result.Failure(err).ShouldHaveSeverity(ErrorSeverity.Error));
    }

    [Fact]
    public void ShouldHaveSeverity_ResultT_Passes()
    {
        var err = Error.Create("C", "D").WithSeverity(ErrorSeverity.Critical).Build();
        Result<int>.Failure(err).ShouldHaveSeverity(ErrorSeverity.Critical);
    }

    [Fact]
    public void ShouldHaveSeverity_ResultT_ThrowsWhenDifferent()
    {
        var err = Error.Create("C", "D").WithSeverity(ErrorSeverity.Critical).Build();
        Assert.Throws<ResultAssertionException>(() => Result<int>.Failure(err).ShouldHaveSeverity(ErrorSeverity.Error));
    }

    [Fact]
    public void ShouldHaveMetadata_Result_Passes()
    {
        Result.Failure(TestError).ShouldHaveMetadata("k", "v");
    }

    [Fact]
    public void ShouldHaveMetadata_Result_ThrowsWhenDifferent()
    {
        Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveMetadata("k", "other"));
        Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveMetadata("missing", "v"));
    }

    [Fact]
    public void ShouldHaveMetadata_ResultT_Passes()
    {
        Result<int>.Failure(TestError).ShouldHaveMetadata("k", "v");
    }

    [Fact]
    public void ShouldHaveMetadata_ResultT_ThrowsWhenDifferent()
    {
        Assert.Throws<ResultAssertionException>(() => Result<int>.Failure(TestError).ShouldHaveMetadata("k", "other"));
        Assert.Throws<ResultAssertionException>(() => Result<int>.Failure(TestError).ShouldHaveMetadata("missing", "v"));
    }

    [Fact]
    public void ShouldHaveInnerErrors_Result_Passes()
    {
        var err = Error.Failure("F", "D", Error.Validation("V", "D"));
        Result.Failure(err).ShouldHaveInnerErrors(1);
    }

    [Fact]
    public void ShouldHaveInnerErrors_Result_ThrowsWhenDifferent()
    {
        var err = Error.Failure("F", "D", Error.Validation("V", "D"));
        Assert.Throws<ResultAssertionException>(() => Result.Failure(err).ShouldHaveInnerErrors(0));
    }

    [Fact]
    public void ShouldHaveInnerErrors_ResultT_Passes()
    {
        var err = Error.Failure("F", "D", Error.Validation("V", "D"));
        Result<int>.Failure(err).ShouldHaveInnerErrors(1);
    }

    [Fact]
    public void ShouldHaveInnerErrors_ResultT_ThrowsWhenDifferent()
    {
        var err = Error.Failure("F", "D", Error.Validation("V", "D"));
        Assert.Throws<ResultAssertionException>(() => Result<int>.Failure(err).ShouldHaveInnerErrors(0));
    }

    [Fact]
    public void ShouldBeRetryable_Result_Passes()
    {
        var err = Error.Create("C", "D").WithRetryability(ErrorRetryability.Transient).Build();
        Result.Failure(err).ShouldBeRetryable();
    }

    [Fact]
    public void ShouldBeRetryable_Result_ThrowsWhenDifferent()
    {
        var err = Error.Create("C", "D").WithRetryability(ErrorRetryability.Permanent).Build();
        Assert.Throws<ResultAssertionException>(() => Result.Failure(err).ShouldBeRetryable());
    }

    [Fact]
    public void ShouldBeRetryable_ResultT_Passes()
    {
        var err = Error.Create("C", "D").WithRetryability(ErrorRetryability.Transient).Build();
        Result<int>.Failure(err).ShouldBeRetryable();
    }

    [Fact]
    public void ShouldBeRetryable_ResultT_ThrowsWhenDifferent()
    {
        var err = Error.Create("C", "D").WithRetryability(ErrorRetryability.Permanent).Build();
        Assert.Throws<ResultAssertionException>(() => Result<int>.Failure(err).ShouldBeRetryable());
    }

    [Fact]
    public void ShouldBePermanent_Result_Passes()
    {
        var err = Error.Create("C", "D").WithRetryability(ErrorRetryability.Permanent).Build();
        Result.Failure(err).ShouldBePermanent();
    }

    [Fact]
    public void ShouldBePermanent_Result_ThrowsWhenDifferent()
    {
        var err = Error.Create("C", "D").WithRetryability(ErrorRetryability.Transient).Build();
        Assert.Throws<ResultAssertionException>(() => Result.Failure(err).ShouldBePermanent());
    }

    [Fact]
    public void ShouldBePermanent_ResultT_Passes()
    {
        var err = Error.Create("C", "D").WithRetryability(ErrorRetryability.Permanent).Build();
        Result<int>.Failure(err).ShouldBePermanent();
    }

    [Fact]
    public void ShouldBePermanent_ResultT_ThrowsWhenDifferent()
    {
        var err = Error.Create("C", "D").WithRetryability(ErrorRetryability.Transient).Build();
        Assert.Throws<ResultAssertionException>(() => Result<int>.Failure(err).ShouldBePermanent());
    }

    [Fact]
    public void ShouldHaveTraceId_Result_Passes()
    {
        Result.Failure(TestError).ShouldHaveTraceId("trace");
    }

    [Fact]
    public void ShouldHaveTraceId_Result_ThrowsWhenDifferent()
    {
        Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveTraceId("other"));
    }

    [Fact]
    public void ShouldHaveTraceId_ResultT_Passes()
    {
        Result<int>.Failure(TestError).ShouldHaveTraceId("trace");
    }

    [Fact]
    public void ShouldHaveTraceId_ResultT_ThrowsWhenDifferent()
    {
        Assert.Throws<ResultAssertionException>(() => Result<int>.Failure(TestError).ShouldHaveTraceId("other"));
    }

    [Fact]
    public void ShouldHaveCorrelationId_Result_Passes()
    {
        Result.Failure(TestError).ShouldHaveCorrelationId("corr");
    }

    [Fact]
    public void ShouldHaveCorrelationId_Result_ThrowsWhenDifferent()
    {
        Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveCorrelationId("other"));
    }

    [Fact]
    public void ShouldHaveCorrelationId_ResultT_Passes()
    {
        Result<int>.Failure(TestError).ShouldHaveCorrelationId("corr");
    }

    [Fact]
    public void ShouldHaveCorrelationId_ResultT_ThrowsWhenDifferent()
    {
        Assert.Throws<ResultAssertionException>(() => Result<int>.Failure(TestError).ShouldHaveCorrelationId("other"));
    }

    [Fact]
    public void ShouldHaveDescription_Result_Passes()
    {
        Result.Failure(TestError).ShouldHaveDescription("ValDesc");
    }

    [Fact]
    public void ShouldHaveDescription_Result_ThrowsWhenDifferent()
    {
        Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveDescription("Other"));
    }

    [Fact]
    public void ShouldHaveDescription_ResultT_Passes()
    {
        Result<int>.Failure(TestError).ShouldHaveDescription("ValDesc");
    }

    [Fact]
    public void ShouldHaveDescription_ResultT_ThrowsWhenDifferent()
    {
        Assert.Throws<ResultAssertionException>(() => Result<int>.Failure(TestError).ShouldHaveDescription("Other"));
    }

    [Fact]
    public void ShouldContainInnerError_Result_Passes()
    {
        var err = Error.Failure("F", "D", Error.Validation("V", "D"));
        Result.Failure(err).ShouldContainInnerError("V");
    }

    [Fact]
    public void ShouldContainInnerError_Result_ThrowsWhenMissing()
    {
        var err = Error.Failure("F", "D", Error.Validation("V", "D"));
        Assert.Throws<ResultAssertionException>(() => Result.Failure(err).ShouldContainInnerError("Other"));
        Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldContainInnerError("Other"));
    }

    [Fact]
    public void ShouldContainInnerError_ResultT_Passes()
    {
        var err = Error.Failure("F", "D", Error.Validation("V", "D"));
        Result<int>.Failure(err).ShouldContainInnerError("V");
    }

    [Fact]
    public void ShouldContainInnerError_ResultT_ThrowsWhenMissing()
    {
        var err = Error.Failure("F", "D", Error.Validation("V", "D"));
        Assert.Throws<ResultAssertionException>(() => Result<int>.Failure(err).ShouldContainInnerError("Other"));
        Assert.Throws<ResultAssertionException>(() => Result<int>.Failure(TestError).ShouldContainInnerError("Other"));
    }


    [Fact]
    public async Task ShouldBeSuccessAsync_TaskResult_Passes()
    {
        var result = await Task.FromResult(Result.Success()).ShouldBeSuccessAsync();
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task ShouldBeSuccessAsync_TaskResultT_Passes()
    {
        var value = await Task.FromResult(Result<int>.Success(42)).ShouldBeSuccessAsync();
        Assert.Equal(42, value);
    }

    [Fact]
    public async Task ShouldBeFailureAsync_TaskResult_Passes()
    {
        var error = await Task.FromResult(Result.Failure(TestError)).ShouldBeFailureAsync();
        Assert.Equal(TestError, error);
    }

    [Fact]
    public async Task ShouldBeFailureAsync_TaskResultT_Passes()
    {
        var error = await Task.FromResult(Result<int>.Failure(TestError)).ShouldBeFailureAsync();
        Assert.Equal(TestError, error);
    }

    [Fact]
    public async Task ShouldBeSuccessAsync_ValueTaskResult_Passes()
    {
        var result = await new ValueTask<Result>(Result.Success()).ShouldBeSuccessAsync();
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task ShouldBeSuccessAsync_ValueTaskResultT_Passes()
    {
        var value = await new ValueTask<Result<int>>(Result<int>.Success(42)).ShouldBeSuccessAsync();
        Assert.Equal(42, value);
    }

    [Fact]
    public async Task ShouldBeFailureAsync_ValueTaskResult_Passes()
    {
        var error = await new ValueTask<Result>(Result.Failure(TestError)).ShouldBeFailureAsync();
        Assert.Equal(TestError, error);
    }

    [Fact]
    public async Task ShouldBeFailureAsync_ValueTaskResultT_Passes()
    {
        var error = await new ValueTask<Result<int>>(Result<int>.Failure(TestError)).ShouldBeFailureAsync();
        Assert.Equal(TestError, error);
    }


    [Fact]
    public void ShouldBeSuccess_ResultT_Throws_WithGenericTypeName()
    {
        var result = Result<Tuple<int, string>>.Failure(TestError);

        var ex = Assert.Throws<ResultAssertionException>(() => result.ShouldBeSuccess());

        Assert.Contains("Tuple<int, string>", ex.Message);
    }

    [Fact]
    public void ShouldBeFailure_ResultT_Throws_WithGenericTypeName()
    {
        var result = Result<Tuple<int, string>>.Success(Tuple.Create(1, "1"));

        var ex = Assert.Throws<ResultAssertionException>(() => result.ShouldBeFailure());

        Assert.Contains("Tuple<int, string>", ex.Message);
    }

    private static async ValueTask<Result> GetUncompletedValueTask(Result r)
    {
        await Task.Yield();
        return r;
    }

    private static async ValueTask<Result<T>> GetUncompletedValueTask<T>(Result<T> r)
    {
        await Task.Yield();
        return r;
    }

    private static async Task<Result> GetUncompletedTask(Result r)
    {
        await Task.Yield();
        return r;
    }

    private static async Task<Result<T>> GetUncompletedTask<T>(Result<T> r)
    {
        await Task.Yield();
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


    [Fact]
    public void Assertions_Negative_Coverage()
    {
        var s = Result.Success();
        var st = Result.Success(5);
        var f = Result.Failure(Error.Failure("X", "X"));
        var ft = Result.Failure<int>(Error.Failure("X", "X"));
        var u = default(Result);
        var ut = default(Result<int>);

        try { f.ShouldBeSuccess(); } catch { }
        try { u.ShouldBeSuccess(); } catch { }
        try { ft.ShouldBeSuccess(); } catch { }
        try { ut.ShouldBeSuccess(); } catch { }

        try { s.ShouldBeFailure(); } catch { }
        try { u.ShouldBeFailure(); } catch { }
        try { st.ShouldBeFailure(); } catch { }
        try { ut.ShouldBeFailure(); } catch { }

        try { s.ShouldBeUninitialized(); } catch { }
        try { f.ShouldBeUninitialized(); } catch { }
        try { st.ShouldBeUninitialized(); } catch { }
        try { ft.ShouldBeUninitialized(); } catch { }

        try { st.ShouldHaveValue(6); } catch { }
        try { ft.ShouldHaveValue(5); } catch { }

    }


    private static async ValueTask<Result> SlowValueFailure(Error e) { await Task.Yield(); return Result.Failure(e); }

    [Fact]
    public async Task Cover_ShouldHaveErrorTypeAsync_Slow()
    {
        var e = Error.Failure("code", "msg");
        var vt = SlowValueFailure(e);
        await vt.ShouldHaveErrorTypeAsync(ErrorType.Failure);
    }
}






