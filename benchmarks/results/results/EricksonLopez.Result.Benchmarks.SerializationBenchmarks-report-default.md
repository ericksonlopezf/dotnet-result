
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.4 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.400
  [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.19 (9.0.19, 9.0.1926.36724), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v3


 Method                          | Job       | Runtime   | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
-------------------------------- |---------- |---------- |-----------:|------:|-------:|----------:|------------:|
 Serialize_Result_Success        | .NET 10.0 | .NET 10.0 |   135.1 ns |  0.39 | 0.0038 |      64 B |        0.42 |
 Serialize_Result_Success        | .NET 9.0  | .NET 9.0  |   148.2 ns |  0.43 | 0.0038 |      64 B |        0.42 |
 Serialize_Result_Success        | .NET 8.0  | .NET 8.0  |   166.4 ns |  0.48 | 0.0038 |      64 B |        0.42 |
 Serialize_ResultOfT_Success     | .NET 10.0 | .NET 10.0 |   197.1 ns |  0.57 | 0.0043 |      72 B |        0.47 |
 Serialize_ResultOfT_Success     | .NET 8.0  | .NET 8.0  |   262.7 ns |  0.75 | 0.0043 |      72 B |        0.47 |
 Serialize_ResultOfT_Success     | .NET 9.0  | .NET 9.0  |   264.0 ns |  0.76 | 0.0043 |      72 B |        0.47 |
 Serialize_Error_NoMetadata      | .NET 10.0 | .NET 10.0 |   266.4 ns |  0.76 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_NoMetadata      | .NET 9.0  | .NET 9.0  |   342.6 ns |  0.98 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_NoMetadata      | .NET 8.0  | .NET 8.0  |   348.4 ns |  1.00 | 0.0091 |     152 B |        1.00 |
 Serialize_Error_StringMetadata  | .NET 10.0 | .NET 10.0 |   599.3 ns |  1.72 | 0.0219 |     376 B |        2.47 |
 Serialize_Error_InnerErrors     | .NET 10.0 | .NET 10.0 |   695.5 ns |  2.00 | 0.0238 |     408 B |        2.68 |
 Serialize_Error_StringMetadata  | .NET 9.0  | .NET 9.0  |   704.0 ns |  2.02 | 0.0219 |     376 B |        2.47 |
 Serialize_Error_StringMetadata  | .NET 8.0  | .NET 8.0  |   740.4 ns |  2.13 | 0.0219 |     376 B |        2.47 |
 Serialize_Error_MixedMetadata   | .NET 10.0 | .NET 10.0 |   833.3 ns |  2.39 | 0.0305 |     512 B |        3.37 |
 Serialize_Error_InnerErrors     | .NET 8.0  | .NET 8.0  |   893.6 ns |  2.56 | 0.0238 |     408 B |        2.68 |
 Serialize_Error_InnerErrors     | .NET 9.0  | .NET 9.0  |   901.1 ns |  2.59 | 0.0238 |     408 B |        2.68 |
 Serialize_Result_Failure        | .NET 10.0 | .NET 10.0 |   935.8 ns |  2.69 | 0.0334 |     560 B |        3.68 |
 Serialize_ResultOfT_Failure     | .NET 10.0 | .NET 10.0 |   949.0 ns |  2.72 | 0.0324 |     560 B |        3.68 |
 Serialize_Error_MixedMetadata   | .NET 9.0  | .NET 9.0  | 1,038.3 ns |  2.98 | 0.0305 |     512 B |        3.37 |
 Serialize_Error_MixedMetadata   | .NET 8.0  | .NET 8.0  | 1,055.8 ns |  3.03 | 0.0305 |     512 B |        3.37 |
 Serialize_ResultOfT_Failure     | .NET 9.0  | .NET 9.0  | 1,082.4 ns |  3.11 | 0.0324 |     560 B |        3.68 |
 Serialize_Result_Failure        | .NET 9.0  | .NET 9.0  | 1,101.0 ns |  3.16 | 0.0324 |     560 B |        3.68 |
 Serialize_Result_Failure        | .NET 8.0  | .NET 8.0  | 1,137.6 ns |  3.27 | 0.0324 |     560 B |        3.68 |
 Serialize_ResultOfT_Failure     | .NET 8.0  | .NET 8.0  | 1,155.4 ns |  3.32 | 0.0324 |     560 B |        3.68 |
 Deserialize_Error_MixedMetadata | .NET 9.0  | .NET 9.0  | 1,827.1 ns |  5.24 | 0.1087 |    1832 B |       12.05 |
 Deserialize_Error_MixedMetadata | .NET 10.0 | .NET 10.0 | 1,837.6 ns |  5.27 | 0.1087 |    1832 B |       12.05 |
 Deserialize_Error_MixedMetadata | .NET 8.0  | .NET 8.0  | 1,892.8 ns |  5.43 | 0.1087 |    1832 B |       12.05 |
 RoundTrip_Result                | .NET 10.0 | .NET 10.0 | 2,661.3 ns |  7.64 | 0.1488 |    2504 B |       16.47 |
 RoundTrip_Result                | .NET 9.0  | .NET 9.0  | 3,198.5 ns |  9.18 | 0.1488 |    2504 B |       16.47 |
 RoundTrip_Result                | .NET 8.0  | .NET 8.0  | 3,338.5 ns |  9.58 | 0.1488 |    2504 B |       16.47 |
