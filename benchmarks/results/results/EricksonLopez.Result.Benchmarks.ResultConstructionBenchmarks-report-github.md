```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3


```
| Method                   | Job       | Runtime   | Mean        | Median      | Ratio       | Allocated | Alloc Ratio |
|------------------------- |---------- |---------- |------------:|------------:|------------:|----------:|------------:|
| Success_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |       0.000 |         - |          NA |
| Failure_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |       0.000 |         - |          NA |
| Success_Int              | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |       0.000 |         - |          NA |
| ImplicitConversion_Value | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |       0.000 |         - |          NA |
| Failure_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |       0.000 |         - |          NA |
| Success_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |       0.036 |         - |          NA |
| Failure_Int              | .NET 10.0 | .NET 10.0 |   0.0001 ns |   0.0000 ns |       0.083 |         - |          NA |
| ImplicitConversion_Error | .NET 10.0 | .NET 10.0 |   0.0002 ns |   0.0000 ns |       0.209 |         - |          NA |
| Success_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0022 ns |   0.0023 ns |       2.007 |         - |          NA |
| Failure_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0193 ns |   0.0185 ns |      17.509 |         - |          NA |
| Success_String           | .NET 10.0 | .NET 10.0 |   0.2335 ns |   0.2334 ns |     211.831 |         - |          NA |
| Success_String           | .NET 8.0  | .NET 8.0  |   0.2529 ns |   0.2530 ns |     229.448 |         - |          NA |
| Success_String           | .NET 9.0  | .NET 9.0  |   0.3297 ns |   0.3296 ns |     299.169 |         - |          NA |
| Success_Int              | .NET 9.0  | .NET 9.0  |   7.0918 ns |   7.0921 ns |   6,434.768 |         - |          NA |
| Failure_Int              | .NET 9.0  | .NET 9.0  |   7.0945 ns |   7.0943 ns |   6,437.217 |         - |          NA |
| ImplicitConversion_Error | .NET 9.0  | .NET 9.0  |   7.0971 ns |   7.0949 ns |   6,439.569 |         - |          NA |
| ImplicitConversion_Value | .NET 9.0  | .NET 9.0  |   7.0985 ns |   7.0990 ns |   6,440.862 |         - |          NA |
| ImplicitConversion_Error | .NET 8.0  | .NET 8.0  |   7.1121 ns |   7.1119 ns |   6,453.178 |         - |          NA |
| Failure_Int              | .NET 8.0  | .NET 8.0  |   7.1145 ns |   7.1151 ns |   6,455.439 |         - |          NA |
| ImplicitConversion_Value | .NET 8.0  | .NET 8.0  |   7.1146 ns |   7.1136 ns |   6,455.509 |         - |          NA |
| Success_Int              | .NET 8.0  | .NET 8.0  |   7.1151 ns |   7.1153 ns |   6,455.944 |         - |          NA |
| Success_Guid             | .NET 10.0 | .NET 10.0 | 657.6310 ns | 657.4087 ns | 596,706.898 |         - |          NA |
| Success_Guid             | .NET 9.0  | .NET 9.0  | 678.3419 ns | 678.2456 ns | 615,499.127 |         - |          NA |
| Success_Guid             | .NET 8.0  | .NET 8.0  | 680.3886 ns | 680.2256 ns | 617,356.190 |         - |          NA |
