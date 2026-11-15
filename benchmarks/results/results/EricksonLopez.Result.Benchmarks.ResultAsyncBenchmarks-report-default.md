
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.68GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v4


 Method                      | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
---------------------------- |---------- |---------- |----------:|------:|-------:|----------:|------------:|
 Ensure_SyncCompleted_Passes | .NET 9.0  | .NET 9.0  |  15.83 ns |  0.96 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 10.0 | .NET 10.0 |  15.91 ns |  0.97 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 10.0 | .NET 10.0 |  15.91 ns |  0.97 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 9.0  | .NET 9.0  |  15.93 ns |  0.97 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Fails  | .NET 9.0  | .NET 9.0  |  16.14 ns |  0.98 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Passes | .NET 8.0  | .NET 8.0  |  16.20 ns |  0.98 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Fails  | .NET 8.0  | .NET 8.0  |  16.29 ns |  0.99 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 9.0  | .NET 9.0  |  16.31 ns |  0.99 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 8.0  | .NET 8.0  |  16.37 ns |  0.99 | 0.0095 |     160 B |        1.00 |
 Bind_SyncCompleted          | .NET 10.0 | .NET 10.0 |  16.37 ns |  0.99 | 0.0100 |     168 B |        1.05 |
 Ensure_SyncCompleted_Passes | .NET 10.0 | .NET 10.0 |  16.45 ns |  1.00 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 8.0  | .NET 8.0  |  16.47 ns |  1.00 | 0.0095 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 8.0  | .NET 8.0  |  16.84 ns |  1.02 | 0.0095 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 10.0 | .NET 10.0 |  17.14 ns |  1.04 | 0.0095 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 9.0  | .NET 9.0  |  18.98 ns |  1.15 | 0.0095 |     160 B |        1.00 |
 Bind_SyncCompleted          | .NET 8.0  | .NET 8.0  |  19.91 ns |  1.21 | 0.0100 |     168 B |        1.05 |
 Bind_SyncCompleted          | .NET 9.0  | .NET 9.0  |  20.15 ns |  1.22 | 0.0100 |     168 B |        1.05 |
 Tap_SyncCompleted           | .NET 10.0 | .NET 10.0 |  22.05 ns |  1.34 | 0.0148 |     248 B |        1.55 |
 Ensure_SyncCompleted_Fails  | .NET 10.0 | .NET 10.0 |  24.79 ns |  1.51 | 0.0095 |     160 B |        1.00 |
 Tap_SyncCompleted           | .NET 8.0  | .NET 8.0  |  24.92 ns |  1.51 | 0.0148 |     248 B |        1.55 |
 Tap_SyncCompleted           | .NET 9.0  | .NET 9.0  |  26.87 ns |  1.63 | 0.0148 |     248 B |        1.55 |
 Map_AsyncCompleted          | .NET 9.0  | .NET 9.0  | 707.29 ns | 42.94 | 0.0134 |     239 B |        1.49 |
 Failure_Map_AsyncCompleted  | .NET 9.0  | .NET 9.0  | 731.04 ns | 44.38 | 0.0143 |     239 B |        1.49 |
 Map_AsyncCompleted          | .NET 10.0 | .NET 10.0 | 734.79 ns | 44.61 | 0.0143 |     239 B |        1.49 |
 Failure_Map_AsyncCompleted  | .NET 10.0 | .NET 10.0 | 737.74 ns | 44.79 | 0.0143 |     240 B |        1.50 |
 Map_AsyncCompleted          | .NET 8.0  | .NET 8.0  | 741.07 ns | 44.99 | 0.0143 |     240 B |        1.50 |
 Failure_Map_AsyncCompleted  | .NET 8.0  | .NET 8.0  | 741.73 ns | 45.03 | 0.0143 |     240 B |        1.50 |
