# Research: WebUI Visual Product Cards

**Feature**: `002-webui-visual-product-cards`
**Date**: 2026-05-05
**Status**: Complete — no NEEDS CLARIFICATION items remain after spec clarification session.

---

## Topic 1: BMP Data URI support in modern browsers

**Decision**: Use `data:image/bmp;base64,{base64string}` inline data URIs directly in `<img src="...">` attributes.

**Rationale**: The Catalog microservice generates images as 10×10 solid-colour BMPs encoded as base64 strings. HTML `<img>` elements support data URIs natively in all evergreen browsers (Chrome, Firefox, Edge, Safari) without any server-side conversion or additional endpoints. No C# code needs to decode or re-encode the image data — the raw `Image` string from the JSON response is inserted directly as the `src` value.

**Alternatives considered**:
- Serving images from a separate image endpoint — rejected (FR-006 requires no new endpoints; increases request count).
- Converting BMP to PNG/JPEG server-side — rejected (adds unnecessary CPU work; base64 BMP works in all target browsers; no quality benefit for a 10×10 swatch).
- Using `<canvas>` to render decoded pixel data — rejected (overly complex; no third-party JS frameworks allowed; plain data URI is simpler and sufficient).

**Evidence**: BMP MIME type `image/bmp` is in the IANA registry and supported by all W3C-compliant browsers. The `data:` URI scheme is specified in RFC 2397 and has universal browser support.

---

## Topic 2: CSS Grid for multi-column card layout without any framework

**Decision**: Use native CSS Grid (`display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 1rem;`) scoped via a `<style>` block inside the Razor view.

**Rationale**: CSS Grid with `auto-fill` and `minmax` provides a responsive multi-column layout that adjusts column count to the container width — no JS, no framework, no build tooling. `gap` provides uniform spacing between cards. Cards use `display: flex; flex-direction: column` internally to stack image and metadata vertically. The grey placeholder for missing images is achieved with a CSS fallback rule (empty `src` hides the `<img>`; a sibling `<div>` with `background-color: #ccc` is conditionally rendered).

**Alternatives considered**:
- Bootstrap grid classes — rejected (no third-party JS frameworks; Bootstrap requires JS; not installed).
- Flexbox wrap — rejected (CSS Grid `auto-fill` is more semantically correct for a product grid and handles variable column counts more cleanly).
- Separate `site.css` addition — rejected (scoping styles inside the view avoids polluting the global stylesheet and keeps the feature self-contained).

---

## Topic 3: Graceful image degradation (grey placeholder)

**Decision**: Conditionally render a `<div class="product-img-placeholder">` when `product.Image` is null or empty; render `<img>` only when image data is present.

**Rationale**: An `<img>` with an empty or invalid `src` attribute triggers a browser broken-image icon, which fails FR-007. A Razor `@if` check on `product.Image` cleanly separates the two cases server-side. The placeholder `<div>` is styled to 128×128 px with `background-color: #cccccc`.

**Alternatives considered**:
- `onerror` JavaScript fallback on `<img>` — rejected (requires raw JS; the Razor conditional is simpler and avoids any JS at all; broken-image icon still flashes before `onerror` fires for truly empty `src`).
- CSS `background-image` with fallback — rejected (more complex; CSS `background-image` on an element does not degrade as cleanly as a conditional element).

---

## Topic 4: No JavaScript required

**Decision**: This feature requires zero JavaScript.

**Rationale**: The card grid, image display, and grey placeholder are all achievable with Razor conditionals and CSS only. The user instruction to avoid third-party JS frameworks and use raw JS only when required is fully satisfied by the absence of any JS at all.

---

## Resolved Clarifications (from spec clarification session)

| Decision | Value |
|---|---|
| Image display size | 128×128 px |
| Pagination | None — single page, all 100 products |
| Image alt attribute | Empty (`alt=""`) — decorative per WCAG 2.1 H67 |
| Card layout direction | Multi-column grid |
| Missing image fallback | Neutral grey placeholder box, 128×128 px |
