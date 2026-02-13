# API Documentation

## 1. Authentication (v1 - Proposed)

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| POST | `/api/v1/auth/login` | Login (email/NIC + password) -> Returns JWT & user info | No |
| POST | `/api/v1/auth/refresh` | Refresh access token | Yes (Refresh Token) |
| GET | `/api/v1/auth/me` | Get current authenticated user + default branch + permissions + preferences | Yes (Bearer Token) |

### POST `/api/v1/auth/login`
**Request Body**
```json
{
  "user_name": "email or nic",
  "password": "password"
}
```
**Response**
```json
{
  "success": true,
  "message": "Login successful",
  "data": { "token": "...", "user": { ... } }
}
```

### GET `/api/v1/auth/me`
**Response**
```json
{
  "success": true,
  "data": { "user": { ... }, "default_branch": { ... }, "permissions": [...] }
}
```

---

# 2. Existing Endpoints (v0.1)

## 2.1 Authentication & Authorization
Base Path: `/api/ssms/v0.1/`

### POST `/login`
**Request Body** (`LoginRequestDto`)
```json
{
  "UserId": "string",
  "Password": "password",
  "CompanyKey": 1,
  "ProjectKey": 1
}
```
**Response** (`LoginResponseDto`)
```json
{
  "Token": "jwt_token_string",
  "ExpireAt": "2026-02-05T15:30:00Z"
}
```

### POST `/logout`
**Request Body**: None (Uses Authorization Header)
**Response**: `"Logged out successfully"`

---

## 2.2 User Management
Base Path: `/api/ssms/v0.1/users`

### POST `/`
**Description**: Create a new user.
**Request Body** (`CreateUserDto`)
```json
{
  "UsrNm": "username",
  "UsrId": "userid",
  "NewPwd": "password",
  "ConfirmPwd": "password",
  "PwdTip": "hint"
}
```

### PUT `/{usrKy}`
**Description**: Change user password.
**Request Body** (`ChangePasswordDto`)
```json
{
  "UsrKy": 123,
  "OldPwd": "current_password",
  "NewPwd": "new_password",
  "ConfirmPwd": "new_password",
  "PwdTip": "new_hint"
}
```

---

## 2.3 Customers
Base Path: `/api/ssms/v0.1/Customer`

### POST `/`
**Description**: Add a new customer.
**Request Body** (`AddCustomerAddressDto`)
```json
{
  "AdrNm": "Customer Name",
  "FstNm": "First",
  "MidNm": "Middle",
  "LstNm": "Last",
  "Address": "Full Address",
  "TP1": "Phone 1",
  "TP2": "Phone 2",
  "EMail": "customer@example.com",
  "ourCd": "CUS",
  "GPSLoc": "lat,long"
}
```

### PUT `/`
**Description**: Update customer details.
**Request Body** (`UpdateCustomerAddressDto`)
```json
{
  "AdrKy": 123,
  "AdrNm": "Updated Name",
  "FstNm": "First",
  "MidNm": "Middle",
  "LstNm": "Last",
  "Address": "Updated Address",
  "TP1": "Phone 1",
  "TP2": "Phone 2",
  "EMail": "new@example.com",
  "GPSLoc": "lat,long"
}
```

---

## 2.4 Vehicles
Base Path: `/api/ssms/v0.1/vehicle`

### POST `/`
**Description**: Register a new vehicle.
**Request Body** (`CreateVehicleRequestDto`)
```json
{
  "VehicleId": "WP CA-1234",
  "VehicleTypKy": 1,
  "FuelTyp": "Petrol",
  "CurrentMileage": 50000,
  "FuelLevel": 0.5,
  "Make": "Toyota",
  "Model": "Corolla",
  "Year": 2020,
  "ChassisNo": "ABC12345",
  "EngineNo": "ENG12345",
  "Description": "Silver Sedan",
  "Owner": {
    "AdrKy": 123, // If existing
    // Or new owner details
    "AdrNm": "Owner Name"
  },
  "Drivers": [
    { "AdrKy": 456, "AdrNm": "Driver Name" }
  ]
}
```

### PUT `/`
**Description**: Update vehicle details.
**Request Body**: Same as POST, but includes `VehicleKy`.

### GET `/{vehicleKy}`
**Response** (`VehicleDetailDto`)
```json
{
  "VehicleKy": 10,
  "VehicleId": "WP CA-1234",
  "VehicleTyp": "Car",
  "FuelTyp": "Petrol",
  "Owner": { ... },
  "Drivers": [ ... ]
}
```

