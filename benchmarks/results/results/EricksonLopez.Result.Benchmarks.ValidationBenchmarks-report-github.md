```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
INTEL XEON PLATINUM 8573C 2.30GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4


```
| Method                       | Job       | Runtime   | FieldCount | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------- |---------- |----------- |-----------:|------:|-------:|----------:|------------:|
| ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 4          |   8.680 ns |  0.58 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 4          |   9.221 ns |  0.61 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 4          |  15.045 ns |  1.00 |      - |         - |          NA |
| ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 4          |  56.126 ns |  3.73 | 0.0017 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 4          |  70.832 ns |  4.71 | 0.0017 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 4          |  85.781 ns |  5.70 | 0.0017 |     144 B |          NA |
| ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 4          | 390.763 ns | 25.98 | 0.0062 |     552 B |          NA |
| ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 4          | 428.994 ns | 28.52 | 0.0062 |     552 B |          NA |
| ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 4          | 493.573 ns | 32.81 | 0.0057 |     552 B |          NA |
|                              |           |           |            |            |       |        |           |             |
| ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 10         |  12.160 ns |  0.63 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 10         |  13.771 ns |  0.71 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 10         |  19.280 ns |  1.00 |      - |         - |          NA |
| ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 10         |  66.492 ns |  3.45 | 0.0017 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 10         |  93.538 ns |  4.85 | 0.0017 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 10         | 125.747 ns |  6.52 | 0.0017 |     144 B |          NA |
| ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 10         | 572.998 ns | 29.72 | 0.0114 |    1032 B |          NA |
| ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 10         | 593.965 ns | 30.81 | 0.0114 |    1032 B |          NA |
| ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 10         | 695.785 ns | 36.09 | 0.0114 |    1032 B |          NA |
