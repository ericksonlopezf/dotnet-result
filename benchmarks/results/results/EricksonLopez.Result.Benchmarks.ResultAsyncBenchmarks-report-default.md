
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.21GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4


 Method                      | Job       | Runtime   | Mean      | Median    | Ratio | Gen0   | Allocated | Alloc Ratio |
---------------------------- |---------- |---------- |----------:|----------:|------:|-------:|----------:|------------:|
 Failure_Map_SyncCompleted   | .NET 10.0 | .NET 10.0 |  12.19 ns |  12.12 ns |  0.93 | 0.0096 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 9.0  | .NET 9.0  |  12.33 ns |  12.20 ns |  0.94 | 0.0096 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 10.0 | .NET 10.0 |  12.49 ns |  12.59 ns |  0.96 | 0.0096 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 10.0 | .NET 10.0 |  12.60 ns |  12.53 ns |  0.97 | 0.0096 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 9.0  | .NET 9.0  |  12.74 ns |  12.53 ns |  0.98 | 0.0096 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 8.0  | .NET 8.0  |  12.89 ns |  12.91 ns |  0.99 | 0.0096 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 8.0  | .NET 8.0  |  12.94 ns |  12.87 ns |  0.99 | 0.0096 |     160 B |        1.00 |
 Ensure_SyncCompleted_Fails  | .NET 9.0  | .NET 9.0  |  13.05 ns |  13.01 ns |  1.00 | 0.0096 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 8.0  | .NET 8.0  |  13.06 ns |  12.96 ns |  1.00 | 0.0096 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 9.0  | .NET 9.0  |  13.09 ns |  13.08 ns |  1.00 | 0.0096 |     160 B |        1.00 |
 Ensure_SyncCompleted_Passes | .NET 8.0  | .NET 8.0  |  13.16 ns |  13.13 ns |  1.01 | 0.0096 |     160 B |        1.00 |
 Ensure_SyncCompleted_Passes | .NET 9.0  | .NET 9.0  |  13.28 ns |  13.29 ns |  1.02 | 0.0095 |     160 B |        1.00 |
 Ensure_SyncCompleted_Fails  | .NET 10.0 | .NET 10.0 |  13.32 ns |  13.40 ns |  1.02 | 0.0096 |     160 B |        1.00 |
 Bind_SyncCompleted          | .NET 8.0  | .NET 8.0  |  13.61 ns |  13.60 ns |  1.04 | 0.0100 |     168 B |        1.05 |
 Ensure_SyncCompleted_Fails  | .NET 8.0  | .NET 8.0  |  13.66 ns |  13.67 ns |  1.05 | 0.0096 |     160 B |        1.00 |
 Bind_SyncCompleted          | .NET 10.0 | .NET 10.0 |  13.77 ns |  13.87 ns |  1.05 | 0.0100 |     168 B |        1.05 |
 Ensure_SyncCompleted_Passes | .NET 10.0 | .NET 10.0 |  14.57 ns |  14.57 ns |  1.12 | 0.0095 |     160 B |        1.00 |
 Bind_SyncCompleted          | .NET 9.0  | .NET 9.0  |  14.68 ns |  14.68 ns |  1.12 | 0.0100 |     168 B |        1.05 |
 Tap_SyncCompleted           | .NET 10.0 | .NET 10.0 |  16.74 ns |  16.37 ns |  1.28 | 0.0148 |     248 B |        1.55 |
 Tap_SyncCompleted           | .NET 8.0  | .NET 8.0  |  17.90 ns |  17.86 ns |  1.37 | 0.0148 |     248 B |        1.55 |
 Tap_SyncCompleted           | .NET 9.0  | .NET 9.0  |  17.93 ns |  17.69 ns |  1.37 | 0.0148 |     248 B |        1.55 |
 Failure_Map_AsyncCompleted  | .NET 10.0 | .NET 10.0 | 571.87 ns | 569.04 ns | 43.82 | 0.0134 |     239 B |        1.49 |
 Map_AsyncCompleted          | .NET 9.0  | .NET 9.0  | 587.12 ns | 586.33 ns | 44.98 | 0.0134 |     239 B |        1.49 |
 Failure_Map_AsyncCompleted  | .NET 9.0  | .NET 9.0  | 592.82 ns | 594.34 ns | 45.42 | 0.0134 |     239 B |        1.49 |
 Map_AsyncCompleted          | .NET 8.0  | .NET 8.0  | 605.48 ns | 602.73 ns | 46.39 | 0.0134 |     239 B |        1.49 |
 Map_AsyncCompleted          | .NET 10.0 | .NET 10.0 | 613.46 ns | 609.95 ns | 47.00 | 0.0134 |     239 B |        1.49 |
 Failure_Map_AsyncCompleted  | .NET 8.0  | .NET 8.0  | 615.57 ns | 615.63 ns | 47.16 | 0.0134 |     239 B |        1.49 |
