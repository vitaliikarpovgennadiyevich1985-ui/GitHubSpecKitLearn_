# Feature Specification: Catalog Product Image Field & 100-Product Endpoint

**Feature Branch**: `001-catalog-product-image`  
**Created**: 2026-05-05  
**Status**: Draft  
**Input**: User description: "Catalog microservice's Product entity should have new field added to store image. In addition to this update Catalog microservice API endpoint `/products` to return 100 predefined products stored in memory where all products should have all fields populated with new image field as well"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Retrieve Products with Image (Priority: P1)

An API consumer calls the `/products` endpoint and receives a complete list of 100 products, each including an `Image` field alongside the existing product fields.

**Why this priority**: This is the core deliverable. Both the new image field and the expanded product catalog are required together. Without this, no downstream UI or integration can display products with visual content.

**Independent Test**: Can be fully tested by sending a GET request to `/products` and verifying: response contains exactly 100 items, each item includes a non-empty `Image` field, and all other fields (ProductId, Title, Description, Price) are also populated.

**Acceptance Scenarios**:

1. **Given** the `/products` endpoint is called with a valid authorization token, **When** the request is processed, **Then** the response contains exactly 100 product items.
4. **Given** the `/products` endpoint is called twice in succession, **When** the same product is retrieved from each response, **Then** its `ProductId` is identical in both responses.
2. **Given** the `/products` endpoint returns 100 products, **When** a consumer inspects any product in the list, **Then** the product includes a non-empty `Image` field alongside ProductId, Title, Description, and Price.
3. **Given** a product in the response list, **When** the `Image` field is examined, **Then** it contains a valid Base64-encoded string representing binary image data.

---

### User Story 2 - Product Data Completeness (Priority: P2)

An API consumer relies on all product fields being consistently populated for every product returned, with no products having empty or missing field values.

**Why this priority**: Data completeness ensures that consumers can safely render all product information without null-checks or fallback logic. This is a data quality concern secondary to the primary endpoint functionality.

**Independent Test**: Can be tested by iterating over all 100 products in the response and asserting that ProductId, Title, Description, Price, and `Image` are all non-null and non-empty for every item.

**Acceptance Scenarios**:

1. **Given** the full list of 100 products is returned, **When** each product is inspected, **Then** no product has a null or empty value for any field including `Image`.
2. **Given** the Product entity definition, **When** the entity is serialized to the API response, **Then** `Image` appears in the serialized output at the same level as existing fields.

---

### Edge Cases

- What happens when the authorization token is missing or invalid? The endpoint continues to enforce existing authorization requirements and returns 401/403 as before.
- How does the system handle a request when the in-memory product list is empty? This cannot occur because the 100 products are statically defined and always available.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Product entity MUST include a new `Image` field (a non-empty string value).
- **FR-002**: The `/products` endpoint MUST return exactly 100 products.
- **FR-003**: Every product returned by the `/products` endpoint MUST have all fields populated: ProductId, Title, Description, Price, and `Image`.
- **FR-004**: The 100 products MUST be predefined and held in memory; no external data source or persistent storage is required.
- **FR-005**: The `Image` field value for each product MUST be a non-empty Base64-encoded string representing binary image data.
- **FR-006**: All existing authorization and authentication requirements for the `/products` endpoint MUST remain unchanged.
- **FR-007**: Each product in the in-memory list MUST have a fixed, stable `ProductId` that does not change between requests.

### Key Entities

- **Product**: Represents a catalog item. Key attributes: unique identifier (`ProductId`), display title (`Title`), description (`Description`), price (`Price`), and image (`Image`). `Image` is the new field being added.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The `/products` endpoint returns exactly 100 products on every call.
- **SC-002**: 100% of products in the response have the `Image` field populated with a non-empty Base64-encoded string.
- **SC-003**: 100% of products in the response have all existing fields (ProductId, Title, Description, Price) populated with non-empty/non-zero values.
- **SC-004**: The endpoint response structure is backward-compatible — existing consumers reading current fields experience no breaking change.
- **SC-005**: The `ProductId` for each product is identical across multiple calls to `/products` (stable identity).

## Assumptions

- The `Image` field stores a Base64-encoded string representing binary image data (not a URL or file path).
- The 100 products are static and hardcoded in-memory; no database, seed script, or file-based storage is required.
- Existing authorization policy ("Catalog-Microservice-Read-Api") and authentication configuration remain unchanged.
- `Image` values are sample Base64-encoded image data embedded directly in the in-memory dataset.
- The `Image` field name follows the existing PascalCase naming convention of the `Product` entity.
- No pagination, filtering, or sorting of the 100 products is required as part of this feature.

## Clarifications

### Session 2026-05-05

- Q: What should the canonical property name be for the new image field on the `Product` entity? → A: `Image`
- Q: Should each of the 100 in-memory products have a stable, fixed `ProductId`, or regenerated on every request? → A: Fixed stable IDs
- Q: What format should the `Image` field value use for the 100 in-memory products? → A: Base64 binary image
