```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.68GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v4


```
| Method                          | Job       | Runtime   | Mean        | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |---------- |---------- |------------:|------:|-------:|----------:|------------:|
| Serialize_Result_Success        | .NET 10.0 | .NET 10.0 |    96.64 ns |  0.36 | 0.0038 |      64 B |        0.42 |
| Serialize_Result_Success        | .NET 9.0  | .NET 9.0  |   116.19 ns |  0.43 | 0.0038 |      64 B |        0.42 |
| Serialize_Result_Success        | .NET 8.0  | .NET 8.0  |   122.41 ns |  0.45 | 0.0038 |      64 B |        0.42 |
| Serialize_ResultOfT_Success     | .NET 10.0 | .NET 10.0 |   154.49 ns |  0.57 | 0.0043 |      72 B |        0.47 |
| Serialize_ResultOfT_Success     | .NET 8.0  | .NET 8.0  |   197.66 ns |  0.73 | 0.0043 |      72 B |        0.47 |
| Serialize_ResultOfT_Success     | .NET 9.0  | .NET 9.0  |   199.56 ns |  0.74 | 0.0043 |      72 B |        0.47 |
| Serialize_Error_NoMetadata      | .NET 10.0 | .NET 10.0 |   205.65 ns |  0.76 | 0.0091 |     152 B |        1.00 |
| Serialize_Error_NoMetadata      | .NET 9.0  | .NET 9.0  |   251.77 ns |  0.93 | 0.0091 |     152 B |        1.00 |
| Serialize_Error_NoMetadata      | .NET 8.0  | .NET 8.0  |   270.73 ns |  1.00 | 0.0091 |     152 B |        1.00 |
| Serialize_Error_StringMetadata  | .NET 10.0 | .NET 10.0 |   438.12 ns |  1.62 | 0.0224 |     376 B |        2.47 |
| Serialize_Error_InnerErrors     | .NET 10.0 | .NET 10.0 |   506.23 ns |  1.87 | 0.0238 |     408 B |        2.68 |
| Serialize_Error_StringMetadata  | .NET 9.0  | .NET 9.0  |   520.44 ns |  1.92 | 0.0219 |     376 B |        2.47 |
| Serialize_Error_StringMetadata  | .NET 8.0  | .NET 8.0  |   566.07 ns |  2.09 | 0.0219 |     376 B |        2.47 |
| Serialize_Error_MixedMetadata   | .NET 10.0 | .NET 10.0 |   588.07 ns |  2.17 | 0.0305 |     512 B |        3.37 |
| Serialize_Error_InnerErrors     | .NET 9.0  | .NET 9.0  |   661.73 ns |  2.44 | 0.0238 |     408 B |        2.68 |
| Serialize_Result_Failure        | .NET 10.0 | .NET 10.0 |   665.83 ns |  2.46 | 0.0334 |     560 B |        3.68 |
| Serialize_ResultOfT_Failure     | .NET 10.0 | .NET 10.0 |   671.80 ns |  2.48 | 0.0334 |     560 B |        3.68 |
| Serialize_Error_InnerErrors     | .NET 8.0  | .NET 8.0  |   698.70 ns |  2.58 | 0.0238 |     408 B |        2.68 |
| Serialize_Error_MixedMetadata   | .NET 9.0  | .NET 9.0  |   742.22 ns |  2.74 | 0.0305 |     512 B |        3.37 |
| Serialize_Error_MixedMetadata   | .NET 8.0  | .NET 8.0  |   779.15 ns |  2.88 | 0.0305 |     512 B |        3.37 |
| Serialize_Result_Failure        | .NET 9.0  | .NET 9.0  |   796.05 ns |  2.94 | 0.0334 |     560 B |        3.68 |
| Serialize_ResultOfT_Failure     | .NET 9.0  | .NET 9.0  |   808.63 ns |  2.99 | 0.0334 |     560 B |        3.68 |
| Serialize_ResultOfT_Failure     | .NET 8.0  | .NET 8.0  |   846.44 ns |  3.13 | 0.0334 |     560 B |        3.68 |
| Serialize_Result_Failure        | .NET 8.0  | .NET 8.0  |   867.96 ns |  3.21 | 0.0334 |     560 B |        3.68 |
| Deserialize_Error_MixedMetadata | .NET 10.0 | .NET 10.0 | 1,051.89 ns |  3.89 | 0.1087 |    1832 B |       12.05 |
| Deserialize_Error_MixedMetadata | .NET 9.0  | .NET 9.0  | 1,334.70 ns |  4.93 | 0.1087 |    1832 B |       12.05 |
| Deserialize_Error_MixedMetadata | .NET 8.0  | .NET 8.0  | 1,489.61 ns |  5.50 | 0.1087 |    1832 B |       12.05 |
| RoundTrip_Result                | .NET 10.0 | .NET 10.0 | 2,110.22 ns |  7.79 | 0.1488 |    2504 B |       16.47 |
| RoundTrip_Result                | .NET 9.0  | .NET 9.0  | 2,511.14 ns |  9.28 | 0.1488 |    2504 B |       16.47 |
| RoundTrip_Result                | .NET 8.0  | .NET 8.0  | 2,705.36 ns |  9.99 | 0.1488 |    2504 B |       16.47 |
