```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
INTEL XEON PLATINUM 8573C 2.30GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 9.0  : .NET 9.0.20 (9.0.20, 9.0.2026.41315), X64 RyuJIT x86-64-v4
  .NET 8.0  : .NET 8.0.31 (8.0.31, 8.0.3126.42015), X64 RyuJIT x86-64-v4


```
| Method                          | Job       | Runtime   | Mean        | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |---------- |---------- |------------:|------:|-------:|----------:|------------:|
| Serialize_Result_Success        | .NET 10.0 | .NET 10.0 |    89.50 ns |  0.38 | 0.0007 |      64 B |        0.42 |
| Serialize_Result_Success        | .NET 9.0  | .NET 9.0  |    91.77 ns |  0.39 | 0.0007 |      64 B |        0.42 |
| Serialize_Result_Success        | .NET 8.0  | .NET 8.0  |   108.79 ns |  0.46 | 0.0007 |      64 B |        0.42 |
| Serialize_ResultOfT_Success     | .NET 10.0 | .NET 10.0 |   137.48 ns |  0.58 | 0.0007 |      72 B |        0.47 |
| Serialize_ResultOfT_Success     | .NET 9.0  | .NET 9.0  |   171.52 ns |  0.73 | 0.0007 |      72 B |        0.47 |
| Serialize_ResultOfT_Success     | .NET 8.0  | .NET 8.0  |   174.08 ns |  0.74 | 0.0007 |      72 B |        0.47 |
| Serialize_Error_NoMetadata      | .NET 10.0 | .NET 10.0 |   182.09 ns |  0.77 | 0.0017 |     152 B |        1.00 |
| Serialize_Error_NoMetadata      | .NET 9.0  | .NET 9.0  |   229.52 ns |  0.97 | 0.0017 |     152 B |        1.00 |
| Serialize_Error_NoMetadata      | .NET 8.0  | .NET 8.0  |   236.23 ns |  1.00 | 0.0014 |     152 B |        1.00 |
| Serialize_Error_InnerErrors     | .NET 10.0 | .NET 10.0 |   482.89 ns |  2.04 | 0.0048 |     408 B |        2.68 |
| Serialize_Error_StringMetadata  | .NET 8.0  | .NET 8.0  |   547.30 ns |  2.32 | 0.0038 |     376 B |        2.47 |
| Serialize_Error_InnerErrors     | .NET 8.0  | .NET 8.0  |   601.93 ns |  2.55 | 0.0048 |     408 B |        2.68 |
| Serialize_Error_InnerErrors     | .NET 9.0  | .NET 9.0  |   609.10 ns |  2.58 | 0.0048 |     408 B |        2.68 |
| Serialize_Error_MixedMetadata   | .NET 8.0  | .NET 8.0  |   763.79 ns |  3.23 | 0.0057 |     512 B |        3.37 |
| Serialize_ResultOfT_Failure     | .NET 8.0  | .NET 8.0  |   862.27 ns |  3.65 | 0.0067 |     560 B |        3.68 |
| Serialize_Result_Failure        | .NET 8.0  | .NET 8.0  |   866.85 ns |  3.67 | 0.0067 |     560 B |        3.68 |
| Serialize_Error_StringMetadata  | .NET 10.0 | .NET 10.0 |   960.47 ns |  4.07 | 0.0038 |     376 B |        2.47 |
| Serialize_Error_StringMetadata  | .NET 9.0  | .NET 9.0  | 1,023.66 ns |  4.33 | 0.0038 |     376 B |        2.47 |
| Deserialize_Error_MixedMetadata | .NET 10.0 | .NET 10.0 | 1,082.35 ns |  4.58 | 0.0210 |    1832 B |       12.05 |
| Serialize_Error_MixedMetadata   | .NET 10.0 | .NET 10.0 | 1,409.78 ns |  5.97 | 0.0057 |     512 B |        3.37 |
| Deserialize_Error_MixedMetadata | .NET 9.0  | .NET 9.0  | 1,424.82 ns |  6.03 | 0.0210 |    1832 B |       12.05 |
| Deserialize_Error_MixedMetadata | .NET 8.0  | .NET 8.0  | 1,440.37 ns |  6.10 | 0.0210 |    1832 B |       12.05 |
| Serialize_ResultOfT_Failure     | .NET 10.0 | .NET 10.0 | 1,478.09 ns |  6.26 | 0.0057 |     560 B |        3.68 |
| Serialize_Result_Failure        | .NET 10.0 | .NET 10.0 | 1,479.17 ns |  6.26 | 0.0057 |     560 B |        3.68 |
| Serialize_Error_MixedMetadata   | .NET 9.0  | .NET 9.0  | 1,504.88 ns |  6.37 | 0.0057 |     512 B |        3.37 |
| Serialize_Result_Failure        | .NET 9.0  | .NET 9.0  | 1,543.80 ns |  6.54 | 0.0057 |     560 B |        3.68 |
| Serialize_ResultOfT_Failure     | .NET 9.0  | .NET 9.0  | 1,592.48 ns |  6.74 | 0.0057 |     560 B |        3.68 |
| RoundTrip_Result                | .NET 8.0  | .NET 8.0  | 2,740.66 ns | 11.60 | 0.0267 |    2504 B |       16.47 |
| RoundTrip_Result                | .NET 10.0 | .NET 10.0 | 2,893.25 ns | 12.25 | 0.0267 |    2504 B |       16.47 |
| RoundTrip_Result                | .NET 9.0  | .NET 9.0  | 3,434.94 ns | 14.54 | 0.0267 |    2504 B |       16.47 |
