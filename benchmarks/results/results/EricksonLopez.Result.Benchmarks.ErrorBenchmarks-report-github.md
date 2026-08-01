```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8875/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.302
  [Host]    : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.29 (8.0.29, 8.0.2926.32403), X64 RyuJIT x86-64-v4


```
| Method                 | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------- |---------- |---------- |--------------- |------------ |------------ |-----------:|------:|-------:|----------:|------------:|
| Factory_Failure        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   9.898 ns |  1.00 | 0.0021 |     104 B |        1.00 |
| Factory_Validation     | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   9.911 ns |  1.00 | 0.0021 |     104 B |        1.00 |
| Builder_Simple         | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  11.362 ns |  1.15 | 0.0021 |     104 B |        1.00 |
| Error_Equality         | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  18.401 ns |  1.86 | 0.0041 |     208 B |        2.00 |
| Error_GetHashCode      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  21.813 ns |  2.21 | 0.0021 |     104 B |        1.00 |
| Builder_Full           | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  72.366 ns |  7.32 | 0.0074 |     376 B |        3.62 |
| Builder_WithMetadata   | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  94.007 ns |  9.51 | 0.0079 |     400 B |        3.85 |
| WithMetadata_Chain_3   | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 128.867 ns | 13.03 | 0.0160 |     808 B |        7.77 |
| Builder_WithMetadata_3 | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 136.772 ns | 13.83 | 0.0119 |     608 B |        5.85 |
|                        |           |           |                |             |             |            |       |        |           |             |
| Factory_Validation     | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  10.725 ns |  0.97 | 0.0021 |     104 B |        1.00 |
| Factory_Failure        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  11.107 ns |  1.00 | 0.0021 |     104 B |        1.00 |
| Builder_Simple         | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  15.771 ns |  1.42 | 0.0021 |     104 B |        1.00 |
| Error_Equality         | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  19.163 ns |  1.73 | 0.0041 |     208 B |        2.00 |
| Error_GetHashCode      | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  23.195 ns |  2.09 | 0.0021 |     104 B |        1.00 |
| Builder_Full           | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 131.149 ns | 11.82 | 0.0074 |     376 B |        3.62 |
| Builder_WithMetadata   | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 139.179 ns | 12.54 | 0.0079 |     400 B |        3.85 |
| WithMetadata_Chain_3   | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 177.588 ns | 16.00 | 0.0203 |    1024 B |        9.85 |
| Builder_WithMetadata_3 | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 188.433 ns | 16.98 | 0.0119 |     608 B |        5.85 |
|                        |           |           |                |             |             |            |       |        |           |             |
| Factory_Failure        | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  10.355 ns |  1.00 | 0.0021 |     104 B |        1.00 |
| Factory_Validation     | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  10.588 ns |  1.02 | 0.0021 |     104 B |        1.00 |
| Builder_Simple         | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  16.651 ns |  1.61 | 0.0021 |     104 B |        1.00 |
| Error_Equality         | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  18.176 ns |  1.76 | 0.0041 |     208 B |        2.00 |
| Error_GetHashCode      | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  23.052 ns |  2.23 | 0.0021 |     104 B |        1.00 |
| Builder_Full           | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           | 112.938 ns | 10.91 | 0.0074 |     376 B |        3.62 |
| Builder_WithMetadata   | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           | 135.938 ns | 13.14 | 0.0079 |     400 B |        3.85 |
| Builder_WithMetadata_3 | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           | 180.786 ns | 17.47 | 0.0119 |     608 B |        5.85 |
| WithMetadata_Chain_3   | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           | 204.133 ns | 19.73 | 0.0215 |    1088 B |       10.46 |
