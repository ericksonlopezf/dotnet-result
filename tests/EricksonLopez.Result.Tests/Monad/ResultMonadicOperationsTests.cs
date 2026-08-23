// Copyright © Erickson Lopez. MIT License.
using System;
using System.Runtime.CompilerServices;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

#nullable enable
namespace EricksonLopez.Result.Tests.Monad;

public class ResultMonadicOperationsTests
{
    [Fact]
    public void ResultT_Switch_ExecutesCorrectBranch()
    {
        var success = Result.Success(42);
        var failure = Result.Failure<int>(Error.Unexpected("Err", "Msg"));

        int successVal = 0;
        success.Execute(v => successVal = v, _ => Assert.Fail("Should not call failure"));
        Assert.Equal(42, successVal);

        string? errorVal = null;
        failure.Execute(_ => Assert.Fail("Should not call success"), e => errorVal = e.Code);
        Assert.Equal("Err", errorVal);
    }

    [Fact]
    public void NonGenericResult_Bind_ChainsPipeline()
    {
        var result = Result.Success()
            .Bind(() => Result.Success(100));

        result.ShouldBeSuccess();
        Assert.Equal(100, result.Value);
    }

    [Fact]
    public void Result_Combine_ReadOnlySpan_AggregatesErrors()
    {
        var r1 = Result.Success();
        var r2 = Result.Failure(Error.NotFound("E1", "Error 1"));
        var r3 = Result.Failure(Error.Validation("E2", "Error 2"));

        var combined = Result.Combine([r1, r2, r3]);

        var error = combined.ShouldBeFailure();
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, error.Code);
        Assert.True(error.HasInnerErrors);
        Assert.Equal(2, error.InnerErrors.Length);
    }

    [Fact]
    public void Result_Merge_CombinesGuardAndTypedResult()
    {
        var guardSuccess = Result.Success();
        var guardFailure = Result.Failure(Error.Unauthorized("A1", "No access"));

        var typed = Result.Success("Data");

        var m1 = Result.Merge(guardSuccess, typed);
        Assert.Equal("Data", m1.ShouldBeSuccess());

        var m2 = Result.Merge(guardFailure, typed);
        m2.ShouldHaveErrorCode("A1");
    }

}

