```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3


```
| Method                       | Job       | Runtime   | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------- |---------- |-----------:|------:|-------:|----------:|------------:|
| Map_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.5443 ns |  0.04 |      - |         - |        0.00 |
| Tap_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.8428 ns |  0.07 |      - |         - |        0.00 |
| Map_Failure_Lambda           | .NET 10.0 | .NET 10.0 |  0.8556 ns |  0.07 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 10.0 | .NET 10.0 |  1.0299 ns |  0.08 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 10.0 | .NET 10.0 |  1.1874 ns |  0.09 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 10.0 | .NET 10.0 |  1.2053 ns |  0.09 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 10.0 | .NET 10.0 |  1.2317 ns |  0.10 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 10.0 | .NET 10.0 |  1.2353 ns |  0.10 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 10.0 | .NET 10.0 |  1.8646 ns |  0.14 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 9.0  | .NET 9.0  |  1.8703 ns |  0.14 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 8.0  | .NET 8.0  |  1.8910 ns |  0.15 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 10.0 | .NET 10.0 |  3.7344 ns |  0.29 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 9.0  | .NET 9.0  |  3.7422 ns |  0.29 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 8.0  | .NET 8.0  |  3.7609 ns |  0.29 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 9.0  | .NET 9.0  |  4.0553 ns |  0.31 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 9.0  | .NET 9.0  |  4.0756 ns |  0.32 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 8.0  | .NET 8.0  |  4.3848 ns |  0.34 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 10.0 | .NET 10.0 |  4.6837 ns |  0.36 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 8.0  | .NET 8.0  |  4.6989 ns |  0.36 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 10.0 | .NET 10.0 |  4.7037 ns |  0.36 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 9.0  | .NET 9.0  |  5.8368 ns |  0.45 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 9.0  | .NET 9.0  |  6.1435 ns |  0.48 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 8.0  | .NET 8.0  |  7.7689 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 8.0  | .NET 8.0  |  8.1755 ns |  0.63 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 8.0  | .NET 8.0  |  8.1832 ns |  0.63 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 9.0  | .NET 9.0  |  8.3161 ns |  0.64 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 9.0  | .NET 9.0  |  8.3261 ns |  0.64 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 9.0  | .NET 9.0  |  8.3358 ns |  0.64 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 8.0  | .NET 8.0  |  8.3393 ns |  0.65 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 8.0  | .NET 8.0  |  8.3450 ns |  0.65 |      - |         - |        0.00 |
| Tap_Success_Lambda           | .NET 9.0  | .NET 9.0  |  9.1300 ns |  0.71 | 0.0038 |      64 B |        1.00 |
| Tap_Success_Lambda           | .NET 8.0  | .NET 8.0  |  9.3366 ns |  0.72 | 0.0038 |      64 B |        1.00 |
| Map_Success_Lambda           | .NET 9.0  | .NET 9.0  | 11.0414 ns |  0.85 | 0.0038 |      64 B |        1.00 |
| Map_Failure_Lambda           | .NET 8.0  | .NET 8.0  | 11.9093 ns |  0.92 | 0.0038 |      64 B |        1.00 |
| Map_Failure_Lambda           | .NET 9.0  | .NET 9.0  | 12.3002 ns |  0.95 | 0.0038 |      64 B |        1.00 |
| Map_Success_Lambda           | .NET 8.0  | .NET 8.0  | 12.9280 ns |  1.00 | 0.0038 |      64 B |        1.00 |
| FullPipeline_Lambda          | .NET 10.0 | .NET 10.0 | 14.8901 ns |  1.15 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 9.0  | .NET 9.0  | 16.4703 ns |  1.27 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 8.0  | .NET 8.0  | 16.9526 ns |  1.31 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 10.0 | .NET 10.0 | 17.3640 ns |  1.34 | 0.0019 |      32 B |        0.50 |
| Match_Success_Lambda         | .NET 10.0 | .NET 10.0 | 30.6002 ns |  2.37 | 0.0019 |      32 B |        0.50 |
| FullPipeline_Lambda          | .NET 8.0  | .NET 8.0  | 31.0758 ns |  2.40 | 0.0095 |     160 B |        2.50 |
| Match_Success_Lambda         | .NET 9.0  | .NET 9.0  | 34.0419 ns |  2.63 | 0.0019 |      32 B |        0.50 |
| Match_Success_TState         | .NET 10.0 | .NET 10.0 | 34.2845 ns |  2.65 | 0.0024 |      40 B |        0.62 |
| FullPipeline_Lambda          | .NET 9.0  | .NET 9.0  | 34.5995 ns |  2.68 | 0.0095 |     160 B |        2.50 |
| Match_Success_TState         | .NET 9.0  | .NET 9.0  | 37.9845 ns |  2.94 | 0.0024 |      40 B |        0.62 |
| Match_Success_Lambda         | .NET 8.0  | .NET 8.0  | 54.7638 ns |  4.24 | 0.0019 |      32 B |        0.50 |
| Match_Success_TState         | .NET 8.0  | .NET 8.0  | 61.8042 ns |  4.78 | 0.0024 |      40 B |        0.62 |
