
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.32 (8.0.32, 8.0.3226.50405), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.21 (9.0.21, 9.0.2126.50404), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4


 Method                                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | TypesCount | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0    | Allocated | Alloc Ratio |
------------------------------------------ |---------- |---------- |--------------- |------------ |------------ |----------- |---------:|----------:|----------:|------:|--------:|--------:|----------:|------------:|
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **10**         | **1.472 ms** | **0.0021 ms** | **0.0016 ms** |  **1.14** |    **0.00** |  **1.9531** |  **51.97 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 10         | 1.292 ms | 0.0033 ms | 0.0029 ms |  1.00 |    0.00 |  1.9531 |  51.97 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 10         | 1.229 ms | 0.0040 ms | 0.0036 ms |  0.95 |    0.00 |  1.9531 |  51.83 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 10         | 1.466 ms | 0.0445 ms | 0.0024 ms |  1.14 |    0.00 |  1.9531 |  51.76 KB |        1.00 |
                                           |           |           |                |             |             |            |          |           |           |       |         |         |           |             |
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **25**         | **3.740 ms** | **0.0089 ms** | **0.0070 ms** |  **1.16** |    **0.01** |  **7.8125** | **130.89 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 25         | 3.216 ms | 0.0256 ms | 0.0213 ms |  1.00 |    0.01 |  7.8125 | 130.61 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 25         | 3.094 ms | 0.0208 ms | 0.0194 ms |  0.96 |    0.01 |  7.8125 | 130.89 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 25         | 3.759 ms | 0.5521 ms | 0.0303 ms |  1.17 |    0.01 |  7.8125 | 130.75 KB |        1.00 |
                                           |           |           |                |             |             |            |          |           |           |       |         |         |           |             |
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **50**         | **7.715 ms** | **0.0899 ms** | **0.0797 ms** |  **1.12** |    **0.03** | **15.6250** | **267.43 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 50         | 6.872 ms | 0.1361 ms | 0.1457 ms |  1.00 |    0.03 | 15.6250 | 267.44 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 50         | 6.539 ms | 0.1285 ms | 0.1626 ms |  0.95 |    0.03 | 15.6250 | 267.79 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 50         | 7.870 ms | 2.1520 ms | 0.1180 ms |  1.15 |    0.03 | 15.6250 | 267.65 KB |        1.00 |
