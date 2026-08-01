using System;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ErrorExhaustiveTests
{
    [Fact]
    public void Error_TraceId_Initialization()
    {
        var e1 = Error.Failure("code", "msg");
        Assert.Null(e1.TraceId);
        
        var e2 = Error.Create("code", "msg").WithTraceId(Guid.NewGuid().ToString()).Build();
        Assert.NotNull(e2.TraceId);
    }
    
    [Fact]
    public void Error_Failure_Validation()
    {
        Assert.Throws<ArgumentException>(() => Error.Failure("", "msg"));
    }
}



