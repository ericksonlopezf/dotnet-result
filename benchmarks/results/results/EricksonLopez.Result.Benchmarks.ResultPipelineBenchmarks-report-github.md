```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.21GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4


```
| Method                       | Job       | Runtime   | Mean       | Median     | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------- |---------- |-----------:|-----------:|------:|-------:|----------:|------------:|
| Map_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.0000 ns |  0.0000 ns | 0.000 |      - |         - |        0.00 |
| Map_Failure_Lambda           | .NET 10.0 | .NET 10.0 |  0.0398 ns |  0.0436 ns | 0.006 |      - |         - |        0.00 |
| Tap_Success_Lambda           | .NET 10.0 | .NET 10.0 |  0.0569 ns |  0.0560 ns | 0.008 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 10.0 | .NET 10.0 |  0.2332 ns |  0.2349 ns | 0.034 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 10.0 | .NET 10.0 |  0.2759 ns |  0.2864 ns | 0.040 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 10.0 | .NET 10.0 |  0.3198 ns |  0.3179 ns | 0.047 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 10.0 | .NET 10.0 |  0.4614 ns |  0.4605 ns | 0.067 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 10.0 | .NET 10.0 |  0.4936 ns |  0.4857 ns | 0.072 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 10.0 | .NET 10.0 |  0.6128 ns |  0.6118 ns | 0.089 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 9.0  | .NET 9.0  |  0.6214 ns |  0.6273 ns | 0.090 |      - |         - |        0.00 |
| Tap_Success_TState           | .NET 8.0  | .NET 8.0  |  0.6252 ns |  0.6164 ns | 0.091 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 9.0  | .NET 9.0  |  1.8470 ns |  1.8395 ns | 0.269 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 8.0  | .NET 8.0  |  1.9260 ns |  1.9252 ns | 0.280 |      - |         - |        0.00 |
| Bind_Failure_Lambda          | .NET 10.0 | .NET 10.0 |  2.1373 ns |  2.1499 ns | 0.311 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 9.0  | .NET 9.0  |  2.3302 ns |  2.3243 ns | 0.339 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 9.0  | .NET 9.0  |  2.3533 ns |  2.3421 ns | 0.343 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 8.0  | .NET 8.0  |  2.3765 ns |  2.3736 ns | 0.346 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 8.0  | .NET 8.0  |  2.3887 ns |  2.3650 ns | 0.348 |      - |         - |        0.00 |
| Bind_Success_TState          | .NET 10.0 | .NET 10.0 |  2.5626 ns |  2.5897 ns | 0.373 |      - |         - |        0.00 |
| Bind_Success_Lambda          | .NET 10.0 | .NET 10.0 |  2.6004 ns |  2.6028 ns | 0.378 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 9.0  | .NET 9.0  |  3.5744 ns |  3.5725 ns | 0.520 |      - |         - |        0.00 |
| Map_Failure_TState           | .NET 8.0  | .NET 8.0  |  3.6849 ns |  3.7039 ns | 0.536 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 8.0  | .NET 8.0  |  3.8840 ns |  3.8947 ns | 0.565 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 8.0  | .NET 8.0  |  3.8969 ns |  3.9062 ns | 0.567 |      - |         - |        0.00 |
| Ensure_Success_Passes_Lambda | .NET 9.0  | .NET 9.0  |  3.9078 ns |  3.8955 ns | 0.569 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 8.0  | .NET 8.0  |  3.9301 ns |  3.9229 ns | 0.572 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 9.0  | .NET 9.0  |  3.9322 ns |  3.9421 ns | 0.572 |      - |         - |        0.00 |
| Map_Success_TState           | .NET 9.0  | .NET 9.0  |  3.9631 ns |  3.9610 ns | 0.577 |      - |         - |        0.00 |
| Ensure_Success_Passes_TState | .NET 9.0  | .NET 9.0  |  4.0037 ns |  4.0063 ns | 0.583 |      - |         - |        0.00 |
| Ensure_Success_Fails         | .NET 8.0  | .NET 8.0  |  4.0311 ns |  4.0453 ns | 0.587 |      - |         - |        0.00 |
| Tap_Success_Lambda           | .NET 8.0  | .NET 8.0  |  4.7841 ns |  4.7633 ns | 0.696 | 0.0038 |      64 B |        1.00 |
| Tap_Success_Lambda           | .NET 9.0  | .NET 9.0  |  4.7854 ns |  4.7310 ns | 0.696 | 0.0038 |      64 B |        1.00 |
| Map_Failure_Lambda           | .NET 8.0  | .NET 8.0  |  6.5586 ns |  6.5236 ns | 0.955 | 0.0038 |      64 B |        1.00 |
| Map_Failure_Lambda           | .NET 9.0  | .NET 9.0  |  6.7503 ns |  6.5906 ns | 0.982 | 0.0038 |      64 B |        1.00 |
| Map_Success_Lambda           | .NET 8.0  | .NET 8.0  |  6.8734 ns |  6.8266 ns | 1.000 | 0.0038 |      64 B |        1.00 |
| Map_Success_Lambda           | .NET 9.0  | .NET 9.0  |  6.9896 ns |  6.9870 ns | 1.017 | 0.0038 |      64 B |        1.00 |
| FullPipeline_Lambda          | .NET 10.0 | .NET 10.0 |  8.3253 ns |  8.3152 ns | 1.212 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 9.0  | .NET 9.0  |  8.4637 ns |  8.4150 ns | 1.232 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 8.0  | .NET 8.0  |  8.6608 ns |  8.6686 ns | 1.261 | 0.0019 |      32 B |        0.50 |
| FullPipeline_TState          | .NET 10.0 | .NET 10.0 |  9.0420 ns |  8.9416 ns | 1.316 | 0.0019 |      32 B |        0.50 |
| Match_Success_Lambda         | .NET 10.0 | .NET 10.0 | 14.1901 ns | 14.1600 ns | 2.065 | 0.0019 |      32 B |        0.50 |
| Match_Success_TState         | .NET 10.0 | .NET 10.0 | 14.9293 ns | 14.9343 ns | 2.173 | 0.0024 |      40 B |        0.62 |
| FullPipeline_Lambda          | .NET 9.0  | .NET 9.0  | 16.4624 ns | 16.3588 ns | 2.396 | 0.0095 |     160 B |        2.50 |
| FullPipeline_Lambda          | .NET 8.0  | .NET 8.0  | 16.4983 ns | 16.4939 ns | 2.401 | 0.0095 |     160 B |        2.50 |
| Match_Success_Lambda         | .NET 9.0  | .NET 9.0  | 17.3550 ns | 17.2767 ns | 2.526 | 0.0019 |      32 B |        0.50 |
| Match_Success_TState         | .NET 9.0  | .NET 9.0  | 19.5437 ns | 19.5930 ns | 2.844 | 0.0024 |      40 B |        0.62 |
| Match_Success_Lambda         | .NET 8.0  | .NET 8.0  | 27.8382 ns | 27.9390 ns | 4.052 | 0.0019 |      32 B |        0.50 |
| Match_Success_TState         | .NET 8.0  | .NET 8.0  | 29.6581 ns | 29.6220 ns | 4.317 | 0.0024 |      40 B |        0.62 |
