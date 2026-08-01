using System;
using System.Reflection;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class CoreInternalCoverageTests
{
    [Fact]
    public void Result_ErrorThrowsOnSuccess()
    {

        var r = Result.Success();
        Assert.Throws<InvalidOperationException>(() => r.Error);
    }

    [Fact]
    public void Result_Switch_Failure()
    {
        var r = Result.Failure(Error.Failure("E", "M"));
        bool successCalled = false;
        bool failureCalled = false;
        r.Execute(() => successCalled = true, e => failureCalled = true);
        Assert.False(successCalled);
        Assert.True(failureCalled);
    }

    [Fact]
    public void Result_IResultOutcome_Properties()
    {

        IResultOutcome rSuccess = Result.Success();
        Assert.Null(rSuccess.Error);
        Assert.Null(rSuccess.RawValue);

        IResultOutcome rFailure = Result.Failure(Error.Failure("E", "M"));
        Assert.NotNull(rFailure.Error);
        Assert.Null(rFailure.RawValue);
    }

    [Fact]
    [Obsolete("Testing obsolete Finally", error: false)]
    #pragma warning disable CS0618
    public void Result_Finally()
    {
        var r = Result.Success();
        bool called = false;
        r.Finally(_ => called = true);
        Assert.True(called);
    }
    #pragma warning restore CS0618

    [Fact]
    public void Result_DebuggerDisplay()
    {

        var method = typeof(Result).GetMethod("GetDebuggerDisplay", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        
        var success = Result.Success();
        Assert.Equal("Success", method.Invoke(success, null));

        var failure = Result.Failure(Error.Failure("E", "M"));
        Assert.Equal("Failure (E)", method.Invoke(failure, null));

        var uninit = default(Result);
        Assert.Equal("Uninitialized", method.Invoke(uninit, null));
    }

    [Fact]
    public void ResultOfT_ValueThrowsOnFailure()
    {

        var r = Result.Failure<int>(Error.Failure("E", "M"));
        Assert.Throws<InvalidOperationException>(() => r.Value);
    }

    [Fact]
    public void ResultOfT_ErrorThrowsOnSuccess()
    {

        var r = Result.Success(42);
        Assert.Throws<InvalidOperationException>(() => r.Error);
    }

    [Fact]
    public void ResultOfT_Switch_Failure()
    {
        var r = Result.Failure<int>(Error.Failure("E", "M"));
        bool successCalled = false;
        bool failureCalled = false;
        r.Execute(v => successCalled = true, e => failureCalled = true);
        Assert.False(successCalled);
        Assert.True(failureCalled);
    }

    [Fact]
    [Obsolete("Testing obsolete Finally", error: false)]
    #pragma warning disable CS0618
    public void ResultOfT_Finally()
    {
        var r = Result.Success(42);
        bool called = false;
        r.Finally(_ => called = true);
        Assert.True(called);
    }
    #pragma warning restore CS0618

    [Fact]
    public void ResultOfT_TryGetValue()
    {

        var rSuccess = Result.Success(42);
        Assert.True(rSuccess.TryGetValue(out var val1));
        Assert.Equal(42, val1);

        var rFailure = Result.Failure<int>(Error.Failure("E", "M"));
        Assert.False(rFailure.TryGetValue(out var val2));
        Assert.Equal(default, val2);
    }

    [Fact]
    public void ResultOfT_GetValueOrDefault()
    {

        var rSuccess = Result.Success(42);
        Assert.Equal(42, rSuccess.GetValueOrDefault(100));

        var rFailure = Result.Failure<int>(Error.Failure("E", "M"));
        Assert.Equal(100, rFailure.GetValueOrDefault(100));
    }

    [Fact]
    public void ResultOfT_Recover_Success()
    {
        var r = Result.Success(42);
        var r2 = r.Recover(e => Result.Success(100));
        Assert.Equal(42, r2.Value);
    }

    [Fact]
    public void ResultOfT_DiscardValue()
    {
        var rSuccess = Result.Success(42);
        var dr = rSuccess.DiscardValue();
        Assert.True(dr.IsSuccess);

        var rFailure = Result.Failure<int>(Error.Failure("E", "M"));
        var dr2 = rFailure.DiscardValue();
        Assert.True(dr2.IsFailure);
    }

    [Fact]
    [Obsolete("Testing obsoleted methods", error: false)]
    #pragma warning disable CS0618
    public void ResultOfT_Obsolete_Methods()
    {

        var r = Result.Success(42);
        Assert.True(r.ToResult().IsSuccess);
        Assert.True(r.WithoutValue().IsSuccess);
    }
    #pragma warning restore CS0618

    [Fact]
    public void ResultOfT_Deconstruct_2Args()
    {
        var rSuccess = Result.Success(42);
        rSuccess.Deconstruct(out bool s1, out Error? e1);
        Assert.True(s1);
        Assert.Null(e1);

        var rFailure = Result.Failure<int>(Error.Failure("E", "M"));
        rFailure.Deconstruct(out bool s2, out Error? e2);
        Assert.False(s2);
        Assert.NotNull(e2);
    }

    [Fact]
    public void ResultOfT_Uninitialized_EqualityAndHash()
    {
        var u1 = default(Result<int>);
        var u2 = default(Result<int>);
        var s = Result.Success(42);

        Assert.True(u1.Equals(u2));
        Assert.False(u1.Equals(s));
        Assert.Equal(u1.GetHashCode(), u2.GetHashCode());
    }

    [Fact]
    public void ResultOfT_DebuggerDisplay()
    {

        var method = typeof(Result<int>).GetMethod("GetDebuggerDisplay", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        
        var success = Result.Success(42);
        Assert.Equal("Success (42)", method.Invoke(success, null));

        var failure = Result.Failure<int>(Error.Failure("E", "M"));
        Assert.Equal("Failure (E)", method.Invoke(failure, null));

        var uninit = default(Result<int>);
        Assert.Equal("Uninitialized", method.Invoke(uninit, null));
    }
}
