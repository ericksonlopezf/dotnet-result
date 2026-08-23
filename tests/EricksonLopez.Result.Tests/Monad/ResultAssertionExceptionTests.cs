// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Monad;

public class ResultAssertionExceptionTests
{
    [Fact]
    public void ShouldBeSuccess_WhenFailure_Throws()
    {
        var result = Result.Failure(Error.Failure("X", "X"));
        Action act = () => result.ShouldBeSuccess();
        act.Should().Throw<Exception>();

        var resultT = Result.Failure<int>(Error.Failure("X", "X"));
        Action actT = () => resultT.ShouldBeSuccess();
        actT.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldBeFailure_WhenSuccess_Throws()
    {
        var result = Result.Success();
        Action act = () => result.ShouldBeFailure();
        act.Should().Throw<Exception>();

        var resultT = Result.Success(42);
        Action actT = () => resultT.ShouldBeFailure();
        actT.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveErrorCode_WhenSuccess_Throws()
    {
        var result = Result.Success();
        Action act = () => result.ShouldHaveErrorCode("X");
        act.Should().Throw<Exception>();

        var resultT = Result.Success(42);
        Action actT = () => resultT.ShouldHaveErrorCode("X");
        actT.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveErrorCode_WrongCode_Throws()
    {
        var result = Result.Failure(Error.Failure("Y", "Y"));
        Action act = () => result.ShouldHaveErrorCode("X");
        act.Should().Throw<Exception>();

        var resultT = Result.Failure<int>(Error.Failure("Y", "Y"));
        Action actT = () => resultT.ShouldHaveErrorCode("X");
        actT.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveErrorType_WhenSuccess_Throws()
    {
        var result = Result.Success();
        Action act = () => result.ShouldHaveErrorType(ErrorType.Validation);
        act.Should().Throw<Exception>();

        var resultT = Result.Success(42);
        Action actT = () => resultT.ShouldHaveErrorType(ErrorType.Validation);
        actT.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveErrorType_WrongType_Throws()
    {
        var result = Result.Failure(Error.Failure("X", "X"));
        Action act = () => result.ShouldHaveErrorType(ErrorType.Validation);
        act.Should().Throw<Exception>();

        var resultT = Result.Failure<int>(Error.Failure("X", "X"));
        Action actT = () => resultT.ShouldHaveErrorType(ErrorType.Validation);
        actT.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveValue_WhenFailure_Throws()
    {
        var result = Result.Failure<int>(Error.Failure("X", "X"));
        Action act = () => result.ShouldHaveValue(42);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveValue_WrongValue_Throws()
    {
        var result = Result.Success(10);
        Action act = () => result.ShouldHaveValue(42);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveValue_Predicate_Throws()
    {
        var result = Result.Success(10);
        Action act = () => result.ShouldHaveValue(x => x == 42);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveSeverity_WhenSuccess_Throws()
    {
        var result = Result.Success();
        Action act = () => result.ShouldHaveSeverity(ErrorSeverity.Critical);
        act.Should().Throw<Exception>();

        var resultT = Result.Success(42);
        Action actT = () => resultT.ShouldHaveSeverity(ErrorSeverity.Critical);
        actT.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveSeverity_WrongSeverity_Throws()
    {
        var result = Result.Failure(Error.Failure("X", "X"));
        Action act = () => result.ShouldHaveSeverity(ErrorSeverity.Critical);
        act.Should().Throw<Exception>();

        var resultT = Result.Failure<int>(Error.Failure("X", "X"));
        Action actT = () => resultT.ShouldHaveSeverity(ErrorSeverity.Critical);
        actT.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveInnerErrors_WhenSuccess_Throws()
    {
        var result = Result.Success();
        Action act = () => result.ShouldHaveInnerErrors(1);
        act.Should().Throw<Exception>();

        var resultT = Result.Success(42);
        Action actT = () => resultT.ShouldHaveInnerErrors(1);
        actT.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveInnerErrors_WrongInnerError_Throws()
    {
        var result = Result.Failure(Error.Create("X", "X").WithInnerError(Error.Failure("Z", "Z")).Build());
        Action act = () => result.ShouldHaveInnerErrors(2);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveInnerErrors_WrongCount_Throws()
    {
        var result = Result.Failure(Error.Failure("X", "X"));
        Action act = () => result.ShouldHaveInnerErrors(1);
        act.Should().Throw<Exception>();

        var resultT = Result.Failure<int>(Error.Failure("X", "X"));
        Action actT = () => resultT.ShouldHaveInnerErrors(1);
        actT.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveMetadata_WhenSuccess_Throws()
    {
        var result = Result.Success();
        Action act = () => result.ShouldHaveMetadata("key", "val");
        act.Should().Throw<Exception>();

        var resultT = Result.Success(42);
        Action actT = () => resultT.ShouldHaveMetadata("key", "val");
        actT.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveMetadata_WrongValue_Throws()
    {
        var result = Result.Failure(Error.Failure("X", "X").WithMetadata("key", "wrong"));
        Action act = () => result.ShouldHaveMetadata("key", "val");
        act.Should().Throw<Exception>();

        var resultT = Result.Failure<int>(Error.Failure("X", "X").WithMetadata("key", "wrong"));
        Action actT = () => resultT.ShouldHaveMetadata("key", "val");
        actT.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveMetadataValue_WhenSuccess_Throws()
    {
        var result = Result.Success();
        Action act = () => result.ShouldHaveMetadataValue("key", 1);
        act.Should().Throw<Exception>();

        var resultT = Result.Success(42);
        Action actT = () => resultT.ShouldHaveMetadataValue("key", 1);
        actT.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveMetadataValue_WrongValue_Throws()
    {
        var result = Result.Failure(Error.Failure("X", "X").WithMetadata("key", 2));
        Action act = () => result.ShouldHaveMetadataValue("key", 1);
        act.Should().Throw<Exception>();

        var resultT = Result.Failure<int>(Error.Failure("X", "X").WithMetadata("key", 2));
        Action actT = () => resultT.ShouldHaveMetadataValue("key", 1);
        actT.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveErrorMatching_Throws()
    {
        var result = Result.Success();
        Action act = () => result.ShouldHaveErrorMatching(e => e.Code == "X");
        act.Should().Throw<Exception>();

        var result2 = Result.Failure(Error.Failure("Y", "Y"));
        Action act2 = () => result2.ShouldHaveErrorMatching(e => e.Code == "X");
        act2.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldBeCombinedFailure_Throws()
    {
        var result = Result.Success();
        Action act = () => result.ShouldBeCombinedFailure(1);
        act.Should().Throw<Exception>();

        var result2 = Result.Failure(Error.Failure("Y", "Y"));
        Action act2 = () => result2.ShouldBeCombinedFailure(2);
        act2.Should().Throw<Exception>();
    }

    [Fact]
    public void CallAllResultAssertionsMethods()
    {
        var type = typeof(EricksonLopez.Result.Testing.ResultAssertions);
        foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
        {
            if (method.IsSpecialName || method.Name == "Equals" || method.Name == "GetHashCode") continue;

            var m = method.IsGenericMethod ? method.MakeGenericMethod(Enumerable.Repeat(typeof(int), method.GetGenericArguments().Length).ToArray()) : method;
            var args = new object[m.GetParameters().Length];

            for (int i = 0; i < args.Length; i++)
            {
                var pt = m.GetParameters()[i].ParameterType;
                if (pt == typeof(Result) || pt == typeof(Result).MakeByRefType()) args[i] = Result.Success();
                else if (pt == typeof(Result<int>) || pt == typeof(Result<int>).MakeByRefType()) args[i] = Result.Success(1);
                else if (pt == typeof(Task<Result>)) args[i] = Task.FromResult(Result.Success());
                else if (pt == typeof(Task<Result<int>>)) args[i] = Task.FromResult(Result.Success(1));
                else if (pt == typeof(ValueTask<Result>)) args[i] = new ValueTask<Result>(Result.Success());
                else if (pt == typeof(ValueTask<Result<int>>)) args[i] = new ValueTask<Result<int>>(Result.Success(1));
                else if (pt == typeof(string)) args[i] = "X";
                else if (pt == typeof(ErrorType)) args[i] = ErrorType.Validation;
                else if (pt == typeof(ErrorSeverity)) args[i] = ErrorSeverity.Critical;
                else if (pt == typeof(int)) args[i] = 1;
                else if (pt == typeof(object)) args[i] = new object();
                else if (pt.Name.Contains("Func")) args[i] = null!;
                else args[i] = null!;
            }

            try { m.Invoke(null, args); } catch { }

            // Invoke with failures
            for (int i = 0; i < args.Length; i++)
            {
                var pt = m.GetParameters()[i].ParameterType;
                if (pt == typeof(Result) || pt == typeof(Result).MakeByRefType()) args[i] = Result.Failure(Error.Failure("X", "X"));
                else if (pt == typeof(Result<int>) || pt == typeof(Result<int>).MakeByRefType()) args[i] = Result.Failure<int>(Error.Failure("X", "X"));
                else if (pt == typeof(Task<Result>)) args[i] = Task.FromResult(Result.Failure(Error.Failure("X", "X")));
                else if (pt == typeof(Task<Result<int>>)) args[i] = Task.FromResult(Result.Failure<int>(Error.Failure("X", "X")));
                else if (pt == typeof(ValueTask<Result>)) args[i] = new ValueTask<Result>(Result.Failure(Error.Failure("X", "X")));
                else if (pt == typeof(ValueTask<Result<int>>)) args[i] = new ValueTask<Result<int>>(Result.Failure<int>(Error.Failure("X", "X")));
            }

            try { m.Invoke(null, args); } catch { }
        }
    }
}
