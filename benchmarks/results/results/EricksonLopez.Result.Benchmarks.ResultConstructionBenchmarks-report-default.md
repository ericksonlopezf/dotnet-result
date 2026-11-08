
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.63GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4


 Method                   | Job       | Runtime   | Mean        | Median      | Ratio      | Allocated | Alloc Ratio |
------------------------- |---------- |---------- |------------:|------------:|-----------:|----------:|------------:|
 Success_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |      0.000 |         - |          NA |
 Failure_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |      0.000 |         - |          NA |
 Success_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |      0.001 |         - |          NA |
 Success_String           | .NET 10.0 | .NET 10.0 |   0.0016 ns |   0.0000 ns |      0.090 |         - |          NA |
 Failure_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0030 ns |   0.0010 ns |      0.162 |         - |          NA |
 Failure_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0035 ns |   0.0012 ns |      0.191 |         - |          NA |
 Success_Int              | .NET 10.0 | .NET 10.0 |   0.0166 ns |   0.0164 ns |      0.906 |         - |          NA |
 ImplicitConversion_Value | .NET 10.0 | .NET 10.0 |   0.0173 ns |   0.0172 ns |      0.944 |         - |          NA |
 Success_String           | .NET 9.0  | .NET 9.0  |   0.0174 ns |   0.0175 ns |      0.953 |         - |          NA |
 Success_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0185 ns |   0.0177 ns |      1.012 |         - |          NA |
 Success_String           | .NET 8.0  | .NET 8.0  |   0.0198 ns |   0.0188 ns |      1.081 |         - |          NA |
 ImplicitConversion_Error | .NET 10.0 | .NET 10.0 |   0.2574 ns |   0.2571 ns |     14.089 |         - |          NA |
 Failure_Int              | .NET 10.0 | .NET 10.0 |   0.2594 ns |   0.2599 ns |     14.199 |         - |          NA |
 Success_Int              | .NET 9.0  | .NET 9.0  |   4.5683 ns |   4.5680 ns |    250.040 |         - |          NA |
 ImplicitConversion_Value | .NET 8.0  | .NET 8.0  |   4.5689 ns |   4.5689 ns |    250.072 |         - |          NA |
 Success_Int              | .NET 8.0  | .NET 8.0  |   4.5701 ns |   4.5695 ns |    250.138 |         - |          NA |
 ImplicitConversion_Error | .NET 8.0  | .NET 8.0  |   4.5703 ns |   4.5703 ns |    250.149 |         - |          NA |
 Failure_Int              | .NET 8.0  | .NET 8.0  |   4.5705 ns |   4.5704 ns |    250.160 |         - |          NA |
 ImplicitConversion_Value | .NET 9.0  | .NET 9.0  |   4.5774 ns |   4.5723 ns |    250.534 |         - |          NA |
 ImplicitConversion_Error | .NET 9.0  | .NET 9.0  |   4.5857 ns |   4.5855 ns |    250.992 |         - |          NA |
 Failure_Int              | .NET 9.0  | .NET 9.0  |   4.5881 ns |   4.5863 ns |    251.122 |         - |          NA |
 Success_Guid             | .NET 10.0 | .NET 10.0 | 718.8694 ns | 718.4233 ns | 39,346.027 |         - |          NA |
 Success_Guid             | .NET 9.0  | .NET 9.0  | 824.2439 ns | 824.7517 ns | 45,113.514 |         - |          NA |
 Success_Guid             | .NET 8.0  | .NET 8.0  | 826.8093 ns | 826.7520 ns | 45,253.925 |         - |          NA |
