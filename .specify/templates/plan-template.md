# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]
**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION]  
**Primary Dependencies**: [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION]  
**Storage**: [if applicable, e.g., PostgreSQL, CoreData, files or N/A]  
**Testing**: [e.g., pytest, XCTest, cargo test or NEEDS CLARIFICATION]  
**Target Platform**: [e.g., Linux server, iOS 15+, WASM or NEEDS CLARIFICATION]
**Project Type**: [e.g., library/cli/web-service/mobile-app/compiler/desktop-app or NEEDS CLARIFICATION]  
**Performance Goals**: [domain-specific, e.g., 1000 req/s, 10k lines/sec, 60 fps or NEEDS CLARIFICATION]  
**Constraints**: [domain-specific, e.g., <200ms p95, <100MB memory, offline-capable or NEEDS CLARIFICATION]  
**Scale/Scope**: [domain-specific, e.g., 10k users, 1M LOC, 50 screens or NEEDS CLARIFICATION]

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify the following before proceeding. Mark each ✅ pass or ❌ fail (fail = blocked):

| # | Gate | Status |
|---|---|---|
| G1 | Service targets .NET 8+ with minimal hosting (`Program.cs` only, no `Startup.cs`) | |
| G2 | Service has four layers: Domain / Application / Infrastructure / Api (project references enforced) | |
| G3 | Domain project has zero dependencies on other layers or external packages | |
| G4 | Application project does NOT reference Infrastructure | |
| G5 | No business logic placed in Controllers or Minimal API handlers | |
| G6 | EF Core entities are NOT exposed outside the Infrastructure project | |
| G7 | Service owns its own database schema (no shared DB with another service) | |
| G8 | Inter-service communication uses async messaging (or HTTP/gRPC with written justification) | |
| G9 | Integration contracts (events/DTOs) are explicitly versioned | |
| G10 | Structured logging, health checks, OpenTelemetry, FluentValidation, and global exception handling are wired | |
| G11 | Unit tests planned for Domain + Application layers | |
| G12 | Integration tests planned for Infrastructure + API layers (TestContainers for DB) | |
| G13 | No hard-coded connection strings, secrets, or URLs | |

**Complexity Violations** (fill only if a gate fails and an ADR exception is needed):

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)
src/
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# [REMOVE IF UNUSED] Option 2: Web application (when "frontend" + "backend" detected)
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/

# [REMOVE IF UNUSED] Option 3: Mobile + API (when "iOS/Android" detected)
api/
└── [same as backend above]

ios/ or android/
└── [platform-specific structure: feature modules, UI flows, platform tests]
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Complexity Tracking

> **Fill ONLY if a Constitution Check gate failed and an ADR exception was granted**

| Gate Failed | Why Exception Needed | Simpler Alternative Rejected Because |
|-------------|----------------------|--------------------------------------|
| [e.g., G3 — Domain references external lib] | [specific reason] | [why restructuring is not feasible] |
| [e.g., G7 — shared DB required] | [specific reason] | [why separate schemas are not feasible] |
