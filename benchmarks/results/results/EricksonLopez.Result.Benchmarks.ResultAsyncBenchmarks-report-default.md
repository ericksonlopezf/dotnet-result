
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.63GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4


 Method                      | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
---------------------------- |---------- |---------- |----------:|------:|-------:|----------:|------------:|
 Map_SyncCompleted           | .NET 10.0 | .NET 10.0 |  15.82 ns |  0.93 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 10.0 | .NET 10.0 |  15.83 ns |  0.93 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Passes | .NET 10.0 | .NET 10.0 |  16.13 ns |  0.94 | 0.0095 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 10.0 | .NET 10.0 |  16.16 ns |  0.95 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Fails  | .NET 10.0 | .NET 10.0 |  16.18 ns |  0.95 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 9.0  | .NET 9.0  |  16.22 ns |  0.95 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 9.0  | .NET 9.0  |  16.45 ns |  0.96 | 0.0095 |     160 B |        1.00 |
 Bind_SyncCompleted          | .NET 10.0 | .NET 10.0 |  16.47 ns |  0.96 | 0.0100 |     168 B |        1.05 |
 Ensure_SyncCompleted_Passes | .NET 8.0  | .NET 8.0  |  16.48 ns |  0.96 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Passes | .NET 9.0  | .NET 9.0  |  16.62 ns |  0.97 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 8.0  | .NET 8.0  |  16.78 ns |  0.98 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Fails  | .NET 9.0  | .NET 9.0  |  16.93 ns |  0.99 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 8.0  | .NET 8.0  |  17.09 ns |  1.00 | 0.0095 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 8.0  | .NET 8.0  |  17.13 ns |  1.00 | 0.0095 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 9.0  | .NET 9.0  |  17.26 ns |  1.01 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Fails  | .NET 8.0  | .NET 8.0  |  17.39 ns |  1.02 | 0.0095 |     160 B |        1.00 |
 Bind_SyncCompleted          | .NET 8.0  | .NET 8.0  |  20.56 ns |  1.20 | 0.0100 |     168 B |        1.05 |
 Bind_SyncCompleted          | .NET 9.0  | .NET 9.0  |  20.77 ns |  1.22 | 0.0100 |     168 B |        1.05 |
 Tap_SyncCompleted           | .NET 10.0 | .NET 10.0 |  22.59 ns |  1.32 | 0.0148 |     248 B |        1.55 |
 Tap_SyncCompleted           | .NET 8.0  | .NET 8.0  |  25.59 ns |  1.50 | 0.0148 |     248 B |        1.55 |
 Tap_SyncCompleted           | .NET 9.0  | .NET 9.0  |  26.89 ns |  1.57 | 0.0148 |     248 B |        1.55 |
 Map_AsyncCompleted          | .NET 10.0 | .NET 10.0 | 717.56 ns | 41.99 | 0.0143 |     239 B |        1.49 |
 Map_AsyncCompleted          | .NET 8.0  | .NET 8.0  | 722.09 ns | 42.26 | 0.0143 |     240 B |        1.50 |
 Failure_Map_AsyncCompleted  | .NET 9.0  | .NET 9.0  | 725.72 ns | 42.47 | 0.0143 |     240 B |        1.50 |
 Failure_Map_AsyncCompleted  | .NET 10.0 | .NET 10.0 | 726.96 ns | 42.54 | 0.0143 |     239 B |        1.49 |
 Map_AsyncCompleted          | .NET 9.0  | .NET 9.0  | 728.07 ns | 42.61 | 0.0143 |     239 B |        1.49 |
 Failure_Map_AsyncCompleted  | .NET 8.0  | .NET 8.0  | 740.08 ns | 43.31 | 0.0143 |     240 B |        1.50 |
