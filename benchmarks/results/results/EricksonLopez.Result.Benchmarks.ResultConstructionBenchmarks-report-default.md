
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3

RatioSD=?  

 Method                   | Job       | Runtime   | Mean        | Median      | Ratio | Allocated | Alloc Ratio |
------------------------- |---------- |---------- |------------:|------------:|------:|----------:|------------:|
 Success_Int              | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 ImplicitConversion_Value | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 Success_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 Failure_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 Success_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 Success_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0002 ns |   0.0002 ns |     ? |         - |           ? |
 Failure_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0013 ns |   0.0003 ns |     ? |         - |           ? |
 Failure_Int              | .NET 10.0 | .NET 10.0 |   0.0227 ns |   0.0223 ns |     ? |         - |           ? |
 ImplicitConversion_Error | .NET 10.0 | .NET 10.0 |   0.0229 ns |   0.0227 ns |     ? |         - |           ? |
 Failure_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0230 ns |   0.0226 ns |     ? |         - |           ? |
 Success_String           | .NET 8.0  | .NET 8.0  |   0.0230 ns |   0.0226 ns |     ? |         - |           ? |
 Success_String           | .NET 10.0 | .NET 10.0 |   0.0437 ns |   0.0438 ns |     ? |         - |           ? |
 Success_String           | .NET 9.0  | .NET 9.0  |   0.3750 ns |   0.3746 ns |     ? |         - |           ? |
 ImplicitConversion_Value | .NET 9.0  | .NET 9.0  |   5.9078 ns |   5.9075 ns |     ? |         - |           ? |
 ImplicitConversion_Error | .NET 8.0  | .NET 8.0  |   5.9085 ns |   5.9080 ns |     ? |         - |           ? |
 Success_Int              | .NET 9.0  | .NET 9.0  |   5.9086 ns |   5.9090 ns |     ? |         - |           ? |
 ImplicitConversion_Value | .NET 8.0  | .NET 8.0  |   5.9090 ns |   5.9091 ns |     ? |         - |           ? |
 Success_Int              | .NET 8.0  | .NET 8.0  |   5.9094 ns |   5.9076 ns |     ? |         - |           ? |
 Failure_Int              | .NET 9.0  | .NET 9.0  |   5.9095 ns |   5.9095 ns |     ? |         - |           ? |
 Failure_Int              | .NET 8.0  | .NET 8.0  |   5.9100 ns |   5.9100 ns |     ? |         - |           ? |
 ImplicitConversion_Error | .NET 9.0  | .NET 9.0  |   5.9103 ns |   5.9102 ns |     ? |         - |           ? |
 Success_Guid             | .NET 10.0 | .NET 10.0 | 882.1096 ns | 882.3513 ns |     ? |         - |           ? |
 Success_Guid             | .NET 9.0  | .NET 9.0  | 897.5663 ns | 897.5470 ns |     ? |         - |           ? |
 Success_Guid             | .NET 8.0  | .NET 8.0  | 897.7213 ns | 897.6317 ns |     ? |         - |           ? |
