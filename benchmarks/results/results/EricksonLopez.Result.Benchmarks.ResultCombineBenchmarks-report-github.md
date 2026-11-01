```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.21GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4


```
| Method               | Job       | Runtime   | Count | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------- |---------- |---------- |------ |-----------:|------:|-------:|----------:|------------:|
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 4     |   2.593 ns |  0.67 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 4     |   3.075 ns |  0.79 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 4     |   3.530 ns |  0.91 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 4     |   3.663 ns |  0.94 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 4     |   3.688 ns |  0.95 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 4     |   3.894 ns |  1.00 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 4     |  59.528 ns | 15.29 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 4     |  62.212 ns | 15.98 | 0.0162 |     272 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 4     |  80.930 ns | 20.79 | 0.0162 |     272 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 4     | 101.246 ns | 26.01 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 4     | 104.832 ns | 26.93 | 0.0162 |     272 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 4     | 112.930 ns | 29.01 | 0.0143 |     240 B |          NA |
|                      |           |           |       |            |       |        |           |             |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 16    |   7.216 ns |  0.88 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 16    |   7.645 ns |  0.94 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 16    |   8.164 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 16    |   8.673 ns |  1.06 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 16    |   9.622 ns |  1.18 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 16    |  10.056 ns |  1.23 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 16    |  79.193 ns |  9.70 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 16    | 108.616 ns | 13.31 | 0.0281 |     472 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 16    | 115.781 ns | 14.18 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 16    | 118.665 ns | 14.54 | 0.0281 |     472 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 16    | 137.764 ns | 16.88 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 16    | 153.960 ns | 18.86 | 0.0281 |     472 B |          NA |
|                      |           |           |       |            |       |        |           |             |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 64    |  21.657 ns |  0.83 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 64    |  22.852 ns |  0.88 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 64    |  26.110 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 64    |  26.687 ns |  1.02 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 64    |  34.415 ns |  1.32 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 64    |  34.766 ns |  1.33 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 64    | 178.140 ns |  6.82 | 0.0434 |     728 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 64    | 198.530 ns |  7.61 | 0.0434 |     728 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 64    | 233.706 ns |  8.95 | 0.0434 |     728 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 64    | 260.662 ns |  9.99 | 0.0739 |    1240 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 64    | 274.966 ns | 10.53 | 0.0739 |    1240 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 64    | 308.788 ns | 11.83 | 0.0739 |    1240 B |          NA |
