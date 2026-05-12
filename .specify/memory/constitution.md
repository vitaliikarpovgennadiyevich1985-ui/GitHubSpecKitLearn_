<!--
SYNC IMPACT REPORT
Version Change: (none → 1.0.0) — initial population from template placeholders
Modified Principles: N/A (all new, first ratification)
Added Sections:
  - I. Technology Baseline
  - II. Microservices Architecture
  - III. Clean Architecture (Mandatory)
  - IV. Architectural Dependency Rules
  - V. Communication & Integration
  - VI. Data & Persistence
  - VII. Cross-Cutting Concerns
  - VIII. Code Quality
  - Testing Strategy
  - Project Structure Convention
  - Prohibited Practices
  - Primary Goal
  - Governance
Removed Sections: N/A
Templates Updated:
  ✅ .specify/templates/plan-template.md — Constitution Check gates populated
  ✅ .specify/templates/spec-template.md — no structural changes required
  ✅ .specify/templates/tasks-template.md — no structural changes required
Follow-up TODOs: None — all placeholders resolved
-->

# .NET Microservices Solution Constitution

## Core Principles

### I. Technology Baseline

The solution MUST target the latest stable .NET LTS or current-stable SDK (minimum .NET 10).
All services MUST use the latest C# language features available in the target SDK.
Every service MUST use the minimal hosting model (`Program.cs` only; no `Startup.cs`).
The built-in ASP.NET Core DI container MUST be used unless an external DI framework is
explicitly justified and documented in an Architecture Decision Record (ADR).
HTTP APIs MUST use ASP.NET Core (Minimal APIs or Controllers), applied consistently within
each microservice — mixing patterns within a single service is prohibited.

**Rationale**: Consistency across the fleet reduces cognitive overhead, eliminates version
drift, and ensures the team benefits from latest platform improvements without per-service
archaeology.

### II. Microservices Architecture

Every microservice MUST be:

- **Independently deployable** — no coordinated deployment required with another service.
- **Independently versioned** — each service owns its own semantic version.
- **Independently scalable** — no shared state or resources that prevent horizontal scaling.
- **Data sovereign** — each service owns exactly one database schema or database instance;
  cross-service database sharing is strictly prohibited.

**Rationale**: These four properties are the non-negotiable definition of a microservice in
this system. Violating any one of them converts a microservice into a distributed monolith.

### III. Clean Architecture (Mandatory per Microservice)

Every microservice MUST be structured into exactly four layers:

| Layer | Responsibility |
|---|---|
| **Domain** | Entities, Value Objects, Domain Events, Domain Exceptions. Zero external dependencies. |
| **Application** | Use Cases, CQRS Commands/Queries, Application interfaces, DTOs, business rule orchestration. |
| **Infrastructure** | EF Core/ORM, external HTTP clients, messaging, file storage, implementations of Application interfaces. |
| **Presentation** | ASP.NET Core API (Controllers or Minimal APIs), request/response models, input validation, auth handling. |

CQRS MUST be applied in the Application layer. Domain entities MUST encapsulate their own
invariants; anemic domain models are prohibited.

**Rationale**: Clean Architecture enforces a stable inward dependency direction that makes
each layer independently testable and replaceable without cascading changes across the service.

### IV. Architectural Dependency Rules (Strict)

- Domain layer MUST NOT import any other layer or external package beyond BCL primitives.
- Application layer MUST NOT depend on Infrastructure.
- Infrastructure MUST depend only on Application and Domain.
- Presentation MUST depend only on Application.
- No business logic is permitted in Controllers or Minimal API handlers.
- EF Core entities MUST NOT be exposed outside the Infrastructure layer.
- No direct database access from Application or Domain layers.

These rules MUST be enforced as compile-time project reference constraints and validated in CI.

**Rationale**: A single layer violation unravels testability and replaceability guarantees.
Enforcing at compile time prevents drift by making violations build errors, not code-review
findings.

### V. Communication & Integration

- Asynchronous messaging (RabbitMQ, Azure Service Bus, or Kafka) MUST be preferred for
  inter-service communication.
- Synchronous HTTP or gRPC calls are permitted only when justified in writing.
- All integration contracts (events, DTOs) MUST be explicitly versioned.
- Event consumers MUST be idempotent.
- All inter-service communication MUST implement retries, dead-letter handling, and
  resilience patterns (e.g., Polly circuit breaker, exponential back-off).
- Direct code-level dependencies between microservices (shared assemblies, shared domain
  models) are prohibited.

**Rationale**: Async messaging decouples services in time and failure domain. Idempotency
and resilience patterns ensure the system degrades gracefully rather than failing in cascade.

