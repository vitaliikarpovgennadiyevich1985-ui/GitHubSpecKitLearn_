# Implementation Plan: Catalog Product Image Field & 100-Product Endpoint

**Branch**: `001-catalog-product-image` | **Date**: 2026-05-05 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-catalog-product-image/spec.md`

## Summary

Add an `Image` field (Base64-encoded BMP binary image data) to the `Product` entity in the Catalog Microservice and update the `/products` endpoint to return 100 predefined in-memory products with stable IDs, all fields fully populated, and programmatically generated unique images. Images are 10×10 solid-color BMPs, each with a distinct hue — no external dependencies required.

## Technical Context

**Language/Version**: C# 13 / .NET 10  
**Primary Dependencies**: `Microsoft.AspNetCore.Authentication.JwtBearer 10.0.7` (existing); no new NuGet packages required — image generation uses only BCL (`System.IO`, `System.BitConverter`)  
**Storage**: None — 100-product catalog is a static in-memory array  
**Testing**: xUnit (planned — no test projects currently exist in this service)  
**Target Platform**: Windows / Linux container; ASP.NET Core Minimal API  
**Project Type**: Web service (microservice)  
**Performance Goals**: N/A — static in-memory dataset, negligible computation  
**Constraints**: No external NuGet packages for image generation; dataset is a compile-time constant  
**Scale/Scope**: 100 in-memory products; single flat project (existing structure)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Gate | Status |
|---|---|---|
| G1 | Service targets .NET 8+ with minimal hosting (`Program.cs` only, no `Startup.cs`) | ✅ .NET 10, `Program.cs` only |
| G2 | Service has four layers: Domain / Application / Infrastructure / Api (project references enforced) | ❌ Single flat project — pre-existing violation (see ADR below) |
| G3 | Domain project has zero dependencies on other layers or external packages | ❌ No Domain project — pre-existing violation |
| G4 | Application project does NOT reference Infrastructure | ❌ No Application project — pre-existing violation |
| G5 | No business logic placed in Controllers or Minimal API handlers | ❌ Product list returned from Minimal API handler — pre-existing pattern; mitigated by isolating data into `ProductCatalog` class |
| G6 | EF Core entities are NOT exposed outside the Infrastructure project | ✅ No EF Core used |
| G7 | Service owns its own database schema (no shared DB with another service) | ✅ No database — in-memory only |
| G8 | Inter-service communication uses async messaging (or HTTP/gRPC with written justification) | ✅ No inter-service communication introduced |
| G9 | Integration contracts (events/DTOs) are explicitly versioned | ✅ No integration events introduced |
| G10 | Structured logging, health checks, OpenTelemetry, FluentValidation, and global exception handling are wired | ❌ None wired — pre-existing violation |
| G11 | Unit tests planned for Domain + Application layers | ❌ No test projects exist — pre-existing violation; out of scope for this feature |
| G12 | Integration tests planned for Infrastructure + API layers (TestContainers for DB) | ❌ No test projects exist — pre-existing violation; out of scope for this feature |
| G13 | No hard-coded connection strings, secrets, or URLs | ✅ Auth authority from `builder.Configuration["IDENTITYSERVER_HTTPS"]`; image data is generated not hard-coded secrets |

**Post-Phase-1 Re-check**: G2–G5 and G10–G12 remain the same — no new violations introduced; G5 partially mitigated by `ProductCatalog` data class.

## Project Structure

### Documentation (this feature)

```text
specs/001-catalog-product-image/
├── plan.md              ← This file
├── research.md          ← Phase 0 output
├── data-model.md        ← Phase 1 output
├── quickstart.md        ← Phase 1 output
├── contracts/
│   └── products-endpoint.md  ← Phase 1 output
└── tasks.md             ← Phase 2 output (/speckit.tasks)
```

### Source Code

```text
Asp.Net.Core.Learning.CatalogMicroservice/
├── Models/
│   └── Product.cs                  ← Add Image field (string, non-null)
├── Data/                           ← NEW folder
│   ├── ProductCatalog.cs           ← NEW: static list of 100 products with stable IDs
│   └── ProductImageGenerator.cs   ← NEW: generates 10×10 BMP Base64 per unique hue
└── Program.cs                      ← Update /products handler to use ProductCatalog
```

**Structure Decision**: Existing flat single-project structure retained (see Complexity Tracking). A new `Data/` folder is introduced to separate the static catalog data and image generation logic from the API handler, partially mitigating G5.

## Complexity Tracking

| Gate Failed | Why Exception Needed | Simpler Alternative Rejected Because |
|-------------|----------------------|--------------------------------------|
| G2 — no 4-layer Clean Architecture | Pre-existing learning/demo service; this feature adds an `Image` field and 100 in-memory products — a 4-layer refactor is out of scope | Refactoring to Domain/Application/Infrastructure/Api projects is a separate, larger work item that should be planned independently |
| G3 — no Domain project | Same as G2 | Same as G2 |
| G4 — no Application project | Same as G2 | Same as G2 |
| G5 — data logic in Minimal API handler | Pre-existing pattern; mitigated by extracting 100-product list and image generation to `Data/ProductCatalog.cs` — handler becomes a thin delegate | Moving to a full Application-layer use-case is covered by the G2 exception |
| G10 — no cross-cutting concerns | Pre-existing; adding Serilog/health checks/OTel/FluentValidation is a cross-cutting concern story, separate from this feature | These concerns span all endpoints and are not introduced by the image field |
| G11 — no unit tests | No test projects exist in this service; test infrastructure is a separate work item | Test project scaffolding is out of scope for a field addition |
| G12 — no integration tests | Same as G11 | Same as G11 |
