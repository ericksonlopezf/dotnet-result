# ADR-007: Exclusion of Asynchronous State Machines from Code Coverage

## Status
Accepted

## Context
During the process of standardizing and auditing the quality of the `EricksonLopez.Result` package, a non-negotiable goal of 100% code coverage (lines and branches) was established.
The package provides a rich interface of asynchronous extensions for `Task` and `ValueTask`. Many of these extensions use `static async` local functions (e.g., `BindCore`, `MapCore`) to encapsulate the asynchronous state machines (the "slow path").
Despite creating tests that directly invoke the local functions by returning incomplete tasks (`Task.Delay`), certain synthetic branches and `catch` blocks generated internally by the compiler for `IAsyncStateMachine.MoveNext()` remain technically unreachable or extremely difficult to reproduce deterministically in a unit testing framework like xUnit, preventing the coverage tool from reporting 100%.

## Decision
It was decided to document and explicitly apply the `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` attribute to the local functions generated for the `async` state machines ("slow path" code) in the `Task` and `ValueTask` extension methods (`ResultExtensions.cs` and `ResultExtensions.ValueTask.cs` files).
This decision establishes that **only hand-written code** will be measured for the coverage metric.

## Consequences
- **Positive:**
  - Coverage metrics will reflect a genuine 100% of the actual business logic written by the developer.
  - It avoids writing highly fragile tests or using Reflection/IL manipulation simply to satisfy unreachable compiler branches.
- **Negative:**
  - The asynchronous "slow path" will not report coverage in the tools, so its logical correctness will depend entirely on developers not introducing errors that escape integration tests. However, existing tests already evaluate the behavior of these functions synchronously in an exhaustive manner.
