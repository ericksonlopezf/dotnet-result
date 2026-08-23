// Copyright © Erickson Lopez. MIT License.
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using EricksonLopez.Result.AspNetCore.Sample;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResultHttpOptions();

var app = builder.Build();

var apiGroup = app.MapGroup("/api")
    .AddResultEndpointFilter();

AspNetCoreEndpoints.MapEndpoints(apiGroup);

await app.RunAsync();


