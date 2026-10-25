
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.32 (8.0.32, 8.0.3226.50405), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.21 (9.0.21, 9.0.2126.50404), X64 RyuJIT x86-64-v3


 Method                  | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
------------------------ |---------- |---------- |----------:|------:|-------:|----------:|------------:|
 Builder_Simple          | .NET 10.0 | .NET 10.0 |  23.18 ns |  0.86 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 10.0 | .NET 10.0 |  24.25 ns |  0.90 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 10.0 | .NET 10.0 |  24.43 ns |  0.91 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 8.0  | .NET 8.0  |  26.94 ns |  1.00 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 8.0  | .NET 8.0  |  27.02 ns |  1.00 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 9.0  | .NET 9.0  |  27.60 ns |  1.02 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 9.0  | .NET 9.0  |  27.67 ns |  1.03 | 0.0062 |     104 B |        1.00 |
 Builder_Simple          | .NET 9.0  | .NET 9.0  |  37.49 ns |  1.39 | 0.0062 |     104 B |        1.00 |
 Builder_Simple          | .NET 8.0  | .NET 8.0  |  39.96 ns |  1.48 | 0.0062 |     104 B |        1.00 |
 Error_Equality          | .NET 10.0 | .NET 10.0 |  46.68 ns |  1.73 | 0.0124 |     208 B |        2.00 |
 Error_GetHashCode       | .NET 10.0 | .NET 10.0 |  48.53 ns |  1.80 | 0.0062 |     104 B |        1.00 |
 Error_GetHashCode       | .NET 9.0  | .NET 9.0  |  49.14 ns |  1.82 | 0.0062 |     104 B |        1.00 |
 Error_GetHashCode       | .NET 8.0  | .NET 8.0  |  49.26 ns |  1.83 | 0.0062 |     104 B |        1.00 |
 Error_Equality          | .NET 8.0  | .NET 8.0  |  52.12 ns |  1.93 | 0.0124 |     208 B |        2.00 |
 Error_Equality          | .NET 9.0  | .NET 9.0  |  52.38 ns |  1.94 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_5         | .NET 10.0 | .NET 10.0 |  66.61 ns |  2.47 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_5         | .NET 9.0  | .NET 9.0  | 115.59 ns |  4.29 | 0.0124 |     208 B |        2.00 |
 Builder_Full            | .NET 10.0 | .NET 10.0 | 140.52 ns |  5.22 | 0.0224 |     376 B |        3.62 |
 Builder_Chain_5         | .NET 8.0  | .NET 8.0  | 140.60 ns |  5.22 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_7         | .NET 10.0 | .NET 10.0 | 144.95 ns |  5.38 | 0.0224 |     376 B |        3.62 |
 Builder_Chain_7         | .NET 9.0  | .NET 9.0  | 218.01 ns |  8.09 | 0.0224 |     376 B |        3.62 |
 Builder_Full            | .NET 9.0  | .NET 9.0  | 223.74 ns |  8.30 | 0.0224 |     376 B |        3.62 |
 Builder_WithMetadata_3  | .NET 10.0 | .NET 10.0 | 271.59 ns | 10.08 | 0.0324 |     544 B |        5.23 |
 Builder_Chain_7         | .NET 8.0  | .NET 8.0  | 278.59 ns | 10.34 | 0.0224 |     376 B |        3.62 |
 Builder_Full            | .NET 8.0  | .NET 8.0  | 279.83 ns | 10.39 | 0.0224 |     376 B |        3.62 |
 WithMetadata_Chain_3    | .NET 10.0 | .NET 10.0 | 303.77 ns | 11.28 | 0.0482 |     808 B |        7.77 |
 Builder_WithMetadata_3  | .NET 8.0  | .NET 8.0  | 390.22 ns | 14.48 | 0.0324 |     544 B |        5.23 |
 Builder_WithMetadata_3  | .NET 9.0  | .NET 9.0  | 393.90 ns | 14.62 | 0.0362 |     608 B |        5.85 |
 Builder_BatchMetadata_5 | .NET 10.0 | .NET 10.0 | 395.44 ns | 14.68 | 0.0310 |     520 B |        5.00 |
 Builder_BatchMetadata_5 | .NET 9.0  | .NET 9.0  | 444.20 ns | 16.49 | 0.0310 |     520 B |        5.00 |
 WithMetadata_Chain_3    | .NET 9.0  | .NET 9.0  | 447.20 ns | 16.60 | 0.0548 |     920 B |        8.85 |
 WithMetadata_Chain_3    | .NET 8.0  | .NET 8.0  | 466.21 ns | 17.30 | 0.0648 |    1088 B |       10.46 |
 Builder_Chain_10        | .NET 10.0 | .NET 10.0 | 541.42 ns | 20.10 | 0.0601 |    1008 B |        9.69 |
 Builder_BatchMetadata_5 | .NET 8.0  | .NET 8.0  | 591.55 ns | 21.96 | 0.0305 |     520 B |        5.00 |
 Builder_Chain_10        | .NET 9.0  | .NET 9.0  | 630.96 ns | 23.42 | 0.0601 |    1008 B |        9.69 |
 Builder_Chain_10        | .NET 8.0  | .NET 8.0  | 818.43 ns | 30.38 | 0.0601 |    1008 B |        9.69 |
