```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.32 (8.0.32, 8.0.3226.50405), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.21 (9.0.21, 9.0.2126.50404), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3


```
| Method                                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | TypesCount | Mean     | Error     | StdDev    | Ratio | Gen0    | Allocated | Alloc Ratio |
|------------------------------------------ |---------- |---------- |--------------- |------------ |------------ |----------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| **&#39;Expression.Compile Cold Start (N types)&#39;** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **10**         | **1.756 ms** | **0.0018 ms** | **0.0015 ms** |  **1.08** |       **-** |  **51.61 KB** |        **1.00** |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 10         | 1.619 ms | 0.0023 ms | 0.0019 ms |  1.00 |       - |  51.61 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 10         | 1.562 ms | 0.0022 ms | 0.0018 ms |  0.96 |       - |  51.89 KB |        1.01 |
| &#39;Expression.Compile Cold Start (N types)&#39; | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 10         | 1.793 ms | 0.0431 ms | 0.0024 ms |  1.11 |       - |  51.61 KB |        1.00 |
|                                           |           |           |                |             |             |            |          |           |           |       |         |           |             |
| **&#39;Expression.Compile Cold Start (N types)&#39;** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **25**         | **4.604 ms** | **0.0154 ms** | **0.0136 ms** |  **1.10** |  **7.8125** | **130.89 KB** |        **1.00** |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 25         | 4.191 ms | 0.0078 ms | 0.0061 ms |  1.00 |  7.8125 | 130.89 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 25         | 4.067 ms | 0.0156 ms | 0.0138 ms |  0.97 |  7.8125 | 130.82 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 25         | 4.603 ms | 0.4806 ms | 0.0263 ms |  1.10 |  7.8125 | 130.75 KB |        1.00 |
|                                           |           |           |                |             |             |            |          |           |           |       |         |           |             |
| **&#39;Expression.Compile Cold Start (N types)&#39;** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **50**         | **9.428 ms** | **0.0215 ms** | **0.0201 ms** |  **1.08** | **15.6250** | **267.64 KB** |        **1.00** |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 50         | 8.703 ms | 0.0430 ms | 0.0403 ms |  1.00 | 15.6250 | 266.91 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 50         | 8.377 ms | 0.0480 ms | 0.0449 ms |  0.96 | 15.6250 |  267.4 KB |        1.00 |
| &#39;Expression.Compile Cold Start (N types)&#39; | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 50         | 9.504 ms | 0.6646 ms | 0.0364 ms |  1.09 | 15.6250 | 267.37 KB |        1.00 |
