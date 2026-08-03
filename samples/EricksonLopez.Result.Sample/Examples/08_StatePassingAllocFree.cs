using System;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class StatePassingAllocFree
{
    public static void Run()
    {
        Console.WriteLine("\n--- 08. STATE PASSING (ALLOCATION-FREE) ---");

        // When you write lambda expressions that capture local variables (closures), 
        // C# compiler creates a hidden class and allocates it on the heap.
        // EricksonLopez.Result provides <TState> overloads to pass the state explicitly 
        // as a parameter, preventing these allocations. This is crucial for high-performance paths.

        Result<int> result = Result.Success(10);
        int multiplier = 5;

        // BAD (Allocates a closure for 'multiplier')
        Result<int> mappedWithClosure = result.Map(val => val * multiplier);

        // GOOD (Zero allocation, state is passed explicitly)
        Result<int> mappedAllocFree = result.Map(
            state: multiplier, 
            mapper: (state, val) => val * state
        );

        Console.WriteLine($"Mapped (Alloc-Free) value: {mappedAllocFree.GetValueOrDefault(0)}");

        // Works for Bind, Match, MatchError, Ensure, TapOnSuccess, etc.
        // Example with TapOnSuccess:
        string logPrefix = "[StateTap]";
        
        mappedAllocFree.TapOnSuccess(
            state: logPrefix, 
            action: (state, val) => Console.WriteLine($" {state} Current value is {val}")
        );

        // Example with Ensure:
        int maxAllowed = 100;
        Result<int> ensureAllocFree = mappedAllocFree.Ensure(
            state: maxAllowed,
            predicate: (state, val) => val <= state,
            errorFactory: (state, val) => Error.Create("Ensure.Failed", $"Value {val} exceeded max {state}").Build()
        );

        Console.WriteLine($"Ensure (Alloc-Free) success: {ensureAllocFree.IsSuccess}");
    }
}
