# API Endpoints Reference Table

## Complete List of All API Endpoints

| # | Method | Endpoint | Headers | Request Body | Response | Notes |
|---|--------|----------|---------|--------------|----------|-------|
| **AUTHENTICATION** |
| 1 | `POST` | `/api/ssms/v0.1/login` | `Content-Type: application/json` | `{"userId": "string", "password": "string", "companyKey": 2, "projectKey": 282}` | `200 OK` - `{"token": "string", "userId": "string", "companyKey": 2, "projectKey": 282}` | No auth required |
| **ACCESS LEVEL** |
| 2 | `GET` | `/api/ssms/v0.1/accessLevel` | `Authorization: Bearer {token}` | None | `200 OK` - Array of access levels | User context required |
| **ITEMS** |
| 3 | `GET` | `/api/ssms/v0.1/items` | `Authorization: Bearer {token}` | None | `200 OK` - Array of all items | Returns all items including inactive |
| 4 | `GET` | `/api/ssms/v0.1/items/active` | `Authorization: Bearer {token}` | None | `200 OK` - Array of active items | Filters `fInAct = false` |
| 5 | `POST` | `/api/ssms/v0.1/items` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"itemCode": "string", "itemType": "string", "partNo": "string?", "itemName": "string", "description": "string?", "itmCat1Ky": 0, "itmCat2Ky": 0, "itmCat3Ky": 0, "itmCat4Ky": 0, "unitKey": 1, "discountPrecentage": 0.0, "costPrice": 0.00, "salesPrice": 0.00, "discountAmount": 0.0, "quantity": 0.0}` | `201 Created` - `{"message": "Item added successfully. ItemKey = {id}"}` | All required fields must be provided |
| 6 | `PUT` | `/api/ssms/v0.1/items` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"itemKey": 1, "itemCode": "string?", "itemType": "string?", "partNo": "string?", "itemName": "string?", "description": "string?", "itmCat1Ky": 0, "itmCat2Ky": 0, "itmCat3Ky": 0, "itmCat4Ky": 0, "unitKey": 1, "discountPrecentage": 0.0, "costPrice": 0.00, "salesPrice": 0.00, "discountAmount": 0.0, "quantity": 0.0, "fInAct": false}` | `200 OK` - `{"message": "Item updated successfully"}` | itemKey is required |
| 7 | `DELETE` | `/api/ssms/v0.1/items/{itemKey}` | `Authorization: Bearer {token}` | None | `200 OK` - `{"message": "Item deleted successfully"}` | Soft delete (sets fInAct = true) |
| **ITEM BATCHES** |
| 8 | `GET` | `/api/ssms/v0.1/items/batch/{itemKey}` | `Authorization: Bearer {token}` | None | `200 OK` - Array of item batches | Returns batches for specific item |
| 9 | `POST` | `/api/ssms/v0.1/items/batch` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"itemKey": 1, "batchNo": "string?", "expirDt": "2027-02-09T00:00:00Z", "costPrice": 0.0, "salePrice": 0.0, "quantity": 0.0}` | `201 Created` - `{"message": "Item batch added successfully", "batchKey": 123}` | itemKey must exist |
| 10 | `PUT` | `/api/ssms/v0.1/items/batch` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"itemBatchKey": 1, "itemKey": 1, "batchNo": "string?", "expirDt": "2027-02-09T00:00:00Z", "costPrice": 0.0, "salePrice": 0.0, "quantity": 0.0}` | `200 OK` - `{"message": "Item batch updated successfully"}` | Both keys required |
| **VEHICLE TYPES** |
| 11 | `GET` | `/api/ssms/v0.1/vehicletype` | `Authorization: Bearer {token}` | None | `200 OK` - Array of vehicle types | Filters ConCd = 'OrdCat1' |
| 12 | `POST` | `/api/ssms/v0.1/vehicletype` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"code": "string", "cdNm": "string"}` | `201 Created` - `{"message": "Vehicle Type added successfully"}` | Code must be unique |
| 13 | `PUT` | `/api/ssms/v0.1/vehicletype` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"cdKy": 1, "code": "string", "cdNm": "string"}` | `200 OK` - `{"message": "Vehicle Type updated successfully"}` | cdKy required |
| 14 | `DELETE` | `/api/ssms/v0.1/vehicletype/{cdKy}` | `Authorization: Bearer {token}` | None | `200 OK` - `{"message": "Vehicle Type deleted successfully"}` | Soft delete |
| **CUSTOMER ACCOUNTS** |
| 15 | `GET` | `/api/ssms/v0.1/CustomerAccount` | `Authorization: Bearer {token}` | None | `200 OK` - Array of customer accounts | Returns all customer type accounts |
| **PAYMENT TERMS** |
| 16 | `GET` | `/api/ssms/v0.1/PaymentTerm` | `Authorization: Bearer {token}` | None | `200 OK` - Array of payment terms | Returns all payment term codes |
| **SALES ACCOUNTS** |
| 17 | `GET` | `/api/ssms/v0.1/SalesAccount` | `Authorization: Bearer {token}` | None | `200 OK` - Array of sales accounts | Returns all sales type accounts |
| **INVOICE DETAILS** |
| 18 | `GET` | `/api/ssms/v0.1/InvoiceDetails` | `Authorization: Bearer {token}` | None | `200 OK` - Array of invoice details | Returns invoice line items |
| **CUSTOMERS** |
| 19 | `GET` | `/api/ssms/v0.1/Customer` | `Authorization: Bearer {token}` | None | `200 OK` - Array of customers | Returns customer with address info |
| 20 | `POST` | `/api/ssms/v0.1/Customer` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"adrNm": "string", "adrId": "string", "adr1": "string?", "city": "string?", "email": "string?", "tel": "string?", "accCd": "string?", "accNm": "string?", "accTyp": "string?"}` | `201 Created` - `{"message": "Customer added successfully"}` | Creates address and account |
| 21 | `PUT` | `/api/ssms/v0.1/Customer` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"adrKy": 1, "accKy": 1, "adrNm": "string?", "adr1": "string?", "city": "string?", "email": "string?", "tel": "string?", "accCd": "string?", "accNm": "string?"}` | `200 OK` - `{"message": "Customer updated successfully"}` | Both keys required |
| **BAYS** |
| 22 | `GET` | `/api/ssms/v0.1/bay` | `Authorization: Bearer {token}` | None | `200 OK` - Array of active bays | Returns bays where IsActive = true |
| 23 | `POST` | `/api/ssms/v0.1/bay` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"bayCd": "string", "bayNm": "string", "isReservationAvailable": true, "description": "string?"}` | `201 Created` - `{"message": "Bay added successfully"}` | bayKy is auto-generated by database. bayCd must be unique |
| 24 | `PUT` | `/api/ssms/v0.1/bay` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"bayKy": 1, "bayCd": "string", "bayNm": "string", "isReservationAvailable": true, "description": "string?"}` | `200 OK` - `{"message": "Bay updated successfully"}` | bayKy required for update |
| 25 | `DELETE` | `/api/ssms/v0.1/bay/{bayKy}` | `Authorization: Bearer {token}` | None | `200 OK` - `{"message": "Bay deleted successfully"}` | Sets IsActive = false |
| **BAY CONTROL** |
| 26 | `GET` | `/api/ssms/v0.1/baycontrol/available` | `Authorization: Bearer {token}` | None | `200 OK` - Array of available bays | Returns currently available bays |
| 27 | `GET` | `/api/ssms/v0.1/baycontrol/status` | `Authorization: Bearer {token}` | None | `200 OK` - Array of bay statuses | Returns all bays with status |
| 28 | `GET` | `/api/ssms/v0.1/baycontrol/reservable` | `Authorization: Bearer {token}` | None | `200 OK` - Array of reservable bays | Returns bays that can be reserved |
| 29 | `POST` | `/api/ssms/v0.1/baycontrol/reservation` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"bayKy": 1, "resDt": "2026-02-10T00:00:00Z", "fromTm": "10:00", "toTm": "11:00", "vehicleId": "string", "customerName": "string?", "contactNo": "string?"}` | `200 OK` - `{"message": "Reservation created successfully", "resKy": 123}` | Bay must be available |
| 30 | `PUT` | `/api/ssms/v0.1/baycontrol/reservation/{resKy}/status` | `Authorization: Bearer {token}` | Query: `?status=Approved` | `200 OK` - `{"message": "Reservation status updated"}` | Status: Pending/Approved/Cancelled |
| 31 | `DELETE` | `/api/ssms/v0.1/baycontrol/reservation/{resKy}` | `Authorization: Bearer {token}` | None | `200 OK` - `{"message": "Reservation deleted successfully"}` | Hard delete |
| 32 | `PUT` | `/api/ssms/v0.1/baycontrol/control` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"bayKy": 1, "status": "string", "serviceOrdKy": 0}` | `200 OK` - `{"message": "Bay control updated"}` | Updates bay operational status |
| **RESERVATIONS** |
| 33 | `GET` | `/api/ssms/v0.1/reservation` | `Authorization: Bearer {token}` | None | `200 OK` - Array of reservations | Returns all bay reservations |
| 34 | `POST` | `/api/ssms/v0.1/reservation` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"bayKy": 1, "resDt": "2026-02-10T00:00:00Z", "fromTm": "10:00", "toTm": "11:00", "vehicleId": "string", "driverName": "string?", "contactNo": "string?"}` | `201 Created` - `{"message": "Reservation created successfully"}` | Validates bay availability |
| 35 | `PUT` | `/api/ssms/v0.1/reservation` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"resKy": 1, "bayKy": 1, "resDt": "2026-02-10T00:00:00Z", "fromTm": "10:00", "toTm": "11:00", "vehicleId": "string", "status": "string?"}` | `200 OK` - `{"message": "Reservation updated successfully"}` | resKy required |
| 36 | `DELETE` | `/api/ssms/v0.1/reservation/{resKy}` | `Authorization: Bearer {token}` | None | `200 OK` - `{"message": "Reservation deleted successfully"}` | Hard delete |
| **SERVICE ORDERS** |
| 37 | `GET` | `/api/ssms/v0.1/serviceorder` | `Authorization: Bearer {token}` | None | `200 OK` - Array of service orders | Returns all service orders |
| 38 | `POST` | `/api/ssms/v0.1/serviceorder` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"trnDt": "2026-02-09T00:00:00Z", "vehicleKy": 1, "serviceType": "string", "description": "string?", "items": [{"itemKy": 1, "qty": 1.0, "unitPrice": 100.00}]}` | `201 Created` - `{"message": "Service order created successfully", "serviceOrdKy": 123}` | Creates order with items |
| 39 | `PUT` | `/api/ssms/v0.1/serviceorder` | `Authorization: Bearer {token}`<br>`Content-Type: application/json` | `{"serviceOrdKy": 1, "status": "string", "items": [{"itemKy": 1, "qty": 1.0, "unitPrice": 100.00}]}` | `200 OK` - `{"message": "Service order updated successfully"}` | serviceOrdKy required |
| 40 | `GET` | `/api/ssms/v0.1/serviceorder/{serviceOrdKy}/items` | `Authorization: Bearer {token}` | None | `200 OK` - Array of service order items | Returns items for specific order |
| **ORDERS (INVOICE)** |
| 41 | `GET` | `/api/ssms/v0.1/order/{trnNo}` | `Authorization: Bearer {token}` | None | `200 OK` - Invoice/Order details | Returns order by transaction number |

---

## Common Headers

### For All Authenticated Endpoints:
```
Authorization: Bearer {your_jwt_token_here}
```

### For POST/PUT Requests (JSON):
```
Content-Type: application/json
Authorization: Bearer {your_jwt_token_here}
```

---

## Authentication Flow

1. **Login** to get JWT token:
```http
POST /api/ssms/v0.1/login
Content-Type: application/json

{
  "userId": "serviceAdmin",
  "password": "11111",
  "companyKey": 2,
  "projectKey": 282
}
```

2. **Use token** in subsequent requests:
```http
GET /api/ssms/v0.1/items
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Response Status Codes

| Code | Meaning | When Used |
|------|---------|-----------|
| `200` | OK | Successful GET, PUT, DELETE operations |
| `201` | Created | Successful POST operations |
| `400` | Bad Request | Invalid input, validation errors |
| `401` | Unauthorized | Missing or invalid JWT token |
| `404` | Not Found | Resource doesn't exist |
| `409` | Conflict | Duplicate entry (e.g., code already exists) |
| `500` | Internal Server Error | Server-side error |

---

## Notes

- **?** indicates optional fields in request bodies
- All timestamps should be in ISO 8601 format: `YYYY-MM-DDTHH:mm:ssZ`
- Soft deletes set `fInAct = true` or `IsActive = false`
- Hard deletes permanently remove the record
- All decimal values use 2 decimal places for currency
- `{parameter}` in URL path indicates a route parameter
