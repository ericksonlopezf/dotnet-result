// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;
using EricksonLopez.Result.Generic;
using Xunit;

namespace EricksonLopez.Result.Generic.Tests;

public class ResultGenericTests
{
    [Fact]
    public void Success_CreatesSuccessResult_WithExpectedProperties()
    {
        var result = Result<int, CustomDomainError>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);

        var ex = Assert.Throws<InvalidOperationException>(() => _ = result.Error);
        Assert.Equal("Cannot access Error on a success result.", ex.Message);
    }

    [Fact]
    public void Success_WithNullReferenceValue_BehavesCorrectly()
    {
        var result = Result<string?, CustomDomainError>.Success(null);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
        Assert.True(result.TryGetValue(out var val));
        Assert.Null(val);
        Assert.Equal("Success()", result.ToString());
        Assert.Equal(HashCode.Combine(true, 0), result.GetHashCode());
    }

    [Fact]
    public void Failure_CreatesFailureResult_WithExpectedProperties()
    {
        var err = new CustomDomainError("Unauthorized", 401);
        var result = Result<int, CustomDomainError>.Failure(err);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Same(err, result.Error);

        var ex = Assert.Throws<InvalidOperationException>(() => _ = result.Value);
        Assert.Contains("Cannot access Value on a failure result. Error:", ex.Message);
        Assert.Contains(err.ToString(), ex.Message);
    }

    [Fact]
    public void Failure_WithNullError_ThrowsArgumentNullException()
    {
        CustomDomainError nullErr = null!;
        Assert.Throws<ArgumentNullException>("error", () => Result<int, CustomDomainError>.Failure(nullErr));
    }

    [Fact]
    public void TryGetValue_WhenSuccess_ReturnsTrueAndSetsValue()
    {
        var result = Result<string, CustomDomainError>.Success("hello");

        Assert.True(result.TryGetValue(out var val));
        Assert.Equal("hello", val);
    }

    [Fact]
    public void TryGetValue_WhenFailure_ReturnsFalseAndSetsDefault()
    {
        var err = new CustomDomainError("Fail", 500);
        var result = Result<int, CustomDomainError>.Failure(err);

        Assert.False(result.TryGetValue(out var val));
        Assert.Equal(0, val);
    }

    [Fact]
    public void TryGetError_WhenSuccess_ReturnsFalseAndSetsNull()
    {
        var result = Result<int, CustomDomainError>.Success(100);

        Assert.False(result.TryGetError(out var error));
        Assert.Null(error);
    }

    [Fact]
    public void TryGetError_WhenFailure_ReturnsTrueAndSetsError()
    {
        var err = new CustomDomainError("Fail", 500);
        var result = Result<string, CustomDomainError>.Failure(err);

        Assert.True(result.TryGetError(out var error));
        Assert.Same(err, error);
    }

    [Fact]
    public void Map_WhenSuccess_TransformsValue()
    {
        var result = Result<int, CustomDomainError>.Success(10);
        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsSuccess);
        Assert.Equal(20, mapped.Value);
    }

    [Fact]
    public void Map_WhenFailure_PreservesErrorWithoutInvokingMapper()
    {
        var err = new CustomDomainError("Err", 500);
        var result = Result<int, CustomDomainError>.Failure(err);
        bool invoked = false;

        var mapped = result.Map(x =>
        {
            invoked = true;
            return x.ToString();
        });

        Assert.False(invoked);
        Assert.True(mapped.IsFailure);
        Assert.Same(err, mapped.Error);
    }

    [Fact]
    public void Map_WithNullMapper_ThrowsArgumentNullException()
    {
        var result = Result<int, CustomDomainError>.Success(10);
        Assert.Throws<ArgumentNullException>("mapper", () => result.Map<string>(null!));
    }

    [Fact]
    public void MapError_WhenFailure_TransformsError()
    {
        var err = new CustomDomainError("Initial", 1);
        var result = Result<int, CustomDomainError>.Failure(err);
        var mapped = result.MapError(e => new AnotherDomainError($"Transformed: {e.Reason}"));

        Assert.True(mapped.IsFailure);
        Assert.Equal("Transformed: Initial", mapped.Error.Detail);
    }

    [Fact]
    public void MapError_WhenSuccess_PreservesValueWithoutInvokingErrorMapper()
    {
        var result = Result<int, CustomDomainError>.Success(42);
        bool invoked = false;

        var mapped = result.MapError(e =>
        {
            invoked = true;
            return new AnotherDomainError(e.Reason);
        });

        Assert.False(invoked);
        Assert.True(mapped.IsSuccess);
        Assert.Equal(42, mapped.Value);
    }

    [Fact]
    public void MapError_WithNullErrorMapper_ThrowsArgumentNullException()
    {
        var result = Result<int, CustomDomainError>.Failure(new CustomDomainError("Err", 500));
        Assert.Throws<ArgumentNullException>("errorMapper", () => result.MapError<AnotherDomainError>(null!));
    }

    [Fact]
    public void Bind_WhenSuccess_ChainsResult()
    {
        var result = Result<int, CustomDomainError>.Success(10);
        var bound = result.Bind(x => Result<string, CustomDomainError>.Success(x.ToString()));

        Assert.True(bound.IsSuccess);
        Assert.Equal("10", bound.Value);
    }

    [Fact]
    public void Bind_WhenFailure_PreservesErrorWithoutInvokingBind()
    {
        var err = new CustomDomainError("Err", 500);
        var result = Result<int, CustomDomainError>.Failure(err);
        bool invoked = false;

        var bound = result.Bind(x =>
        {
            invoked = true;
            return Result<string, CustomDomainError>.Success(x.ToString());
        });

        Assert.False(invoked);
        Assert.True(bound.IsFailure);
        Assert.Same(err, bound.Error);
    }

    [Fact]
    public void Bind_WithNullBindFunc_ThrowsArgumentNullException()
    {
        var result = Result<int, CustomDomainError>.Success(10);
        Assert.Throws<ArgumentNullException>("bind", () => result.Bind<string>(null!));
    }

    [Fact]
    public void Match_WhenSuccess_ExecutesOnSuccessBranch()
    {
        var success = Result<int, CustomDomainError>.Success(42);
        var sMatch = success.Match(v => $"Success: {v}", e => $"Error: {e.Reason}");

        Assert.Equal("Success: 42", sMatch);
    }

    [Fact]
    public void Match_WhenFailure_ExecutesOnFailureBranch()
    {
        var fail = Result<int, CustomDomainError>.Failure(new CustomDomainError("Bad", 400));
        var fMatch = fail.Match(v => $"Success: {v}", e => $"Error: {e.Reason}");

        Assert.Equal("Error: Bad", fMatch);
    }

    [Fact]
    public void Match_WithNullDelegates_ThrowsArgumentNullException()
    {
        var success = Result<int, CustomDomainError>.Success(42);

        Assert.Throws<ArgumentNullException>("onSuccess", () => success.Match<string>(null!, e => e.Reason));
        Assert.Throws<ArgumentNullException>("onFailure", () => success.Match<string>(v => v.ToString(), null!));
    }

    [Fact]
    public void ToResult_WhenSuccess_ConvertsToCoreSuccessResult()
    {
        var success = Result<int, CustomDomainError>.Success(42);
        var coreSuccess = success.ToResult(e => Error.Failure("Code", e.Reason));

        Assert.True(coreSuccess.IsSuccess);
        Assert.Equal(42, coreSuccess.Value);
    }

    [Fact]
    public void ToResult_WhenFailure_ConvertsToCoreFailureResult()
    {
        var fail = Result<int, CustomDomainError>.Failure(new CustomDomainError("Timeout", 504));
        var coreFail = fail.ToResult(e => Error.Failure("Timeout.Code", e.Reason));

        Assert.True(coreFail.IsFailure);
        Assert.Equal("Timeout.Code", coreFail.Error.Code);
        Assert.Equal("Timeout", coreFail.Error.Description);
    }

    [Fact]
    public void ToResult_WithNullErrorMapper_ThrowsArgumentNullException()
    {
        var result = Result<int, CustomDomainError>.Success(42);
        Assert.Throws<ArgumentNullException>("errorMapper", () => result.ToResult(null!));
    }

    [Fact]
    public void ImplicitOperator_FromValue_CreatesSuccessResult()
    {
        Result<int, CustomDomainError> result = 99;

        Assert.True(result.IsSuccess);
        Assert.Equal(99, result.Value);
    }

    [Fact]
    public void ImplicitOperator_FromError_CreatesFailureResult()
    {
        var err = new CustomDomainError("Denied", 403);
        Result<int, CustomDomainError> result = err;

        Assert.True(result.IsFailure);
        Assert.Same(err, result.Error);
    }

    [Fact]
    public void ImplicitOperator_FromNullError_ThrowsArgumentNullException()
    {
        CustomDomainError nullErr = null!;
        Assert.Throws<ArgumentNullException>("error", () =>
        {
            Result<int, CustomDomainError> _ = nullErr;
        });
    }

    [Fact]
    public void Equality_AndOperators_WorkCorrectly()
    {
        var s1 = Result<int, CustomDomainError>.Success(42);
        var s2 = Result<int, CustomDomainError>.Success(42);
        var s3 = Result<int, CustomDomainError>.Success(99);

        var err1 = new CustomDomainError("Bad", 400);
        var err2 = new CustomDomainError("Bad", 400);
        var err3 = new CustomDomainError("Other", 500);

        var f1 = Result<int, CustomDomainError>.Failure(err1);
        var f2 = Result<int, CustomDomainError>.Failure(err2);
        var f3 = Result<int, CustomDomainError>.Failure(err3);

        // Success equality
        Assert.True(s1.Equals(s2));
        Assert.True(s1 == s2);
        Assert.False(s1 != s2);
        Assert.True(s1.Equals((object)s2));

        // Success inequality
        Assert.False(s1.Equals(s3));
        Assert.False(s1 == s3);
        Assert.True(s1 != s3);

        // Failure equality
        Assert.True(f1.Equals(f2));
        Assert.True(f1 == f2);
        Assert.False(f1 != f2);

        // Failure inequality
        Assert.False(f1.Equals(f3));
        Assert.False(f1 == f3);
        Assert.True(f1 != f3);

        // Success vs Failure
        Assert.False(s1.Equals(f1));
        Assert.False(s1 == f1);
        Assert.True(s1 != f1);
        Assert.False(f1.Equals(s1));

        // Object equality with invalid types / null
        Assert.False(s1.Equals((object?)null));
        Assert.False(s1.Equals("not-a-result"));
        Assert.False(f1.Equals((object?)null));
        Assert.False(f1.Equals(42));
    }

    [Fact]
    public void GetHashCode_ReturnsConsistentHashCode()
    {
        var s1 = Result<int, CustomDomainError>.Success(42);
        var s2 = Result<int, CustomDomainError>.Success(42);
        var s3 = Result<int, CustomDomainError>.Success(99);

        Assert.Equal(s1.GetHashCode(), s2.GetHashCode());
        Assert.NotEqual(s1.GetHashCode(), s3.GetHashCode());
        Assert.Equal(HashCode.Combine(true, 42.GetHashCode()), s1.GetHashCode());

        var sNull1 = Result<string?, CustomDomainError>.Success(null);
        var sNull2 = Result<string?, CustomDomainError>.Success(null);
        Assert.Equal(sNull1.GetHashCode(), sNull2.GetHashCode());
        Assert.Equal(HashCode.Combine(true, 0), sNull1.GetHashCode());

        var err1 = new CustomDomainError("Bad", 400);
        var err2 = new CustomDomainError("Bad", 400);
        var err3 = new CustomDomainError("Different", 500);
        var f1 = Result<int, CustomDomainError>.Failure(err1);
        var f2 = Result<int, CustomDomainError>.Failure(err2);
        var f3 = Result<int, CustomDomainError>.Failure(err3);

        Assert.Equal(f1.GetHashCode(), f2.GetHashCode());
        Assert.NotEqual(f1.GetHashCode(), f3.GetHashCode());
        Assert.NotEqual(s1.GetHashCode(), f1.GetHashCode());
        Assert.Equal(HashCode.Combine(false, err1.GetHashCode()), f1.GetHashCode());

        var def1 = default(Result<int, CustomDomainError>);
        var def2 = default(Result<int, CustomDomainError>);
        Assert.Equal(def1.GetHashCode(), def2.GetHashCode());
        Assert.Equal(HashCode.Combine(false, 0), def1.GetHashCode());
        Assert.NotEqual(def1.GetHashCode(), f1.GetHashCode());
        Assert.NotEqual(def1.GetHashCode(), s1.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsExpectedRepresentation()
    {
        var s = Result<int, CustomDomainError>.Success(42);
        Assert.Equal("Success(42)", s.ToString());

        var sNull = Result<string?, CustomDomainError>.Success(null);
        Assert.Equal("Success()", sNull.ToString());

        var err = new CustomDomainError("NotFound", 404);
        var f = Result<int, CustomDomainError>.Failure(err);
        Assert.Equal($"Failure({err})", f.ToString());

        var def = default(Result<int, CustomDomainError>);
        Assert.Equal("Failure()", def.ToString());
    }
}
