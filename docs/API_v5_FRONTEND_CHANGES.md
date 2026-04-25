# ScanPet API v5 — Frontend Change Notes

This document lists every contract change between **v4** and **v5**. Anything not listed is unchanged.

---

## 1. Refunds (BREAKING)

### Endpoint moved
- **v4:** `POST /api/orders/refund/{sku}`
- **v5:** `POST /api/orders/refund`

### Body
```json
{
  "orderId": "guid",
  "refundToInventoryId": "guid",
  "refundReason": "Defective product",
  "items": [
    { "orderItemId": "guid", "quantity": 1 }
  ]
}
```
- `items[]` accepts either `orderItemId` **or** `itemId` per line.
- Multiple lines per request are supported (single transaction).

### Response (200)
```json
{
  "success": true,
  "data": {
    "orderId": "guid",
    "orderNumber": "ORD-...",
    "orderStatus": 4,
    "orderStatusName": "PartiallyRefunded",
    "refundedPercent": 40.0,
    "refundedAmount": 60.00,
    "refundedItems": [
      {
        "orderItemId": "guid", "itemId": "guid", "itemName": "Pet Food",
        "itemImageUrl": "...", "serialNumber": "PF-001",
        "quantity": 1, "totalRefundedQuantity": 1, "orderedQuantity": 5,
        "refundedAmount": 30.00, "refundedPercent": 20.0,
        "status": 1, "statusName": "PartiallyRefunded"
      }
    ]
  }
}
```

### Order status values (extended)
| value | name |
|---|---|
| 0 | Pending |
| 1 | Confirmed |
| 2 | Cancelled |
| **3** | **Refunded** *(new — every line fully refunded)* |
| **4** | **PartiallyRefunded** *(new — at least one line refunded)* |

`GET /api/orders/{id}` and `GET /api/orders` now return:
```json
{
  "refundedQuantity": 3,
  "refundedAmount": 90.00,
  "refundedPercent": 60.0,
  "refundedItems": [
    { "orderItemId":"...", "itemId":"...", "refundedQuantity":3, "orderedQuantity":5, "refundedAmount":90, "refundedPercent":60 }
  ],
  "orderItems": [
    { "..." : "...", "refundedPercent": 60.0, "itemImageUrl": "..." }
  ]
}
```

---

## 2. User profile photo (NEW)

| endpoint | who | body |
|---|---|---|
| `POST /api/users/{id}/photo` | admin | multipart, field `photo` |
| `DELETE /api/users/{id}/photo` | admin | – |
| `POST /api/auth/me/photo` | self | multipart, field `photo` |
| `DELETE /api/auth/me/photo` | self | – |

`UserDto`, `LoginResponse.user` and `GET /api/auth/me` now expose **`photoUrl`** (nullable).

---

## 3. Item images everywhere

Every endpoint that returns an item now also returns **`itemImageUrl`**:
- `GET /api/orders/{id}` – `orderItems[].itemImageUrl`, `refundedItems` summaries
- `GET /api/inventories/item-counts` – `globalItemTotals[].itemImageUrl`, `inventories[].items[].itemImageUrl`
- `GET /api/inventories/{id}/item-counts` – `items[].itemImageUrl`
- `GET /api/inventories/low-stock`, `GET /api/inventories/items/{itemId}`, `GET /api/inventories/{id}/items` already exposed it.

---

## 4. Global keyword search

Every paginated list endpoint accepts an optional **`?keyword=`** query param. The match is case-insensitive and runs against:
1. The entity's `[Searchable]` fields (Name, SKU, Description, ClientName, OrderNumber, …)
2. **Every translation row in every language** (so an Arabic-language record matches an Arabic search term even when `Accept-Language=en`).

Endpoints with keyword:
- `GET /api/items?keyword=`
- `GET /api/orders?keyword=`
- `GET /api/users?keyword=`
- (Roles, Locations, Colors, Inventories list endpoints already use the same base — pass `keyword` if/when the controller signature is updated.)

---

## 5. Discounts (NEW)

```
GET    /api/discounts/item/{itemId}
POST   /api/discounts
PUT    /api/discounts/{id}
DELETE /api/discounts/{id}
```

### Body
```json
{
  "scope": 0,
  "itemId": "guid",
  "inventoryId": null,
  "locationId": null,
  "amount": 2.50,
  "label": "Spring sale",
  "isStackable": true,
  "isRevertable": true,
  "startsAt": null,
  "expiresAt": null
}
```

| `scope` | meaning | required ID |
|---|---|---|
| 0 | Item (everywhere) | `itemId` |
| 1 | ItemInventory (this item, this warehouse) | `itemId` + `inventoryId` |
| 2 | ItemLocation (this item, every warehouse in a Location) | `itemId` + `locationId` |

### Stack / Exclusive rules

- Each row carries its own **`isStackable`** boolean.
- The handler pre-computes the effective per-unit discount as
  **(sum of all stackable amounts)** + **(largest absolute amount among non-stackable rows)**.
- Negative amounts are accepted (= surcharge).
- **`amount` is stored as `null` whenever the value is 0** — frontend should treat `null` and `0` interchangeably for display ("No discount") but only send `null` (or omit) on save when there is no discount.

### Response

`GET /api/discounts/item/{itemId}` returns the full list, e.g.

```json
[
  { "id":"...", "scope":0, "scopeName":"Item",          "amount":2.50, "isStackable":true,  "isRevertable":true },
  { "id":"...", "scope":1, "scopeName":"ItemInventory", "amount":5.00, "isStackable":false, "inventoryId":"..." },
  { "id":"...", "scope":2, "scopeName":"ItemLocation",  "amount":1.00, "isStackable":true,  "locationId":"..." }
]
```

The `OrderItemDto` and `ItemInventoryDto` already carry placeholders for `discounts[]`, `effectivePerUnitDiscount` and `effectiveUnitPrice` so the UI can stop calling the discount endpoint on the hot path once the backend populates them.

### Effect on order creation

`POST /api/orders` automatically subtracts the resolved per-unit discount from each line. The total stored on the order is `? qty × max(0, unitPrice ? discount)`.

---

## 6. Multilanguage

A single endpoint covers every entity:

```
POST /api/translations/{entityType}/{entityId}
GET  /api/translations/{entityType}/{entityId}
```

`entityType` is the simple entity name: `Item`, `Color`, `Location`, `Inventory`, `Role`.

### POST body

```json
[
  { "lang": "ar", "name": "???? ???????", "description": "??????" },
  { "lang": "en", "name": "Pet food",     "description": "Best" }
]
```

`POST` **replaces** all translations for that entity (keys other than `lang`/`languageCode` are field names, values are strings).

### GET behaviour
- `Accept-Language: en` ? returns `{ language:"en", fields:{...} }`
- `Accept-Language: ar` ? returns `{ language:"ar", fields:{...} }` (falls back to `en` when missing)
- `Accept-Language: all` ? returns every language as a map.

### Search across all languages
Because the translation `Value` column is `[Searchable]`, every paginated list endpoint with `?keyword=` already searches **all** languages — you do not need to send the same query in multiple languages.

---

## 7. Audit hygiene

`createdAt` / `createdBy` are set on insert, **`updatedAt` / `updatedBy` are guaranteed `null`** until the entity is updated for the first time. Frontend code that displayed "updated" text on freshly created records should now check for null.

---

## 8. Migration & data
A consolidated migration `V5_Refund_Photo_Discounts_Translations` was applied. It adds:
- `Users.PhotoUrl`
- `Orders.OrderStatus` extended values 3 / 4
- `Discounts` table
- `EntityTranslations` table

No existing rows are deleted.
