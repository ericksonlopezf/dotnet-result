```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.32 (8.0.32, 8.0.3226.50405), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.21 (9.0.21, 9.0.2126.50404), X64 RyuJIT x86-64-v4


```
| Method                      | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------- |---------- |----------:|------:|-------:|----------:|------------:|
| Ensure_SyncCompleted_Fails  | .NET 10.0 | .NET 10.0 |  15.76 ns |  0.96 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 10.0 | .NET 10.0 |  15.84 ns |  0.96 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 8.0  | .NET 8.0  |  16.42 ns |  1.00 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 8.0  | .NET 8.0  |  16.45 ns |  1.00 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 8.0  | .NET 8.0  |  16.53 ns |  1.00 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 8.0  | .NET 8.0  |  16.53 ns |  1.00 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 10.0 | .NET 10.0 |  16.57 ns |  1.01 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 9.0  | .NET 9.0  |  16.62 ns |  1.01 | 0.0095 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 10.0 | .NET 10.0 |  16.95 ns |  1.03 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 9.0  | .NET 9.0  |  17.07 ns |  1.04 | 0.0095 |     160 B |        1.00 |
| Failure_Map_SyncCompleted   | .NET 8.0  | .NET 8.0  |  17.14 ns |  1.04 | 0.0095 |     160 B |        1.00 |
| Bind_SyncCompleted          | .NET 10.0 | .NET 10.0 |  17.19 ns |  1.04 | 0.0100 |     168 B |        1.05 |
| Map_SyncCompleted_TState    | .NET 9.0  | .NET 9.0  |  17.72 ns |  1.08 | 0.0095 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 9.0  | .NET 9.0  |  17.74 ns |  1.08 | 0.0095 |     160 B |        1.00 |
| Failure_Map_SyncCompleted   | .NET 9.0  | .NET 9.0  |  18.22 ns |  1.11 | 0.0095 |     160 B |        1.00 |
| Failure_Map_SyncCompleted   | .NET 10.0 | .NET 10.0 |  18.55 ns |  1.13 | 0.0095 |     160 B |        1.00 |
| Bind_SyncCompleted          | .NET 8.0  | .NET 8.0  |  19.90 ns |  1.21 | 0.0100 |     168 B |        1.05 |
| Bind_SyncCompleted          | .NET 9.0  | .NET 9.0  |  20.73 ns |  1.26 | 0.0100 |     168 B |        1.05 |
| Tap_SyncCompleted           | .NET 10.0 | .NET 10.0 |  23.81 ns |  1.45 | 0.0148 |     248 B |        1.55 |
| Tap_SyncCompleted           | .NET 8.0  | .NET 8.0  |  25.06 ns |  1.52 | 0.0148 |     248 B |        1.55 |
| Tap_SyncCompleted           | .NET 9.0  | .NET 9.0  |  30.68 ns |  1.87 | 0.0148 |     248 B |        1.55 |
| Failure_Map_AsyncCompleted  | .NET 8.0  | .NET 8.0  | 767.12 ns | 46.64 | 0.0143 |     240 B |        1.50 |
| Failure_Map_AsyncCompleted  | .NET 9.0  | .NET 9.0  | 769.15 ns | 46.77 | 0.0143 |     239 B |        1.49 |
| Map_AsyncCompleted          | .NET 10.0 | .NET 10.0 | 769.32 ns | 46.78 | 0.0143 |     239 B |        1.49 |
| Map_AsyncCompleted          | .NET 8.0  | .NET 8.0  | 776.94 ns | 47.24 | 0.0143 |     240 B |        1.50 |
| Failure_Map_AsyncCompleted  | .NET 10.0 | .NET 10.0 | 778.89 ns | 47.36 | 0.0143 |     239 B |        1.49 |
| Map_AsyncCompleted          | .NET 9.0  | .NET 9.0  | 779.69 ns | 47.41 | 0.0143 |     240 B |        1.50 |
