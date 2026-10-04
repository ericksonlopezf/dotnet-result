
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3


 Method                       | Job       | Runtime   | FieldCount | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
----------------------------- |---------- |---------- |----------- |-----------:|------:|-------:|----------:|------------:|
 ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 4          |   9.120 ns |  0.64 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 4          |  13.140 ns |  0.92 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 4          |  14.243 ns |  1.00 |      - |         - |          NA |
 ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 4          |  74.656 ns |  5.24 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 4          | 102.220 ns |  7.18 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 4          | 104.424 ns |  7.33 | 0.0086 |     144 B |          NA |
 ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 4          | 296.506 ns | 20.82 | 0.0329 |     552 B |          NA |
 ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 4          | 319.866 ns | 22.46 | 0.0329 |     552 B |          NA |
 ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 4          | 389.247 ns | 27.33 | 0.0329 |     552 B |          NA |
                              |           |           |            |            |       |        |           |             |
 ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 10         |  14.115 ns |  0.73 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 10         |  19.218 ns |  1.00 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 10         |  20.535 ns |  1.07 |      - |         - |          NA |
 ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 10         |  81.825 ns |  4.26 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 10         |  89.988 ns |  4.68 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 10         | 133.198 ns |  6.93 | 0.0086 |     144 B |          NA |
 ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 10         | 504.634 ns | 26.26 | 0.0610 |    1032 B |          NA |
 ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 10         | 505.203 ns | 26.29 | 0.0610 |    1032 B |          NA |
 ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 10         | 687.773 ns | 35.79 | 0.0610 |    1032 B |          NA |
