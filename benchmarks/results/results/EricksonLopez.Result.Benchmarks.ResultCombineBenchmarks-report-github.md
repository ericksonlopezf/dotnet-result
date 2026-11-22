```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.68GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4


```
| Method               | Job       | Runtime   | Count | Mean       | Median     | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------- |---------- |---------- |------ |-----------:|-----------:|------:|-------:|----------:|------------:|
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 4     |   4.866 ns |   4.854 ns |  0.83 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 4     |   5.448 ns |   5.440 ns |  0.93 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 4     |   5.830 ns |   5.833 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 4     |   5.866 ns |   5.857 ns |  1.01 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 4     |   8.786 ns |   8.797 ns |  1.51 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 4     |   9.250 ns |   9.270 ns |  1.59 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 4     |  85.284 ns |  85.337 ns | 14.63 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 4     |  93.514 ns |  93.355 ns | 16.04 | 0.0162 |     272 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 4     | 118.228 ns | 118.040 ns | 20.28 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 4     | 133.041 ns | 133.063 ns | 22.82 | 0.0162 |     272 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 4     | 159.993 ns | 160.382 ns | 27.44 | 0.0162 |     272 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 4     | 161.150 ns | 161.202 ns | 27.64 | 0.0143 |     240 B |          NA |
|                      |           |           |       |            |            |       |        |           |             |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 16    |  10.501 ns |  10.464 ns |  0.93 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 16    |  11.304 ns |  11.311 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 16    |  11.478 ns |  11.477 ns |  1.02 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 16    |  11.536 ns |  11.517 ns |  1.02 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 16    |  15.129 ns |  15.110 ns |  1.34 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 16    |  18.463 ns |  18.412 ns |  1.63 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 16    | 125.210 ns | 125.548 ns | 11.08 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 16    | 143.744 ns | 143.994 ns | 12.72 | 0.0281 |     472 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 16    | 155.521 ns | 155.375 ns | 13.76 | 0.0200 |     336 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 16    | 186.216 ns | 186.551 ns | 16.47 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 16    | 191.707 ns | 191.335 ns | 16.96 | 0.0281 |     472 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 16    | 212.695 ns | 212.737 ns | 18.82 | 0.0281 |     472 B |          NA |
|                      |           |           |       |            |            |       |        |           |             |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 64    |  37.815 ns |  37.677 ns |  0.91 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 64    |  38.179 ns |  38.093 ns |  0.92 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 64    |  38.499 ns |  38.436 ns |  0.92 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 64    |  41.790 ns |  42.736 ns |  1.00 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 64    |  55.385 ns |  55.413 ns |  1.33 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 64    |  56.744 ns |  56.388 ns |  1.36 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 64    | 240.488 ns | 240.421 ns |  5.77 | 0.0434 |     728 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 64    | 275.836 ns | 275.573 ns |  6.62 | 0.0434 |     728 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 64    | 341.420 ns | 339.494 ns |  8.19 | 0.0434 |     728 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 64    | 349.590 ns | 348.126 ns |  8.39 | 0.0739 |    1240 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 64    | 408.783 ns | 408.225 ns |  9.80 | 0.0739 |    1240 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 64    | 443.075 ns | 441.173 ns | 10.63 | 0.0739 |    1240 B |          NA |
