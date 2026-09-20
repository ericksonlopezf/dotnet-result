
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3


 Method               | Job       | Runtime   | Count | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
--------------------- |---------- |---------- |------ |-----------:|------:|-------:|----------:|------------:|
 Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 4     |   6.342 ns |  0.92 |      - |         - |          NA |
 Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 4     |   6.347 ns |  0.92 |      - |         - |          NA |
 Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 4     |   6.403 ns |  0.93 |      - |         - |          NA |
 Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 4     |   6.869 ns |  1.00 |      - |         - |          NA |
 Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 4     |   7.434 ns |  1.08 |      - |         - |          NA |
 Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 4     |   7.890 ns |  1.15 |      - |         - |          NA |
 Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 4     | 115.570 ns | 16.82 | 0.0143 |     240 B |          NA |
 Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 4     | 131.272 ns | 19.11 | 0.0162 |     272 B |          NA |
 Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 4     | 155.172 ns | 22.59 | 0.0143 |     240 B |          NA |
 Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 4     | 165.417 ns | 24.08 | 0.0162 |     272 B |          NA |
 Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 4     | 190.775 ns | 27.77 | 0.0143 |     240 B |          NA |
 Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 4     | 208.875 ns | 30.41 | 0.0162 |     272 B |          NA |
                      |           |           |       |            |       |        |           |             |
 Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 16    |  14.191 ns |  0.96 |      - |         - |          NA |
 Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 16    |  14.751 ns |  1.00 |      - |         - |          NA |
 Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 16    |  14.929 ns |  1.01 |      - |         - |          NA |
 Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 16    |  15.451 ns |  1.05 |      - |         - |          NA |
 Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 16    |  16.375 ns |  1.11 |      - |         - |          NA |
 Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 16    |  17.726 ns |  1.20 |      - |         - |          NA |
 Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 16    | 176.676 ns | 11.98 | 0.0200 |     336 B |          NA |
 Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 16    | 201.647 ns | 13.67 | 0.0200 |     336 B |          NA |
 Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 16    | 209.892 ns | 14.23 | 0.0281 |     472 B |          NA |
 Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 16    | 235.580 ns | 15.97 | 0.0281 |     472 B |          NA |
 Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 16    | 246.698 ns | 16.72 | 0.0200 |     336 B |          NA |
 Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 16    | 300.061 ns | 20.34 | 0.0281 |     472 B |          NA |
                      |           |           |       |            |       |        |           |             |
 Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 64    |  44.305 ns |  0.89 |      - |         - |          NA |
 Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 64    |  49.118 ns |  0.98 |      - |         - |          NA |
 Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 64    |  49.993 ns |  1.00 |      - |         - |          NA |
 Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 64    |  50.836 ns |  1.02 |      - |         - |          NA |
 Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 64    |  61.382 ns |  1.23 |      - |         - |          NA |
 Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 64    |  66.388 ns |  1.33 |      - |         - |          NA |
 Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 64    | 331.652 ns |  6.63 | 0.0434 |     728 B |          NA |
 Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 64    | 339.547 ns |  6.79 | 0.0434 |     728 B |          NA |
 Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 64    | 440.550 ns |  8.81 | 0.0434 |     728 B |          NA |
 Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 64    | 486.892 ns |  9.74 | 0.0739 |    1240 B |          NA |
 Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 64    | 495.255 ns |  9.91 | 0.0734 |    1240 B |          NA |
 Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 64    | 614.386 ns | 12.29 | 0.0734 |    1240 B |          NA |
