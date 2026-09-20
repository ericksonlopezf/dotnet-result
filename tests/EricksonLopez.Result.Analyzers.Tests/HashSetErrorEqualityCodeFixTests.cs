// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading.Tasks;
using EricksonLopez.Result.Analyzers;
using Xunit;

namespace EricksonLopez.Result.Analyzers.Tests;

public class HashSetErrorEqualityCodeFixTests
{
    [Fact]
    public async Task RESULT007_CodeFix_Rewrites_HashSet_To_Use_StrictComparer()
    {
        const string source = @"
public class TestService
{
    public void Check()
    {
        var set = new System.Collections.Generic.HashSet<EricksonLopez.Result.Error>();
    }
}";
        var fixedCode = await AnalyzerTestHelper.ApplyCodeFixAsync<HashSetErrorEqualityAnalyzer, HashSetErrorEqualityCodeFix>(source, "RESULT007");
        Assert.Contains("new System.Collections.Generic.HashSet<EricksonLopez.Result.Error>(ErrorEqualityComparer.Strict)", fixedCode);
    }

    [Fact]
    public async Task RESULT007_CodeFix_Rewrites_Linq_Distinct_To_Use_StrictComparer()
    {
        const string source = @"
public class TestService
{
    public void Check(System.Collections.Generic.IEnumerable<EricksonLopez.Result.Error> errors)
    {
        var distinct = errors.Distinct();
    }
}";
        var fixedCode = await AnalyzerTestHelper.ApplyCodeFixAsync<HashSetErrorEqualityAnalyzer, HashSetErrorEqualityCodeFix>(source, "RESULT007");
        Assert.Contains("errors.Distinct(ErrorEqualityComparer.Strict)", fixedCode);
    }

    [Fact]
    public async Task RESULT007_CodeFix_Offers_Both_Strict_And_Default_Actions()
    {
        const string source = @"
public class TestService
{
    public void Check()
    {
        var set = new System.Collections.Generic.HashSet<EricksonLopez.Result.Error>();
    }
}";
        var actions = await AnalyzerTestHelper.GetCodeActionsAsync<HashSetErrorEqualityAnalyzer, HashSetErrorEqualityCodeFix>(source, "RESULT007");
        Assert.Equal(2, actions.Length);
        Assert.Contains(actions, a => a.Title == "Use ErrorEqualityComparer.Strict");
        Assert.Contains(actions, a => a.Title == "Use ErrorEqualityComparer.Default");
    }
}
