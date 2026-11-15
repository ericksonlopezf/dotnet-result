
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.68GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v4


 Method                  | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
------------------------ |---------- |---------- |----------:|------:|-------:|----------:|------------:|
 Builder_Simple          | .NET 10.0 | .NET 10.0 |  17.39 ns |  0.85 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 10.0 | .NET 10.0 |  18.13 ns |  0.88 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 10.0 | .NET 10.0 |  18.56 ns |  0.90 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 8.0  | .NET 8.0  |  20.53 ns |  1.00 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 8.0  | .NET 8.0  |  20.90 ns |  1.02 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 9.0  | .NET 9.0  |  20.92 ns |  1.02 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 9.0  | .NET 9.0  |  22.30 ns |  1.09 | 0.0062 |     104 B |        1.00 |
 Builder_Simple          | .NET 8.0  | .NET 8.0  |  26.27 ns |  1.28 | 0.0062 |     104 B |        1.00 |
 Builder_Simple          | .NET 9.0  | .NET 9.0  |  28.19 ns |  1.37 | 0.0062 |     104 B |        1.00 |
 Error_Equality          | .NET 10.0 | .NET 10.0 |  35.54 ns |  1.73 | 0.0124 |     208 B |        2.00 |
 Error_GetHashCode       | .NET 8.0  | .NET 8.0  |  36.62 ns |  1.78 | 0.0062 |     104 B |        1.00 |
 Error_GetHashCode       | .NET 10.0 | .NET 10.0 |  38.04 ns |  1.85 | 0.0062 |     104 B |        1.00 |
 Error_Equality          | .NET 9.0  | .NET 9.0  |  40.83 ns |  1.99 | 0.0124 |     208 B |        2.00 |
 Error_GetHashCode       | .NET 9.0  | .NET 9.0  |  41.38 ns |  2.02 | 0.0062 |     104 B |        1.00 |
 Error_Equality          | .NET 8.0  | .NET 8.0  |  41.39 ns |  2.02 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_5         | .NET 10.0 | .NET 10.0 |  48.16 ns |  2.35 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_5         | .NET 9.0  | .NET 9.0  |  87.51 ns |  4.26 | 0.0124 |     208 B |        2.00 |
 Builder_Full            | .NET 10.0 | .NET 10.0 | 100.80 ns |  4.91 | 0.0224 |     376 B |        3.62 |
 Builder_Chain_7         | .NET 10.0 | .NET 10.0 | 105.71 ns |  5.15 | 0.0224 |     376 B |        3.62 |
 Builder_Chain_5         | .NET 8.0  | .NET 8.0  | 112.79 ns |  5.49 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_7         | .NET 9.0  | .NET 9.0  | 168.82 ns |  8.22 | 0.0224 |     376 B |        3.62 |
 Builder_Full            | .NET 9.0  | .NET 9.0  | 170.63 ns |  8.31 | 0.0224 |     376 B |        3.62 |
 WithMetadata_Chain_3    | .NET 10.0 | .NET 10.0 | 214.42 ns | 10.44 | 0.0482 |     808 B |        7.77 |
 Builder_Chain_7         | .NET 8.0  | .NET 8.0  | 219.19 ns | 10.68 | 0.0224 |     376 B |        3.62 |
 Builder_Full            | .NET 8.0  | .NET 8.0  | 219.87 ns | 10.71 | 0.0224 |     376 B |        3.62 |
 Builder_WithMetadata_3  | .NET 10.0 | .NET 10.0 | 233.08 ns | 11.35 | 0.0362 |     608 B |        5.85 |
 Builder_WithMetadata_3  | .NET 9.0  | .NET 9.0  | 274.29 ns | 13.36 | 0.0362 |     608 B |        5.85 |
 Builder_BatchMetadata_5 | .NET 10.0 | .NET 10.0 | 302.23 ns | 14.72 | 0.0310 |     520 B |        5.00 |
 Builder_WithMetadata_3  | .NET 8.0  | .NET 8.0  | 315.66 ns | 15.38 | 0.0324 |     544 B |        5.23 |
 WithMetadata_Chain_3    | .NET 9.0  | .NET 9.0  | 321.45 ns | 15.66 | 0.0548 |     920 B |        8.85 |
 Builder_BatchMetadata_5 | .NET 9.0  | .NET 9.0  | 337.98 ns | 16.46 | 0.0310 |     520 B |        5.00 |
 WithMetadata_Chain_3    | .NET 8.0  | .NET 8.0  | 380.53 ns | 18.54 | 0.0648 |    1088 B |       10.46 |
 Builder_BatchMetadata_5 | .NET 8.0  | .NET 8.0  | 425.88 ns | 20.75 | 0.0310 |     520 B |        5.00 |
 Builder_Chain_10        | .NET 10.0 | .NET 10.0 | 489.98 ns | 23.87 | 0.0677 |    1136 B |       10.92 |
 Builder_Chain_10        | .NET 9.0  | .NET 9.0  | 529.15 ns | 25.78 | 0.0601 |    1008 B |        9.69 |
 Builder_Chain_10        | .NET 8.0  | .NET 8.0  | 665.90 ns | 32.44 | 0.0601 |    1008 B |        9.69 |
