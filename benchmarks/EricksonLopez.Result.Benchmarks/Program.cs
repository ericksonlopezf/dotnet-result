using BenchmarkDotNet.Running;
using EricksonLopez.Result.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(ResultConstructionBenchmarks).Assembly).Run(args);
