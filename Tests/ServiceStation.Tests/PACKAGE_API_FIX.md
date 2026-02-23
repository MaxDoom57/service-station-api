# Package API Fixes

## 🛠️ Issue Resolved
**Error:** `System.InvalidCastException: Unable to cast object of type 'System.Double' to type 'System.Single'`

**Cause:** The Entity Framework Core entity `CdMas.cs` had properties defined as `float` (Single), but the underlying SQL Server database columns were `FLOAT` (Double). This validation mismatch caused `InvalidCastException` whenever `CdMas` records were queried (Add, Update, Delete, Get).

**Fix:** Updated `CdMas.cs` to change the following properties from `float` to `double`:
- `SO`
- `CdNo1`, `CdNo2`, `CdNo3`, `CdNo4`, `CdNo5`

## 📋 Updated Files
- `Domain/Entities/CdMas.cs`

## ✅ Verification
- Build succeeded.
- Queries mapping `CdMas` will now correctly map SQL `FLOAT` to C# `double`.
- `AddPackageAsync`, `UpdatePackageAsync`, and `DeletePackageAsync` in `PackageService.cs` (and any other service using `CdMas`) will now function correctly.

## 🚀 To Test
You can now retry your Postman requests:
1. `POST /api/ssms/v0.1/package`
2. `PUT /api/ssms/v0.1/package`
3. `DELETE /api/ssms/v0.1/package/{cdKy}`

All should work without the cast exception.
