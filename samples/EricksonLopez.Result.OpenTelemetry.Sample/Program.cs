// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.OpenTelemetry;
using EricksonLopez.Result.OpenTelemetry.Sample;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

using var meter = new Meter(ResultMetrics.MeterName, "1.0.0");
using var metrics = new ResultMetrics(meter, ownsMeter: false);

var services = new ServiceCollection();
services.AddSingleton(metrics);
services.AddOpenTelemetry().WithTracing(b => b.AddConsoleExporter());

var provider = services.BuildServiceProvider();
var resolvedMetrics = provider.GetRequiredService<ResultMetrics>();

Console.WriteLine("--- Running OpenTelemetry Sample ---");
await OpenTelemetryTracing.RunAsync(resolvedMetrics);
Console.WriteLine("--- Finished OpenTelemetry Sample ---");
