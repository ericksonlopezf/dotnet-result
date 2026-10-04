
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


 Method                                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | TypesCount | Mean     | Error     | StdDev    | Ratio | Gen0    | Allocated | Alloc Ratio |
------------------------------------------ |---------- |---------- |--------------- |------------ |------------ |----------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **10**         | **1.836 ms** | **0.0047 ms** | **0.0042 ms** |  **1.14** |       **-** |  **51.82 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 10         | 1.616 ms | 0.0092 ms | 0.0082 ms |  1.00 |       - |  51.75 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 10         | 1.553 ms | 0.0057 ms | 0.0050 ms |  0.96 |       - |  51.61 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 10         | 1.843 ms | 0.1150 ms | 0.0063 ms |  1.14 |       - |  51.61 KB |        1.00 |
                                           |           |           |                |             |             |            |          |           |           |       |         |           |             |
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **25**         | **4.781 ms** | **0.0166 ms** | **0.0139 ms** |  **1.16** |  **7.8125** | **130.75 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 25         | 4.137 ms | 0.0133 ms | 0.0117 ms |  1.00 |  7.8125 | 130.75 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 25         | 4.001 ms | 0.0081 ms | 0.0068 ms |  0.97 |  7.8125 | 130.61 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 25         | 4.774 ms | 0.3261 ms | 0.0179 ms |  1.15 |  7.8125 | 130.96 KB |        1.00 |
                                           |           |           |                |             |             |            |          |           |           |       |         |           |             |
 **'Expression.Compile Cold Start (N types)'** | **.NET 10.0** | **.NET 10.0** | **Default**        | **Default**     | **Default**     | **50**         | **9.787 ms** | **0.0187 ms** | **0.0156 ms** |  **1.14** | **15.6250** | **267.43 KB** |        **1.00** |
 'Expression.Compile Cold Start (N types)' | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 50         | 8.616 ms | 0.0289 ms | 0.0257 ms |  1.00 | 15.6250 | 267.35 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     | 50         | 8.376 ms | 0.0233 ms | 0.0207 ms |  0.97 | 15.6250 | 267.19 KB |        1.00 |
 'Expression.Compile Cold Start (N types)' | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 50         | 9.798 ms | 0.6150 ms | 0.0337 ms |  1.14 | 15.6250 | 267.16 KB |        1.00 |
