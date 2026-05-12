# Tasks: Catalog Product Image Field & 100-Product Endpoint

**Input**: Design documents from `specs/001-catalog-product-image/`  
**Prerequisites**: [plan.md](plan.md) · [spec.md](spec.md) · [research.md](research.md) · [data-model.md](data-model.md) · [contracts/products-endpoint.md](contracts/products-endpoint.md)

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Parallelizable — operates on a different file, no dependency on an in-progress task
- **[US1] / [US2]**: User story this task belongs to
- No `[P]` or `[Story]` = setup/foundational or polish phase task

---

## Phase 1: Setup

**Purpose**: Create the new `Data/` folder structure required by the plan.

- [X] T001 Create folder `Asp.Net.Core.Learning.CatalogMicroservice/Data/` (empty placeholder — subsequent tasks populate it)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared building blocks that both user stories depend on. Must complete before Phase 3 and Phase 4.

**⚠️ CRITICAL**: Both user stories share `Product.cs` and `ProductImageGenerator.cs`. These must be complete before any story-specific work begins.

- [X] T002 Add `Image` property (`public string Image { get; set; }`) to `Asp.Net.Core.Learning.CatalogMicroservice/Models/Product.cs` — insert after the `Price` property, keeping existing properties unchanged (FR-001)
- [X] T003 Create `Asp.Net.Core.Learning.CatalogMicroservice/Data/ProductImageGenerator.cs` — static class with a single method `GenerateBase64Bmp(byte r, byte g, byte b): string` that constructs a 374-byte 10×10 24-bit BMP in memory (14-byte BITMAPFILEHEADER + 40-byte BITMAPINFOHEADER + 320-byte BGR pixel rows with 2-byte row padding) using only BCL (`System.IO.MemoryStream`, `System.BitConverter`, `System.Convert.ToBase64String`) and returns the result as a Base64 string — no NuGet packages (see research.md §4 for exact byte layout)

**Checkpoint**: `Product.Image` exists; `ProductImageGenerator.GenerateBase64Bmp` produces a non-empty Base64 string for any RGB input — foundational work is unblocked.

---

## Phase 3: User Story 1 — Retrieve Products with Image (Priority: P1) 🎯 MVP

**Goal**: `GET /products` returns exactly 100 products, each with all fields populated including a unique non-empty Base64 BMP `Image`.

**Independent Test**: Call `GET /products` with a valid token → assert `response.Count == 100`, every item has non-empty `Image`, `ProductId`, `Title`, `Description`, and `Price > 0`.

### Implementation for User Story 1

- [X] T004 [US1] Create `Asp.Net.Core.Learning.CatalogMicroservice/Data/ProductCatalog.cs` — static class with a single `static readonly IReadOnlyList<Product> All` property. Build the list once in the static constructor: for each `i` in `[0, 99]`, derive hue as `i * 3.6` degrees, convert HSV (H=hue, S=1, V=1) to RGB using the standard 6-sector algorithm (see research.md §2), call `ProductImageGenerator.GenerateBase64Bmp(r, g, b)`, set `ProductId` to `new Guid(System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes($"product-{i}"))).ToString()`, `Title = $"Product {i + 1}"`, `Description = $"Description for Product {i + 1}"`, `Price = (i + 1) * 10m` (FR-002, FR-003, FR-004, FR-005, FR-007)
- [X] T005 [US1] Update the `app.MapGet("/products", ...)` handler in `Asp.Net.Core.Learning.CatalogMicroservice/Program.cs` to return `ProductCatalog.All` instead of the inline two-item list — handler body becomes a single `return ProductCatalog.All;`; add `using Asp.Net.Core.Learning.CatalogMicroservice.Data;` at the top if needed; remove the unused `IHttpContextAccessor` parameter (FR-002, FR-006)

**Checkpoint**: Run the service, call `GET /products` with a valid token — response contains exactly 100 items, each with a non-empty `Image` field. SC-001, SC-002, SC-004, SC-005 satisfied.

