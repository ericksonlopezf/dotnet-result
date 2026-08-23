// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using EricksonLopez.Result;
using EricksonLopez.Result.MediatR;
using MediatR;

namespace EricksonLopez.Result.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class MediatRColdStartBenchmarks
{
    private static readonly CancellationToken Token = CancellationToken.None;
    private static readonly Exception DummyEx = new InvalidOperationException("Test");
    private static readonly Error DummyError = Error.Unexpected("Test.Error", "msg");

    // We use a local class to simulate the behavior without actually invoking MediatR 
    // container resolutions, since we specifically want to benchmark the cold start
    // cost of static generic instantiation and Expression.Compile() inside ResultExceptionBehavior.
    //
    // However, since ResultExceptionBehavior caches statically per closed type,
    // if we benchmark standard execution, we only measure the cache *hit*.
    // To measure the *cold start* (which happens once per type), we need to create
    // N unique closed types at runtime and force the static initializer.
    // Since we cannot dynamically declare generic types easily in BenchmarkDotNet loop,
    // we use a technique: reflection to create the generic type dynamically.

    [Params(10, 25, 50)]
    public int TypesCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
    }

    [Benchmark(Description = "Expression.Compile Cold Start (N types)")]
    public void MeasureColdStartCompilation()
    {
        for (int i = 0; i < TypesCount; i++)
        {
            // Simulate what BuildFailureFactory does for Result<T>
            // We use different types for T to avoid hitting reflection caches
            var t = i switch
            {
                0 => typeof(int),
                1 => typeof(string),
                2 => typeof(Guid),
                3 => typeof(double),
                4 => typeof(float),
                5 => typeof(decimal),
                6 => typeof(long),
                7 => typeof(short),
                8 => typeof(byte),
                9 => typeof(bool),
                10 => typeof(char),
                11 => typeof(DateTime),
                12 => typeof(DateTimeOffset),
                13 => typeof(TimeSpan),
                14 => typeof(object),
                15 => typeof(System.Text.StringBuilder),
                16 => typeof(System.IO.MemoryStream),
                17 => typeof(System.Net.Http.HttpClient),
                18 => typeof(CancellationTokenSource),
                19 => typeof(System.Text.RegularExpressions.Regex),
                20 => typeof(System.Uri),
                21 => typeof(System.Version),
                22 => typeof(System.Type),
                23 => typeof(System.Reflection.Assembly),
                24 => typeof(System.Reflection.MethodInfo),
                25 => typeof(Tuple<int, int>),
                26 => typeof(Tuple<string, string>),
                27 => typeof(Tuple<Guid, Guid>),
                28 => typeof(Tuple<double, double>),
                29 => typeof(Tuple<float, float>),
                30 => typeof(Tuple<decimal, decimal>),
                31 => typeof(Tuple<long, long>),
                32 => typeof(Tuple<short, short>),
                33 => typeof(Tuple<byte, byte>),
                34 => typeof(Tuple<bool, bool>),
                35 => typeof(Tuple<char, char>),
                36 => typeof(Tuple<DateTime, DateTime>),
                37 => typeof(Tuple<DateTimeOffset, DateTimeOffset>),
                38 => typeof(Tuple<TimeSpan, TimeSpan>),
                39 => typeof(Tuple<object, object>),
                40 => typeof(ValueTuple<int, int>),
                41 => typeof(ValueTuple<string, string>),
                42 => typeof(ValueTuple<Guid, Guid>),
                43 => typeof(ValueTuple<double, double>),
                44 => typeof(ValueTuple<float, float>),
                45 => typeof(ValueTuple<decimal, decimal>),
                46 => typeof(ValueTuple<long, long>),
                47 => typeof(ValueTuple<short, short>),
                48 => typeof(ValueTuple<byte, byte>),
                49 => typeof(ValueTuple<bool, bool>),
                _ => typeof(Tuple<int, int, int>) // Fallback
            };

            var valueType = t;
            var responseType = typeof(Result<>).MakeGenericType(valueType);

            var errorParam = System.Linq.Expressions.Expression.Parameter(typeof(Error), "error");
            var failureMethod = typeof(Result)
                .GetMethod(nameof(Result.Failure), 1, new[] { typeof(Error) })!
                .MakeGenericMethod(valueType);

            var callExpr = System.Linq.Expressions.Expression.Call(failureMethod, errorParam);
            var castExpr = System.Linq.Expressions.Expression.Convert(callExpr, responseType);
            var lambda = System.Linq.Expressions.Expression.Lambda(castExpr, errorParam);

            // The expensive operation
            var compiled = lambda.Compile();
        }
    }
}


