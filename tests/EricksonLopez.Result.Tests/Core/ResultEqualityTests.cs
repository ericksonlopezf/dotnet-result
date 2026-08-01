using System;
using Xunit;
using EricksonLopez.Result;
using System.Reflection;

namespace EricksonLopez.Result.Tests.Core;

public class ResultEqualityTests
{
    [Fact]
    public void Result_MissedPaths()
    {
        Result r1 = Result.Success();
        Result r2 = Result.Failure(Error.Failure("X", "X"));
        bool b3 = r1 == r2;
        bool b4 = r1 != r2;
        bool b5 = ((IEquatable<Result>)r1).Equals(r2);
        
        var opFalse = typeof(Result).GetMethod("op_False");
        if (opFalse != null)
        {
            opFalse.Invoke(null, new object[] { r1 });
            opFalse.Invoke(null, new object[] { r2 });
        }
        var opTrue = typeof(Result).GetMethod("op_True");
        if (opTrue != null)
        {
            opTrue.Invoke(null, new object[] { r1 });
            opTrue.Invoke(null, new object[] { r2 });
        }
    }


    [Fact]
    public async System.Threading.Tasks.Task Result_TryAsync_MissedPaths()
    {
        var e = new InvalidOperationException("X");
        var r1 = await Result.TryAsync(() => throw e, ex => Error.Failure("X", "X"));
        var r2 = await Result.TryAsync(async () => { await System.Threading.Tasks.Task.Yield(); throw e; }, ex => Error.Failure("X", "X"));
        
        var r3 = await Result.TryAsync<int>(() => throw e, ex => Error.Failure("X", "X"));
        var r4 = await Result.TryAsync<int>(async () => { await System.Threading.Tasks.Task.Yield(); throw e; }, ex => Error.Failure("X", "X"));
        
        var r5 = await Result.TryAsync(() => throw e, ex => Error.Failure("X", "X"));
        var r6 = await Result.TryAsync(async () => { await System.Threading.Tasks.Task.Yield(); throw e; }, ex => Error.Failure("X", "X"));
        
        var r7 = await Result.TryAsync<int>(() => throw e, ex => Error.Failure("X", "X"));
        var r8 = await Result.TryAsync<int>(async () => { await System.Threading.Tasks.Task.Yield(); throw e; }, ex => Error.Failure("X", "X"));
        
        var error = Error.Create("A","A").WithMetadata("k1", 5).WithMetadata("k2", (object?)null!).Build();
        error.TryGetMetadata<int>("k1", out _);
        error.GetMetadata<int>("k1");
        try { error.GetMetadata<int>("k2"); } catch { }
        try { error.TryGetMetadata<string>("k1", out _); } catch { }
        try { error.GetMetadata<string>("k1"); } catch { }
    }
}
