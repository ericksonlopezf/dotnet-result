using System;
using System.Threading.Tasks;
using Xunit;
using EricksonLopez.Result.Testing;

namespace EricksonLopez.Result.Testing.Tests;

public class ResultAssertionsTests
{
    private static readonly Error TestError = Error.Create("ValCode", "ValDesc").WithType(ErrorType.Validation).WithMetadata("k", "v").WithCorrelationId("corr").WithTraceId("trace").Build();
    

    [Fact]
    public void ShouldBeSuccess_Result_ReturnsResult_WhenSuccess()
    {
        var result = Result.Success();

        var returned = result.ShouldBeSuccess();

        Assert.True(returned.IsSuccess);
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
        Assert.True(result.IsSuccess);
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
        Assert.True(result.IsSuccess);
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
}

