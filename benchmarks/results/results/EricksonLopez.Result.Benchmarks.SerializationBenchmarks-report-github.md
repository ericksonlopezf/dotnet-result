```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3


```
| Method                          | Job       | Runtime   | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |---------- |---------- |-----------:|------:|-------:|----------:|------------:|
| Serialize_Result_Success        | .NET 10.0 | .NET 10.0 |   138.6 ns |  0.40 | 0.0038 |      64 B |        0.42 |
| Serialize_Result_Success        | .NET 9.0  | .NET 9.0  |   148.6 ns |  0.42 | 0.0038 |      64 B |        0.42 |
| Serialize_Result_Success        | .NET 8.0  | .NET 8.0  |   163.0 ns |  0.46 | 0.0038 |      64 B |        0.42 |
| Serialize_ResultOfT_Success     | .NET 10.0 | .NET 10.0 |   206.0 ns |  0.59 | 0.0043 |      72 B |        0.47 |
| Serialize_ResultOfT_Success     | .NET 9.0  | .NET 9.0  |   250.5 ns |  0.71 | 0.0043 |      72 B |        0.47 |
| Serialize_ResultOfT_Success     | .NET 8.0  | .NET 8.0  |   258.1 ns |  0.74 | 0.0043 |      72 B |        0.47 |
| Serialize_Error_NoMetadata      | .NET 10.0 | .NET 10.0 |   259.9 ns |  0.74 | 0.0091 |     152 B |        1.00 |
| Serialize_Error_NoMetadata      | .NET 9.0  | .NET 9.0  |   345.8 ns |  0.99 | 0.0091 |     152 B |        1.00 |
| Serialize_Error_NoMetadata      | .NET 8.0  | .NET 8.0  |   350.7 ns |  1.00 | 0.0091 |     152 B |        1.00 |
| Serialize_Error_StringMetadata  | .NET 10.0 | .NET 10.0 |   602.5 ns |  1.72 | 0.0219 |     376 B |        2.47 |
| Serialize_Error_InnerErrors     | .NET 10.0 | .NET 10.0 |   712.7 ns |  2.03 | 0.0238 |     408 B |        2.68 |
| Serialize_Error_StringMetadata  | .NET 9.0  | .NET 9.0  |   719.1 ns |  2.05 | 0.0219 |     376 B |        2.47 |
| Serialize_Error_StringMetadata  | .NET 8.0  | .NET 8.0  |   745.4 ns |  2.13 | 0.0219 |     376 B |        2.47 |
| Serialize_Error_MixedMetadata   | .NET 10.0 | .NET 10.0 |   817.9 ns |  2.33 | 0.0305 |     512 B |        3.37 |
| Serialize_Error_InnerErrors     | .NET 8.0  | .NET 8.0  |   893.7 ns |  2.55 | 0.0238 |     408 B |        2.68 |
| Serialize_Error_InnerErrors     | .NET 9.0  | .NET 9.0  |   904.5 ns |  2.58 | 0.0238 |     408 B |        2.68 |
| Serialize_ResultOfT_Failure     | .NET 10.0 | .NET 10.0 |   925.5 ns |  2.64 | 0.0334 |     560 B |        3.68 |
| Serialize_Result_Failure        | .NET 10.0 | .NET 10.0 |   939.4 ns |  2.68 | 0.0334 |     560 B |        3.68 |
| Serialize_Error_MixedMetadata   | .NET 9.0  | .NET 9.0  |   970.2 ns |  2.77 | 0.0305 |     512 B |        3.37 |
| Serialize_Result_Failure        | .NET 9.0  | .NET 9.0  | 1,059.0 ns |  3.02 | 0.0324 |     560 B |        3.68 |
| Serialize_ResultOfT_Failure     | .NET 9.0  | .NET 9.0  | 1,060.9 ns |  3.03 | 0.0324 |     560 B |        3.68 |
| Serialize_Error_MixedMetadata   | .NET 8.0  | .NET 8.0  | 1,082.9 ns |  3.09 | 0.0305 |     512 B |        3.37 |
| Serialize_Result_Failure        | .NET 8.0  | .NET 8.0  | 1,145.2 ns |  3.27 | 0.0324 |     560 B |        3.68 |
| Serialize_ResultOfT_Failure     | .NET 8.0  | .NET 8.0  | 1,192.9 ns |  3.40 | 0.0324 |     560 B |        3.68 |
| Deserialize_Error_MixedMetadata | .NET 10.0 | .NET 10.0 | 1,353.4 ns |  3.86 | 0.1087 |    1832 B |       12.05 |
| Deserialize_Error_MixedMetadata | .NET 9.0  | .NET 9.0  | 1,740.5 ns |  4.96 | 0.1087 |    1832 B |       12.05 |
| Deserialize_Error_MixedMetadata | .NET 8.0  | .NET 8.0  | 1,799.1 ns |  5.13 | 0.1087 |    1832 B |       12.05 |
| RoundTrip_Result                | .NET 10.0 | .NET 10.0 | 2,580.4 ns |  7.36 | 0.1488 |    2504 B |       16.47 |
| RoundTrip_Result                | .NET 9.0  | .NET 9.0  | 3,178.6 ns |  9.06 | 0.1488 |    2504 B |       16.47 |
| RoundTrip_Result                | .NET 8.0  | .NET 8.0  | 3,414.1 ns |  9.74 | 0.1488 |    2504 B |       16.47 |
