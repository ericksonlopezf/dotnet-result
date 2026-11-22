
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.68GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v4


 Method                      | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
---------------------------- |---------- |---------- |----------:|------:|-------:|----------:|------------:|
 Ensure_SyncCompleted_Passes | .NET 10.0 | .NET 10.0 |  15.39 ns |  0.90 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 10.0 | .NET 10.0 |  15.56 ns |  0.91 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Fails  | .NET 10.0 | .NET 10.0 |  15.75 ns |  0.92 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 10.0 | .NET 10.0 |  16.07 ns |  0.94 | 0.0095 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 10.0 | .NET 10.0 |  16.19 ns |  0.95 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Passes | .NET 9.0  | .NET 9.0  |  16.36 ns |  0.96 | 0.0095 |     160 B |        1.00 |
 Bind_SyncCompleted          | .NET 10.0 | .NET 10.0 |  16.55 ns |  0.97 | 0.0100 |     168 B |        1.05 |
 Ensure_SyncCompleted_Fails  | .NET 8.0  | .NET 8.0  |  16.86 ns |  0.99 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Fails  | .NET 9.0  | .NET 9.0  |  16.94 ns |  0.99 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 8.0  | .NET 8.0  |  17.11 ns |  1.00 | 0.0095 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 9.0  | .NET 9.0  |  17.11 ns |  1.00 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 8.0  | .NET 8.0  |  17.17 ns |  1.00 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 9.0  | .NET 9.0  |  17.44 ns |  1.02 | 0.0095 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 8.0  | .NET 8.0  |  17.48 ns |  1.02 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Passes | .NET 8.0  | .NET 8.0  |  17.54 ns |  1.03 | 0.0095 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 9.0  | .NET 9.0  |  17.64 ns |  1.03 | 0.0095 |     160 B |        1.00 |
 Bind_SyncCompleted          | .NET 8.0  | .NET 8.0  |  20.16 ns |  1.18 | 0.0100 |     168 B |        1.05 |
 Bind_SyncCompleted          | .NET 9.0  | .NET 9.0  |  20.96 ns |  1.23 | 0.0100 |     168 B |        1.05 |
 Tap_SyncCompleted           | .NET 10.0 | .NET 10.0 |  25.44 ns |  1.49 | 0.0148 |     248 B |        1.55 |
 Tap_SyncCompleted           | .NET 8.0  | .NET 8.0  |  25.59 ns |  1.50 | 0.0148 |     248 B |        1.55 |
 Tap_SyncCompleted           | .NET 9.0  | .NET 9.0  |  27.87 ns |  1.63 | 0.0148 |     248 B |        1.55 |
 Map_AsyncCompleted          | .NET 9.0  | .NET 9.0  | 717.06 ns | 41.92 | 0.0143 |     239 B |        1.49 |
 Map_AsyncCompleted          | .NET 8.0  | .NET 8.0  | 720.88 ns | 42.15 | 0.0143 |     239 B |        1.49 |
 Failure_Map_AsyncCompleted  | .NET 9.0  | .NET 9.0  | 721.59 ns | 42.19 | 0.0143 |     239 B |        1.49 |
 Map_AsyncCompleted          | .NET 10.0 | .NET 10.0 | 733.31 ns | 42.87 | 0.0143 |     239 B |        1.49 |
 Failure_Map_AsyncCompleted  | .NET 10.0 | .NET 10.0 | 738.38 ns | 43.17 | 0.0143 |     239 B |        1.49 |
 Failure_Map_AsyncCompleted  | .NET 8.0  | .NET 8.0  | 745.58 ns | 43.59 | 0.0143 |     240 B |        1.50 |
