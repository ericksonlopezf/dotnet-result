
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.68GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v4


 Method                          | Job       | Runtime   | Mean        | Ratio | Gen0   | Allocated | Alloc Ratio |
-------------------------------- |---------- |---------- |------------:|------:|-------:|----------:|------------:|
 Serialize_Result_Success        | .NET 10.0 | .NET 10.0 |    95.92 ns |  0.36 | 0.0038 |      64 B |        0.42 |
 Serialize_Result_Success        | .NET 9.0  | .NET 9.0  |   114.87 ns |  0.43 | 0.0038 |      64 B |        0.42 |
 Serialize_Result_Success        | .NET 8.0  | .NET 8.0  |   120.70 ns |  0.45 | 0.0038 |      64 B |        0.42 |
 Serialize_ResultOfT_Success     | .NET 10.0 | .NET 10.0 |   151.30 ns |  0.57 | 0.0043 |      72 B |        0.47 |
 Serialize_ResultOfT_Success     | .NET 9.0  | .NET 9.0  |   197.00 ns |  0.74 | 0.0043 |      72 B |        0.47 |
 Serialize_ResultOfT_Success     | .NET 8.0  | .NET 8.0  |   201.53 ns |  0.76 | 0.0043 |      72 B |        0.47 |
 Serialize_Error_NoMetadata      | .NET 10.0 | .NET 10.0 |   213.68 ns |  0.80 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_NoMetadata      | .NET 9.0  | .NET 9.0  |   251.15 ns |  0.94 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_NoMetadata      | .NET 8.0  | .NET 8.0  |   266.70 ns |  1.00 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_StringMetadata  | .NET 10.0 | .NET 10.0 |   426.90 ns |  1.60 | 0.0224 |     376 B |        2.47 |
 Serialize_Error_InnerErrors     | .NET 10.0 | .NET 10.0 |   509.06 ns |  1.91 | 0.0238 |     408 B |        2.68 |
 Serialize_Error_StringMetadata  | .NET 9.0  | .NET 9.0  |   529.58 ns |  1.99 | 0.0219 |     376 B |        2.47 |
 Serialize_Error_StringMetadata  | .NET 8.0  | .NET 8.0  |   560.10 ns |  2.10 | 0.0219 |     376 B |        2.47 |
 Serialize_Error_MixedMetadata   | .NET 10.0 | .NET 10.0 |   584.93 ns |  2.19 | 0.0305 |     512 B |        3.37 |
 Serialize_Error_InnerErrors     | .NET 9.0  | .NET 9.0  |   661.71 ns |  2.48 | 0.0238 |     408 B |        2.68 |
 Serialize_ResultOfT_Failure     | .NET 10.0 | .NET 10.0 |   673.72 ns |  2.53 | 0.0334 |     560 B |        3.68 |
 Serialize_Result_Failure        | .NET 10.0 | .NET 10.0 |   688.58 ns |  2.58 | 0.0334 |     560 B |        3.68 |
 Serialize_Error_InnerErrors     | .NET 8.0  | .NET 8.0  |   691.79 ns |  2.59 | 0.0238 |     408 B |        2.68 |
 Serialize_Error_MixedMetadata   | .NET 9.0  | .NET 9.0  |   716.50 ns |  2.69 | 0.0305 |     512 B |        3.37 |
 Serialize_Error_MixedMetadata   | .NET 8.0  | .NET 8.0  |   773.82 ns |  2.90 | 0.0305 |     512 B |        3.37 |
 Serialize_Result_Failure        | .NET 9.0  | .NET 9.0  |   813.78 ns |  3.05 | 0.0334 |     560 B |        3.68 |
 Serialize_ResultOfT_Failure     | .NET 9.0  | .NET 9.0  |   834.80 ns |  3.13 | 0.0334 |     560 B |        3.68 |
 Serialize_Result_Failure        | .NET 8.0  | .NET 8.0  |   841.38 ns |  3.15 | 0.0334 |     560 B |        3.68 |
 Serialize_ResultOfT_Failure     | .NET 8.0  | .NET 8.0  |   855.58 ns |  3.21 | 0.0334 |     560 B |        3.68 |
 Deserialize_Error_MixedMetadata | .NET 10.0 | .NET 10.0 | 1,012.94 ns |  3.80 | 0.1087 |    1832 B |       12.05 |
 Deserialize_Error_MixedMetadata | .NET 9.0  | .NET 9.0  | 1,387.29 ns |  5.20 | 0.1087 |    1832 B |       12.05 |
 Deserialize_Error_MixedMetadata | .NET 8.0  | .NET 8.0  | 1,444.33 ns |  5.42 | 0.1087 |    1832 B |       12.05 |
 RoundTrip_Result                | .NET 10.0 | .NET 10.0 | 2,046.45 ns |  7.67 | 0.1488 |    2504 B |       16.47 |
 RoundTrip_Result                | .NET 9.0  | .NET 9.0  | 2,483.59 ns |  9.31 | 0.1488 |    2504 B |       16.47 |
 RoundTrip_Result                | .NET 8.0  | .NET 8.0  | 2,708.09 ns | 10.15 | 0.1488 |    2504 B |       16.47 |
