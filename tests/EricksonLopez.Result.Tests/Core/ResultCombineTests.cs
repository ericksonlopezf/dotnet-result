using System;
using System.Linq;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultCombineTests
{
    [Fact]
    public void Combine_Span_Success()
    {

        var r = Result.Combine([Result.Success(), Result.Success()]);
        Assert.True(r.IsSuccess);

        var empty = Result.Combine([]);
        Assert.True(empty.IsSuccess);
    }

    [Fact]
    public void Combine_Span_SingleFailure()
    {
        var e = Error.Failure("E", "M");
        var r = Result.Combine([Result.Success(), Result.Failure(e)]);
        Assert.True(r.IsFailure);
        Assert.Equal(e, r.Error);
    }

    [Fact]
    public void Combine_T_Span_Success()
    {

        var r = Result.Combine([Result.Success(1), Result.Success(2)]);
        Assert.True(r.IsSuccess);
        Assert.Equal(2, r.Value.Count);
        Assert.Equal(1, r.Value[0]);
        Assert.Equal(2, r.Value[1]);

        var empty = Result.Combine<int>([]);
        Assert.True(empty.IsSuccess);
        Assert.Empty(empty.Value);
    }

    [Fact]
    public void Combine_T_Span_String_SingleFailure()
    {
        var e = Error.Failure("E", "M");
        var r = Result.Combine([Result.Success("1"), Result.Failure<string>(e)]);
        Assert.True(r.IsFailure);
        Assert.Equal(e, r.Error);
    }

    [Fact]
    public void Combine_T_Span_SingleFailure()
    {
        var e = Error.Failure("E", "M");
        var r = Result.Combine([Result.Success(1), Result.Failure<int>(e)]);
        Assert.True(r.IsFailure);
        Assert.Equal(e, r.Error);
    }

    [Fact]
    public void Combine_Span_MultipleFailures_PreservesErrorsInOrder()
    {
        var e1 = Error.Failure("E1", "M1");
        var e2 = Error.Failure("E2", "M2");
        var e3 = Error.Failure("E3", "M3");

        var r = Result.Combine([Result.Success(), Result.Failure(e1), Result.Failure(e2), Result.Success(), Result.Failure(e3)]);
        
        Assert.True(r.IsFailure);
        Assert.Equal(3, r.Error.InnerErrors.Length);
        Assert.Equal(e1, r.Error.InnerErrors[0]);
        Assert.Equal(e2, r.Error.InnerErrors[1]);
        Assert.Equal(e3, r.Error.InnerErrors[2]);
    }

    [Fact]
    public void Combine_IEnumerable_MultipleFailures_PreservesErrorsInOrder()
    {
        var e1 = Error.Failure("E1", "M1");
        var e2 = Error.Failure("E2", "M2");
        var e3 = Error.Failure("E3", "M3");

        var enumerable = new[] { Result.Success(), Result.Failure(e1), Result.Failure(e2), Result.Success(), Result.Failure(e3) }.AsEnumerable();
        var r = Result.Combine(enumerable.ToArray());
        
        Assert.True(r.IsFailure);
        Assert.Equal(3, r.Error.InnerErrors.Length);
        Assert.Equal(e1, r.Error.InnerErrors[0]);
        Assert.Equal(e2, r.Error.InnerErrors[1]);
        Assert.Equal(e3, r.Error.InnerErrors[2]);
    }

    [Fact]
    public void Combine_T_Span_MultipleFailures_PreservesErrorsInOrder()
    {
        var e1 = Error.Failure("E1", "M1");
        var e2 = Error.Failure("E2", "M2");
        var e3 = Error.Failure("E3", "M3");

        var r = Result.Combine([Result.Success(1), Result.Failure<int>(e1), Result.Failure<int>(e2), Result.Failure<int>(e3)]);
        
        Assert.True(r.IsFailure);
        Assert.Equal(3, r.Error.InnerErrors.Length);
        Assert.Equal(e1, r.Error.InnerErrors[0]);
        Assert.Equal(e2, r.Error.InnerErrors[1]);
        Assert.Equal(e3, r.Error.InnerErrors[2]);
    }

    [Fact]
    public void Combine_T_IEnumerable_MultipleFailures_PreservesErrorsInOrder()
    {
        var e1 = Error.Failure("E1", "M1");
        var e2 = Error.Failure("E2", "M2");
        var e3 = Error.Failure("E3", "M3");

        var enumerable = new[] { Result.Success(1), Result.Failure<int>(e1), Result.Failure<int>(e2), Result.Failure<int>(e3) }.AsEnumerable();
        var r = Result.Combine(enumerable.ToArray());
        
        Assert.True(r.IsFailure);
        Assert.Equal(3, r.Error.InnerErrors.Length);
        Assert.Equal(e1, r.Error.InnerErrors[0]);
        Assert.Equal(e2, r.Error.InnerErrors[1]);
        Assert.Equal(e3, r.Error.InnerErrors[2]);
    }

    [Fact]
    public void Combine_T_Span_MultipleFailures()
    {
        var e1 = Error.Failure("E1", "M1");
        var e2 = Error.Failure("E2", "M2");
        var r = Result.Combine([Result.Failure<int>(e1), Result.Success(1), Result.Failure<int>(e2)]);
        Assert.True(r.IsFailure);
        Assert.Equal(WellKnownErrors.CombinedFailuresCode, r.Error.Code);
        Assert.Equal(2, r.Error.InnerErrors.Length);
        Assert.Equal(e1, r.Error.InnerErrors[0]);
        Assert.Equal(e2, r.Error.InnerErrors[1]);
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
        Assert.True(r.IsFailure);
        Assert.Equal(3, r.Error.InnerErrors.Length);
    }

    [Fact]
    public void Combine_T_Span_MultipleFailures_Three()
    {
        var e1 = Error.Failure("E1", "M1");
        var e2 = Error.Failure("E2", "M2");
        var e3 = Error.Failure("E3", "M3");
        var r = Result.Combine([Result.Failure<int>(e1), Result.Failure<int>(e2), Result.Failure<int>(e3)]);
        Assert.True(r.IsFailure);
        Assert.Equal(3, r.Error.InnerErrors.Length);
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
        Result.Combine(s, f, f);
        Result.Combine(f, s, f);
        Result.Combine(f, f, s);

        // Combine 4
        Result.Combine(s, f, f, f);
        Result.Combine(f, s, f, f);
        Result.Combine(f, f, s, f);
        Result.Combine(f, f, f, s);

        // Combine 5
        Result.Combine(s, f, f, f, f);
        Result.Combine(f, s, f, f, f);
        Result.Combine(f, f, s, f, f);
        Result.Combine(f, f, f, s, f);
        Result.Combine(f, f, f, f, s);
    }
}


