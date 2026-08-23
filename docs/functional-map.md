# Phase 2 — Complete Functional Map
**EricksonLopez.Result** — Component interaction from input to output

---

## 1. General Architecture Diagram

```mermaid
graph TB
    subgraph "Client Layer"
        Client([HTTP Client / Test / Consumer])
    end

    subgraph "API Layer — EricksonLopez.Result.AspNetCore"
        Filter[ResultEndpointFilter\nUnwrap Result → IResult]
        HttpExt[.ToHttpResult\nRFC 9457 ProblemDetails]
        HttpOpts[ResultHttpOptions\nErrorType → HTTP Status]
    end

    subgraph "Application Layer"
        Handler[Application Handler\nCommand / Query]
        MediatRBehavior[ResultExceptionBehavior\nException → Result.Failure]
        FVExt[FluentValidation.ToValidationResult\nValidationResult → Result]
    end

    subgraph "Domain Layer"
        ResultCore["Result / Result&lt;T&gt;\n(readonly struct — zero heap)"]
        ErrorCore["Error\n(sealed class — structured diagnostic)"]
        ErrorBuilder["ErrorBuilder\n(readonly struct — fluent copy-on-write)"]
        WellKnown["WellKnownErrors\n(sentinel constants)"]
    end

    subgraph "Composition Layer"
        Combine[Result.Combine\nAggregate N results]
        ValidateAll[Result.ValidateAll\nAccumulate failures]
        Merge[Result.Merge\nGuard + typed passthrough]
        LINQ[ResultLinqExtensions\nfrom x in result select ...]
        SyncExt[ResultSyncExtensions\nin-param, zero-copy]
        AsyncExt[ResultExtensions\nTask/ValueTask pipelines]
    end

    subgraph "Observability — EricksonLopez.Result.OpenTelemetry"
        TraceOutcome[ResultActivityExtensions.TraceOutcome\nAnnotate Activity.Current]
        Metrics[ResultMetrics\nDI + static counters]
    end

    subgraph "Serialization — EricksonLopez.Result.Serialization"
        JsonConverter[ResultJsonConverter&lt;T&gt;\nSystem.Text.Json integration]
    end

    subgraph "Testing — EricksonLopez.Result.Testing"
        Assertions[ResultAssertions\n.ShouldBeSuccess / .ShouldBeFailure]
    end

    Client --> Filter
    Filter --> HttpExt
    Filter --> HttpOpts
    Filter --> Handler
    Handler --> MediatRBehavior
    Handler --> FVExt
    Handler --> ResultCore
    ResultCore --> ErrorCore
    ResultCore --> Combine
    ResultCore --> ValidateAll
    ResultCore --> Merge
    ResultCore --> LINQ
    ResultCore --> SyncExt
    ResultCore --> AsyncExt
    ErrorCore --> ErrorBuilder
    ErrorCore --> WellKnown
    ResultCore --> TraceOutcome
    ResultCore --> Metrics
    ResultCore --> JsonConverter
    ResultCore --> Assertions
```

---

## 2. Primary Flow: Railway-Oriented Pipeline

```mermaid
flowchart LR
    Input([Input]) --> Try{Result.Try / TryAsync}
    Try -->|Exception caught| FailTrack

    subgraph "Success Track"
        Try -->|Ok| Ensure{.Ensure\npredicate?}
        Ensure -->|Pass| Bind[.Bind\nAsync service call]
        Bind -->|Ok| Map[.Map\nTransform value]
        Map -->|Ok| TapOnSuccess[.TapOnSuccess\nLogging side-effect]
        TapOnSuccess --> Output([Result.Success&lt;T&gt;])
    end

    subgraph "Failure Track"
        Ensure -->|Fail| FailTrack[Result.Failure&lt;T&gt;]
        Bind -->|Fail| FailTrack
        TapOnFailure[.TapOnFailure\nError logging]
        Recover[.Recover\nCorrective fallback?]
        MapError[.MapError\nError enrichment]
    end

    FailTrack --> TapOnFailure
    TapOnFailure --> Recover
    Recover -->|Recovered| Output
    Recover -->|Still failing| MapError
    MapError --> Terminal([Return failure to caller])
```

---

## 3. Result&lt;T&gt; State Diagram

```mermaid
stateDiagram-v2
    [*] --> Uninitialized : default(Result&lt;T&gt;)
    Uninitialized --> Success : Result.Success(value)
    Uninitialized --> Failure : Result.Failure(error)
    Success --> Failure : .Ensure(fails)\n.Bind(returns failure)
    Success --> Success : .Map / .Bind(success)\n.TapOnSuccess / .Inspect
    Failure --> Success : .Recover(returns success)
    Failure --> Failure : .MapError\n.TapOnFailure\n.Recover(still fails)
    Success --> [*] : .Match / .Execute\n.GetValueOrDefault\n.DiscardValue
    Failure --> [*] : .Match / .Execute\n.MapFailure\n.GetValueOrFallback
    note right of Uninitialized
        ⚠ Most APIs throw InvalidOperationException
        for uninitialized state.
        GetValueOrDefault and TryGetValue are safe.
    end note
```

---

## 4. Sequence Diagram: ASP.NET Core Request → Result → HTTP Response

