# Data Model: WebUI Visual Product Cards

**Feature**: `002-webui-visual-product-cards`
**Date**: 2026-05-05

---

## Entity: Product (UI Model)

**File**: `Asp.Net.Core.Learning.UI/Models/Product.cs`
**Purpose**: Represents a single catalog item received from the Catalog microservice. Used as the view model for the catalog page.

| Field | Type | Source | Notes |
|---|---|---|---|
| `ProductId` | `string` | Microservice response | Stable GUID-derived identifier; not displayed on the card |
| `Title` | `string` | Microservice response | Displayed on each card — first metadata line |
| `Description` | `string` | Microservice response | Displayed on each card — second metadata line |
| `Price` | `decimal` | Microservice response | Displayed on each card — third metadata line, formatted as currency |
| `Image` *(new)* | `string` | Microservice response | Base64-encoded BMP image data. Rendered as inline `data:image/bmp;base64,…` data URI in an `<img>` element at 128×128 px. Empty string when no image available. |

### Validation Rules

- `Image` may be empty string — the view degrades gracefully to a grey placeholder box (128×128 px).
- No server-side validation required — the field is display-only; it is never written back to any service.

### State Transitions

None — `Product` is a read-only view model. No mutations occur within the UI project.

---

## View Concept: Product Card

**File**: `Asp.Net.Core.Learning.UI/Views/Catalog/Index.cshtml`
**Purpose**: Visual unit representing a single product in the catalog grid.

### Card Anatomy

```
┌──────────────────────────────┐
│  [128×128 image or grey box] │
│                              │
│  Title                       │
│  Description                 │
│  Price                       │
└──────────────────────────────┘
```

### Layout

- Container: CSS Grid, `auto-fill` columns with `minmax(220px, 1fr)` — adapts to page width.
- Card: fixed-width column item with border, padding, and gap separating adjacent cards.
- Image region: 128×128 px. When `Image` is non-empty, renders `<img src="data:image/bmp;base64,{Image}" alt="" width="128" height="128">`. When `Image` is empty, renders `<div>` of 128×128 px with grey background (`#cccccc`).
- Metadata region: `display: flex; flex-direction: column` — each field on its own line.

### Serialisation / Deserialisation

`System.Text.Json` (used by `GetFromJsonAsync`) automatically maps the `Image` JSON field to the `Image` C# property by convention (case-insensitive match). No custom `JsonConverter` or `JsonPropertyName` attribute is required.

---

## Change Summary

| File | Change Type | Description |
|---|---|---|
| `Asp.Net.Core.Learning.UI/Models/Product.cs` | Modify | Add `public string Image { get; set; } = string.Empty;` |
| `Asp.Net.Core.Learning.UI/Views/Catalog/Index.cshtml` | Rewrite | CSS Grid card layout with inline image rendering |
| `Asp.Net.Core.Learning.UI/Infrastructure/CatalogService.cs` | No change | `GetFromJsonAsync` auto-deserialises new field |
| `Asp.Net.Core.Learning.UI/Contracts/ICatalogService.cs` | No change | Interface signature unchanged |
| `Asp.Net.Core.Learning.UI/Controllers/CatalogController.cs` | No change | Controller unchanged |
