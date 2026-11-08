```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.63GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4


```
| Method                       | Job       | Runtime   | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------- |---------- |-----------:|------:|-------:|----------:|------------:|
| Map_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.2431 ns |  0.03 |      - |         - |        0.00 |
| Map_Failure_Lambda           | .NET 10.0 | .NET 10.0 |  0.4965 ns |  0.06 |      - |         - |        0.00 |
| Tap_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.6042 ns |  0.07 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 10.0 | .NET 10.0 |  0.7225 ns |  0.08 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 10.0 | .NET 10.0 |  0.7531 ns |  0.09 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 10.0 | .NET 10.0 |  0.7564 ns |  0.09 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 10.0 | .NET 10.0 |  0.7610 ns |  0.09 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 10.0 | .NET 10.0 |  0.7676 ns |  0.09 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 8.0  | .NET 8.0  |  1.4355 ns |  0.17 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 9.0  | .NET 9.0  |  1.4427 ns |  0.17 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 10.0 | .NET 10.0 |  2.3238 ns |  0.27 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 10.0 | .NET 10.0 |  2.7332 ns |  0.32 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 9.0  | .NET 9.0  |  2.7576 ns |  0.32 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 9.0  | .NET 9.0  |  3.1965 ns |  0.37 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 9.0  | .NET 9.0  |  3.2053 ns |  0.37 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 8.0  | .NET 8.0  |  3.7498 ns |  0.44 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 8.0  | .NET 8.0  |  3.8179 ns |  0.45 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 10.0 | .NET 10.0 |  4.1708 ns |  0.49 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 10.0 | .NET 10.0 |  4.3143 ns |  0.50 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 8.0  | .NET 8.0  |  4.5282 ns |  0.53 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 8.0  | .NET 8.0  |  5.1521 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 8.0  | .NET 8.0  |  5.1615 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 9.0  | .NET 9.0  |  5.1745 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 8.0  | .NET 8.0  |  5.1801 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 9.0  | .NET 9.0  |  5.1804 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 9.0  | .NET 9.0  |  5.1913 ns |  0.61 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 9.0  | .NET 9.0  |  5.4138 ns |  0.63 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 9.0  | .NET 9.0  |  5.4192 ns |  0.63 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 8.0  | .NET 8.0  |  5.4610 ns |  0.64 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 8.0  | .NET 8.0  |  5.4888 ns |  0.64 |      - |         - |        0.00 |
| Tap_Success_Lambda           | .NET 9.0  | .NET 9.0  |  7.6712 ns |  0.90 | 0.0038 |      64 B |        1.00 |
| Tap_Success_Lambda           | .NET 8.0  | .NET 8.0  |  7.9447 ns |  0.93 | 0.0038 |      64 B |        1.00 |
| Map_Failure_Lambda           | .NET 8.0  | .NET 8.0  |  8.5206 ns |  0.99 | 0.0038 |      64 B |        1.00 |
| Map_Success_Lambda           | .NET 8.0  | .NET 8.0  |  8.5655 ns |  1.00 | 0.0038 |      64 B |        1.00 |
| Map_Failure_Lambda           | .NET 9.0  | .NET 9.0  |  8.5866 ns |  1.00 | 0.0038 |      64 B |        1.00 |
| Map_Success_Lambda           | .NET 9.0  | .NET 9.0  |  8.6014 ns |  1.00 | 0.0038 |      64 B |        1.00 |
| FullPipeline_TState          | .NET 10.0 | .NET 10.0 | 11.9184 ns |  1.39 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 9.0  | .NET 9.0  | 12.1663 ns |  1.42 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 8.0  | .NET 8.0  | 12.2258 ns |  1.43 | 0.0019 |      32 B |        0.50 |
| FullPipeline_Lambda          | .NET 10.0 | .NET 10.0 | 12.8831 ns |  1.50 | 0.0019 |      32 B |        0.50 |
| Match_Success_Lambda         | .NET 10.0 | .NET 10.0 | 23.8121 ns |  2.78 | 0.0019 |      32 B |        0.50 |
| Match_Success_TState         | .NET 10.0 | .NET 10.0 | 24.3795 ns |  2.85 | 0.0024 |      40 B |        0.62 |
| FullPipeline_Lambda          | .NET 9.0  | .NET 9.0  | 26.3831 ns |  3.08 | 0.0095 |     160 B |        2.50 |
| Match_Success_Lambda         | .NET 9.0  | .NET 9.0  | 26.6249 ns |  3.11 | 0.0019 |      32 B |        0.50 |
| FullPipeline_Lambda          | .NET 8.0  | .NET 8.0  | 27.8063 ns |  3.25 | 0.0095 |     160 B |        2.50 |
| Match_Success_TState         | .NET 9.0  | .NET 9.0  | 28.6522 ns |  3.35 | 0.0024 |      40 B |        0.62 |
| Match_Success_Lambda         | .NET 8.0  | .NET 8.0  | 45.6544 ns |  5.33 | 0.0019 |      32 B |        0.50 |
| Match_Success_TState         | .NET 8.0  | .NET 8.0  | 54.4505 ns |  6.36 | 0.0024 |      40 B |        0.62 |
