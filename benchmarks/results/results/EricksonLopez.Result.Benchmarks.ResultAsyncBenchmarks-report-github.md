```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3


```
| Method                      | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------- |---------- |----------:|------:|-------:|----------:|------------:|
| Bind_SyncCompleted          | .NET 10.0 | .NET 10.0 |  20.87 ns |  0.88 | 0.0100 |     168 B |        1.05 |
| Map_SyncCompleted_TState    | .NET 10.0 | .NET 10.0 |  22.13 ns |  0.94 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 8.0  | .NET 8.0  |  22.79 ns |  0.96 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 10.0 | .NET 10.0 |  22.81 ns |  0.97 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 10.0 | .NET 10.0 |  22.91 ns |  0.97 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 9.0  | .NET 9.0  |  23.14 ns |  0.98 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 8.0  | .NET 8.0  |  23.23 ns |  0.98 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 9.0  | .NET 9.0  |  23.61 ns |  1.00 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 8.0  | .NET 8.0  |  23.66 ns |  1.00 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 9.0  | .NET 9.0  |  23.99 ns |  1.01 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 9.0  | .NET 9.0  |  24.13 ns |  1.02 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 10.0 | .NET 10.0 |  24.21 ns |  1.02 | 0.0095 |     160 B |        1.00 |
| Failure_Map_SyncCompleted   | .NET 10.0 | .NET 10.0 |  24.23 ns |  1.03 | 0.0095 |     160 B |        1.00 |
| Failure_Map_SyncCompleted   | .NET 8.0  | .NET 8.0  |  24.56 ns |  1.04 | 0.0095 |     160 B |        1.00 |
| Bind_SyncCompleted          | .NET 8.0  | .NET 8.0  |  24.57 ns |  1.04 | 0.0100 |     168 B |        1.05 |
| Failure_Map_SyncCompleted   | .NET 9.0  | .NET 9.0  |  24.65 ns |  1.04 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 8.0  | .NET 8.0  |  24.82 ns |  1.05 | 0.0095 |     160 B |        1.00 |
| Bind_SyncCompleted          | .NET 9.0  | .NET 9.0  |  24.97 ns |  1.06 | 0.0100 |     168 B |        1.05 |
| Tap_SyncCompleted           | .NET 10.0 | .NET 10.0 |  29.65 ns |  1.25 | 0.0148 |     248 B |        1.55 |
| Tap_SyncCompleted           | .NET 8.0  | .NET 8.0  |  33.21 ns |  1.41 | 0.0148 |     248 B |        1.55 |
| Tap_SyncCompleted           | .NET 9.0  | .NET 9.0  |  34.17 ns |  1.45 | 0.0148 |     248 B |        1.55 |
| Map_AsyncCompleted          | .NET 9.0  | .NET 9.0  | 800.83 ns | 33.88 | 0.0143 |     240 B |        1.50 |
| Map_AsyncCompleted          | .NET 10.0 | .NET 10.0 | 803.08 ns | 33.97 | 0.0143 |     240 B |        1.50 |
| Failure_Map_AsyncCompleted  | .NET 8.0  | .NET 8.0  | 807.58 ns | 34.16 | 0.0143 |     240 B |        1.50 |
| Failure_Map_AsyncCompleted  | .NET 9.0  | .NET 9.0  | 807.66 ns | 34.17 | 0.0143 |     240 B |        1.50 |
| Failure_Map_AsyncCompleted  | .NET 10.0 | .NET 10.0 | 823.42 ns | 34.83 | 0.0134 |     240 B |        1.50 |
| Map_AsyncCompleted          | .NET 8.0  | .NET 8.0  | 866.70 ns | 36.67 | 0.0143 |     240 B |        1.50 |
