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
| Failure_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |      0.001 |         - |          NA |
| Success_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0002 ns |   0.0000 ns |      0.012 |         - |          NA |
| Failure_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0018 ns |   0.0000 ns |      0.107 |         - |          NA |
| Success_String           | .NET 10.0 | .NET 10.0 |   0.0067 ns |   0.0016 ns |      0.390 |         - |          NA |
| ImplicitConversion_Value | .NET 10.0 | .NET 10.0 |   0.0172 ns |   0.0171 ns |      0.999 |         - |          NA |
| Success_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0172 ns |   0.0170 ns |      1.002 |         - |          NA |
| Success_Int              | .NET 10.0 | .NET 10.0 |   0.0182 ns |   0.0168 ns |      1.058 |         - |          NA |
| Success_String           | .NET 8.0  | .NET 8.0  |   0.0188 ns |   0.0170 ns |      1.093 |         - |          NA |
| Success_String           | .NET 9.0  | .NET 9.0  |   0.0199 ns |   0.0182 ns |      1.157 |         - |          NA |
| Failure_Int              | .NET 10.0 | .NET 10.0 |   0.2548 ns |   0.2545 ns |     14.812 |         - |          NA |
| ImplicitConversion_Error | .NET 10.0 | .NET 10.0 |   0.2588 ns |   0.2575 ns |     15.045 |         - |          NA |
| Failure_Int              | .NET 8.0  | .NET 8.0  |   4.5669 ns |   4.5669 ns |    265.495 |         - |          NA |
| Success_Int              | .NET 8.0  | .NET 8.0  |   4.5689 ns |   4.5684 ns |    265.609 |         - |          NA |
| ImplicitConversion_Error | .NET 8.0  | .NET 8.0  |   4.5699 ns |   4.5687 ns |    265.669 |         - |          NA |
| Success_Int              | .NET 9.0  | .NET 9.0  |   4.5706 ns |   4.5695 ns |    265.709 |         - |          NA |
| ImplicitConversion_Value | .NET 8.0  | .NET 8.0  |   4.5707 ns |   4.5693 ns |    265.715 |         - |          NA |
| ImplicitConversion_Value | .NET 9.0  | .NET 9.0  |   4.5725 ns |   4.5693 ns |    265.820 |         - |          NA |
| Failure_Int              | .NET 9.0  | .NET 9.0  |   4.5846 ns |   4.5844 ns |    266.521 |         - |          NA |
| ImplicitConversion_Error | .NET 9.0  | .NET 9.0  |   4.5893 ns |   4.5887 ns |    266.797 |         - |          NA |
| Success_Guid             | .NET 10.0 | .NET 10.0 | 714.2286 ns | 714.1540 ns | 41,521.376 |         - |          NA |
| Success_Guid             | .NET 9.0  | .NET 9.0  | 815.0108 ns | 814.8895 ns | 47,380.307 |         - |          NA |
| Success_Guid             | .NET 8.0  | .NET 8.0  | 822.0615 ns | 821.3056 ns | 47,790.195 |         - |          NA |
