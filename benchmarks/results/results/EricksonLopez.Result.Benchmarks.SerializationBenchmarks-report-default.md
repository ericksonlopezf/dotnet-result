
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.21GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4


 Method                          | Job       | Runtime   | Mean        | Ratio | Gen0   | Allocated | Alloc Ratio |
-------------------------------- |---------- |---------- |------------:|------:|-------:|----------:|------------:|
 Serialize_Result_Success        | .NET 10.0 | .NET 10.0 |    63.13 ns |  0.36 | 0.0038 |      64 B |        0.42 |
 Serialize_Result_Success        | .NET 9.0  | .NET 9.0  |    73.81 ns |  0.42 | 0.0038 |      64 B |        0.42 |
 Serialize_Result_Success        | .NET 8.0  | .NET 8.0  |    79.55 ns |  0.45 | 0.0038 |      64 B |        0.42 |
 Serialize_ResultOfT_Success     | .NET 10.0 | .NET 10.0 |    99.78 ns |  0.57 | 0.0043 |      72 B |        0.47 |
 Serialize_Error_NoMetadata      | .NET 10.0 | .NET 10.0 |   125.14 ns |  0.71 | 0.0091 |     152 B |        1.00 |
 Serialize_ResultOfT_Success     | .NET 8.0  | .NET 8.0  |   136.72 ns |  0.78 | 0.0043 |      72 B |        0.47 |
 Serialize_ResultOfT_Success     | .NET 9.0  | .NET 9.0  |   139.78 ns |  0.80 | 0.0043 |      72 B |        0.47 |
 Serialize_Error_NoMetadata      | .NET 9.0  | .NET 9.0  |   173.65 ns |  0.99 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_NoMetadata      | .NET 8.0  | .NET 8.0  |   175.31 ns |  1.00 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_StringMetadata  | .NET 10.0 | .NET 10.0 |   297.48 ns |  1.70 | 0.0224 |     376 B |        2.47 |
 Serialize_Error_InnerErrors     | .NET 10.0 | .NET 10.0 |   340.24 ns |  1.94 | 0.0243 |     408 B |        2.68 |
 Serialize_Error_StringMetadata  | .NET 9.0  | .NET 9.0  |   364.18 ns |  2.08 | 0.0224 |     376 B |        2.47 |
 Serialize_Error_StringMetadata  | .NET 8.0  | .NET 8.0  |   372.97 ns |  2.13 | 0.0224 |     376 B |        2.47 |
 Serialize_Error_MixedMetadata   | .NET 10.0 | .NET 10.0 |   401.77 ns |  2.29 | 0.0305 |     512 B |        3.37 |
 Serialize_Error_InnerErrors     | .NET 8.0  | .NET 8.0  |   456.30 ns |  2.60 | 0.0243 |     408 B |        2.68 |
 Serialize_Error_InnerErrors     | .NET 9.0  | .NET 9.0  |   471.16 ns |  2.69 | 0.0243 |     408 B |        2.68 |
 Serialize_ResultOfT_Failure     | .NET 10.0 | .NET 10.0 |   477.86 ns |  2.73 | 0.0334 |     560 B |        3.68 |
 Serialize_Result_Failure        | .NET 10.0 | .NET 10.0 |   505.62 ns |  2.88 | 0.0334 |     560 B |        3.68 |
 Serialize_Error_MixedMetadata   | .NET 9.0  | .NET 9.0  |   515.72 ns |  2.94 | 0.0305 |     512 B |        3.37 |
 Serialize_Error_MixedMetadata   | .NET 8.0  | .NET 8.0  |   565.23 ns |  3.22 | 0.0305 |     512 B |        3.37 |
 Serialize_ResultOfT_Failure     | .NET 9.0  | .NET 9.0  |   588.24 ns |  3.36 | 0.0334 |     560 B |        3.68 |
 Serialize_Result_Failure        | .NET 9.0  | .NET 9.0  |   590.07 ns |  3.37 | 0.0334 |     560 B |        3.68 |
 Serialize_Result_Failure        | .NET 8.0  | .NET 8.0  |   644.21 ns |  3.67 | 0.0334 |     560 B |        3.68 |
 Serialize_ResultOfT_Failure     | .NET 8.0  | .NET 8.0  |   650.86 ns |  3.71 | 0.0334 |     560 B |        3.68 |
 Deserialize_Error_MixedMetadata | .NET 10.0 | .NET 10.0 |   769.06 ns |  4.39 | 0.1087 |    1832 B |       12.05 |
 Deserialize_Error_MixedMetadata | .NET 9.0  | .NET 9.0  | 1,031.39 ns |  5.88 | 0.1087 |    1832 B |       12.05 |
 Deserialize_Error_MixedMetadata | .NET 8.0  | .NET 8.0  | 1,047.44 ns |  5.98 | 0.1087 |    1832 B |       12.05 |
 RoundTrip_Result                | .NET 10.0 | .NET 10.0 | 1,531.56 ns |  8.74 | 0.1488 |    2504 B |       16.47 |
 RoundTrip_Result                | .NET 9.0  | .NET 9.0  | 1,853.97 ns | 10.58 | 0.1488 |    2504 B |       16.47 |
 RoundTrip_Result                | .NET 8.0  | .NET 8.0  | 2,056.89 ns | 11.73 | 0.1488 |    2504 B |       16.47 |
