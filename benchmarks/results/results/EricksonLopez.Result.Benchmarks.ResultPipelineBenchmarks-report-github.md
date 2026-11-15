```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.68GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v4


```
| Method                       | Job       | Runtime   | Mean       | Median     | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------- |---------- |-----------:|-----------:|------:|-------:|----------:|------------:|
| Map_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.2417 ns |  0.2414 ns |  0.03 |      - |         - |        0.00 |
| Map_Failure_Lambda           | .NET 10.0 | .NET 10.0 |  0.4988 ns |  0.4963 ns |  0.06 |      - |         - |        0.00 |
| Tap_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.5951 ns |  0.5953 ns |  0.07 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 10.0 | .NET 10.0 |  0.7225 ns |  0.7214 ns |  0.08 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 10.0 | .NET 10.0 |  0.7412 ns |  0.7411 ns |  0.09 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 10.0 | .NET 10.0 |  0.7533 ns |  0.7538 ns |  0.09 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 10.0 | .NET 10.0 |  0.7572 ns |  0.7600 ns |  0.09 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 10.0 | .NET 10.0 |  0.7757 ns |  0.7783 ns |  0.09 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 9.0  | .NET 9.0  |  1.4379 ns |  1.4367 ns |  0.17 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 8.0  | .NET 8.0  |  1.4396 ns |  1.4396 ns |  0.17 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 10.0 | .NET 10.0 |  1.5546 ns |  1.5562 ns |  0.18 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 9.0  | .NET 9.0  |  2.7521 ns |  2.7496 ns |  0.32 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 9.0  | .NET 9.0  |  3.1971 ns |  3.1953 ns |  0.37 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 10.0 | .NET 10.0 |  3.2783 ns |  3.2778 ns |  0.38 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 9.0  | .NET 9.0  |  3.6884 ns |  3.6078 ns |  0.43 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 8.0  | .NET 8.0  |  3.7617 ns |  3.7601 ns |  0.44 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 8.0  | .NET 8.0  |  3.8011 ns |  3.7957 ns |  0.44 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 8.0  | .NET 8.0  |  4.1463 ns |  4.1465 ns |  0.48 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 10.0 | .NET 10.0 |  4.5277 ns |  4.5254 ns |  0.53 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 10.0 | .NET 10.0 |  4.5533 ns |  4.5513 ns |  0.53 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 8.0  | .NET 8.0  |  5.1485 ns |  5.1486 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 9.0  | .NET 9.0  |  5.1557 ns |  5.1564 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 8.0  | .NET 8.0  |  5.1731 ns |  5.1725 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 8.0  | .NET 8.0  |  5.1748 ns |  5.1759 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 9.0  | .NET 9.0  |  5.1847 ns |  5.1838 ns |  0.60 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 9.0  | .NET 9.0  |  5.2034 ns |  5.2021 ns |  0.61 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 9.0  | .NET 9.0  |  5.4085 ns |  5.4090 ns |  0.63 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 8.0  | .NET 8.0  |  5.4193 ns |  5.4199 ns |  0.63 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 9.0  | .NET 9.0  |  5.4195 ns |  5.4192 ns |  0.63 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 8.0  | .NET 8.0  |  5.4302 ns |  5.4302 ns |  0.63 |      - |         - |        0.00 |
| Tap_Success_Lambda           | .NET 8.0  | .NET 8.0  |  7.4417 ns |  7.4525 ns |  0.87 | 0.0038 |      64 B |        1.00 |
| Tap_Success_Lambda           | .NET 9.0  | .NET 9.0  |  7.5975 ns |  7.5955 ns |  0.88 | 0.0038 |      64 B |        1.00 |
| Map_Failure_Lambda           | .NET 8.0  | .NET 8.0  |  8.4937 ns |  8.4942 ns |  0.99 | 0.0038 |      64 B |        1.00 |
| Map_Success_Lambda           | .NET 9.0  | .NET 9.0  |  8.5494 ns |  8.5416 ns |  1.00 | 0.0038 |      64 B |        1.00 |
| Map_Success_Lambda           | .NET 8.0  | .NET 8.0  |  8.5880 ns |  8.5891 ns |  1.00 | 0.0038 |      64 B |        1.00 |
| Map_Failure_Lambda           | .NET 9.0  | .NET 9.0  |  8.5911 ns |  8.5771 ns |  1.00 | 0.0038 |      64 B |        1.00 |
| FullPipeline_Lambda          | .NET 10.0 | .NET 10.0 | 11.1832 ns | 11.1864 ns |  1.30 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 10.0 | .NET 10.0 | 11.7275 ns | 11.7220 ns |  1.37 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 9.0  | .NET 9.0  | 11.8827 ns | 11.8873 ns |  1.38 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 8.0  | .NET 8.0  | 12.8418 ns | 12.7971 ns |  1.50 | 0.0019 |      32 B |        0.50 |
| Match_Success_Lambda         | .NET 10.0 | .NET 10.0 | 23.2555 ns | 23.2381 ns |  2.71 | 0.0019 |      32 B |        0.50 |
| Match_Success_TState         | .NET 10.0 | .NET 10.0 | 24.4351 ns | 24.4489 ns |  2.85 | 0.0024 |      40 B |        0.62 |
| FullPipeline_Lambda          | .NET 8.0  | .NET 8.0  | 25.5276 ns | 25.5187 ns |  2.97 | 0.0095 |     160 B |        2.50 |
| FullPipeline_Lambda          | .NET 9.0  | .NET 9.0  | 26.2752 ns | 26.3077 ns |  3.06 | 0.0095 |     160 B |        2.50 |
| Match_Success_Lambda         | .NET 9.0  | .NET 9.0  | 26.4398 ns | 26.4407 ns |  3.08 | 0.0019 |      32 B |        0.50 |
| Match_Success_TState         | .NET 9.0  | .NET 9.0  | 27.2640 ns | 27.2309 ns |  3.17 | 0.0024 |      40 B |        0.62 |
| Match_Success_Lambda         | .NET 8.0  | .NET 8.0  | 44.3886 ns | 44.3740 ns |  5.17 | 0.0019 |      32 B |        0.50 |
| Match_Success_TState         | .NET 8.0  | .NET 8.0  | 55.6030 ns | 55.6193 ns |  6.47 | 0.0024 |      40 B |        0.62 |
