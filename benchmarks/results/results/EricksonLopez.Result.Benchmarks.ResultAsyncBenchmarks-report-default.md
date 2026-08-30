
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.4 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.400
  [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.19 (9.0.19, 9.0.1926.36724), X64 RyuJIT x86-64-v3


 Method                      | Job       | Runtime   | Mean      | Median    | Ratio | Gen0   | Allocated | Alloc Ratio |
---------------------------- |---------- |---------- |----------:|----------:|------:|-------:|----------:|------------:|
 Bind_SyncCompleted          | .NET 10.0 | .NET 10.0 |  22.45 ns |  22.51 ns |  0.84 | 0.0100 |     168 B |        1.05 |
 Map_SyncCompleted_TState    | .NET 10.0 | .NET 10.0 |  23.58 ns |  23.62 ns |  0.88 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 10.0 | .NET 10.0 |  23.92 ns |  23.82 ns |  0.89 | 0.0095 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 10.0 | .NET 10.0 |  24.01 ns |  24.09 ns |  0.90 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Fails  | .NET 10.0 | .NET 10.0 |  24.29 ns |  24.30 ns |  0.91 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Passes | .NET 10.0 | .NET 10.0 |  24.70 ns |  24.54 ns |  0.92 | 0.0095 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 8.0  | .NET 8.0  |  25.72 ns |  25.74 ns |  0.96 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 9.0  | .NET 9.0  |  25.79 ns |  25.66 ns |  0.96 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 9.0  | .NET 9.0  |  25.85 ns |  25.81 ns |  0.96 | 0.0095 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 9.0  | .NET 9.0  |  26.16 ns |  25.59 ns |  0.98 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 8.0  | .NET 8.0  |  26.78 ns |  27.06 ns |  1.00 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 8.0  | .NET 8.0  |  26.87 ns |  27.37 ns |  1.00 | 0.0095 |     160 B |        1.00 |
 Bind_SyncCompleted          | .NET 9.0  | .NET 9.0  |  27.67 ns |  27.74 ns |  1.03 | 0.0100 |     168 B |        1.05 |
 Ensure_SyncCompleted_Fails  | .NET 8.0  | .NET 8.0  |  27.77 ns |  27.30 ns |  1.04 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Passes | .NET 9.0  | .NET 9.0  |  27.83 ns |  28.07 ns |  1.04 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Fails  | .NET 9.0  | .NET 9.0  |  27.87 ns |  28.01 ns |  1.04 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Passes | .NET 8.0  | .NET 8.0  |  28.19 ns |  28.32 ns |  1.05 | 0.0095 |     160 B |        1.00 |
 Bind_SyncCompleted          | .NET 8.0  | .NET 8.0  |  29.85 ns |  30.20 ns |  1.11 | 0.0100 |     168 B |        1.05 |
 Tap_SyncCompleted           | .NET 10.0 | .NET 10.0 |  30.85 ns |  30.90 ns |  1.15 | 0.0148 |     248 B |        1.55 |
 Tap_SyncCompleted           | .NET 8.0  | .NET 8.0  |  34.31 ns |  33.95 ns |  1.28 | 0.0148 |     248 B |        1.55 |
 Tap_SyncCompleted           | .NET 9.0  | .NET 9.0  |  37.38 ns |  37.38 ns |  1.40 | 0.0148 |     248 B |        1.55 |
 Failure_Map_AsyncCompleted  | .NET 10.0 | .NET 10.0 | 803.41 ns | 807.15 ns | 29.99 | 0.0143 |     240 B |        1.50 |
 Map_AsyncCompleted          | .NET 9.0  | .NET 9.0  | 810.59 ns | 809.50 ns | 30.25 | 0.0143 |     240 B |        1.50 |
 Map_AsyncCompleted          | .NET 10.0 | .NET 10.0 | 828.56 ns | 828.34 ns | 30.92 | 0.0143 |     240 B |        1.50 |
 Failure_Map_AsyncCompleted  | .NET 9.0  | .NET 9.0  | 837.18 ns | 835.09 ns | 31.25 | 0.0143 |     240 B |        1.50 |
 Failure_Map_AsyncCompleted  | .NET 8.0  | .NET 8.0  | 853.13 ns | 852.87 ns | 31.84 | 0.0143 |     240 B |        1.50 |
 Map_AsyncCompleted          | .NET 8.0  | .NET 8.0  | 878.07 ns | 879.22 ns | 32.77 | 0.0134 |     240 B |        1.50 |
