```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.63GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4


```
| Method                       | Job       | Runtime   | FieldCount | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------- |---------- |----------- |-----------:|------:|-------:|----------:|------------:|
| ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 4          |   7.063 ns |  0.62 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 4          |   9.409 ns |  0.82 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 4          |  11.427 ns |  1.00 |      - |         - |          NA |
| ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 4          |  52.671 ns |  4.61 | 0.0086 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 4          |  59.149 ns |  5.18 | 0.0086 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 4          |  88.167 ns |  7.72 | 0.0086 |     144 B |          NA |
| ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 4          | 216.273 ns | 18.93 | 0.0329 |     552 B |          NA |
| ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 4          | 247.299 ns | 21.64 | 0.0329 |     552 B |          NA |
| ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 4          | 314.250 ns | 27.50 | 0.0329 |     552 B |          NA |
|                              |           |           |            |            |       |        |           |             |
| ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 10         |  10.922 ns |  0.73 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 10         |  14.929 ns |  1.00 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 10         |  15.355 ns |  1.03 |      - |         - |          NA |
| ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 10         |  58.948 ns |  3.95 | 0.0086 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 10         |  66.099 ns |  4.43 | 0.0086 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 10         |  96.124 ns |  6.44 | 0.0086 |     144 B |          NA |
| ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 10         | 389.711 ns | 26.10 | 0.0615 |    1032 B |          NA |
| ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 10         | 432.520 ns | 28.97 | 0.0615 |    1032 B |          NA |
| ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 10         | 577.630 ns | 38.69 | 0.0610 |    1032 B |          NA |