### VI. Data & Persistence

- Each microservice owns its database schema; cross-service joins and shared data models
  are prohibited.
- EF Core is the default ORM. Deviations require documented performance justification (ADR).
- Each service MUST maintain its own independent migration history.
- No shared entity models may be referenced across microservice boundaries.

**Rationale**: Database sovereignty is the infrastructure expression of service independence.
Shared schemas create the tightest possible coupling and defeat the purpose of microservices.

### VII. Cross-Cutting Concerns

All microservices MUST implement:

- **Structured Logging**: Serilog (or equivalent). Machine-readable JSON output in
  production; human-readable in development.
- **Health Checks**: ASP.NET Core health check endpoints (`/health`, `/health/ready`).
- **Configuration**: Environment-based centralized configuration; no hard-coded values
  (connection strings, secrets, or URLs).
- **Distributed Tracing**: OpenTelemetry instrumentation (preferred); trace context MUST
  be propagated across service boundaries.
- **Validation**: FluentValidation (or equivalent) at the Presentation/Application boundary.
- **Global Exception Handling**: Middleware that maps domain and application exceptions to
  RFC 7807 Problem Details responses.

**Rationale**: Uniform observability and resilience reduce MTTR and allow cross-service
correlation during incidents without bespoke per-service tooling.

### VIII. Code Quality

- Business logic MUST NOT be duplicated across services.
- SOLID principles MUST be applied strictly throughout all layers.
- Composition over inheritance MUST be preferred.
- All public APIs MUST have explicit input validation before any business logic executes.
- Unit tests for Domain and Application layers are required for every service.
- Integration tests for Infrastructure and API layers are required for every service.

**Rationale**: These rules protect long-term maintainability. Duplication across services
defeats independent evolution; absent tests make refactoring unsafe.

## Testing Strategy

| Scope | Target Layers | Tooling |
|---|---|---|
| Unit Tests | Domain + Application | xUnit, Moq / NSubstitute |
| Integration Tests | Infrastructure + API | xUnit, TestContainers, WebApplicationFactory |

- External dependencies MUST be mocked at Application boundaries only (not inside Domain).
- TestContainers MUST be used for database-dependent integration tests.
- Test projects MUST reside in the `/tests` folder alongside each service's source tree.

## Project Structure Convention

Every microservice MUST follow this folder layout:

```text
/src
  /ServiceName.Domain
  /ServiceName.Application
  /ServiceName.Infrastructure
  /ServiceName.Api
/tests
  /ServiceName.UnitTests
  /ServiceName.IntegrationTests
```

Deviation from this layout requires an explicit note in the service's `README.md`.

## Prohibited Practices

The following are strictly prohibited without an approved ADR exception:

1. Shared database between any two microservices.
2. Direct code-level dependency between microservices (shared class libraries that couple services).
3. Business logic in API Controllers or Minimal API handlers.
4. Skipping the Application layer in a request flow (Presentation → Infrastructure directly).
5. "God services" or shared core logic libraries that couple multiple microservices.
6. EF Core entities surfaced in API response contracts or outside Infrastructure.
7. Hard-coded configuration values (connection strings, secrets, URLs).
8. Synchronous inter-service HTTP calls without documented justification and resilience patterns.

## Primary Goal

The system MUST be:

- **Highly maintainable** — each service can be understood, modified, and tested independently.
- **Loosely coupled** — no change in one service forces a change in another.
- **Testable at every layer** — Domain, Application, Infrastructure, and API all have
  automated test coverage.
- **Cloud-native ready** — containerizable, health-check-exposed, environment-configured,
  and OpenTelemetry-instrumented.
- **Horizontally scalable** — each microservice scales independently without shared state
  constraints.

## Governance

This constitution supersedes all prior architectural guidance, README conventions, and
ad-hoc team agreements within this solution.

**Amendment Procedure**:
- Amendments require a written ADR capturing motivation, alternatives considered, and
  a migration plan for existing services.
- Backward-incompatible changes (principle removal or redefinition) require team consensus
  and increment the MAJOR version.
- New principles or material expansions increment the MINOR version.
- Clarifications and wording fixes increment the PATCH version.

**Compliance**:
- All pull requests MUST verify compliance with this constitution before merge.
- CI MUST enforce project reference constraints that prevent cross-layer dependency violations.
- Architecture violations discovered in review MUST be resolved before merge; they are not
  deferrable to follow-up tickets.

**Runtime Guidance**: For day-to-day development guidance, refer to each service's
`README.md` and the `docs/` folder at the repository root.

**Version**: 1.0.0 | **Ratified**: 2026-05-05 | **Last Amended**: 2026-05-05
