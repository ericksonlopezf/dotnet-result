using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.Result.AspNetCore;
using EricksonLopez.Result.AspNetCore.Sample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResultHttpOptions();

var app = builder.Build();

var apiGroup = app.MapGroup("/api")
    .AddResultEndpointFilter();

AspNetCoreEndpoints.MapEndpoints(apiGroup);

app.Run();
