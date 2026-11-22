
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.68GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v4


 Method                       | Job       | Runtime   | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
----------------------------- |---------- |---------- |-----------:|------:|-------:|----------:|------------:|
 Map_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.2403 ns |  0.03 |      - |         - |        0.00 |
 Map_Failure_Lambda           | .NET 10.0 | .NET 10.0 |  0.5000 ns |  0.06 |      - |         - |        0.00 |
 Tap_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.5816 ns |  0.06 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 10.0 | .NET 10.0 |  0.7315 ns |  0.08 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 10.0 | .NET 10.0 |  0.7552 ns |  0.08 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 10.0 | .NET 10.0 |  0.7622 ns |  0.08 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 10.0 | .NET 10.0 |  0.7699 ns |  0.08 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 10.0 | .NET 10.0 |  0.9297 ns |  0.10 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 10.0 | .NET 10.0 |  1.4220 ns |  0.16 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 8.0  | .NET 8.0  |  1.4419 ns |  0.16 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 9.0  | .NET 9.0  |  1.4447 ns |  0.16 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 9.0  | .NET 9.0  |  2.7515 ns |  0.30 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 9.0  | .NET 9.0  |  3.1489 ns |  0.35 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 9.0  | .NET 9.0  |  3.2112 ns |  0.35 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 10.0 | .NET 10.0 |  3.2754 ns |  0.36 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 8.0  | .NET 8.0  |  3.7971 ns |  0.42 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 8.0  | .NET 8.0  |  3.8252 ns |  0.42 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 8.0  | .NET 8.0  |  4.1945 ns |  0.46 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 10.0 | .NET 10.0 |  4.5263 ns |  0.50 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 10.0 | .NET 10.0 |  4.5851 ns |  0.50 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 8.0  | .NET 8.0  |  5.1662 ns |  0.57 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 8.0  | .NET 8.0  |  5.1741 ns |  0.57 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 9.0  | .NET 9.0  |  5.1759 ns |  0.57 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 8.0  | .NET 8.0  |  5.1896 ns |  0.57 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 9.0  | .NET 9.0  |  5.1948 ns |  0.57 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 9.0  | .NET 9.0  |  5.1963 ns |  0.57 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 8.0  | .NET 8.0  |  5.4034 ns |  0.59 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 9.0  | .NET 9.0  |  5.4113 ns |  0.60 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 9.0  | .NET 9.0  |  5.4210 ns |  0.60 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 8.0  | .NET 8.0  |  5.4765 ns |  0.60 |      - |         - |        0.00 |
 Tap_Success_Lambda           | .NET 8.0  | .NET 8.0  |  7.4133 ns |  0.82 | 0.0038 |      64 B |        1.00 |
 Tap_Success_Lambda           | .NET 9.0  | .NET 9.0  |  7.7507 ns |  0.85 | 0.0038 |      64 B |        1.00 |
 Map_Success_Lambda           | .NET 9.0  | .NET 9.0  |  8.5144 ns |  0.94 | 0.0038 |      64 B |        1.00 |
 Map_Failure_Lambda           | .NET 9.0  | .NET 9.0  |  8.5625 ns |  0.94 | 0.0038 |      64 B |        1.00 |
 Map_Failure_Lambda           | .NET 8.0  | .NET 8.0  |  8.9157 ns |  0.98 | 0.0038 |      64 B |        1.00 |
 Map_Success_Lambda           | .NET 8.0  | .NET 8.0  |  9.0839 ns |  1.00 | 0.0038 |      64 B |        1.00 |
 FullPipeline_TState          | .NET 10.0 | .NET 10.0 | 11.9926 ns |  1.32 | 0.0019 |      32 B |        0.50 |
 FullPipeline_Lambda          | .NET 10.0 | .NET 10.0 | 12.1574 ns |  1.34 | 0.0019 |      32 B |        0.50 |
 FullPipeline_TState          | .NET 9.0  | .NET 9.0  | 12.5928 ns |  1.39 | 0.0019 |      32 B |        0.50 |
 FullPipeline_TState          | .NET 8.0  | .NET 8.0  | 13.0586 ns |  1.44 | 0.0019 |      32 B |        0.50 |
 Match_Success_Lambda         | .NET 10.0 | .NET 10.0 | 23.8949 ns |  2.63 | 0.0019 |      32 B |        0.50 |
 Match_Success_TState         | .NET 10.0 | .NET 10.0 | 24.9860 ns |  2.75 | 0.0024 |      40 B |        0.62 |
 FullPipeline_Lambda          | .NET 8.0  | .NET 8.0  | 25.6155 ns |  2.82 | 0.0095 |     160 B |        2.50 |
 Match_Success_Lambda         | .NET 9.0  | .NET 9.0  | 26.1213 ns |  2.88 | 0.0019 |      32 B |        0.50 |
 FullPipeline_Lambda          | .NET 9.0  | .NET 9.0  | 26.7955 ns |  2.95 | 0.0095 |     160 B |        2.50 |
 Match_Success_TState         | .NET 9.0  | .NET 9.0  | 27.3367 ns |  3.01 | 0.0024 |      40 B |        0.62 |
 Match_Success_Lambda         | .NET 8.0  | .NET 8.0  | 44.0247 ns |  4.85 | 0.0019 |      32 B |        0.50 |
 Match_Success_TState         | .NET 8.0  | .NET 8.0  | 53.6503 ns |  5.91 | 0.0024 |      40 B |        0.62 |
