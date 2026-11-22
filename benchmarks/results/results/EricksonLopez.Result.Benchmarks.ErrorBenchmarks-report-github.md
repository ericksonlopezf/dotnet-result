```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.68GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v4


```
| Method                  | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------ |---------- |---------- |----------:|------:|-------:|----------:|------------:|
| Builder_Simple          | .NET 10.0 | .NET 10.0 |  18.35 ns |  0.87 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 10.0 | .NET 10.0 |  19.39 ns |  0.92 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 10.0 | .NET 10.0 |  19.60 ns |  0.93 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 9.0  | .NET 9.0  |  20.74 ns |  0.98 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 8.0  | .NET 8.0  |  20.87 ns |  0.99 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 8.0  | .NET 8.0  |  21.13 ns |  1.00 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 9.0  | .NET 9.0  |  22.22 ns |  1.05 | 0.0062 |     104 B |        1.00 |
| Builder_Simple          | .NET 8.0  | .NET 8.0  |  27.07 ns |  1.28 | 0.0062 |     104 B |        1.00 |
| Builder_Simple          | .NET 9.0  | .NET 9.0  |  28.30 ns |  1.34 | 0.0062 |     104 B |        1.00 |
| Error_GetHashCode       | .NET 9.0  | .NET 9.0  |  36.81 ns |  1.74 | 0.0062 |     104 B |        1.00 |
| Error_GetHashCode       | .NET 8.0  | .NET 8.0  |  37.04 ns |  1.75 | 0.0062 |     104 B |        1.00 |
| Error_Equality          | .NET 10.0 | .NET 10.0 |  38.10 ns |  1.80 | 0.0124 |     208 B |        2.00 |
| Error_GetHashCode       | .NET 10.0 | .NET 10.0 |  38.97 ns |  1.84 | 0.0062 |     104 B |        1.00 |
| Error_Equality          | .NET 9.0  | .NET 9.0  |  41.79 ns |  1.98 | 0.0124 |     208 B |        2.00 |
| Error_Equality          | .NET 8.0  | .NET 8.0  |  41.89 ns |  1.98 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_5         | .NET 10.0 | .NET 10.0 |  52.66 ns |  2.49 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_5         | .NET 9.0  | .NET 9.0  |  87.71 ns |  4.15 | 0.0124 |     208 B |        2.00 |
| Builder_Full            | .NET 10.0 | .NET 10.0 | 102.58 ns |  4.86 | 0.0224 |     376 B |        3.62 |
| Builder_Chain_7         | .NET 10.0 | .NET 10.0 | 106.35 ns |  5.04 | 0.0224 |     376 B |        3.62 |
| Builder_Chain_5         | .NET 8.0  | .NET 8.0  | 113.29 ns |  5.36 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_7         | .NET 9.0  | .NET 9.0  | 171.62 ns |  8.13 | 0.0224 |     376 B |        3.62 |
| Builder_Full            | .NET 9.0  | .NET 9.0  | 177.45 ns |  8.40 | 0.0224 |     376 B |        3.62 |
| Builder_WithMetadata_3  | .NET 10.0 | .NET 10.0 | 221.26 ns | 10.48 | 0.0324 |     544 B |        5.23 |
| Builder_Full            | .NET 8.0  | .NET 8.0  | 226.76 ns | 10.74 | 0.0224 |     376 B |        3.62 |
| Builder_Chain_7         | .NET 8.0  | .NET 8.0  | 232.30 ns | 11.00 | 0.0224 |     376 B |        3.62 |
| WithMetadata_Chain_3    | .NET 10.0 | .NET 10.0 | 279.13 ns | 13.22 | 0.0520 |     872 B |        8.38 |
| Builder_WithMetadata_3  | .NET 9.0  | .NET 9.0  | 280.82 ns | 13.30 | 0.0362 |     608 B |        5.85 |
| Builder_BatchMetadata_5 | .NET 10.0 | .NET 10.0 | 302.06 ns | 14.30 | 0.0310 |     520 B |        5.00 |
| WithMetadata_Chain_3    | .NET 9.0  | .NET 9.0  | 327.25 ns | 15.49 | 0.0548 |     920 B |        8.85 |
| Builder_BatchMetadata_5 | .NET 9.0  | .NET 9.0  | 354.96 ns | 16.81 | 0.0310 |     520 B |        5.00 |
| WithMetadata_Chain_3    | .NET 8.0  | .NET 8.0  | 393.58 ns | 18.63 | 0.0648 |    1088 B |       10.46 |
| Builder_WithMetadata_3  | .NET 8.0  | .NET 8.0  | 431.83 ns | 20.44 | 0.0362 |     608 B |        5.85 |
| Builder_Chain_10        | .NET 10.0 | .NET 10.0 | 433.38 ns | 20.52 | 0.0601 |    1008 B |        9.69 |
| Builder_BatchMetadata_5 | .NET 8.0  | .NET 8.0  | 485.49 ns | 22.99 | 0.0305 |     520 B |        5.00 |
| Builder_Chain_10        | .NET 9.0  | .NET 9.0  | 602.35 ns | 28.52 | 0.0677 |    1136 B |       10.92 |
| Builder_Chain_10        | .NET 8.0  | .NET 8.0  | 689.64 ns | 32.65 | 0.0601 |    1008 B |        9.69 |