```mermaid
sequenceDiagram
    participant Client as HTTP Client
    participant Filter as ResultEndpointFilter
    participant Handler as Minimal API Handler
    participant Domain as Domain / Application Service
    participant OTel as OpenTelemetry Activity

    Client->>Filter: POST /orders {payload}
    Filter->>Handler: Invoke(context)
    Handler->>Domain: CreateOrderAsync(command)
    
    alt Domain validates input
        Domain-->>Handler: Result.Failure(Error.Validation(...))
        Handler-->>Filter: Result&lt;OrderDto&gt; (failure)
        Filter->>Filter: ToHttpResult() → 400 ProblemDetails
        Filter-->>Client: 400 Bad Request (RFC 9457 JSON)
    else Domain succeeds
        Domain-->>Handler: Result.Success(order)
        Handler->>Handler: .Map(order => dto)
        Handler->>OTel: TraceOutcome("CreateOrder", activity)
        Handler-->>Filter: Result&lt;OrderDto&gt; (success)
        Filter->>Filter: ToHttpResult() → 200 OK
        Filter-->>Client: 200 OK (OrderDto JSON)
    end
```

---

## 5. Package Dependencies Diagram

```mermaid
graph TD
    Core["EricksonLopez.Result\n(Core — no external deps)"]
    
    AspNetCore["EricksonLopez.Result.AspNetCore\n(Microsoft.AspNetCore.Http)"]
    FluentVal["EricksonLopez.Result.FluentValidation\n(FluentValidation)"]
    MediatR["EricksonLopez.Result.MediatR\n(MediatR)"]
    OpenApi["EricksonLopez.Result.OpenApi\n(Microsoft.AspNetCore.OpenApi)"]
    OpenTelemetry["EricksonLopez.Result.OpenTelemetry\n(System.Diagnostics — BCL only)"]
    Serialization["EricksonLopez.Result.Serialization\n(System.Text.Json — BCL only)"]
    Testing["EricksonLopez.Result.Testing\n(no assertion framework dep)"]
    Analyzers["EricksonLopez.Result.Analyzers\n(Roslyn SDK — build-time only)"]

    Core --> AspNetCore
    Core --> FluentVal
    Core --> MediatR
    Core --> OpenApi
    Core --> OpenTelemetry
    Core --> Serialization
    Core --> Testing
    Core -.->|build-time only| Analyzers
```

---

## 6. ValidateAll Pipeline Diagram (Error Accumulation)

```mermaid
flowchart TD
    Input([Input: T value]) --> V1[validator 1\nFunc&lt;T, Result&gt;]
    Input --> V2[validator 2\nFunc&lt;T, Result&gt;]
    Input --> V3[validator 3\nFunc&lt;T, Result&gt;]
    
    V1 -->|Failure| Acc[Error Accumulator\nArrayPool&lt;Error&gt; — zero extra heap]
    V2 -->|Failure| Acc
    V3 -->|Failure| Acc
    V1 -->|Success| SkipV1[skip]
    V2 -->|Success| SkipV2[skip]
    V3 -->|Success| SkipV3[skip]
    
    Acc --> CountCheck{failureCount?}
    CountCheck -->|0| ReturnSuccess([Result.Success])
    CountCheck -->|1| ReturnSingle([Result.Failure\nsingle Error])
    CountCheck -->|2+| ReturnCompound([Result.Failure\nError.Validation\nWellKnownErrors.CombinedFailuresCode\nwith InnerErrors array])
```

---

## 7. Combine Diagram — Heterogeneous vs Homogeneous

```mermaid
graph LR
    subgraph "Heterogeneous Tuple Overloads"
        R1["Result&lt;T1&gt;"] --> T2
        R2["Result&lt;T2&gt;"] --> T2
        R3["Result&lt;T3&gt;"] --> T3
        T2["Combine&lt;T1,T2&gt;\nResult&lt;(T1,T2)&gt;"]
        T3["Combine&lt;T1,T2,T3&gt;\nResult&lt;(T1,T2,T3)&gt;"]
    end
    subgraph "Homogeneous List Overload"
        RA["Result&lt;T&gt;[]"] --> LA
        LA["Combine&lt;T&gt;(params)\nResult&lt;IReadOnlyList&lt;T&gt;&gt;"]
    end
    subgraph "Non-Generic Guard"
        RG1[Result] --> G
        RG2[Result] --> G
        G["Combine(params Result[])\nResult"]
    end
```

---

## 8. Error Handling Flow Diagram

```mermaid
flowchart TD
    Error([Error Occurs]) --> ClassifyType{Error.Type?}
    
    ClassifyType -->|Validation| Val[ErrorType.Validation\nSeverity: Warning\nHTTP: 400 Bad Request]
    ClassifyType -->|NotFound| NF[ErrorType.NotFound\nSeverity: Warning\nHTTP: 404 Not Found]
    ClassifyType -->|Conflict| Con[ErrorType.Conflict\nSeverity: Warning\nHTTP: 409 Conflict]
    ClassifyType -->|Unauthorized| Unauth[ErrorType.Unauthorized\nSeverity: Error\nHTTP: 401 Unauthorized]
    ClassifyType -->|Forbidden| Forb[ErrorType.Forbidden\nSeverity: Error\nHTTP: 403 Forbidden]
    ClassifyType -->|Unexpected| Unexp[ErrorType.Unexpected\nSeverity: Critical\nHTTP: 500 Internal Server Error]
    ClassifyType -->|Unavailable| Unav[ErrorType.Unavailable\nSeverity: Error\nHTTP: 503 Service Unavailable\nRetryability: Transient]
    
    Val --> RetryCheck
    NF --> RetryCheck
    Con --> RetryCheck
    Unauth --> RetryCheck
    Forb --> RetryCheck
    Unexp --> RetryCheck
    Unav --> RetryCheck
    
    RetryCheck{Retryability?} -->|Transient| Retry[Retry after delay]
    RetryCheck -->|Permanent| Dead[Dead letter / reject]
    RetryCheck -->|NotApplicable| Direct[Direct failure response]
```
