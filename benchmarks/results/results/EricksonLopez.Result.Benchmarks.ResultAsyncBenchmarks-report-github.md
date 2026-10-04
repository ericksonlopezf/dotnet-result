```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3


```
| Method                      | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------- |---------- |----------:|------:|-------:|----------:|------------:|
| Failure_Map_SyncCompleted   | .NET 10.0 | .NET 10.0 |  20.37 ns |  0.96 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 10.0 | .NET 10.0 |  20.44 ns |  0.96 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 10.0 | .NET 10.0 |  20.48 ns |  0.96 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 10.0 | .NET 10.0 |  20.74 ns |  0.97 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 9.0  | .NET 9.0  |  20.76 ns |  0.97 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 9.0  | .NET 9.0  |  20.85 ns |  0.98 | 0.0095 |     160 B |        1.00 |
| Bind_SyncCompleted          | .NET 10.0 | .NET 10.0 |  20.97 ns |  0.98 | 0.0100 |     168 B |        1.05 |
| Map_SyncCompleted_TState    | .NET 9.0  | .NET 9.0  |  21.04 ns |  0.99 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 8.0  | .NET 8.0  |  21.30 ns |  1.00 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 8.0  | .NET 8.0  |  21.35 ns |  1.00 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 8.0  | .NET 8.0  |  21.39 ns |  1.00 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 8.0  | .NET 8.0  |  21.78 ns |  1.02 | 0.0095 |     160 B |        1.00 |
| Failure_Map_SyncCompleted   | .NET 8.0  | .NET 8.0  |  21.93 ns |  1.03 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 10.0 | .NET 10.0 |  22.08 ns |  1.04 | 0.0095 |     160 B |        1.00 |
| Failure_Map_SyncCompleted   | .NET 9.0  | .NET 9.0  |  22.59 ns |  1.06 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 9.0  | .NET 9.0  |  23.15 ns |  1.09 | 0.0095 |     160 B |        1.00 |
| Bind_SyncCompleted          | .NET 8.0  | .NET 8.0  |  24.47 ns |  1.15 | 0.0100 |     168 B |        1.05 |
| Bind_SyncCompleted          | .NET 9.0  | .NET 9.0  |  26.09 ns |  1.23 | 0.0100 |     168 B |        1.05 |
| Tap_SyncCompleted           | .NET 10.0 | .NET 10.0 |  28.42 ns |  1.33 | 0.0148 |     248 B |        1.55 |
| Tap_SyncCompleted           | .NET 8.0  | .NET 8.0  |  30.13 ns |  1.41 | 0.0148 |     248 B |        1.55 |
| Tap_SyncCompleted           | .NET 9.0  | .NET 9.0  |  36.03 ns |  1.69 | 0.0148 |     248 B |        1.55 |
| Failure_Map_AsyncCompleted  | .NET 9.0  | .NET 9.0  | 976.72 ns | 45.85 | 0.0134 |     240 B |        1.50 |
| Map_AsyncCompleted          | .NET 10.0 | .NET 10.0 | 985.43 ns | 46.26 | 0.0134 |     239 B |        1.49 |
| Map_AsyncCompleted          | .NET 8.0  | .NET 8.0  | 985.60 ns | 46.27 | 0.0143 |     240 B |        1.50 |
| Map_AsyncCompleted          | .NET 9.0  | .NET 9.0  | 991.18 ns | 46.53 | 0.0134 |     239 B |        1.49 |
| Failure_Map_AsyncCompleted  | .NET 8.0  | .NET 8.0  | 993.97 ns | 46.66 | 0.0143 |     240 B |        1.50 |
| Failure_Map_AsyncCompleted  | .NET 10.0 | .NET 10.0 | 998.01 ns | 46.85 | 0.0143 |     240 B |        1.50 |
