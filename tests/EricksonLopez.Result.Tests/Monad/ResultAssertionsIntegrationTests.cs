// Copyright © Erickson Lopez. MIT License.
using System;
using System.Runtime.CompilerServices;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

#nullable enable
namespace EricksonLopez.Result.Tests.Monad;

[Trait("Category", "Integration")]
public class ResultAssertionsIntegrationTests
{
    [Fact]
    public void ResultT_IsZeroAllocation_SizeMatchesOptimizedStruct()
    {

        var sizeOfResult = Unsafe.SizeOf<Result<int>>();
        Assert.True(sizeOfResult <= 32, $"Size of Result<int> was {sizeOfResult}, expected <= 32");
    }

    [Fact]
    public void Result_IsZeroAllocation_SizeMatchesOptimizedStruct()
    {

        var sizeOfResult = Unsafe.SizeOf<Result>();
        Assert.True(sizeOfResult <= 24, $"Size of Result was {sizeOfResult}, expected <= 24");
    }

    [Fact]
    public void ErrorProperty_SuccessResult_ThrowsWithMessage()
    {
        var result = Result.Success();
        var ex = Assert.Throws<InvalidOperationException>(() => result.Error);
        Assert.Equal("Cannot access the Error of a successful result.", ex.Message);
    }

    [Fact]
    public void SwitchTState_Uninitialized_Throws()
    {
        Result result = default;
        Assert.Throws<InvalidOperationException>(() => result.Execute(1, _ => { }, (_, _) => { }));
    }

    [Fact]
    public void BindTState_Uninitialized_Throws()
    {
        Result result = default;
        Assert.Throws<InvalidOperationException>(() => result.Bind(1, _ => Result.Success()));
    }

    [Fact]
    public void IResultOutcome_Error_SuccessResult_ReturnsNull()
    {
        IResultOutcome outcome = Result.Success();
        Assert.Null(outcome.Error);
    }

#pragma warning disable CA2201, CS0162
}

