// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultLinqExtensionsTests : ResultExtensionsTestsBase
{
    [Fact]
    public void Select_WhenSourceIsSuccess_ReturnsMappedValue()
    {
        var result = from x in Result.Success(10)
                     select x * 2;
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public void Select_WhenSourceIsFailure_PropagatesFailure()
    {
        var result = from x in Result.Failure<int>(TestError)
                     select x * 2;
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestError);
    }

    [Fact]
    public void Select_OnUninitialized_ThrowsInvalidOperationException()
    {
        Result<int> uninitialized = default;
        var ex = Assert.Throws<InvalidOperationException>(() =>
            uninitialized.Select(x => x * 2));
        ex.Message.Should().Contain("Cannot use LINQ Select on an uninitialized default Result<TSource>");
    }

    [Fact]
    public void SelectMany_WhenBothAreSuccess_ReturnsCombinedValue()
    {
        var result = from x in Result.Success(10)
                     from y in Result.Success(20)
                     select x + y;
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(30);
    }

    [Fact]
    public void SelectMany_WhenOuterIsFailure_PropagatesOuterFailure()
    {
        var result = from x in Result.Failure<int>(TestError)
                     from y in Result.Success(20)
                     select x + y;
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestError);
    }

    [Fact]
    public void SelectMany_WhenInnerIsFailure_PropagatesInnerFailure()
    {
        var result = from x in Result.Success(10)
                     from y in Result.Failure<int>(TestError2)
                     select x + y;
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestError2);
    }

    [Fact]
    public void SelectMany_OnUninitialized_ThrowsInvalidOperationException()
    {
        Result<int> uninitialized = default;
        var ex = Assert.Throws<InvalidOperationException>(() =>
            uninitialized.SelectMany(x => Result.Success(x), (x, y) => x + y));
        ex.Message.Should().Contain("Cannot use LINQ SelectMany on an uninitialized default Result<TSource>");
    }

    [Fact]
    public void Where_WhenPredicatePasses_ReturnsSuccess()
    {
        var result = from x in Result.Success(15)
                     where x > 10
                     select x;
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(15);
    }

    [Fact]
    public void Where_WhenPredicateFails_ReturnsFilteredOutError()
    {
        var result = from x in Result.Success(5)
                     where x > 10
                     select x;
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Result.FilteredOut");
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Description.Should().Be("The result value did not satisfy the filter predicate.");
    }

    [Fact]
    public void Where_WhenSourceIsFailure_PreservesOriginalError()
    {
        var result = from x in Result.Failure<int>(TestError)
                     where x > 0
                     select x;
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestError);
    }

    [Fact]
    public void Where_OnUninitialized_ThrowsInvalidOperationException()
    {
        Result<int> uninitialized = default;
        var ex = Assert.Throws<InvalidOperationException>(() =>
            uninitialized.Where(x => x > 0));
        ex.Message.Should().Contain("Cannot use LINQ Where on an uninitialized default Result<TSource>");
    }
}


