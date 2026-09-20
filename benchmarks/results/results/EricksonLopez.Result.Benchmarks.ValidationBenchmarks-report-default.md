
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3


 Method                       | Job       | Runtime   | FieldCount | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
----------------------------- |---------- |---------- |----------- |----------:|------:|-------:|----------:|------------:|
 ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 4          |  10.10 ns |  0.72 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 4          |  12.33 ns |  0.88 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 4          |  14.05 ns |  1.00 |      - |         - |          NA |
 ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 4          |  69.05 ns |  4.91 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 4          |  79.37 ns |  5.65 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 4          |  97.54 ns |  6.94 | 0.0086 |     144 B |          NA |
 ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 4          | 284.15 ns | 20.22 | 0.0329 |     552 B |          NA |
 ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 4          | 308.00 ns | 21.91 | 0.0329 |     552 B |          NA |
 ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 4          | 365.67 ns | 26.02 | 0.0329 |     552 B |          NA |
                              |           |           |            |           |       |        |           |             |
 ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 10         |  15.50 ns |  0.60 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 10         |  24.74 ns |  0.96 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 10         |  25.69 ns |  1.00 |      - |         - |          NA |
 ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 10         |  80.29 ns |  3.13 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 10         |  91.62 ns |  3.57 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 10         | 113.70 ns |  4.43 | 0.0086 |     144 B |          NA |
 ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 10         | 492.11 ns | 19.16 | 0.0610 |    1032 B |          NA |
 ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 10         | 504.36 ns | 19.64 | 0.0610 |    1032 B |          NA |
 ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 10         | 635.36 ns | 24.74 | 0.0610 |    1032 B |          NA |
