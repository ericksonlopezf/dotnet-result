
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.21 (9.0.21, 9.0.2126.50404), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.32 (8.0.32, 8.0.3226.50405), X64 RyuJIT x86-64-v3


 Method                          | Job       | Runtime   | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
-------------------------------- |---------- |---------- |-----------:|------:|-------:|----------:|------------:|
 Serialize_Result_Success        | .NET 10.0 | .NET 10.0 |   134.3 ns |  0.39 | 0.0038 |      64 B |        0.42 |
 Serialize_Result_Success        | .NET 9.0  | .NET 9.0  |   150.3 ns |  0.43 | 0.0038 |      64 B |        0.42 |
 Serialize_Result_Success        | .NET 8.0  | .NET 8.0  |   157.0 ns |  0.45 | 0.0038 |      64 B |        0.42 |
 Serialize_ResultOfT_Success     | .NET 10.0 | .NET 10.0 |   228.5 ns |  0.66 | 0.0043 |      72 B |        0.47 |
 Serialize_ResultOfT_Success     | .NET 8.0  | .NET 8.0  |   253.1 ns |  0.73 | 0.0043 |      72 B |        0.47 |
 Serialize_ResultOfT_Success     | .NET 9.0  | .NET 9.0  |   254.3 ns |  0.74 | 0.0043 |      72 B |        0.47 |
 Serialize_Error_NoMetadata      | .NET 10.0 | .NET 10.0 |   267.7 ns |  0.77 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_NoMetadata      | .NET 9.0  | .NET 9.0  |   337.3 ns |  0.97 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_NoMetadata      | .NET 8.0  | .NET 8.0  |   346.0 ns |  1.00 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_StringMetadata  | .NET 10.0 | .NET 10.0 |   614.9 ns |  1.78 | 0.0219 |     376 B |        2.47 |
 Serialize_Error_InnerErrors     | .NET 10.0 | .NET 10.0 |   695.0 ns |  2.01 | 0.0238 |     408 B |        2.68 |
 Serialize_Error_StringMetadata  | .NET 9.0  | .NET 9.0  |   697.3 ns |  2.02 | 0.0219 |     376 B |        2.47 |
 Serialize_Error_StringMetadata  | .NET 8.0  | .NET 8.0  |   747.2 ns |  2.16 | 0.0219 |     376 B |        2.47 |
 Serialize_Error_MixedMetadata   | .NET 10.0 | .NET 10.0 |   855.7 ns |  2.47 | 0.0305 |     512 B |        3.37 |
 Serialize_Error_InnerErrors     | .NET 9.0  | .NET 9.0  |   895.2 ns |  2.59 | 0.0238 |     408 B |        2.68 |
 Serialize_Error_InnerErrors     | .NET 8.0  | .NET 8.0  |   900.7 ns |  2.60 | 0.0238 |     408 B |        2.68 |
 Serialize_ResultOfT_Failure     | .NET 10.0 | .NET 10.0 |   932.2 ns |  2.69 | 0.0334 |     560 B |        3.68 |
 Serialize_Result_Failure        | .NET 10.0 | .NET 10.0 |   937.9 ns |  2.71 | 0.0334 |     560 B |        3.68 |
 Serialize_Error_MixedMetadata   | .NET 9.0  | .NET 9.0  |   986.0 ns |  2.85 | 0.0305 |     512 B |        3.37 |
 Serialize_Error_MixedMetadata   | .NET 8.0  | .NET 8.0  | 1,069.3 ns |  3.09 | 0.0305 |     512 B |        3.37 |
 Serialize_Result_Failure        | .NET 9.0  | .NET 9.0  | 1,085.9 ns |  3.14 | 0.0324 |     560 B |        3.68 |
 Serialize_ResultOfT_Failure     | .NET 9.0  | .NET 9.0  | 1,086.9 ns |  3.14 | 0.0324 |     560 B |        3.68 |
 Serialize_Result_Failure        | .NET 8.0  | .NET 8.0  | 1,159.5 ns |  3.35 | 0.0324 |     560 B |        3.68 |
 Serialize_ResultOfT_Failure     | .NET 8.0  | .NET 8.0  | 1,164.0 ns |  3.36 | 0.0324 |     560 B |        3.68 |
 Deserialize_Error_MixedMetadata | .NET 10.0 | .NET 10.0 | 1,364.2 ns |  3.94 | 0.1087 |    1832 B |       12.05 |
 Deserialize_Error_MixedMetadata | .NET 9.0  | .NET 9.0  | 1,763.1 ns |  5.10 | 0.1087 |    1832 B |       12.05 |
 Deserialize_Error_MixedMetadata | .NET 8.0  | .NET 8.0  | 1,868.1 ns |  5.40 | 0.1087 |    1832 B |       12.05 |
 RoundTrip_Result                | .NET 10.0 | .NET 10.0 | 2,645.0 ns |  7.65 | 0.1488 |    2504 B |       16.47 |
 RoundTrip_Result                | .NET 9.0  | .NET 9.0  | 3,200.0 ns |  9.25 | 0.1488 |    2504 B |       16.47 |
 RoundTrip_Result                | .NET 8.0  | .NET 8.0  | 3,397.2 ns |  9.82 | 0.1488 |    2504 B |       16.47 |
