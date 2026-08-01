using System;
using Xunit;
using AwesomeAssertions;
using EricksonLopez.Result.Testing;

namespace EricksonLopez.Result.Tests.Core;

public class ResultAdditionalTests
{
    [Fact]
    public void DefaultResult_IsUninitialized()
    {
        Result r = default;
        Assert.True(r.IsUninitialized);
        Assert.False(r.IsSuccess);
        Assert.False(r.IsFailure);
        
        // operator true/false
        bool isTrue = r ? true : false;
        Assert.False(isTrue);

        if (r) { Assert.Fail("operator true on default returned true"); }
        else { Assert.True(true); }
    }

    [Fact]
    public void DefaultResultT_IsUninitialized()
    {
        Result<int> r = default;
        Assert.True(r.IsUninitialized);
        Assert.False(r.IsSuccess);
        Assert.False(r.IsFailure);
    }

    [Fact]
    public void DefaultResult_ThrowsOnMethods()
    {
        Result r = default;
        Assert.Equal(WellKnownErrors.UninitializedError, r.Error);

        Assert.Throws<InvalidOperationException>(() => r.Match(() => 1, e => 2));
        Assert.Throws<InvalidOperationException>(() => r.Match(0, (s) => 1, (s, e) => 2));
        
        Assert.Throws<InvalidOperationException>(() => r.Execute(() => {}, e => {}));
        Assert.Throws<InvalidOperationException>(() => r.Execute(0, (s) => {}, (s, e) => {}));
        
        Assert.Throws<InvalidOperationException>(() => r.MapFailure(e => 1, 0));
        Assert.Throws<InvalidOperationException>(() => r.MapFailure(0, (s, e) => 1, 0));
    }

    [Fact]
    public void DefaultResultT_ThrowsOnMethods()
    {
        Result<int> r = default;
        Assert.Equal(WellKnownErrors.UninitializedError, r.Error);

        var ex = Assert.Throws<InvalidOperationException>(() => r.Value);

        Assert.Throws<InvalidOperationException>(() => r.Match(v => 1, e => 2));
        Assert.Throws<InvalidOperationException>(() => r.Match(0, (s, v) => 1, (s, e) => 2));
        
        Assert.Throws<InvalidOperationException>(() => r.Execute(v => {}, e => {}));
        Assert.Throws<InvalidOperationException>(() => r.Execute(0, (s, v) => {}, (s, e) => {}));
        
        Assert.Throws<InvalidOperationException>(() => r.MapFailure(e => 1, 0));
        Assert.Throws<InvalidOperationException>(() => r.MapFailure(0, (s, e) => 1, 0));
    }

    [Fact]
    public void ImplicitOperator_ErrorToResult()
    {
        Error e = Error.Failure("A", "B");
        Result r = e;
        Assert.True(r.IsFailure);
        Assert.Equal(e, r.Error);

        Result<int> rt = e;
        Assert.True(rt.IsFailure);
        Assert.Equal(e, rt.Error);
    }

    [Fact]
    public void SuccessResult_Error_Throws()
    {
        Result r = Result.Success();
        Assert.Throws<InvalidOperationException>(() => r.Error);

        Result<int> rt = Result.Success(42);
        Assert.Throws<InvalidOperationException>(() => rt.Error);
    }
}
