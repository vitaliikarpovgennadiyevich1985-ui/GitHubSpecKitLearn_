# Phase 1 Data Model — Catalog Items Pagination

**Feature**: 003-catalog-items-pagination  
**Date**: 2026-05-06

This feature does not introduce a new domain entity. It introduces one transport/DTO type and one UI view-model.

## Catalog Item (existing)

Unchanged. Reproduced here for reference only.

| Field | Type | Notes |
|---|---|---|
| `ProductId` | `string` (GUID) | Primary identifier; tie-breaker for the deterministic sort. |
| `Title` | `string` | Primary sort key (ascending). Non-empty. |
| `Description` | `string` | Non-empty. |
| `Price` | `decimal` | > 0. |
| `Image` | `string` (base64 BMP) | Non-empty. |

The deterministic ordering for paging is `OrderBy(p => p.Title).ThenBy(p => p.ProductId)` (FR-016).

## PagedResult\<T\> (NEW — microservice + UI)

Represents one page of any collection. Defined symmetrically in both projects so each owns its serialization shape (microservice as the producer, UI as the consumer / model class).

| Field | Type | Description |
|---|---|---|
| `Items` | `IReadOnlyList<T>` (microservice) / `List<T>` (UI) | The current page of items, in the deterministic sort order. May be empty when `pageNumber` exceeds `totalPages`. |
| `TotalCount` | `int` | Count of all items across all pages. `>= 0`. |
| `PageNumber` | `int` | Echo of the **effective** page number (after normalisation). `>= 1`. |
| `PageSize` | `int` | Echo of the **effective** page size (after normalisation and clamping). `1 <= PageSize <= 100`. |

**Validation rules** (applied by `ProductCatalog.GetPage` before constructing the result):
- `pageNumber` null or `< 1` → coerce to `1` (FR-002, FR-005).
- `pageSize` null or `< 1` → coerce to `20` (FR-003, FR-005).
- `pageSize > 100` → coerce to `100` (FR-004).
- `pageNumber > totalPages` → return `Items = []`, all other fields populated (FR-008).

**Derived values** (computed by the UI, not transported):
- `TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize)`. When `TotalCount == 0`, `TotalPages = 0` and the pager renders an empty-catalog state.
- `HasPrevious = PageNumber > 1` (drives FR-012 disabled state on the Previous button).
- `HasNext = PageNumber < TotalPages` (drives FR-012 disabled state on the Next button).

## CatalogPageViewModel (NEW — UI only)

Razor view model passed from `CatalogController` to `Views/Catalog/Index.cshtml` and the partials.

| Field | Type | Description |
|---|---|---|
| `Items` | `IReadOnlyList<Product>` | The page's products, ready for the existing card grid. |
| `PageNumber` | `int` | Effective page number from the response. |
| `PageSize` | `int` | Effective page size from the response. |
| `TotalCount` | `int` | From the response, used to compute `TotalPages` in the view. |
| `TotalPages` | `int` (computed) | `(int)Math.Ceiling(TotalCount / (double)PageSize)`; `0` when `TotalCount = 0`. |
| `PageWindow` | `IReadOnlyList<int?>` | The numbered page list to render. Each element is either a page number or `null` to indicate an ellipsis (FR-021). |
| `AvailablePageSizes` | `IReadOnlyList<int>` | Always `[10, 20, 50, 100]` (FR-018). |

**Window builder** (R-006):
1. Start with the set `{1, totalPages, currentPage - 1, currentPage, currentPage + 1}`, dropping any value outside `[1, totalPages]`.
2. Sort ascending.
3. Walk the sorted list; whenever consecutive entries differ by more than 1, insert an ellipsis (`null`) between them.
4. Result is a list of `int?`.

## State transitions (UI only)

The catalog page exists in exactly one of these states at any moment:

| State | Trigger | Render |
|---|---|---|
| **Idle (data shown)** | Successful response received | Card grid + pager footer |
| **Loading** | User clicks Previous/Next/page number, or changes page size, or clicks Retry | Spinner replaces grid; pager controls disabled |
| **Error** | Fetch promise rejects, or response is non-2xx | Full-page error block with Retry button; pager hidden |
| **Empty catalog** | `TotalCount == 0` on a successful response | Empty state message; pager hidden |

The "Idle" → "Loading" → ("Idle" \| "Error") cycle is the only loop; "Empty catalog" is an Idle sub-state.
