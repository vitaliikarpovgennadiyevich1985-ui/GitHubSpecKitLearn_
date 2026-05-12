# Tasks: WebUI Visual Product Cards

**Input**: Design documents from `specs/002-webui-visual-product-cards/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**User Arguments**: No third-party JS frameworks. Raw JS only if required. (Research confirmed: zero JS needed for this feature.)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the existing project wires up cleanly for this feature — no new projects or packages required.

- [x] T001 Verify `Asp.Net.Core.Learning.CatalogMicroservice` `/products` endpoint returns `image` field by running the Aspire AppHost and inspecting the JSON response

---

## Phase 2: Foundational (Blocking Prerequisite)

**Purpose**: Extend the UI `Product` model with the `Image` property so that `GetFromJsonAsync` deserialises the field automatically. This one-line change unblocks both user story phases.

**⚠️ CRITICAL**: Both user story phases depend on this model change.

- [x] T002 Add `public string Image { get; set; } = string.Empty;` to `Asp.Net.Core.Learning.UI/Models/Product.cs`

**Checkpoint**: `Product.Image` is populated from the microservice response — User Story phases can now begin.

---

## Phase 3: User Story 1 — View Product Catalog with Images (Priority: P1) 🎯 MVP

**Goal**: Replace the plain text product list with a multi-column CSS Grid of product cards, each showing the product image (128×128 px inline data URI) and metadata (Title, Description, Price) in a vertical layout.

**Independent Test**: Start the Aspire AppHost, log in, navigate to `/Catalog` — every card displays a coloured square image and three lines of metadata; cards are visually separated.

### Implementation for User Story 1

- [x] T003 [US1] Rewrite `Asp.Net.Core.Learning.UI/Views/Catalog/Index.cshtml`: replace the `@foreach` plain `<div>` loop with a `<div class="product-grid">` CSS Grid container wrapping `<div class="product-card">` elements
- [x] T004 [US1] Add scoped `<style>` block to `Index.cshtml` with `.product-grid` rule: `display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 1rem; padding: 1rem;`
- [x] T005 [US1] Add `.product-card` CSS rule to `Index.cshtml` scoped style: `border: 1px solid #dee2e6; border-radius: 4px; padding: 0.75rem; display: flex; flex-direction: column; gap: 0.5rem;`
- [x] T006 [US1] Inside each card in `Index.cshtml`, render `<img src="data:image/bmp;base64,@product.Image" alt="" width="128" height="128" />` inside an `@if (!string.IsNullOrEmpty(product.Image))` Razor guard
- [x] T007 [US1] Inside each card in `Index.cshtml`, add `<div class="product-meta">` containing three `<span>` elements: `@product.Title` (bold), `@product.Description`, `@product.Price.ToString("C")` — each on its own line
- [x] T008 [US1] Add `.product-meta` CSS rule: `display: flex; flex-direction: column; gap: 0.25rem;`

**Checkpoint**: User Story 1 is fully functional. Navigate to `/Catalog` — multi-column grid of cards with images and metadata is visible.

---

## Phase 4: User Story 2 — Consistent Card Layout Across All Products (Priority: P2)

**Goal**: Ensure all 100 cards are visually uniform regardless of content length differences — no card bleeds into another.

**Independent Test**: Load all 100 products; verify uniform card width, consistent spacing, and no layout breakage for products with varying description lengths.

### Implementation for User Story 2

- [x] T009 [US2] Add `align-items: flex-start` to `.product-card` CSS rule in `Index.cshtml` to prevent card stretch across grid rows with variable-height siblings
- [x] T010 [US2] Add `width: 128px; height: 128px; background-color: #cccccc;` CSS rule for `.product-img-placeholder` class in `Index.cshtml` scoped style
- [x] T011 [US2] Add `else` branch to the Razor image guard in `Index.cshtml`: render `<div class="product-img-placeholder"></div>` when `product.Image` is null or empty (grey placeholder, 128×128 px per FR-007)

**Checkpoint**: All 100 cards render with consistent layout; grey placeholder renders for any product with missing image data.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final validation against all acceptance criteria before feature sign-off.

- [x] T012 [P] Verify in browser DevTools that no extra network requests are made beyond the single `/products` call (FR-006, SC-004)
- [x] T013 [P] Inspect any `<img>` element in browser DevTools — confirm `width` and `height` attributes are both `128` (SC-002)
- [x] T014 [P] Confirm all `<img>` elements have `alt=""` (empty, not missing) per FR-009 / WCAG 2.1 H67
- [x] T015 Run the full acceptance verification table from `specs/002-webui-visual-product-cards/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 verification — **BLOCKS both user story phases**
- **User Story 1 (Phase 3)**: Depends on Foundational (T002) — can start as soon as `Product.Image` exists
- **User Story 2 (Phase 4)**: Depends on Phase 3 being complete — builds on top of the rendered cards
- **Polish (Phase 5)**: Depends on both user story phases being complete

### Within User Story 1

- T003 (grid container) before T006/T007 (card contents)
- T004, T005, T008 (CSS rules) can be written together with their corresponding HTML — no strict ordering among CSS tasks
- T006 (image) and T007 (metadata) are independent of each other within the card

### Within User Story 2

- T009, T010 (CSS additions) are independent of each other
- T011 (placeholder `else` branch) depends on T010 (`.product-img-placeholder` class existing)

### Parallel Opportunities

- T004, T005, T008 (CSS rules in Phase 3) can be authored in parallel
- T006 and T007 (image and metadata regions in Phase 3) can be authored in parallel
- T009 and T010 (Phase 4 CSS additions) can be authored in parallel
- T012, T013, T014 (Phase 5 verifications) can be done in parallel

---

## Parallel Example: User Story 1

```
# These tasks touch the same file but different regions — author in sequence or split by section:
T004 — add .product-grid CSS rule
T005 — add .product-card CSS rule
T008 — add .product-meta CSS rule

# These are independent card sub-elements:
T006 — image <img> with Razor guard
T007 — metadata <div> with three <span> lines
```

---

## Implementation Strategy

### MVP (User Story 1 Only — 6 tasks after T001–T002)

1. Complete Phase 1: T001 — verify microservice response
2. Complete Phase 2: T002 — add `Image` property to `Product.cs`
3. Complete Phase 3: T003 → T008 — rewrite `Index.cshtml`
4. **STOP and VALIDATE**: Navigate to `/Catalog` — images and cards must be visible
5. Continue to Phase 4 (US2) only after US1 is confirmed

### Full Delivery (both user stories — 15 tasks total)

Complete all phases in order: Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5.

### Key Constraint

No JavaScript required anywhere. All layout, image display, and degradation are achieved via Razor conditionals and CSS only, satisfying the user argument constraint.
