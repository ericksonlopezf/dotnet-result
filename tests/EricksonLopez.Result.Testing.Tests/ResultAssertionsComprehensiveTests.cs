// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Testing.Tests;

public class ResultAssertionsComprehensiveTests
{
    private static readonly Error TestError = Error.Create("Code1", "Desc1")
        .WithType(ErrorType.Validation)
        .WithSeverity(ErrorSeverity.Warning)
        .WithRetryability(ErrorRetryability.Transient)
        .WithMetadata("k", "v")
        .WithCorrelationId("corr")
        .WithTraceId("trace")
        .Build();

    private static readonly Error TestError2 = Error.Create("Code2", "Desc2")
        .WithType(ErrorType.Failure)
        .WithSeverity(ErrorSeverity.Error)
        .WithRetryability(ErrorRetryability.Permanent)
        .Build();

    private static readonly Error CombinedError = Error.Create("Result.CombinedErrors", "Combined error")
        .WithInnerErrors(new[] { TestError, TestError2 })
        .Build();

    private static async Task<Result> DelayResult(Result r)
    {
        await Task.Yield();
        return r;
    }

    private static async Task<Result<T>> DelayResult<T>(Result<T> r)
    {
        await Task.Yield();
        return r;
    }

    private static async ValueTask<Result> DelayValueResult(Result r)
    {
        await Task.Yield();
        return r;
    }

    private static async ValueTask<Result<T>> DelayValueResult<T>(Result<T> r)
    {
        await Task.Yield();
        return r;
    }

    // --- Synchronous Result and Result<T> Assertions (Positive & Negative) ---

    [Fact]
    public void ShouldBeSuccess_CustomMessage_And_Uninitialized()
    {
        Result.Success().ShouldBeSuccess("custom");
        Result.Success(123).ShouldBeSuccess("custom");

        var ex1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldBeSuccess("custom failure msg"));
        Assert.Equal("custom failure msg", ex1.Message);

