# Tasks: Catalog Items Pagination

**Input**: Design documents from `/specs/003-catalog-items-pagination/`
**Prerequisites**: `plan.md` (required), `spec.md` (required for user stories), `research.md`, `data-model.md`, `contracts/products-endpoint.md`, `quickstart.md`

**Tests**: No explicit TDD/test-task mandate exists in the feature spec, so this task list focuses on implementation and manual validation via `quickstart.md`.

**Organization**: Tasks are grouped by user story so each story remains independently implementable and verifiable.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Add shared pagination models and view-model scaffolding used by all stories.

- [X] T001 Create pagination response model in Asp.Net.Core.Learning.CatalogMicroservice/Models/PagedResult.cs
- [X] T002 [P] Create pagination response model in Asp.Net.Core.Learning.UI/Models/PagedResult.cs
- [X] T003 [P] Create catalog paging view model in Asp.Net.Core.Learning.UI/Models/CatalogPageViewModel.cs
- [X] T004 [P] Create paged catalog partial view scaffold in Asp.Net.Core.Learning.UI/Views/Catalog/_CatalogPage.cshtml

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Wire shared contracts and request/response plumbing that all user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T005 Update paged service contract signature in Asp.Net.Core.Learning.UI/Contracts/ICatalogService.cs
- [X] T006 Update HTTP catalog client to pass pageNumber/pageSize and parse paged response in Asp.Net.Core.Learning.UI/Infrastructure/CatalogService.cs
- [X] T007 Add controller mapping helpers for paged results to CatalogPageViewModel in Asp.Net.Core.Learning.UI/Controllers/CatalogController.cs
- [X] T008 Update products endpoint signature to accept pageNumber/pageSize query params in Asp.Net.Core.Learning.CatalogMicroservice/Program.cs

**Checkpoint**: Foundation ready. User stories can now be implemented in priority order.

---

## Phase 3: User Story 1 - Browse catalog one page at a time (Priority: P1) 🎯 MVP

**Goal**: Return and render only one page of products (default 20), with page-to-page navigation driven by new backend calls.

**Independent Test**: Open catalog page, verify 20 cards shown on first load, click Next then Previous and confirm card set and page indicator update correctly.

- [X] T009 [US1] Implement ProductCatalog.GetPage(pageNumber, pageSize) returning items/totalCount/pageNumber/pageSize in Asp.Net.Core.Learning.CatalogMicroservice/Data/ProductCatalog.cs
- [X] T010 [US1] Wire /products handler to call ProductCatalog.GetPage and return paged envelope in Asp.Net.Core.Learning.CatalogMicroservice/Program.cs
- [X] T011 [US1] Implement Catalog/Index paged data flow using pageNumber/pageSize defaults in Asp.Net.Core.Learning.UI/Controllers/CatalogController.cs
- [X] T012 [P] [US1] Implement card-grid rendering for returned page items in Asp.Net.Core.Learning.UI/Views/Catalog/_CatalogPage.cshtml
- [X] T013 [US1] Render catalog host and current page indicator from CatalogPageViewModel in Asp.Net.Core.Learning.UI/Views/Catalog/Index.cshtml
- [X] T014 [US1] Add Previous/Next click handlers that trigger API calls and replace catalog host content in Asp.Net.Core.Learning.UI/Views/Catalog/Index.cshtml
- [X] T015 [US1] Add paged partial endpoint action for async page refreshes in Asp.Net.Core.Learning.UI/Controllers/CatalogController.cs

**Checkpoint**: User Story 1 is independently functional and demo-ready (MVP).

---

## Phase 4: User Story 2 - Disable boundaries and improve pager controls (Priority: P2)

**Goal**: Prevent invalid navigation and allow faster page access via page-size selector and windowed numbered links.

**Independent Test**: Verify Previous is disabled on first page, Next disabled on last page, page-size selector resets to page 1 and reloads, numbered page links navigate correctly.

- [X] T016 [US2] Disable Previous/Next controls at page boundaries in Asp.Net.Core.Learning.UI/Views/Catalog/_CatalogPage.cshtml
- [X] T017 [US2] Add page-size selector (10/20/50/100, default 20) to pager UI in Asp.Net.Core.Learning.UI/Views/Catalog/_CatalogPage.cshtml
- [X] T018 [US2] Add windowed numbered page list with ellipsis rendering in Asp.Net.Core.Learning.UI/Views/Catalog/_CatalogPage.cshtml
- [X] T019 [US2] Implement page-window builder and selector reset-to-page-1 behavior in Asp.Net.Core.Learning.UI/Controllers/CatalogController.cs
- [X] T020 [US2] Handle numbered-page clicks and page-size changes in async pagination script in Asp.Net.Core.Learning.UI/Views/Catalog/Index.cshtml

**Checkpoint**: User Stories 1 and 2 both work independently with robust paging UX.

---

## Phase 5: User Story 3 - Efficient handling and resilient page transitions (Priority: P2)

**Goal**: Ensure deterministic and bounded backend pagination for large datasets, plus resilient UI loading/error behavior.

**Independent Test**: Verify deterministic ordering across adjacent pages, out-of-range page returns empty items, loading spinner appears during in-flight requests, and failure shows retryable full-page error.

