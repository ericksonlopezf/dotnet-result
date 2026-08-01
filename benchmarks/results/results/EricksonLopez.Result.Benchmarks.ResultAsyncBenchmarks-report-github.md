```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8875/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.302
  [Host]    : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.29 (8.0.29, 8.0.2926.32403), X64 RyuJIT x86-64-v4


```
| Method                      | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------- |---------- |--------------- |------------ |------------ |-----------:|------:|-------:|----------:|------------:|
| Map_SyncCompleted           | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   7.542 ns |  1.00 | 0.0032 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   7.558 ns |  1.00 | 0.0032 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   7.671 ns |  1.02 | 0.0032 |     160 B |        1.00 |
| Failure_Map_SyncCompleted   | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   7.686 ns |  1.02 | 0.0032 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   7.840 ns |  1.04 | 0.0032 |     160 B |        1.00 |
| Bind_SyncCompleted          | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   8.135 ns |  1.08 | 0.0033 |     168 B |        1.05 |
| Tap_SyncCompleted           | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   9.241 ns |  1.23 | 0.0049 |     248 B |        1.55 |
| Failure_Map_AsyncCompleted  | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 463.249 ns | 61.44 | 0.0038 |     237 B |        1.48 |
| Map_AsyncCompleted          | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 464.694 ns | 61.63 | 0.0043 |     237 B |        1.48 |
|                             |           |           |                |             |             |            |       |        |           |             |
| Failure_Map_SyncCompleted   | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  12.806 ns |  0.97 | 0.0032 |     160 B |        1.00 |
| Ensure_SyncCompleted_Passes | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  12.947 ns |  0.98 | 0.0032 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  13.052 ns |  0.99 | 0.0032 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  13.230 ns |  1.00 | 0.0032 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  13.321 ns |  1.01 | 0.0032 |     160 B |        1.00 |
| Tap_SyncCompleted           | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  13.869 ns |  1.05 | 0.0049 |     248 B |        1.55 |
| Bind_SyncCompleted          | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  14.041 ns |  1.06 | 0.0033 |     168 B |        1.05 |
| Map_AsyncCompleted          | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 462.571 ns | 34.97 | 0.0043 |     238 B |        1.49 |
| Failure_Map_AsyncCompleted  | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 467.218 ns | 35.32 | 0.0043 |     237 B |        1.48 |
|                             |           |           |                |             |             |            |       |        |           |             |
| Ensure_SyncCompleted_Passes | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  12.890 ns |  0.97 | 0.0032 |     160 B |        1.00 |
| Ensure_SyncCompleted_Fails  | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  12.990 ns |  0.98 | 0.0032 |     160 B |        1.00 |
| Map_SyncCompleted_TState    | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  13.048 ns |  0.98 | 0.0032 |     160 B |        1.00 |
| Failure_Map_SyncCompleted   | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  13.250 ns |  1.00 | 0.0032 |     160 B |        1.00 |
| Map_SyncCompleted           | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  13.283 ns |  1.00 | 0.0032 |     160 B |        1.00 |
| Tap_SyncCompleted           | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  13.737 ns |  1.03 | 0.0049 |     248 B |        1.55 |
| Bind_SyncCompleted          | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  14.494 ns |  1.09 | 0.0033 |     168 B |        1.05 |
| Map_AsyncCompleted          | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           | 465.761 ns | 35.07 | 0.0043 |     238 B |        1.49 |
| Failure_Map_AsyncCompleted  | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           | 471.348 ns | 35.49 | 0.0043 |     238 B |        1.49 |
