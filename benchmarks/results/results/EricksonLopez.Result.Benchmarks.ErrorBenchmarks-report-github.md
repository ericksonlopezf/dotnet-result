```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3


```
| Method                  | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------ |---------- |---------- |----------:|------:|-------:|----------:|------------:|
| Builder_Simple          | .NET 10.0 | .NET 10.0 |  22.68 ns |  0.89 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 10.0 | .NET 10.0 |  23.81 ns |  0.93 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 10.0 | .NET 10.0 |  23.87 ns |  0.93 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 8.0  | .NET 8.0  |  25.04 ns |  0.98 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 8.0  | .NET 8.0  |  25.59 ns |  1.00 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 9.0  | .NET 9.0  |  26.51 ns |  1.04 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 9.0  | .NET 9.0  |  26.68 ns |  1.04 | 0.0062 |     104 B |        1.00 |
| Builder_Simple          | .NET 8.0  | .NET 8.0  |  34.49 ns |  1.35 | 0.0062 |     104 B |        1.00 |
| Builder_Simple          | .NET 9.0  | .NET 9.0  |  35.86 ns |  1.40 | 0.0062 |     104 B |        1.00 |
| Error_Equality          | .NET 10.0 | .NET 10.0 |  46.08 ns |  1.80 | 0.0124 |     208 B |        2.00 |
| Error_GetHashCode       | .NET 9.0  | .NET 9.0  |  46.75 ns |  1.83 | 0.0062 |     104 B |        1.00 |
| Error_GetHashCode       | .NET 8.0  | .NET 8.0  |  47.25 ns |  1.85 | 0.0062 |     104 B |        1.00 |
| Error_GetHashCode       | .NET 10.0 | .NET 10.0 |  49.34 ns |  1.93 | 0.0062 |     104 B |        1.00 |
| Error_Equality          | .NET 8.0  | .NET 8.0  |  49.75 ns |  1.94 | 0.0124 |     208 B |        2.00 |
| Error_Equality          | .NET 9.0  | .NET 9.0  |  52.46 ns |  2.05 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_5         | .NET 10.0 | .NET 10.0 |  64.79 ns |  2.53 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_5         | .NET 9.0  | .NET 9.0  | 112.56 ns |  4.40 | 0.0124 |     208 B |        2.00 |
| Builder_Full            | .NET 10.0 | .NET 10.0 | 135.88 ns |  5.31 | 0.0224 |     376 B |        3.62 |
| Builder_Chain_7         | .NET 10.0 | .NET 10.0 | 140.65 ns |  5.50 | 0.0224 |     376 B |        3.62 |
| Builder_Chain_5         | .NET 8.0  | .NET 8.0  | 144.89 ns |  5.66 | 0.0124 |     208 B |        2.00 |
| Builder_Full            | .NET 9.0  | .NET 9.0  | 216.69 ns |  8.47 | 0.0224 |     376 B |        3.62 |
| Builder_Chain_7         | .NET 9.0  | .NET 9.0  | 224.20 ns |  8.76 | 0.0224 |     376 B |        3.62 |
| Builder_Full            | .NET 8.0  | .NET 8.0  | 281.40 ns | 11.00 | 0.0224 |     376 B |        3.62 |
| Builder_Chain_7         | .NET 8.0  | .NET 8.0  | 287.57 ns | 11.24 | 0.0224 |     376 B |        3.62 |
| Builder_WithMetadata_3  | .NET 10.0 | .NET 10.0 | 303.94 ns | 11.88 | 0.0324 |     544 B |        5.23 |
| WithMetadata_Chain_3    | .NET 10.0 | .NET 10.0 | 334.15 ns | 13.06 | 0.0520 |     872 B |        8.38 |
| Builder_BatchMetadata_5 | .NET 10.0 | .NET 10.0 | 353.28 ns | 13.81 | 0.0310 |     520 B |        5.00 |
| Builder_WithMetadata_3  | .NET 9.0  | .NET 9.0  | 395.79 ns | 15.47 | 0.0362 |     608 B |        5.85 |
| WithMetadata_Chain_3    | .NET 9.0  | .NET 9.0  | 413.35 ns | 16.16 | 0.0548 |     920 B |        8.85 |
| Builder_BatchMetadata_5 | .NET 9.0  | .NET 9.0  | 444.45 ns | 17.37 | 0.0310 |     520 B |        5.00 |
| Builder_WithMetadata_3  | .NET 8.0  | .NET 8.0  | 464.59 ns | 18.16 | 0.0362 |     608 B |        5.85 |
| WithMetadata_Chain_3    | .NET 8.0  | .NET 8.0  | 517.30 ns | 20.22 | 0.0648 |    1088 B |       10.46 |
| Builder_BatchMetadata_5 | .NET 8.0  | .NET 8.0  | 544.34 ns | 21.28 | 0.0305 |     520 B |        5.00 |
| Builder_Chain_10        | .NET 10.0 | .NET 10.0 | 575.00 ns | 22.47 | 0.0639 |    1072 B |       10.31 |
| Builder_Chain_10        | .NET 9.0  | .NET 9.0  | 669.65 ns | 26.17 | 0.0639 |    1072 B |       10.31 |
| Builder_Chain_10        | .NET 8.0  | .NET 8.0  | 871.41 ns | 34.06 | 0.0601 |    1008 B |        9.69 |
