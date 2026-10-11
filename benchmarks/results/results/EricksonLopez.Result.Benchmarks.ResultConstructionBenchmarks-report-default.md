
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.37GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4

RatioSD=?  

 Method                   | Job       | Runtime   | Mean        | Median      | Ratio | Allocated | Alloc Ratio |
------------------------- |---------- |---------- |------------:|------------:|------:|----------:|------------:|
 Success_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 Success_Int              | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 Success_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 Success_String           | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 Failure_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0011 ns |   0.0000 ns |     ? |         - |           ? |
 Failure_Int              | .NET 10.0 | .NET 10.0 |   0.0014 ns |   0.0000 ns |     ? |         - |           ? |
 Failure_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0036 ns |   0.0014 ns |     ? |         - |           ? |
 Success_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0056 ns |   0.0011 ns |     ? |         - |           ? |
 ImplicitConversion_Error | .NET 10.0 | .NET 10.0 |   0.0060 ns |   0.0010 ns |     ? |         - |           ? |
 ImplicitConversion_Value | .NET 10.0 | .NET 10.0 |   0.0068 ns |   0.0000 ns |     ? |         - |           ? |
 Success_String           | .NET 8.0  | .NET 8.0  |   0.0140 ns |   0.0142 ns |     ? |         - |           ? |
 Success_String           | .NET 10.0 | .NET 10.0 |   0.0143 ns |   0.0031 ns |     ? |         - |           ? |
 Failure_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0186 ns |   0.0210 ns |     ? |         - |           ? |
 ImplicitConversion_Value | .NET 8.0  | .NET 8.0  |   2.6387 ns |   2.6385 ns |     ? |         - |           ? |
 Success_Int              | .NET 8.0  | .NET 8.0  |   2.6667 ns |   2.6652 ns |     ? |         - |           ? |
 Failure_Int              | .NET 8.0  | .NET 8.0  |   2.7375 ns |   2.7303 ns |     ? |         - |           ? |
 ImplicitConversion_Error | .NET 8.0  | .NET 8.0  |   2.7629 ns |   2.7861 ns |     ? |         - |           ? |
 Success_Int              | .NET 9.0  | .NET 9.0  |   2.7944 ns |   2.7958 ns |     ? |         - |           ? |
 Failure_Int              | .NET 9.0  | .NET 9.0  |   2.7982 ns |   2.8045 ns |     ? |         - |           ? |
 ImplicitConversion_Error | .NET 9.0  | .NET 9.0  |   2.8087 ns |   2.8088 ns |     ? |         - |           ? |
 ImplicitConversion_Value | .NET 9.0  | .NET 9.0  |   2.8399 ns |   2.8413 ns |     ? |         - |           ? |
 Success_Guid             | .NET 10.0 | .NET 10.0 | 448.2443 ns | 448.9402 ns |     ? |         - |           ? |
 Success_Guid             | .NET 8.0  | .NET 8.0  | 472.9151 ns | 473.9795 ns |     ? |         - |           ? |
 Success_Guid             | .NET 9.0  | .NET 9.0  | 480.1641 ns | 479.3136 ns |     ? |         - |           ? |
