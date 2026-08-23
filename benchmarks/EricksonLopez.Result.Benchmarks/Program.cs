// Copyright © Erickson Lopez. MIT License.
using BenchmarkDotNet.Running;
using EricksonLopez.Result;
using EricksonLopez.Result.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(ResultConstructionBenchmarks).Assembly).Run(args);

