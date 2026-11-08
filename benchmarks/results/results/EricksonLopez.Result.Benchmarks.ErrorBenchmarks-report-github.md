```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.63GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4


```
| Method                  | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------ |---------- |---------- |----------:|------:|-------:|----------:|------------:|
| Builder_Simple          | .NET 10.0 | .NET 10.0 |  17.97 ns |  0.85 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 10.0 | .NET 10.0 |  18.15 ns |  0.86 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 10.0 | .NET 10.0 |  18.40 ns |  0.87 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 9.0  | .NET 9.0  |  20.58 ns |  0.97 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 9.0  | .NET 9.0  |  20.91 ns |  0.99 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 8.0  | .NET 8.0  |  21.21 ns |  1.00 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 8.0  | .NET 8.0  |  21.66 ns |  1.02 | 0.0062 |     104 B |        1.00 |
| Builder_Simple          | .NET 8.0  | .NET 8.0  |  26.78 ns |  1.26 | 0.0062 |     104 B |        1.00 |
| Builder_Simple          | .NET 9.0  | .NET 9.0  |  28.60 ns |  1.35 | 0.0062 |     104 B |        1.00 |
| Error_Equality          | .NET 10.0 | .NET 10.0 |  36.44 ns |  1.72 | 0.0124 |     208 B |        2.00 |
| Error_GetHashCode       | .NET 9.0  | .NET 9.0  |  37.15 ns |  1.75 | 0.0062 |     104 B |        1.00 |
| Error_GetHashCode       | .NET 8.0  | .NET 8.0  |  37.37 ns |  1.76 | 0.0062 |     104 B |        1.00 |
| Error_GetHashCode       | .NET 10.0 | .NET 10.0 |  38.05 ns |  1.79 | 0.0062 |     104 B |        1.00 |
| Error_Equality          | .NET 9.0  | .NET 9.0  |  41.08 ns |  1.94 | 0.0124 |     208 B |        2.00 |
| Error_Equality          | .NET 8.0  | .NET 8.0  |  42.34 ns |  2.00 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_5         | .NET 10.0 | .NET 10.0 |  49.44 ns |  2.33 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_5         | .NET 9.0  | .NET 9.0  |  97.17 ns |  4.58 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_7         | .NET 10.0 | .NET 10.0 | 108.38 ns |  5.11 | 0.0224 |     376 B |        3.62 |
| Builder_Full            | .NET 10.0 | .NET 10.0 | 109.47 ns |  5.16 | 0.0224 |     376 B |        3.62 |
| Builder_Chain_5         | .NET 8.0  | .NET 8.0  | 120.88 ns |  5.70 | 0.0124 |     208 B |        2.00 |
| Builder_Full            | .NET 9.0  | .NET 9.0  | 169.57 ns |  8.00 | 0.0224 |     376 B |        3.62 |
| Builder_Chain_7         | .NET 9.0  | .NET 9.0  | 173.46 ns |  8.18 | 0.0224 |     376 B |        3.62 |
| WithMetadata_Chain_3    | .NET 10.0 | .NET 10.0 | 219.05 ns | 10.33 | 0.0482 |     808 B |        7.77 |
| Builder_Full            | .NET 8.0  | .NET 8.0  | 224.42 ns | 10.58 | 0.0224 |     376 B |        3.62 |
| Builder_Chain_7         | .NET 8.0  | .NET 8.0  | 226.76 ns | 10.69 | 0.0224 |     376 B |        3.62 |
| Builder_WithMetadata_3  | .NET 10.0 | .NET 10.0 | 232.42 ns | 10.96 | 0.0362 |     608 B |        5.85 |
| Builder_BatchMetadata_5 | .NET 10.0 | .NET 10.0 | 258.09 ns | 12.17 | 0.0310 |     520 B |        5.00 |
| Builder_WithMetadata_3  | .NET 9.0  | .NET 9.0  | 284.91 ns | 13.44 | 0.0362 |     608 B |        5.85 |
| WithMetadata_Chain_3    | .NET 9.0  | .NET 9.0  | 296.25 ns | 13.97 | 0.0510 |     856 B |        8.23 |
| Builder_BatchMetadata_5 | .NET 9.0  | .NET 9.0  | 366.84 ns | 17.30 | 0.0310 |     520 B |        5.00 |
| Builder_WithMetadata_3  | .NET 8.0  | .NET 8.0  | 375.37 ns | 17.70 | 0.0362 |     608 B |        5.85 |
| WithMetadata_Chain_3    | .NET 8.0  | .NET 8.0  | 388.72 ns | 18.33 | 0.0648 |    1088 B |       10.46 |
| Builder_BatchMetadata_5 | .NET 8.0  | .NET 8.0  | 453.68 ns | 21.39 | 0.0310 |     520 B |        5.00 |
| Builder_Chain_10        | .NET 10.0 | .NET 10.0 | 504.91 ns | 23.81 | 0.0677 |    1136 B |       10.92 |
| Builder_Chain_10        | .NET 9.0  | .NET 9.0  | 519.14 ns | 24.48 | 0.0639 |    1072 B |       10.31 |
| Builder_Chain_10        | .NET 8.0  | .NET 8.0  | 773.49 ns | 36.48 | 0.0677 |    1136 B |       10.92 |
