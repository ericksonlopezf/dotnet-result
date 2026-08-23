// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultCombineTests
{
    [Fact]
    public void Combine_Span_Success()
    {
        var r = Result.Combine([Result.Success(), Result.Success()]);
        r.ShouldBeSuccess();

        var empty = Result.Combine([]);
        empty.ShouldBeSuccess();
    }

    [Fact]
    public void Combine_Span_SingleFailure()
    {
        var e = Error.Failure("E", "M");
        var r = Result.Combine([Result.Success(), Result.Failure(e)]);
        r.ShouldBeFailure().Should().BeSameAs(e);
    }

    [Fact]
    public void Combine_T_Span_Success()
    {
        var r = Result.Combine([Result.Success(1), Result.Success(2)]);
        var list = r.ShouldBeSuccess();
        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);

        var empty = Result.Combine<int>([]);
        empty.ShouldBeSuccess().Should().BeEmpty();
    }

    [Fact]
    public void Combine_T_Span_String_SingleFailure()
    {
        var e = Error.Failure("E", "M");
        var r = Result.Combine([Result.Success("1"), Result.Failure<string>(e)]);
        r.ShouldBeFailure().Should().BeSameAs(e);
    }

    [Fact]
    public void Combine_T_Span_SingleFailure()
    {
        var e = Error.Failure("E", "M");
        var r = Result.Combine([Result.Success(1), Result.Failure<int>(e)]);
        r.ShouldBeFailure().Should().BeSameAs(e);
    }

    [Fact]
    public void Combine_Span_MultipleFailures_PreservesErrorsInOrder()
    {
        var e1 = Error.Failure("E1", "M1");
        var e2 = Error.Failure("E2", "M2");
        var e3 = Error.Failure("E3", "M3");

        var r = Result.Combine([Result.Success(), Result.Failure(e1), Result.Failure(e2), Result.Success(), Result.Failure(e3)]);

        var error = r.ShouldBeFailure();
        Assert.Equal(3, error.InnerErrors.Length);
        Assert.Equal(e1, error.InnerErrors[0]);
        Assert.Equal(e2, error.InnerErrors[1]);
        Assert.Equal(e3, error.InnerErrors[2]);
    }

    [Fact]
    public void Combine_IEnumerable_MultipleFailures_PreservesErrorsInOrder()
    {
        var e1 = Error.Failure("E1", "M1");
        var e2 = Error.Failure("E2", "M2");
        var e3 = Error.Failure("E3", "M3");

        var enumerable = new[] { Result.Success(), Result.Failure(e1), Result.Failure(e2), Result.Success(), Result.Failure(e3) }.AsEnumerable();
        var r = Result.Combine(enumerable.ToArray());

        var error = r.ShouldBeFailure();
        Assert.Equal(3, error.InnerErrors.Length);
        Assert.Equal(e1, error.InnerErrors[0]);
        Assert.Equal(e2, error.InnerErrors[1]);
        Assert.Equal(e3, error.InnerErrors[2]);
    }

    [Fact]
    public void Combine_T_Span_MultipleFailures_PreservesErrorsInOrder()
    {
        var e1 = Error.Failure("E1", "M1");
        var e2 = Error.Failure("E2", "M2");
        var e3 = Error.Failure("E3", "M3");

        var r = Result.Combine([Result.Success(1), Result.Failure<int>(e1), Result.Failure<int>(e2), Result.Failure<int>(e3)]);

        var error = r.ShouldBeFailure();
        Assert.Equal(3, error.InnerErrors.Length);
        Assert.Equal(e1, error.InnerErrors[0]);
        Assert.Equal(e2, error.InnerErrors[1]);
        Assert.Equal(e3, error.InnerErrors[2]);
    }

    [Fact]
    public void Combine_T_IEnumerable_MultipleFailures_PreservesErrorsInOrder()
    {
        var e1 = Error.Failure("E1", "M1");
        var e2 = Error.Failure("E2", "M2");
        var e3 = Error.Failure("E3", "M3");

        var enumerable = new[] { Result.Success(1), Result.Failure<int>(e1), Result.Failure<int>(e2), Result.Failure<int>(e3) }.AsEnumerable();
        var r = Result.Combine(enumerable.ToArray());

        var error = r.ShouldBeFailure();
        Assert.Equal(3, error.InnerErrors.Length);
        Assert.Equal(e1, error.InnerErrors[0]);
        Assert.Equal(e2, error.InnerErrors[1]);
        Assert.Equal(e3, error.InnerErrors[2]);
    }

    [Fact]
    public void Combine_T_Span_MultipleFailures()
    {
        var e1 = Error.Failure("E1", "M1");
        var e2 = Error.Failure("E2", "M2");
        var r = Result.Combine([Result.Failure<int>(e1), Result.Success(1), Result.Failure<int>(e2)]);
        var error = r.ShouldBeFailure();
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, error.Code);
        Assert.Equal(2, error.InnerErrors.Length);
        Assert.Equal(e1, error.InnerErrors[0]);
        Assert.Equal(e2, error.InnerErrors[1]);
    }

    [Fact]
    public void Combine_T1_T2_Tests()
    {
        var s1 = Result.Success(1);
        var s2 = Result.Success("A");
        var f1 = Result.Failure<int>(Error.Failure("E1", "M1"));
        var f2 = Result.Failure<string>(Error.Failure("E2", "M2"));

        Assert.True(Result.Combine(s1, s2).IsSuccess);
        Assert.Equal("E1", Result.Combine(f1, s2).Error!.Code);
        Assert.Equal("E2", Result.Combine(s1, f2).Error!.Code);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, Result.Combine(f1, f2).Error!.Code);
    }

    [Fact]
    public void Combine_T1_T2_T3_Tests()
    {
        var s1 = Result.Success(1);
        var s2 = Result.Success("A");
        var s3 = Result.Success(true);
        var f1 = Result.Failure<int>(Error.Failure("E1", "M1"));
        var f2 = Result.Failure<string>(Error.Failure("E2", "M2"));
        var f3 = Result.Failure<bool>(Error.Failure("E3", "M3"));

        Assert.True(Result.Combine(s1, s2, s3).IsSuccess);
        Assert.Equal("E1", Result.Combine(f1, s2, s3).Error!.Code);
        Assert.Equal("E2", Result.Combine(s1, f2, s3).Error!.Code);
        Assert.Equal("E3", Result.Combine(s1, s2, f3).Error!.Code);

        Assert.Equal(WellKnownErrors.CombinedFailuresCode, Result.Combine(f1, f2, s3).Error!.Code);
        Assert.Equal(2, Result.Combine(f1, f2, s3).Error!.InnerErrors.Length);

        var errs3 = Result.Combine(f1, f2, f3).Error!.InnerErrors;
        Assert.Equal(3, errs3.Length);
        Assert.Equal("E1", errs3[0].Code);
        Assert.Equal("E3", errs3[2].Code);
    }

    [Fact]
    public void Combine_Span_MultipleFailures_Three()
    {
        var e1 = Error.Failure("E1", "M1");
        var e2 = Error.Failure("E2", "M2");
        var e3 = Error.Failure("E3", "M3");
        var r = Result.Combine([Result.Failure(e1), Result.Failure(e2), Result.Failure(e3)]);
        var error = r.ShouldBeFailure();
        Assert.Equal(3, error.InnerErrors.Length);
    }

    [Fact]
    public void Combine_T_Span_MultipleFailures_Three()
    {
        var e1 = Error.Failure("E1", "M1");
        var e2 = Error.Failure("E2", "M2");
        var e3 = Error.Failure("E3", "M3");
        var r = Result.Combine([Result.Failure<int>(e1), Result.Failure<int>(e2), Result.Failure<int>(e3)]);
        var error = r.ShouldBeFailure();
        Assert.Equal(3, error.InnerErrors.Length);
    }

    [Fact]
    public void Combine_T1_T2_T3_T4_Tests()
    {
        var s = Result.Success(1);
        var e1 = Error.Failure("E1", "desc");
        var f1 = Result.Failure<int>(e1);
        var e2 = Error.Failure("E2", "desc");
        var f2 = Result.Failure<int>(e2);
        var e3 = Error.Failure("E3", "desc");
        var f3 = Result.Failure<int>(e3);
        var e4 = Error.Failure("E4", "desc");
        var f4 = Result.Failure<int>(e4);

        Assert.True(Result.Combine(s, s, s, s).IsSuccess);
        Assert.Equal("E1", Result.Combine(f1, s, s, s).Error!.Code);
        Assert.Equal("E2", Result.Combine(s, f2, s, s).Error!.Code);
        Assert.Equal("E3", Result.Combine(s, s, f3, s).Error!.Code);
        Assert.Equal("E4", Result.Combine(s, s, s, f4).Error!.Code);
        var errs4 = Result.Combine(f1, f2, f3, f4).Error!.InnerErrors;
        Assert.Equal(4, errs4.Length);
        Assert.Equal("E1", errs4[0].Code);
        Assert.Equal("E4", errs4[3].Code);
    }

    [Fact]
    public void Combine_T1_T2_T3_T4_T5_Tests()
    {
        var s = Result.Success(1);
        var e1 = Error.Failure("E1", "desc");
        var f1 = Result.Failure<int>(e1);
        var e2 = Error.Failure("E2", "desc");
        var f2 = Result.Failure<int>(e2);
        var e3 = Error.Failure("E3", "desc");
        var f3 = Result.Failure<int>(e3);
        var e4 = Error.Failure("E4", "desc");
        var f4 = Result.Failure<int>(e4);
        var e5 = Error.Failure("E5", "desc");
        var f5 = Result.Failure<int>(e5);

        Assert.True(Result.Combine(s, s, s, s, s).IsSuccess);
        Assert.Equal("E1", Result.Combine(f1, s, s, s, s).Error!.Code);
        Assert.Equal("E2", Result.Combine(s, f2, s, s, s).Error!.Code);
        Assert.Equal("E3", Result.Combine(s, s, f3, s, s).Error!.Code);
        Assert.Equal("E4", Result.Combine(s, s, s, f4, s).Error!.Code);
        Assert.Equal("E5", Result.Combine(s, s, s, s, f5).Error!.Code);
        var errs5 = Result.Combine(f1, f2, f3, f4, f5).Error!.InnerErrors;
        Assert.Equal(5, errs5.Length);
        Assert.Equal("E1", errs5[0].Code);
        Assert.Equal("E5", errs5[4].Code);
    }
    [Fact]
    public void Combine_T3_T4_T5_MultipleFailures_CapturesAllErrors()
    {
        var e1 = Error.Failure("1", "1");
        var e2 = Error.Failure("2", "2");
        var e3 = Error.Failure("3", "3");
        var e4 = Error.Failure("4", "4");
        var e5 = Error.Failure("5", "5");

        var f1 = Result.Failure<int>(e1);
        var f2 = Result.Failure<int>(e2);
        var f3 = Result.Failure<int>(e3);
        var f4 = Result.Failure<int>(e4);
        var f5 = Result.Failure<int>(e5);

        var r3 = Result.Combine(f1, f2, f3);
        Assert.True(r3.IsFailure);
        Assert.Equal(3, r3.Error.InnerErrors.Length);
        Assert.Equal(e1, r3.Error.InnerErrors[0]);
        Assert.Equal(e2, r3.Error.InnerErrors[1]);
        Assert.Equal(e3, r3.Error.InnerErrors[2]);

        var r4 = Result.Combine(f1, f2, f3, f4);
        Assert.True(r4.IsFailure);
        Assert.Equal(4, r4.Error.InnerErrors.Length);

        var r5 = Result.Combine(f1, f2, f3, f4, f5);
        Assert.True(r5.IsFailure);
        Assert.Equal(5, r5.Error.InnerErrors.Length);
    }

    [Fact]
    public void Combine_MixedSuccessAndFailure_ChecksAllBranches()
    {
        var s = Result.Success(1);
        var f = Result.Failure<int>(Error.Failure("F", "fail"));

        var r3 = Result.Combine(s, f, s);
        Assert.True(r3.IsFailure);

        var r4 = Result.Combine(s, f, s, f);
        Assert.True(r4.IsFailure);

        var r5 = Result.Combine(s, f, s, f, s);
        Assert.True(r5.IsFailure);
    }

    [Fact]
    public void Combine_MixedSuccessAndFailure_ChecksAllBranches2()
    {
        var s = Result.Success(1);
        var f = Result.Failure<int>(Error.Failure("F", "fail"));

        // Combine 3
        var c3_1 = Result.Combine(s, f, f);
        Assert.True(c3_1.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, c3_1.Error.Code);
        Assert.Equal(2, c3_1.Error.InnerErrors.Length);

        var c3_2 = Result.Combine(f, s, f);
        Assert.True(c3_2.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, c3_2.Error.Code);
        Assert.Equal(2, c3_2.Error.InnerErrors.Length);

        var c3_3 = Result.Combine(f, f, s);
        Assert.True(c3_3.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, c3_3.Error.Code);
        Assert.Equal(2, c3_3.Error.InnerErrors.Length);

        // Combine 4
        var c4_1 = Result.Combine(s, f, f, f);
        Assert.True(c4_1.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, c4_1.Error.Code);
        Assert.Equal(3, c4_1.Error.InnerErrors.Length);

        var c4_2 = Result.Combine(f, s, f, f);
        Assert.True(c4_2.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, c4_2.Error.Code);
        Assert.Equal(3, c4_2.Error.InnerErrors.Length);

        var c4_3 = Result.Combine(f, f, s, f);
        Assert.True(c4_3.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, c4_3.Error.Code);
        Assert.Equal(3, c4_3.Error.InnerErrors.Length);

        var c4_4 = Result.Combine(f, f, f, s);
        Assert.True(c4_4.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, c4_4.Error.Code);
        Assert.Equal(3, c4_4.Error.InnerErrors.Length);

        // Combine 5
        var c5_1 = Result.Combine(s, f, f, f, f);
        Assert.True(c5_1.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, c5_1.Error.Code);
        Assert.Equal(4, c5_1.Error.InnerErrors.Length);

        var c5_2 = Result.Combine(f, s, f, f, f);
        Assert.True(c5_2.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, c5_2.Error.Code);
        Assert.Equal(4, c5_2.Error.InnerErrors.Length);

        var c5_3 = Result.Combine(f, f, s, f, f);
        Assert.True(c5_3.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, c5_3.Error.Code);
        Assert.Equal(4, c5_3.Error.InnerErrors.Length);

        var c5_4 = Result.Combine(f, f, f, s, f);
        Assert.True(c5_4.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, c5_4.Error.Code);
        Assert.Equal(4, c5_4.Error.InnerErrors.Length);

        var c5_5 = Result.Combine(f, f, f, f, s);
        Assert.True(c5_5.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, c5_5.Error.Code);
        Assert.Equal(4, c5_5.Error.InnerErrors.Length);
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
    public void Result_Merge_WhenGuardSucceedsButNextFails_ReturnsNextFailure()
    {
        var guardSuccess = Result.Success();
        var nextFailure = Result.Failure<string>(Error.Validation("V1", "Invalid"));

        var merged = Result.Merge(guardSuccess, nextFailure);

        merged.IsFailure.Should().BeTrue();
        merged.Error.Code.Should().Be("V1");
    }

    [Fact]
    public void Result_Merge_WhenGuardFailsAndNextFails_ReturnsGuardFailure()
    {
        var guardFailure = Result.Failure(Error.Unauthorized("A1", "No access"));
        var nextFailure = Result.Failure<string>(Error.Validation("V1", "Invalid"));

        var merged = Result.Merge(guardFailure, nextFailure);

        merged.IsFailure.Should().BeTrue();
        merged.Error.Code.Should().Be("A1");
    }

    [Fact]
    public void Combine_ArrayOverload_DirectInvocation_BehavesIdentically()
    {
        Result[] resultsArray = [Result.Success(), Result.Success()];
        var r = Result.Combine(resultsArray);
        r.IsSuccess.Should().BeTrue();

        Result<int>[] typedArray = [Result.Success(10), Result.Success(20)];
        var rT = Result.Combine(typedArray);
        rT.IsSuccess.Should().BeTrue();
        rT.Value.Should().Equal(10, 20);
    }

    [Fact]
    public void Combine_Span_WhenContainsUninitialized_ThrowsInvalidOperationException()
    {
        Result uninit = default;
        var act = () => Result.Combine([Result.Success(), uninit]);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*uninitialized*");
    }

    [Fact]
    public void Combine_TypedSpan_WhenContainsUninitialized_ThrowsInvalidOperationException()
    {
        Result<int> uninit = default;
        var act = () => Result.Combine([Result.Success(1), uninit]);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*uninitialized*");
    }

    [Fact]
    public void Combine_Tuples_WhenContainsUninitialized_ThrowsInvalidOperationException()
    {
        Result<int> uninit = default;
        Result<string> uninitString = default;
        var s1 = Result.Success(1);
        var s2 = Result.Success("2");
        var s3 = Result.Success(3.0);
        var s4 = Result.Success(true);

        Assert.Throws<InvalidOperationException>(() => Result.Combine(uninit, s2));
        Assert.Throws<InvalidOperationException>(() => Result.Combine(s1, uninitString));
        
        Assert.Throws<InvalidOperationException>(() => Result.Combine(uninit, s2, s3));
        Assert.Throws<InvalidOperationException>(() => Result.Combine(s1, uninitString, s3));
        Assert.Throws<InvalidOperationException>(() => Result.Combine(s1, s2, uninit));
        
        Assert.Throws<InvalidOperationException>(() => Result.Combine(uninit, s2, s3, s4));
        Assert.Throws<InvalidOperationException>(() => Result.Combine(s1, uninitString, s3, s4));
        Assert.Throws<InvalidOperationException>(() => Result.Combine(s1, s2, uninit, s4));
        Assert.Throws<InvalidOperationException>(() => Result.Combine(s1, s2, s3, uninitString));
        
        Assert.Throws<InvalidOperationException>(() => Result.Combine(uninit, s2, s3, s4, s1));
        Assert.Throws<InvalidOperationException>(() => Result.Combine(s1, uninitString, s3, s4, s1));
        Assert.Throws<InvalidOperationException>(() => Result.Combine(s1, s2, uninit, s4, s1));
        Assert.Throws<InvalidOperationException>(() => Result.Combine(s1, s2, s3, uninitString, s1));
        Assert.Throws<InvalidOperationException>(() => Result.Combine(s1, s2, s3, s4, uninit));
    }

    [Fact]
    public void Merge_WhenGuardOrNextIsUninitialized_ThrowsInvalidOperationException()
    {
        Result uninitGuard = default;
        Result<int> uninitNext = default;
        var validGuard = Result.Success();
        var validNext = Result.Success(42);

        Assert.Throws<InvalidOperationException>(() => Result.Merge(uninitGuard, validNext));
        Assert.Throws<InvalidOperationException>(() => Result.Merge(validGuard, uninitNext));
    }
}