        var ex2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldBeSuccess("custom generic failure msg"));
        Assert.Equal("custom generic failure msg", ex2.Message);

        Result uninit = default;
        var ex3 = Assert.Throws<ResultAssertionException>(() => uninit.ShouldBeSuccess());
        Assert.Contains("uninitialized", ex3.Message);

        Result<int> uninitT = default;
        var ex4 = Assert.Throws<ResultAssertionException>(() => uninitT.ShouldBeSuccess());
        Assert.Contains("uninitialized", ex4.Message);
    }

    [Fact]
    public void ShouldHaveValue_EdgeCases()
    {
        Result.Success(42).ShouldHaveValue(42, "msg");

        var ex1 = Assert.Throws<ResultAssertionException>(() => Result.Success(42).ShouldHaveValue(99, "custom diff"));
        Assert.Equal("custom diff", ex1.Message);

        var ex2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldHaveValue(42));
        Assert.Contains("Expected Result<int> to be Success", ex2.Message);

        Result<int> uninit = default;
        var ex3 = Assert.Throws<ResultAssertionException>(() => uninit.ShouldHaveValue(42));
        Assert.Contains("uninitialized", ex3.Message);
    }

    [Fact]
    public void ShouldBeFailure_EdgeCases()
    {
        Result.Failure(TestError).ShouldBeFailure("msg");
        Result.Failure<int>(TestError).ShouldBeFailure("msg");

        var ex1 = Assert.Throws<ResultAssertionException>(() => Result.Success().ShouldBeFailure("custom success msg"));
        Assert.Equal("custom success msg", ex1.Message);

        var ex2 = Assert.Throws<ResultAssertionException>(() => Result.Success(42).ShouldBeFailure("custom success generic msg"));
        Assert.Equal("custom success generic msg", ex2.Message);

        Result uninit = default;
        var ex3 = Assert.Throws<ResultAssertionException>(() => uninit.ShouldBeFailure());
        Assert.Contains("uninitialized", ex3.Message);

        Result<int> uninitT = default;
        var ex4 = Assert.Throws<ResultAssertionException>(() => uninitT.ShouldBeFailure());
        Assert.Contains("uninitialized", ex4.Message);
    }

    [Fact]
    public void ShouldHaveErrorCode_And_ErrorType_And_Severity()
    {
        Result.Failure(TestError).ShouldHaveErrorCode("Code1");
        Result.Failure<int>(TestError).ShouldHaveErrorCode("Code1");

        var ex1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveErrorCode("Diff"));
        Assert.Contains("Expected error code 'Diff'", ex1.Message);

        var ex2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldHaveErrorCode("Diff"));
        Assert.Contains("Expected error code 'Diff'", ex2.Message);

        Result.Failure(TestError).ShouldHaveErrorType(ErrorType.Validation);
        Result.Failure<int>(TestError).ShouldHaveErrorType(ErrorType.Validation);

        var ex3 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveErrorType(ErrorType.Conflict));
        Assert.Contains("Expected ErrorType 'Conflict'", ex3.Message);

        var ex4 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldHaveErrorType(ErrorType.Conflict));
        Assert.Contains("Expected ErrorType 'Conflict'", ex4.Message);

        Result.Failure(TestError).ShouldHaveSeverity(ErrorSeverity.Warning);
        Result.Failure<int>(TestError).ShouldHaveSeverity(ErrorSeverity.Warning);

        var ex5 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveSeverity(ErrorSeverity.Critical));
        Assert.Contains("Expected ErrorSeverity 'Critical'", ex5.Message);

        var ex6 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldHaveSeverity(ErrorSeverity.Critical));
        Assert.Contains("Expected ErrorSeverity 'Critical'", ex6.Message);
    }

    [Fact]
    public void ShouldHaveRetryability_Retryable_Permanent()
    {
        Result.Failure(TestError).ShouldBeRetryable();
        Result.Failure<int>(TestError).ShouldBeRetryable();

        var ex3 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError2).ShouldBeRetryable());
        Assert.Contains("Transient", ex3.Message);

        var ex4 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError2).ShouldBeRetryable());
        Assert.Contains("Transient", ex4.Message);

        Result.Failure(TestError2).ShouldBePermanent();
        Result.Failure<int>(TestError2).ShouldBePermanent();

        var ex5 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldBePermanent());
        Assert.Contains("Permanent", ex5.Message);

        var ex6 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldBePermanent());
        Assert.Contains("Permanent", ex6.Message);
    }

    [Fact]
    public void ShouldHaveDescription_TraceId_CorrelationId()
    {
        Result.Failure(TestError).ShouldHaveDescription("Desc1");
        Result.Failure<int>(TestError).ShouldHaveDescription("Desc1");

        var ex1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveDescription("Diff"));
        Assert.Contains("Expected error Description to be 'Diff'", ex1.Message);

        var ex2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldHaveDescription("Diff"));
        Assert.Contains("Expected error Description to be 'Diff'", ex2.Message);

        Result.Failure(TestError).ShouldHaveTraceId("trace");
        Result.Failure<int>(TestError).ShouldHaveTraceId("trace");

        var ex3 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveTraceId("Diff"));
        Assert.Contains("Expected error TraceId to be 'Diff', but got 'trace'", ex3.Message);

        var ex4 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldHaveTraceId("Diff"));
        Assert.Contains("Expected error TraceId to be 'Diff', but got 'trace'", ex4.Message);

        var exNullTrace1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError2).ShouldHaveTraceId("trace"));
        Assert.Contains("Expected error TraceId to be 'trace', but got '<null>'", exNullTrace1.Message);

        var exNullTrace2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError2).ShouldHaveTraceId("trace"));
        Assert.Contains("Expected error TraceId to be 'trace', but got '<null>'", exNullTrace2.Message);

        Result.Failure(TestError).ShouldHaveCorrelationId("corr");
        Result.Failure<int>(TestError).ShouldHaveCorrelationId("corr");

        var ex5 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveCorrelationId("Diff"));
        Assert.Contains("Expected error CorrelationId to be 'Diff', but got 'corr'", ex5.Message);

        var ex6 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldHaveCorrelationId("Diff"));
        Assert.Contains("Expected error CorrelationId to be 'Diff', but got 'corr'", ex6.Message);

        var exNullCorr1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError2).ShouldHaveCorrelationId("corr"));
        Assert.Contains("Expected error CorrelationId to be 'corr', but got '<null>'", exNullCorr1.Message);

        var exNullCorr2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError2).ShouldHaveCorrelationId("corr"));
        Assert.Contains("Expected error CorrelationId to be 'corr', but got '<null>'", exNullCorr2.Message);
    }

    [Fact]
    public void ShouldHaveMetadata_MetadataKey_NotHaveMetadata_MetadataValue()
    {
        Result.Failure(TestError).ShouldHaveMetadata("k", "v");
        Result.Failure<int>(TestError).ShouldHaveMetadata("k", "v");

        var ex1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveMetadata("k", "diff"));
        Assert.Contains("Expected metadata key 'k' with value 'diff'", ex1.Message);

        var ex2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldHaveMetadata("k", "diff"));
        Assert.Contains("Expected metadata key 'k' with value 'diff'", ex2.Message);

        var exNoMeta1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError2).ShouldHaveMetadata("k", "v"));
        Assert.Equal("Expected metadata key 'k' with value 'v', but got ''.", exNoMeta1.Message);

        var exNoMeta2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError2).ShouldHaveMetadata("k", "v"));
        Assert.Equal("Expected metadata key 'k' with value 'v', but got ''.", exNoMeta2.Message);


        Result.Failure(TestError).ShouldHaveMetadataKey("k");
        Result.Failure<int>(TestError).ShouldHaveMetadataKey("k");

        Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveMetadataKey("missing"));
        Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldHaveMetadataKey("missing"));
        Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError2).ShouldHaveMetadataKey("k"));
        Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError2).ShouldHaveMetadataKey("k"));

        Result.Failure(TestError).ShouldNotHaveMetadata("missing");
        Result.Failure<int>(TestError).ShouldNotHaveMetadata("missing");
        Result.Failure(TestError2).ShouldNotHaveMetadata("k");
        Result.Failure<int>(TestError2).ShouldNotHaveMetadata("k");

        Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldNotHaveMetadata("k"));
        Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldNotHaveMetadata("k"));

        Result.Failure(TestError).ShouldHaveMetadataValue("k", "v", "msg");
        Result.Failure<int>(TestError).ShouldHaveMetadataValue("k", "v", "msg");

        var exMissMeta1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveMetadataValue("missing", "v", "custom miss meta msg"));
        Assert.Equal("custom miss meta msg", exMissMeta1.Message);

        var exMissMeta2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldHaveMetadataValue("missing", "v", "custom generic miss meta msg"));
        Assert.Equal("custom generic miss meta msg", exMissMeta2.Message);

        var exNoMetaVal1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError2).ShouldHaveMetadataValue("k", "v"));
        Assert.Contains("Expected metadata key 'k' to be present, but it was not found.", exNoMetaVal1.Message);

        var exNoMetaVal2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError2).ShouldHaveMetadataValue("k", "v"));
        Assert.Contains("Expected metadata key 'k' to be present, but it was not found.", exNoMetaVal2.Message);
    }

    [Fact]
    public void ShouldHaveInnerErrors_InnerErrorCount_ContainInnerError_CombinedFailure()
    {
        Result.Failure(CombinedError).ShouldHaveInnerErrors(2);
        Result.Failure<int>(CombinedError).ShouldHaveInnerErrors(2);

        var ex1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveInnerErrors(2));
        Assert.Contains("Expected 2 inner errors", ex1.Message);

        var ex2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldHaveInnerErrors(2));
        Assert.Contains("Expected 2 inner errors", ex2.Message);

        Result.Failure(TestError).ShouldHaveNoInnerErrors("msg");
        Result.Failure<int>(TestError).ShouldHaveNoInnerErrors("msg");
        Result.Failure(TestError).ShouldNotHaveInnerErrors("msg");
        Result.Failure<int>(TestError).ShouldNotHaveInnerErrors("msg");

        var ex3 = Assert.Throws<ResultAssertionException>(() => Result.Failure(CombinedError).ShouldHaveNoInnerErrors("custom has inners msg"));
        Assert.Equal("custom has inners msg", ex3.Message);

        var ex4 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(CombinedError).ShouldHaveNoInnerErrors("custom generic has inners msg"));
        Assert.Equal("custom generic has inners msg", ex4.Message);

        var ex5 = Assert.Throws<ResultAssertionException>(() => Result.Failure(CombinedError).ShouldNotHaveInnerErrors("custom not inners msg"));
        Assert.Equal("custom not inners msg", ex5.Message);

        var ex6 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(CombinedError).ShouldNotHaveInnerErrors("custom generic not inners msg"));
        Assert.Equal("custom generic not inners msg", ex6.Message);

        Result.Failure(CombinedError).ShouldHaveInnerErrorCount(2, "msg");
        Result.Failure<int>(CombinedError).ShouldHaveInnerErrorCount(2, "msg");

        var exCount1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(CombinedError).ShouldHaveInnerErrorCount(3, "custom count msg"));
        Assert.Equal("custom count msg", exCount1.Message);

        var exCount2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(CombinedError).ShouldHaveInnerErrorCount(3, "custom generic count msg"));
        Assert.Equal("custom generic count msg", exCount2.Message);

        Result.Failure(CombinedError).ShouldContainInnerError("Code1");
        Result.Failure<int>(CombinedError).ShouldContainInnerError("Code1");

        var exNoMatch1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(CombinedError).ShouldContainInnerError("NotFound"));
        Assert.Contains("Expected at least one inner error with code 'NotFound'", exNoMatch1.Message);

        var exNoMatch2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(CombinedError).ShouldContainInnerError("NotFound"));
        Assert.Contains("Expected at least one inner error with code 'NotFound'", exNoMatch2.Message);

        var exNoInnerMatch1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldContainInnerError("Code1"));
        Assert.Equal("Expected at least one inner error with code 'Code1', but error has no inner errors.", exNoInnerMatch1.Message);

        var exNoInnerMatch2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldContainInnerError("Code1"));
        Assert.Equal("Expected at least one inner error with code 'Code1', but error has no inner errors.", exNoInnerMatch2.Message);


        Result.Failure(CombinedError).ShouldBeCombinedFailure(2, "msg");
        Result.Failure<int>(CombinedError).ShouldBeCombinedFailure(2, "msg");

        var exComb1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(CombinedError).ShouldBeCombinedFailure(3, "custom comb msg"));
        Assert.Equal("custom comb msg", exComb1.Message);

        var exComb2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(CombinedError).ShouldBeCombinedFailure(3, "custom generic comb msg"));
        Assert.Equal("custom generic comb msg", exComb2.Message);

        Result.Failure(CombinedError).ShouldHaveInnerErrorsMatching(errs => errs.Length == 2, "msg");
        Result.Failure<int>(CombinedError).ShouldHaveInnerErrorsMatching(errs => errs.Length == 2, "msg");

        var exMatchPred1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(CombinedError).ShouldHaveInnerErrorsMatching(errs => errs.Length == 5, "custom inners match msg"));
        Assert.Equal("custom inners match msg", exMatchPred1.Message);

        var exMatchPred2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(CombinedError).ShouldHaveInnerErrorsMatching(errs => errs.Length == 5, "custom generic inners match msg"));
        Assert.Equal("custom generic inners match msg", exMatchPred2.Message);
    }

    [Fact]
    public void ShouldStrictlyEqual_ShouldBeUninitialized_ShouldSatisfy()
    {
        Result.Failure(TestError).ShouldStrictlyEqual(TestError);
        Result.Failure<int>(TestError).ShouldStrictlyEqual(TestError);

        var ex1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldStrictlyEqual(TestError2, "custom strict msg"));
        Assert.Equal("custom strict msg", ex1.Message);

        var ex2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldStrictlyEqual(TestError2, "custom generic strict msg"));
        Assert.Equal("custom generic strict msg", ex2.Message);

        Result uninit = default;
        uninit.ShouldBeUninitialized("msg");

        Result<int> uninitT = default;
        uninitT.ShouldBeUninitialized("msg");

        var ex3 = Assert.Throws<ResultAssertionException>(() => Result.Success().ShouldBeUninitialized("custom uninit msg"));
        Assert.Equal("custom uninit msg", ex3.Message);

        var ex4 = Assert.Throws<ResultAssertionException>(() => Result.Success(10).ShouldBeUninitialized("custom generic uninit msg"));
        Assert.Equal("custom generic uninit msg", ex4.Message);

        bool satisfied = false;
        Result.Success().ShouldSatisfy(r => satisfied = true, "msg");
        Assert.True(satisfied);

        Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldSatisfy(r => { }));

        int satisfiedVal = 0;
        Result.Success(42).ShouldSatisfy(v => satisfiedVal = v, "msg");
        Assert.Equal(42, satisfiedVal);

        bool satisfiedErr = false;
        Result.Failure(TestError).ShouldSatisfyError(e => satisfiedErr = true, "msg");
        Assert.True(satisfiedErr);

        satisfiedErr = false;
        Result.Failure<int>(TestError).ShouldSatisfyError(e => satisfiedErr = true, "msg");
        Assert.True(satisfiedErr);

        Result.Failure(TestError).ShouldHaveErrorMatching(e => e.Code == "Code1", "msg");
        Result.Failure<int>(TestError).ShouldHaveErrorMatching(e => e.Code == "Code1", "msg");

        var exMatch1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldHaveErrorMatching(e => e.Code == "Mismatch", "custom match msg"));
        Assert.Equal("custom match msg", exMatch1.Message);

        var exMatch2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldHaveErrorMatching(e => e.Code == "Mismatch", "custom generic match msg"));
        Assert.Equal("custom generic match msg", exMatch2.Message);

        Result.Failure(CombinedError).ShouldHaveErrorCount(2, "msg");
        Result.Failure<int>(CombinedError).ShouldHaveErrorCount(2, "msg");
        Result.Failure(CombinedError).ShouldHaveErrorCount(2);
        Result.Failure<int>(CombinedError).ShouldHaveErrorCount(2);

        var exCount1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(CombinedError).ShouldHaveErrorCount(3, "custom count msg"));
        Assert.Equal("custom count msg", exCount1.Message);

        var exCount2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(CombinedError).ShouldHaveErrorCount(3, "custom generic count msg"));
        Assert.Equal("custom generic count msg", exCount2.Message);

        var exCount3 = Assert.Throws<ResultAssertionException>(() => Result.Failure(CombinedError).ShouldHaveErrorCount(3));
        Assert.Contains("Expected the result error to have 3 inner error(s), but found 2.", exCount3.Message);

        var exCount4 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(CombinedError).ShouldHaveErrorCount(3));
        Assert.Contains("Expected the Result<int> error to have 3 inner error(s), but found 2.", exCount4.Message);
    }

    // --- Task Async Assertions (Fast Path & Slow Path) ---

    [Fact]
    public async Task Task_Async_Assertions_Fast_And_SlowPath()
    {
        // Fast Path Task
        var s1 = await Task.FromResult(Result.Success()).ShouldBeSuccessAsync();
        s1.ShouldBeSuccess();
        var s2 = await Task.FromResult(Result.Success(42)).ShouldBeSuccessAsync();
        Assert.Equal(42, s2);
        var v1 = await Task.FromResult(Result.Success(42)).ShouldHaveValueAsync(42);
        Assert.Equal(42, v1);
        var e1 = await Task.FromResult(Result.Failure(TestError)).ShouldBeFailureAsync();
        Assert.Equal(TestError, e1);
        var e2 = await Task.FromResult(Result.Failure<int>(TestError)).ShouldBeFailureAsync();
        Assert.Equal(TestError, e2);
        var e3 = await Task.FromResult(Result.Failure(TestError)).ShouldHaveErrorCodeAsync("Code1");
        Assert.Equal("Code1", e3.Code);
        var e4 = await Task.FromResult(Result.Failure<int>(TestError)).ShouldHaveErrorCodeAsync("Code1");
        Assert.Equal("Code1", e4.Code);
        var e5 = await Task.FromResult(Result.Failure(TestError)).ShouldHaveErrorTypeAsync(ErrorType.Validation);
        Assert.Equal(ErrorType.Validation, e5.Type);
        var e6 = await Task.FromResult(Result.Failure<int>(TestError)).ShouldHaveErrorTypeAsync(ErrorType.Validation);
        Assert.Equal(ErrorType.Validation, e6.Type);
        var e7 = await Task.FromResult(Result.Failure(TestError)).ShouldHaveSeverityAsync(ErrorSeverity.Warning);
        Assert.Equal(ErrorSeverity.Warning, e7.Severity);
        var e8 = await Task.FromResult(Result.Failure<int>(TestError)).ShouldHaveSeverityAsync(ErrorSeverity.Warning);
        Assert.Equal(ErrorSeverity.Warning, e8.Severity);
        var e9 = await Task.FromResult(Result.Failure(TestError)).ShouldHaveDescriptionAsync("Desc1");
        Assert.Equal("Desc1", e9.Description);
        var e10 = await Task.FromResult(Result.Failure<int>(TestError)).ShouldHaveDescriptionAsync("Desc1");
        Assert.Equal("Desc1", e10.Description);
        var e11 = await Task.FromResult(Result.Failure(TestError)).ShouldHaveTraceIdAsync("trace");
        Assert.Equal("trace", e11.TraceId);
        var e12 = await Task.FromResult(Result.Failure<int>(TestError)).ShouldHaveTraceIdAsync("trace");
        Assert.Equal("trace", e12.TraceId);
        var e13 = await Task.FromResult(Result.Failure(TestError)).ShouldHaveCorrelationIdAsync("corr");
        Assert.Equal("corr", e13.CorrelationId);
        var e14 = await Task.FromResult(Result.Failure<int>(TestError)).ShouldHaveCorrelationIdAsync("corr");
        Assert.Equal("corr", e14.CorrelationId);
        var e15 = await Task.FromResult(Result.Failure(TestError)).ShouldHaveMetadataAsync("k", "v");
        Assert.Equal("v", e15.Metadata["k"]);
        var e16 = await Task.FromResult(Result.Failure<int>(TestError)).ShouldHaveMetadataAsync("k", "v");
        Assert.Equal("v", e16.Metadata["k"]);
        var e17 = await Task.FromResult(Result.Failure(TestError)).ShouldBeRetryableAsync();
        Assert.Equal(ErrorRetryability.Transient, e17.Retryability);
        var e18 = await Task.FromResult(Result.Failure<int>(TestError)).ShouldBeRetryableAsync();
        Assert.Equal(ErrorRetryability.Transient, e18.Retryability);
        var e19 = await Task.FromResult(Result.Failure(TestError2)).ShouldBePermanentAsync();
        Assert.Equal(ErrorRetryability.Permanent, e19.Retryability);
        var e20 = await Task.FromResult(Result.Failure<int>(TestError2)).ShouldBePermanentAsync();
        Assert.Equal(ErrorRetryability.Permanent, e20.Retryability);
        var e21 = await Task.FromResult(Result.Failure(CombinedError)).ShouldHaveInnerErrorsAsync(2);
        Assert.Equal(2, e21.InnerErrors.Length);
        var e22 = await Task.FromResult(Result.Failure<int>(CombinedError)).ShouldHaveInnerErrorsAsync(2);
        Assert.Equal(2, e22.InnerErrors.Length);
        var e23 = await Task.FromResult(Result.Failure(CombinedError)).ShouldContainInnerErrorAsync("Code1");
        Assert.Equal("Result.CombinedErrors", e23.Code);
        var e24 = await Task.FromResult(Result.Failure<int>(CombinedError)).ShouldContainInnerErrorAsync("Code1");
        Assert.Equal("Result.CombinedErrors", e24.Code);
        var e25 = await Task.FromResult(Result.Failure(CombinedError)).ShouldBeCombinedFailureAsync(2);
        Assert.Equal(2, e25.InnerErrors.Length);
        var e26 = await Task.FromResult(Result.Failure<int>(CombinedError)).ShouldBeCombinedFailureAsync(2);
        Assert.Equal(2, e26.InnerErrors.Length);
        var e27 = await Task.FromResult(Result.Failure(TestError)).ShouldNotHaveInnerErrorsAsync();
        Assert.Empty(e27.InnerErrors);
        var e28 = await Task.FromResult(Result.Failure<int>(TestError)).ShouldNotHaveInnerErrorsAsync();
        Assert.Empty(e28.InnerErrors);
        var e29 = await Task.FromResult(Result.Failure(CombinedError)).ShouldHaveInnerErrorCountAsync(2);
        Assert.Equal(2, e29.InnerErrors.Length);
        var e30 = await Task.FromResult(Result.Failure<int>(CombinedError)).ShouldHaveInnerErrorCountAsync(2);
        Assert.Equal(2, e30.InnerErrors.Length);
        var u1 = await Task.FromResult((Result)default).ShouldBeUninitializedAsync();
        Assert.True(u1.IsUninitialized);
        var u2 = await Task.FromResult((Result<int>)default).ShouldBeUninitializedAsync();
        Assert.True(u2.IsUninitialized);
        var sat1 = await Task.FromResult(Result.Success()).ShouldSatisfyAsync(r => { });
        sat1.ShouldBeSuccess();
        var sat2 = await Task.FromResult(Result.Success(42)).ShouldSatisfyAsync(v => { });
        Assert.Equal(42, sat2);
        var sat3 = await Task.FromResult(Result.Failure(TestError)).ShouldSatisfyErrorAsync(e => { });
        Assert.Equal(TestError, sat3);
        var sat4 = await Task.FromResult(Result.Failure<int>(TestError)).ShouldSatisfyErrorAsync(e => { });
        Assert.Equal(TestError, sat4);
        var em1 = await Task.FromResult(Result.Failure(TestError)).ShouldHaveErrorMatchingAsync(e => e.Code == "Code1");
        Assert.Equal(TestError, em1);
        var em2 = await Task.FromResult(Result.Failure<int>(TestError)).ShouldHaveErrorMatchingAsync(e => e.Code == "Code1");
        Assert.Equal(TestError, em2);

        // Slow Path Task
        var s1s = await DelayResult(Result.Success()).ShouldBeSuccessAsync();
        s1s.ShouldBeSuccess();
        var s2s = await DelayResult(Result.Success(42)).ShouldBeSuccessAsync();
        Assert.Equal(42, s2s);
        var v1s = await DelayResult(Result.Success(42)).ShouldHaveValueAsync(42);
        Assert.Equal(42, v1s);
        var e1s = await DelayResult(Result.Failure(TestError)).ShouldBeFailureAsync();
        Assert.Equal(TestError, e1s);
        var e2s = await DelayResult(Result.Failure<int>(TestError)).ShouldBeFailureAsync();
        Assert.Equal(TestError, e2s);
        var e3s = await DelayResult(Result.Failure(TestError)).ShouldHaveErrorCodeAsync("Code1");
        Assert.Equal("Code1", e3s.Code);
        var e4s = await DelayResult(Result.Failure<int>(TestError)).ShouldHaveErrorCodeAsync("Code1");
        Assert.Equal("Code1", e4s.Code);
        var e5s = await DelayResult(Result.Failure(TestError)).ShouldHaveErrorTypeAsync(ErrorType.Validation);
        Assert.Equal(ErrorType.Validation, e5s.Type);
        var e6s = await DelayResult(Result.Failure<int>(TestError)).ShouldHaveErrorTypeAsync(ErrorType.Validation);
        Assert.Equal(ErrorType.Validation, e6s.Type);
        var e7s = await DelayResult(Result.Failure(TestError)).ShouldHaveSeverityAsync(ErrorSeverity.Warning);
        Assert.Equal(ErrorSeverity.Warning, e7s.Severity);
        var e8s = await DelayResult(Result.Failure<int>(TestError)).ShouldHaveSeverityAsync(ErrorSeverity.Warning);
        Assert.Equal(ErrorSeverity.Warning, e8s.Severity);
        var e9s = await DelayResult(Result.Failure(TestError)).ShouldHaveDescriptionAsync("Desc1");
        Assert.Equal("Desc1", e9s.Description);
        var e10s = await DelayResult(Result.Failure<int>(TestError)).ShouldHaveDescriptionAsync("Desc1");
        Assert.Equal("Desc1", e10s.Description);
        var e11s = await DelayResult(Result.Failure(TestError)).ShouldHaveTraceIdAsync("trace");
        Assert.Equal("trace", e11s.TraceId);
        var e12s = await DelayResult(Result.Failure<int>(TestError)).ShouldHaveTraceIdAsync("trace");
        Assert.Equal("trace", e12s.TraceId);
        var e13s = await DelayResult(Result.Failure(TestError)).ShouldHaveCorrelationIdAsync("corr");
        Assert.Equal("corr", e13s.CorrelationId);
        var e14s = await DelayResult(Result.Failure<int>(TestError)).ShouldHaveCorrelationIdAsync("corr");
        Assert.Equal("corr", e14s.CorrelationId);
        var e15s = await DelayResult(Result.Failure(TestError)).ShouldHaveMetadataAsync("k", "v");
        Assert.Equal("v", e15s.Metadata["k"]);
        var e16s = await DelayResult(Result.Failure<int>(TestError)).ShouldHaveMetadataAsync("k", "v");
        Assert.Equal("v", e16s.Metadata["k"]);
        var e17s = await DelayResult(Result.Failure(TestError)).ShouldBeRetryableAsync();
        Assert.Equal(ErrorRetryability.Transient, e17s.Retryability);
        var e18s = await DelayResult(Result.Failure<int>(TestError)).ShouldBeRetryableAsync();
        Assert.Equal(ErrorRetryability.Transient, e18s.Retryability);
        var e19s = await DelayResult(Result.Failure(TestError2)).ShouldBePermanentAsync();
        Assert.Equal(ErrorRetryability.Permanent, e19s.Retryability);
        var e20s = await DelayResult(Result.Failure<int>(TestError2)).ShouldBePermanentAsync();
        Assert.Equal(ErrorRetryability.Permanent, e20s.Retryability);
        var e21s = await DelayResult(Result.Failure(CombinedError)).ShouldHaveInnerErrorsAsync(2);
        Assert.Equal(2, e21s.InnerErrors.Length);
        var e22s = await DelayResult(Result.Failure<int>(CombinedError)).ShouldHaveInnerErrorsAsync(2);
        Assert.Equal(2, e22s.InnerErrors.Length);
        var e23s = await DelayResult(Result.Failure(CombinedError)).ShouldContainInnerErrorAsync("Code1");
        Assert.Equal("Result.CombinedErrors", e23s.Code);
        var e24s = await DelayResult(Result.Failure<int>(CombinedError)).ShouldContainInnerErrorAsync("Code1");
        Assert.Equal("Result.CombinedErrors", e24s.Code);
        var e25s = await DelayResult(Result.Failure(CombinedError)).ShouldBeCombinedFailureAsync(2);
        Assert.Equal(2, e25s.InnerErrors.Length);
        var e26s = await DelayResult(Result.Failure<int>(CombinedError)).ShouldBeCombinedFailureAsync(2);
        Assert.Equal(2, e26s.InnerErrors.Length);
        var e27s = await DelayResult(Result.Failure(TestError)).ShouldNotHaveInnerErrorsAsync();
        Assert.Empty(e27s.InnerErrors);
        var e28s = await DelayResult(Result.Failure<int>(TestError)).ShouldNotHaveInnerErrorsAsync();
        Assert.Empty(e28s.InnerErrors);
        var e29s = await DelayResult(Result.Failure(CombinedError)).ShouldHaveInnerErrorCountAsync(2);
        Assert.Equal(2, e29s.InnerErrors.Length);
        var e30s = await DelayResult(Result.Failure<int>(CombinedError)).ShouldHaveInnerErrorCountAsync(2);
        Assert.Equal(2, e30s.InnerErrors.Length);
        var u1s = await DelayResult((Result)default).ShouldBeUninitializedAsync();
        Assert.True(u1s.IsUninitialized);
        var u2s = await DelayResult((Result<int>)default).ShouldBeUninitializedAsync();
        Assert.True(u2s.IsUninitialized);
        var sat1s = await DelayResult(Result.Success()).ShouldSatisfyAsync(r => { });
        sat1s.ShouldBeSuccess();
        var sat2s = await DelayResult(Result.Success(42)).ShouldSatisfyAsync(v => { });
        Assert.Equal(42, sat2s);
        var sat3s = await DelayResult(Result.Failure(TestError)).ShouldSatisfyErrorAsync(e => { });
        Assert.Equal(TestError, sat3s);
        var sat4s = await DelayResult(Result.Failure<int>(TestError)).ShouldSatisfyErrorAsync(e => { });
        Assert.Equal(TestError, sat4s);
        var em1s = await DelayResult(Result.Failure(TestError)).ShouldHaveErrorMatchingAsync(e => e.Code == "Code1");
        Assert.Equal(TestError, em1s);
        var em2s = await DelayResult(Result.Failure<int>(TestError)).ShouldHaveErrorMatchingAsync(e => e.Code == "Code1");
        Assert.Equal(TestError, em2s);
    }

    // --- ValueTask Async Assertions (Fast Path & Slow Path) ---

    [Fact]
    public async Task ValueTask_Async_Assertions_Fast_And_SlowPath()
    {
        // Fast Path ValueTask
        var s1 = await ValueTask.FromResult(Result.Success()).ShouldBeSuccessAsync();
        s1.ShouldBeSuccess();
        var s2 = await ValueTask.FromResult(Result.Success(42)).ShouldBeSuccessAsync();
        Assert.Equal(42, s2);
        var v1 = await ValueTask.FromResult(Result.Success(42)).ShouldHaveValueAsync(42);
        Assert.Equal(42, v1);
        var e1 = await ValueTask.FromResult(Result.Failure(TestError)).ShouldBeFailureAsync();
        Assert.Equal(TestError, e1);
        var e2 = await ValueTask.FromResult(Result.Failure<int>(TestError)).ShouldBeFailureAsync();
        Assert.Equal(TestError, e2);
        var e3 = await ValueTask.FromResult(Result.Failure(TestError)).ShouldHaveErrorCodeAsync("Code1");
        Assert.Equal("Code1", e3.Code);
        var e4 = await ValueTask.FromResult(Result.Failure<int>(TestError)).ShouldHaveErrorCodeAsync("Code1");
        Assert.Equal("Code1", e4.Code);
        var e5 = await ValueTask.FromResult(Result.Failure(TestError)).ShouldHaveErrorTypeAsync(ErrorType.Validation);
        Assert.Equal(ErrorType.Validation, e5.Type);
        var e6 = await ValueTask.FromResult(Result.Failure<int>(TestError)).ShouldHaveErrorTypeAsync(ErrorType.Validation);
        Assert.Equal(ErrorType.Validation, e6.Type);
        var e7 = await ValueTask.FromResult(Result.Failure(TestError)).ShouldHaveSeverityAsync(ErrorSeverity.Warning);
        Assert.Equal(ErrorSeverity.Warning, e7.Severity);
        var e8 = await ValueTask.FromResult(Result.Failure<int>(TestError)).ShouldHaveSeverityAsync(ErrorSeverity.Warning);
        Assert.Equal(ErrorSeverity.Warning, e8.Severity);
        var e9 = await ValueTask.FromResult(Result.Failure(TestError)).ShouldHaveDescriptionAsync("Desc1");
        Assert.Equal("Desc1", e9.Description);
        var e10 = await ValueTask.FromResult(Result.Failure<int>(TestError)).ShouldHaveDescriptionAsync("Desc1");
        Assert.Equal("Desc1", e10.Description);
        var e11 = await ValueTask.FromResult(Result.Failure(TestError)).ShouldHaveTraceIdAsync("trace");
        Assert.Equal("trace", e11.TraceId);
        var e12 = await ValueTask.FromResult(Result.Failure<int>(TestError)).ShouldHaveTraceIdAsync("trace");
        Assert.Equal("trace", e12.TraceId);
        var e13 = await ValueTask.FromResult(Result.Failure(TestError)).ShouldHaveCorrelationIdAsync("corr");
        Assert.Equal("corr", e13.CorrelationId);
        var e14 = await ValueTask.FromResult(Result.Failure<int>(TestError)).ShouldHaveCorrelationIdAsync("corr");
        Assert.Equal("corr", e14.CorrelationId);
        var e15 = await ValueTask.FromResult(Result.Failure(TestError)).ShouldHaveMetadataAsync("k", "v");
        Assert.Equal("v", e15.Metadata["k"]);
        var e16 = await ValueTask.FromResult(Result.Failure<int>(TestError)).ShouldHaveMetadataAsync("k", "v");
        Assert.Equal("v", e16.Metadata["k"]);
        var e17 = await ValueTask.FromResult(Result.Failure(TestError)).ShouldBeRetryableAsync();
        Assert.Equal(ErrorRetryability.Transient, e17.Retryability);
        var e18 = await ValueTask.FromResult(Result.Failure<int>(TestError)).ShouldBeRetryableAsync();
        Assert.Equal(ErrorRetryability.Transient, e18.Retryability);
        var e19 = await ValueTask.FromResult(Result.Failure(TestError2)).ShouldBePermanentAsync();
        Assert.Equal(ErrorRetryability.Permanent, e19.Retryability);
        var e20 = await ValueTask.FromResult(Result.Failure<int>(TestError2)).ShouldBePermanentAsync();
        Assert.Equal(ErrorRetryability.Permanent, e20.Retryability);
        var e21 = await ValueTask.FromResult(Result.Failure(CombinedError)).ShouldHaveInnerErrorsAsync(2);
        Assert.Equal(2, e21.InnerErrors.Length);
        var e22 = await ValueTask.FromResult(Result.Failure<int>(CombinedError)).ShouldHaveInnerErrorsAsync(2);
        Assert.Equal(2, e22.InnerErrors.Length);
        var e23 = await ValueTask.FromResult(Result.Failure(CombinedError)).ShouldContainInnerErrorAsync("Code1");
        Assert.Equal("Result.CombinedErrors", e23.Code);
        var e24 = await ValueTask.FromResult(Result.Failure<int>(CombinedError)).ShouldContainInnerErrorAsync("Code1");
        Assert.Equal("Result.CombinedErrors", e24.Code);
        var e25 = await ValueTask.FromResult(Result.Failure(CombinedError)).ShouldBeCombinedFailureAsync(2);
        Assert.Equal(2, e25.InnerErrors.Length);
        var e26 = await ValueTask.FromResult(Result.Failure<int>(CombinedError)).ShouldBeCombinedFailureAsync(2);
        Assert.Equal(2, e26.InnerErrors.Length);
        var e27 = await ValueTask.FromResult(Result.Failure(TestError)).ShouldNotHaveInnerErrorsAsync();
        Assert.Empty(e27.InnerErrors);
        var e28 = await ValueTask.FromResult(Result.Failure<int>(TestError)).ShouldNotHaveInnerErrorsAsync();
        Assert.Empty(e28.InnerErrors);
        var e29 = await ValueTask.FromResult(Result.Failure(CombinedError)).ShouldHaveInnerErrorCountAsync(2);
        Assert.Equal(2, e29.InnerErrors.Length);
        var e30 = await ValueTask.FromResult(Result.Failure<int>(CombinedError)).ShouldHaveInnerErrorCountAsync(2);
        Assert.Equal(2, e30.InnerErrors.Length);
        var u1 = await ValueTask.FromResult((Result)default).ShouldBeUninitializedAsync();
        Assert.True(u1.IsUninitialized);
        var u2 = await ValueTask.FromResult((Result<int>)default).ShouldBeUninitializedAsync();
        Assert.True(u2.IsUninitialized);
        var sat1 = await ValueTask.FromResult(Result.Success()).ShouldSatisfyAsync(r => { });
        sat1.ShouldBeSuccess();
        var sat2 = await ValueTask.FromResult(Result.Success(42)).ShouldSatisfyAsync(v => { });
        Assert.Equal(42, sat2);
        var sat3 = await ValueTask.FromResult(Result.Failure(TestError)).ShouldSatisfyErrorAsync(e => { });
        Assert.Equal(TestError, sat3);
        var sat4 = await ValueTask.FromResult(Result.Failure<int>(TestError)).ShouldSatisfyErrorAsync(e => { });
        Assert.Equal(TestError, sat4);
        var em1 = await ValueTask.FromResult(Result.Failure(TestError)).ShouldHaveErrorMatchingAsync(e => e.Code == "Code1");
        Assert.Equal(TestError, em1);
        var em2 = await ValueTask.FromResult(Result.Failure<int>(TestError)).ShouldHaveErrorMatchingAsync(e => e.Code == "Code1");
        Assert.Equal(TestError, em2);

        // Slow Path ValueTask
        var s1s = await DelayValueResult(Result.Success()).ShouldBeSuccessAsync();
        s1s.ShouldBeSuccess();
        var s2s = await DelayValueResult(Result.Success(42)).ShouldBeSuccessAsync();
        Assert.Equal(42, s2s);
        var v1s = await DelayValueResult(Result.Success(42)).ShouldHaveValueAsync(42);
        Assert.Equal(42, v1s);
        var e1s = await DelayValueResult(Result.Failure(TestError)).ShouldBeFailureAsync();
        Assert.Equal(TestError, e1s);
        var e2s = await DelayValueResult(Result.Failure<int>(TestError)).ShouldBeFailureAsync();
        Assert.Equal(TestError, e2s);
        var e3s = await DelayValueResult(Result.Failure(TestError)).ShouldHaveErrorCodeAsync("Code1");
        Assert.Equal("Code1", e3s.Code);
        var e4s = await DelayValueResult(Result.Failure<int>(TestError)).ShouldHaveErrorCodeAsync("Code1");
        Assert.Equal("Code1", e4s.Code);
        var e5s = await DelayValueResult(Result.Failure(TestError)).ShouldHaveErrorTypeAsync(ErrorType.Validation);
        Assert.Equal(ErrorType.Validation, e5s.Type);
        var e6s = await DelayValueResult(Result.Failure<int>(TestError)).ShouldHaveErrorTypeAsync(ErrorType.Validation);
        Assert.Equal(ErrorType.Validation, e6s.Type);
        var e7s = await DelayValueResult(Result.Failure(TestError)).ShouldHaveSeverityAsync(ErrorSeverity.Warning);
        Assert.Equal(ErrorSeverity.Warning, e7s.Severity);
        var e8s = await DelayValueResult(Result.Failure<int>(TestError)).ShouldHaveSeverityAsync(ErrorSeverity.Warning);
        Assert.Equal(ErrorSeverity.Warning, e8s.Severity);
        var e9s = await DelayValueResult(Result.Failure(TestError)).ShouldHaveDescriptionAsync("Desc1");
        Assert.Equal("Desc1", e9s.Description);
        var e10s = await DelayValueResult(Result.Failure<int>(TestError)).ShouldHaveDescriptionAsync("Desc1");
        Assert.Equal("Desc1", e10s.Description);
        var e11s = await DelayValueResult(Result.Failure(TestError)).ShouldHaveTraceIdAsync("trace");
        Assert.Equal("trace", e11s.TraceId);
        var e12s = await DelayValueResult(Result.Failure<int>(TestError)).ShouldHaveTraceIdAsync("trace");
        Assert.Equal("trace", e12s.TraceId);
        var e13s = await DelayValueResult(Result.Failure(TestError)).ShouldHaveCorrelationIdAsync("corr");
        Assert.Equal("corr", e13s.CorrelationId);
        var e14s = await DelayValueResult(Result.Failure<int>(TestError)).ShouldHaveCorrelationIdAsync("corr");
        Assert.Equal("corr", e14s.CorrelationId);
        var e15s = await DelayValueResult(Result.Failure(TestError)).ShouldHaveMetadataAsync("k", "v");
        Assert.Equal("v", e15s.Metadata["k"]);
        var e16s = await DelayValueResult(Result.Failure<int>(TestError)).ShouldHaveMetadataAsync("k", "v");
        Assert.Equal("v", e16s.Metadata["k"]);
        var e17s = await DelayValueResult(Result.Failure(TestError)).ShouldBeRetryableAsync();
        Assert.Equal(ErrorRetryability.Transient, e17s.Retryability);
        var e18s = await DelayValueResult(Result.Failure<int>(TestError)).ShouldBeRetryableAsync();
        Assert.Equal(ErrorRetryability.Transient, e18s.Retryability);
        var e19s = await DelayValueResult(Result.Failure(TestError2)).ShouldBePermanentAsync();
        Assert.Equal(ErrorRetryability.Permanent, e19s.Retryability);
        var e20s = await DelayValueResult(Result.Failure<int>(TestError2)).ShouldBePermanentAsync();
        Assert.Equal(ErrorRetryability.Permanent, e20s.Retryability);
        var e21s = await DelayValueResult(Result.Failure(CombinedError)).ShouldHaveInnerErrorsAsync(2);
        Assert.Equal(2, e21s.InnerErrors.Length);
        var e22s = await DelayValueResult(Result.Failure<int>(CombinedError)).ShouldHaveInnerErrorsAsync(2);
        Assert.Equal(2, e22s.InnerErrors.Length);
        var e23s = await DelayValueResult(Result.Failure(CombinedError)).ShouldContainInnerErrorAsync("Code1");
        Assert.Equal("Result.CombinedErrors", e23s.Code);
        var e24s = await DelayValueResult(Result.Failure<int>(CombinedError)).ShouldContainInnerErrorAsync("Code1");
        Assert.Equal("Result.CombinedErrors", e24s.Code);
        var e25s = await DelayValueResult(Result.Failure(CombinedError)).ShouldBeCombinedFailureAsync(2);
        Assert.Equal(2, e25s.InnerErrors.Length);
        var e26s = await DelayValueResult(Result.Failure<int>(CombinedError)).ShouldBeCombinedFailureAsync(2);
        Assert.Equal(2, e26s.InnerErrors.Length);
        var e27s = await DelayValueResult(Result.Failure(TestError)).ShouldNotHaveInnerErrorsAsync();
        Assert.Empty(e27s.InnerErrors);
        var e28s = await DelayValueResult(Result.Failure<int>(TestError)).ShouldNotHaveInnerErrorsAsync();
        Assert.Empty(e28s.InnerErrors);
        var e29s = await DelayValueResult(Result.Failure(CombinedError)).ShouldHaveInnerErrorCountAsync(2);
        Assert.Equal(2, e29s.InnerErrors.Length);
        var e30s = await DelayValueResult(Result.Failure<int>(CombinedError)).ShouldHaveInnerErrorCountAsync(2);
        Assert.Equal(2, e30s.InnerErrors.Length);
        var u1s = await DelayValueResult((Result)default).ShouldBeUninitializedAsync();
        Assert.True(u1s.IsUninitialized);
        var u2s = await DelayValueResult((Result<int>)default).ShouldBeUninitializedAsync();
        Assert.True(u2s.IsUninitialized);
        var sat1s = await DelayValueResult(Result.Success()).ShouldSatisfyAsync(r => { });
        sat1s.ShouldBeSuccess();
        var sat2s = await DelayValueResult(Result.Success(42)).ShouldSatisfyAsync(v => { });
        Assert.Equal(42, sat2s);
        var sat3s = await DelayValueResult(Result.Failure(TestError)).ShouldSatisfyErrorAsync(e => { });
        Assert.Equal(TestError, sat3s);
        var sat4s = await DelayValueResult(Result.Failure<int>(TestError)).ShouldSatisfyErrorAsync(e => { });
        Assert.Equal(TestError, sat4s);
        var em1s = await DelayValueResult(Result.Failure(TestError)).ShouldHaveErrorMatchingAsync(e => e.Code == "Code1");
        Assert.Equal(TestError, em1s);
        var em2s = await DelayValueResult(Result.Failure<int>(TestError)).ShouldHaveErrorMatchingAsync(e => e.Code == "Code1");
        Assert.Equal(TestError, em2s);
    }

    [Fact]
    public void Type_Formatting_Tests()
    {
        // Test Result<T> failure message formatting for various types (nested generic, array, nullable, multiple generics)
        var exSingleGeneric = Assert.Throws<ResultAssertionException>(() => Result.Failure<List<int>>(TestError).ShouldBeSuccess());
        Assert.Contains("List<int>", exSingleGeneric.Message);

        var exMultiGeneric = Assert.Throws<ResultAssertionException>(() => Result.Failure<Dictionary<string, int>>(TestError).ShouldBeSuccess());
        Assert.Contains("Dictionary<string, int>", exMultiGeneric.Message);

        var exTwoGenerics = Assert.Throws<ResultAssertionException>(() => Result.Failure<KeyValuePair<string, int>>(TestError).ShouldBeSuccess());
        Assert.Contains("KeyValuePair<string, int>", exTwoGenerics.Message);

        var exArray = Assert.Throws<ResultAssertionException>(() => Result.Failure<string[]>(TestError).ShouldBeSuccess());
        Assert.Contains("String[]", exArray.Message);

        var exNullable = Assert.Throws<ResultAssertionException>(() => Result.Failure<int?>(TestError).ShouldBeSuccess());
        Assert.Contains("Nullable<int>", exNullable.Message);
    }

    [Fact]
    public void Test_Remaining_Branches_And_Messages()
    {
        // 1. ShouldHaveValue predicate
        Result.Success(10).ShouldHaveValue(x => x > 5);
        Result.Success(10).ShouldHaveValue(x => x > 5, "custom predicate pass");
        var exPred1 = Assert.Throws<ResultAssertionException>(() => Result.Success(10).ShouldHaveValue(x => x < 5));
        Assert.Contains("Expected Result value to satisfy the predicate", exPred1.Message);
        var exPred2 = Assert.Throws<ResultAssertionException>(() => Result.Success(10).ShouldHaveValue(x => x < 5, "custom pred fail"));
        Assert.Equal("custom pred fail", exPred2.Message);

        // 2. ShouldHaveMetadataValue type mismatch and value mismatch (default and custom messages)
        var errWithIntMeta = Error.Create("Err", "Desc").WithMetadata("num", 123).Build();
        var exMetaType1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(errWithIntMeta).ShouldHaveMetadataValue<string>("num", "123"));
        Assert.Contains("Expected metadata key 'num' to be of type 'String'", exMetaType1.Message);
        var exMetaType2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(errWithIntMeta).ShouldHaveMetadataValue<int, string>("num", "123"));
        Assert.Contains("Expected metadata key 'num' to be of type 'String'", exMetaType2.Message);

        var exMetaValMismatch1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(errWithIntMeta).ShouldHaveMetadataValue("num", 999));
        Assert.Contains("Expected metadata['num'] = '999'", exMetaValMismatch1.Message);
        var exMetaValMismatch2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(errWithIntMeta).ShouldHaveMetadataValue("num", 999));
        Assert.Contains("Expected metadata['num'] = '999'", exMetaValMismatch2.Message);

        // Null value in metadata
        var errWithNullMeta = Error.Create("Err", "Desc").WithMetadata("nullKey", (string)null!).Build();
        var exNullMeta1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(errWithNullMeta).ShouldHaveMetadataValue("nullKey", 0));
        Assert.Contains("<null>", exNullMeta1.Message);
        var exNullMeta2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(errWithNullMeta).ShouldHaveMetadataValue<int, int>("nullKey", 0));
        Assert.Contains("<null>", exNullMeta2.Message);

        // 3. ShouldBeCombinedFailure error code mismatch (default and custom messages)
        var exCombCode1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldBeCombinedFailure(1));
        Assert.Contains("Expected a combined failure (code 'Result.CombinedErrors')", exCombCode1.Message);
        var exCombCode2 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldBeCombinedFailure(1, "custom comb code err"));
        Assert.Equal("custom comb code err", exCombCode2.Message);

        var exCombCode3 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldBeCombinedFailure(1));
        Assert.Contains("Expected a combined failure (code 'Result.CombinedErrors')", exCombCode3.Message);
        var exCombCode4 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldBeCombinedFailure(1, "custom generic comb code err"));
        Assert.Equal("custom generic comb code err", exCombCode4.Message);

        // 4. ShouldBeCombinedFailure inner count mismatch (default message)
        var exCombCount1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(CombinedError).ShouldBeCombinedFailure(5));
        Assert.Contains("Expected combined failure to contain 5 inner error(s), but found 2.", exCombCount1.Message);
        var exCombCount2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(CombinedError).ShouldBeCombinedFailure(5));
        Assert.Contains("Expected Result<int> combined failure to contain 5 inner error(s), but found 2.", exCombCount2.Message);

        // 5. ShouldStrictlyEqual default messages
        var exStrict1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldStrictlyEqual(TestError2));
        Assert.Contains("Expected error to strictly equal 'Code2' (all fields)", exStrict1.Message);
        var exStrict2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldStrictlyEqual(TestError2));
        Assert.Contains("Expected Result<int> error to strictly equal 'Code2' (all fields)", exStrict2.Message);

        // 6. ShouldNotHaveInnerErrors and ShouldHaveNoInnerErrors default messages
        var exNoInners1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(CombinedError).ShouldNotHaveInnerErrors());
        Assert.Contains("Expected error 'Result.CombinedErrors' to have no inner errors, but found 2.", exNoInners1.Message);
        var exNoInners2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(CombinedError).ShouldNotHaveInnerErrors());
        Assert.Contains("Expected Result<int> error 'Result.CombinedErrors' to have no inner errors, but found 2.", exNoInners2.Message);

        var exHaveNoInners1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(CombinedError).ShouldHaveNoInnerErrors());
        Assert.Contains("Expected no inner errors, but got 2 inner error(s).", exHaveNoInners1.Message);
        var exHaveNoInners2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(CombinedError).ShouldHaveNoInnerErrors());
        Assert.Contains("Expected no inner errors, but got 2 inner error(s).", exHaveNoInners2.Message);

        // 7. ShouldHaveInnerErrorCount default messages
        var exInnerCount1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(CombinedError).ShouldHaveInnerErrorCount(5));
        Assert.Contains("Expected error 'Result.CombinedErrors' to have exactly 5 inner error(s), but found 2.", exInnerCount1.Message);
        var exInnerCount2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(CombinedError).ShouldHaveInnerErrorCount(5));
        Assert.Contains("Expected Result<int> error 'Result.CombinedErrors' to have exactly 5 inner error(s), but found 2.", exInnerCount2.Message);

        var exMetaTypeCust1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(errWithIntMeta).ShouldHaveMetadataValue<string>("num", "123", "custom type mismatch"));
        Assert.Equal("custom type mismatch", exMetaTypeCust1.Message);
        var exMetaTypeCust2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(errWithIntMeta).ShouldHaveMetadataValue<int, string>("num", "123", "custom type mismatch"));
        Assert.Equal("custom type mismatch", exMetaTypeCust2.Message);

        var exMetaValCust1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(errWithIntMeta).ShouldHaveMetadataValue("num", 999, "custom val mismatch"));
        Assert.Equal("custom val mismatch", exMetaValCust1.Message);
        var exMetaValCust2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(errWithIntMeta).ShouldHaveMetadataValue("num", 999, "custom val mismatch"));
        Assert.Equal("custom val mismatch", exMetaValCust2.Message);

        // 8. ShouldBeUninitialized state in message
        var exUninitSuccess1 = Assert.Throws<ResultAssertionException>(() => Result.Success().ShouldBeUninitialized());
        Assert.Contains("Success", exUninitSuccess1.Message);
        var exUninitSuccess2 = Assert.Throws<ResultAssertionException>(() => Result.Success(10).ShouldBeUninitialized());
        Assert.Contains("Success", exUninitSuccess2.Message);
        var exUninitFail1 = Assert.Throws<ResultAssertionException>(() => Result.Failure(TestError).ShouldBeUninitialized());
        Assert.Contains("Failure", exUninitFail1.Message);
        var exUninitFail2 = Assert.Throws<ResultAssertionException>(() => Result.Failure<int>(TestError).ShouldBeUninitialized());
        Assert.Contains("Failure", exUninitFail2.Message);
    }

    [Fact]
    public async Task ShouldBeUninitializedAsync_Failure_Throws()
    {
        await Assert.ThrowsAsync<ResultAssertionException>(async () => await ValueTask.FromResult(Result.Success()).ShouldBeUninitializedAsync());
        await Assert.ThrowsAsync<ResultAssertionException>(async () => await DelayValueResult(Result.Success()).ShouldBeUninitializedAsync());
        await Assert.ThrowsAsync<ResultAssertionException>(async () => await ValueTask.FromResult(Result.Success(10)).ShouldBeUninitializedAsync());
        await Assert.ThrowsAsync<ResultAssertionException>(async () => await DelayValueResult(Result.Success(10)).ShouldBeUninitializedAsync());
        await Assert.ThrowsAsync<ResultAssertionException>(async () => await Task.FromResult(Result.Success()).ShouldBeUninitializedAsync());
        await Assert.ThrowsAsync<ResultAssertionException>(async () => await DelayResult(Result.Success()).ShouldBeUninitializedAsync());
        await Assert.ThrowsAsync<ResultAssertionException>(async () => await Task.FromResult(Result.Success(10)).ShouldBeUninitializedAsync());
        await Assert.ThrowsAsync<ResultAssertionException>(async () => await DelayResult(Result.Success(10)).ShouldBeUninitializedAsync());
    }
}






