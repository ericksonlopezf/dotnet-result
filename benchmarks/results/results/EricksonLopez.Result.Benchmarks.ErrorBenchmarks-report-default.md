
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3


 Method                  | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
------------------------ |---------- |---------- |----------:|------:|-------:|----------:|------------:|
 Builder_Simple          | .NET 10.0 | .NET 10.0 |  21.36 ns |  0.81 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 10.0 | .NET 10.0 |  22.36 ns |  0.85 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 10.0 | .NET 10.0 |  23.38 ns |  0.89 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 8.0  | .NET 8.0  |  25.11 ns |  0.95 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 9.0  | .NET 9.0  |  25.85 ns |  0.98 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 9.0  | .NET 9.0  |  25.89 ns |  0.98 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 8.0  | .NET 8.0  |  26.37 ns |  1.00 | 0.0062 |     104 B |        1.00 |
 Builder_Simple          | .NET 9.0  | .NET 9.0  |  36.45 ns |  1.38 | 0.0062 |     104 B |        1.00 |
 Builder_Simple          | .NET 8.0  | .NET 8.0  |  38.37 ns |  1.46 | 0.0062 |     104 B |        1.00 |
 Error_Equality          | .NET 10.0 | .NET 10.0 |  43.11 ns |  1.64 | 0.0124 |     208 B |        2.00 |
 Error_GetHashCode       | .NET 10.0 | .NET 10.0 |  46.53 ns |  1.77 | 0.0062 |     104 B |        1.00 |
 Error_GetHashCode       | .NET 9.0  | .NET 9.0  |  46.91 ns |  1.78 | 0.0062 |     104 B |        1.00 |
 Error_GetHashCode       | .NET 8.0  | .NET 8.0  |  47.23 ns |  1.79 | 0.0062 |     104 B |        1.00 |
 Error_Equality          | .NET 8.0  | .NET 8.0  |  48.41 ns |  1.84 | 0.0124 |     208 B |        2.00 |
 Error_Equality          | .NET 9.0  | .NET 9.0  |  51.99 ns |  1.97 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_5         | .NET 10.0 | .NET 10.0 |  63.43 ns |  2.41 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_5         | .NET 9.0  | .NET 9.0  | 111.86 ns |  4.24 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_7         | .NET 10.0 | .NET 10.0 | 130.84 ns |  4.97 | 0.0224 |     376 B |        3.62 |
 Builder_Chain_5         | .NET 8.0  | .NET 8.0  | 136.04 ns |  5.16 | 0.0124 |     208 B |        2.00 |
 Builder_Full            | .NET 10.0 | .NET 10.0 | 140.82 ns |  5.34 | 0.0224 |     376 B |        3.62 |
 Builder_Full            | .NET 9.0  | .NET 9.0  | 213.12 ns |  8.09 | 0.0224 |     376 B |        3.62 |
 Builder_Chain_7         | .NET 9.0  | .NET 9.0  | 217.05 ns |  8.24 | 0.0224 |     376 B |        3.62 |
 Builder_Chain_7         | .NET 8.0  | .NET 8.0  | 265.82 ns | 10.09 | 0.0224 |     376 B |        3.62 |
 Builder_Full            | .NET 8.0  | .NET 8.0  | 281.56 ns | 10.68 | 0.0224 |     376 B |        3.62 |
 WithMetadata_Chain_3    | .NET 10.0 | .NET 10.0 | 309.91 ns | 11.76 | 0.0482 |     808 B |        7.77 |
 Builder_WithMetadata_3  | .NET 10.0 | .NET 10.0 | 315.21 ns | 11.96 | 0.0362 |     608 B |        5.85 |
 Builder_BatchMetadata_5 | .NET 10.0 | .NET 10.0 | 346.45 ns | 13.15 | 0.0310 |     520 B |        5.00 |
 Builder_WithMetadata_3  | .NET 9.0  | .NET 9.0  | 346.87 ns | 13.16 | 0.0362 |     608 B |        5.85 |
 Builder_BatchMetadata_5 | .NET 9.0  | .NET 9.0  | 404.08 ns | 15.33 | 0.0310 |     520 B |        5.00 |
 WithMetadata_Chain_3    | .NET 9.0  | .NET 9.0  | 414.11 ns | 15.71 | 0.0548 |     920 B |        8.85 |
 WithMetadata_Chain_3    | .NET 8.0  | .NET 8.0  | 424.28 ns | 16.10 | 0.0610 |    1024 B |        9.85 |
 Builder_WithMetadata_3  | .NET 8.0  | .NET 8.0  | 438.61 ns | 16.64 | 0.0362 |     608 B |        5.85 |
 Builder_BatchMetadata_5 | .NET 8.0  | .NET 8.0  | 544.85 ns | 20.68 | 0.0305 |     520 B |        5.00 |
 Builder_Chain_10        | .NET 10.0 | .NET 10.0 | 604.18 ns | 22.93 | 0.0639 |    1072 B |       10.31 |
 Builder_Chain_10        | .NET 9.0  | .NET 9.0  | 654.80 ns | 24.85 | 0.0639 |    1072 B |       10.31 |
 Builder_Chain_10        | .NET 8.0  | .NET 8.0  | 872.05 ns | 33.09 | 0.0639 |    1072 B |       10.31 |
