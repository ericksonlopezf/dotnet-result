```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
INTEL XEON PLATINUM 8573C 2.30GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4


```
| Method                   | Job       | Runtime   | Mean        | Median      | Ratio      | Allocated | Alloc Ratio |
|------------------------- |---------- |---------- |------------:|------------:|-----------:|----------:|------------:|
| ImplicitConversion_Value | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |      0.000 |         - |          NA |
| Failure_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0000 ns |   0.0000 ns |      0.000 |         - |          NA |
| Success_String           | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |      0.000 |         - |          NA |
| Success_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |      0.003 |         - |          NA |
| Failure_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0008 ns |   0.0000 ns |      0.053 |         - |          NA |
| Failure_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0013 ns |   0.0005 ns |      0.087 |         - |          NA |
| Success_String           | .NET 10.0 | .NET 10.0 |   0.0031 ns |   0.0000 ns |      0.202 |         - |          NA |
| Success_Int              | .NET 10.0 | .NET 10.0 |   0.0087 ns |   0.0073 ns |      0.563 |         - |          NA |
| Success_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0148 ns |   0.0143 ns |      0.962 |         - |          NA |
| Success_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0173 ns |   0.0151 ns |      1.127 |         - |          NA |
| Success_String           | .NET 8.0  | .NET 8.0  |   0.0297 ns |   0.0322 ns |      1.936 |         - |          NA |
| Failure_Int              | .NET 10.0 | .NET 10.0 |   0.2835 ns |   0.2840 ns |     18.460 |         - |          NA |
| ImplicitConversion_Error | .NET 10.0 | .NET 10.0 |   0.2848 ns |   0.2852 ns |     18.548 |         - |          NA |
| ImplicitConversion_Error | .NET 9.0  | .NET 9.0  |   2.8050 ns |   2.8027 ns |    182.652 |         - |          NA |
| ImplicitConversion_Value | .NET 8.0  | .NET 8.0  |   2.8341 ns |   2.8326 ns |    184.542 |         - |          NA |
| Success_Int              | .NET 8.0  | .NET 8.0  |   2.8347 ns |   2.8263 ns |    184.581 |         - |          NA |
| Failure_Int              | .NET 8.0  | .NET 8.0  |   2.8406 ns |   2.8400 ns |    184.966 |         - |          NA |
| Failure_Int              | .NET 9.0  | .NET 9.0  |   2.8438 ns |   2.8447 ns |    185.174 |         - |          NA |
| Success_Int              | .NET 9.0  | .NET 9.0  |   2.8450 ns |   2.8427 ns |    185.252 |         - |          NA |
| ImplicitConversion_Value | .NET 9.0  | .NET 9.0  |   2.8460 ns |   2.8466 ns |    185.319 |         - |          NA |
| ImplicitConversion_Error | .NET 8.0  | .NET 8.0  |   2.8539 ns |   2.8537 ns |    185.836 |         - |          NA |
| Success_Guid             | .NET 9.0  | .NET 9.0  | 410.3037 ns | 410.3232 ns | 26,717.125 |         - |          NA |
| Success_Guid             | .NET 10.0 | .NET 10.0 | 412.3109 ns | 412.5617 ns | 26,847.822 |         - |          NA |
| Success_Guid             | .NET 8.0  | .NET 8.0  | 425.5370 ns | 425.7255 ns | 27,709.049 |         - |          NA |
