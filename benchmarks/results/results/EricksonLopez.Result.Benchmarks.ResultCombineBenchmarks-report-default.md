
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
INTEL XEON PLATINUM 8573C 2.30GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4


 Method               | Job       | Runtime   | Count | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
--------------------- |---------- |---------- |------ |-----------:|------:|-------:|----------:|------------:|
 Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 4     |   4.350 ns |  0.84 |      - |         - |          NA |
 Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 4     |   4.563 ns |  0.88 |      - |         - |          NA |
 Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 4     |   5.063 ns |  0.98 |      - |         - |          NA |
 Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 4     |   5.167 ns |  1.00 |      - |         - |          NA |
 Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 4     |   5.776 ns |  1.12 |      - |         - |          NA |
 Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 4     |   5.898 ns |  1.14 |      - |         - |          NA |
 Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 4     |  99.568 ns | 19.27 | 0.0029 |     240 B |          NA |
 Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 4     | 110.626 ns | 21.41 | 0.0032 |     272 B |          NA |
 Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 4     | 126.676 ns | 24.52 | 0.0029 |     240 B |          NA |
 Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 4     | 136.682 ns | 26.46 | 0.0031 |     272 B |          NA |
 Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 4     | 149.535 ns | 28.94 | 0.0029 |     240 B |          NA |
 Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 4     | 150.897 ns | 29.21 | 0.0031 |     272 B |          NA |
                      |           |           |       |            |       |        |           |             |
 Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 16    |  11.290 ns |  1.00 |      - |         - |          NA |
 Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 16    |  11.313 ns |  1.00 |      - |         - |          NA |
 Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 16    |  11.591 ns |  1.03 |      - |         - |          NA |
 Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 16    |  11.962 ns |  1.06 |      - |         - |          NA |
 Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 16    |  12.441 ns |  1.10 |      - |         - |          NA |
 Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 16    |  13.117 ns |  1.16 |      - |         - |          NA |
 Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 16    | 134.745 ns | 11.94 | 0.0038 |     336 B |          NA |
 Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 16    | 170.379 ns | 15.09 | 0.0038 |     336 B |          NA |
 Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 16    | 172.192 ns | 15.25 | 0.0055 |     472 B |          NA |
 Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 16    | 180.200 ns | 15.96 | 0.0038 |     336 B |          NA |
 Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 16    | 193.405 ns | 17.13 | 0.0055 |     472 B |          NA |
 Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 16    | 216.509 ns | 19.18 | 0.0055 |     472 B |          NA |
                      |           |           |       |            |       |        |           |             |
 Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 64    |  38.190 ns |  0.99 |      - |         - |          NA |
 Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 64    |  38.704 ns |  1.00 |      - |         - |          NA |
 Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 64    |  38.779 ns |  1.00 |      - |         - |          NA |
 Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 64    |  39.233 ns |  1.01 |      - |         - |          NA |
 Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 64    |  39.791 ns |  1.03 |      - |         - |          NA |
 Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 64    |  40.540 ns |  1.05 |      - |         - |          NA |
 Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 64    | 281.602 ns |  7.28 | 0.0086 |     728 B |          NA |
 Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 64    | 328.571 ns |  8.49 | 0.0086 |     728 B |          NA |
 Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 64    | 334.001 ns |  8.63 | 0.0086 |     728 B |          NA |
 Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 64    | 397.398 ns | 10.27 | 0.0148 |    1240 B |          NA |
 Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 64    | 426.494 ns | 11.02 | 0.0148 |    1240 B |          NA |
 Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 64    | 488.120 ns | 12.61 | 0.0148 |    1240 B |          NA |
