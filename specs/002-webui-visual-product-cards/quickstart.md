# Quickstart: WebUI Visual Product Cards

**Feature**: `002-webui-visual-product-cards`
**Date**: 2026-05-05

---

## Prerequisites

- .NET 10 SDK installed
- Solution opens cleanly in Visual Studio or VS Code
- Feature `001-catalog-product-image` is implemented — the Catalog microservice must return the `Image` field from `/products`
- Aspire AppHost project (`Asp.Net.Core.Learning.AspireServer/Asp.Net.Core.Learning.AspireServer.AppHost`) is used to run all services together

---

## Minimal Implementation Steps

These steps are the minimum required to satisfy all acceptance criteria. Complete them in order.

### Step 1 — Add `Image` property to UI `Product` model

File: `Asp.Net.Core.Learning.UI/Models/Product.cs`

Add one property:

```csharp
public string Image { get; set; } = string.Empty;
```

This is the only C# code change required. `GetFromJsonAsync` will automatically populate this field from the microservice response.

---

### Step 2 — Rewrite `Views/Catalog/Index.cshtml`

File: `Asp.Net.Core.Learning.UI/Views/Catalog/Index.cshtml`

Replace the entire file content with a CSS Grid card layout:

```razor
@model IEnumerable<Product>
@{
    ViewData["Title"] = "Catalog";
}

<style>
    .product-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
        gap: 1rem;
        padding: 1rem;
    }

    .product-card {
        border: 1px solid #dee2e6;
        border-radius: 4px;
        padding: 0.75rem;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    .product-img-placeholder {
        width: 128px;
        height: 128px;
        background-color: #cccccc;
    }

    .product-meta {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
    }
</style>

<div class="product-grid">
    @foreach (var product in Model)
    {
        <div class="product-card">
            @if (!string.IsNullOrEmpty(product.Image))
            {
                <img src="data:image/bmp;base64,@product.Image"
                     alt=""
                     width="128"
                     height="128" />
            }
            else
            {
                <div class="product-img-placeholder"></div>
            }
            <div class="product-meta">
                <span><strong>@product.Title</strong></span>
                <span>@product.Description</span>
                <span>@product.Price.ToString("C")</span>
            </div>
        </div>
    }
</div>
```

---

## Running the Feature

1. Start all services via the Aspire AppHost:

   ```
   cd Asp.Net.Core.Learning.AspireServer\Asp.Net.Core.Learning.AspireServer.AppHost
   dotnet run
   ```

2. Open the Aspire dashboard URL printed to the console.
3. Navigate to the Web UI service URL.
4. Log in with a test user (via the Identity Server).
5. Navigate to the Catalog page (`/Catalog`).

---

## Acceptance Verification

| Criterion | How to verify |
|---|---|
| Every card shows an image | Visually confirm coloured squares appear in each card on the catalog page |
| Images display at 128×128 px | Inspect any `<img>` element in browser DevTools — width/height attributes must be 128 |
| Metadata on separate lines | Each card shows Title, Description, Price as three distinct lines |
| Multi-column grid | Multiple product cards appear per row (exact count depends on window width) |
| Cards separated | Border and gap visually separate each card |
| Empty image degrades to grey box | Temporarily set `product.Image = ""` in `CatalogService` to verify grey placeholder renders |
| Zero products renders cleanly | Temporarily return empty list from `GetProducts()` — page must load without error |
| No extra network requests | Browser DevTools Network tab — only one request to `/products` (no image fetch requests) |

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| Images not appearing | `Image` property missing from `Product.cs` | Add the property per Step 1 |
| Broken-image icon showing | `data:image/bmp;base64,` prefix missing or `Image` value is null | Check Razor `@if (!string.IsNullOrEmpty(...))` guard |
| All grey placeholders | Catalog microservice feature 001 not yet implemented | Verify `/products` response includes `image` field |
| Layout looks like old plain list | View file not saved or browser cache stale | Hard-refresh (`Ctrl+Shift+R`) |
