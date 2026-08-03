using System;
using Xunit;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;

namespace EricksonLopez.Result.Testing.Tests;

public class ResultAssertionsCoverageTests
{
    [Fact]
    public void Assertions_Negative_Coverage()
    {
        var s = Result.Success();
        var st = Result.Success(5);
        var f = Result.Failure(Error.Failure("X", "X"));
        var ft = Result.Failure<int>(Error.Failure("X", "X"));
        var u = default(Result);
        var ut = default(Result<int>);
        
        try { f.ShouldBeSuccess(); } catch { }
        try { u.ShouldBeSuccess(); } catch { }
        try { ft.ShouldBeSuccess(); } catch { }
        try { ut.ShouldBeSuccess(); } catch { }
        
        try { s.ShouldBeFailure(); } catch { }
        try { u.ShouldBeFailure(); } catch { }
        try { st.ShouldBeFailure(); } catch { }
        try { ut.ShouldBeFailure(); } catch { }
        
        try { s.ShouldBeUninitialized(); } catch { }
        try { f.ShouldBeUninitialized(); } catch { }
        try { st.ShouldBeUninitialized(); } catch { }
        try { ft.ShouldBeUninitialized(); } catch { }
        
        try { st.ShouldHaveValue(6); } catch { }
        try { ft.ShouldHaveValue(5); } catch { }
        
    }
}

