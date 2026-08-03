using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;

namespace EricksonLopez.Result.AspNetCore.Sample;

public static class AspNetCoreEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        // 12. ASP.NET CORE MINIMAL APIs
        
        // This endpoint demonstrates returning a Result<T> directly.
        // The `AddResultEndpointFilter()` configured in Program.cs automatically 
        // intercepts `Result` and maps it to the appropriate HTTP status code!
        app.MapGet("/users/{id:int}", (int id) =>
        {
            if (id == 999)
            {
                var error = Error.Create("User.NotFound", $"User {id} was not found.")
                    .WithType(ErrorType.NotFound)
                    .Build();
                    
                return Result.Failure<string>(error);
            }
            
            if (id < 0)
            {
                var error = Error.Create("User.InvalidId", "ID cannot be negative.")
                    .WithType(ErrorType.Validation)
                    .Build();
                    
                return Result.Failure<string>(error);
            }

            return Result.Success($"User Profile {id}");
        })
        .WithName("GetUserById");

        // Alternatively, if you don't use the endpoint filter, you can manually map to IResult:
        app.MapGet("/manual/{id:int}", (int id) =>
        {
            if (id == 999)
            {
                return Result.Failure<string>(
                    Error.Create("User.NotFound", "Not found").WithType(ErrorType.NotFound).Build()
                ).ToHttpResult(); // Manual mapping
            }

            return Result.Success("Manual success").ToHttpResult();
        });

        // 3. ToProblemDetails and custom ResultHttpOptions
        app.MapGet("/problem/{id:int}", (int id) =>
        {
            var options = new ResultHttpOptions()
                .ConfigureStatusCode(ErrorType.Validation, 422) // Custom override
                .ConfigureTitleOverride(ErrorType.Validation, "Custom Validation Title");

            var error = Error.Validation("Input.Bad", "Bad input provided.");
            var result = Result.Failure<string>(error);
            
            // ToProblemDetails explicitly returns an IResult formatted as ProblemDetails regardless of IsSuccess
            return result.ToProblemDetails(options);
        });
    }
}

