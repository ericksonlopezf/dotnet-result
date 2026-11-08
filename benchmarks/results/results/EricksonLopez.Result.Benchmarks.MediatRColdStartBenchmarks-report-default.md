
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.63GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4


 Method                                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | TypesCount | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0    | Allocated | Alloc Ratio |
------------------------------------------ |---------- |---------- |--------------- |------------ |------------ |----------- |---------:|----------:|----------:|------:|--------:|--------:|----------:|------------:|
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **10**         | **1.478 ms** | **0.0054 ms** | **0.0042 ms** |  **1.14** |    **0.01** |  **1.9531** |  **51.83 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 10         | 1.298 ms | 0.0066 ms | 0.0059 ms |  1.00 |    0.01 |  1.9531 |  51.83 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 10         | 1.227 ms | 0.0048 ms | 0.0040 ms |  0.95 |    0.01 |  1.9531 |  51.83 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 10         | 1.469 ms | 0.0447 ms | 0.0024 ms |  1.13 |    0.01 |  1.9531 |  51.76 KB |        1.00 |
                                           |           |           |                |             |             |            |          |           |           |       |         |         |           |             |
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **25**         | **3.755 ms** | **0.0185 ms** | **0.0173 ms** |  **1.16** |    **0.01** |  **7.8125** | **130.75 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 25         | 3.225 ms | 0.0168 ms | 0.0149 ms |  1.00 |    0.01 |  7.8125 | 130.89 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 25         | 3.125 ms | 0.0170 ms | 0.0150 ms |  0.97 |    0.01 |  7.8125 | 130.75 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 25         | 3.772 ms | 0.5603 ms | 0.0307 ms |  1.17 |    0.01 |  7.8125 | 130.89 KB |        1.00 |
                                           |           |           |                |             |             |            |          |           |           |       |         |         |           |             |
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **50**         | **7.799 ms** | **0.0310 ms** | **0.0290 ms** |  **1.15** |    **0.01** | **15.6250** | **267.36 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 50         | 6.766 ms | 0.0415 ms | 0.0368 ms |  1.00 |    0.01 | 15.6250 |  267.4 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 50         | 6.490 ms | 0.0286 ms | 0.0239 ms |  0.96 |    0.01 | 15.6250 | 267.12 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 50         | 7.771 ms | 2.1572 ms | 0.1182 ms |  1.15 |    0.02 | 15.6250 | 267.23 KB |        1.00 |
