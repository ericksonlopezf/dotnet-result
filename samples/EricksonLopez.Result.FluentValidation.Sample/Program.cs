// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.FluentValidation.Sample;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddValidatorsFromAssemblyContaining<Program>();

var provider = services.BuildServiceProvider();

Console.WriteLine("--- Running FluentValidation Sample ---");
await FluentValidationExample.RunAsync(provider);
Console.WriteLine("--- Finished FluentValidation Sample ---");



