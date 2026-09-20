```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


```
| Method                                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | TypesCount | Mean     | Error     | StdDev    | Ratio | Gen0    | Allocated | Alloc Ratio |
|------------------------------------------ |---------- |---------- |--------------- |------------ |------------ |----------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| **&#39;Expression.Compile Cold Start (N types)&#39;** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **10**         | **1.737 ms** | **0.0026 ms** | **0.0025 ms** |  **1.08** |       **-** |  **51.61 KB** |        **1.00** |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 10         | 1.606 ms | 0.0113 ms | 0.0105 ms |  1.00 |       - |  51.61 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 10         | 1.580 ms | 0.0020 ms | 0.0018 ms |  0.98 |  1.9531 |  51.97 KB |        1.01 |
| &#39;Expression.Compile Cold Start (N types)&#39; | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 10         | 1.765 ms | 0.2405 ms | 0.0132 ms |  1.10 |       - |  51.75 KB |        1.00 |
|                                           |           |           |                |             |             |            |          |           |           |       |         |           |             |
| **&#39;Expression.Compile Cold Start (N types)&#39;** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **25**         | **4.523 ms** | **0.0279 ms** | **0.0248 ms** |  **1.11** |  **7.8125** | **130.89 KB** |        **1.00** |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 25         | 4.062 ms | 0.0080 ms | 0.0071 ms |  1.00 |  7.8125 |  131.1 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 25         | 4.051 ms | 0.0211 ms | 0.0198 ms |  1.00 |  7.8125 |  131.1 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 25         | 4.539 ms | 0.3441 ms | 0.0189 ms |  1.12 |  7.8125 | 130.89 KB |        1.00 |
|                                           |           |           |                |             |             |            |          |           |           |       |         |           |             |
| **&#39;Expression.Compile Cold Start (N types)&#39;** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **50**         | **9.274 ms** | **0.0393 ms** | **0.0367 ms** |  **1.10** | **15.6250** | **266.81 KB** |        **1.00** |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 50         | 8.453 ms | 0.0449 ms | 0.0420 ms |  1.00 | 15.6250 | 267.77 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 50         | 8.205 ms | 0.0180 ms | 0.0150 ms |  0.97 | 15.6250 | 266.35 KB |        0.99 |
| &#39;Expression.Compile Cold Start (N types)&#39; | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 50         | 9.243 ms | 1.0580 ms | 0.0580 ms |  1.09 | 15.6250 |  267.3 KB |        1.00 |
