
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
INTEL XEON PLATINUM 8573C 2.30GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4


 Method                                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | TypesCount | Mean       | Error     | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
------------------------------------------ |---------- |---------- |--------------- |------------ |------------ |----------- |-----------:|----------:|---------:|------:|--------:|----------:|------------:|
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **10**         |   **998.3 μs** |   **8.21 μs** |  **7.28 μs** |  **1.12** |    **0.01** |  **51.67 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 10         |   894.2 μs |   4.83 μs |  4.28 μs |  1.00 |    0.01 |  51.67 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 10         |   845.7 μs |   9.76 μs |  8.65 μs |  0.95 |    0.01 |  51.67 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 10         | 1,011.0 μs |  72.18 μs |  3.96 μs |  1.13 |    0.01 |  51.59 KB |        1.00 |
                                           |           |           |                |             |             |            |            |           |          |       |         |           |             |
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **25**         | **2,388.6 μs** |  **22.27 μs** | **19.74 μs** |  **1.12** |    **0.01** | **130.11 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 25         | 2,125.8 μs |  21.83 μs | 18.23 μs |  1.00 |    0.01 | 130.24 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 25         | 2,099.7 μs |  31.84 μs | 28.23 μs |  0.99 |    0.02 | 129.96 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 25         | 2,377.1 μs | 288.38 μs | 15.81 μs |  1.12 |    0.01 | 129.91 KB |        1.00 |
                                           |           |           |                |             |             |            |            |           |          |       |         |           |             |
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **50**         | **4,927.9 μs** |  **36.51 μs** | **32.37 μs** |  **1.05** |    **0.01** | **265.12 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 50         | 4,699.1 μs |  48.77 μs | 43.23 μs |  1.00 |    0.01 | 264.97 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 50         | 4,231.9 μs |  29.26 μs | 27.37 μs |  0.90 |    0.01 | 265.25 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 50         | 4,882.0 μs | 943.45 μs | 51.71 μs |  1.04 |    0.01 | 264.87 KB |        1.00 |
