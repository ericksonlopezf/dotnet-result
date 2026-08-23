// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultThrowHelperTests
{
    [Fact]
    public void ThrowUninitialized_ThrowsInvalidOperationException_WithExpectedMessage()
    {
        var action = () => ResultThrowHelper.ThrowUninitialized();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot operate on an uninitialized default Result. Always construct Result via Result.Success() or Result.Failure(error).");
    }

    [Fact]
    public void ThrowUninitializedOfT_ThrowsInvalidOperationException_WithExpectedMessage()
    {
        var action = () => ResultThrowHelper.ThrowUninitializedOfT();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot operate on an uninitialized default Result<TValue>. Always construct Result<TValue> via Result.Success(value) or Result.Failure(error).");
    }
}