---

## 2.5 Products & Stock
Base Path: `/api/ssms/v0.1/items`

### POST `/`
**Description**: Add a new item.
**Request Body** (`AddItemDTO`)
```json
{
  "itemCode": "ITM001",
  "itemType": "Part",
  "partNo": "PN-123",
  "itemName": "Oil Filter",
  "description": "Oil Filter for Corolla",
  "itmCat1Ky": 1,
  "unitKey": 1,
  "costPrice": 1500.00,
  "salesPrice": 2500.00,
  "quantity": 100
}
```

### POST `/batch`
**Description**: Add an item batch.
**Request Body** (`AddItemBatchDTO`)
```json
{
  "itemKey": 10,
  "batchNo": "BATCH-001",
  "expirDt": "2027-12-31",
  "costPrice": 1500,
  "salePrice": 2500,
  "quantity": 50
}
```

### Stock Operations
Base Path: `/api/ssms/v0.1/Stock`

#### POST `/addition`
**Description**: Create stock addition.
**Request Body** (`StockAddPostDTO`)
```json
{
  "trnNo": null, // null for new
  "trnDate": "2026-02-05",
  "locKey": 1,
  "description": "Initial Stock",
  "items": [
    {
      "itemKey": 10,
      "quantity": 50,
      "costPrice": 1500,
      "salePrice": 2500,
      "unitKey": 1
    }
  ]
}
```

#### POST `/deduction`
**Description**: Create stock deduction.
**Request Body** (`StockDeductionPostDTO`)
```json
{
  "trnDate": "2026-02-05",
  "locKey": 1,
  "description": "Damaged items",
  "items": [
    {
      "itemKey": 10,
      "quantity": 2,
      "costPrice": 1500,
      "salePrice": 2500
    }
  ]
}
```

---

## 2.6 Sales & Orders
Base Path: `/api/ssms/v0.1`

### POST `/Invoice`
**Description**: Create an invoice.
**Request Body** (`InvoiceDto`)
```json
{
  "DocNo": "INV001",
  "YurRef": "REF123",
  "AdrCd": "CUS001",
  "AccKy": 1,
  "PmtTrm1": "Cash",
  "PmtTrm1Amt": 5000.00,
  "Amt": 5000.00,
  "DisAmt": 0,
  "Items": [
    {
      "ItmKy": 10,
      "Qty": 2,
      "TrnPri": 2500.00
    }
  ]
}
```

---

## 2.7 Service Orders
Base Path: `/api/ssms/v0.1/serviceorder`

### POST `/`
**Description**: Create a new service order.
**Request Body** (`CreateServiceOrderDto`)
```json
{
  "CustomerName": "John Doe",
  "CustomerPhone": "0771234567",
  "VehicleId": "WP CA-1234",
  "CurrentMileage": 50000,
  "DamageNote": "Scratch on bumper",
  "AdditionalNotes": "Urgent",
  "PackageKy": 1,
  "BayKy": 1
}
```

### POST `/item`
**Description**: Add a service item to an order.
**Request Body** (`AddServiceItemDto`)
```json
{
  "ServiceOrdKy": 100,
  "ItemName": "Oil Change",
  "Price": 5000.00,
  "EstimatedTime": "30 mins"
}
```

### PUT `/item/approval`
**Description**: Approve or reject a service item.
**Request Body** (`ApproveServiceItemDto`)
```json
{
  "ServiceOrdDetKy": 501,
  "IsApproved": true,
  "CustName": "John Doe",
  "IpAddress": "192.168.1.10",
  "Device": "Mobile"
}
```

### PUT `/item/status`
**Description**: Update status of a service item.
**Request Body** (`UpdateItemStatusDto`)
```json
{
  "ServiceOrdDetKy": 501,
  "Status": "InProgress"
}
```

### GET `/`
**Description**: Get all service orders.
**Response**: List of `ServiceOrderDto`.

### GET `/{ordKy}`
**Description**: Get details of a specific service order.
**Response** (`ServiceOrderDto`)
```json
{
  "ServiceOrdKy": 100,
  "ServiceOrdNo": "SO-001",
  "VehicleId": "WP CA-1234",
  "CustomerName": "John Doe",
  "PackageName": "Full Service",
  "Status": "Pending",
  "Items": [
    {
      "ServiceOrdDetKy": 501,
      "ItemName": "Oil Change",
      "Price": 5000.00,
      "Status": "InProgress",
      "IsApproved": true
    }
  ]
}
```

