using Application.DTOs;
using Application.DTOs.ItemBatch;
using Application.DTOs.Items;
using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.Reports;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Services
{
    public class ItemService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IValidationService _validator;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;
        private readonly CommonLookupService _lookup;

        public ItemService(
            IDynamicDbContextFactory factory, 
            IValidationService validator, 
            IUserRequestContext userContext,
            IUserKeyService userKeyService,
            CommonLookupService lookup)
        {
            _factory = factory;
            _validator = validator;
            _userContext = userContext;
            _userKeyService = userKeyService;
            _lookup = lookup;
        }

        //--------------------------------------------------
        // GET ITEMS DETAILS
        //--------------------------------------------------
        public async Task<List<Item>> GetAllItemsAsync()
        {
            using var db = await _factory.CreateDbContextAsync();
            return await db.Items.ToListAsync();
        }

        public async Task<List<ItemsWithoutFInActDTO>> GetItemsWithoutFInActAsync()
        {
            using var db = await _factory.CreateDbContextAsync();

            var data = await db.Set<vewItmMasVsf>()
                .AsNoTracking()
                .Where(x => x.fInAct == false)
                .OrderBy(x => x.ItmNm)
                .Select(x => new ItemsWithoutFInActDTO
                {
                    CKy = _userContext.CompanyKey,     // if not in view, use context
                    ItmKy = x.ItmKy,
                    ItmTypKy = x.ItmTypKy,
                    BseItmCd = null,                  // not available in vewItmMasVsf
                    ItmCd = x.ItmCd,
                    PartNo = x.PartNo,
                    ItmNm = x.ItmNm ?? "",
                    ItmTypCd = null,                  // not available in vewItmMasVsf
                    ItmTyp = null,                    // not available in vewItmMasVsf
                    //ItmCat1Ky = x.ItmCat1Ky,
                    //ItmCat2Ky = x.ItmCat2Ky,
                    //ItmCat3Ky = x.ItmCat3Ky,
                    ItmCat4Ky = null,                 // not in this view
                    CosPri = x.CosPri,
                    SlsPri = x.SlsPri,
                    SlsPri2 = x.SlsPri2,
                    UnitKy = null,                    // not in view
                    Unit = x.Unit ?? "",
                    ItmPrp1Ky = null,
                    ItmPrp2Ky = null,
                    BUKy = x.BUKy,
                    Wrnty = x.Wrnty,
                    fSrlNo = x.fSrlNo,
                    ItmRem = x.ItmRem,
                    //PrftMrgn = x.PrftMrgn,
                    DisAmt = null,
                    //DisPer = x.DisPer,
                    ResrvQty = null
                })
                .ToListAsync();

            return data;
        }



        //--------------------------------------------------
        // ADD NEW ITEM
        //--------------------------------------------------
        public async Task<(bool success, string message, int statusCode)> AddItemAsync(AddItemDTO dto)
        {
            short locKy = 276; // fixed location as per requirement

            var userKey = await _userKeyService.GetUserKeyAsync(
                _userContext.UserId,
                _userContext.CompanyKey
            );

            if (userKey == null)
                return (false, "User key not found", 400);

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();

            try
            {
                // validations
                if (await _validator.IsExistItemCode(dto.itemCode))
                    return (false, "Item Code already exists", 409);

                if (!await _validator.IsExistItemType(dto.itemType))
                    return (false, "Invalid Item Type", 400);

                if (!await _validator.IsValidUnitKey(dto.unitKey))
                    return (false, "Invalid Unit Key", 400);

                // get item type key
                short itemTypeKy = await _lookup.GetItemTypeKeyAsync(dto.itemType);
                if (itemTypeKy == 0)
                    return (false, "Item type key not found", 400);

                if (db.Database.IsRelational())
                {
                    await conn.OpenAsync();
                    using var tx = conn.BeginTransaction();

                    // 1️⃣ Insert into ItmMas (NO OUTPUT)
                    using var cmdInsert = conn.CreateCommand();
                    cmdInsert.Transaction = tx;
                    cmdInsert.CommandText = @"
                INSERT INTO ItmMas
                (
                    CKy, ItmCd, ItmTypKy, ItmTyp, PartNo, ItmNm, Des, LocKy,
                    ItmCat1Ky, ItmCat2Ky, ItmCat3Ky, ItmCat4Ky, UnitKy,
                    CosPri, SlsPri, Qty,
                    EntUsrKy, EntDtm
                )
                VALUES
                (
                    @CKy, @ItmCd, @ItmTypKy, @ItmTyp, @PartNo, @ItmNm, @Des, @LocKy,
                    @ItmCat1Ky, @ItmCat2Ky, @ItmCat3Ky, @ItmCat4Ky, @UnitKy,
                    @CosPri, @SlsPri, @Qty,
                    @EntUsrKy, GETDATE()
                );
            ";

                    cmdInsert.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
                    cmdInsert.Parameters.Add(new SqlParameter("@ItmCd", dto.itemCode));
                    cmdInsert.Parameters.Add(new SqlParameter("@ItmTypKy", itemTypeKy));
                    cmdInsert.Parameters.Add(new SqlParameter("@ItmTyp", dto.itemType));
                    cmdInsert.Parameters.Add(new SqlParameter("@PartNo", (object?)dto.partNo ?? DBNull.Value));
                    cmdInsert.Parameters.Add(new SqlParameter("@ItmNm", dto.itemName));
                    cmdInsert.Parameters.Add(new SqlParameter("@Des", (object?)dto.description ?? DBNull.Value));
                    cmdInsert.Parameters.Add(new SqlParameter("@LocKy", locKy));
                    cmdInsert.Parameters.Add(new SqlParameter("@ItmCat1Ky", (object?)dto.itmCat1Ky ?? DBNull.Value));
                    cmdInsert.Parameters.Add(new SqlParameter("@ItmCat2Ky", (object?)dto.itmCat2Ky ?? DBNull.Value));
                    cmdInsert.Parameters.Add(new SqlParameter("@ItmCat3Ky", (object?)dto.itmCat3Ky ?? DBNull.Value));
                    cmdInsert.Parameters.Add(new SqlParameter("@ItmCat4Ky", (object?)dto.itmCat4Ky ?? DBNull.Value));
                    cmdInsert.Parameters.Add(new SqlParameter("@UnitKy", dto.unitKey));
                    cmdInsert.Parameters.Add(new SqlParameter("@CosPri", dto.costPrice));
                    cmdInsert.Parameters.Add(new SqlParameter("@SlsPri", dto.salesPrice));
                    cmdInsert.Parameters.Add(new SqlParameter("@Qty", (object?)dto.quantity ?? DBNull.Value));
                    cmdInsert.Parameters.Add(new SqlParameter("@EntUsrKy", userKey));

                    await cmdInsert.ExecuteNonQueryAsync();

                    // 2️⃣ Read back ItmKy safely
                    using var cmdGetKey = conn.CreateCommand();
                    cmdGetKey.Transaction = tx;
                    cmdGetKey.CommandText = @"
                SELECT ItmKy
                FROM ItmMas
                WHERE ItmCd = @ItmCd
                  AND CKy = @CKy
                  AND fInAct = 0;
            ";

                    cmdGetKey.Parameters.Add(new SqlParameter("@ItmCd", dto.itemCode));
                    cmdGetKey.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));

                    var itmKyObj = await cmdGetKey.ExecuteScalarAsync();
                    if (itmKyObj == null)
                        throw new Exception("Failed to retrieve item key after insert");

                    int itmKy = Convert.ToInt32(itmKyObj);

                    await tx.CommitAsync();

                    return (true, $"Item added successfully. ItemKey = {itmKy}", 201);
                }
                else
                {
                    // In-Memory Fallback
                    var maxKey = await db.ItmMas.AnyAsync() ? await db.ItmMas.MaxAsync(x => x.ItmKy) : 0;
                    var newItem = new ItmMas
                    {
                        ItmKy = maxKey + 1,
                        CKy = _userContext.CompanyKey,
                        ItmCd = dto.itemCode,
                        ItmTypKy = itemTypeKy,
                        ItmTyp = dto.itemType,
                        PartNo = dto.partNo,
                        ItmNm = dto.itemName,
                        Des = dto.description,
                        ItmCat1Ky = dto.itmCat1Ky ?? 0,
                        ItmCat2Ky = dto.itmCat2Ky ?? 0,
                        ItmCat3Ky = dto.itmCat3Ky ?? 0,
                        ItmCat4Ky = dto.itmCat4Ky ?? 0,
                        UnitKy = dto.unitKey,
                        CosPri = dto.costPrice,
                        SlsPri = dto.salesPrice,
                        Qty = dto.quantity ?? 0,
                        EntUsrKy = userKey.Value,
                        EntDtm = DateTime.Now,
                        // Initialize required properties with defaults
                        LocKy = locKy,
                        fInAct = false,
                        fApr = true,
                        fObs = false,
                        ItmPriCatKy = 0,
                        ItmPrp1Ky = 0,
                        ItmPrp2Ky = 0,
                        Wrnty = 0,
                        fCtrlItm = false,
                        fSrlNo = false,
                        fChkStk = false,
                        BUKy = 0,
                        DisPer = 0,
                        SlsPri2 = 0,
                        DisAmt = 0,
                        Rac1Ky = 0,
                        Rac2Ky = 0,
                        SupAdrKy = 0,
                        PrftMrgn = 0
                    };
                    
                    db.ItmMas.Add(newItem);
                    await db.SaveChangesAsync();
                    
                    return (true, $"Item added successfully. ItemKey = {newItem.ItmKy}", 201);
                }
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 500);
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }



        //--------------------------------------------------
        // UPDATE EXISTING ITEM
        //--------------------------------------------------
        public async Task<(bool success, string message, int statusCode)> UpdateItemAsync(UpdateItemDTO dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            var conn = db.Database.GetDbConnection();

            if (dto.itemKey <= 0)
                return (false, "Invalid Item Key", 400);

            try
            {
                await conn.OpenAsync();

                // -----------------------------------------
                // 1. Duplicate ItemCode check (only if provided)
                // -----------------------------------------
                if (!string.IsNullOrWhiteSpace(dto.itemCode))
                {
                    using var checkCmd = conn.CreateCommand();
                    checkCmd.CommandText = @"
                                SELECT 1
                                FROM ItmMas
                                WHERE ItmCd = @ItmCd
                                  AND ItmKy <> @ItmKy
                                  AND fInAct = 0;
                            ";

                    checkCmd.Parameters.Add(new SqlParameter("@ItmCd", dto.itemCode));
                    checkCmd.Parameters.Add(new SqlParameter("@ItmKy", dto.itemKey));

                    var exists = await checkCmd.ExecuteScalarAsync();
                    if (exists != null)
                        return (false, "Item Code already exists", 409);
                }

                // -----------------------------------------
                // 2. Update
                // -----------------------------------------
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                            UPDATE ItmMas
                            SET
                                ItmCd     = ISNULL(@ItmCd, ItmCd),
                                ItmTyp    = ISNULL(@ItmTyp, ItmTyp),
                                PartNo    = @PartNo,
                                ItmNm     = @ItmNm,
                                Des       = @Des,
                                ItmCat1Ky = @ItmCat1Ky,
                                ItmCat2Ky = @ItmCat2Ky,
                                ItmCat3Ky = @ItmCat3Ky,
                                ItmCat4Ky = @ItmCat4Ky,
                                UnitKy    = @UnitKy,
                                CosPri    = @CosPri,
                                SlsPri    = @SlsPri,
                                Qty       = @Qty
                            WHERE ItmKy = @ItmKy;
                        ";

                cmd.Parameters.Add(new SqlParameter("@ItmKy", dto.itemKey));

                cmd.Parameters.Add(new SqlParameter("@ItmCd",
                    string.IsNullOrWhiteSpace(dto.itemCode) ? (object)DBNull.Value : dto.itemCode));

                cmd.Parameters.Add(new SqlParameter("@ItmTyp", dto.itemType ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@PartNo", dto.partNo ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@ItmNm", dto.itemName ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Des", dto.description ?? (object)DBNull.Value));

                cmd.Parameters.Add(new SqlParameter("@ItmCat1Ky", dto.itmCat1Ky ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@ItmCat2Ky", dto.itmCat2Ky ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@ItmCat3Ky", dto.itmCat3Ky ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@ItmCat4Ky", dto.itmCat4Ky ?? (object)DBNull.Value));

                cmd.Parameters.Add(new SqlParameter("@UnitKy", dto.unitKey ?? (object)DBNull.Value));
                //cmd.Parameters.Add(new SqlParameter("@DisPer", dto.discountPrecentage ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@CosPri", dto.costPrice ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@SlsPri", dto.salesPrice ?? (object)DBNull.Value));
                //cmd.Parameters.Add(new SqlParameter("@DisAmt", dto.discountAmount ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Qty", dto.quantity ?? (object)DBNull.Value));
                //cmd.Parameters.Add(new SqlParameter("@fInAct", dto.fInAct ?? false));

                int affected = await cmd.ExecuteNonQueryAsync();

                if (affected == 0)
                    return (false, "Item not found or not updated", 404);

                return (true, "Item updated successfully", 200);
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                return (false, "Item Code already exists", 409);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 500);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }


        //--------------------------------------------------
        // DELETE EXISTING ITEM
        //--------------------------------------------------
        public async Task<(bool success, string message, int statusCode)> DeleteItemAsync(int itemKey)
        {
            if (itemKey <= 0)
                return (false, "Invalid Item Key", 400);

            using var db = await _factory.CreateDbContextAsync();
            var conn = db.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
            UPDATE ItmMas
            SET fInAct = 1
            WHERE ItmKy = @ItmKy;
        ";

                cmd.Parameters.Add(new SqlParameter("@ItmKy", itemKey));

                int affected = await cmd.ExecuteNonQueryAsync();

                if (affected == 0)
                    return (false, "Item not found", 404);

                return (true, "Item deleted successfully", 200);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 500);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }


        //--------------------------------------------------
        // GET ITEM BATCH DETAILS
        //--------------------------------------------------
        public async Task<List<vewItmBatch>> GetItemBatchesAsync(int itemKey)
        {
            try
            {
                using var db = await _factory.CreateDbContextAsync();

                //int itemKey = await db.Items
                //    .Where(x => x.ItmCd == itemCode)
                //    .Select(x => x.ItmKy)
                //    .FirstOrDefaultAsync();

                if (itemKey <= 0)
                    throw new Exception("Invalid Item Code");

                return await db.vewItmBatch
                    .Where(x => x.ItmKy == itemKey)
                    .OrderBy(x => x.ExpirDt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetItemBatchesAsync: {ex.Message}");
                return new List<vewItmBatch>();
            }
        }


        //--------------------------------------------------
        // ADD NEW ITEM BATCH
        //--------------------------------------------------
        public async Task<(bool success, string message, int statusCode, int? batchKey)> AddItemBatchAsync(AddItemBatchDTO dto)
        {
            if (dto.itemKey <= 0)
                return (false, "Invalid ItemKey", 400, null);

            using var db = await _factory.CreateDbContextAsync();
            var conn = db.Database.GetDbConnection();

            try
            {
                // ---------------------------------------------------------
                // 1. Check if Item exists
                // ---------------------------------------------------------
                bool itemExists = await db.Items.AnyAsync(x => x.ItmKy == dto.itemKey);

                if (!itemExists)
                    return (false, "Item not found. Cannot add batch.", 404, null);

                // ---------------------------------------------------------
                // 2. Insert batch
                // ---------------------------------------------------------
                if (db.Database.IsRelational())
                {
                    await conn.OpenAsync();

                    using var cmd = conn.CreateCommand();

                    cmd.CommandText = @"
                            INSERT INTO ItmBatch
                            (
                                ItmKy, BatchNo, ExpirDt, CosPri, SalePri, Qty
                            )
                            OUTPUT INSERTED.ItmBatchKy
                            VALUES
                            (
                                @ItmKy, @BatchNo, @ExpirDt, @CosPri, @SalePri, @Qty
                            );
                        ";

                    cmd.Parameters.Add(new SqlParameter("@ItmKy", dto.itemKey));
                    cmd.Parameters.Add(new SqlParameter("@BatchNo", dto.batchNo ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@ExpirDt", dto.expirDt ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@CosPri", dto.costPrice ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@SalePri", dto.salePrice ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@Qty", dto.quantity ?? (object)DBNull.Value));

                    var newBatchKyObj = await cmd.ExecuteScalarAsync();
                    int newBatchKey = Convert.ToInt32(newBatchKyObj);

                    return (true, "Item batch added successfully", 201, newBatchKey);
                }
                else
                {
                    // Fallback for InMemory Provider
                    var maxKey = await db.ItmBatch.AnyAsync() ? await db.ItmBatch.MaxAsync(x => x.ItmBatchKy) : 0;
                    var newBatch = new ItmBatch
                    {
                        ItmBatchKy = maxKey + 1,
                        ItmKy = dto.itemKey,
                        BatchNo = dto.batchNo,
                        ExpirDt = dto.expirDt,
                        CosPri = dto.costPrice ?? 0,
                        SalePri = dto.salePrice ?? 0,
                        Qty = dto.quantity ?? 0
                    };

                    db.ItmBatch.Add(newBatch);
                    await db.SaveChangesAsync();

                    return (true, "Item batch added successfully", 201, newBatch.ItmBatchKy);
                }
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 500, null);
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }


        //--------------------------------------------------
        // UPDATE EXISTING ITEM BATCH
        //--------------------------------------------------
        public async Task<(bool success, string message, int statusCode)> UpdateItemBatchAsync(UpdateItemBatchDTO dto)
        {
            if (dto.itemBatchKey <= 0)
                return (false, "Invalid ItemBatchKey", 400);

            if (dto.itemKey <= 0)
                return (false, "Invalid ItemKey", 400);

            using var db = await _factory.CreateDbContextAsync();
            var conn = db.Database.GetDbConnection();

            try
            {
                // -------------------------------------
                // 1. Check ItmBatch exists
                // -------------------------------------
                bool batchExists = await db.ItmBatch.AnyAsync(x => x.ItmBatchKy == dto.itemBatchKey);

                if (!batchExists)
                    return (false, "Item batch not found", 404);

                // -------------------------------------
                // 2. Check Item exists
                // -------------------------------------
                bool itemExists = await db.Items.AnyAsync(x => x.ItmKy == dto.itemKey);

                if (!itemExists)
                    return (false, "Item not found", 404);

                // -------------------------------------
                // 3. Perform update
                // -------------------------------------
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                            UPDATE ItmBatch
                            SET
                                ItmKy = @ItmKy,
                                BatchNo = @BatchNo,
                                ExpirDt = @ExpirDt,
                                CosPri = @CosPri,
                                SalePri = @SalePri,
                                Qty = @Qty
                            WHERE ItmBatchKy = @ItmBatchKy;
                        ";

                cmd.Parameters.Add(new SqlParameter("@ItmBatchKy", dto.itemBatchKey));
                cmd.Parameters.Add(new SqlParameter("@ItmKy", dto.itemKey));
                cmd.Parameters.Add(new SqlParameter("@BatchNo", dto.batchNo ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@ExpirDt", dto.expirDt ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@CosPri", dto.costPrice ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@SalePri", dto.salePrice ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Qty", dto.quantity ?? (object)DBNull.Value));

                int affected = await cmd.ExecuteNonQueryAsync();

                if (affected == 0)
                    return (false, "Update failed or no changes applied", 400);

                return (true, "Item batch updated successfully", 200);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 500);
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }
    }
}