---

## Phase 4: User Story 2 — Product Data Completeness (Priority: P2)

**Goal**: Every one of the 100 products has no null or empty value for any field — verified structurally by inspecting the catalog initialization logic.

**Independent Test**: Iterate all 100 products from `ProductCatalog.All` and assert no field is null or empty/zero — can be validated at startup or via the `/products` response.

### Implementation for User Story 2

- [X] T006 [P] [US2] Add a startup validation guard in `Asp.Net.Core.Learning.CatalogMicroservice/Data/ProductCatalog.cs` — immediately after building the list in the static constructor, assert `All.Count == 100` and that no product has a null/empty `ProductId`, `Title`, `Description`, `Image`, or `Price <= 0`; throw `InvalidOperationException` with a descriptive message if any assertion fails so data defects surface at startup rather than silently at runtime (FR-003, SC-003)
- [X] T007 [P] [US2] Verify `Image` serialization: confirm that ASP.NET Core's default `System.Text.Json` serializer includes the `Image` property in the JSON output by checking the `Product` class has no `[JsonIgnore]` or access-modifier issue that would suppress it — no code change needed if the property is `public`; add `[JsonPropertyName("image")]` annotation only if the default camelCase serialization differs from the contract in `contracts/products-endpoint.md` (SC-002, SC-004)

**Checkpoint**: Application starts without throwing; `GET /products` response has all fields populated for all 100 products. SC-003 satisfied.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Clean up the handler signature and remove dead code left from the original two-product stub.

- [X] T008 Remove the now-unused `builder.Services.AddHttpContextAccessor()` call from `Asp.Net.Core.Learning.CatalogMicroservice/Program.cs` if `IHttpContextAccessor` is no longer injected anywhere in the service (the `/products` handler no longer uses it)
- [X] T009 [P] Verify the project builds with zero warnings (`dotnet build -warnaserror`) and fix any nullable reference warnings introduced by the new `Image` property on `Product` — if `<Nullable>enable</Nullable>` is set in the `.csproj`, ensure `Image` is initialized (e.g., `= string.Empty` or `required`) consistent with the existing property style

---

## Dependency Graph

```
T001 (create Data/ folder)
  └─► T002 (add Image to Product.cs)
  └─► T003 (ProductImageGenerator)
        └─► T004 (ProductCatalog — depends on T002 + T003)
              ├─► T005 (update Program.cs handler — depends on T004)
              └─► T006 (startup validation — depends on T004)
  └─► T007 (serialization check — depends on T002, independent of T003–T006)
T008 (cleanup — depends on T005)
T009 (build verification — depends on all above)
```

**Parallel opportunities**:
- T002 and T003 can be worked in parallel (different files).
- T006 and T007 can be worked in parallel after their respective prerequisites complete.
- T008 and T009 can be worked in parallel once T005 is done.

---

## Implementation Strategy

| Phase | Scope | Delivers |
|---|---|---|
| **MVP** (P1 only) | T001 → T003 → T004 → T005 | Working `/products` endpoint with 100 products and unique `Image` fields |
| **Complete** (P1 + P2) | + T006, T007 | Data completeness guard + serialization verification |
| **Final** | + T008, T009 | Clean build, no dead code |

**Suggested MVP sequence**: T001 → T002 + T003 (parallel) → T004 → T005 → verify SC-001..SC-005.

---

## Summary

| Metric | Value |
|---|---|
| Total tasks | 9 |
| User Story 1 tasks | 2 (T004, T005) |
| User Story 2 tasks | 2 (T006, T007) |
| Foundational tasks | 2 (T002, T003) |
| Setup tasks | 1 (T001) |
| Polish tasks | 2 (T008, T009) |
| Parallel opportunities | T002‖T003, T006‖T007, T008‖T009 |
| MVP scope | T001–T005 (User Story 1 complete) |
| Format validation | ✅ All tasks have checkbox, sequential ID, labels where required, and exact file paths |