---

## 2.8 Bay Management
Base Path: `/api/ssms/v0.1`

### POST `/bay`
**Request Body** (`CreateBayDto`)
```json
{
  "BayCd": "B01",
  "BayNm": "Washing Bay 1",
  "IsReservationAvailable": true
}
```

### POST `/baycontrol/reservation`
**Request Body** (`CreateReservationDto`)
```json
{
  "VehicleKy": 10,
  "BayKy": 1,
  "FromDtm": "2026-02-06T10:00:00",
  "ToDtm": "2026-02-06T11:00:00",
  "ResType": "Online"
}
```

### PUT `/baycontrol/update-status`
**Request Body** (`UpdateBayControlDto`)
```json
{
  "BayKy": 1,
  "IsOccupied": true,
  "VehicleKy": 10,
  "CurrentActivity": "Washing",
  "EstimatedFinishDtm": "2026-02-05T16:00:00"
}
```

---

## 2.9 Master Data
Base Path: `/api/ssms/v0.1`

### POST `/code` (or `/create`)
**Request Body** (`CreateCodeDto`)
```json
{
  "Code": "ABC",
  "CdNm": "Description",
  "ConCd": "CatCode"
}
```

### POST `/lookups/trnNoLast`
**Request Body** (`GetLastTrnNoRequestDto`)
```json
{
  "OurCd": "INV"
}
```

---

## 2.10 Reservation Management
Base Path: `/api/ssms/v0.1/reservation`

### POST `/`
**Description**: Create a new reservation with vehicle and owner details.
**Request Body** (`CreateFullReservationDto`)
```json
{
  "VehicleId": "WP CA-1234",
  "NewVehicleDetails": {
    "VehicleId": "WP CA-1234",
    "VehicleTypKy": 1,
    "FuelTyp": "Petrol",
    "Make": "Toyota",
    "Model": "Corolla",
    "Owner": {
      "FstNm": "John",
      "LstNm": "Doe",
      "Address": "123 Main St",
      "TP1": "0771234567"
    },
    "Drivers": []
  },
  "PackageKy": 1,
  "BayKy": 1,
  "BookingFrom": "2026-02-10T10:00:00",
  "BookingTo": "2026-02-10T12:00:00",
  "Remarks": "First service"
}
```
**Response**
```json
{
  "message": "Reservation placed successfully, awaiting approval.",
  "resKy": 123
}
```

### PUT `/{resKy}`
**Description**: Update an existing reservation.
**Request Body**: Same as POST.

### DELETE `/{resKy}`
**Description**: Delete (soft delete) a reservation.
**Response**: `"Reservation deleted"`

### PUT `/{resKy}/approval`
**Description**: Approve or reject a reservation.
**Request Body** (`ApproveReservationDto`)
```json
{
  "IsApproved": true
}
```

### GET `/`
**Description**: Get all reservations.
**Response**: List of `ReservationDetailDto`.

### GET `/vehicle/{vehicleId}`
**Description**: Get reservations by vehicle ID.
**Response**: List of `ReservationDetailDto`.

### GET `/date/{date}`
**Description**: Get reservations by date (format: YYYY-MM-DD).
**Response**: List of `ReservationDetailDto`.

**Response Example** (`ReservationDetailDto`)
```json
{
  "ResKy": 123,
  "VehicleKy": 10,
  "VehicleId": "WP CA-1234",
  "VehicleType": "Car",
  "OwnerName": "John Doe",
  "OwnerPhone": "0771234567",
  "PackageKy": 1,
  "PackageName": "Full Service",
  "BayKy": 1,
  "BayName": "Bay 1",
  "FromDtm": "2026-02-10T10:00:00",
  "ToDtm": "2026-02-10T12:00:00",
  "ResStatus": "Approved",
  "Remarks": "First service"
}
```

---

## 2.11 Order Management
Base Path: `/api/ssms/v0.1/order`

