```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.37GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4


```
| Method               | Job       | Runtime   | Count | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------- |---------- |---------- |------ |-----------:|------:|-------:|----------:|------------:|
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 4     |   2.822 ns |  0.82 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 4     |   3.066 ns |  0.89 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 4     |   3.452 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 4     |   3.530 ns |  1.02 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 4     |   3.580 ns |  1.04 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 4     |   7.775 ns |  2.25 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 4     |  56.049 ns | 16.24 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 4     |  58.757 ns | 17.02 | 0.0162 |     272 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 4     |  69.299 ns | 20.08 | 0.0143 |     240 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 4     |  89.853 ns | 26.03 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 4     |  96.674 ns | 28.01 | 0.0162 |     272 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 4     | 111.989 ns | 32.45 | 0.0162 |     272 B |          NA |
|                      |           |           |       |            |       |        |           |             |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 16    |   7.348 ns |  0.89 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 16    |   7.620 ns |  0.92 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 16    |   8.295 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 16    |   8.819 ns |  1.06 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 16    |   9.781 ns |  1.18 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 16    |  10.044 ns |  1.21 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 16    |  75.443 ns |  9.10 | 0.0200 |     336 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 16    |  95.865 ns | 11.56 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 16    | 103.526 ns | 12.49 | 0.0281 |     472 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 16    | 116.692 ns | 14.07 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 16    | 125.793 ns | 15.17 | 0.0281 |     472 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 16    | 163.449 ns | 19.71 | 0.0281 |     472 B |          NA |
|                      |           |           |       |            |       |        |           |             |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 64    |  21.460 ns |  0.85 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 64    |  22.691 ns |  0.90 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 64    |  25.217 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 64    |  26.259 ns |  1.04 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 64    |  35.347 ns |  1.40 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 64    |  35.376 ns |  1.40 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 64    | 179.449 ns |  7.12 | 0.0434 |     728 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 64    | 184.476 ns |  7.32 | 0.0434 |     728 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 64    | 201.577 ns |  7.99 | 0.0434 |     728 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 64    | 246.036 ns |  9.76 | 0.0739 |    1240 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 64    | 275.263 ns | 10.92 | 0.0739 |    1240 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 64    | 276.899 ns | 10.98 | 0.0739 |    1240 B |          NA |
