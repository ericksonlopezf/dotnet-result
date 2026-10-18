```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.21 (9.0.21, 9.0.2126.50404), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.32 (8.0.32, 8.0.3226.50405), X64 RyuJIT x86-64-v4


```
| Method                          | Job       | Runtime   | Mean        | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |---------- |---------- |------------:|------:|-------:|----------:|------------:|
| Serialize_Result_Success        | .NET 10.0 | .NET 10.0 |    95.63 ns |  0.36 | 0.0038 |      64 B |        0.42 |
| Serialize_Result_Success        | .NET 9.0  | .NET 9.0  |   114.70 ns |  0.43 | 0.0038 |      64 B |        0.42 |
| Serialize_Result_Success        | .NET 8.0  | .NET 8.0  |   120.53 ns |  0.45 | 0.0038 |      64 B |        0.42 |
| Serialize_ResultOfT_Success     | .NET 10.0 | .NET 10.0 |   156.82 ns |  0.59 | 0.0043 |      72 B |        0.47 |
| Serialize_Error_NoMetadata      | .NET 10.0 | .NET 10.0 |   195.30 ns |  0.73 | 0.0091 |     152 B |        1.00 |
| Serialize_ResultOfT_Success     | .NET 9.0  | .NET 9.0  |   197.39 ns |  0.74 | 0.0043 |      72 B |        0.47 |
| Serialize_ResultOfT_Success     | .NET 8.0  | .NET 8.0  |   205.48 ns |  0.77 | 0.0043 |      72 B |        0.47 |
| Serialize_Error_NoMetadata      | .NET 9.0  | .NET 9.0  |   257.96 ns |  0.97 | 0.0091 |     152 B |        1.00 |
| Serialize_Error_NoMetadata      | .NET 8.0  | .NET 8.0  |   267.25 ns |  1.00 | 0.0091 |     152 B |        1.00 |
| Serialize_Error_StringMetadata  | .NET 10.0 | .NET 10.0 |   430.98 ns |  1.61 | 0.0224 |     376 B |        2.47 |
| Serialize_Error_InnerErrors     | .NET 10.0 | .NET 10.0 |   512.90 ns |  1.92 | 0.0238 |     408 B |        2.68 |
| Serialize_Error_StringMetadata  | .NET 9.0  | .NET 9.0  |   520.37 ns |  1.95 | 0.0219 |     376 B |        2.47 |
| Serialize_Error_StringMetadata  | .NET 8.0  | .NET 8.0  |   568.91 ns |  2.13 | 0.0219 |     376 B |        2.47 |
| Serialize_Error_MixedMetadata   | .NET 10.0 | .NET 10.0 |   580.26 ns |  2.17 | 0.0305 |     512 B |        3.37 |
| Serialize_ResultOfT_Failure     | .NET 10.0 | .NET 10.0 |   655.54 ns |  2.45 | 0.0334 |     560 B |        3.68 |
| Serialize_Error_InnerErrors     | .NET 9.0  | .NET 9.0  |   656.10 ns |  2.46 | 0.0238 |     408 B |        2.68 |
| Serialize_Result_Failure        | .NET 10.0 | .NET 10.0 |   660.51 ns |  2.47 | 0.0334 |     560 B |        3.68 |
| Serialize_Error_InnerErrors     | .NET 8.0  | .NET 8.0  |   702.78 ns |  2.63 | 0.0238 |     408 B |        2.68 |
| Serialize_Error_MixedMetadata   | .NET 9.0  | .NET 9.0  |   762.06 ns |  2.85 | 0.0305 |     512 B |        3.37 |
| Serialize_Error_MixedMetadata   | .NET 8.0  | .NET 8.0  |   779.28 ns |  2.92 | 0.0305 |     512 B |        3.37 |
| Serialize_Result_Failure        | .NET 9.0  | .NET 9.0  |   779.84 ns |  2.92 | 0.0334 |     560 B |        3.68 |
| Serialize_ResultOfT_Failure     | .NET 9.0  | .NET 9.0  |   797.42 ns |  2.98 | 0.0334 |     560 B |        3.68 |
| Serialize_Result_Failure        | .NET 8.0  | .NET 8.0  |   836.34 ns |  3.13 | 0.0334 |     560 B |        3.68 |
| Serialize_ResultOfT_Failure     | .NET 8.0  | .NET 8.0  |   861.86 ns |  3.22 | 0.0334 |     560 B |        3.68 |
| Deserialize_Error_MixedMetadata | .NET 10.0 | .NET 10.0 | 1,012.54 ns |  3.79 | 0.1087 |    1832 B |       12.05 |
| Deserialize_Error_MixedMetadata | .NET 9.0  | .NET 9.0  | 1,310.02 ns |  4.90 | 0.1087 |    1832 B |       12.05 |
| Deserialize_Error_MixedMetadata | .NET 8.0  | .NET 8.0  | 1,467.19 ns |  5.49 | 0.1087 |    1832 B |       12.05 |
| RoundTrip_Result                | .NET 10.0 | .NET 10.0 | 2,128.15 ns |  7.96 | 0.1488 |    2504 B |       16.47 |
| RoundTrip_Result                | .NET 9.0  | .NET 9.0  | 2,512.27 ns |  9.40 | 0.1488 |    2504 B |       16.47 |
| RoundTrip_Result                | .NET 8.0  | .NET 8.0  | 2,636.39 ns |  9.87 | 0.1488 |    2504 B |       16.47 |
