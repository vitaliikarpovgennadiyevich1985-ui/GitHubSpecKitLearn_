# Feature Specification: WebUI Visual Product Cards

**Feature Branch**: `002-webui-visual-product-cards`
**Created**: 2026-05-05
**Status**: Draft
**Input**: User description: "WebUI - Products - Visual Cards - In the main Web UI project Asp.Net.Core.Learning.UI, the product catalog items retrieved from the Catalog microservice are currently displayed without images. Enhance the UI to properly display catalog items together with their associated images, with a card-based layout per product showing image and metadata."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Product Catalog with Images (Priority: P1)

A user navigates to the product catalog page in the Web UI and sees each product displayed as a distinct visual card. Each card shows the product image alongside its title, description, and price in a structured layout.

**Why this priority**: This is the core deliverable — without image display in a card layout, the feature is not complete. All other stories build on top of this.

**Independent Test**: Navigate to the catalog page while the Catalog microservice is running. Verify that each product renders as a card with an image visible, and that product metadata (title, description, price) appears in a vertical layout within the card.

**Acceptance Scenarios**:

1. **Given** a user is authenticated and the catalog page loads successfully, **When** the catalog view is displayed, **Then** each product appears as a visually distinct card with clear separation from neighbouring cards.
2. **Given** the Catalog microservice returns products with image data, **When** the catalog page renders, **Then** each product card displays the product image.
3. **Given** the catalog page renders, **When** a user views any product card, **Then** the card shows title, description, and price each on its own line in a vertical layout.

---

### User Story 2 - Consistent Card Layout Across All Products (Priority: P2)

A user scrolling through the full product listing (100 products) sees cards that are uniform in size and spacing, regardless of content length differences between products.

**Why this priority**: Visual consistency ensures a professional and readable product listing; without it the P1 feature degrades in quality.

**Independent Test**: Load the catalog page with all 100 products. Verify that all cards have consistent dimensions, spacing, and alignment with no card visually bleeding into another.

**Acceptance Scenarios**:

1. **Given** 100 products are returned by the microservice, **When** all cards are rendered, **Then** all cards have consistent width and the spacing between them is uniform.
2. **Given** two products with different description lengths, **When** both are rendered as cards, **Then** both cards maintain consistent alignment so the layout does not break.

---

### Edge Cases

- What happens when the Catalog microservice returns a product with an empty or missing image? The card must still render with the remaining metadata; a neutral grey placeholder box (128×128 px) is shown in the image area.
- What happens when the catalog returns zero products? The view must render without errors, showing an empty state.
- What happens when the image data is malformed (not valid base64 BMP)? The card must still render the metadata without crashing.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The UI `Product` model MUST include an `Image` property that maps to the `Image` field returned by the Catalog microservice `/products` endpoint (base64-encoded BMP data).
- **FR-002**: The `CatalogService` MUST deserialize the `Image` field from the microservice response into the UI `Product` model automatically, with no change to the existing HTTP call.
- **FR-003**: The catalog view MUST render each product as a visually distinct card with clear separation from adjacent cards using spacing and/or borders, arranged in a multi-column grid layout.
- **FR-004**: Each product card MUST display the product image inline within the card, rendered from the base64 image data returned by the microservice, at a fixed display size of 128×128 px.
- **FR-005**: Each product card MUST display product metadata (Title, Description, Price) in a structured vertical layout where each field appears on its own line.
- **FR-006**: Image display MUST be integrated into the existing product retrieval flow without adding new API endpoints or separate image requests.
- **FR-007**: The catalog view MUST degrade gracefully when a product's image data is empty — the card MUST still render with available metadata and a neutral grey placeholder box (128×128 px) MUST be shown in place of the missing image.
- **FR-008**: The catalog view MUST display all products on a single page with no pagination; all items returned by the microservice MUST be visible without additional navigation.
- **FR-009**: Product image elements MUST carry an empty `alt` attribute (decorative role per WCAG 2.1 technique H67) because images are programmatically generated colour swatches with no semantic content.

### Key Entities

- **Product (UI Model)**: Represents a catalog item as received from the Catalog microservice. Fields: `ProductId`, `Title`, `Description`, `Price`, `Image` (base64-encoded BMP string). The `Image` field is new — it is already present in the microservice response but not yet mapped in the UI model.
- **Product Card (View Component)**: The visual unit representing a single product in the catalog listing. Contains an image region and a metadata region.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Every product card on the catalog page displays an image without requiring any additional user action or page reload.
- **SC-002**: All 100 product cards render on the catalog page without any layout breakage or overlapping elements; each product image is displayed at 128×128 px.
- **SC-003**: Each card clearly shows title, description, and price as separate lines — no field is hidden, truncated unexpectedly, or merged with another on the same line.
- **SC-004**: The catalog page loads and displays all cards within the same response time as the current implementation (no additional network round-trips introduced).
- **SC-005**: The card-based grid layout is visually distinguishable — a first-time visitor can identify where one product ends and the next begins without ambiguity, and multiple products are visible per row.

## Clarifications

### Session 2026-05-05

- Q: What display size should product images be rendered at inside each card? → A: 128×128 px medium thumbnail
- Q: Should the catalog page show all 100 products on a single page or paginate them? → A: Single page, all 100 products
- Q: Should product image elements include descriptive alt text for accessibility? → A: Decorative — empty alt=""
- Q: Should product cards flow in a vertical list or a multi-column grid? → A: Multi-column grid
- Q: What should appear in the image area when a product's image data is empty or missing? → A: Neutral grey placeholder box, same size (128×128 px)

## Assumptions

- The Catalog microservice already returns the `Image` field as a base64-encoded BMP string in the `/products` response (implemented in feature 001-catalog-product-image).
- The `Image` field on the UI `Product` model can be added as a plain string property; no binary conversion is needed in C# code because HTML `<img>` tags support inline base64 data URIs.
- The image format is BMP (10×10 solid-color), which is supported by all target browsers via `data:image/bmp;base64,...` data URIs.
- The catalog page displays all 100 products on a single page with no pagination; the fixed dataset size makes pagination unnecessary.
- Mobile responsiveness is out of scope for this feature; the card layout targets desktop browsers.
- No new NuGet packages are required — the card layout is implemented with HTML and CSS within the existing Razor view.
- Authentication and authorization for the catalog page remain unchanged.
- The existing `CatalogService.GetProducts()` HTTP call already retrieves the full product payload including `Image`; only the deserialization model needs updating.
