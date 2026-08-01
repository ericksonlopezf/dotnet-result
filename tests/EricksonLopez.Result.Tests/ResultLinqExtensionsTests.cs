using System;
using Xunit;

namespace EricksonLopez.Result.Tests;

public class ResultLinqExtensionsTests
{
    [Fact]
    public void Select_Throws_WhenUninitialized()
    {
        Result<int> uninitialized = default;
        Assert.Throws<InvalidOperationException>(() => uninitialized.Select(x => x * 2));
    }

    [Fact]
    public void SelectMany_Throws_WhenUninitialized()
    {
        Result<int> uninitialized = default;
        Assert.Throws<InvalidOperationException>(() => uninitialized.SelectMany(x => Result.Success(x), (x, y) => x + y));
    }

    [Fact]
    public void Where_Throws_WhenUninitialized()
    {
        Result<int> uninitialized = default;
        Assert.Throws<InvalidOperationException>(() => uninitialized.Where(x => x > 0));
    }

    [Fact]
    public void Where_FiltersOutValue()
    {
        var result = Result.Success(5);
        var filtered = result.Where(x => x > 10);
        Assert.True(filtered.IsFailure);
        Assert.Equal("Result.FilteredOut", filtered.Error.Code);
    }
}
