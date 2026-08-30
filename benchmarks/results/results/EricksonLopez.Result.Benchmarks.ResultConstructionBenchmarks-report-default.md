
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.4 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.400
  [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.19 (9.0.19, 9.0.1926.36724), X64 RyuJIT x86-64-v3

RatioSD=?  

 Method                   | Job       | Runtime   | Mean        | Median      | Ratio | Allocated | Alloc Ratio |
------------------------- |---------- |---------- |------------:|------------:|------:|----------:|------------:|
 Success_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 Failure_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 Success_Int              | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 ImplicitConversion_Value | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 Success_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 Failure_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 Failure_Int              | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |     ? |         - |           ? |
 ImplicitConversion_Error | .NET 10.0 | .NET 10.0 |   0.0001 ns |   0.0000 ns |     ? |         - |           ? |
 Success_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0002 ns |   0.0000 ns |     ? |         - |           ? |
 Failure_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0200 ns |   0.0193 ns |     ? |         - |           ? |
 Success_String           | .NET 8.0  | .NET 8.0  |   0.0892 ns |   0.0888 ns |     ? |         - |           ? |
 Success_String           | .NET 10.0 | .NET 10.0 |   0.2326 ns |   0.2325 ns |     ? |         - |           ? |
 Success_String           | .NET 9.0  | .NET 9.0  |   0.3372 ns |   0.3371 ns |     ? |         - |           ? |
 ImplicitConversion_Value | .NET 9.0  | .NET 9.0  |   7.0927 ns |   7.0926 ns |     ? |         - |           ? |
 Failure_Int              | .NET 9.0  | .NET 9.0  |   7.0936 ns |   7.0937 ns |     ? |         - |           ? |
 Success_Int              | .NET 9.0  | .NET 9.0  |   7.0943 ns |   7.0947 ns |     ? |         - |           ? |
 ImplicitConversion_Error | .NET 9.0  | .NET 9.0  |   7.0961 ns |   7.0957 ns |     ? |         - |           ? |
 ImplicitConversion_Error | .NET 8.0  | .NET 8.0  |   7.1132 ns |   7.1126 ns |     ? |         - |           ? |
 ImplicitConversion_Value | .NET 8.0  | .NET 8.0  |   7.1138 ns |   7.1139 ns |     ? |         - |           ? |
 Failure_Int              | .NET 8.0  | .NET 8.0  |   7.1154 ns |   7.1148 ns |     ? |         - |           ? |
 Success_Int              | .NET 8.0  | .NET 8.0  |   7.1158 ns |   7.1156 ns |     ? |         - |           ? |
 Success_Guid             | .NET 10.0 | .NET 10.0 | 658.4704 ns | 658.4838 ns |     ? |         - |           ? |
 Success_Guid             | .NET 9.0  | .NET 9.0  | 678.2779 ns | 678.0391 ns |     ? |         - |           ? |
 Success_Guid             | .NET 8.0  | .NET 8.0  | 679.7901 ns | 679.3055 ns |     ? |         - |           ? |
