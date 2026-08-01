```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8875/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.302
  [Host]    : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.29 (8.0.29, 8.0.2926.32403), X64 RyuJIT x86-64-v4

RatioSD=?  

```
| Method                   | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean       | Median     | Ratio | Allocated | Alloc Ratio |
|------------------------- |---------- |---------- |--------------- |------------ |------------ |-----------:|-----------:|------:|----------:|------------:|
| Success_NonGeneric       | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  0.0000 ns |  0.0000 ns |     ? |         - |           ? |
| Success_String           | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  0.0000 ns |  0.0000 ns |     ? |         - |           ? |
| Success_Int              | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  0.0032 ns |  0.0011 ns |     ? |         - |           ? |
| ImplicitConversion_Value | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  0.0060 ns |  0.0064 ns |     ? |         - |           ? |
| Failure_NonGeneric       | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  0.7574 ns |  0.7587 ns |     ? |         - |           ? |
| ImplicitConversion_Error | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  0.7767 ns |  0.7756 ns |     ? |         - |           ? |
| Failure_Int              | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  0.7816 ns |  0.7843 ns |     ? |         - |           ? |
| Success_Guid             | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 35.4368 ns | 35.4602 ns |     ? |         - |           ? |
|                          |           |           |                |             |             |            |            |       |           |             |
| Success_Int              | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  0.0000 ns |  0.0000 ns |     ? |         - |           ? |
| ImplicitConversion_Value | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  0.0000 ns |  0.0000 ns |     ? |         - |           ? |
| Success_NonGeneric       | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  0.0029 ns |  0.0009 ns |     ? |         - |           ? |
| Success_String           | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  0.0144 ns |  0.0153 ns |     ? |         - |           ? |
| ImplicitConversion_Error | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  0.7600 ns |  0.7610 ns |     ? |         - |           ? |
| Failure_Int              | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  0.7624 ns |  0.7638 ns |     ? |         - |           ? |
| Failure_NonGeneric       | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |  0.7746 ns |  0.7727 ns |     ? |         - |           ? |
| Success_Guid             | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     | 39.1728 ns | 39.1720 ns |     ? |         - |           ? |
|                          |           |           |                |             |             |            |            |       |           |             |
| Success_NonGeneric       | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  0.0001 ns |  0.0000 ns |     ? |         - |           ? |
| Success_Int              | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  0.0013 ns |  0.0000 ns |     ? |         - |           ? |
| ImplicitConversion_Value | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  0.0015 ns |  0.0000 ns |     ? |         - |           ? |
| Success_String           | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  0.0160 ns |  0.0129 ns |     ? |         - |           ? |
| ImplicitConversion_Error | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  0.7598 ns |  0.7605 ns |     ? |         - |           ? |
| Failure_NonGeneric       | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  0.7704 ns |  0.7731 ns |     ? |         - |           ? |
| Failure_Int              | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           |  0.7803 ns |  0.7789 ns |     ? |         - |           ? |
| Success_Guid             | .NET 8.0  | .NET 8.0  | 3              | 1           | 3           | 38.6259 ns | 38.5900 ns |     ? |         - |           ? |
