```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.21 (9.0.21, 9.0.2126.50404), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.32 (8.0.32, 8.0.3226.50405), X64 RyuJIT x86-64-v4


```
| Method                  | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------ |---------- |---------- |----------:|------:|-------:|----------:|------------:|
| Builder_Simple          | .NET 10.0 | .NET 10.0 |  17.17 ns |  0.83 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 10.0 | .NET 10.0 |  18.09 ns |  0.87 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 10.0 | .NET 10.0 |  18.50 ns |  0.89 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 9.0  | .NET 9.0  |  20.61 ns |  0.99 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 9.0  | .NET 9.0  |  20.72 ns |  1.00 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 8.0  | .NET 8.0  |  20.73 ns |  1.00 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 8.0  | .NET 8.0  |  20.81 ns |  1.00 | 0.0062 |     104 B |        1.00 |
| Builder_Simple          | .NET 8.0  | .NET 8.0  |  26.86 ns |  1.30 | 0.0062 |     104 B |        1.00 |
| Builder_Simple          | .NET 9.0  | .NET 9.0  |  28.60 ns |  1.38 | 0.0062 |     104 B |        1.00 |
| Error_GetHashCode       | .NET 9.0  | .NET 9.0  |  36.04 ns |  1.74 | 0.0062 |     104 B |        1.00 |
| Error_Equality          | .NET 10.0 | .NET 10.0 |  36.36 ns |  1.75 | 0.0124 |     208 B |        2.00 |
| Error_GetHashCode       | .NET 8.0  | .NET 8.0  |  36.94 ns |  1.78 | 0.0062 |     104 B |        1.00 |
| Error_GetHashCode       | .NET 10.0 | .NET 10.0 |  38.03 ns |  1.84 | 0.0062 |     104 B |        1.00 |
| Error_Equality          | .NET 9.0  | .NET 9.0  |  40.53 ns |  1.96 | 0.0124 |     208 B |        2.00 |
| Error_Equality          | .NET 8.0  | .NET 8.0  |  42.68 ns |  2.06 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_5         | .NET 10.0 | .NET 10.0 |  50.75 ns |  2.45 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_5         | .NET 9.0  | .NET 9.0  |  87.25 ns |  4.21 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_7         | .NET 10.0 | .NET 10.0 | 107.02 ns |  5.16 | 0.0224 |     376 B |        3.62 |
| Builder_Full            | .NET 10.0 | .NET 10.0 | 107.39 ns |  5.18 | 0.0224 |     376 B |        3.62 |
| Builder_Chain_5         | .NET 8.0  | .NET 8.0  | 112.02 ns |  5.41 | 0.0124 |     208 B |        2.00 |
| Builder_Full            | .NET 9.0  | .NET 9.0  | 170.20 ns |  8.21 | 0.0224 |     376 B |        3.62 |
| Builder_Chain_7         | .NET 9.0  | .NET 9.0  | 172.43 ns |  8.32 | 0.0224 |     376 B |        3.62 |
| Builder_WithMetadata_3  | .NET 10.0 | .NET 10.0 | 218.40 ns | 10.54 | 0.0324 |     544 B |        5.23 |
| Builder_Full            | .NET 8.0  | .NET 8.0  | 222.34 ns | 10.73 | 0.0224 |     376 B |        3.62 |
| Builder_Chain_7         | .NET 8.0  | .NET 8.0  | 222.93 ns | 10.76 | 0.0224 |     376 B |        3.62 |
| Builder_WithMetadata_3  | .NET 9.0  | .NET 9.0  | 234.78 ns | 11.33 | 0.0324 |     544 B |        5.23 |
| Builder_BatchMetadata_5 | .NET 10.0 | .NET 10.0 | 277.60 ns | 13.39 | 0.0310 |     520 B |        5.00 |
| WithMetadata_Chain_3    | .NET 10.0 | .NET 10.0 | 281.98 ns | 13.61 | 0.0520 |     872 B |        8.38 |
| Builder_WithMetadata_3  | .NET 8.0  | .NET 8.0  | 316.94 ns | 15.29 | 0.0324 |     544 B |        5.23 |
| Builder_BatchMetadata_5 | .NET 9.0  | .NET 9.0  | 322.60 ns | 15.57 | 0.0310 |     520 B |        5.00 |
| WithMetadata_Chain_3    | .NET 8.0  | .NET 8.0  | 336.39 ns | 16.23 | 0.0610 |    1024 B |        9.85 |
| WithMetadata_Chain_3    | .NET 9.0  | .NET 9.0  | 341.80 ns | 16.49 | 0.0548 |     920 B |        8.85 |
| Builder_BatchMetadata_5 | .NET 8.0  | .NET 8.0  | 424.35 ns | 20.48 | 0.0310 |     520 B |        5.00 |
| Builder_Chain_10        | .NET 10.0 | .NET 10.0 | 473.14 ns | 22.83 | 0.0677 |    1136 B |       10.92 |
| Builder_Chain_10        | .NET 9.0  | .NET 9.0  | 519.67 ns | 25.07 | 0.0639 |    1072 B |       10.31 |
| Builder_Chain_10        | .NET 8.0  | .NET 8.0  | 834.22 ns | 40.25 | 0.0639 |    1072 B |       10.31 |
