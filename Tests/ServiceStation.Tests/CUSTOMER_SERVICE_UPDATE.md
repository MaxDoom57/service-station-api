# Customer Service Update - Database Schema Implementation

## 📋 Summary
Updated the `CustomerService.AddCustomerAsync` method to properly create customer records across three related tables: **Address**, **AccMas**, and **AccAdr**.

---

## 🔄 Changes Made

### **1. Updated `AddCustomerAsync` Method**

**Previous Behavior:**
- Created record in `Address` table
- Created record in `AddressCdRel` table

**New Behavior:**
- ✅ Creates record in **Address** table
- ✅ Creates record in **AccMas** table (customer account)
- ✅ Creates record in **AccAdr** table (relationship between account and address)

---

## 📊 Database Tables

### **1. Address Table**
Stores customer address and contact information.

**Fields Created:**
- `AdrKy` (auto-generated primary key)
- `CKy` (company key from user context)
- `AdrCd` (auto-generated unique code - format: `{AdrNm}{random2digits}`)
- `fInAct` = `false` (active)
- `AdrNm` (customer name)
- `FstNm`, `MidNm`, `LstNm` (name components)
- `Address` (full address)
- `TP1`, `TP2` (phone numbers)
- `EMail` (email)
- `EntUsrKy`, `EntDtm` (audit fields)

### **2. AccMas Table**
Stores customer account information.

**Fields Created:**
- `AccKy` (auto-generated primary key)
- `CKy` (company key from user context)
- `AccCd` (auto-generated unique code - format: `ACC-yyyyMMddHHmmssfff`)
- `AccNm` (same as `AdrNm` from Address)
- `AccTyp` = `"CUS"` (customer account type)
- `AccTypKy` (foreign key to CdMas for "CUS" type)
- `fInAct` = `false` (active)
- `EntUsrKy`, `EntDtm` (audit fields)

**Fields NOT Set (as per requirements):**
- `AccLvl`, `fCtrlAcc`, `CtrlAccKy`, `fBasAcc`, `fMultiAdr`, `fDefault`, `fBlckList`, `CrLmt`, `CrDays`, `BUKy`

### **3. AccAdr Table**
Links customer account to their address (relationship table).

**Fields Created:**
- `AccKy` (from AccMas)
- `AdrKy` (from Address)

---

## 🆕 New Helper Method

### **`GenerateUniqueAccCdAsync`**
Generates unique account codes with datetime format.

**Format:** `ACC-yyyyMMddHHmmssfff`

**Example:** `ACC-20260209153045123`

**Features:**
- Includes milliseconds for uniqueness
- Checks database for collisions
- Retries with 1ms delay if collision detected
- Scoped to company (CKy)

---

## 📝 Request/Response Examples

### **POST /api/ssms/v0.1/Customer**

**Request:**
```json
{
  "adrNm": "John Doe",
  "fstNm": "John",
  "midNm": "Michael",
  "lstNm": "Doe",
  "address": "123 Main Street, Colombo",
  "tp1": "0771234567",
  "tp2": "0112345678",
  "eMail": "john.doe@example.com",
  "ourCd": "CUS"
}
```

**Database Operations:**
```sql
-- 1. Insert into Address
INSERT INTO Address (CKy, AdrCd, AdrNm, FstNm, ...) 
VALUES (2, 'JohnDoe45', 'John Doe', 'John', ...)
-- Returns: AdrKy = 100

-- 2. Insert into AccMas
INSERT INTO AccMas (CKy, AccCd, AccNm, AccTyp, AccTypKy, ...) 
VALUES (2, 'ACC-20260209153045123', 'John Doe', 'CUS', 10, ...)
-- Returns: AccKy = 500

-- 3. Insert into AccAdr
INSERT INTO AccAdr (AccKy, AdrKy) 
VALUES (500, 100)
```

**Response:**
```json
{
  "message": "Customer added successfully"
}
```

---

## ✅ Validation Rules (Unchanged)

1. **Email Format:** Must match regex pattern `^[^@\s]+@[^@\s]+\.[^@\s]+$`
2. **TP1/TP2:** Must be exactly 10 digits (if provided)
3. **AdrNm:** Must be unique (checked via `IsExistAdrNm`)
4. **UserKey:** Must exist for current user and company

---

## 🔧 Transaction Safety

All three inserts are wrapped in a **single transaction**:
- If any insert fails, all changes are rolled back
- Ensures data consistency across tables
- No orphaned records

---

## 📌 Notes

1. **AccCd Format:** Uses datetime with milliseconds to ensure uniqueness even for rapid consecutive requests
2. **AccNm vs AdrNm:** AccNm is set to the same value as AdrNm as per requirements
3. **AccTyp:** Always set to "CUS" for customer accounts (can be changed via `ourCd` parameter)
4. **Minimal AccMas Fields:** Only essential fields are set; optional fields are left as NULL or default values
5. **AccAdr Simplicity:** Only contains the relationship keys, no additional fields

---

## 🚀 Testing

To test the updated endpoint:

```bash
curl -X POST "http://localhost:7060/api/ssms/v0.1/Customer" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "adrNm": "Test Customer",
    "fstNm": "Test",
    "lstNm": "Customer",
    "tp1": "0771234567",
    "eMail": "test@example.com"
  }'
```

Check the database:
```sql
-- Verify Address created
SELECT * FROM Address WHERE AdrNm = 'Test Customer'

-- Verify Account created
SELECT * FROM AccMas WHERE AccNm = 'Test Customer'

-- Verify relationship created
SELECT aa.*, ad.AdrNm, ac.AccCd 
FROM AccAdr aa
JOIN Address ad ON aa.AdrKy = ad.AdrKy
JOIN AccMas ac ON aa.AccKy = ac.AccKy
WHERE ad.AdrNm = 'Test Customer'
```

---

## ⚠️ Breaking Changes

**None** - The API interface remains the same. Only the internal database operations changed from `AddressCdRel` to `AccMas` + `AccAdr`.

---

**Updated By:** Antigravity  
**Date:** 2026-02-09  
**Build Status:** ✅ Success (142 warnings - pre-existing)
