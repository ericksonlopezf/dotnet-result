
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.32 (8.0.32, 8.0.3226.50405), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.21 (9.0.21, 9.0.2126.50404), X64 RyuJIT x86-64-v4


 Method                       | Job       | Runtime   | Mean       | Median     | Ratio | Gen0   | Allocated | Alloc Ratio |
----------------------------- |---------- |---------- |-----------:|-----------:|------:|-------:|----------:|------------:|
 Map_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.2400 ns |  0.2396 ns |  0.03 |      - |         - |        0.00 |
 Map_Failure_Lambda           | .NET 10.0 | .NET 10.0 |  0.5000 ns |  0.4969 ns |  0.06 |      - |         - |        0.00 |
 Tap_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.5839 ns |  0.5823 ns |  0.07 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 10.0 | .NET 10.0 |  0.7220 ns |  0.7212 ns |  0.08 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 10.0 | .NET 10.0 |  0.7441 ns |  0.7446 ns |  0.09 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 10.0 | .NET 10.0 |  0.7520 ns |  0.7517 ns |  0.09 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 10.0 | .NET 10.0 |  0.7527 ns |  0.7538 ns |  0.09 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 10.0 | .NET 10.0 |  0.7692 ns |  0.7692 ns |  0.09 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 10.0 | .NET 10.0 |  1.4223 ns |  1.4206 ns |  0.16 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 8.0  | .NET 8.0  |  1.4352 ns |  1.4352 ns |  0.16 |      - |         - |        0.00 |
 Tap_Success_TState           | .NET 9.0  | .NET 9.0  |  1.4371 ns |  1.4367 ns |  0.16 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 10.0 | .NET 10.0 |  2.7305 ns |  2.7297 ns |  0.31 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 9.0  | .NET 9.0  |  2.7448 ns |  2.7447 ns |  0.31 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 9.0  | .NET 9.0  |  3.3393 ns |  3.1929 ns |  0.38 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 9.0  | .NET 9.0  |  3.3647 ns |  3.2199 ns |  0.39 |      - |         - |        0.00 |
 Bind_Failure_Lambda          | .NET 8.0  | .NET 8.0  |  3.7460 ns |  3.7430 ns |  0.43 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 8.0  | .NET 8.0  |  3.7825 ns |  3.7828 ns |  0.43 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 10.0 | .NET 10.0 |  4.0758 ns |  4.0746 ns |  0.47 |      - |         - |        0.00 |
 Bind_Success_Lambda          | .NET 8.0  | .NET 8.0  |  4.1823 ns |  4.1549 ns |  0.48 |      - |         - |        0.00 |
 Bind_Success_TState          | .NET 10.0 | .NET 10.0 |  4.1862 ns |  4.1853 ns |  0.48 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 8.0  | .NET 8.0  |  5.1448 ns |  5.1447 ns |  0.59 |      - |         - |        0.00 |
 Ensure_Success_Fails         | .NET 9.0  | .NET 9.0  |  5.1544 ns |  5.1542 ns |  0.59 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 8.0  | .NET 8.0  |  5.1621 ns |  5.1596 ns |  0.59 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 8.0  | .NET 8.0  |  5.1760 ns |  5.1767 ns |  0.59 |      - |         - |        0.00 |
 Ensure_Success_Passes_TState | .NET 9.0  | .NET 9.0  |  5.1841 ns |  5.1861 ns |  0.59 |      - |         - |        0.00 |
 Ensure_Success_Passes_Lambda | .NET 9.0  | .NET 9.0  |  5.1880 ns |  5.1868 ns |  0.59 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 9.0  | .NET 9.0  |  5.4034 ns |  5.4024 ns |  0.62 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 9.0  | .NET 9.0  |  5.4162 ns |  5.4158 ns |  0.62 |      - |         - |        0.00 |
 Map_Success_TState           | .NET 8.0  | .NET 8.0  |  5.4660 ns |  5.4633 ns |  0.63 |      - |         - |        0.00 |
 Map_Failure_TState           | .NET 8.0  | .NET 8.0  |  5.4764 ns |  5.4769 ns |  0.63 |      - |         - |        0.00 |
 Tap_Success_Lambda           | .NET 9.0  | .NET 9.0  |  7.7989 ns |  7.7837 ns |  0.89 | 0.0038 |      64 B |        1.00 |
 Tap_Success_Lambda           | .NET 8.0  | .NET 8.0  |  7.8132 ns |  7.7900 ns |  0.89 | 0.0038 |      64 B |        1.00 |
 Map_Failure_Lambda           | .NET 8.0  | .NET 8.0  |  8.6356 ns |  8.6291 ns |  0.99 | 0.0038 |      64 B |        1.00 |
 Map_Success_Lambda           | .NET 9.0  | .NET 9.0  |  8.7235 ns |  8.7317 ns |  1.00 | 0.0038 |      64 B |        1.00 |
 Map_Success_Lambda           | .NET 8.0  | .NET 8.0  |  8.7379 ns |  8.7263 ns |  1.00 | 0.0038 |      64 B |        1.00 |
 Map_Failure_Lambda           | .NET 9.0  | .NET 9.0  |  8.9295 ns |  8.9125 ns |  1.02 | 0.0038 |      64 B |        1.00 |
 FullPipeline_Lambda          | .NET 10.0 | .NET 10.0 | 11.1133 ns | 11.1055 ns |  1.27 | 0.0019 |      32 B |        0.50 |
 FullPipeline_TState          | .NET 10.0 | .NET 10.0 | 11.7862 ns | 11.7752 ns |  1.35 | 0.0019 |      32 B |        0.50 |
 FullPipeline_TState          | .NET 9.0  | .NET 9.0  | 11.8604 ns | 11.8613 ns |  1.36 | 0.0019 |      32 B |        0.50 |
 FullPipeline_TState          | .NET 8.0  | .NET 8.0  | 12.6585 ns | 12.6758 ns |  1.45 | 0.0019 |      32 B |        0.50 |
 Match_Success_Lambda         | .NET 10.0 | .NET 10.0 | 23.5372 ns | 23.5377 ns |  2.69 | 0.0019 |      32 B |        0.50 |
 Match_Success_TState         | .NET 10.0 | .NET 10.0 | 24.0536 ns | 24.0284 ns |  2.75 | 0.0024 |      40 B |        0.62 |
 FullPipeline_Lambda          | .NET 9.0  | .NET 9.0  | 25.9450 ns | 25.9534 ns |  2.97 | 0.0095 |     160 B |        2.50 |
 Match_Success_Lambda         | .NET 9.0  | .NET 9.0  | 26.2065 ns | 26.2018 ns |  3.00 | 0.0019 |      32 B |        0.50 |
 Match_Success_TState         | .NET 9.0  | .NET 9.0  | 27.5698 ns | 27.5505 ns |  3.16 | 0.0024 |      40 B |        0.62 |
 FullPipeline_Lambda          | .NET 8.0  | .NET 8.0  | 27.8057 ns | 27.8086 ns |  3.18 | 0.0095 |     160 B |        2.50 |
 Match_Success_Lambda         | .NET 8.0  | .NET 8.0  | 43.9192 ns | 43.9370 ns |  5.03 | 0.0019 |      32 B |        0.50 |
 Match_Success_TState         | .NET 8.0  | .NET 8.0  | 53.3056 ns | 53.3474 ns |  6.10 | 0.0024 |      40 B |        0.62 |
