# EricksonLopez.Result.MediatR

> **Warning: Legacy Transitional Bridge & Non-AOT Package**  
> This package is provided strictly as a transitional bridge for legacy codebases migrating towards `EricksonLopez.Mediator`.

## Overview
`EricksonLopez.Result.MediatR` provides MediatR pipeline behaviors integrated with `EricksonLopez.Result`.

## AOT & Trimming Compatibility Notice
- **Native AOT Compatible**: `No` (`<IsAotCompatible>false</IsAotCompatible>`)
- **Trimmable**: `No` (`<IsTrimmable>false</IsTrimmable>`)
- **Technical Rationale**: Third-party `MediatR` relies heavily on runtime reflection and expression compilation (`Expression.Lambda.Compile()`).

## Migration & Deprecation Roadmap
1. **Current Status**: Maintenance mode for legacy compatibility.
2. **Recommended Target**: Migrate to [`EricksonLopez.Mediator`](https://github.com/ericksonlopezf/dotnet-mediator), which is built from the ground up for Native AOT using Roslyn compile-time source generation.
3. **Deprecation Plan**: This package is slated for deprecation in upcoming major releases once full ecosystem migration to `EricksonLopez.Mediator` is complete.
