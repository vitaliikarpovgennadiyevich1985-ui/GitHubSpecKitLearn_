# Quickstart — Catalog Items Pagination

**Feature**: 003-catalog-items-pagination  
**Audience**: Developer implementing the feature; reviewer validating it manually.

## Prerequisites

- .NET 10 SDK installed.
- Solution restored: `dotnet restore` from repo root.
- Aspire AppHost can launch the IdentityServer + Catalog microservice + UI together.

## 1. Run the solution

From the repository root:

```powershell
dotnet run --project Asp.Net.Core.Learning.AspireServer/Asp.Net.Core.Learning.AspireServer.AppHost
```

Wait for the Aspire dashboard to report all four services healthy (IdentityServer, Catalog, ShoppingBasket, UI).

## 2. Verify the catalog endpoint

Acquire a token through the UI login (browser → click any authenticated link → log in). Then from the UI's `/Account/Tokens` page copy the access token and exercise the endpoint directly:

```powershell
$token = "<paste access token>"
curl -k "https://catalogmicroservice/products" -H "Authorization: Bearer $token"
```

Expected: `200 OK`, body has `items` (20), `totalCount` (100), `pageNumber` (1), `pageSize` (20).

```powershell
curl -k "https://catalogmicroservice/products?pageNumber=2&pageSize=10" -H "Authorization: Bearer $token"
```

Expected: `200 OK`, `items.length == 10`, `pageNumber == 2`, `pageSize == 10`.

```powershell
curl -k "https://catalogmicroservice/products?pageNumber=999&pageSize=20" -H "Authorization: Bearer $token"
```

Expected: `200 OK`, `items.length == 0`, `pageNumber == 999`, `totalCount == 100`.

## 3. Verify the UI page (manual acceptance)

Navigate to the UI: open the URL shown in the Aspire dashboard for `Asp.Net.Core.Learning.UI`, log in, click **Products**.

Walk through these checks (mapped to spec acceptance scenarios):

| # | Check | Maps to |
|---|---|---|
| 1 | First load shows exactly 20 cards | US1 / SC-001 |
| 2 | Page indicator reads "Page 1 of 5" | US1 / SC-003 |
| 3 | Click "Next" → grid is replaced by spinner briefly, then page 2 (next 20 products) appears; indicator reads "Page 2 of 5"; URL is unchanged | US1 / FR-013 / FR-019 |
| 4 | Click "Previous" → returns to page 1 | US1 |
| 5 | On page 1, "Previous" is disabled | US2 / FR-012 |
| 6 | Navigate to page 5; "Next" is disabled | US2 / FR-012 |
| 7 | Change page-size selector from 20 → 50 → grid reloads, shows 50 cards, indicator reads "Page 1 of 2" | FR-018 |
| 8 | Change page-size to 100 → 1 page total; both Previous and Next disabled | FR-012 / FR-018 |
| 9 | Numbered list shows `1 2 3 4 5` for 5 pages with `pageSize=20`; current page is highlighted and not clickable | FR-020 |
| 10 | Click any numbered link → triggers a single new request and renders that page | FR-020 / SC-002 |
| 11 | With `pageSize=10` (10 pages), windowed list shows e.g. `1 … 4 5 6 … 10` when on page 5 | FR-021 |
| 12 | Stop the Catalog microservice in the Aspire dashboard, then click Next → grid is cleared, full-page error with Retry button is shown; restart the service and click Retry → page renders normally | FR-017 |

## 4. Verify deterministic ordering

```powershell
$a = curl -k "https://catalogmicroservice/products?pageNumber=1&pageSize=10" -H "Authorization: Bearer $token" | ConvertFrom-Json
$b = curl -k "https://catalogmicroservice/products?pageNumber=2&pageSize=10" -H "Authorization: Bearer $token" | ConvertFrom-Json
($a.items + $b.items).productId | Group-Object | Where-Object Count -gt 1
```

Expected: no output (no duplicate products across consecutive pages — confirms FR-016 sort stability).

## 5. Verify performance characteristic (smoke)

This is a smoke test only; full SC-004/SC-005 verification will be possible once a database is wired in. With the current in-memory store:

```powershell
1..50 | ForEach-Object {
  Measure-Command { curl -k "https://catalogmicroservice/products?pageNumber=$_" -H "Authorization: Bearer $token" } | Select-Object -ExpandProperty TotalMilliseconds
} | Measure-Object -Average -Maximum
```

Expected: average < 50 ms; no monotonic growth across page numbers (slicing is O(pageSize), not O(totalCount × pageNumber)).
