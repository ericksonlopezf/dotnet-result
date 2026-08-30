
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.4 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.400
  [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.19 (9.0.19, 9.0.1926.36724), X64 RyuJIT x86-64-v3


 Method                       | Job       | Runtime   | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
----------------------------- |---------- |---------- |-----------:|------:|-------:|----------:|------------:|
 Map_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.5500 ns |  0.04 |      - |         - |        0.00 |
 Map_Failure_Lambda           | .NET 10.0 | .NET 10.0 |  0.8556 ns |  0.07 |      - |         - |        0.00 |
 Tap_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.8668 ns |  0.07 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 10.0 | .NET 10.0 |  1.0277 ns |  0.08 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 10.0 | .NET 10.0 |  1.1882 ns |  0.10 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 10.0 | .NET 10.0 |  1.2079 ns |  0.10 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 10.0 | .NET 10.0 |  1.2314 ns |  0.10 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 10.0 | .NET 10.0 |  1.2325 ns |  0.10 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 8.0  | .NET 8.0  |  1.7660 ns |  0.14 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 9.0  | .NET 9.0  |  1.8612 ns |  0.15 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 10.0 | .NET 10.0 |  1.9473 ns |  0.16 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 9.0  | .NET 9.0  |  3.7435 ns |  0.30 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 8.0  | .NET 8.0  |  3.7625 ns |  0.30 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 9.0  | .NET 9.0  |  4.0583 ns |  0.32 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 9.0  | .NET 9.0  |  4.0763 ns |  0.33 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 10.0 | .NET 10.0 |  4.1574 ns |  0.33 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 8.0  | .NET 8.0  |  4.3868 ns |  0.35 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 8.0  | .NET 8.0  |  4.6999 ns |  0.38 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 10.0 | .NET 10.0 |  5.3058 ns |  0.42 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 10.0 | .NET 10.0 |  5.3246 ns |  0.43 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 9.0  | .NET 9.0  |  5.8395 ns |  0.47 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 8.0  | .NET 8.0  |  5.9727 ns |  0.48 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 9.0  | .NET 9.0  |  6.1441 ns |  0.49 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 8.0  | .NET 8.0  |  6.2235 ns |  0.50 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 8.0  | .NET 8.0  |  8.1957 ns |  0.66 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 9.0  | .NET 9.0  |  8.3201 ns |  0.67 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 9.0  | .NET 9.0  |  8.3207 ns |  0.67 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 9.0  | .NET 9.0  |  8.3361 ns |  0.67 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 8.0  | .NET 8.0  |  8.3428 ns |  0.67 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 8.0  | .NET 8.0  |  8.3452 ns |  0.67 |      - |         - |        0.00 |
 Tap_Success_Lambda           | .NET 9.0  | .NET 9.0  |  9.2757 ns |  0.74 | 0.0038 |      64 B |        1.00 |
 Tap_Success_Lambda           | .NET 8.0  | .NET 8.0  |  9.4788 ns |  0.76 | 0.0038 |      64 B |        1.00 |
 Map_Success_Lambda           | .NET 9.0  | .NET 9.0  | 10.7387 ns |  0.86 | 0.0038 |      64 B |        1.00 |
 Map_Failure_Lambda           | .NET 9.0  | .NET 9.0  | 11.7490 ns |  0.94 | 0.0038 |      64 B |        1.00 |
 Map_Failure_Lambda           | .NET 8.0  | .NET 8.0  | 12.2794 ns |  0.98 | 0.0038 |      64 B |        1.00 |
 Map_Success_Lambda           | .NET 8.0  | .NET 8.0  | 12.5029 ns |  1.00 | 0.0038 |      64 B |        1.00 |
 FullPipeline_Lambda          | .NET 10.0 | .NET 10.0 | 15.1037 ns |  1.21 | 0.0019 |      32 B |        0.50 |
 FullPipeline_TState          | .NET 8.0  | .NET 8.0  | 16.0103 ns |  1.28 | 0.0019 |      32 B |        0.50 |
 FullPipeline_TState          | .NET 9.0  | .NET 9.0  | 16.2371 ns |  1.30 | 0.0019 |      32 B |        0.50 |
 FullPipeline_TState          | .NET 10.0 | .NET 10.0 | 16.3745 ns |  1.31 | 0.0019 |      32 B |        0.50 |
 Match_Success_Lambda         | .NET 10.0 | .NET 10.0 | 29.0116 ns |  2.32 | 0.0019 |      32 B |        0.50 |
 FullPipeline_Lambda          | .NET 8.0  | .NET 8.0  | 29.4422 ns |  2.35 | 0.0095 |     160 B |        2.50 |
 FullPipeline_Lambda          | .NET 9.0  | .NET 9.0  | 31.7192 ns |  2.54 | 0.0095 |     160 B |        2.50 |
 Match_Success_Lambda         | .NET 9.0  | .NET 9.0  | 33.1593 ns |  2.65 | 0.0019 |      32 B |        0.50 |
 Match_Success_TState         | .NET 10.0 | .NET 10.0 | 33.1874 ns |  2.65 | 0.0024 |      40 B |        0.62 |
 Match_Success_TState         | .NET 9.0  | .NET 9.0  | 39.3093 ns |  3.14 | 0.0024 |      40 B |        0.62 |
 Match_Success_Lambda         | .NET 8.0  | .NET 8.0  | 52.9727 ns |  4.24 | 0.0019 |      32 B |        0.50 |
 Match_Success_TState         | .NET 8.0  | .NET 8.0  | 58.0698 ns |  4.64 | 0.0024 |      40 B |        0.62 |
