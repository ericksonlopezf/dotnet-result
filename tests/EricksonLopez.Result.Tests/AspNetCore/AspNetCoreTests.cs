using System;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace EricksonLopez.Result.Tests.AspNetCore;

public class AspNetCoreTests
{
    [Fact]
    public void ToHttpResult_Success_ReturnsNoContentOrOk()
    {
        var nonGeneric = Result.Success();
        var httpResult1 = nonGeneric.ToHttpResult();
        Assert.NotNull(httpResult1);

        var generic = Result.Success("Payload");
        var httpResult2 = generic.ToHttpResult();
        Assert.NotNull(httpResult2);
    }

    [Fact]
    public void ToHttpResult_Failure_ReturnsProblemDetails()
    {
        var failure = Result.Failure(Error.NotFound("Item.NotFound", "Item was not found"));
        var httpResult = failure.ToHttpResult();
        Assert.NotNull(httpResult);
    }

    [Fact]
    public void ResultHttpOptions_CustomStatusCodeMapping_IsRespected()
    {
        var options = new ResultHttpOptions();
        options.ConfigureStatusCode(ErrorType.Validation, StatusCodes.Status422UnprocessableEntity);

        var validationError = Error.Validation("Input.Invalid", "Invalid format");
        var result = Result.Failure(validationError);

        var httpResult = result.ToHttpResult(options);
        Assert.NotNull(httpResult);
    }

    [Fact]
    public async Task ResultEndpointFilter_SuccessGeneric_ReturnsOk()
    {
        var filter = new ResultEndpointFilter(new ResultHttpOptions());
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext());
        EndpointFilterDelegate next = _ => new ValueTask<object?>(Result<string>.Success("test"));

        var result = await filter.InvokeAsync(context, next);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<object>>(result);
    }

    [Fact]
    public async Task ResultEndpointFilter_FailureGeneric_ReturnsProblemDetails()
    {
        var filter = new ResultEndpointFilter(new ResultHttpOptions());
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext());
        EndpointFilterDelegate next = _ => new ValueTask<object?>(Result<string>.Failure(Error.NotFound("A", "B")));

        var result = await filter.InvokeAsync(context, next);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>(result);
    }

    [Fact]
    public void ResultHttpOptions_DefaultStatusCode_ReturnedForUnmappedErrorType()
    {
        var options = new ResultHttpOptions();
        var customError = Error.Create("Custom", "Custom Error").WithType(ErrorType.Custom).Build();
        var result = Result.Failure(customError);
        var httpResult = result.ToHttpResult(options);
        
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>(httpResult);
        var problem = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)httpResult;
        // Audit B2.3: ErrorType.Custom default changed from 400 to 500.
        // Custom errors may indicate server-side/domain failures — 400 incorrectly implies client fault.
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
    }
}
