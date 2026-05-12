# Phase 0 Research — Catalog Items Pagination

**Feature**: 003-catalog-items-pagination  
**Date**: 2026-05-06

This document resolves all open questions referenced as `NEEDS CLARIFICATION` in the plan and records best-practice decisions for areas where multiple approaches were viable.

## R-001 — Where pagination is applied

**Decision**: Apply pagination at the LINQ-to-objects layer over `ProductCatalog.All` using `OrderBy().ThenBy().Skip(skip).Take(pageSize)` and a separate `.Count` on the unfiltered source for `totalCount`.

**Rationale**:
- The catalog microservice does not yet have a database. The only data source is the static `IReadOnlyList<Product>` produced at startup.
- LINQ `Skip`/`Take` translates 1:1 to EF Core `OFFSET`/`FETCH` in SQL Server. When persistence is added later, the same call shape moves into the EF query without changing the public API.
- `IReadOnlyList<Product>.Count` is O(1), so `totalCount` adds no measurable overhead.

**Alternatives considered**:
- *Materialise full list, slice in handler.* Rejected — even though the underlying source is in-memory, doing the slice in the handler couples the API surface to a non-paged source and breaks FR-006 ("apply pagination at the persistence/query layer") in spirit.
- *Lazy `IEnumerable` pipeline returned to handler.* Rejected — the response must include `totalCount`, which forces enumeration of the source anyway; an explicit `Count` keeps intent clear.

## R-002 — Response envelope shape

**Decision**: Return `PagedResult<T>` with exactly four properties: `Items` (`IReadOnlyList<T>`), `TotalCount` (`int`), `PageNumber` (`int`), `PageSize` (`int`). Serialised via `System.Text.Json` defaults (camelCase via the existing ASP.NET Core configuration).

**Rationale**:
- Matches FR-007 verbatim. Adds no fields that the UI does not need (e.g., no `TotalPages` — the UI computes it as `(int)Math.Ceiling(totalCount / (double)pageSize)`; centralising it server-side risks divergence if the UI ever filters).
- Generic `PagedResult<T>` is reusable for future paged endpoints without copy-paste.

**Alternatives considered**:
- *Bare array + headers (`X-Total-Count`).* Rejected — adds parsing burden on the UI and breaks the explicit response shape required by FR-007.
- *Include `TotalPages` and `HasNext`/`HasPrevious` server-side.* Rejected — derived data; one source of truth (`totalCount` + `pageSize`) is preferred.

## R-003 — Parameter parsing & validation

**Decision**: Accept `pageNumber` and `pageSize` as `int?` query-string parameters in the Minimal API handler. Apply normalisation in a single `Normalize` helper inside `ProductCatalog.GetPage`:
- `pageNumber ?? 1`; if `< 1`, coerce to `1`.
- `pageSize ?? 20`; if `< 1`, coerce to `20`; if `> 100`, coerce to `100`.

**Rationale**:
- Maps directly to FR-002, FR-003, FR-004, FR-005.
- Returning HTTP 400 for invalid values is explicitly disallowed by the spec ("substitutes safe defaults … rather than returning an error").
- Centralising the rules in `GetPage` means the handler stays a one-liner and the rules are unit-testable as a pure function once tests are added.

**Alternatives considered**:
- *FluentValidation pipeline.* Rejected — would require a new package and a pipeline behaviour; the constitution recommends FluentValidation generally but the current project does not have it wired (pre-existing G10 violation), and adding it for two parameters is disproportionate.

## R-004 — UI fetch strategy

**Decision**: Use a server-rendered Razor view as the entry point (`/Catalog?pageNumber=N&pageSize=S`). Pagination, page-size change, and Retry are wired with a small `<script>` block that calls `fetch('/Catalog/Page?pageNumber=N&pageSize=S')`, which returns a partial Razor view (`_Cards.cshtml` + `_Pager.cshtml`) and replaces the grid container in-place.

**Rationale**:
- Stays within the constitution's "no third-party JS frameworks" rule (vanilla `fetch`, no React/Vue/HTMX).
- Server-side rendering keeps the markup template authoritative on the server and avoids duplicating card markup in JavaScript.
- A full page reload on every Next/Previous click would also satisfy the spec, but causes a layout flash and discards the page-size selector state; partial fetch is the simpler UX for equal complexity.

**Alternatives considered**:
- *Full page reloads on every navigation.* Rejected — fails to deliver the loading-state UX (the whole page flashes white) and complicates the Retry flow.
- *HTMX.* Rejected — adds a third-party JS framework, which the constitution and feature scope forbid.

## R-005 — Loading & error state implementation

**Decision**:
- A single `<div id="catalog-host">` wraps the cards + pager.
- During a fetch, the host's inner HTML is replaced by a centred spinner (Bootstrap `spinner-border`, already present via `bootstrap.min.css`); the page-size selector is set to `disabled` for the duration of the request.
- On success, the partial response replaces the host's inner HTML.
- On failure, the host's inner HTML is replaced by an error block containing a message and a `Retry` button bound to the same fetch with the most recent `pageNumber`/`pageSize`.

**Rationale**:
- Bootstrap's spinner ships with the existing layout; no new CSS or JS dependencies.
- Disabling the selector at fetch time covers FR-019 without needing a global state machine.

**Alternatives considered**:
- *Skeleton card placeholders.* Rejected — more visual polish than the feature requires and adds CSS surface.
- *Inline status text only.* Rejected — fails FR-019's "loading indicator" requirement.

## R-006 — Numbered page list windowing algorithm

**Decision**: Server-computed window using this rule:
- Always include page `1` and page `totalPages`.
- Always include `currentPage - 1`, `currentPage`, `currentPage + 1` when in range.
- Sort and de-duplicate; insert an ellipsis token wherever consecutive page numbers in the visible set differ by more than 1.

**Rationale**:
- Produces the canonical pattern (`1 … 4 5 6 … 99`) from the spec example with constant width regardless of total page count.
- Computing server-side keeps the JavaScript trivial (just swap the partial).

**Alternatives considered**:
- *Window of fixed N around current.* Rejected — does not show first/last anchors, which the spec example explicitly includes.
- *Client-side computation.* Rejected — duplicates logic and would need its own tests.

## R-007 — Concurrency / "rapid clicks" handling

**Decision**: A monotonically increasing `requestSeq` integer in the page script. Each fetch captures the current value; on response, only render if the captured value matches the latest. Stale responses are discarded.

**Rationale**:
- Satisfies the existing edge case ("If a user clicks 'Next' rapidly, only the most recent request's results are rendered.") with ~10 lines of vanilla JS.
- No need for `AbortController` plumbing.

**Alternatives considered**:
- *`AbortController` per click.* Equivalent correctness but more verbose; chosen alternative is simpler.

## R-008 — Compatibility with future EF Core migration

**Decision**: Sign `GetPage` as `(int? pageNumber, int? pageSize) → PagedResult<Product>`. Internally over `IEnumerable<Product>` today; over `IQueryable<Product>` tomorrow when EF arrives. The translation `OrderBy(p => p.Title).ThenBy(p => p.ProductId).Skip(skip).Take(take)` is identical for both providers.

**Rationale**: Zero contract change at the swap point; only the implementation body changes.

**Alternatives considered**: None — this is a simple compatibility note, not a competing approach.
