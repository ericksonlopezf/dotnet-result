```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.37GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4


```
| Method                  | Job       | Runtime   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------ |---------- |---------- |----------:|------:|-------:|----------:|------------:|
| Builder_Simple          | .NET 10.0 | .NET 10.0 |  12.30 ns |  0.82 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 10.0 | .NET 10.0 |  12.70 ns |  0.85 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 10.0 | .NET 10.0 |  12.88 ns |  0.86 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 9.0  | .NET 9.0  |  14.15 ns |  0.94 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 9.0  | .NET 9.0  |  14.25 ns |  0.95 | 0.0062 |     104 B |        1.00 |
| Factory_Failure         | .NET 8.0  | .NET 8.0  |  14.98 ns |  1.00 | 0.0062 |     104 B |        1.00 |
| Factory_Validation      | .NET 8.0  | .NET 8.0  |  15.10 ns |  1.01 | 0.0062 |     104 B |        1.00 |
| Builder_Simple          | .NET 9.0  | .NET 9.0  |  19.23 ns |  1.28 | 0.0062 |     104 B |        1.00 |
| Builder_Simple          | .NET 8.0  | .NET 8.0  |  19.78 ns |  1.32 | 0.0062 |     104 B |        1.00 |
| Error_Equality          | .NET 10.0 | .NET 10.0 |  24.35 ns |  1.63 | 0.0124 |     208 B |        2.00 |
| Error_GetHashCode       | .NET 10.0 | .NET 10.0 |  24.50 ns |  1.64 | 0.0062 |     104 B |        1.00 |
| Error_Equality          | .NET 9.0  | .NET 9.0  |  25.65 ns |  1.71 | 0.0124 |     208 B |        2.00 |
| Error_GetHashCode       | .NET 9.0  | .NET 9.0  |  25.88 ns |  1.73 | 0.0062 |     104 B |        1.00 |
| Error_GetHashCode       | .NET 8.0  | .NET 8.0  |  27.00 ns |  1.80 | 0.0062 |     104 B |        1.00 |
| Error_Equality          | .NET 8.0  | .NET 8.0  |  27.26 ns |  1.82 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_5         | .NET 10.0 | .NET 10.0 |  35.04 ns |  2.34 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_5         | .NET 9.0  | .NET 9.0  |  61.88 ns |  4.13 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_7         | .NET 10.0 | .NET 10.0 |  69.54 ns |  4.64 | 0.0224 |     376 B |        3.62 |
| Builder_Full            | .NET 10.0 | .NET 10.0 |  73.49 ns |  4.91 | 0.0224 |     376 B |        3.62 |
| Builder_Chain_5         | .NET 8.0  | .NET 8.0  |  79.42 ns |  5.30 | 0.0124 |     208 B |        2.00 |
| Builder_Chain_7         | .NET 9.0  | .NET 9.0  | 113.06 ns |  7.55 | 0.0224 |     376 B |        3.62 |
| Builder_Full            | .NET 9.0  | .NET 9.0  | 117.97 ns |  7.88 | 0.0224 |     376 B |        3.62 |
| WithMetadata_Chain_3    | .NET 10.0 | .NET 10.0 | 152.13 ns | 10.16 | 0.0482 |     808 B |        7.77 |
| Builder_Full            | .NET 8.0  | .NET 8.0  | 152.30 ns | 10.17 | 0.0224 |     376 B |        3.62 |
| Builder_WithMetadata_3  | .NET 10.0 | .NET 10.0 | 152.85 ns | 10.20 | 0.0362 |     608 B |        5.85 |
| Builder_Chain_7         | .NET 8.0  | .NET 8.0  | 154.11 ns | 10.29 | 0.0224 |     376 B |        3.62 |
| Builder_WithMetadata_3  | .NET 9.0  | .NET 9.0  | 193.00 ns | 12.88 | 0.0362 |     608 B |        5.85 |
| Builder_BatchMetadata_5 | .NET 10.0 | .NET 10.0 | 198.15 ns | 13.23 | 0.0310 |     520 B |        5.00 |
| WithMetadata_Chain_3    | .NET 9.0  | .NET 9.0  | 214.87 ns | 14.34 | 0.0548 |     920 B |        8.85 |
| Builder_BatchMetadata_5 | .NET 9.0  | .NET 9.0  | 215.97 ns | 14.42 | 0.0310 |     520 B |        5.00 |
| WithMetadata_Chain_3    | .NET 8.0  | .NET 8.0  | 224.88 ns | 15.01 | 0.0610 |    1024 B |        9.85 |
| Builder_WithMetadata_3  | .NET 8.0  | .NET 8.0  | 233.73 ns | 15.60 | 0.0362 |     608 B |        5.85 |
| Builder_BatchMetadata_5 | .NET 8.0  | .NET 8.0  | 321.45 ns | 21.46 | 0.0310 |     520 B |        5.00 |
| Builder_Chain_10        | .NET 10.0 | .NET 10.0 | 337.45 ns | 22.53 | 0.0677 |    1136 B |       10.92 |
| Builder_Chain_10        | .NET 9.0  | .NET 9.0  | 394.81 ns | 26.36 | 0.0677 |    1136 B |       10.92 |
| Builder_Chain_10        | .NET 8.0  | .NET 8.0  | 523.06 ns | 34.92 | 0.0677 |    1136 B |       10.92 |
