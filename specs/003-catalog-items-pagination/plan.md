# Implementation Plan: Catalog Items Pagination

**Branch**: `003-catalog-items-pagination` | **Date**: 2026-05-06 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-catalog-items-pagination/spec.md`

## Summary

Add server-side pagination to the catalog read flow. The Catalog microservice's `/products` endpoint accepts optional `pageNumber` (1-based) and `pageSize` query-string parameters, applies a deterministic sort (`Title` asc, then `ProductId` asc), slices the in-memory catalog with `Skip`/`Take` (the only available data source today), and returns a `PagedResult<Product>` envelope (`items`, `totalCount`, `pageNumber`, `pageSize`). The UI's `CatalogService` is updated to call the new contract; the `CatalogController` accepts `pageNumber`/`pageSize` from the request and passes a `CatalogPageViewModel` to the view. The Razor view renders the existing 3-column card grid for the returned subset and adds a pagination footer: page-size selector (10/20/50/100), Previous/Next buttons, a windowed numbered page list (`1 … 4 5 6 … 99`), and a current/total page indicator. While a request is in flight the grid is replaced by a spinner and all controls are disabled. On request failure the grid is cleared and a full-page error block with a Retry button is rendered. No new NuGet packages, no client-side framework, no new endpoints introduced beyond the parameter additions on `/products`.

## Technical Context

**Language/Version**: C# 13 / .NET 10  
**Primary Dependencies**: ASP.NET Core Minimal APIs (Catalog microservice), ASP.NET Core MVC + Razor (UI). No new NuGet packages.  
**Storage**: Catalog microservice currently reads from an in-memory `ProductCatalog.All` (`IReadOnlyList<Product>`); no database. Pagination is applied via LINQ `OrderBy().ThenBy().Skip().Take()` plus `Count` on the same source. The shape (`Skip`/`Take` over `IQueryable`) is identical to what an EF Core implementation would emit, so no rework is required when persistence is later added.  
**Testing**: xUnit (planned, no test projects exist for either service today — pre-existing gap, not addressed by this feature).  
**Target Platform**: Windows / Linux container; ASP.NET Core 10.  
**Project Type**: Web application (Catalog microservice + MVC UI front-end).  
**Performance Goals**: Per-request response time and memory must not scale with total catalog size (SC-004, SC-005). For the current 100-item dataset the request budget is < 50 ms p95.  
**Constraints**: No new NuGet packages; no client-side JS framework (vanilla JS for fetch + DOM swap acceptable); response shape is fixed by FR-007 (`items`, `totalCount`, `pageNumber`, `pageSize`).  
**Scale/Scope**: 100 products today, designed to scale to 10,000+ without changes (SC-004/SC-005).

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Gate | Status |
|---|---|---|
| G1 | Service targets .NET 8+ with minimal hosting (`Program.cs` only, no `Startup.cs`) | ✅ .NET 10, `Program.cs` only in both projects |
| G2 | Service has four layers: Domain / Application / Infrastructure / Api (project references enforced) | ❌ Both projects are single flat projects — pre-existing violation; this feature does not introduce or worsen it |
| G3 | Domain project has zero dependencies on other layers or external packages | ❌ No Domain project — pre-existing violation |
| G4 | Application project does NOT reference Infrastructure | ❌ No layered separation — pre-existing violation |
| G5 | No business logic placed in Controllers or Minimal API handlers | ⚠️ Paging LINQ is encapsulated in `ProductCatalog.GetPage(pageNumber, pageSize)`; the Minimal API handler is a one-line dispatch. See ADR in **Complexity Tracking** below. |
| G6 | EF Core entities are NOT exposed outside the Infrastructure project | ✅ N/A — no EF Core in use yet |
| G7 | Service owns its own database schema (no shared DB with another service) | ✅ N/A — no database |
| G8 | Inter-service communication uses async messaging (or HTTP/gRPC with written justification) | ✅ UI→microservice HTTP is user-facing synchronous; not service-to-service messaging |
| G9 | Integration contracts (events/DTOs) are explicitly versioned | ⚠️ The `/products` contract changes shape (now returns a wrapper). See ADR in **Complexity Tracking**. |
| G10 | Structured logging, health checks, OpenTelemetry, FluentValidation, and global exception handling are wired | ❌ Pre-existing violation — out of scope for this feature |
| G11 | Unit tests planned for Domain + Application layers | ❌ No test projects exist — pre-existing violation, out of scope |
| G12 | Integration tests planned for Infrastructure + API layers (TestContainers for DB) | ❌ No test projects exist — pre-existing violation, out of scope |
| G13 | No hard-coded connection strings, secrets, or URLs | ✅ Aspire service discovery is used; no hard-coded URLs introduced |

**Post-Phase-1 re-check**: gate statuses are unchanged after Phase 1 design. The two soft warnings on G5 and G9 are documented in **Complexity Tracking** below; no new violations introduced.

## Project Structure

### Documentation (this feature)

```text
specs/003-catalog-items-pagination/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── products-endpoint.md
└── tasks.md             # Phase 2 output (not created by /speckit.plan)
```

### Source Code (files changed by this feature)

```text
Asp.Net.Core.Learning.CatalogMicroservice/
├── Models/
│   └── PagedResult.cs                   ← NEW: response envelope
├── Data/
│   └── ProductCatalog.cs                ← add GetPage(pageNumber, pageSize)
└── Program.cs                           ← /products handler accepts pageNumber/pageSize

Asp.Net.Core.Learning.UI/
├── Models/
│   ├── PagedResult.cs                   ← NEW: deserialization target (mirrors microservice)
│   └── CatalogPageViewModel.cs          ← NEW: view model passed to Index.cshtml
├── Contracts/
│   └── ICatalogService.cs               ← GetProducts(pageNumber, pageSize)
├── Infrastructure/
│   └── CatalogService.cs                ← pass pageNumber/pageSize as query string
├── Controllers/
│   └── CatalogController.cs             ← accept ?pageNumber=&pageSize=, build view model
└── Views/Catalog/
    └── Index.cshtml                     ← add pagination footer + loading + error blocks
```

No changes required to authentication, authorization, configuration, layout, or `wwwroot/`.

**Structure Decision**: Both projects retain their existing single-project flat layout (pre-existing convention). This feature adds 3 new files and modifies 6 existing files; no folder reorganisation.

## Complexity Tracking

| Gate Failed | Why Exception Needed | Simpler Alternative Rejected Because |
|-------------|----------------------|--------------------------------------|
| G5 — paging LINQ runs adjacent to the Minimal API handler | The microservice has no Application layer. Introducing Domain/Application/Infrastructure/Api projects to host a `GetProductsPagedQuery` handler is a multi-day refactor disproportionate to a single LINQ pipeline. The handler is a one-line dispatch to `ProductCatalog.GetPage`, so the rule's intent (no business logic in handlers) is honored even though the project layering is not. | A future ticket can lift `ProductCatalog.GetPage` into an `IGetProductsPagedQuery` in a new Application project; the handler will then be a one-line dispatch. The current shape is forward-compatible. |
| G9 — `/products` response shape changes from `Product[]` to `PagedResult<Product>` | The endpoint has exactly one consumer (the UI), and both ship in the same solution and same deployment. Side-by-side `/products/v2` would double the surface for zero external benefit. | When a second consumer is added, version the contract with a new path or `Accept` header at that point; for now in-place evolution is documented in [contracts/products-endpoint.md](contracts/products-endpoint.md). |
