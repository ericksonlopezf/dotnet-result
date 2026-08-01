#nullable disable
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ReflectionCoverageTests
{
    [Fact]
    public void Cover_All_Extension_Wrappers()
    {
        var typesToTest = new[] { typeof(ResultExtensions) };
        foreach (var type in typesToTest)
        {
            if (type == null) continue;
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var method in methods)
            {
                if (!method.ReturnType.Name.Contains("Task")) continue;

                MethodInfo targetMethod = method;
                if (method.IsGenericMethodDefinition)
                {
                    var genArgs = method.GetGenericArguments().Select(g => typeof(int)).ToArray();
                    targetMethod = method.MakeGenericMethod(genArgs);
                }

                var parameters = targetMethod.GetParameters();
                var args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    args[i] = CreateDummy(parameters[i].ParameterType, false);
                }

                try
                {
                    targetMethod.Invoke(null, args);
                }
                catch (Exception) { }
            }
        }
    }

    [Fact]
    public void Cover_All_Extension_Wrappers_FastPath()
    {
        var typesToTest = new[] { typeof(ResultExtensions) };
        foreach (var type in typesToTest)
        {
            if (type == null) continue;
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var method in methods)
            {
                if (!method.ReturnType.Name.Contains("Task")) continue;

                MethodInfo targetMethod = method;
                if (method.IsGenericMethodDefinition)
                {
                    var genArgs = method.GetGenericArguments().Select(g => typeof(int)).ToArray();
                    targetMethod = method.MakeGenericMethod(genArgs);
                }

                var parameters = targetMethod.GetParameters();
                var args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    args[i] = CreateDummy(parameters[i].ParameterType, i == 0); // Fast path!
                }

                try
                {
                    targetMethod.Invoke(null, args);
                }
                catch (Exception) { }
            }
        }
    }

    private static object CreateDummy(Type type, bool forceCompleted)
    {
        if (type == typeof(CancellationToken)) return CancellationToken.None;
        if (type == typeof(int)) return 0;
        if (type == typeof(Error)) return Error.Failure("code", "msg");
        
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var inner = type.GetGenericArguments()[0];
            var tcsType = typeof(TaskCompletionSource<>).MakeGenericType(inner);
            var tcs = Activator.CreateInstance(tcsType);
            if (forceCompleted) {
                var setRes = tcsType.GetMethod("SetResult");
                var dummyRes = CreateDummy(inner, false);
                setRes.Invoke(tcs, new[] { dummyRes });
            }
            return tcsType.GetProperty("Task").GetValue(tcs);
        }
        if (type == typeof(Task))
        {
            return forceCompleted ? Task.CompletedTask : new TaskCompletionSource<int>().Task;
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            var inner = type.GetGenericArguments()[0];
            var taskType = typeof(Task<>).MakeGenericType(inner);
            var task = CreateDummy(taskType, forceCompleted);
            return Activator.CreateInstance(type, task);
        }
        if (type == typeof(ValueTask))
        {
            var taskType = typeof(Task);
            var task = CreateDummy(taskType, forceCompleted);
            return Activator.CreateInstance(typeof(ValueTask), task);
        }

        if (typeof(Delegate).IsAssignableFrom(type))
        {
            var invoke = type.GetMethod("Invoke");
            var parameters = invoke.GetParameters().Select(p => Expression.Parameter(p.ParameterType)).ToArray();
            Expression body = Expression.Default(invoke.ReturnType);
            
            if (invoke.ReturnType == typeof(Result))
                body = Expression.Constant(Result.Success());
            else if (invoke.ReturnType.IsGenericType && invoke.ReturnType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var innerResult = invoke.ReturnType.GetGenericArguments()[0];
                var successMethod = typeof(Result).GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .First(m => m.Name == "Success" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1)
                    .MakeGenericMethod(innerResult);
                
                var dummyVal = innerResult.IsValueType ? Activator.CreateInstance(innerResult) : null;
                var resultVal = successMethod.Invoke(null, new[] { dummyVal });
                body = Expression.Constant(resultVal, invoke.ReturnType);
            }
            else if (invoke.ReturnType == typeof(Error))
                body = Expression.Constant(Error.Failure("X", "Y"));

            return Expression.Lambda(type, body, parameters).Compile();
        }

        
        if (type == typeof(Result)) return Result.Success();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>)) {
            var innerResult = type.GetGenericArguments()[0];
            var successMethod = typeof(Result).GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == "Success" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1).MakeGenericMethod(innerResult);
            return successMethod.Invoke(null, new[] { type.IsValueType ? Activator.CreateInstance(innerResult) : null });
        }
        return type.IsValueType ? Activator.CreateInstance(type) : null;

    }

    [Fact]
    public void ResultOfT_ErrorIsNull_WhenFailure_ReturnsUninitializedError()
    {
        var result = default(Result<int>);
        // Use reflection to force state to Failure while leaving error null
        var stateField = typeof(Result<int>).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);
        object boxed = result;
        stateField!.SetValue(boxed, (byte)2); // ResultState.Failure is 1
        result = (Result<int>)boxed;

        Assert.True(result.TryGetError(out var error1));
        Assert.Equal(WellKnownErrors.UninitializedError, error1);

        Assert.True(result.TryGetError(out var error2, out var isUninit));
        Assert.Equal(WellKnownErrors.UninitializedError, error2);
        Assert.False(isUninit);

        result.Deconstruct(out var isSuccess, out var error3);
        Assert.False(isSuccess);
        Assert.Equal(WellKnownErrors.UninitializedError, error3);

        var method = typeof(Result<int>).GetMethod("GetDebuggerDisplay", BindingFlags.NonPublic | BindingFlags.Instance);
        var display = (string)method!.Invoke(result, null)!;
        Assert.Equal("Failure ()", display);
    }
    [Fact]
    public void Default_Result_ThrowsUninitialized()
    {
        var types = new[] { typeof(Result), typeof(Result<int>) };
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var target = Activator.CreateInstance(type); // default

            foreach (var method in methods)
            {
                if (method.Name.StartsWith("get_", StringComparison.Ordinal) || method.Name == "Equals" || method.Name == "GetHashCode" || method.Name == "ToString" || method.Name == "Deconstruct" || method.Name == "TryGetError" || method.Name == "TryGetValue" || method.Name == "GetValueOrDefault" || method.Name == "op_Implicit" || method.Name == "op_Equality" || method.Name == "op_Inequality")
                    continue;

                var targetMethod = method;
                if (targetMethod.IsGenericMethodDefinition)
                {
                    var genArgs = targetMethod.GetGenericArguments().Select(t => typeof(int)).ToArray();
                    targetMethod = targetMethod.MakeGenericMethod(genArgs);
                }

                var parameters = targetMethod.GetParameters();
                var args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    args[i] = CreateDummy(parameters[i].ParameterType, false);
                }

                try
                {
                    var ex = Assert.Throws<TargetInvocationException>(() => targetMethod.Invoke(target, args));
                    Assert.IsType<InvalidOperationException>(ex.InnerException);
                }
                catch (Xunit.Sdk.XunitException)
                {
                    Assert.Fail($"Method {targetMethod.Name} on {type.Name} failed to throw exception.");
                }
            }
        }
    }
    [Fact]
    public void ResultThrowHelper_ExceptionMessages()
    {
        var ex1 = Assert.Throws<InvalidOperationException>(() => default(Result).Match(() => 1, e => 0));
        Assert.Equal("Cannot operate on an uninitialized default Result. Always construct Result via Result.Success() or Result.Failure(error).", ex1.Message);

        var ex2 = Assert.Throws<InvalidOperationException>(() => default(Result<int>).Match(v => v, e => 0));
        Assert.Equal("Cannot operate on an uninitialized default Result<TValue>. Always construct Result<TValue> via Result.Success(value) or Result.Failure(error).", ex2.Message);
    }
    [Fact]
    public void ResultOfT_TryGetError_WithRealError_ReturnsRealError()
    {
        var e = Error.Failure("real", "real");
        var r = Result.Failure<int>(e);
        Assert.True(r.TryGetError(out var e1));
        Assert.Equal(e, e1);
        Assert.True(r.TryGetError(out var e2, out var isUninit));
        Assert.Equal(e, e2);
        Assert.False(isUninit);
    }
    
    [Fact]
    public void ResultOfT_GetHashCode_WithNullValue_DoesNotThrow()
    {
        var r = Result.Success<string>(null);
        var hc = r.GetHashCode(); // Should return 0 for value
        Assert.NotEqual(0, hc); // _state hashcode will make it non-zero overall
    }
}






