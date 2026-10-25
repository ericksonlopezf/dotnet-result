```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.21 (9.0.21, 9.0.2126.50404), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.32 (8.0.32, 8.0.3226.50405), X64 RyuJIT x86-64-v3

RatioSD=?  

```
| Method                   | Job       | Runtime   | Mean        | Median      | Ratio | Allocated | Alloc Ratio |
|------------------------- |---------- |---------- |------------:|------------:|------:|----------:|------------:|
| Success_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
| Failure_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
| Success_Int              | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
| ImplicitConversion_Value | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
| Failure_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
| Success_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0002 ns |   0.0000 ns |     ? |         - |           ? |
| Failure_Int              | .NET 10.0 | .NET 10.0 |   0.0006 ns |   0.0000 ns |     ? |         - |           ? |
| Success_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0006 ns |   0.0004 ns |     ? |         - |           ? |
| ImplicitConversion_Error | .NET 10.0 | .NET 10.0 |   0.0011 ns |   0.0005 ns |     ? |         - |           ? |
| Failure_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0204 ns |   0.0195 ns |     ? |         - |           ? |
| Success_String           | .NET 10.0 | .NET 10.0 |   0.0774 ns |   0.0769 ns |     ? |         - |           ? |
| Success_String           | .NET 8.0  | .NET 8.0  |   0.0958 ns |   0.0957 ns |     ? |         - |           ? |
| Success_String           | .NET 9.0  | .NET 9.0  |   0.3396 ns |   0.3394 ns |     ? |         - |           ? |
| ImplicitConversion_Value | .NET 9.0  | .NET 9.0  |   7.0925 ns |   7.0923 ns |     ? |         - |           ? |
| Failure_Int              | .NET 9.0  | .NET 9.0  |   7.0931 ns |   7.0928 ns |     ? |         - |           ? |
| ImplicitConversion_Error | .NET 9.0  | .NET 9.0  |   7.0940 ns |   7.0938 ns |     ? |         - |           ? |
| Success_Int              | .NET 9.0  | .NET 9.0  |   7.0949 ns |   7.0949 ns |     ? |         - |           ? |
| Success_Int              | .NET 8.0  | .NET 8.0  |   7.1140 ns |   7.1148 ns |     ? |         - |           ? |
| ImplicitConversion_Error | .NET 8.0  | .NET 8.0  |   7.1147 ns |   7.1147 ns |     ? |         - |           ? |
| ImplicitConversion_Value | .NET 8.0  | .NET 8.0  |   7.1148 ns |   7.1148 ns |     ? |         - |           ? |
| Failure_Int              | .NET 8.0  | .NET 8.0  |   7.1150 ns |   7.1150 ns |     ? |         - |           ? |
| Success_Guid             | .NET 10.0 | .NET 10.0 | 663.0213 ns | 661.9642 ns |     ? |         - |           ? |
| Success_Guid             | .NET 8.0  | .NET 8.0  | 675.1212 ns | 675.0313 ns |     ? |         - |           ? |
| Success_Guid             | .NET 9.0  | .NET 9.0  | 685.6176 ns | 685.4436 ns |     ? |         - |           ? |
