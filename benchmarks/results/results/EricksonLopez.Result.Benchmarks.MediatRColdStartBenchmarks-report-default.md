
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.21GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4


 Method                                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | TypesCount | Mean       | Error       | StdDev    | Ratio | RatioSD | Gen0    | Gen1    | Allocated | Alloc Ratio |
------------------------------------------ |---------- |---------- |--------------- |------------ |------------ |----------- |-----------:|------------:|----------:|------:|--------:|--------:|--------:|----------:|------------:|
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **10**         |   **699.9 μs** |    **13.90 μs** |  **11.61 μs** |  **1.07** |    **0.02** |  **2.9297** |  **1.9531** |  **51.91 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 10         |   656.8 μs |    11.90 μs |  10.55 μs |  1.00 |    0.02 |  2.9297 |  1.9531 |   51.9 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 10         |   573.0 μs |     4.90 μs |   4.58 μs |  0.87 |    0.01 |  2.9297 |  1.9531 |   51.9 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 10         |   723.1 μs |   230.21 μs |  12.62 μs |  1.10 |    0.02 |  2.9297 |  1.9531 |  51.83 KB |        1.00 |
                                           |           |           |                |             |             |            |            |             |           |       |         |         |         |           |             |
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **25**         | **1,700.2 μs** |    **26.01 μs** |  **23.06 μs** |  **1.09** |    **0.02** |  **7.8125** |  **3.9063** | **131.14 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 25         | 1,562.0 μs |    20.37 μs |  15.90 μs |  1.00 |    0.01 |  7.8125 |  3.9063 | 130.71 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 25         | 1,403.9 μs |    28.04 μs |  31.16 μs |  0.90 |    0.02 |  7.8125 |  5.8594 | 130.95 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 25         | 1,756.1 μs | 2,488.74 μs | 136.42 μs |  1.12 |    0.07 |  7.8125 |  3.9063 | 130.66 KB |        1.00 |
                                           |           |           |                |             |             |            |            |             |           |       |         |         |         |           |             |
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **50**         | **3,487.5 μs** |    **49.16 μs** |  **43.58 μs** |  **1.06** |    **0.02** | **15.6250** |  **7.8125** | **267.51 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 50         | 3,298.7 μs |    56.54 μs |  47.21 μs |  1.00 |    0.02 | 15.6250 |  7.8125 | 267.34 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 50         | 2,895.4 μs |    28.56 μs |  23.85 μs |  0.88 |    0.01 | 15.6250 | 11.7188 | 266.48 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 50         | 3,611.6 μs | 2,847.35 μs | 156.07 μs |  1.10 |    0.04 | 15.6250 |  7.8125 | 266.76 KB |        1.00 |
