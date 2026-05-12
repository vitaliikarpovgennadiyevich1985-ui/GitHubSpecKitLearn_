# Data Model: Catalog Product Image Field & 100-Product Endpoint

**Phase**: 1 — Design  
**Date**: 2026-05-05  
**Feature**: [spec.md](spec.md) | [research.md](research.md)

---

## Entity: Product

### Current Schema (before this feature)

| Field | Type | Constraints | Notes |
|---|---|---|---|
| `ProductId` | `string` | non-null | GUID string; was `Guid.NewGuid()` per request |
| `Title` | `string` | non-null | Display name |
| `Description` | `string` | non-null | Product description |
| `Price` | `decimal` | non-null, > 0 | Monetary value |

### Updated Schema (this feature)

| Field | Type | Constraints | Notes |
|---|---|---|---|
| `ProductId` | `string` | non-null | Deterministic GUID (stable across requests) — see research.md §3 |
| `Title` | `string` | non-null | Display name |
| `Description` | `string` | non-null | Product description |
| `Price` | `decimal` | non-null, > 0 | Monetary value |
| **`Image`** | `string` | **non-null, non-empty** | **NEW** — Base64-encoded 10×10 solid-color BMP; unique per product |

### Change Summary

- **Added**: `Image` (`string`) — Base64-encoded BMP binary image data.
- **Unchanged**: `ProductId`, `Title`, `Description`, `Price` — types and semantics unchanged.
- **Behavioral change**: `ProductId` transitions from `Guid.NewGuid()` (random per request) to a deterministic stable value derived from the product index.

---

## Supporting Data: In-Memory Catalog

### ProductCatalog (static data class — not a persisted entity)

Holds the predefined list of 100 `Product` instances at application startup.

| Aspect | Detail |
|---|---|
| Count | Exactly 100 products |
| ProductId | `MD5("product-{i}")` as GUID string, `i` ∈ `[0, 99]` |
| Title | `"Product {i+1}"` (e.g., `"Product 1"` … `"Product 100"`) |
| Description | `"Description for Product {i+1}"` |
| Price | `(i + 1) * 10m` (i.e., 10.00, 20.00 … 1000.00) |
| Image | Base64 BMP: 10×10 solid color, hue = `i × 3.6°` (HSV, S=1, V=1) |

### ProductImageGenerator (utility — not a domain entity)

Generates a Base64-encoded BMP image given an RGB color.

| Input | Type | Description |
|---|---|---|
| `r` | `byte` | Red channel (0–255) |
| `g` | `byte` | Green channel (0–255) |
| `b` | `byte` | Blue channel (0–255) |

| Output | Type | Description |
|---|---|---|
| Base64 string | `string` | BMP file bytes encoded as Base64; suitable for `data:image/bmp;base64,...` URI |

**Internal behavior** (see research.md §4 for byte layout):
1. Build 14-byte `BITMAPFILEHEADER` (type "BM", size 374, offset 54).
2. Build 40-byte `BITMAPINFOHEADER` (10×10, 24-bit, no compression).
3. Build 320-byte pixel data: 10 rows (bottom-up), each 32 bytes (30 bytes BGR × 10 pixels + 2 padding bytes).
4. Concatenate all three, return `Convert.ToBase64String(bytes)`.

---

## Entity Relationships

```
ProductCatalog (static)
  └── [0..99] Product
                ├── ProductId : string (stable GUID)
                ├── Title     : string
                ├── Description : string
                ├── Price     : decimal
                └── Image     : string (Base64 BMP)
```

No relationships to other entities or services. The catalog is self-contained.

---

## Validation Rules

| Field | Rule | Enforcement Point |
|---|---|---|
| `Image` | Non-null, non-empty | `ProductCatalog` — enforced at construction time (generated value is always valid) |
| `ProductId` | Non-null, stable | `ProductCatalog` — deterministic derivation guarantees non-null |
| `Title` | Non-null | `ProductCatalog` — string interpolation guarantees non-null |
| `Description` | Non-null | `ProductCatalog` — string interpolation guarantees non-null |
| `Price` | > 0 | `ProductCatalog` — formula `(i+1)*10m` guarantees 10.00–1000.00 |

---

## State Transitions

None — `Product` is an immutable data record in this feature. There are no state changes, status fields, or lifecycle events.
