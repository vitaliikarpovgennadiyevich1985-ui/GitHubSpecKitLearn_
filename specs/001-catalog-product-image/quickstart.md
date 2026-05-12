# Quickstart: Catalog Product Image Field & 100-Product Endpoint

**Date**: 2026-05-05  
**Feature**: [spec.md](spec.md) | [contracts/products-endpoint.md](contracts/products-endpoint.md)

---

## Prerequisites

- .NET 10 SDK installed
- Identity Server running (provides JWT tokens) — typically `Asp.Net.Core.Learning.IdentityServer` at the URL configured in `IDENTITYSERVER_HTTPS`
- Valid access token with `scope: Catalog-Microservice-Read-Api`

---

## Running the Catalog Microservice

```powershell
cd Asp.Net.Core.Learning.CatalogMicroservice
dotnet run
# Listens on https://localhost:7087 and http://localhost:5125
```

---

## Obtaining an Access Token

Obtain a Bearer token from the Identity Server with the required scope. Replace the values below with your actual Identity Server URL, client credentials, and scope:

```powershell
$tokenResponse = Invoke-RestMethod `
  -Uri "https://localhost:<identity-server-port>/connect/token" `
  -Method Post `
  -Body @{
      grant_type    = "client_credentials"
      client_id     = "<your-client-id>"
      client_secret = "<your-client-secret>"
      scope         = "Catalog-Microservice-Read-Api"
  }

$token = $tokenResponse.access_token
```

---

## Calling the /products Endpoint

```powershell
$response = Invoke-RestMethod `
  -Uri "https://localhost:7087/products" `
  -Method Get `
  -Headers @{ Authorization = "Bearer $token" }

$response.Count          # Should be 100
$response[0]             # Inspect first product
$response[0].image       # Non-empty Base64 BMP string
```

---

## Manual Verification Checklist

After calling the endpoint, verify the following against [spec.md](spec.md) success criteria:

| Check | Command | Expected |
|---|---|---|
| SC-001: Exactly 100 products | `$response.Count` | `100` |
| SC-002: All have non-empty Image | `$response \| Where-Object { [string]::IsNullOrEmpty($_.image) }` | Empty result (0 items) |
| SC-003: All fields populated | `$response \| Where-Object { [string]::IsNullOrEmpty($_.productId) -or [string]::IsNullOrEmpty($_.title) }` | Empty result |
| SC-004: Backward-compatible fields exist | `$response[0].productId, $response[0].title, $response[0].description, $response[0].price` | All non-null |
| SC-005: Stable IDs across calls | Call twice; compare `$r1[0].productId -eq $r2[0].productId` | `True` |

---

## Rendering an Image (Optional Sanity Check)

To verify the `image` field contains a valid BMP, save it to a file and open it:

```powershell
$imageBytes = [Convert]::FromBase64String($response[0].image)
[System.IO.File]::WriteAllBytes("C:\Temp\product-0.bmp", $imageBytes)
Start-Process "C:\Temp\product-0.bmp"   # Opens in default image viewer
```

Expected: a small 10×10 solid-color image opens without errors.

---

## Expected Errors

| Scenario | Expected Response |
|---|---|
| No Authorization header | `401 Unauthorized` |
| Token without required scope | `403 Forbidden` |
| Expired token | `401 Unauthorized` |
