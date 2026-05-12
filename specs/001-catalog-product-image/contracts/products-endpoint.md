# API Contract: GET /products

**Version**: 1.0  
**Service**: Catalog Microservice  
**Date**: 2026-05-05  
**Feature**: [spec.md](../spec.md)

---

## Endpoint

```
GET /products
```

## Authentication & Authorization

| Requirement | Detail |
|---|---|
| Authentication scheme | Bearer JWT (`at+jwt` type validation enforced) |
| Authorization policy | `Catalog-Microservice-Read-Api` |
| Required claim | `scope` = `"Catalog-Microservice-Read-Api"` |
| Additional requirement | `CanReturnProductsRequirement` (existing custom handler) |

Requests without a valid token → **401 Unauthorized**  
Requests with a valid token but missing scope → **403 Forbidden**

---

## Request

No query parameters, path parameters, or request body.

### Headers

| Header | Required | Value |
|---|---|---|
| `Authorization` | Yes | `Bearer <access_token>` |

---

## Response

### 200 OK

Returns a JSON array of exactly 100 `Product` objects.

**Content-Type**: `application/json`

#### Product Object Schema

| Field | Type | Nullable | Description |
|---|---|---|---|
| `productId` | `string` | No | Stable deterministic GUID string (e.g., `"a1b2c3d4-..."`) |
| `title` | `string` | No | Display name (e.g., `"Product 1"`) |
| `description` | `string` | No | Product description (e.g., `"Description for Product 1"`) |
| `price` | `number` (decimal) | No | Price in monetary units (10.00–1000.00) |
| `image` | `string` | No | Base64-encoded BMP image data (10×10 solid-color, unique per product) |

> **Note on `image` field**: The value is raw Base64 (no data URI prefix). To use in an HTML `<img>` tag: `src="data:image/bmp;base64,{image}"`.

#### Example Response (abbreviated)

```json
[
  {
    "productId": "a3f4e5d6-1234-5678-abcd-ef0123456789",
    "title": "Product 1",
    "description": "Description for Product 1",
    "price": 10.00,
    "image": "Qk12AQAAAAAAAD..."
  },
  {
    "productId": "b4c5d6e7-2345-6789-bcde-f01234567890",
    "title": "Product 2",
    "description": "Description for Product 2",
    "price": 20.00,
    "image": "Qk12AQAAAAAAAD..."
  },
  ...
  {
    "productId": "...",
    "title": "Product 100",
    "description": "Description for Product 100",
    "price": 1000.00,
    "image": "Qk12AQAAAAAAAD..."
  }
]
```

### Non-2xx Responses

| Status | Condition |
|---|---|
| 401 Unauthorized | Missing or invalid Bearer token |
| 403 Forbidden | Valid token but `scope` claim does not include `Catalog-Microservice-Read-Api`, or `CanReturnProductsRequirement` not satisfied |

---

## Contract Stability

| Aspect | Guarantee |
|---|---|
| Array length | Always exactly 100 items |
| Field set | `productId`, `title`, `description`, `price`, `image` — all always present and non-null |
| `productId` stability | Same value returned on every call for the same product index |
| `image` format | Base64-encoded BMP; always non-empty |
| Backward compatibility | All fields present in prior contract (`productId`, `title`, `description`, `price`) remain unchanged in type and semantics |

---

## Breaking Change Analysis

| Change | Breaking? | Notes |
|---|---|---|
| Add `image` field to response | **No** — additive change | Existing consumers that do not read `image` are unaffected |
| `productId` now stable (was random) | **Potentially** — if any consumer stored and compared IDs across calls, the values will change once. | Considered acceptable: the prior IDs were never stable so no persistent references could exist |
