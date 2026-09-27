
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
INTEL XEON PLATINUM 8573C 2.30GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4


 Method                  | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
------------------------ |---------- |---------- |----------:|------:|-------:|----------:|------------:|
 Builder_Simple          | .NET 10.0 | .NET 10.0 |  19.78 ns |  0.71 | 0.0012 |     104 B |        1.00 |
 Factory_Failure         | .NET 10.0 | .NET 10.0 |  19.94 ns |  0.72 | 0.0012 |     104 B |        1.00 |
 Factory_Validation      | .NET 10.0 | .NET 10.0 |  21.35 ns |  0.77 | 0.0012 |     104 B |        1.00 |
 Factory_Validation      | .NET 9.0  | .NET 9.0  |  26.53 ns |  0.96 | 0.0012 |     104 B |        1.00 |
 Factory_Validation      | .NET 8.0  | .NET 8.0  |  27.31 ns |  0.99 | 0.0012 |     104 B |        1.00 |
 Factory_Failure         | .NET 8.0  | .NET 8.0  |  27.69 ns |  1.00 | 0.0012 |     104 B |        1.00 |
 Factory_Failure         | .NET 9.0  | .NET 9.0  |  27.98 ns |  1.01 | 0.0012 |     104 B |        1.00 |
 Error_GetHashCode       | .NET 10.0 | .NET 10.0 |  36.04 ns |  1.30 | 0.0012 |     104 B |        1.00 |
 Error_GetHashCode       | .NET 8.0  | .NET 8.0  |  37.13 ns |  1.34 | 0.0012 |     104 B |        1.00 |
 Builder_Simple          | .NET 9.0  | .NET 9.0  |  37.40 ns |  1.35 | 0.0012 |     104 B |        1.00 |
 Builder_Simple          | .NET 8.0  | .NET 8.0  |  37.81 ns |  1.37 | 0.0012 |     104 B |        1.00 |
 Error_GetHashCode       | .NET 9.0  | .NET 9.0  |  37.81 ns |  1.37 | 0.0012 |     104 B |        1.00 |
 Error_Equality          | .NET 10.0 | .NET 10.0 |  40.01 ns |  1.45 | 0.0024 |     208 B |        2.00 |
 Error_Equality          | .NET 9.0  | .NET 9.0  |  51.67 ns |  1.87 | 0.0024 |     208 B |        2.00 |
 Builder_Chain_5         | .NET 10.0 | .NET 10.0 |  53.28 ns |  1.92 | 0.0024 |     208 B |        2.00 |
 Error_Equality          | .NET 8.0  | .NET 8.0  |  53.76 ns |  1.94 | 0.0024 |     208 B |        2.00 |
 Builder_Chain_5         | .NET 9.0  | .NET 9.0  |  93.50 ns |  3.38 | 0.0024 |     208 B |        2.00 |
 Builder_Chain_5         | .NET 8.0  | .NET 8.0  | 119.11 ns |  4.30 | 0.0024 |     208 B |        2.00 |
 Builder_Full            | .NET 10.0 | .NET 10.0 | 124.60 ns |  4.50 | 0.0043 |     376 B |        3.62 |
 Builder_Chain_7         | .NET 10.0 | .NET 10.0 | 125.68 ns |  4.54 | 0.0043 |     376 B |        3.62 |
 Builder_Chain_7         | .NET 9.0  | .NET 9.0  | 190.95 ns |  6.90 | 0.0043 |     376 B |        3.62 |
 Builder_Full            | .NET 9.0  | .NET 9.0  | 198.65 ns |  7.17 | 0.0043 |     376 B |        3.62 |
 Builder_Full            | .NET 8.0  | .NET 8.0  | 220.19 ns |  7.95 | 0.0043 |     376 B |        3.62 |
 Builder_Chain_7         | .NET 8.0  | .NET 8.0  | 221.09 ns |  7.98 | 0.0043 |     376 B |        3.62 |
 Builder_WithMetadata_3  | .NET 10.0 | .NET 10.0 | 270.83 ns |  9.78 | 0.0062 |     544 B |        5.23 |
 Builder_BatchMetadata_5 | .NET 10.0 | .NET 10.0 | 275.77 ns |  9.96 | 0.0062 |     520 B |        5.00 |
 WithMetadata_Chain_3    | .NET 10.0 | .NET 10.0 | 276.46 ns |  9.98 | 0.0095 |     808 B |        7.77 |
 Builder_WithMetadata_3  | .NET 9.0  | .NET 9.0  | 320.96 ns | 11.59 | 0.0072 |     608 B |        5.85 |
 Builder_WithMetadata_3  | .NET 8.0  | .NET 8.0  | 352.51 ns | 12.73 | 0.0072 |     608 B |        5.85 |
 WithMetadata_Chain_3    | .NET 9.0  | .NET 9.0  | 356.32 ns | 12.87 | 0.0100 |     856 B |        8.23 |
 WithMetadata_Chain_3    | .NET 8.0  | .NET 8.0  | 359.73 ns | 12.99 | 0.0119 |    1024 B |        9.85 |
 Builder_BatchMetadata_5 | .NET 9.0  | .NET 9.0  | 402.59 ns | 14.54 | 0.0062 |     520 B |        5.00 |
 Builder_BatchMetadata_5 | .NET 8.0  | .NET 8.0  | 433.44 ns | 15.65 | 0.0062 |     520 B |        5.00 |
 Builder_Chain_10        | .NET 10.0 | .NET 10.0 | 556.00 ns | 20.08 | 0.0134 |    1136 B |       10.92 |
 Builder_Chain_10        | .NET 9.0  | .NET 9.0  | 605.88 ns | 21.88 | 0.0134 |    1136 B |       10.92 |
 Builder_Chain_10        | .NET 8.0  | .NET 8.0  | 634.61 ns | 22.92 | 0.0114 |    1008 B |        9.69 |
