
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.21GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4

RatioSD=?  

 Method                   | Job       | Runtime   | Mean        | Median      | Ratio | Allocated | Alloc Ratio |
------------------------- |---------- |---------- |------------:|------------:|------:|----------:|------------:|
 Success_Int              | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 Success_String           | .NET 9.0  | .NET 9.0  |   0.0002 ns |   0.0000 ns |     ? |         - |           ? |
 Success_String           | .NET 10.0 | .NET 10.0 |   0.0010 ns |   0.0000 ns |     ? |         - |           ? |
 Failure_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0011 ns |   0.0000 ns |     ? |         - |           ? |
 Success_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0013 ns |   0.0000 ns |     ? |         - |           ? |
 ImplicitConversion_Value | .NET 10.0 | .NET 10.0 |   0.0027 ns |   0.0000 ns |     ? |         - |           ? |
 Success_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0053 ns |   0.0040 ns |     ? |         - |           ? |
 Success_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0082 ns |   0.0081 ns |     ? |         - |           ? |
 Failure_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0117 ns |   0.0133 ns |     ? |         - |           ? |
 Failure_Int              | .NET 10.0 | .NET 10.0 |   0.0148 ns |   0.0134 ns |     ? |         - |           ? |
 ImplicitConversion_Error | .NET 10.0 | .NET 10.0 |   0.0155 ns |   0.0119 ns |     ? |         - |           ? |
 Success_String           | .NET 8.0  | .NET 8.0  |   0.0204 ns |   0.0181 ns |     ? |         - |           ? |
 Failure_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0271 ns |   0.0360 ns |     ? |         - |           ? |
 Failure_Int              | .NET 9.0  | .NET 9.0  |   2.7245 ns |   2.7328 ns |     ? |         - |           ? |
 ImplicitConversion_Value | .NET 9.0  | .NET 9.0  |   2.7549 ns |   2.7576 ns |     ? |         - |           ? |
 Success_Int              | .NET 8.0  | .NET 8.0  |   2.8014 ns |   2.8022 ns |     ? |         - |           ? |
 ImplicitConversion_Error | .NET 9.0  | .NET 9.0  |   2.8026 ns |   2.8113 ns |     ? |         - |           ? |
 ImplicitConversion_Error | .NET 8.0  | .NET 8.0  |   2.8070 ns |   2.8105 ns |     ? |         - |           ? |
 Failure_Int              | .NET 8.0  | .NET 8.0  |   2.8146 ns |   2.8159 ns |     ? |         - |           ? |
 ImplicitConversion_Value | .NET 8.0  | .NET 8.0  |   2.8328 ns |   2.8274 ns |     ? |         - |           ? |
 Success_Int              | .NET 9.0  | .NET 9.0  |   2.8426 ns |   2.8473 ns |     ? |         - |           ? |
 Success_Guid             | .NET 10.0 | .NET 10.0 | 459.9695 ns | 460.1911 ns |     ? |         - |           ? |
 Success_Guid             | .NET 9.0  | .NET 9.0  | 487.3981 ns | 487.3341 ns |     ? |         - |           ? |
 Success_Guid             | .NET 8.0  | .NET 8.0  | 494.8176 ns | 497.5376 ns |     ? |         - |           ? |
