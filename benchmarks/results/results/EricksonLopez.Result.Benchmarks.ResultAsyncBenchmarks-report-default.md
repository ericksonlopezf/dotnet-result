
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
INTEL XEON PLATINUM 8573C 2.30GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4


 Method                      | Job       | Runtime   | Mean        | Ratio | Gen0   | Allocated | Alloc Ratio |
---------------------------- |---------- |---------- |------------:|------:|-------:|----------:|------------:|
 Ensure_SyncCompleted_Fails  | .NET 10.0 | .NET 10.0 |    16.94 ns |  0.95 | 0.0019 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 10.0 | .NET 10.0 |    16.95 ns |  0.95 | 0.0019 |     160 B |        1.00 |
 Ensure_SyncCompleted_Passes | .NET 10.0 | .NET 10.0 |    16.99 ns |  0.95 | 0.0019 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 10.0 | .NET 10.0 |    17.16 ns |  0.96 | 0.0019 |     160 B |        1.00 |
 Ensure_SyncCompleted_Passes | .NET 8.0  | .NET 8.0  |    17.18 ns |  0.96 | 0.0019 |     160 B |        1.00 |
 Ensure_SyncCompleted_Fails  | .NET 8.0  | .NET 8.0  |    17.40 ns |  0.97 | 0.0019 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 9.0  | .NET 9.0  |    17.52 ns |  0.98 | 0.0019 |     160 B |        1.00 |
 Ensure_SyncCompleted_Fails  | .NET 9.0  | .NET 9.0  |    17.62 ns |  0.99 | 0.0019 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 10.0 | .NET 10.0 |    17.67 ns |  0.99 | 0.0019 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 9.0  | .NET 9.0  |    17.69 ns |  0.99 | 0.0019 |     160 B |        1.00 |
 Map_SyncCompleted           | .NET 8.0  | .NET 8.0  |    17.86 ns |  1.00 | 0.0019 |     160 B |        1.00 |
 Map_SyncCompleted_TState    | .NET 8.0  | .NET 8.0  |    18.07 ns |  1.01 | 0.0019 |     160 B |        1.00 |
 Ensure_SyncCompleted_Passes | .NET 9.0  | .NET 9.0  |    18.23 ns |  1.02 | 0.0019 |     160 B |        1.00 |
 Failure_Map_SyncCompleted   | .NET 9.0  | .NET 9.0  |    18.52 ns |  1.04 | 0.0019 |     160 B |        1.00 |
 Bind_SyncCompleted          | .NET 10.0 | .NET 10.0 |    18.54 ns |  1.04 | 0.0020 |     168 B |        1.05 |
 Failure_Map_SyncCompleted   | .NET 8.0  | .NET 8.0  |    18.59 ns |  1.04 | 0.0019 |     160 B |        1.00 |
 Bind_SyncCompleted          | .NET 8.0  | .NET 8.0  |    20.49 ns |  1.15 | 0.0020 |     168 B |        1.05 |
 Tap_SyncCompleted           | .NET 10.0 | .NET 10.0 |    24.10 ns |  1.35 | 0.0030 |     248 B |        1.55 |
 Bind_SyncCompleted          | .NET 9.0  | .NET 9.0  |    25.39 ns |  1.42 | 0.0020 |     168 B |        1.05 |
 Tap_SyncCompleted           | .NET 8.0  | .NET 8.0  |    27.96 ns |  1.57 | 0.0029 |     248 B |        1.55 |
 Tap_SyncCompleted           | .NET 9.0  | .NET 9.0  |    30.17 ns |  1.69 | 0.0029 |     248 B |        1.55 |
 Failure_Map_AsyncCompleted  | .NET 10.0 | .NET 10.0 | 1,069.42 ns | 59.88 | 0.0019 |     235 B |        1.47 |
 Failure_Map_AsyncCompleted  | .NET 9.0  | .NET 9.0  | 1,078.79 ns | 60.40 | 0.0019 |     235 B |        1.47 |
 Map_AsyncCompleted          | .NET 10.0 | .NET 10.0 | 1,081.28 ns | 60.54 | 0.0019 |     235 B |        1.47 |
 Map_AsyncCompleted          | .NET 9.0  | .NET 9.0  | 1,082.94 ns | 60.63 | 0.0019 |     234 B |        1.46 |
 Failure_Map_AsyncCompleted  | .NET 8.0  | .NET 8.0  | 1,091.72 ns | 61.12 | 0.0019 |     235 B |        1.47 |
 Map_AsyncCompleted          | .NET 8.0  | .NET 8.0  | 1,115.36 ns | 62.45 | 0.0019 |     233 B |        1.46 |
