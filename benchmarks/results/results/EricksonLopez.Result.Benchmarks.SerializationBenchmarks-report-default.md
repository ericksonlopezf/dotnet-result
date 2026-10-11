
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.37GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4


 Method                          | Job       | Runtime   | Mean        | Median      | Ratio | Gen0   | Allocated | Alloc Ratio |
-------------------------------- |---------- |---------- |------------:|------------:|------:|-------:|----------:|------------:|
 Serialize_Result_Success        | .NET 10.0 | .NET 10.0 |    63.87 ns |    63.83 ns |  0.38 | 0.0038 |      64 B |        0.42 |
 Serialize_Result_Success        | .NET 9.0  | .NET 9.0  |    70.62 ns |    70.34 ns |  0.42 | 0.0038 |      64 B |        0.42 |
 Serialize_Result_Success        | .NET 8.0  | .NET 8.0  |    77.74 ns |    76.16 ns |  0.46 | 0.0038 |      64 B |        0.42 |
 Serialize_ResultOfT_Success     | .NET 10.0 | .NET 10.0 |    96.24 ns |    96.46 ns |  0.57 | 0.0043 |      72 B |        0.47 |
 Serialize_Error_NoMetadata      | .NET 10.0 | .NET 10.0 |   121.44 ns |   121.63 ns |  0.72 | 0.0091 |     152 B |        1.00 |
 Serialize_ResultOfT_Success     | .NET 8.0  | .NET 8.0  |   129.28 ns |   129.05 ns |  0.77 | 0.0043 |      72 B |        0.47 |
 Serialize_ResultOfT_Success     | .NET 9.0  | .NET 9.0  |   130.10 ns |   130.31 ns |  0.77 | 0.0043 |      72 B |        0.47 |
 Serialize_Error_NoMetadata      | .NET 9.0  | .NET 9.0  |   157.16 ns |   157.06 ns |  0.94 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_NoMetadata      | .NET 8.0  | .NET 8.0  |   168.00 ns |   167.52 ns |  1.00 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_StringMetadata  | .NET 10.0 | .NET 10.0 |   288.42 ns |   285.66 ns |  1.72 | 0.0224 |     376 B |        2.47 |
 Serialize_Error_InnerErrors     | .NET 10.0 | .NET 10.0 |   333.48 ns |   333.24 ns |  1.99 | 0.0243 |     408 B |        2.68 |
 Serialize_Error_StringMetadata  | .NET 9.0  | .NET 9.0  |   344.74 ns |   341.63 ns |  2.05 | 0.0224 |     376 B |        2.47 |
 Serialize_Error_StringMetadata  | .NET 8.0  | .NET 8.0  |   374.49 ns |   374.07 ns |  2.23 | 0.0224 |     376 B |        2.47 |
 Serialize_Error_MixedMetadata   | .NET 10.0 | .NET 10.0 |   402.75 ns |   402.38 ns |  2.40 | 0.0305 |     512 B |        3.37 |
 Serialize_Error_InnerErrors     | .NET 9.0  | .NET 9.0  |   432.03 ns |   430.24 ns |  2.57 | 0.0243 |     408 B |        2.68 |
 Serialize_Error_InnerErrors     | .NET 8.0  | .NET 8.0  |   448.20 ns |   449.53 ns |  2.67 | 0.0243 |     408 B |        2.68 |
 Serialize_ResultOfT_Failure     | .NET 10.0 | .NET 10.0 |   477.75 ns |   478.71 ns |  2.84 | 0.0334 |     560 B |        3.68 |
 Serialize_Error_MixedMetadata   | .NET 9.0  | .NET 9.0  |   510.25 ns |   506.96 ns |  3.04 | 0.0305 |     512 B |        3.37 |
 Serialize_Result_Failure        | .NET 10.0 | .NET 10.0 |   515.54 ns |   517.01 ns |  3.07 | 0.0334 |     560 B |        3.68 |
 Serialize_Error_MixedMetadata   | .NET 8.0  | .NET 8.0  |   542.71 ns |   538.32 ns |  3.23 | 0.0305 |     512 B |        3.37 |
 Serialize_Result_Failure        | .NET 9.0  | .NET 9.0  |   564.61 ns |   566.52 ns |  3.36 | 0.0334 |     560 B |        3.68 |
 Serialize_Result_Failure        | .NET 8.0  | .NET 8.0  |   569.48 ns |   568.70 ns |  3.39 | 0.0334 |     560 B |        3.68 |
 Serialize_ResultOfT_Failure     | .NET 9.0  | .NET 9.0  |   615.14 ns |   614.07 ns |  3.66 | 0.0334 |     560 B |        3.68 |
 Serialize_ResultOfT_Failure     | .NET 8.0  | .NET 8.0  |   618.87 ns |   618.45 ns |  3.68 | 0.0334 |     560 B |        3.68 |
 Deserialize_Error_MixedMetadata | .NET 10.0 | .NET 10.0 |   753.22 ns |   754.50 ns |  4.48 | 0.1087 |    1832 B |       12.05 |
 Deserialize_Error_MixedMetadata | .NET 9.0  | .NET 9.0  |   918.40 ns |   917.33 ns |  5.47 | 0.1087 |    1832 B |       12.05 |
 Deserialize_Error_MixedMetadata | .NET 8.0  | .NET 8.0  | 1,010.53 ns | 1,010.82 ns |  6.02 | 0.1087 |    1832 B |       12.05 |
 RoundTrip_Result                | .NET 10.0 | .NET 10.0 | 1,480.49 ns | 1,478.09 ns |  8.81 | 0.1488 |    2504 B |       16.47 |
 RoundTrip_Result                | .NET 9.0  | .NET 9.0  | 1,759.48 ns | 1,751.94 ns | 10.47 | 0.1488 |    2504 B |       16.47 |
 RoundTrip_Result                | .NET 8.0  | .NET 8.0  | 1,877.54 ns | 1,887.17 ns | 11.18 | 0.1488 |    2504 B |       16.47 |
