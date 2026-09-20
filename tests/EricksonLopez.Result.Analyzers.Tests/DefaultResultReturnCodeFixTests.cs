// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading.Tasks;
using EricksonLopez.Result.Analyzers;
using Xunit;

namespace EricksonLopez.Result.Analyzers.Tests;

public class DefaultResultReturnCodeFixTests
{
    [Fact]
    public async Task RESULT012_CodeFix_Rewrites_ReturnDefault_To_ResultSuccess()
    {
        const string source = @"
public class TestService
{
    public EricksonLopez.Result.Result GetStatus()
    {
        return default;
    }
}";
        var fixedCode = await AnalyzerTestHelper.ApplyCodeFixAsync<DefaultResultReturnAnalyzer, DefaultResultReturnCodeFix>(source, "RESULT012");
        Assert.Contains("return Result.Success();", fixedCode);
        Assert.DoesNotContain("return default;", fixedCode);
    }

    [Fact]
    public async Task RESULT012_CodeFix_Rewrites_ReturnDefaultOfT_To_ResultSuccess()
    {
        const string source = @"
public class TestService
{
    public EricksonLopez.Result.Result<int> GetCount()
    {
        return default;
    }
}";
        var fixedCode = await AnalyzerTestHelper.ApplyCodeFixAsync<DefaultResultReturnAnalyzer, DefaultResultReturnCodeFix>(source, "RESULT012");
        Assert.Contains("return Result.Success(default(int)!);", fixedCode);
        Assert.DoesNotContain("return default;", fixedCode);
    }

    [Fact]
    public async Task RESULT012_CodeFix_Offers_Both_Success_And_Failure_Actions()
    {
        const string source = @"
public class TestService
{
    public EricksonLopez.Result.Result GetStatus()
    {
        return default;
    }
}";
        var actions = await AnalyzerTestHelper.GetCodeActionsAsync<DefaultResultReturnAnalyzer, DefaultResultReturnCodeFix>(source, "RESULT012");
        Assert.Equal(2, actions.Length);
        Assert.Contains(actions, a => a.Title.StartsWith("Return Result.Success", StringComparison.Ordinal));
        Assert.Contains(actions, a => a.Title.StartsWith("Return Result.Failure", StringComparison.Ordinal));
    }
}
