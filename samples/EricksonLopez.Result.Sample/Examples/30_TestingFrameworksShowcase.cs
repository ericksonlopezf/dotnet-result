// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using EricksonLopez.Result.Testing.NUnit;
using EricksonLopez.Result.Testing.XUnit;

namespace EricksonLopez.Result.Sample.Examples;

public static class TestingFrameworksShowcase
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 30. TEST RUNNER ADAPTERS SHOWCASE (NUNIT & XUNIT)");
        Console.WriteLine("========================================================");

        // 1. Standard Testing Assertions (agnostic)
        Console.WriteLine("\n[1] Framework-agnostic ResultAssertions:");
        Result<int> successResult = Result.Success(42);
        int value = successResult.ShouldBeSuccess();
        Console.WriteLine($"  ShouldBeSuccess returned unwrapped value: {value}");

        Result failureResult = Result.Failure(Error.Validation("Input.Invalid", "Invalid number."));
        Error err = failureResult.ShouldBeFailure();
        Console.WriteLine($"  ShouldBeFailure returned error code: [{err.Code}]");

        // 2. Configure NUnit Exception Adapter
        Console.WriteLine("\n[2] ResultNUnitAssertionConfig — configure NUnit exceptions:");
        ResultNUnitAssertionConfig.UseNUnitExceptions();
        ResultAssertionException.ExceptionFactory = static msg => new ResultAssertionNUnitException(msg);
        var nunitEx = ResultAssertionException.ExceptionFactory("Expected Result to be Success in NUnit runner");
        Console.WriteLine($"  Configured NUnit adapter: {nunitEx.GetType().Name}");
        Console.WriteLine($"  Message: {nunitEx.Message}");

        // 3. Configure xUnit Exception Adapter
        Console.WriteLine("\n[3] ResultXUnitAssertionConfig — configure xUnit exceptions:");
        ResultXUnitAssertionConfig.UseXUnitExceptions();
        ResultAssertionException.ExceptionFactory = static msg => new ResultAssertionXUnitException(msg);
        var xunitEx = ResultAssertionException.ExceptionFactory("Expected Result to be Failure in xUnit runner");
        Console.WriteLine($"  Configured xUnit adapter: {xunitEx.GetType().Name}");
        Console.WriteLine($"  Message: {xunitEx.Message}");
    }
}