- [X] T021 [US3] Enforce deterministic ordering (Title asc, ProductId asc), parameter normalization, and pageSize clamping in Asp.Net.Core.Learning.CatalogMicroservice/Data/ProductCatalog.cs
- [X] T022 [US3] Handle empty-page and empty-catalog render states for paged responses in Asp.Net.Core.Learning.UI/Views/Catalog/_CatalogPage.cshtml
- [X] T023 [US3] Replace grid with loading indicator and disable pager controls while requests are in flight in Asp.Net.Core.Learning.UI/Views/Catalog/Index.cshtml
- [X] T024 [US3] Render full-page error block with Retry action when page request fails in Asp.Net.Core.Learning.UI/Views/Catalog/Index.cshtml
- [X] T025 [US3] Ignore stale async responses from rapid user clicks using request sequencing in Asp.Net.Core.Learning.UI/Views/Catalog/Index.cshtml

**Checkpoint**: All user stories are independently functional with resilient behavior and bounded server-side pagination.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final consistency pass across docs, contracts, and runtime behavior.

- [X] T026 Update endpoint behavior/examples to match final implementation in specs/003-catalog-items-pagination/contracts/products-endpoint.md
- [X] T027 [P] Run and align manual validation steps with implemented UI/API behavior in specs/003-catalog-items-pagination/quickstart.md
- [X] T028 Clean up pagination-related dead code and naming inconsistencies in Asp.Net.Core.Learning.CatalogMicroservice/Program.cs and Asp.Net.Core.Learning.UI/Controllers/CatalogController.cs

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies; start immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1; blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2; defines MVP.
- **Phase 4 (US2)**: Depends on Phase 3 outputs (pager UI and controller mapping).
- **Phase 5 (US3)**: Depends on Phase 3 outputs; can proceed in parallel with late Phase 4 work once baseline paging is complete.
- **Phase 6 (Polish)**: Depends on completion of selected user stories.

### User Story Dependencies

- **US1 (P1)**: Starts after Foundational; no dependency on other stories.
- **US2 (P2)**: Depends on US1 pager baseline (`_CatalogPage.cshtml`, `Index.cshtml`, `CatalogController.cs`).
- **US3 (P2)**: Depends on US1 baseline paging flow; independent from most US2 tasks except shared view file merge coordination.

### Within Each User Story

- Backend paging contract and API behavior first.
- Controller/view-model mapping second.
- Razor rendering and script interactions third.
- Error/loading/resilience refinements last.

---

## Parallel Opportunities

- **Setup**: `T002`, `T003`, and `T004` can run in parallel after `T001` starts.
- **US1**: `T012` can run in parallel with backend tasks `T009`/`T010` once response shape is known.
- **US2**: `T017` and `T018` can be split between two developers, then merged before `T020`.
- **US3**: `T022` (partial markup) can run parallel to `T021` (backend normalization/sort).
- **Polish**: `T027` can run in parallel with `T026`.

---

## Parallel Example: User Story 1

```bash
# Backend and view work can proceed together once paged envelope is defined:
Task: "Implement ProductCatalog.GetPage(pageNumber, pageSize) returning items/totalCount/pageNumber/pageSize in Asp.Net.Core.Learning.CatalogMicroservice/Data/ProductCatalog.cs"
Task: "Implement card-grid rendering for returned page items in Asp.Net.Core.Learning.UI/Views/Catalog/_CatalogPage.cshtml"
```

## Parallel Example: User Story 2

```bash
# Pager UI can be split before script wiring:
Task: "Add page-size selector (10/20/50/100, default 20) to pager UI in Asp.Net.Core.Learning.UI/Views/Catalog/_CatalogPage.cshtml"
Task: "Add windowed numbered page list with ellipsis rendering in Asp.Net.Core.Learning.UI/Views/Catalog/_CatalogPage.cshtml"
```

## Parallel Example: User Story 3

```bash
# Backend correctness and UI resiliency can progress concurrently:
Task: "Enforce deterministic ordering (Title asc, ProductId asc), parameter normalization, and pageSize clamping in Asp.Net.Core.Learning.CatalogMicroservice/Data/ProductCatalog.cs"
Task: "Replace grid with loading indicator and disable pager controls while requests are in flight in Asp.Net.Core.Learning.UI/Views/Catalog/Index.cshtml"
```

---

## Implementation Strategy

### MVP First (US1 only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 (US1).
3. Validate US1 independently using quickstart checks: first page shows 20 items, Next/Previous trigger new API calls, and indicator updates.
4. Demo/deploy MVP.

### Incremental Delivery

1. Deliver US1 (core server-side pagination).
2. Deliver US2 (boundary-safe navigation + page-size + numbered list).
3. Deliver US3 (deterministic ordering + loading/error resiliency + stale-response guard).
4. Finish with Phase 6 polish.

### Parallel Team Strategy

1. Team aligns on Phase 1-2 contracts.
2. Developer A owns microservice paging (`ProductCatalog.cs`, `Program.cs`).
3. Developer B owns controller/view-model mapping (`CatalogController.cs`, models).
4. Developer C owns Razor + JS pager UX (`Index.cshtml`, `_CatalogPage.cshtml`).

---

## Notes

- `[P]` tasks target different files or low-conflict segments and are safe to parallelize.
- Every user-story task includes a concrete file path.
- Keep commits small per phase or per user-story checkpoint.
- Re-run quickstart validation after each story checkpoint to preserve independent testability.