### POST `/`
**Description**: Create a new order with order details.
**Request Body** (`CreateOrderDto`)
```json
{
  "LocKy": 1,
  "OrdTyp": "SO",
  "OrdTypKy": 1,
  "Adrky": 100,
  "AccKy": 50,
  "PmtTrmKy": 1,
  "SlsPri": 25000.00,
  "DisPer": 5.0,
  "Des": "Service Order for WP CA-1234",
  "DocNo": "SO-001",
  "OrdDt": "2026-02-08T10:00:00",
  "DlryDt": "2026-02-10T10:00:00",
  "OrdFrqKy": 1,
  "OrdStsKy": 1,
  "BUKy": 1,
  "OrdCat1Ky": 1,
  "OrdCat2Ky": 0,
  "OrdCat3Ky": 0,
  "MarPer": 0,
  "Status": "PE",
  "SKy": 1,
  "OrderDetails": [
    {
      "AdrKy": 100,
      "LiNo": 1,
      "ItmKy": 10,
      "ItmCd": "ITM001",
      "Des": "Oil Filter",
      "Status": "PE",
      "OrdQty": 2,
      "BulkQty": 0,
      "EstPri": 1500.00,
      "OrdPri": 1500.00,
      "SlsPri": 2000.00,
      "DisPer": 0,
      "DisAmt": 0,
      "OrdUnitKy": 1,
      "BulkFctr": 1,
      "StkStsKy": 1,
      "NetWt": 0,
      "BUKy": 1,
      "CdKy1": 0,
      "Amt1": 0,
      "Amt2": 0,
      "fNoPrnPri": false,
      "isMatSub": false,
      "isLabSub": false,
      "isPltSub": false,
      "MatAmt": 0,
      "LabAmt": 0,
      "PltAmt": 0,
      "SubConAmt": 0,
      "SubOHP": 0
    }
  ]
}
```
**Response**
```json
{
  "message": "Order created successfully",
  "ordKy": 456
}
```

### PUT `/{ordKy}`
**Description**: Update an existing order.
**Request Body**: Same as POST (without `OrdKy` in body, use route parameter).

### DELETE `/{ordKy}`
**Description**: Delete (soft delete) an order.
**Response**: `"Order deleted successfully"`

### GET `/`
**Description**: Get all orders.
**Response**: List of `OrderListDto`.
```json
[
  {
    "OrdKy": 456,
    "OrdNo": 1001,
    "OrdTyp": "SO",
    "OrdDt": "2026-02-08T10:00:00",
    "Des": "Service Order for WP CA-1234",
    "SlsPri": 25000.00,
    "Status": "PE",
    "fFinish": false,
    "CustomerName": "John Doe"
  }
]
```

### GET `/orderno/{ordNo}`
**Description**: Get order by order number.
**Response**: `OrderDetailResponseDto`.

### GET `/{ordKy}`
**Description**: Get order by order key.
**Response** (`OrderDetailResponseDto`)
```json
{
  "OrdKy": 456,
  "OrdNo": 1001,
  "CKy": 1,
  "LocKy": 1,
  "OrdTyp": "SO",
  "OrdTypKy": 1,
  "Adrky": 100,
  "AccKy": 50,
  "PmtTrmKy": 1,
  "SlsPri": 25000.00,
  "DisPer": 5.0,
  "fInAct": false,
  "fApr": 0,
  "fInv": false,
  "fFinish": false,
  "Des": "Service Order for WP CA-1234",
  "DocNo": "SO-001",
  "OrdDt": "2026-02-08T10:00:00",
  "DlryDt": "2026-02-10T10:00:00",
  "Status": "PE",
  "CustomerName": "John Doe",
  "CustomerAddress": "123 Main St",
  "Details": [
    {
      "OrdDetKy": 789,
      "LiNo": 1,
      "ItmKy": 10,
      "ItmCd": "ITM001",
      "Des": "Oil Filter",
      "Status": "PE",
      "OrdQty": 2,
      "DlvQty": 0,
      "OrdPri": 1500.00,
      "SlsPri": 2000.00,
      "DisPer": 0,
      "DisAmt": 0,
      "Rem": null
    }
  ]
}
```

---

## 2.12 Calendar Management
Base Path: `/api/ssms/v0.1/calendar`

### GET `/unavailable-dates`
**Description**: Get a list of dates that are unavailable for reservation (e.g., holidays, closed days).
**Response**: List of unavailable dates and descriptions.
```json
[
  {
    "date": "2026-12-25T00:00:00",
    "description": "Christmas Day"
  },
  {
    "date": "2026-01-01T00:00:00",
    "description": "New Year"
  }
]
```

---

## Status Codes Reference

| Code | Description |
|------|-------------|
| PE | Pending |
| AP | Approved |
| IP | In Progress |
| CO | Completed |
| CA | Cancelled |
