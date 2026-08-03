using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using EricksonLopez.Result.FluentValidation.Sample;

var services = new ServiceCollection();
services.AddValidatorsFromAssemblyContaining<Program>();

var provider = services.BuildServiceProvider();

Console.WriteLine("--- Running FluentValidation Sample ---");
await FluentValidationExample.RunAsync(provider);
Console.WriteLine("--- Finished FluentValidation Sample ---");
