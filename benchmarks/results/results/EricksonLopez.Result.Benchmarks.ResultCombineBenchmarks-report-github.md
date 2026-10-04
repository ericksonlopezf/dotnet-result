```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


```
| Method               | Job       | Runtime   | Count | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------- |---------- |---------- |------ |-----------:|------:|-------:|----------:|------------:|
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 4     |   5.925 ns |  0.80 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 4     |   6.627 ns |  0.89 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 4     |   7.408 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 4     |   8.359 ns |  1.13 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 4     |   8.392 ns |  1.13 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 4     |  11.641 ns |  1.57 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 4     | 109.183 ns | 14.74 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 4     | 120.880 ns | 16.32 | 0.0162 |     272 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 4     | 145.670 ns | 19.67 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 4     | 146.333 ns | 19.76 | 0.0162 |     272 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 4     | 193.731 ns | 26.15 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 4     | 201.251 ns | 27.17 | 0.0162 |     272 B |          NA |
|                      |           |           |       |            |       |        |           |             |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 16    |  13.706 ns |  0.94 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 16    |  14.558 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 16    |  14.862 ns |  1.02 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 16    |  15.896 ns |  1.09 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 16    |  20.742 ns |  1.42 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 16    |  30.798 ns |  2.12 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 16    | 149.316 ns | 10.26 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 16    | 178.382 ns | 12.25 | 0.0281 |     472 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 16    | 182.734 ns | 12.55 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 16    | 214.813 ns | 14.76 | 0.0281 |     472 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 16    | 232.516 ns | 15.97 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 16    | 272.043 ns | 18.69 | 0.0281 |     472 B |          NA |
|                      |           |           |       |            |       |        |           |             |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 64    |  39.645 ns |  0.73 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 64    |  49.188 ns |  0.91 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 64    |  50.974 ns |  0.94 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 64    |  54.338 ns |  1.00 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 64    |  70.645 ns |  1.30 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 64    |  71.338 ns |  1.31 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 64    | 294.413 ns |  5.42 | 0.0434 |     728 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 64    | 347.908 ns |  6.40 | 0.0434 |     728 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 64    | 411.147 ns |  7.57 | 0.0434 |     728 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 64    | 426.046 ns |  7.84 | 0.0739 |    1240 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 64    | 453.094 ns |  8.34 | 0.0739 |    1240 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 64    | 564.836 ns | 10.39 | 0.0734 |    1240 B |          NA |
