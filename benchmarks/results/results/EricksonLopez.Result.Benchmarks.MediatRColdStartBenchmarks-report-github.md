```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.68GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4


```
| Method                                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | TypesCount | Mean     | Error     | StdDev    | Ratio | Gen0    | Allocated | Alloc Ratio |
|------------------------------------------ |---------- |---------- |--------------- |------------ |------------ |----------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| **&#39;Expression.Compile Cold Start (N types)&#39;** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **10**         | **1.451 ms** | **0.0022 ms** | **0.0019 ms** |  **1.13** |  **1.9531** |  **51.83 KB** |        **1.00** |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 10         | 1.288 ms | 0.0070 ms | 0.0062 ms |  1.00 |  1.9531 |  51.83 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 10         | 1.233 ms | 0.0053 ms | 0.0045 ms |  0.96 |  1.9531 |  51.83 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 10         | 1.452 ms | 0.0738 ms | 0.0040 ms |  1.13 |  1.9531 |   51.9 KB |        1.00 |
|                                           |           |           |                |             |             |            |          |           |           |       |         |           |             |
| **&#39;Expression.Compile Cold Start (N types)&#39;** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **25**         | **3.684 ms** | **0.0081 ms** | **0.0067 ms** |  **1.14** |  **7.8125** | **131.03 KB** |        **1.00** |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 25         | 3.239 ms | 0.0122 ms | 0.0102 ms |  1.00 |  7.8125 | 130.61 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 25         | 3.106 ms | 0.0217 ms | 0.0203 ms |  0.96 |  7.8125 | 130.89 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 25         | 3.709 ms | 1.0286 ms | 0.0564 ms |  1.15 |  7.8125 | 130.82 KB |        1.00 |
|                                           |           |           |                |             |             |            |          |           |           |       |         |           |             |
| **&#39;Expression.Compile Cold Start (N types)&#39;** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **50**         | **7.685 ms** | **0.0525 ms** | **0.0439 ms** |  **1.13** | **15.6250** | **267.43 KB** |        **1.00** |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 50         | 6.789 ms | 0.0337 ms | 0.0315 ms |  1.00 | 15.6250 | 266.49 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 50         | 6.541 ms | 0.0592 ms | 0.0525 ms |  0.96 | 15.6250 | 267.47 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 50         | 7.700 ms | 1.9487 ms | 0.1068 ms |  1.13 | 15.6250 | 267.08 KB |        1.00 |
