```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.68GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4


```
| Method               | Job       | Runtime   | Count | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------- |---------- |---------- |------ |-----------:|------:|-------:|----------:|------------:|
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 4     |   4.859 ns |  0.83 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 4     |   5.423 ns |  0.93 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 4     |   5.824 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 4     |   5.841 ns |  1.00 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 4     |   8.861 ns |  1.52 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 4     |  11.300 ns |  1.94 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 4     |  84.137 ns | 14.45 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 4     |  93.349 ns | 16.03 | 0.0162 |     272 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 4     | 114.364 ns | 19.64 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 4     | 115.550 ns | 19.84 | 0.0162 |     272 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 4     | 158.282 ns | 27.18 | 0.0162 |     272 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 4     | 160.622 ns | 27.58 | 0.0143 |     240 B |          NA |
|                      |           |           |       |            |       |        |           |             |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 16    |  10.467 ns |  0.93 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 16    |  11.266 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 16    |  11.447 ns |  1.02 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 16    |  11.468 ns |  1.02 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 16    |  15.085 ns |  1.34 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 16    |  15.937 ns |  1.41 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 16    | 121.369 ns | 10.77 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 16    | 143.234 ns | 12.71 | 0.0281 |     472 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 16    | 147.302 ns | 13.07 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 16    | 175.646 ns | 15.59 | 0.0281 |     472 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 16    | 191.884 ns | 17.03 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 16    | 245.293 ns | 21.77 | 0.0281 |     472 B |          NA |
|                      |           |           |       |            |       |        |           |             |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 64    |  37.989 ns |  0.90 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 64    |  38.054 ns |  0.90 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 64    |  38.119 ns |  0.90 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 64    |  42.442 ns |  1.00 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 64    |  54.936 ns |  1.29 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 64    |  55.415 ns |  1.31 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 64    | 226.800 ns |  5.35 | 0.0434 |     728 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 64    | 271.948 ns |  6.41 | 0.0434 |     728 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 64    | 332.040 ns |  7.83 | 0.0434 |     728 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 64    | 336.523 ns |  7.93 | 0.0739 |    1240 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 64    | 367.410 ns |  8.66 | 0.0739 |    1240 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 64    | 427.371 ns | 10.07 | 0.0739 |    1240 B |          NA |
