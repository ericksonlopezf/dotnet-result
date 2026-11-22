
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.68GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v4


 Method                       | Job       | Runtime   | FieldCount | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
----------------------------- |---------- |---------- |----------- |-----------:|------:|-------:|----------:|------------:|
 ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 4          |   7.094 ns |  0.62 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 4          |   9.972 ns |  0.87 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 4          |  11.491 ns |  1.00 |      - |         - |          NA |
 ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 4          |  52.987 ns |  4.61 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 4          |  61.346 ns |  5.34 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 4          |  85.566 ns |  7.45 | 0.0086 |     144 B |          NA |
 ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 4          | 220.009 ns | 19.15 | 0.0329 |     552 B |          NA |
 ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 4          | 246.162 ns | 21.42 | 0.0329 |     552 B |          NA |
 ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 4          | 318.101 ns | 27.68 | 0.0329 |     552 B |          NA |
                              |           |           |            |            |       |        |           |             |
 ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 10         |  11.027 ns |  0.74 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 10         |  14.949 ns |  1.00 |      - |         - |          NA |
 ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 10         |  15.614 ns |  1.04 |      - |         - |          NA |
 ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 10         |  69.050 ns |  4.62 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 10         |  94.249 ns |  6.30 | 0.0086 |     144 B |          NA |
 ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 10         | 140.307 ns |  9.39 | 0.0086 |     144 B |          NA |
 ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 10         | 390.451 ns | 26.12 | 0.0615 |    1032 B |          NA |
 ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 10         | 416.547 ns | 27.86 | 0.0615 |    1032 B |          NA |
 ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 10         | 564.969 ns | 37.79 | 0.0610 |    1032 B |          NA |
