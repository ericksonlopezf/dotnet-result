
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.4 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.400
  [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.19 (9.0.19, 9.0.1926.36724), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3


 Method                                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | TypesCount | Mean     | Error     | StdDev    | Ratio | Gen0    | Allocated | Alloc Ratio |
------------------------------------------ |---------- |---------- |--------------- |------------ |------------ |----------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **10**         | **1.778 ms** | **0.0084 ms** | **0.0079 ms** |  **1.10** |       **-** |  **51.75 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 10         | 1.620 ms | 0.0071 ms | 0.0066 ms |  1.00 |       - |  51.61 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 10         | 1.602 ms | 0.0050 ms | 0.0046 ms |  0.99 |  1.9531 |  51.83 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 10         | 1.796 ms | 0.1784 ms | 0.0098 ms |  1.11 |       - |  51.89 KB |        1.01 |
                                           |           |           |                |             |             |            |          |           |           |       |         |           |             |
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **25**         | **4.610 ms** | **0.0156 ms** | **0.0146 ms** |  **1.12** |  **7.8125** | **130.96 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 25         | 4.115 ms | 0.0107 ms | 0.0084 ms |  1.00 |  7.8125 | 130.89 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 25         | 4.061 ms | 0.0159 ms | 0.0133 ms |  0.99 |  7.8125 | 130.61 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 25         | 4.596 ms | 0.1783 ms | 0.0098 ms |  1.12 |  7.8125 | 130.89 KB |        1.00 |
                                           |           |           |                |             |             |            |          |           |           |       |         |           |             |
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **50**         | **9.509 ms** | **0.0359 ms** | **0.0336 ms** |  **1.10** | **15.6250** | **267.29 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 50         | 8.626 ms | 0.0391 ms | 0.0366 ms |  1.00 | 15.6250 | 267.33 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 50         | 8.405 ms | 0.0447 ms | 0.0419 ms |  0.97 | 15.6250 | 266.91 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 50         | 9.574 ms | 0.3396 ms | 0.0186 ms |  1.11 | 15.6250 | 267.58 KB |        1.00 |
