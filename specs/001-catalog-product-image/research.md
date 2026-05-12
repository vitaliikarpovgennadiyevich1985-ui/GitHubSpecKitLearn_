# Research: Catalog Product Image Field & 100-Product Endpoint

**Phase**: 0 — Pre-design research  
**Date**: 2026-05-05  
**Feature**: [spec.md](spec.md) | [plan.md](plan.md)

---

## Research Area 1: In-Memory Binary Image Generation in .NET 10 (No External Packages)

### Decision
**Generate 10×10 pixel solid-color BMP images using pure BCL byte-array construction.**

### Rationale
- BMP (Device-Independent Bitmap) is the simplest binary image format — it requires no compression and has a deterministic, fixed-size byte layout.
- A 10×10 24-bit BMP is exactly **374 bytes** (14-byte file header + 40-byte info header + 320-byte pixel data). Base64-encoded: ~500 characters — compact enough for a JSON payload.
- All required operations (`BitConverter.GetBytes`, `Array.Copy`, `MemoryStream`, `Convert.ToBase64String`) are available in `System.Runtime` / `mscorlib` — no NuGet package needed.
- BMP is universally readable by browsers (via `data:image/bmp;base64,...` URIs), image viewers, and image processing libraries, making the data immediately useful for UI development.

### Alternatives Considered

| Alternative | Why Rejected |
|---|---|
| PNG via `System.Drawing.Common` | Requires `System.Drawing.Common` NuGet package; Windows-only without workaround; overkill for a static learning dataset |
| PNG via `SkiaSharp` | Heavy dependency (~15 MB); cross-platform but adds significant overhead to a learning demo service |
| PNG via manual byte construction | PNG requires zlib/DEFLATE compression (Adler-32 + CRC-32 per chunk); adds 50+ lines of low-level byte math vs. ~30 for BMP |
| Absolute URL to placeholder service | Requires network access to resolve; not binary image data as specified in the clarification session |
| `ImageSharp` (SixLabors) | Additional NuGet dependency; LGPL-licensed; unnecessary for a hardcoded dataset |

---

## Research Area 2: Generating 100 Perceptually Distinct Colors (HSV Color Space)

### Decision
**Distribute 100 hues evenly around the HSV color wheel at full saturation (S=1.0) and full value (V=1.0), then convert to RGB.**

Formula: `hue[i] = i × 3.6°` for `i` in `[0, 99]`

### Rationale
- Evenly-spaced hues in HSV guarantee maximum perceptual distinction between adjacent colors.
- S=1.0 and V=1.0 produce fully saturated, bright colors — visually distinct even at 10×10 size.
- HSV-to-RGB conversion is pure arithmetic (no dependencies).

### HSV → RGB Algorithm (standard)
```
H ∈ [0, 360), S = 1.0, V = 1.0

C = V × S = 1.0
X = C × (1 - |((H/60) mod 2) - 1|)
m = V - C = 0

(R', G', B') = sector-mapped (C, X, 0) → 6 sectors of 60° each
R = (R' + m) × 255
G = (G' + m) × 255
B = (B' + m) × 255
```

### Alternatives Considered

| Alternative | Why Rejected |
|---|---|
| Sequential RGB increments (e.g., R+=2 per product) | Colors wrap and repeat within 128 steps; poor perceptual distinction |
| Fixed palette from a CSS color list | Limited to ~140 named colors, fewer than 100 distinct values |
| Random colors | Non-deterministic — violates the stable/predefined requirement (FR-007 stable IDs, same principle applies to images) |

---

## Research Area 3: Stable ProductId Strategy for 100 In-Memory Products

### Decision
**Pre-generate 100 deterministic GUID strings using a seeded, index-based computation.**

Approach: `Guid` from a fixed SHA-256 hash of the product index:
```csharp
// Simple, index-stable approach:
var id = new Guid(MD5.HashData(Encoding.UTF8.GetBytes($"product-{i}")));
```
This produces the same GUID for the same index on every application start, satisfying FR-007 with no runtime state.

### Alternatives Considered

| Alternative | Why Rejected |
|---|---|
| Hard-coded 100 GUIDs in source code | Works but produces extremely noisy source files; fragile to manual typos |
| Sequential integer IDs (1–100) | Technically simpler but diverges from existing `ProductId` type (string GUID) and changes the contract shape |
| `Guid.NewGuid()` at startup (stored once) | Non-deterministic across restarts — violates FR-007 stable identity |

---

## Research Area 4: BMP File Format — Concrete Byte Layout

### 10×10 24-bit RGB BMP structure (374 bytes total)

```
Offset  Size  Field                Value
------  ----  -----                -----
BITMAPFILEHEADER (14 bytes)
0       2     bfType               0x42 0x4D ("BM")
2       4     bfSize               374 (little-endian)
6       2     bfReserved1          0
8       2     bfReserved2          0
10      4     bfOffBits            54 (header size = 14 + 40)

BITMAPINFOHEADER (40 bytes)
14      4     biSize               40
18      4     biWidth              10
22      4     biHeight             10 (positive = bottom-up)
26      2     biPlanes             1
28      2     biBitCount           24
30      4     biCompression        0 (BI_RGB, no compression)
34      4     biSizeImage          0 (allowed for BI_RGB)
38      4     biXPelsPerMeter      2835 (72 DPI)
42      4     biYPelsPerMeter      2835 (72 DPI)
46      4     biClrUsed            0
50      4     biClrImportant       0

PIXEL DATA (320 bytes)
54      320   Rows (bottom-up), each row = 10 × 3 bytes (B, G, R order) + 2 bytes padding
              Row stride = ceil(10 × 3 / 4) × 4 = 32 bytes
              Total = 32 × 10 = 320 bytes
```

**Key BMP nuances**:
- Pixel order within a row is **BGR** (blue, green, red), not RGB.
- Rows are stored **bottom-to-top** (last row in the image is first in the file).
- Row width must be padded to a **4-byte boundary** (10 × 3 = 30 → pad to 32, i.e., 2 padding bytes).
- All multi-byte integers use **little-endian** byte order.

---

## All Unknowns Resolved

| Original Unknown | Resolution |
|---|---|
| Image field canonical name | `Image` (clarified in spec session) |
| Image format/content | Base64 BMP binary data — 10×10 solid-color pixel image |
| Color uniqueness strategy | HSV hue-distributed: `hue[i] = i × 3.6°` |
| Stable ProductId | Deterministic GUID from MD5 hash of `"product-{i}"` |
| No external packages | Pure BCL byte construction — confirmed feasible |
