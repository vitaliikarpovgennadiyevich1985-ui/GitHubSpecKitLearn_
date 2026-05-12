# Feature Specification: Catalog Items Pagination

**Feature Branch**: `003-catalog-items-pagination`  
**Created**: 2026-05-06  
**Status**: Draft  
**Input**: User description: "Add server-side pagination support for catalog items. Return 20 items per page by default. Introduce pageNumber (1-based) and pageSize parameters. Update the catalog microservice to: accept pageNumber and pageSize, apply pagination at the database/query level (do not load all items into memory), return only the requested subset of items, include in the response: items, totalCount, pageNumber, pageSize. Update the main UI project to: pass pageNumber and pageSize in API requests, render only the returned items (no client-side pagination), add pagination controls (Next, Previous, current page indicator), trigger API calls when page changes. Constraints: do not return all catalog items at once; do not implement client-side pagination; follow existing clean architecture structure."

## Clarifications

### Session 2026-05-06

- Q: How are items ordered across pages to guarantee no duplicates or skips? → A: Order alphabetically by `Title` ascending, with `Id` ascending as a tie-breaker.
- Q: What does the UI do when a page request to the backend fails (network error, 500, timeout)? → A: Clear the grid, show a full-page error message with a Retry button; navigation controls disabled until retry succeeds.
- Q: Should the UI expose a page-size selector? → A: Yes — selector with options 10 / 20 / 50 / 100; changing it triggers a new request and resets to page 1.
- Q: What does the UI show while a page request is in flight? → A: Replace the grid with a spinner/skeleton; navigation and page-size controls disabled until the response arrives.
- Q: Does the UI need a way to jump directly to a specific page beyond Previous/Next? → A: Yes — render a numbered page list (1, 2, 3, ..., N) with windowing for large catalogs.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Browse catalog one page at a time (Priority: P1)

A shopper opens the catalog page and is shown the first 20 products immediately, instead of waiting for all products to load. They can move forward and backward through pages using on-screen controls and always see which page they are on relative to the total number of pages.

**Why this priority**: This is the core value of the feature — bounding the amount of data delivered per request and presenting catalog content in digestible chunks. Without it, neither the performance nor the UX goal is met.

**Independent Test**: Open the catalog page in a browser, confirm 20 product cards render, click "Next" and confirm a new set of products is shown, click "Previous" and confirm the original set returns. Confirm the page indicator updates accordingly.

**Acceptance Scenarios**:

1. **Given** the catalog contains at least 21 products, **When** the user navigates to the catalog page, **Then** the system displays exactly 20 product cards and shows "Page 1 of N" with N reflecting the total number of pages.
2. **Given** the user is on page 1 and additional pages exist, **When** the user clicks "Next", **Then** a new request is sent for page 2, the next 20 products are rendered, and the page indicator updates to "Page 2 of N".
3. **Given** the user is on page 2, **When** the user clicks "Previous", **Then** a new request is sent for page 1, the original 20 products are rendered, and the page indicator updates to "Page 1 of N".

---

### User Story 2 - Disable navigation at boundaries (Priority: P2)

When the shopper is on the first page, the "Previous" control is disabled. When on the last page, the "Next" control is disabled. This prevents wasted requests and dead-end navigation.

**Why this priority**: Improves usability and avoids both unnecessary network traffic and confusing empty results, but the catalog is still usable without it.

**Independent Test**: Navigate to page 1 and confirm "Previous" is disabled. Navigate to the last page and confirm "Next" is disabled.

**Acceptance Scenarios**:

1. **Given** the user is on page 1, **When** the page renders, **Then** the "Previous" control is visibly disabled and clicking it has no effect.
2. **Given** the user is on the last available page, **When** the page renders, **Then** the "Next" control is visibly disabled and clicking it has no effect.

---

### User Story 3 - Efficient handling of large catalogs (Priority: P2)

The system can serve catalogs containing tens of thousands of items without measurable degradation in response time, because only the requested page is materialized from storage.

**Why this priority**: Validates the non-functional intent of the feature — server-side, query-level pagination. Important for production-readiness but not directly user-visible until catalog grows.

**Independent Test**: Seed the catalog with 10,000 items and verify that requesting any page returns within the same time budget as requesting page 1, and that backend memory consumption per request remains bounded (independent of total catalog size).

**Acceptance Scenarios**:

1. **Given** the catalog contains 10,000 items, **When** any single page is requested, **Then** the response is returned within the same performance budget as a request against a 100-item catalog.
2. **Given** the catalog contains 10,000 items, **When** a page is requested, **Then** the backend does not load all 10,000 items into memory before slicing.

---

### Edge Cases

