using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.Result.OpenTelemetry;
using EricksonLopez.Result.OpenTelemetry.Sample;
using OpenTelemetry.Trace;

var services = new ServiceCollection();

services.AddOpenTelemetry().WithTracing(b => b.AddConsoleExporter());
services.AddResultMetrics();

var provider = services.BuildServiceProvider();
var metrics = provider.GetRequiredService<ResultMetrics>();

Console.WriteLine("--- Running OpenTelemetry Sample ---");
await OpenTelemetryTracing.RunAsync(metrics);
Console.WriteLine("--- Finished OpenTelemetry Sample ---");
