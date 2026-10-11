```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.37GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4


```
| Method                      | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------- |---------- |----------:|------:|-------:|----------:|------------:|
| Bind_SyncCompleted          | .NET 10.0 | .NET 10.0 |  10.94 ns |  0.86 | 0.0100 |     168 B |        1.05 |
| Ensure_SyncCompleted_Fails  | .NET 10.0 | .NET 10.0 |  11.69 ns |  0.92 | 0.0096 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 10.0 | .NET 10.0 |  11.73 ns |  0.92 | 0.0096 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 10.0 | .NET 10.0 |  11.92 ns |  0.94 | 0.0096 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 9.0  | .NET 9.0  |  11.97 ns |  0.94 | 0.0096 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 10.0 | .NET 10.0 |  12.04 ns |  0.95 | 0.0096 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 9.0  | .NET 9.0  |  12.20 ns |  0.96 | 0.0096 |     160 B |        1.00 |
| Failure_Map_SyncCompleted   | .NET 10.0 | .NET 10.0 |  12.28 ns |  0.97 | 0.0096 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 8.0  | .NET 8.0  |  12.42 ns |  0.98 | 0.0096 |     160 B |        1.00 |
| Failure_Map_SyncCompleted   | .NET 9.0  | .NET 9.0  |  12.57 ns |  0.99 | 0.0096 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 8.0  | .NET 8.0  |  12.69 ns |  1.00 | 0.0096 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 8.0  | .NET 8.0  |  12.73 ns |  1.00 | 0.0096 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 9.0  | .NET 9.0  |  12.85 ns |  1.01 | 0.0096 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 8.0  | .NET 8.0  |  12.88 ns |  1.01 | 0.0096 |     160 B |        1.00 |
| Failure_Map_SyncCompleted   | .NET 8.0  | .NET 8.0  |  13.01 ns |  1.02 | 0.0096 |     160 B |        1.00 |
| Bind_SyncCompleted          | .NET 8.0  | .NET 8.0  |  13.34 ns |  1.05 | 0.0100 |     168 B |        1.05 |
| Map_SyncCompleted           | .NET 9.0  | .NET 9.0  |  13.35 ns |  1.05 | 0.0096 |     160 B |        1.00 |
| Bind_SyncCompleted          | .NET 9.0  | .NET 9.0  |  14.26 ns |  1.12 | 0.0100 |     168 B |        1.05 |
| Tap_SyncCompleted           | .NET 10.0 | .NET 10.0 |  15.66 ns |  1.23 | 0.0148 |     248 B |        1.55 |
| Tap_SyncCompleted           | .NET 8.0  | .NET 8.0  |  17.06 ns |  1.34 | 0.0148 |     248 B |        1.55 |
| Tap_SyncCompleted           | .NET 9.0  | .NET 9.0  |  18.36 ns |  1.44 | 0.0148 |     248 B |        1.55 |
| Failure_Map_AsyncCompleted  | .NET 8.0  | .NET 8.0  | 554.67 ns | 43.60 | 0.0134 |     238 B |        1.49 |
| Failure_Map_AsyncCompleted  | .NET 10.0 | .NET 10.0 | 577.53 ns | 45.40 | 0.0134 |     239 B |        1.49 |
| Map_AsyncCompleted          | .NET 10.0 | .NET 10.0 | 598.20 ns | 47.02 | 0.0134 |     239 B |        1.49 |
| Failure_Map_AsyncCompleted  | .NET 9.0  | .NET 9.0  | 610.62 ns | 48.00 | 0.0134 |     239 B |        1.49 |
| Map_AsyncCompleted          | .NET 9.0  | .NET 9.0  | 613.78 ns | 48.25 | 0.0134 |     239 B |        1.49 |
| Map_AsyncCompleted          | .NET 8.0  | .NET 8.0  | 630.22 ns | 49.54 | 0.0134 |     239 B |        1.49 |
