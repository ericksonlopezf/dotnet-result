```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.21 (9.0.21, 9.0.2126.50404), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.32 (8.0.32, 8.0.3226.50405), X64 RyuJIT x86-64-v3


```
| Method                       | Job       | Runtime   | FieldCount | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------- |---------- |----------- |-----------:|------:|-------:|----------:|------------:|
| ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 4          |   9.665 ns |  0.64 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 4          |  12.424 ns |  0.82 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 4          |  15.164 ns |  1.00 |      - |         - |          NA |
| ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 4          |  70.428 ns |  4.64 | 0.0086 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 4          |  74.485 ns |  4.91 | 0.0086 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 4          |  95.049 ns |  6.27 | 0.0086 |     144 B |          NA |
| ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 4          | 302.620 ns | 19.96 | 0.0329 |     552 B |          NA |
| ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 4          | 363.980 ns | 24.00 | 0.0329 |     552 B |          NA |
| ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 4          | 398.959 ns | 26.31 | 0.0329 |     552 B |          NA |
|                              |           |           |            |            |       |        |           |             |
| ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 10         |  15.641 ns |  0.74 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 10         |  21.207 ns |  1.00 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 10         |  24.727 ns |  1.17 |      - |         - |          NA |
| ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 10         |  80.597 ns |  3.80 | 0.0086 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 10         |  91.788 ns |  4.33 | 0.0086 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 10         | 116.768 ns |  5.51 | 0.0086 |     144 B |          NA |
| ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 10         | 503.709 ns | 23.75 | 0.0610 |    1032 B |          NA |
| ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 10         | 525.586 ns | 24.78 | 0.0610 |    1032 B |          NA |
| ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 10         | 653.739 ns | 30.83 | 0.0610 |    1032 B |          NA |
