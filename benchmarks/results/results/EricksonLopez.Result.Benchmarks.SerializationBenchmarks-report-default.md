
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.63GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.33 (8.0.33, 8.0.3326.52219), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.22 (9.0.22, 9.0.2226.52218), X64 RyuJIT x86-64-v4


 Method                          | Job       | Runtime   | Mean        | Ratio | Gen0   | Allocated | Alloc Ratio |
-------------------------------- |---------- |---------- |------------:|------:|-------:|----------:|------------:|
 Serialize_Result_Success        | .NET 10.0 | .NET 10.0 |    96.26 ns |  0.35 | 0.0038 |      64 B |        0.42 |
 Serialize_Result_Success        | .NET 8.0  | .NET 8.0  |   120.30 ns |  0.44 | 0.0038 |      64 B |        0.42 |
 Serialize_Result_Success        | .NET 9.0  | .NET 9.0  |   128.27 ns |  0.47 | 0.0038 |      64 B |        0.42 |
 Serialize_ResultOfT_Success     | .NET 10.0 | .NET 10.0 |   159.97 ns |  0.59 | 0.0043 |      72 B |        0.47 |
 Serialize_Error_NoMetadata      | .NET 10.0 | .NET 10.0 |   196.51 ns |  0.72 | 0.0091 |     152 B |        1.00 |
 Serialize_ResultOfT_Success     | .NET 8.0  | .NET 8.0  |   198.21 ns |  0.73 | 0.0043 |      72 B |        0.47 |
 Serialize_ResultOfT_Success     | .NET 9.0  | .NET 9.0  |   207.78 ns |  0.77 | 0.0043 |      72 B |        0.47 |
 Serialize_Error_NoMetadata      | .NET 9.0  | .NET 9.0  |   255.66 ns |  0.94 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_NoMetadata      | .NET 8.0  | .NET 8.0  |   271.16 ns |  1.00 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_StringMetadata  | .NET 10.0 | .NET 10.0 |   433.96 ns |  1.60 | 0.0224 |     376 B |        2.47 |
 Serialize_Error_InnerErrors     | .NET 10.0 | .NET 10.0 |   510.28 ns |  1.88 | 0.0238 |     408 B |        2.68 |
 Serialize_Error_StringMetadata  | .NET 9.0  | .NET 9.0  |   525.24 ns |  1.94 | 0.0219 |     376 B |        2.47 |
 Serialize_Error_StringMetadata  | .NET 8.0  | .NET 8.0  |   567.36 ns |  2.09 | 0.0219 |     376 B |        2.47 |
 Serialize_Error_MixedMetadata   | .NET 10.0 | .NET 10.0 |   617.92 ns |  2.28 | 0.0305 |     512 B |        3.37 |
 Serialize_Error_InnerErrors     | .NET 9.0  | .NET 9.0  |   661.22 ns |  2.44 | 0.0238 |     408 B |        2.68 |
 Serialize_ResultOfT_Failure     | .NET 10.0 | .NET 10.0 |   662.15 ns |  2.44 | 0.0334 |     560 B |        3.68 |
 Serialize_Error_InnerErrors     | .NET 8.0  | .NET 8.0  |   693.47 ns |  2.56 | 0.0238 |     408 B |        2.68 |
 Serialize_Result_Failure        | .NET 10.0 | .NET 10.0 |   694.43 ns |  2.56 | 0.0334 |     560 B |        3.68 |
 Serialize_Error_MixedMetadata   | .NET 9.0  | .NET 9.0  |   731.95 ns |  2.70 | 0.0305 |     512 B |        3.37 |
 Serialize_Error_MixedMetadata   | .NET 8.0  | .NET 8.0  |   786.54 ns |  2.90 | 0.0305 |     512 B |        3.37 |
 Serialize_Result_Failure        | .NET 9.0  | .NET 9.0  |   803.48 ns |  2.96 | 0.0334 |     560 B |        3.68 |
 Serialize_ResultOfT_Failure     | .NET 9.0  | .NET 9.0  |   808.36 ns |  2.98 | 0.0334 |     560 B |        3.68 |
 Serialize_Result_Failure        | .NET 8.0  | .NET 8.0  |   843.54 ns |  3.11 | 0.0334 |     560 B |        3.68 |
 Serialize_ResultOfT_Failure     | .NET 8.0  | .NET 8.0  |   862.06 ns |  3.18 | 0.0334 |     560 B |        3.68 |
 Deserialize_Error_MixedMetadata | .NET 10.0 | .NET 10.0 | 1,021.08 ns |  3.77 | 0.1087 |    1832 B |       12.05 |
 Deserialize_Error_MixedMetadata | .NET 9.0  | .NET 9.0  | 1,362.17 ns |  5.02 | 0.1087 |    1832 B |       12.05 |
 Deserialize_Error_MixedMetadata | .NET 8.0  | .NET 8.0  | 1,481.58 ns |  5.46 | 0.1087 |    1832 B |       12.05 |
 RoundTrip_Result                | .NET 10.0 | .NET 10.0 | 2,071.39 ns |  7.64 | 0.1488 |    2504 B |       16.47 |
 RoundTrip_Result                | .NET 9.0  | .NET 9.0  | 2,545.56 ns |  9.39 | 0.1488 |    2504 B |       16.47 |
 RoundTrip_Result                | .NET 8.0  | .NET 8.0  | 2,777.34 ns | 10.24 | 0.1488 |    2504 B |       16.47 |
