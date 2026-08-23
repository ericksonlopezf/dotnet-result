// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsTestsBase
{
    public static readonly Error TestError = TestErrors.Default;
    public static readonly Error TestError2 = TestErrors.Second;

    protected static CancellationToken CanceledToken => new(canceled: true);

    protected static void AssertFailureInvariant<T>(Result<T> result, Error expectedError)
    {
        result.ShouldBeFailure().Should().BeSameAs(expectedError);
    }

    protected static void AssertFailureInvariant(Result result, Error expectedError)
    {
        result.ShouldBeFailure().Should().BeSameAs(expectedError);
    }

    protected static Task<Result> IncompleteResultTask(Result result) => result.IncompleteTask();

    protected static Task<Result<T>> IncompleteResultTask<T>(Result<T> result) => result.IncompleteTask();

    protected static ValueTask<Result> IncompleteResultValueTask(Result result) => result.IncompleteValueTask();

    protected static ValueTask<Result<T>> IncompleteResultValueTask<T>(Result<T> result) => result.IncompleteValueTask();
}