- **Page beyond range**: When `pageNumber` exceeds the number of available pages, the response returns an empty `items` array with the requested `pageNumber` and `pageSize` echoed back, plus the correct `totalCount`. The UI shows an "no products on this page" state and re-enables navigation back to a valid page.
- **Empty catalog**: When the catalog has zero items, the response returns an empty `items` array with `totalCount = 0`. The UI shows an empty state and disables both navigation controls.
- **Invalid parameters**: When `pageNumber < 1` or `pageSize < 1` or `pageSize > MaxPageSize`, the system substitutes safe defaults (`pageNumber = 1`, `pageSize = 20`, or `pageSize = MaxPageSize` respectively) rather than returning an error.
- **Total count changes between requests**: If items are added or removed between two page requests, the user simply sees the next request reflect the new state; no special reconciliation is performed.
- **Concurrent requests**: If a user clicks "Next" rapidly, only the most recent request's results are rendered.
- **Backend request failure**: If a page request to the catalog microservice fails (network error, non-success HTTP status, or timeout), the UI clears the product grid, displays a full-page error message with a "Retry" button, and keeps the "Previous" and "Next" controls disabled until the user successfully retries the request.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The catalog microservice MUST accept `pageNumber` (1-based integer) and `pageSize` (positive integer) as request parameters when listing catalog items.
- **FR-002**: When `pageNumber` is omitted, the catalog microservice MUST default it to `1`.
- **FR-003**: When `pageSize` is omitted, the catalog microservice MUST default it to `20`.
- **FR-004**: The catalog microservice MUST clamp `pageSize` to a maximum of `100` to prevent abusive requests.
- **FR-005**: The catalog microservice MUST normalize `pageNumber < 1` to `1` and `pageSize < 1` to the default of `20`.
- **FR-006**: The catalog microservice MUST apply pagination at the persistence/query layer such that only the requested subset of items is materialized into memory for the response.
- **FR-007**: The catalog microservice MUST return a response object containing exactly the fields: `items` (the requested page of catalog items), `totalCount` (total number of items across all pages), `pageNumber` (echo of the effective page number), and `pageSize` (echo of the effective page size).
- **FR-008**: When `pageNumber` exceeds the number of available pages, the catalog microservice MUST return an empty `items` array along with the correct `totalCount`, `pageNumber`, and `pageSize` (no error response).
- **FR-009**: The UI MUST send the current `pageNumber` and `pageSize` to the catalog microservice on every page request.
- **FR-010**: The UI MUST render only the items returned by the most recent response (no accumulation across pages, no client-side slicing).
- **FR-011**: The UI MUST display "Previous" and "Next" navigation controls together with a current page indicator showing both the current page number and the total number of pages, plus a numbered page list (see FR-020 and FR-021).
- **FR-012**: The UI MUST disable the "Previous" control when the current page is `1` and disable the "Next" control when the current page equals the total number of pages.
- **FR-013**: The UI MUST trigger a new request to the catalog microservice whenever the user changes the page, and replace the rendered items with the response.
- **FR-014**: The UI MUST NOT request the full catalog or perform client-side pagination over a cached full list.
- **FR-015**: The catalog microservice and the UI MUST follow the existing clean architecture / layering conventions already present in their respective projects (no new architectural patterns introduced).
- **FR-016**: The catalog microservice MUST apply a deterministic, stable ordering at the query layer for every paged request: ascending by `Title`, then ascending by `Id` as the tie-breaker, so that page composition is consistent across requests.
- **FR-017**: When a page request to the catalog microservice fails (network error, non-success HTTP status, or timeout), the UI MUST clear the product grid, display a full-page error message with a "Retry" button, and disable the "Previous" and "Next" controls until the retry succeeds.
- **FR-018**: The UI MUST display a page-size selector with the options `10`, `20`, `50`, and `100` (default `20`). When the user changes the selected page size, the UI MUST reset the current page to `1` and trigger a new request to the catalog microservice with the new `pageSize`.
- **FR-019**: While a page request is in flight, the UI MUST replace the product grid with a loading indicator (spinner or skeleton) and disable the "Previous", "Next", and page-size controls until the response arrives or the request fails.
- **FR-020**: The UI MUST render a numbered page list alongside the "Previous" and "Next" controls. Each visible number MUST be clickable and trigger a request for that specific page. The current page MUST be visually highlighted and not clickable.
- **FR-021**: When the total number of pages is large, the numbered page list MUST use windowing: it MUST show the first page, the last page, a contiguous window around the current page, and ellipsis markers (e.g., `1 … 4 5 6 … 99`) so that the control's width does not grow unbounded with total page count.

### Key Entities

- **Catalog Item**: An existing product entity exposed by the catalog microservice. Pagination does not change its shape; only the way collections of it are returned changes.
- **Paged Result**: A response wrapper containing `items` (a subset of catalog items), `totalCount` (count of all matching items), `pageNumber` (effective requested page), and `pageSize` (effective requested page size).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A default catalog request returns at most 20 items.
- **SC-002**: Navigating from one page to another results in exactly one new backend request and the visible product set changes accordingly.
- **SC-003**: The current page indicator always reflects the page actually displayed and the correct total number of pages, computed from `totalCount` and `pageSize`.
- **SC-004**: With a catalog of 10,000 items, the time to return any single page is within the same performance envelope as returning the first page of a 100-item catalog (no scaling of latency with total count).
- **SC-005**: With a catalog of 10,000 items, the backend memory used to serve any single page request does not scale with total catalog size.
- **SC-006**: Requesting a page beyond the last available page returns an empty result set and an HTTP success status, never an error.
- **SC-007**: The "Previous" control is unusable on page 1 and the "Next" control is unusable on the last page, in 100% of test runs.

## Assumptions

- The default page size of `20` and the maximum page size of `100` are reasonable defaults for a typical catalog; they were not explicitly specified in the feature description. The UI exposes a page-size selector with options `10`, `20`, `50`, `100` (see FR-018).
- Items within a page are returned in a deterministic order: ascending `Title`, then ascending `Id` (see FR-016). Introducing user-facing sorting controls is out of scope for this feature.
- Filtering/searching the catalog is out of scope for this feature; pagination operates over the full catalog.
- The UI continues to render product cards using the existing visual layout (grid of cards) introduced in feature 002; only the dataset feeding the grid changes.
- No URL deep-linking to specific pages is required; clicking "Next" or "Previous" updates the in-page state but is not required to update the browser address bar.
- Authentication and authorization rules for the catalog endpoint are unchanged.
- The catalog microservice's existing persistence layer supports server-side paging (e.g., `Skip`/`Take` translated to SQL `OFFSET`/`FETCH`).
