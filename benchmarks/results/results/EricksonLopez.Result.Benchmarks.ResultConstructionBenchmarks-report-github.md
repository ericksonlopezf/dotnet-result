```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.68GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v4


```
| Method                   | Job       | Runtime   | Mean        | Median      | Ratio      | Allocated | Alloc Ratio |
|------------------------- |---------- |---------- |------------:|------------:|-----------:|----------:|------------:|
| Success_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |      0.000 |         - |          NA |
| Failure_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |      0.000 |         - |          NA |
| Success_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |      0.002 |         - |          NA |
| Failure_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0009 ns |   0.0009 ns |      0.051 |         - |          NA |
| Failure_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0010 ns |   0.0008 ns |      0.060 |         - |          NA |
| Success_String           | .NET 10.0 | .NET 10.0 |   0.0014 ns |   0.0011 ns |      0.081 |         - |          NA |
| ImplicitConversion_Value | .NET 10.0 | .NET 10.0 |   0.0160 ns |   0.0155 ns |      0.918 |         - |          NA |
| Success_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0174 ns |   0.0172 ns |      1.004 |         - |          NA |
| Success_Int              | .NET 10.0 | .NET 10.0 |   0.0178 ns |   0.0177 ns |      1.024 |         - |          NA |
| Success_String           | .NET 9.0  | .NET 9.0  |   0.0183 ns |   0.0178 ns |      1.055 |         - |          NA |
| Success_String           | .NET 8.0  | .NET 8.0  |   0.0187 ns |   0.0181 ns |      1.076 |         - |          NA |
| Failure_Int              | .NET 10.0 | .NET 10.0 |   0.2563 ns |   0.2566 ns |     14.749 |         - |          NA |
| ImplicitConversion_Error | .NET 10.0 | .NET 10.0 |   0.2566 ns |   0.2561 ns |     14.765 |         - |          NA |
| ImplicitConversion_Value | .NET 8.0  | .NET 8.0  |   4.5665 ns |   4.5666 ns |    262.794 |         - |          NA |
| Success_Int              | .NET 8.0  | .NET 8.0  |   4.5674 ns |   4.5671 ns |    262.849 |         - |          NA |
| ImplicitConversion_Value | .NET 9.0  | .NET 9.0  |   4.5677 ns |   4.5678 ns |    262.868 |         - |          NA |
| ImplicitConversion_Error | .NET 8.0  | .NET 8.0  |   4.5678 ns |   4.5681 ns |    262.869 |         - |          NA |
| Failure_Int              | .NET 8.0  | .NET 8.0  |   4.5684 ns |   4.5684 ns |    262.906 |         - |          NA |
| Success_Int              | .NET 9.0  | .NET 9.0  |   4.5686 ns |   4.5685 ns |    262.918 |         - |          NA |
| ImplicitConversion_Error | .NET 9.0  | .NET 9.0  |   4.5843 ns |   4.5842 ns |    263.822 |         - |          NA |
| Failure_Int              | .NET 9.0  | .NET 9.0  |   4.5852 ns |   4.5853 ns |    263.872 |         - |          NA |
| Success_Guid             | .NET 10.0 | .NET 10.0 | 720.6429 ns | 720.6355 ns | 41,472.114 |         - |          NA |
| Success_Guid             | .NET 9.0  | .NET 9.0  | 824.8160 ns | 824.8028 ns | 47,467.150 |         - |          NA |
| Success_Guid             | .NET 8.0  | .NET 8.0  | 827.7177 ns | 827.6880 ns | 47,634.139 |         - |          NA |
