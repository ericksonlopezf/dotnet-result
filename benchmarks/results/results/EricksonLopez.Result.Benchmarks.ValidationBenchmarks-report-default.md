
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.37GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4


 Method                       | Job       | Runtime   | FieldCount | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
----------------------------- |---------- |---------- |----------- |-----------:|------:|-------:|----------:|------------:|
 ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 4          |   5.187 ns |  0.68 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 4          |   5.274 ns |  0.69 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 4          |   7.640 ns |  1.00 |      - |         - |          NA |
 ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 4          |  35.124 ns |  4.60 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 4          |  35.635 ns |  4.67 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 4          |  47.923 ns |  6.28 | 0.0086 |     144 B |          NA |
 ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 4          | 141.705 ns | 18.57 | 0.0329 |     552 B |          NA |
 ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 4          | 151.983 ns | 19.91 | 0.0329 |     552 B |          NA |
 ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 4          | 228.394 ns | 29.93 | 0.0329 |     552 B |          NA |
                              |           |           |            |            |       |        |           |             |
 ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 10         |   7.360 ns |  0.72 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 10         |   8.887 ns |  0.87 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 10         |  10.243 ns |  1.00 |      - |         - |          NA |
 ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 10         |  38.529 ns |  3.76 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 10         |  43.898 ns |  4.29 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 10         |  54.812 ns |  5.35 | 0.0086 |     144 B |          NA |
 ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 10         | 272.113 ns | 26.58 | 0.0615 |    1032 B |          NA |
 ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 10         | 273.008 ns | 26.67 | 0.0615 |    1032 B |          NA |
 ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 10         | 321.387 ns | 31.39 | 0.0615 |    1032 B |          NA |
