---
applyTo: "**/*.cs,**/*.csproj,**/Controllers/**,**/Managers/**,**/Models/**,**/Dtos/**,**/Mappers/**"
---

# Backend Instructions

You are working on the C# and .NET backend of this repository. Prefer clean architecture, clear responsibilities, and production-grade code.

## Communication
- Communicate with the user in **German** by default.
- Keep code, identifiers, filenames, class names, method names, DTO names, and technical terms in **English**, unless explicitly requested otherwise.
- Explanations, suggestions, reviews, and architectural discussions should be written in **German**.

## Architecture
- Use **Controller** and **Manager** consistently in this project.
- `Controller` types are entry points and orchestrate flows.
- `Manager` types contain workflow logic and business logic.
- Do not introduce alternative types such as `Service`, `Handler`, `Processor`, `Coordinator`, or `Orchestrator` unless explicitly requested.
- Respect existing layers, folder structure, and naming conventions.

## Architectural Thinking
- Proactively point out architectural problems, structural issues, and code smells.
- If a quick fix would degrade the design, state this clearly and propose the cleaner architectural solution first.
- Prefer sustainable architectural improvements over short-term workarounds.
- When a problem indicates a deeper root cause, explain the root cause, not only the visible symptom.

## C# and .NET
- Use modern C# in line with the project’s current language version.
- Prefer small methods, clear responsibilities, and expressive naming.
- Use `async`/`await` correctly and pass `CancellationToken` through when working with async APIs.
- Avoid unnecessary abstractions, premature optimization, and over-engineering.
- Prefer `sealed` or otherwise constrained types when inheritance is not required.

## Models and DTOs
- Keep internal models, external DTOs, request/response models, and domain logic clearly separated.
- For external APIs, model only the fields that are actually used.
- Do not let transport models or provider DTOs leak into internal domain models.
- Use explicit mappers for transitions between layers and between external and internal representations.

## Code Quality
- Write compilable, complete, and production-oriented code wherever possible.
- Do not leave TODOs, placeholders, or intentionally incomplete methods unless explicitly requested.
- Watch for duplication, oversized classes, mixed responsibilities, unnecessary coupling, and fragile string processing.
- Use clear guard clauses and understandable error messages.

## Collaboration
- Present the best clean solution first.
- Where helpful, present a pragmatic short-term alternative as a second option.
- Explain decisions briefly and concretely.
- Continue and refine an existing design direction instead of reinventing it without a clear reason.
