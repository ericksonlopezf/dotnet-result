
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
INTEL XEON PLATINUM 8573C 2.30GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4


 Method                       | Job       | Runtime   | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
----------------------------- |---------- |---------- |-----------:|------:|-------:|----------:|------------:|
 Tap_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.0016 ns | 0.000 |      - |         - |        0.00 |
 Map_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.0045 ns | 0.001 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 10.0 | .NET 10.0 |  0.2788 ns | 0.032 |      - |         - |        0.00 |
 Map_Failure_Lambda           | .NET 10.0 | .NET 10.0 |  0.2801 ns | 0.032 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 10.0 | .NET 10.0 |  0.2866 ns | 0.033 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 10.0 | .NET 10.0 |  0.2940 ns | 0.034 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 10.0 | .NET 10.0 |  0.2995 ns | 0.035 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 8.0  | .NET 8.0  |  0.5432 ns | 0.063 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 9.0  | .NET 9.0  |  0.5709 ns | 0.066 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 10.0 | .NET 10.0 |  0.5710 ns | 0.066 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 10.0 | .NET 10.0 |  0.5815 ns | 0.067 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 8.0  | .NET 8.0  |  3.7390 ns | 0.432 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 10.0 | .NET 10.0 |  3.8421 ns | 0.444 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 8.0  | .NET 8.0  |  3.8778 ns | 0.448 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 9.0  | .NET 9.0  |  3.9165 ns | 0.453 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 8.0  | .NET 8.0  |  3.9218 ns | 0.453 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 8.0  | .NET 8.0  |  3.9265 ns | 0.454 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 9.0  | .NET 9.0  |  3.9318 ns | 0.454 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 9.0  | .NET 9.0  |  3.9389 ns | 0.455 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 8.0  | .NET 8.0  |  3.9435 ns | 0.456 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 9.0  | .NET 9.0  |  3.9455 ns | 0.456 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 8.0  | .NET 8.0  |  3.9560 ns | 0.457 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 9.0  | .NET 9.0  |  3.9587 ns | 0.457 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 9.0  | .NET 9.0  |  4.0061 ns | 0.463 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 9.0  | .NET 9.0  |  4.0315 ns | 0.466 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 9.0  | .NET 9.0  |  4.3501 ns | 0.503 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 10.0 | .NET 10.0 |  4.3937 ns | 0.508 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 8.0  | .NET 8.0  |  4.5286 ns | 0.523 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 8.0  | .NET 8.0  |  4.9020 ns | 0.566 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 10.0 | .NET 10.0 |  5.0024 ns | 0.578 |      - |         - |        0.00 |
 Tap_Success_Lambda           | .NET 9.0  | .NET 9.0  |  7.0069 ns | 0.810 | 0.0008 |      64 B |        1.00 |
 Tap_Success_Lambda           | .NET 8.0  | .NET 8.0  |  7.1194 ns | 0.823 | 0.0008 |      64 B |        1.00 |
 Map_Success_Lambda           | .NET 8.0  | .NET 8.0  |  8.6554 ns | 1.000 | 0.0008 |      64 B |        1.00 |
 Map_Failure_Lambda           | .NET 8.0  | .NET 8.0  |  8.7937 ns | 1.016 | 0.0008 |      64 B |        1.00 |
 Map_Success_Lambda           | .NET 9.0  | .NET 9.0  |  9.0333 ns | 1.044 | 0.0008 |      64 B |        1.00 |
 Map_Failure_Lambda           | .NET 9.0  | .NET 9.0  |  9.1813 ns | 1.061 | 0.0008 |      64 B |        1.00 |
 FullPipeline_Lambda          | .NET 10.0 | .NET 10.0 | 10.9399 ns | 1.264 | 0.0004 |      32 B |        0.50 |
 FullPipeline_TState          | .NET 10.0 | .NET 10.0 | 12.4886 ns | 1.443 | 0.0004 |      32 B |        0.50 |
 FullPipeline_TState          | .NET 9.0  | .NET 9.0  | 12.5751 ns | 1.453 | 0.0004 |      32 B |        0.50 |
 FullPipeline_TState          | .NET 8.0  | .NET 8.0  | 13.7914 ns | 1.594 | 0.0004 |      32 B |        0.50 |
 Match_Success_Lambda         | .NET 10.0 | .NET 10.0 | 21.3900 ns | 2.472 | 0.0004 |      32 B |        0.50 |
 Match_Success_TState         | .NET 10.0 | .NET 10.0 | 21.4084 ns | 2.474 | 0.0005 |      40 B |        0.62 |
 FullPipeline_Lambda          | .NET 8.0  | .NET 8.0  | 25.8014 ns | 2.982 | 0.0019 |     160 B |        2.50 |
 FullPipeline_Lambda          | .NET 9.0  | .NET 9.0  | 27.3941 ns | 3.166 | 0.0019 |     160 B |        2.50 |
 Match_Success_Lambda         | .NET 9.0  | .NET 9.0  | 28.1848 ns | 3.257 | 0.0004 |      32 B |        0.50 |
 Match_Success_TState         | .NET 9.0  | .NET 9.0  | 29.4445 ns | 3.403 | 0.0005 |      40 B |        0.62 |
 Match_Success_Lambda         | .NET 8.0  | .NET 8.0  | 35.9685 ns | 4.156 | 0.0004 |      32 B |        0.50 |
 Match_Success_TState         | .NET 8.0  | .NET 8.0  | 38.0056 ns | 4.392 | 0.0005 |      40 B |        0.62 |
