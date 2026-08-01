using System;
using Xunit;
using EricksonLopez.Result.AspNetCore;

namespace EricksonLopez.Result.Tests.AspNetCore;

public class ResultHttpOptionsTests
{
    [Fact]
    public void ConfigureStatusCode_BeforeFreeze_UpdatesMap()
    {
        var options = new ResultHttpOptions();
        options.ConfigureStatusCode(ErrorType.Domain, 418);
        
        Assert.Equal(418, options.StatusCodeMap[ErrorType.Domain]);
    }

    [Fact]
    public void ConfigureStatusCode_AfterFreeze_ThrowsInvalidOperationException()
    {
        var options = new ResultHttpOptions();
        
        var error = Error.Validation("C", "M");
        Result.Failure(error).ToHttpResult(options); 
        
        // IsFrozen is internal — verified indirectly via the InvalidOperationException below
        Assert.Throws<InvalidOperationException>(() => options.ConfigureStatusCode(ErrorType.Domain, 418));
    }
}
