```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3


```
| Method                       | Job       | Runtime   | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------- |---------- |-----------:|------:|-------:|----------:|------------:|
| Map_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.3755 ns |  0.03 |      - |         - |        0.00 |
| Map_Failure_Lambda           | .NET 10.0 | .NET 10.0 |  0.6394 ns |  0.06 |      - |         - |        0.00 |
| Tap_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.7503 ns |  0.07 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 10.0 | .NET 10.0 |  0.9551 ns |  0.09 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 10.0 | .NET 10.0 |  0.9622 ns |  0.09 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 10.0 | .NET 10.0 |  0.9661 ns |  0.09 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 10.0 | .NET 10.0 |  0.9718 ns |  0.09 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 10.0 | .NET 10.0 |  0.9954 ns |  0.09 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 10.0 | .NET 10.0 |  1.8326 ns |  0.17 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 9.0  | .NET 9.0  |  1.8550 ns |  0.17 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 8.0  | .NET 8.0  |  1.9547 ns |  0.18 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 10.0 | .NET 10.0 |  3.5200 ns |  0.32 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 9.0  | .NET 9.0  |  3.5706 ns |  0.32 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 8.0  | .NET 8.0  |  4.1980 ns |  0.38 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 9.0  | .NET 9.0  |  4.2107 ns |  0.38 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 8.0  | .NET 8.0  |  4.3160 ns |  0.39 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 9.0  | .NET 9.0  |  4.3287 ns |  0.39 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 8.0  | .NET 8.0  |  4.9103 ns |  0.44 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 10.0 | .NET 10.0 |  5.2850 ns |  0.48 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 10.0 | .NET 10.0 |  5.3356 ns |  0.48 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 8.0  | .NET 8.0  |  6.6327 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 9.0  | .NET 9.0  |  6.6642 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 9.0  | .NET 9.0  |  6.6656 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 8.0  | .NET 8.0  |  6.6659 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 8.0  | .NET 8.0  |  6.6747 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 9.0  | .NET 9.0  |  6.6906 ns |  0.60 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 8.0  | .NET 8.0  |  6.9663 ns |  0.63 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 9.0  | .NET 9.0  |  6.9738 ns |  0.63 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 8.0  | .NET 8.0  |  6.9776 ns |  0.63 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 9.0  | .NET 9.0  |  6.9829 ns |  0.63 |      - |         - |        0.00 |
| Tap_Success_Lambda           | .NET 8.0  | .NET 8.0  |  8.7617 ns |  0.79 | 0.0038 |      64 B |        1.00 |
| Tap_Success_Lambda           | .NET 9.0  | .NET 9.0  | 10.1013 ns |  0.91 | 0.0038 |      64 B |        1.00 |
| Map_Failure_Lambda           | .NET 8.0  | .NET 8.0  | 11.0384 ns |  0.99 | 0.0038 |      64 B |        1.00 |
| Map_Success_Lambda           | .NET 9.0  | .NET 9.0  | 11.0557 ns |  1.00 | 0.0038 |      64 B |        1.00 |
| Map_Success_Lambda           | .NET 8.0  | .NET 8.0  | 11.0946 ns |  1.00 | 0.0038 |      64 B |        1.00 |
| Map_Failure_Lambda           | .NET 9.0  | .NET 9.0  | 11.2092 ns |  1.01 | 0.0038 |      64 B |        1.00 |
| FullPipeline_Lambda          | .NET 10.0 | .NET 10.0 | 14.2557 ns |  1.28 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 10.0 | .NET 10.0 | 15.0676 ns |  1.36 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 9.0  | .NET 9.0  | 15.2613 ns |  1.38 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 8.0  | .NET 8.0  | 16.0818 ns |  1.45 | 0.0019 |      32 B |        0.50 |
| Match_Success_Lambda         | .NET 10.0 | .NET 10.0 | 30.0387 ns |  2.71 | 0.0019 |      32 B |        0.50 |
| FullPipeline_Lambda          | .NET 8.0  | .NET 8.0  | 30.7085 ns |  2.77 | 0.0095 |     160 B |        2.50 |
| Match_Success_TState         | .NET 10.0 | .NET 10.0 | 30.8890 ns |  2.78 | 0.0024 |      40 B |        0.62 |
| Match_Success_Lambda         | .NET 9.0  | .NET 9.0  | 33.3562 ns |  3.01 | 0.0019 |      32 B |        0.50 |
| FullPipeline_Lambda          | .NET 9.0  | .NET 9.0  | 33.7720 ns |  3.04 | 0.0095 |     160 B |        2.50 |
| Match_Success_TState         | .NET 9.0  | .NET 9.0  | 35.5027 ns |  3.20 | 0.0024 |      40 B |        0.62 |
| Match_Success_Lambda         | .NET 8.0  | .NET 8.0  | 56.5438 ns |  5.10 | 0.0019 |      32 B |        0.50 |
| Match_Success_TState         | .NET 8.0  | .NET 8.0  | 66.9228 ns |  6.03 | 0.0024 |      40 B |        0.62 |
