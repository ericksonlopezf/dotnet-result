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
| Method                                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | TypesCount | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0    | Allocated | Alloc Ratio |
|------------------------------------------ |---------- |---------- |--------------- |------------ |------------ |----------- |---------:|----------:|----------:|------:|--------:|--------:|----------:|------------:|
| **&#39;Expression.Compile Cold Start (N types)&#39;** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **10**         | **1.458 ms** | **0.0045 ms** | **0.0042 ms** |  **1.13** |    **0.00** |  **1.9531** |  **51.83 KB** |        **1.00** |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 10         | 1.293 ms | 0.0028 ms | 0.0022 ms |  1.00 |    0.00 |  1.9531 |  51.97 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 10         | 1.227 ms | 0.0021 ms | 0.0017 ms |  0.95 |    0.00 |  1.9531 |  51.83 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 10         | 1.461 ms | 0.1709 ms | 0.0094 ms |  1.13 |    0.01 |  1.9531 |  51.76 KB |        1.00 |
|                                           |           |           |                |             |             |            |          |           |           |       |         |         |           |             |
| **&#39;Expression.Compile Cold Start (N types)&#39;** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **25**         | **3.703 ms** | **0.0223 ms** | **0.0209 ms** |  **1.14** |    **0.01** |  **7.8125** | **130.75 KB** |        **1.00** |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 25         | 3.234 ms | 0.0134 ms | 0.0125 ms |  1.00 |    0.01 |  7.8125 | 130.96 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 25         | 3.111 ms | 0.0221 ms | 0.0196 ms |  0.96 |    0.01 |  7.8125 |  131.1 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 25         | 3.757 ms | 1.4022 ms | 0.0769 ms |  1.16 |    0.02 |  7.8125 | 130.75 KB |        1.00 |
|                                           |           |           |                |             |             |            |          |           |           |       |         |         |           |             |
| **&#39;Expression.Compile Cold Start (N types)&#39;** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **50**         | **7.692 ms** | **0.0314 ms** | **0.0278 ms** |  **1.15** |    **0.00** | **15.6250** | **266.95 KB** |        **1.00** |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 50         | 6.703 ms | 0.0205 ms | 0.0171 ms |  1.00 |    0.00 | 15.6250 | 267.54 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 50         | 6.430 ms | 0.0279 ms | 0.0261 ms |  0.96 |    0.00 | 15.6250 | 267.37 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 50         | 7.724 ms | 2.0404 ms | 0.1118 ms |  1.15 |    0.01 | 15.6250 | 267.36 KB |        1.00 |
