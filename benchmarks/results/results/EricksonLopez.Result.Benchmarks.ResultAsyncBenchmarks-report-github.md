```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.32 (8.0.32, 8.0.3226.50405), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.21 (9.0.21, 9.0.2126.50404), X64 RyuJIT x86-64-v3


```
| Method                      | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------- |---------- |----------:|------:|-------:|----------:|------------:|
| Map_SyncCompleted           | .NET 10.0 | .NET 10.0 |  22.47 ns |  0.94 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 10.0 | .NET 10.0 |  22.48 ns |  0.94 | 0.0095 |     160 B |        1.00 |
| Bind_SyncCompleted          | .NET 10.0 | .NET 10.0 |  22.86 ns |  0.96 | 0.0100 |     168 B |        1.05 |
| Failure_Map_SyncCompleted   | .NET 10.0 | .NET 10.0 |  23.13 ns |  0.97 | 0.0095 |     160 B |        1.00 |
| Failure_Map_SyncCompleted   | .NET 8.0  | .NET 8.0  |  23.32 ns |  0.98 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 8.0  | .NET 8.0  |  23.59 ns |  0.99 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 8.0  | .NET 8.0  |  23.91 ns |  1.00 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 8.0  | .NET 8.0  |  24.15 ns |  1.01 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 9.0  | .NET 9.0  |  24.17 ns |  1.01 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 8.0  | .NET 8.0  |  24.18 ns |  1.01 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 10.0 | .NET 10.0 |  24.21 ns |  1.01 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 10.0 | .NET 10.0 |  24.25 ns |  1.01 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 9.0  | .NET 9.0  |  24.59 ns |  1.03 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 9.0  | .NET 9.0  |  24.63 ns |  1.03 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 9.0  | .NET 9.0  |  24.80 ns |  1.04 | 0.0095 |     160 B |        1.00 |
| Failure_Map_SyncCompleted   | .NET 9.0  | .NET 9.0  |  25.54 ns |  1.07 | 0.0095 |     160 B |        1.00 |
| Bind_SyncCompleted          | .NET 9.0  | .NET 9.0  |  26.86 ns |  1.12 | 0.0100 |     168 B |        1.05 |
| Bind_SyncCompleted          | .NET 8.0  | .NET 8.0  |  27.08 ns |  1.13 | 0.0100 |     168 B |        1.05 |
| Tap_SyncCompleted           | .NET 10.0 | .NET 10.0 |  31.13 ns |  1.30 | 0.0148 |     248 B |        1.55 |
| Tap_SyncCompleted           | .NET 8.0  | .NET 8.0  |  35.54 ns |  1.49 | 0.0148 |     248 B |        1.55 |
| Tap_SyncCompleted           | .NET 9.0  | .NET 9.0  |  36.50 ns |  1.53 | 0.0148 |     248 B |        1.55 |
| Map_AsyncCompleted          | .NET 9.0  | .NET 9.0  | 770.93 ns | 32.25 | 0.0143 |     240 B |        1.50 |
| Map_AsyncCompleted          | .NET 10.0 | .NET 10.0 | 782.90 ns | 32.75 | 0.0143 |     240 B |        1.50 |
| Failure_Map_AsyncCompleted  | .NET 10.0 | .NET 10.0 | 818.18 ns | 34.23 | 0.0143 |     240 B |        1.50 |
| Failure_Map_AsyncCompleted  | .NET 9.0  | .NET 9.0  | 828.68 ns | 34.67 | 0.0143 |     240 B |        1.50 |
| Failure_Map_AsyncCompleted  | .NET 8.0  | .NET 8.0  | 842.27 ns | 35.24 | 0.0143 |     240 B |        1.50 |
| Map_AsyncCompleted          | .NET 8.0  | .NET 8.0  | 854.64 ns | 35.75 | 0.0143 |     240 B |        1.50 |
