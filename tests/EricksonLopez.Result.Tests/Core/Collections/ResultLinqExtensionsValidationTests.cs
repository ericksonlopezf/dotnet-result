// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultLinqExtensionsValidationTests
{
    [Fact]
    public void LinqExtensions_NullValidations()
    {
        var r = Result.Success(1);
        Assert.Throws<NullReferenceException>(() => ResultLinqExtensions.Select<int, int>(r, null!));
        Assert.Throws<NullReferenceException>(() => ResultLinqExtensions.SelectMany<int, int, int>(r, null!, (a, b) => 1));
        Assert.Throws<NullReferenceException>(() => ResultLinqExtensions.SelectMany<int, int, int>(r, x => Result.Success(1), null!));
    }
}



