
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.68GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v4


 Method                       | Job       | Runtime   | FieldCount | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
----------------------------- |---------- |---------- |----------- |-----------:|------:|-------:|----------:|------------:|
 ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 4          |   7.073 ns |  0.62 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 4          |  10.770 ns |  0.94 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 4          |  11.428 ns |  1.00 |      - |         - |          NA |
 ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 4          |  51.852 ns |  4.54 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 4          |  58.533 ns |  5.12 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 4          |  82.420 ns |  7.21 | 0.0086 |     144 B |          NA |
 ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 4          | 211.856 ns | 18.54 | 0.0329 |     552 B |          NA |
 ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 4          | 292.459 ns | 25.59 | 0.0329 |     552 B |          NA |
 ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 4          | 309.571 ns | 27.09 | 0.0329 |     552 B |          NA |
                              |           |           |            |            |       |        |           |             |
 ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 10         |  10.951 ns |  0.73 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 10         |  14.935 ns |  1.00 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 10         |  15.615 ns |  1.05 |      - |         - |          NA |
 ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 10         |  57.057 ns |  3.82 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 10         |  66.657 ns |  4.46 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 10         |  92.852 ns |  6.22 | 0.0086 |     144 B |          NA |
 ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 10         | 386.903 ns | 25.91 | 0.0615 |    1032 B |          NA |
 ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 10         | 390.749 ns | 26.16 | 0.0615 |    1032 B |          NA |
 ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 10         | 525.017 ns | 35.15 | 0.0610 |    1032 B |          NA |
