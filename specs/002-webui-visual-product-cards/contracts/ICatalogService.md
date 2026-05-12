# Contract: ICatalogService

**Interface**: `Asp.Net.Core.Learning.UI/Contracts/ICatalogService.cs`
**Consumer**: `Asp.Net.Core.Learning.UI/Controllers/CatalogController.cs`
**Provider**: `Asp.Net.Core.Learning.UI/Infrastructure/CatalogService.cs` → `https://CatalogMicroservice/products`

---

## Interface Definition

```csharp
public interface ICatalogService
{
    Task<IEnumerable<Product>> GetProducts();
}
```

**Change for this feature**: None. The interface signature is unchanged.

---

## Response Shape: `GET /products`

The Catalog microservice returns a JSON array. The UI `Product` model must match the field names (case-insensitive, via `System.Text.Json` defaults).

```json
[
  {
    "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Product 1",
    "description": "Description for Product 1",
    "price": 10.00,
    "image": "<base64-encoded BMP string>"
  }
]
```

### Field Mapping

| JSON field | C# property | Change for this feature |
|---|---|---|
| `productId` | `Product.ProductId` | No change |
| `title` | `Product.Title` | No change |
| `description` | `Product.Description` | No change |
| `price` | `Product.Price` | No change |
| `image` | `Product.Image` | **New** — add property to `Product.cs` |

### Deserialisation

`HttpClient.GetFromJsonAsync<IEnumerable<Product>>("/products")` uses `System.Text.Json` with default options (case-insensitive property name matching). Adding `Image` to the `Product` model is sufficient — no configuration change required.

---

## Pre-conditions

- Caller must present a valid OAuth access token with the `Catalog-Microservice-Read-Api` scope. The `AddUserAccessTokenHandler()` registered in `Program.cs` handles this automatically.
- The Catalog microservice must be running and reachable at the Aspire service name `CatalogMicroservice`.

## Post-conditions

- Returns `IEnumerable<Product>` with all 100 products.
- Each `Product.Image` is a non-empty base64 BMP string (guaranteed by Catalog microservice startup validation in `ProductCatalog.cs`).
