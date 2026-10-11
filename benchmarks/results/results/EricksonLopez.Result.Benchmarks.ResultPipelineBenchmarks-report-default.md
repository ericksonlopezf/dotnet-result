
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.37GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4


 Method                       | Job       | Runtime   | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
----------------------------- |---------- |---------- |-----------:|------:|-------:|----------:|------------:|
 Map_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.0000 ns | 0.000 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 10.0 | .NET 10.0 |  0.0000 ns | 0.000 |      - |         - |        0.00 |
 Map_Failure_Lambda           | .NET 10.0 | .NET 10.0 |  0.0224 ns | 0.004 |      - |         - |        0.00 |
 Tap_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.0437 ns | 0.007 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 10.0 | .NET 10.0 |  0.0922 ns | 0.015 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 10.0 | .NET 10.0 |  0.1992 ns | 0.032 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 10.0 | .NET 10.0 |  0.2725 ns | 0.043 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 10.0 | .NET 10.0 |  0.4323 ns | 0.069 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 9.0  | .NET 9.0  |  0.5752 ns | 0.091 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 8.0  | .NET 8.0  |  0.6483 ns | 0.103 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 10.0 | .NET 10.0 |  0.7046 ns | 0.112 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 8.0  | .NET 8.0  |  1.8261 ns | 0.290 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 9.0  | .NET 9.0  |  1.8989 ns | 0.301 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 10.0 | .NET 10.0 |  2.0379 ns | 0.323 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 9.0  | .NET 9.0  |  2.2505 ns | 0.357 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 8.0  | .NET 8.0  |  2.2529 ns | 0.358 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 8.0  | .NET 8.0  |  2.2809 ns | 0.362 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 9.0  | .NET 9.0  |  2.2970 ns | 0.365 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 10.0 | .NET 10.0 |  2.4717 ns | 0.392 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 10.0 | .NET 10.0 |  2.5294 ns | 0.401 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 9.0  | .NET 9.0  |  3.5624 ns | 0.565 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 8.0  | .NET 8.0  |  3.6008 ns | 0.571 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 9.0  | .NET 9.0  |  3.7363 ns | 0.593 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 8.0  | .NET 8.0  |  3.7561 ns | 0.596 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 8.0  | .NET 8.0  |  3.7576 ns | 0.596 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 9.0  | .NET 9.0  |  3.7843 ns | 0.601 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 8.0  | .NET 8.0  |  3.7859 ns | 0.601 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 9.0  | .NET 9.0  |  3.7976 ns | 0.603 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 9.0  | .NET 9.0  |  3.8681 ns | 0.614 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 8.0  | .NET 8.0  |  3.9975 ns | 0.634 |      - |         - |        0.00 |
 Tap_Success_Lambda           | .NET 9.0  | .NET 9.0  |  4.1143 ns | 0.653 | 0.0038 |      64 B |        1.00 |
 Tap_Success_Lambda           | .NET 8.0  | .NET 8.0  |  4.7194 ns | 0.749 | 0.0038 |      64 B |        1.00 |
 Map_Failure_Lambda           | .NET 8.0  | .NET 8.0  |  6.1980 ns | 0.984 | 0.0038 |      64 B |        1.00 |
 Map_Success_Lambda           | .NET 8.0  | .NET 8.0  |  6.3019 ns | 1.000 | 0.0038 |      64 B |        1.00 |
 Map_Failure_Lambda           | .NET 9.0  | .NET 9.0  |  6.4130 ns | 1.018 | 0.0038 |      64 B |        1.00 |
 Map_Success_Lambda           | .NET 9.0  | .NET 9.0  |  6.4864 ns | 1.029 | 0.0038 |      64 B |        1.00 |
 FullPipeline_Lambda          | .NET 10.0 | .NET 10.0 |  7.6316 ns | 1.211 | 0.0019 |      32 B |        0.50 |
 FullPipeline_TState          | .NET 8.0  | .NET 8.0  |  7.9538 ns | 1.262 | 0.0019 |      32 B |        0.50 |
 FullPipeline_TState          | .NET 9.0  | .NET 9.0  |  8.0158 ns | 1.272 | 0.0019 |      32 B |        0.50 |
 FullPipeline_TState          | .NET 10.0 | .NET 10.0 |  8.6668 ns | 1.375 | 0.0019 |      32 B |        0.50 |
 Match_Success_Lambda         | .NET 10.0 | .NET 10.0 | 13.6898 ns | 2.172 | 0.0019 |      32 B |        0.50 |
 Match_Success_TState         | .NET 10.0 | .NET 10.0 | 14.7808 ns | 2.346 | 0.0024 |      40 B |        0.62 |
 FullPipeline_Lambda          | .NET 8.0  | .NET 8.0  | 15.2059 ns | 2.413 | 0.0095 |     160 B |        2.50 |
 FullPipeline_Lambda          | .NET 9.0  | .NET 9.0  | 15.5906 ns | 2.474 | 0.0095 |     160 B |        2.50 |
 Match_Success_Lambda         | .NET 9.0  | .NET 9.0  | 17.2608 ns | 2.739 | 0.0019 |      32 B |        0.50 |
 Match_Success_TState         | .NET 9.0  | .NET 9.0  | 19.0491 ns | 3.023 | 0.0024 |      40 B |        0.62 |
 Match_Success_Lambda         | .NET 8.0  | .NET 8.0  | 25.7432 ns | 4.085 | 0.0019 |      32 B |        0.50 |
 Match_Success_TState         | .NET 8.0  | .NET 8.0  | 26.8011 ns | 4.253 | 0.0024 |      40 B |        0.62 |
