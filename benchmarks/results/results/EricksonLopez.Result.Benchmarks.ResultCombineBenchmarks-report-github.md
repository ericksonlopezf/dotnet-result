```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.4 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.400
  [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.19 (9.0.19, 9.0.1926.36724), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v3


```
| Method               | Job       | Runtime   | Count | Mean       | Median     | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------- |---------- |---------- |------ |-----------:|-----------:|------:|-------:|----------:|------------:|
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 4     |   6.332 ns |   6.332 ns |  0.92 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 4     |   6.491 ns |   6.491 ns |  0.94 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 4     |   6.554 ns |   6.553 ns |  0.95 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 4     |   6.873 ns |   6.865 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 4     |   7.430 ns |   7.430 ns |  1.08 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 4     |   7.887 ns |   7.888 ns |  1.15 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 4     | 120.733 ns | 121.037 ns | 17.57 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 4     | 140.362 ns | 140.466 ns | 20.42 | 0.0162 |     272 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 4     | 160.010 ns | 160.224 ns | 23.28 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 4     | 179.832 ns | 179.942 ns | 26.17 | 0.0162 |     272 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 4     | 195.259 ns | 195.512 ns | 28.41 | 0.0143 |     240 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 4     | 212.775 ns | 215.928 ns | 30.96 | 0.0162 |     272 B |          NA |
|                      |           |           |       |            |            |       |        |           |             |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 16    |  14.314 ns |  14.331 ns |  0.97 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 16    |  14.738 ns |  14.749 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 16    |  14.986 ns |  14.942 ns |  1.02 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 16    |  15.611 ns |  15.609 ns |  1.06 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 16    |  16.309 ns |  16.290 ns |  1.11 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 16    |  17.462 ns |  17.462 ns |  1.18 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 16    | 179.312 ns | 179.289 ns | 12.17 | 0.0200 |     336 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 16    | 202.120 ns | 202.031 ns | 13.71 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 16    | 228.472 ns | 228.958 ns | 15.50 | 0.0281 |     472 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 16    | 242.238 ns | 242.419 ns | 16.44 | 0.0281 |     472 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 16    | 246.030 ns | 245.404 ns | 16.69 | 0.0200 |     336 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 16    | 297.561 ns | 297.670 ns | 20.19 | 0.0281 |     472 B |          NA |
|                      |           |           |       |            |            |       |        |           |             |
| Combine_AllSuccess   | .NET 9.0  | .NET 9.0  | 64    |  44.000 ns |  43.999 ns |  0.88 |      - |         - |          NA |
| Combine_OneFailure   | .NET 9.0  | .NET 9.0  | 64    |  49.278 ns |  49.338 ns |  0.98 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 8.0  | .NET 8.0  | 64    |  50.033 ns |  50.026 ns |  1.00 |      - |         - |          NA |
| Combine_OneFailure   | .NET 8.0  | .NET 8.0  | 64    |  50.523 ns |  50.675 ns |  1.01 |      - |         - |          NA |
| Combine_AllSuccess   | .NET 10.0 | .NET 10.0 | 64    |  61.326 ns |  61.149 ns |  1.23 |      - |         - |          NA |
| Combine_OneFailure   | .NET 10.0 | .NET 10.0 | 64    |  66.755 ns |  66.758 ns |  1.33 |      - |         - |          NA |
| Combine_HalfFailures | .NET 10.0 | .NET 10.0 | 64    | 351.724 ns | 352.579 ns |  7.03 | 0.0434 |     728 B |          NA |
| Combine_HalfFailures | .NET 9.0  | .NET 9.0  | 64    | 373.717 ns | 374.224 ns |  7.47 | 0.0434 |     728 B |          NA |
| Combine_HalfFailures | .NET 8.0  | .NET 8.0  | 64    | 462.047 ns | 463.759 ns |  9.23 | 0.0434 |     728 B |          NA |
| Combine_AllFailures  | .NET 9.0  | .NET 9.0  | 64    | 513.635 ns | 517.333 ns | 10.27 | 0.0734 |    1240 B |          NA |
| Combine_AllFailures  | .NET 10.0 | .NET 10.0 | 64    | 527.587 ns | 530.098 ns | 10.54 | 0.0734 |    1240 B |          NA |
| Combine_AllFailures  | .NET 8.0  | .NET 8.0  | 64    | 616.064 ns | 616.153 ns | 12.31 | 0.0734 |    1240 B |          NA |
