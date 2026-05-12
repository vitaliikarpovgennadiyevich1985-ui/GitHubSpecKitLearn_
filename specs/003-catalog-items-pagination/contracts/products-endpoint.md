# Contract: `/products` (paged) — v2

**Feature**: 003-catalog-items-pagination  
**Service**: Asp.Net.Core.Learning.CatalogMicroservice  
**Status**: Replaces v1 in-place. v1 is not retained because the only consumer (Asp.Net.Core.Learning.UI) is updated in lockstep within this same feature. See ADR in [plan.md](../plan.md#complexity-tracking) (gate G9 row).

## Endpoint

```
GET /products?pageNumber={int}&pageSize={int}
```

**Authentication**: Unchanged. Requires the `Catalog-Microservice-Read-Api` policy (existing).

## Query parameters

| Name | Type | Required | Default | Constraints | Behavior on violation |
|---|---|---|---|---|---|
| `pageNumber` | `int` | No | `1` | `>= 1` | `< 1` or non-integer → coerced to `1` (no HTTP 400) |
| `pageSize` | `int` | No | `20` | `1..100` | `< 1` → coerced to `20`; `> 100` → coerced to `100`; non-integer → coerced to `20` |

Behavior is defined by FR-002, FR-003, FR-004, FR-005, and FR-008 of the spec.

## Response

**Status**: `200 OK` for all valid (post-coercion) inputs, including `pageNumber` greater than the number of available pages (FR-008). The response body in that case has `items: []` with `totalCount`, `pageNumber`, and `pageSize` populated.

**Content-Type**: `application/json` (camelCase, ASP.NET Core defaults).

**Body schema**:

```json
{
  "items": [
    {
      "productId": "string (GUID)",
      "title": "string",
      "description": "string",
      "price": 0,
      "image": "string (base64 BMP)"
    }
  ],
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 20
}
```

| Field | Type | Notes |
|---|---|---|
| `items` | `Product[]` | The current page in deterministic order: ascending `title`, then ascending `productId` (FR-016). May be empty. |
| `totalCount` | `int` | Total items across all pages (`>= 0`). |
| `pageNumber` | `int` | Effective page number after coercion (`>= 1`). |
| `pageSize` | `int` | Effective page size after coercion (`1..100`). |

The `Product` element shape is unchanged from v1.

## Examples

### Default request

```
GET /products
```

Response (`200 OK`):

```json
{
  "items": [ /* 20 products */ ],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 20
}
```

### Explicit page

```
GET /products?pageNumber=3&pageSize=10
```

Response (`200 OK`):

```json
{
  "items": [ /* products 21..30 in deterministic order */ ],
  "totalCount": 100,
  "pageNumber": 3,
  "pageSize": 10
}
```

### Page beyond range

```
GET /products?pageNumber=999&pageSize=20
```

Response (`200 OK`):

```json
{
  "items": [],
  "totalCount": 100,
  "pageNumber": 999,
  "pageSize": 20
}
```

### Invalid input → coerced

```
GET /products?pageNumber=-5&pageSize=0
```

Response (`200 OK`):

```json
{
  "items": [ /* 20 products */ ],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 20
}
```

```
GET /products?pageSize=10000
```

Response (`200 OK`):

```json
{
  "items": [ /* 100 products — clamped to MaxPageSize */ ],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 100
}
```

## Performance contract

- Response time and memory MUST NOT scale with `totalCount` (SC-004, SC-005). Implementation MUST use `Skip`/`Take` (or equivalent) at the data source, not in-memory full-list slicing.
- For the current 100-item dataset, p95 < 50 ms.

## Versioning notes

- This change replaces the v1 array response in place. It is a **breaking** change for any third-party consumer.
- Within this solution, the only consumer is the UI front-end, which is updated in the same feature commit.
- If a second consumer is introduced in the future, the next change to this contract MUST go through a versioned path (e.g., `/v2/products`) or media-type negotiation.
