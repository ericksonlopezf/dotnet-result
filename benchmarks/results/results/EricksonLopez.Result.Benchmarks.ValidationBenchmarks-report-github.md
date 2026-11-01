```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.21GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4


```
| Method                       | Job       | Runtime   | FieldCount | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |---------- |---------- |----------- |-----------:|------:|-------:|----------:|------------:|
| ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 4          |   5.074 ns |  0.66 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 4          |   5.422 ns |  0.70 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 4          |   7.722 ns |  1.00 |      - |         - |          NA |
| ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 4          |  36.198 ns |  4.69 | 0.0086 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 4          |  37.456 ns |  4.85 | 0.0086 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 4          |  50.138 ns |  6.50 | 0.0086 |     144 B |          NA |
| ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 4          | 161.748 ns | 20.96 | 0.0329 |     552 B |          NA |
| ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 4          | 162.900 ns | 21.11 | 0.0329 |     552 B |          NA |
| ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 4          | 204.615 ns | 26.51 | 0.0329 |     552 B |          NA |
|                              |           |           |            |            |       |        |           |             |
| ValidateAll_AllSuccess       | .NET 9.0  | .NET 9.0  | 10         |   7.920 ns |  0.76 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 10.0 | .NET 10.0 | 10         |   8.943 ns |  0.86 |      - |         - |          NA |
| ValidateAll_AllSuccess       | .NET 8.0  | .NET 8.0  | 10         |  10.462 ns |  1.00 |      - |         - |          NA |
| ValidateAll_OneFailure       | .NET 9.0  | .NET 9.0  | 10         |  43.098 ns |  4.12 | 0.0086 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 10.0 | .NET 10.0 | 10         |  43.413 ns |  4.15 | 0.0086 |     144 B |          NA |
| ValidateAll_OneFailure       | .NET 8.0  | .NET 8.0  | 10         |  57.550 ns |  5.50 | 0.0086 |     144 B |          NA |
| ValidateAll_MultipleFailures | .NET 10.0 | .NET 10.0 | 10         | 283.418 ns | 27.11 | 0.0615 |    1032 B |          NA |
| ValidateAll_MultipleFailures | .NET 9.0  | .NET 9.0  | 10         | 292.689 ns | 27.99 | 0.0615 |    1032 B |          NA |
| ValidateAll_MultipleFailures | .NET 8.0  | .NET 8.0  | 10         | 346.546 ns | 33.14 | 0.0615 |    1032 B |          NA |
