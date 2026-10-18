```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.21 (9.0.21, 9.0.2126.50404), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.32 (8.0.32, 8.0.3226.50405), X64 RyuJIT x86-64-v4


```
| Method                   | Job       | Runtime   | Mean        | Median      | Ratio      | Allocated | Alloc Ratio |
|------------------------- |---------- |---------- |------------:|------------:|-----------:|----------:|------------:|
| Success_String           | .NET 10.0 | .NET 10.0 |   0.0000 ns |   0.0000 ns |      0.000 |         - |          NA |
| Success_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |      0.000 |         - |          NA |
| Failure_NonGeneric       | .NET 9.0  | .NET 9.0  |   0.0000 ns |   0.0000 ns |      0.000 |         - |          NA |
| Failure_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0004 ns |   0.0002 ns |      0.019 |         - |          NA |
| Success_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0007 ns |   0.0006 ns |      0.035 |         - |          NA |
| Failure_NonGeneric       | .NET 10.0 | .NET 10.0 |   0.0016 ns |   0.0003 ns |      0.083 |         - |          NA |
| ImplicitConversion_Value | .NET 10.0 | .NET 10.0 |   0.0172 ns |   0.0170 ns |      0.882 |         - |          NA |
| Success_Int              | .NET 10.0 | .NET 10.0 |   0.0173 ns |   0.0172 ns |      0.889 |         - |          NA |
| Success_String           | .NET 8.0  | .NET 8.0  |   0.0176 ns |   0.0172 ns |      0.904 |         - |          NA |
| Success_String           | .NET 9.0  | .NET 9.0  |   0.0181 ns |   0.0181 ns |      0.930 |         - |          NA |
| Success_NonGeneric       | .NET 8.0  | .NET 8.0  |   0.0199 ns |   0.0185 ns |      1.022 |         - |          NA |
| ImplicitConversion_Error | .NET 10.0 | .NET 10.0 |   0.2571 ns |   0.2567 ns |     13.211 |         - |          NA |
| Failure_Int              | .NET 10.0 | .NET 10.0 |   0.2582 ns |   0.2562 ns |     13.269 |         - |          NA |
| Failure_Int              | .NET 8.0  | .NET 8.0  |   4.5655 ns |   4.5653 ns |    234.633 |         - |          NA |
| Success_Int              | .NET 8.0  | .NET 8.0  |   4.5662 ns |   4.5662 ns |    234.669 |         - |          NA |
| ImplicitConversion_Error | .NET 8.0  | .NET 8.0  |   4.5671 ns |   4.5672 ns |    234.716 |         - |          NA |
| ImplicitConversion_Value | .NET 8.0  | .NET 8.0  |   4.5677 ns |   4.5675 ns |    234.747 |         - |          NA |
| ImplicitConversion_Value | .NET 9.0  | .NET 9.0  |   4.5687 ns |   4.5664 ns |    234.798 |         - |          NA |
| Success_Int              | .NET 9.0  | .NET 9.0  |   4.5703 ns |   4.5666 ns |    234.883 |         - |          NA |
| Failure_Int              | .NET 9.0  | .NET 9.0  |   4.5832 ns |   4.5830 ns |    235.542 |         - |          NA |
| ImplicitConversion_Error | .NET 9.0  | .NET 9.0  |   4.5840 ns |   4.5837 ns |    235.583 |         - |          NA |
| Success_Guid             | .NET 10.0 | .NET 10.0 | 686.3901 ns | 686.3529 ns | 35,275.554 |         - |          NA |
| Success_Guid             | .NET 8.0  | .NET 8.0  | 695.8578 ns | 695.8470 ns | 35,762.125 |         - |          NA |
| Success_Guid             | .NET 9.0  | .NET 9.0  | 699.9368 ns | 699.9394 ns | 35,971.759 |         - |          NA |
