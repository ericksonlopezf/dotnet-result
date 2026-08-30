
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.4 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.400
  [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.19 (9.0.19, 9.0.1926.36724), X64 RyuJIT x86-64-v3


 Method                  | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
------------------------ |---------- |---------- |----------:|------:|-------:|----------:|------------:|
 Builder_Simple          | .NET 10.0 | .NET 10.0 |  20.95 ns |  0.86 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 10.0 | .NET 10.0 |  21.99 ns |  0.90 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 10.0 | .NET 10.0 |  22.03 ns |  0.90 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 8.0  | .NET 8.0  |  24.36 ns |  1.00 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 8.0  | .NET 8.0  |  24.95 ns |  1.02 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 9.0  | .NET 9.0  |  29.98 ns |  1.23 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 9.0  | .NET 9.0  |  30.00 ns |  1.23 | 0.0062 |     104 B |        1.00 |
 Builder_Simple          | .NET 8.0  | .NET 8.0  |  37.44 ns |  1.54 | 0.0062 |     104 B |        1.00 |
 Builder_Simple          | .NET 9.0  | .NET 9.0  |  39.10 ns |  1.61 | 0.0062 |     104 B |        1.00 |
 Error_Equality          | .NET 10.0 | .NET 10.0 |  42.55 ns |  1.75 | 0.0124 |     208 B |        2.00 |
 Error_GetHashCode       | .NET 10.0 | .NET 10.0 |  44.68 ns |  1.83 | 0.0062 |     104 B |        1.00 |
 Error_GetHashCode       | .NET 8.0  | .NET 8.0  |  46.25 ns |  1.90 | 0.0062 |     104 B |        1.00 |
 Error_Equality          | .NET 8.0  | .NET 8.0  |  46.53 ns |  1.91 | 0.0124 |     208 B |        2.00 |
 Error_GetHashCode       | .NET 9.0  | .NET 9.0  |  48.44 ns |  1.99 | 0.0062 |     104 B |        1.00 |
 Error_Equality          | .NET 9.0  | .NET 9.0  |  52.34 ns |  2.15 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_5         | .NET 10.0 | .NET 10.0 |  59.64 ns |  2.45 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_5         | .NET 9.0  | .NET 9.0  | 115.61 ns |  4.75 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_5         | .NET 8.0  | .NET 8.0  | 134.98 ns |  5.54 | 0.0124 |     208 B |        2.00 |
 Builder_Full            | .NET 10.0 | .NET 10.0 | 136.76 ns |  5.61 | 0.0224 |     376 B |        3.62 |
 Builder_Chain_7         | .NET 10.0 | .NET 10.0 | 140.29 ns |  5.76 | 0.0224 |     376 B |        3.62 |
 Builder_Full            | .NET 9.0  | .NET 9.0  | 223.85 ns |  9.19 | 0.0224 |     376 B |        3.62 |
 Builder_Chain_7         | .NET 9.0  | .NET 9.0  | 225.52 ns |  9.26 | 0.0224 |     376 B |        3.62 |
 Builder_WithMetadata_3  | .NET 10.0 | .NET 10.0 | 252.99 ns | 10.38 | 0.0324 |     544 B |        5.23 |
 Builder_Full            | .NET 8.0  | .NET 8.0  | 260.22 ns | 10.68 | 0.0224 |     376 B |        3.62 |
 Builder_Chain_7         | .NET 8.0  | .NET 8.0  | 260.82 ns | 10.71 | 0.0224 |     376 B |        3.62 |
 Builder_WithMetadata_3  | .NET 9.0  | .NET 9.0  | 327.62 ns | 13.45 | 0.0324 |     544 B |        5.23 |
 WithMetadata_Chain_3    | .NET 10.0 | .NET 10.0 | 338.77 ns | 13.91 | 0.0520 |     872 B |        8.38 |
 Builder_BatchMetadata_5 | .NET 10.0 | .NET 10.0 | 349.20 ns | 14.33 | 0.0310 |     520 B |        5.00 |
 Builder_WithMetadata_3  | .NET 8.0  | .NET 8.0  | 371.00 ns | 15.23 | 0.0324 |     544 B |        5.23 |
 WithMetadata_Chain_3    | .NET 9.0  | .NET 9.0  | 399.56 ns | 16.40 | 0.0510 |     856 B |        8.23 |
 Builder_BatchMetadata_5 | .NET 9.0  | .NET 9.0  | 424.79 ns | 17.44 | 0.0310 |     520 B |        5.00 |
 WithMetadata_Chain_3    | .NET 8.0  | .NET 8.0  | 477.56 ns | 19.60 | 0.0648 |    1088 B |       10.46 |
 Builder_BatchMetadata_5 | .NET 8.0  | .NET 8.0  | 535.47 ns | 21.98 | 0.0305 |     520 B |        5.00 |
 Builder_Chain_10        | .NET 10.0 | .NET 10.0 | 586.27 ns | 24.07 | 0.0639 |    1072 B |       10.31 |
 Builder_Chain_10        | .NET 9.0  | .NET 9.0  | 707.26 ns | 29.03 | 0.0677 |    1136 B |       10.92 |
 Builder_Chain_10        | .NET 8.0  | .NET 8.0  | 827.80 ns | 33.98 | 0.0639 |    1072 B |       10.31 |
