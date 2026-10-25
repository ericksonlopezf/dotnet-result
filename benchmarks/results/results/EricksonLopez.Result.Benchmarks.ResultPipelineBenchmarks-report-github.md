```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.32 (8.0.32, 8.0.3226.50405), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.21 (9.0.21, 9.0.2126.50404), X64 RyuJIT x86-64-v3


```
| Method                       | Job       | Runtime   | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------- |---------- |-----------:|------:|-------:|----------:|------------:|
| Map_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.5470 ns |  0.04 |      - |         - |        0.00 |
| Tap_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.8436 ns |  0.07 |      - |         - |        0.00 |
| Map_Failure_Lambda           | .NET 10.0 | .NET 10.0 |  0.8568 ns |  0.07 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 10.0 | .NET 10.0 |  1.1865 ns |  0.09 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 10.0 | .NET 10.0 |  1.2103 ns |  0.09 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 10.0 | .NET 10.0 |  1.2245 ns |  0.09 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 10.0 | .NET 10.0 |  1.2345 ns |  0.10 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 10.0 | .NET 10.0 |  1.8568 ns |  0.14 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 8.0  | .NET 8.0  |  1.8961 ns |  0.15 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 10.0 | .NET 10.0 |  1.9245 ns |  0.15 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 9.0  | .NET 9.0  |  1.9492 ns |  0.15 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 10.0 | .NET 10.0 |  3.7408 ns |  0.29 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 9.0  | .NET 9.0  |  3.7416 ns |  0.29 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 9.0  | .NET 9.0  |  4.0545 ns |  0.31 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 9.0  | .NET 9.0  |  4.0747 ns |  0.31 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 8.0  | .NET 8.0  |  4.3874 ns |  0.34 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 10.0 | .NET 10.0 |  4.6821 ns |  0.36 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 10.0 | .NET 10.0 |  4.6958 ns |  0.36 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 8.0  | .NET 8.0  |  5.0102 ns |  0.39 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 8.0  | .NET 8.0  |  5.3251 ns |  0.41 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 8.0  | .NET 8.0  |  6.1893 ns |  0.48 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 9.0  | .NET 9.0  |  6.1897 ns |  0.48 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 8.0  | .NET 8.0  |  7.9094 ns |  0.61 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 9.0  | .NET 9.0  |  8.0068 ns |  0.62 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 8.0  | .NET 8.0  |  8.1956 ns |  0.63 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 9.0  | .NET 9.0  |  8.3217 ns |  0.64 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 8.0  | .NET 8.0  |  8.3312 ns |  0.64 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 9.0  | .NET 9.0  |  8.3339 ns |  0.64 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 9.0  | .NET 9.0  |  8.3366 ns |  0.64 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 8.0  | .NET 8.0  |  8.3461 ns |  0.64 |      - |         - |        0.00 |
| Tap_Success_Lambda           | .NET 9.0  | .NET 9.0  |  8.8783 ns |  0.69 | 0.0038 |      64 B |        1.00 |
| Tap_Success_Lambda           | .NET 8.0  | .NET 8.0  |  9.4703 ns |  0.73 | 0.0038 |      64 B |        1.00 |
| Map_Failure_Lambda           | .NET 8.0  | .NET 8.0  | 11.6562 ns |  0.90 | 0.0038 |      64 B |        1.00 |
| Map_Failure_Lambda           | .NET 9.0  | .NET 9.0  | 11.7629 ns |  0.91 | 0.0038 |      64 B |        1.00 |
| Map_Success_Lambda           | .NET 8.0  | .NET 8.0  | 12.9504 ns |  1.00 | 0.0038 |      64 B |        1.00 |
| Map_Success_Lambda           | .NET 9.0  | .NET 9.0  | 13.3261 ns |  1.03 | 0.0038 |      64 B |        1.00 |
| FullPipeline_Lambda          | .NET 10.0 | .NET 10.0 | 14.5462 ns |  1.12 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 9.0  | .NET 9.0  | 16.2653 ns |  1.26 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 8.0  | .NET 8.0  | 16.5456 ns |  1.28 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 10.0 | .NET 10.0 | 16.6297 ns |  1.28 | 0.0019 |      32 B |        0.50 |
| Match_Success_Lambda         | .NET 10.0 | .NET 10.0 | 29.1730 ns |  2.25 | 0.0019 |      32 B |        0.50 |
| FullPipeline_Lambda          | .NET 9.0  | .NET 9.0  | 30.9561 ns |  2.39 | 0.0095 |     160 B |        2.50 |
| FullPipeline_Lambda          | .NET 8.0  | .NET 8.0  | 31.7103 ns |  2.45 | 0.0095 |     160 B |        2.50 |
| Match_Success_TState         | .NET 10.0 | .NET 10.0 | 33.4318 ns |  2.58 | 0.0024 |      40 B |        0.62 |
| Match_Success_Lambda         | .NET 9.0  | .NET 9.0  | 33.5657 ns |  2.59 | 0.0019 |      32 B |        0.50 |
| Match_Success_TState         | .NET 9.0  | .NET 9.0  | 39.4126 ns |  3.04 | 0.0024 |      40 B |        0.62 |
| Match_Success_Lambda         | .NET 8.0  | .NET 8.0  | 52.8607 ns |  4.08 | 0.0019 |      32 B |        0.50 |
| Match_Success_TState         | .NET 8.0  | .NET 8.0  | 56.4094 ns |  4.36 | 0.0024 |      40 B |        0.62 |
