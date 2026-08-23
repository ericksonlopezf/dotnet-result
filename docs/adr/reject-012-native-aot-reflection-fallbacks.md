# Architectural Decision Record: REJECT-012
## Rejection of Reflection-Based Fallbacks for Native AOT Trimming

### Status
**REJECTED (Permanent Directorial Invariant)**

### Context
Suggestions were evaluated to allow dynamic reflection fallbacks when source-generated serializers or Roslyn generators are unavailable.

### Decision
Permanently rejected. The entire EricksonLopez ecosystem strictly adheres to `IsAotCompatible=true`, `IsTrimmable=true`, and `TreatWarningsAsErrors=true`. All serialization, dispatching, and mapping must use compile-time source generation or strongly-typed generic abstractions with zero trimming warnings.

### Consequences
- 100% Native AOT compilation guarantee.
- Ultra-low memory footprints and instant cold starts in cloud container runtimes.
