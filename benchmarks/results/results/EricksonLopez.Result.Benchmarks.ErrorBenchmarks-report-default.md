
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.21GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4


 Method                  | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
------------------------ |---------- |---------- |----------:|------:|-------:|----------:|------------:|
 Builder_Simple          | .NET 10.0 | .NET 10.0 |  13.49 ns |  0.90 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 10.0 | .NET 10.0 |  13.50 ns |  0.90 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 10.0 | .NET 10.0 |  13.74 ns |  0.92 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 9.0  | .NET 9.0  |  14.87 ns |  1.00 | 0.0062 |     104 B |        1.00 |
 Factory_Failure         | .NET 8.0  | .NET 8.0  |  14.95 ns |  1.00 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 8.0  | .NET 8.0  |  15.35 ns |  1.03 | 0.0062 |     104 B |        1.00 |
 Factory_Validation      | .NET 9.0  | .NET 9.0  |  15.42 ns |  1.03 | 0.0062 |     104 B |        1.00 |
 Builder_Simple          | .NET 8.0  | .NET 8.0  |  19.95 ns |  1.34 | 0.0062 |     104 B |        1.00 |
 Builder_Simple          | .NET 9.0  | .NET 9.0  |  21.67 ns |  1.45 | 0.0062 |     104 B |        1.00 |
 Error_GetHashCode       | .NET 10.0 | .NET 10.0 |  25.16 ns |  1.68 | 0.0062 |     104 B |        1.00 |
 Error_Equality          | .NET 10.0 | .NET 10.0 |  25.40 ns |  1.70 | 0.0124 |     208 B |        2.00 |
 Error_GetHashCode       | .NET 9.0  | .NET 9.0  |  28.60 ns |  1.91 | 0.0062 |     104 B |        1.00 |
 Error_GetHashCode       | .NET 8.0  | .NET 8.0  |  28.92 ns |  1.94 | 0.0062 |     104 B |        1.00 |
 Error_Equality          | .NET 8.0  | .NET 8.0  |  29.27 ns |  1.96 | 0.0124 |     208 B |        2.00 |
 Error_Equality          | .NET 9.0  | .NET 9.0  |  29.49 ns |  1.97 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_5         | .NET 10.0 | .NET 10.0 |  36.37 ns |  2.43 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_5         | .NET 9.0  | .NET 9.0  |  67.51 ns |  4.52 | 0.0124 |     208 B |        2.00 |
 Builder_Chain_7         | .NET 10.0 | .NET 10.0 |  73.59 ns |  4.93 | 0.0224 |     376 B |        3.62 |
 Builder_Full            | .NET 10.0 | .NET 10.0 |  73.97 ns |  4.95 | 0.0224 |     376 B |        3.62 |
 Builder_Chain_5         | .NET 8.0  | .NET 8.0  |  80.70 ns |  5.40 | 0.0124 |     208 B |        2.00 |
 Builder_Full            | .NET 9.0  | .NET 9.0  | 121.55 ns |  8.13 | 0.0224 |     376 B |        3.62 |
 Builder_Chain_7         | .NET 9.0  | .NET 9.0  | 127.53 ns |  8.53 | 0.0224 |     376 B |        3.62 |
 Builder_Chain_7         | .NET 8.0  | .NET 8.0  | 159.45 ns | 10.67 | 0.0224 |     376 B |        3.62 |
 Builder_Full            | .NET 8.0  | .NET 8.0  | 162.81 ns | 10.90 | 0.0224 |     376 B |        3.62 |
 Builder_WithMetadata_3  | .NET 10.0 | .NET 10.0 | 166.42 ns | 11.14 | 0.0362 |     608 B |        5.85 |
 WithMetadata_Chain_3    | .NET 10.0 | .NET 10.0 | 181.88 ns | 12.17 | 0.0520 |     872 B |        8.38 |
 Builder_BatchMetadata_5 | .NET 10.0 | .NET 10.0 | 198.01 ns | 13.25 | 0.0310 |     520 B |        5.00 |
 WithMetadata_Chain_3    | .NET 9.0  | .NET 9.0  | 223.06 ns | 14.93 | 0.0548 |     920 B |        8.85 |
 Builder_WithMetadata_3  | .NET 9.0  | .NET 9.0  | 224.56 ns | 15.03 | 0.0362 |     608 B |        5.85 |
 Builder_WithMetadata_3  | .NET 8.0  | .NET 8.0  | 243.59 ns | 16.30 | 0.0362 |     608 B |        5.85 |
 Builder_BatchMetadata_5 | .NET 9.0  | .NET 9.0  | 245.73 ns | 16.44 | 0.0310 |     520 B |        5.00 |
 WithMetadata_Chain_3    | .NET 8.0  | .NET 8.0  | 267.93 ns | 17.93 | 0.0648 |    1088 B |       10.46 |
 Builder_Chain_10        | .NET 10.0 | .NET 10.0 | 302.22 ns | 20.23 | 0.0601 |    1008 B |        9.69 |
 Builder_BatchMetadata_5 | .NET 8.0  | .NET 8.0  | 310.25 ns | 20.76 | 0.0310 |     520 B |        5.00 |
 Builder_Chain_10        | .NET 9.0  | .NET 9.0  | 390.58 ns | 26.14 | 0.0677 |    1136 B |       10.92 |
 Builder_Chain_10        | .NET 8.0  | .NET 8.0  | 518.56 ns | 34.70 | 0.0677 |    1136 B |       10.92 |
