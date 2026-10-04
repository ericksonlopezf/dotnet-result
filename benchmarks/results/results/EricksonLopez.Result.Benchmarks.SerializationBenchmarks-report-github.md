```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v3


```
| Method                          | Job       | Runtime   | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |---------- |---------- |-----------:|------:|-------:|----------:|------------:|
| Serialize_Result_Success        | .NET 10.0 | .NET 10.0 |   125.9 ns |  0.37 | 0.0038 |      64 B |        0.42 |
| Serialize_Result_Success        | .NET 9.0  | .NET 9.0  |   143.8 ns |  0.42 | 0.0038 |      64 B |        0.42 |
| Serialize_Result_Success        | .NET 8.0  | .NET 8.0  |   163.4 ns |  0.48 | 0.0038 |      64 B |        0.42 |
| Serialize_ResultOfT_Success     | .NET 10.0 | .NET 10.0 |   204.3 ns |  0.60 | 0.0043 |      72 B |        0.47 |
| Serialize_ResultOfT_Success     | .NET 9.0  | .NET 9.0  |   249.8 ns |  0.73 | 0.0043 |      72 B |        0.47 |
| Serialize_Error_NoMetadata      | .NET 10.0 | .NET 10.0 |   250.4 ns |  0.73 | 0.0091 |     152 B |        1.00 |
| Serialize_ResultOfT_Success     | .NET 8.0  | .NET 8.0  |   267.1 ns |  0.78 | 0.0043 |      72 B |        0.47 |
| Serialize_Error_NoMetadata      | .NET 9.0  | .NET 9.0  |   327.5 ns |  0.96 | 0.0091 |     152 B |        1.00 |
| Serialize_Error_NoMetadata      | .NET 8.0  | .NET 8.0  |   341.2 ns |  1.00 | 0.0091 |     152 B |        1.00 |
| Serialize_Error_StringMetadata  | .NET 10.0 | .NET 10.0 |   554.9 ns |  1.63 | 0.0219 |     376 B |        2.47 |
| Serialize_Error_InnerErrors     | .NET 10.0 | .NET 10.0 |   649.4 ns |  1.90 | 0.0238 |     408 B |        2.68 |
| Serialize_Error_StringMetadata  | .NET 9.0  | .NET 9.0  |   665.0 ns |  1.95 | 0.0219 |     376 B |        2.47 |
| Serialize_Error_StringMetadata  | .NET 8.0  | .NET 8.0  |   724.9 ns |  2.12 | 0.0219 |     376 B |        2.47 |
| Serialize_Error_MixedMetadata   | .NET 10.0 | .NET 10.0 |   768.3 ns |  2.25 | 0.0305 |     512 B |        3.37 |
| Serialize_Error_InnerErrors     | .NET 9.0  | .NET 9.0  |   843.3 ns |  2.47 | 0.0229 |     408 B |        2.68 |
| Serialize_Result_Failure        | .NET 10.0 | .NET 10.0 |   853.1 ns |  2.50 | 0.0334 |     560 B |        3.68 |
| Serialize_ResultOfT_Failure     | .NET 10.0 | .NET 10.0 |   887.6 ns |  2.60 | 0.0334 |     560 B |        3.68 |
| Serialize_Error_InnerErrors     | .NET 8.0  | .NET 8.0  |   893.6 ns |  2.62 | 0.0238 |     408 B |        2.68 |
| Serialize_Error_MixedMetadata   | .NET 9.0  | .NET 9.0  |   938.0 ns |  2.75 | 0.0305 |     512 B |        3.37 |
| Serialize_Error_MixedMetadata   | .NET 8.0  | .NET 8.0  | 1,003.4 ns |  2.94 | 0.0305 |     512 B |        3.37 |
| Serialize_ResultOfT_Failure     | .NET 9.0  | .NET 9.0  | 1,044.5 ns |  3.06 | 0.0324 |     560 B |        3.68 |
| Serialize_Result_Failure        | .NET 9.0  | .NET 9.0  | 1,050.6 ns |  3.08 | 0.0324 |     560 B |        3.68 |
| Serialize_Result_Failure        | .NET 8.0  | .NET 8.0  | 1,075.6 ns |  3.15 | 0.0324 |     560 B |        3.68 |
| Serialize_ResultOfT_Failure     | .NET 8.0  | .NET 8.0  | 1,076.5 ns |  3.16 | 0.0324 |     560 B |        3.68 |
| Deserialize_Error_MixedMetadata | .NET 10.0 | .NET 10.0 | 1,346.3 ns |  3.95 | 0.1087 |    1832 B |       12.05 |
| Deserialize_Error_MixedMetadata | .NET 9.0  | .NET 9.0  | 1,695.5 ns |  4.97 | 0.1087 |    1832 B |       12.05 |
| Deserialize_Error_MixedMetadata | .NET 8.0  | .NET 8.0  | 1,890.1 ns |  5.54 | 0.1087 |    1832 B |       12.05 |
| RoundTrip_Result                | .NET 10.0 | .NET 10.0 | 2,641.8 ns |  7.74 | 0.1488 |    2504 B |       16.47 |
| RoundTrip_Result                | .NET 9.0  | .NET 9.0  | 3,104.0 ns |  9.10 | 0.1488 |    2504 B |       16.47 |
| RoundTrip_Result                | .NET 8.0  | .NET 8.0  | 3,417.5 ns | 10.02 | 0.1488 |    2504 B |       16.47 |
