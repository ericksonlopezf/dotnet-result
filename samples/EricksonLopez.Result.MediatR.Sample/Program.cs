using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using EricksonLopez.Result.MediatR;
using EricksonLopez.Result.MediatR.Sample;

var services = new ServiceCollection();

services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ResultExceptionBehavior<,>));
});

var provider = services.BuildServiceProvider();

Console.WriteLine("--- Running MediatR Sample ---");
await MediatRExample.RunAsync(provider);
Console.WriteLine("--- Finished MediatR Sample ---");
