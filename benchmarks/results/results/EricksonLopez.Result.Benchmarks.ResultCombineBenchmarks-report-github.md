```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.21 (9.0.21, 9.0.2126.50404), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.32 (8.0.32, 8.0.3226.50405), X64 RyuJIT x86-64-v3


```
| Method               | Job       | Runtime   | Count | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------- |---------- |---------- |------ |-----------:|------:|-------:|----------:|------------:|
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 4     |   6.218 ns |  0.91 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 4     |   6.375 ns |  0.93 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 4     |   6.755 ns |  0.98 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 4     |   6.870 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 4     |   7.423 ns |  1.08 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 4     |   7.896 ns |  1.15 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 4     | 119.465 ns | 17.39 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 4     | 138.301 ns | 20.13 | 0.0162 |     272 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 4     | 157.752 ns | 22.96 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 4     | 162.646 ns | 23.68 | 0.0162 |     272 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 4     | 191.158 ns | 27.83 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 4     | 204.465 ns | 29.76 | 0.0162 |     272 B |          NA |
|                      |           |           |       |            |       |        |           |             |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 16    |  14.336 ns |  0.96 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 16    |  14.862 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 16    |  14.951 ns |  1.01 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 16    |  15.403 ns |  1.04 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 16    |  16.438 ns |  1.11 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 16    |  17.486 ns |  1.18 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 16    | 170.147 ns | 11.45 | 0.0200 |     336 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 16    | 189.995 ns | 12.78 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 16    | 221.358 ns | 14.89 | 0.0281 |     472 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 16    | 230.297 ns | 15.50 | 0.0281 |     472 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 16    | 243.330 ns | 16.37 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 16    | 291.097 ns | 19.59 | 0.0281 |     472 B |          NA |
|                      |           |           |       |            |       |        |           |             |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 64    |  44.001 ns |  0.88 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 64    |  49.297 ns |  0.99 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 64    |  50.037 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 64    |  50.301 ns |  1.01 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 64    |  60.982 ns |  1.22 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 64    |  67.316 ns |  1.35 |      - |         - |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 64    | 355.627 ns |  7.11 | 0.0434 |     728 B |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 64    | 358.624 ns |  7.17 | 0.0434 |     728 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 64    | 442.188 ns |  8.84 | 0.0434 |     728 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 64    | 501.452 ns | 10.02 | 0.0734 |    1240 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 64    | 505.079 ns | 10.09 | 0.0734 |    1240 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 64    | 605.764 ns | 12.11 | 0.0734 |    1240 B |          NA |
