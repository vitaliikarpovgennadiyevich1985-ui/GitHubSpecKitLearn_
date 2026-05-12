# Implementation Plan: WebUI Visual Product Cards

**Branch**: `002-webui-visual-product-cards` | **Date**: 2026-05-05 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-webui-visual-product-cards/spec.md`

## Summary

Add an `Image` string property to the UI `Product` model so that the existing `GetFromJsonAsync` call in `CatalogService` automatically deserialises the base64 BMP field already present in the Catalog microservice `/products` response. Update `Views/Catalog/Index.cshtml` to render all 100 products as a multi-column CSS Grid of cards — each card displaying a 128×128 px inline image (data URI) and structured metadata (Title, Description, Price). Cards degrade gracefully to a grey placeholder when image data is absent. No new NuGet packages, no third-party JS frameworks, no additional network requests.

## Technical Context

**Language/Version**: C# 13 / .NET 10  
**Primary Dependencies**: ASP.NET Core MVC, Duende.AccessTokenManagement (existing) — no new packages required  
**Storage**: N/A — UI project owns no database  
**Testing**: xUnit (planned; no test project currently exists for this service)  
**Target Platform**: Windows / Linux container; ASP.NET Core MVC  
**Project Type**: Web application (MVC frontend)  
**Performance Goals**: N/A — bounded static dataset (100 products); no new network round-trips  
**Constraints**: No third-party JS frameworks (raw JS only if needed); no new NuGet packages; card layout implemented with HTML + CSS only  
**Scale/Scope**: 100 products, single page, desktop browser target

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Gate | Status |
|---|---|---|
| G1 | Service targets .NET 8+ with minimal hosting (`Program.cs` only, no `Startup.cs`) | ✅ .NET 10, `Program.cs` only |
| G2 | Service has four layers: Domain / Application / Infrastructure / Api (project references enforced) | ❌ Single flat project — pre-existing violation (see ADR below) |
| G3 | Domain project has zero dependencies on other layers or external packages | ❌ No Domain project — pre-existing violation |
| G4 | Application project does NOT reference Infrastructure | ❌ No layered separation — pre-existing violation |
| G5 | No business logic placed in Controllers or Minimal API handlers | ✅ `CatalogController` is a thin pass-through; no business logic |
| G6 | EF Core entities are NOT exposed outside the Infrastructure project | ✅ N/A — UI project uses no EF Core |
| G7 | Service owns its own database schema (no shared DB with another service) | ✅ N/A — UI project owns no database |
| G8 | Inter-service communication uses async messaging (or HTTP/gRPC with written justification) | ✅ UI→microservice HTTP calls are user-facing synchronous requests; not service-to-service messaging |
| G9 | Integration contracts (events/DTOs) are explicitly versioned | ✅ N/A — UI publishes no integration events |
| G10 | Structured logging, health checks, OpenTelemetry, FluentValidation, and global exception handling are wired | ❌ None wired — pre-existing violation (out of scope for this feature) |
| G11 | Unit tests planned for Domain + Application layers | ❌ No test projects exist — pre-existing violation (out of scope for this feature) |
| G12 | Integration tests planned for Infrastructure + API layers (TestContainers for DB) | ❌ No test projects exist — pre-existing violation (out of scope for this feature) |
| G13 | No hard-coded connection strings, secrets, or URLs | ✅ `BaseAddress` uses Aspire service name `CatalogMicroservice`; no hard-coded URLs |

**Post-Phase-1 re-check**: gates G2–G4, G10–G12 remain pre-existing violations. This feature introduces no new violations.

## Project Structure

### Documentation (this feature)

```text
specs/002-webui-visual-product-cards/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── ICatalogService.md
└── tasks.md             # Phase 2 output (not created by /speckit.plan)
```

### Source Code (files changed by this feature)

```text
Asp.Net.Core.Learning.UI/
├── Models/
│   └── Product.cs              ← add Image property
└── Views/
    └── Catalog/
        └── Index.cshtml        ← rewrite to CSS Grid card layout
```

No changes required to:
- `Infrastructure/CatalogService.cs` — `GetFromJsonAsync` auto-deserialises the new `Image` property
- `Contracts/ICatalogService.cs` — interface signature unchanged
- `Controllers/CatalogController.cs` — controller unchanged
- `Program.cs` — no new service registrations required
- `wwwroot/` — card styles are scoped inline to the view; no separate CSS file required

**Structure Decision**: Single flat project (pre-existing); this feature touches exactly two files in the existing structure.

## Complexity Tracking

| Gate Failed | Why Exception Needed | Simpler Alternative Rejected Because |
|-------------|----------------------|--------------------------------------|
| G2 — No four-layer structure | UI project pre-dates this constitution; refactoring to four-layer MVC is a separate architectural initiative | Layering an MVC frontend would require significant restructuring out of scope for a UI card feature |
| G3 — No Domain project | Same as G2 | Same as G2 |
| G4 — No Application/Infrastructure separation | Same as G2 | Same as G2 |
| G10 — Cross-cutting concerns not wired | Pre-existing gap across all services; requires a dedicated cross-cutting hardening feature | Adding Serilog/health checks/OTel is out of scope for a UI card display feature |
| G11/G12 — No test projects | Pre-existing gap; no test infrastructure exists | Setting up test infrastructure is a separate concern out of scope here |
