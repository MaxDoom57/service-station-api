using Application.DTOs;
using Application.DTOs.Stock_Addition;
using Application.DTOs.StockDeduction;
using Application.Interfaces;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Services
{
    public class StockService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;
        private readonly IValidationService _validator;

        public StockService(
            IDynamicDbContextFactory factory, 
            IUserRequestContext userContext, 
            IUserKeyService userKeyService, 
            IValidationService validator)
        {
            _factory = factory;
            _userContext = userContext;
            _userKeyService = userKeyService;
            _validator = validator;
        }

        public async Task<bool> IsValidTranDate(string tranType, DateTime trnDt)
        {
            using var db = await _factory.CreateDbContextAsync();
            var conn = db.Database.GetDbConnection();

            await conn.OpenAsync();

            try
            {
                // ---------------------------------------------
                // 1. Server date
                // ---------------------------------------------
                DateTime svrDt = DateTime.Today;

                // ---------------------------------------------
                // 2. Read backward and forward days from CdMas
                // ---------------------------------------------
                int backwardDays = 0;
                int forwardDays = 0;

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                SELECT CdNo4, CdNo5
                FROM CdMas
                WHERE ConCd = 'TrnTyp'
                  AND OurCd = @TranType
                  AND fInAct = 0
            ";

                    cmd.Parameters.Add(new SqlParameter("@TranType", tranType));

                    using var rdr = await cmd.ExecuteReaderAsync();
                    if (await rdr.ReadAsync())
                    {
                        backwardDays = rdr.IsDBNull(0) ? 0 : Convert.ToInt32(rdr[0]);
                        forwardDays = rdr.IsDBNull(1) ? 0 : Convert.ToInt32(rdr[1]);
                    }
                }

                // ---------------------------------------------
                // 3. Backward date validation
                // ---------------------------------------------
                if ((svrDt - trnDt.Date).TotalDays > backwardDays)
                    return false;

                // ---------------------------------------------
                // 4. Forward date validation
                // ---------------------------------------------
                if ((trnDt.Date - svrDt).TotalDays > forwardDays)
                    return false;

                return true;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }


        // ============================================================
        // GET: Get STOCK ADDITION DETAILS BY TRANSACTION NUMBER
        // ============================================================
        public async Task<(bool success, string message, int statusCode, StkAddFullResponseDTO data)>GetStockAdditionByTrnNoAsync(int trnNo)
        {
            try
            {
                using var db = await _factory.CreateDbContextAsync();
                using var conn = db.Database.GetDbConnection();
                await conn.OpenAsync();

                // -------------------------------
                // HEADER QUERY
                // -------------------------------
                var cmdHdr = conn.CreateCommand();
                cmdHdr.CommandText = @"
                    SELECT LocKy, TrnDt, Des, TrnKy
                    FROM vewStkAddHdr
                    WHERE TrnNo = @TrnNo AND CKy = @CKy;
                ";

                cmdHdr.Parameters.Add(new SqlParameter("@TrnNo", trnNo));
                cmdHdr.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));

                StkAddHeaderDTO? header = null;

                using (var rdr = await cmdHdr.ExecuteReaderAsync())
                {
                    if (await rdr.ReadAsync())
                    {
                        header = new StkAddHeaderDTO
                        {
                            LocKy = rdr.GetInt16(0),
                            TrnDt = rdr.IsDBNull(1) ? null : rdr.GetDateTime(1),
                            Des = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                            TrnKy = rdr.GetInt32(3)
                        };
                    }
                }

                if (header == null)
                    return (false, "Stock addition not found", 404, null);

                // -------------------------------
                // DETAIL QUERY
                // -------------------------------
                var cmdDtl = conn.CreateCommand();
                cmdDtl.CommandText = @"
                    SELECT ItmKy, ItmCd, ItmNm, Unit, CosPri, SlsPri, TrnPri, Qty, ItmTrnKy
                    FROM vewStkAddDtls
                    WHERE TrnKy = @TrnKy;
                ";

                cmdDtl.Parameters.Add(new SqlParameter("@TrnKy", header.TrnKy));

                var details = new List<StkAddDetailDTO>();

                using (var rdr = await cmdDtl.ExecuteReaderAsync())
                {
                    while (await rdr.ReadAsync())
                    {
                        details.Add(new StkAddDetailDTO
                        {
                            ItmKy = rdr.GetInt32(0),
                            ItmCd = rdr.IsDBNull(1) ? null : rdr.GetString(1),
                            ItmNm = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                            Unit = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                            CosPri = rdr.IsDBNull(4) ? null : rdr.GetDecimal(4),
                            SlsPri = rdr.IsDBNull(5) ? null : rdr.GetDecimal(5),
                            TrnPri = rdr.IsDBNull(6) ? null : rdr.GetDecimal(6),
                            Qty = rdr.IsDBNull(7) ? null : rdr.GetDouble(7),
                            ItmTrnKy = rdr.GetInt32(8),
                            updt = 0,
                            del = 0
                        });
                    }
                }

                return (true, "Success", 200, new StkAddFullResponseDTO
                {
                    Header = header,
                    Details = details
                });
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 500, null);
            }
        }



        // ============================================================
        // POST: CREATE NEW STOCK ADDITION
        // ============================================================
        public async Task<(bool success, string message, int statusCode, int trnNo, int trnKy)>CreateStockAdditionAsync(StockAddPostDTO dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                int trnNo;
                int trnKy;
                var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
                if (userKey == null)
                    throw new Exception("User key not found");

                // 1) Get new transaction number
                var cmdGetNo = conn.CreateCommand();
                cmdGetNo.Transaction = tx;
                cmdGetNo.CommandText = @"
                    SELECT ISNULL(MAX(TrnNo), 0) + 1
                    FROM TrnMas
                    WHERE OurCd='STKADD' AND CKy=@CKy";

                cmdGetNo.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
                trnNo = Convert.ToInt32(await cmdGetNo.ExecuteScalarAsync());

                // 2) Insert into TrnMas
                var cmdInsert = conn.CreateCommand();
                cmdInsert.Transaction = tx;

                cmdInsert.CommandText = @"
                    INSERT INTO TrnMas
                    (CKy, TrnNo, OurCd, TrnTypKy, TrnDt, LocKy, fApr, Des, EntUsrKy, EntDtm)
                    VALUES
                    (@CKy, @TrnNo, 'STKADD', 1, @TrnDt, @LocKy, 1, @Des, @UsrKy, GETDATE());
                ";

                cmdInsert.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
                cmdInsert.Parameters.Add(new SqlParameter("@TrnNo", trnNo));
                cmdInsert.Parameters.Add(new SqlParameter("@TrnDt", dto.trnDate));
                cmdInsert.Parameters.Add(new SqlParameter("@LocKy", dto.locKey));
                cmdInsert.Parameters.Add(new SqlParameter("@Des", dto.description ?? (object)DBNull.Value));
                cmdInsert.Parameters.Add(new SqlParameter("@UsrKy", userKey));

                await cmdInsert.ExecuteNonQueryAsync();

                // 3) Get TrnKy
                var cmdGetTrnKy = conn.CreateCommand();
                cmdGetTrnKy.Transaction = tx;
                cmdGetTrnKy.CommandText = @"
                    SELECT TrnKy FROM vewTrnNo
                    WHERE CKy=@CKy AND OurCd='STKADD' AND TrnNo=@TrnNo";

                cmdGetTrnKy.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
                cmdGetTrnKy.Parameters.Add(new SqlParameter("@TrnNo", trnNo));

                trnKy = Convert.ToInt32(await cmdGetTrnKy.ExecuteScalarAsync());

                // 4) Insert detail rows
                int lineNo = 1;
                foreach (var item in dto.items)
                {
                    var cmdInsertItem = conn.CreateCommand();
                    cmdInsertItem.Transaction = tx;

                    cmdInsertItem.CommandText = @"
                        INSERT INTO ItmTrn
                        (TrnKy, ItmKy, Qty, CosPri, SlsPri, TrnPri, LocKy, LiNo)
                        VALUES
                        (@TrnKy, @ItmKy, @Qty, @CosPri, @SlsPri, @TrnPri, @LocKy, @LiNo);
                    ";

                    cmdInsertItem.Parameters.Add(new SqlParameter("@TrnKy", trnKy));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@ItmKy", item.itemKey));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@Qty", item.quantity));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@CosPri", item.costPrice));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@SlsPri", item.salePrice));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@TrnPri", item.salePrice));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@LocKy", dto.locKey));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@LiNo", lineNo++));

                    await cmdInsertItem.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();

                return (true, "Stock addition created", 201, trnNo, trnKy);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, ex.Message, 500, 0, 0);
            }
        }



        // ============================================================
        // PUT: UPDATE EXISTING STOCK ADDITION
        // ============================================================
        public async Task<(bool success, string message, int statusCode, int trnNo, int trnKy)>UpdateStockAdditionAsync(StockAddUpdateDTO dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                int trnKy;
                var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
                if (userKey == null)
                    throw new Exception("User key not found");

                // 1) Get TrnKy from trnNo
                var cmdGetKy = conn.CreateCommand();
                cmdGetKy.Transaction = tx;
                cmdGetKy.CommandText = @"
                    SELECT TrnKy 
                    FROM vewTrnNo 
                    WHERE CKy=@CKy AND OurCd='STKADD' AND TrnNo=@TrnNo";

                cmdGetKy.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
                cmdGetKy.Parameters.Add(new SqlParameter("@TrnNo", dto.trnNo));

                trnKy = Convert.ToInt32(await cmdGetKy.ExecuteScalarAsync());

                // 2) Update TrnMas
                var cmdUpdate = conn.CreateCommand();
                cmdUpdate.Transaction = tx;

                cmdUpdate.CommandText = @"
                    UPDATE TrnMas
                    SET TrnDt=@TrnDt, LocKy=@LocKy, Des=@Des, 
                        EntUsrKy=@UsrKy, EntDtm=GETDATE(), Status='U'
                    WHERE TrnKy=@TrnKy";

                cmdUpdate.Parameters.Add(new SqlParameter("@TrnDt", dto.trnDate));
                cmdUpdate.Parameters.Add(new SqlParameter("@LocKy", dto.locKey));
                cmdUpdate.Parameters.Add(new SqlParameter("@Des", dto.description ?? (object)DBNull.Value));
                cmdUpdate.Parameters.Add(new SqlParameter("@UsrKy", userKey));
                cmdUpdate.Parameters.Add(new SqlParameter("@TrnKy", trnKy));

                await cmdUpdate.ExecuteNonQueryAsync();

                // 3) Delete existing detail rows
                var cmdDelete = conn.CreateCommand();
                cmdDelete.Transaction = tx;
                cmdDelete.CommandText = "DELETE FROM ItmTrn WHERE TrnKy=@TrnKy";
                cmdDelete.Parameters.Add(new SqlParameter("@TrnKy", trnKy));
                await cmdDelete.ExecuteNonQueryAsync();

                // 4) Insert new rows
                int lineNo = 1;
                foreach (var item in dto.items)
                {
                    var cmdInsertItem = conn.CreateCommand();
                    cmdInsertItem.Transaction = tx;

                    cmdInsertItem.CommandText = @"
                        INSERT INTO ItmTrn
                        (TrnKy, ItmKy, Qty, CosPri, SlsPri, TrnPri, LocKy, LiNo)
                        VALUES
                        (@TrnKy, @ItmKy, @Qty, @CosPri, @SlsPri, @TrnPri, @LocKy, @LiNo);";

                    cmdInsertItem.Parameters.Add(new SqlParameter("@TrnKy", trnKy));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@ItmKy", item.itemKey));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@Qty", item.quantity));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@CosPri", item.costPrice));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@SlsPri", item.salePrice));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@TrnPri", item.salePrice));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@LocKy", dto.locKey));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@LiNo", lineNo++));

                    await cmdInsertItem.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();

                return (true, "Stock addition updated", 200, dto.trnNo, trnKy);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, ex.Message, 500, 0, 0);
            }
        }



        // ============================================================
        // DELETE: DELETE STOCK ADDITION
        // ============================================================
        public async Task<(bool success, string message, int statusCode)> DeleteStockAdditionAsync(int trnNo)
        {
            if (trnNo <= 0)
                return (false, "Invalid Transaction Number", 400);

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                // --------------------------------------------------
                // 1. Get TrnKy from vewTrnNo
                // --------------------------------------------------
                int trnKy;

                using (var cmdGet = conn.CreateCommand())
                {
                    cmdGet.Transaction = tx;
                    cmdGet.CommandText = @"
                SELECT TrnKy
                FROM vewTrnNo
                WHERE OurCd = 'STKADD'
                  AND TrnNo = @TrnNo
                  AND CKy   = @CKy
            ";

                    cmdGet.Parameters.Add(new SqlParameter("@TrnNo", trnNo));
                    cmdGet.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));

                    var trnKyObj = await cmdGet.ExecuteScalarAsync();

                    if (trnKyObj == null)
                        return (false, "Invalid Transaction Number", 404);

                    trnKy = Convert.ToInt32(trnKyObj);
                }

                // --------------------------------------------------
                // 2. Get TrnDt from TrnMas
                // --------------------------------------------------
                DateTime trnDate;

                using (var cmdDate = conn.CreateCommand())
                {
                    cmdDate.Transaction = tx;
                    cmdDate.CommandText = @"
                SELECT TrnDt
                FROM TrnMas
                WHERE TrnKy = @TrnKy
                  AND fInAct = 0
            ";

                    cmdDate.Parameters.Add(new SqlParameter("@TrnKy", trnKy));

                    var trnDateObj = await cmdDate.ExecuteScalarAsync();

                    if (trnDateObj == null)
                        return (false, "Transaction not found or already deleted", 404);

                    trnDate = (DateTime)trnDateObj;
                }

                // --------------------------------------------------
                // 3. Validate transaction date
                // --------------------------------------------------
                if (!await IsValidTranDate("STKADD", trnDate))
                    return (false, "You cannot delete transactions for this date", 400);

                // --------------------------------------------------
                // 4. Delete ItmTrn rows
                // --------------------------------------------------
                using (var cmdDeleteDetails = conn.CreateCommand())
                {
                    cmdDeleteDetails.Transaction = tx;
                    cmdDeleteDetails.CommandText =
                        "DELETE FROM ItmTrn WHERE TrnKy = @TrnKy";

                    cmdDeleteDetails.Parameters.Add(new SqlParameter("@TrnKy", trnKy));
                    await cmdDeleteDetails.ExecuteNonQueryAsync();
                }

                // --------------------------------------------------
                // 5. Soft delete TrnMas
                // --------------------------------------------------
                using (var cmdInact = conn.CreateCommand())
                {
                    cmdInact.Transaction = tx;
                    cmdInact.CommandText =
                        "UPDATE TrnMas SET fInAct = 1 WHERE TrnKy = @TrnKy";

                    cmdInact.Parameters.Add(new SqlParameter("@TrnKy", trnKy));
                    await cmdInact.ExecuteNonQueryAsync();
                }

                // --------------------------------------------------
                // 6. Commit
                // --------------------------------------------------
                await tx.CommitAsync();

                return (true, "Stock addition deleted successfully", 200);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, "Error: " + ex.Message, 500);
            }
        }


        //=============================================================
        // STOCK DEDUCTION
        //=============================================================

        //=============================================================
        // GET:GET STOCK DEDUCTION DETAILS
        //=============================================================
        public async Task<(bool success, string message, int statusCode, StkAddFullResponseDTO data)>GetStockDeductionByTrnNoAsync(int trnNo)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            try
            {
                // ---------------------------------------------------------
                // 1. Get TrnKy (same as VB6)
                // ---------------------------------------------------------
                var cmdGetTrnKy = conn.CreateCommand();
                cmdGetTrnKy.CommandText = @"
                            SELECT TrnKy 
                            FROM vewTrnNo 
                            WHERE OurCd='STKDED' AND TrnNo=@TrnNo AND CKy=@CKy";

                cmdGetTrnKy.Parameters.Add(new SqlParameter("@TrnNo", trnNo));
                cmdGetTrnKy.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));

                object? trnKyObj = await cmdGetTrnKy.ExecuteScalarAsync();

                if (trnKyObj == null)
                    return (false, "Invalid transaction number", 404, null);

                int trnKy = Convert.ToInt32(trnKyObj);

                // ---------------------------------------------------------
                // 2. Get Header
                // ---------------------------------------------------------
                var cmdHdr = conn.CreateCommand();
                cmdHdr.CommandText = @"
                            SELECT TrnDt, Des, LocKy, TrnKy 
                            FROM vewStkDedHdr 
                            WHERE TrnNo=@TrnNo AND CKy=@CKy;
                        ";

                cmdHdr.Parameters.Add(new SqlParameter("@TrnNo", trnNo));
                cmdHdr.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));

                StkAddHeaderDTO? header = null;

                using (var rdr = await cmdHdr.ExecuteReaderAsync())
                {
                    if (await rdr.ReadAsync())
                    {
                        header = new StkAddHeaderDTO
                        {
                            TrnDt = rdr.IsDBNull(0) ? null : rdr.GetDateTime(0),
                            Des = rdr.IsDBNull(1) ? null : rdr.GetString(1),
                            LocKy = rdr.GetInt16(2),
                            TrnKy = rdr.GetInt32(3)
                        };
                    }
                    else
                    {
                        return (false, "Invalid transaction number", 404, null);
                    }
                }

                // ---------------------------------------------------------
                // 3. Get Details (Qty must be NEGATIVE like VB6)
                // ---------------------------------------------------------
                var cmdDtl = conn.CreateCommand();
                cmdDtl.CommandText = @"
                            SELECT 
                                ItmKy, ItmCd, ItmNm, Unit, CosPri, SlsPri, TrnPri, Qty, ItmTrnKy
                            FROM vewStkDedDtls
                            WHERE TrnKy = @TrnKy;
                        ";

                cmdDtl.Parameters.Add(new SqlParameter("@TrnKy", trnKy));

                var details = new List<StkAddDetailDTO>();

                using (var rdr = await cmdDtl.ExecuteReaderAsync())
                {
                    while (await rdr.ReadAsync())
                    {
                        var qty = rdr.IsDBNull(7) ? 0 : rdr.GetDouble(7);

                        details.Add(new StkAddDetailDTO
                        {
                            ItmKy = rdr.GetInt32(0),
                            ItmCd = rdr.IsDBNull(1) ? null : rdr.GetString(1),
                            ItmNm = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                            Unit = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                            CosPri = rdr.IsDBNull(4) ? null : rdr.GetDecimal(4),
                            SlsPri = rdr.IsDBNull(5) ? null : rdr.GetDecimal(5),
                            TrnPri = rdr.IsDBNull(6) ? null : rdr.GetDecimal(6),

                            Qty = qty * -1,   // VB6 logic: Qty = -1 * Qty

                            ItmTrnKy = rdr.GetInt32(8),
                            updt = 0,
                            del = 0
                        });
                    }
                }

                return (true, "Success", 200, new StkAddFullResponseDTO
                {
                    Header = header,
                    Details = details
                });
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 500, null);
            }
        }



        //=============================================================
        // POST: CREATE NEW STOCK DEDUCTION
        //=============================================================
        public async Task<(bool success, string message, int statusCode, int trnNo, int trnKy)>CreateStockDeductionAsync(StockDeductionPostDTO dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                // Validate transaction date
                if (!await IsValidTranDate("STKADD", dto.trnDate))
                {
                    return (false, "You cannot enter or alter transactions for this date", 400, 0, 0);
                }

                int trnNo;
                int trnKy;
                int trnTrfLnkKy;
                var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
                if (userKey == null)
                    throw new Exception("User key not found");

                // -------------------------------------------------------
                // 1. Get new TrnNo (same as VB6 GetTrnNoLstSave)
                // -------------------------------------------------------
                var cmdGetTrnNo = conn.CreateCommand();
                cmdGetTrnNo.Transaction = tx;
                cmdGetTrnNo.CommandText = @"
                            SELECT ISNULL(MAX(TrnNo), 0) + 1 
                            FROM TrnMas 
                            WHERE CKy=@CKy AND OurCd='STKDED'";

                cmdGetTrnNo.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
                trnNo = Convert.ToInt32(await cmdGetTrnNo.ExecuteScalarAsync());

                // -------------------------------------------------------
                // 2. Generate TrnTrfLnkKy
                // -------------------------------------------------------
                var cmdGetTrfLnk = conn.CreateCommand();
                cmdGetTrfLnk.Transaction = tx;
                cmdGetTrfLnk.CommandText = @"SELECT ISNULL(MAX(TrnTrfLnkKy),0) + 1 FROM TrnMas";
                trnTrfLnkKy = Convert.ToInt32(await cmdGetTrfLnk.ExecuteScalarAsync());

                // -------------------------------------------------------
                // 3. Insert into TrnMas
                // -------------------------------------------------------
                var cmdInsertMas = conn.CreateCommand();
                cmdInsertMas.Transaction = tx;

                cmdInsertMas.CommandText = @"
                                INSERT INTO TrnMas
                                    (CKy, TrnNo, TrnTrfLnkKy, OurCd, TrnTypKy, TrnDt, LocKy, fApr, Des, EntUsrKy, EntDtm)
                                VALUES
                                    (@CKy, @TrnNo, @TrnTrfLnkKy, 'STKDED', 2,
                                     @TrnDt, @LocKy, 1, @Des, @UsrKy, GETDATE());
                            ";

                cmdInsertMas.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
                cmdInsertMas.Parameters.Add(new SqlParameter("@TrnNo", trnNo));
                cmdInsertMas.Parameters.Add(new SqlParameter("@TrnTrfLnkKy", trnTrfLnkKy));
                cmdInsertMas.Parameters.Add(new SqlParameter("@TrnDt", dto.trnDate));
                cmdInsertMas.Parameters.Add(new SqlParameter("@LocKy", dto.locKey));
                cmdInsertMas.Parameters.Add(new SqlParameter("@Des", dto.description ?? (object)DBNull.Value));
                cmdInsertMas.Parameters.Add(new SqlParameter("@UsrKy", userKey));

                await cmdInsertMas.ExecuteNonQueryAsync();

                // -------------------------------------------------------
                // 4. Read back TrnKy from vewTrnNo
                // -------------------------------------------------------
                var cmdGetTrnKy = conn.CreateCommand();
                cmdGetTrnKy.Transaction = tx;

                cmdGetTrnKy.CommandText = @"
                                SELECT TrnKy 
                                FROM vewTrnNo
                                WHERE CKy=@CKy AND OurCd='STKDED' AND TrnNo=@TrnNo";

                cmdGetTrnKy.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
                cmdGetTrnKy.Parameters.Add(new SqlParameter("@TrnNo", trnNo));

                trnKy = Convert.ToInt32(await cmdGetTrnKy.ExecuteScalarAsync());

                // -------------------------------------------------------
                // 5. Insert Item rows (Qty must be NEGATIVE)
                // -------------------------------------------------------
                int lineNo = 1;

                foreach (var item in dto.items)
                {
                    // Generate ItmTrnTrfLnkKy
                    var cmdGetItemTrf = conn.CreateCommand();
                    cmdGetItemTrf.Transaction = tx;
                    cmdGetItemTrf.CommandText = @"SELECT ISNULL(MAX(ItmTrnTrfLnkKy),0) + 1 FROM ItmTrn";
                    int itmTrf = Convert.ToInt32(await cmdGetItemTrf.ExecuteScalarAsync());

                    var cmdInsertItem = conn.CreateCommand();
                    cmdInsertItem.Transaction = tx;

                    cmdInsertItem.CommandText = @"
                                INSERT INTO ItmTrn
                                (TrnKy, ItmTrnTrfLnkKy, ItmKy, Qty, CosPri, SlsPri, TrnPri, LocKy, LiNo)
                                VALUES
                                (@TrnKy, @ItmTrf, @ItmKy, @Qty, @CosPri, @SlsPri, @TrnPri, @LocKy, @LiNo);
                            ";

                    cmdInsertItem.Parameters.Add(new SqlParameter("@TrnKy", trnKy));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@ItmTrf", itmTrf));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@ItmKy", item.itemKey));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@Qty", item.quantity * -1)); // Negative qty for STKDED
                    cmdInsertItem.Parameters.Add(new SqlParameter("@CosPri", item.costPrice));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@SlsPri", item.salePrice));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@TrnPri", item.salePrice));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@LocKy", dto.locKey));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@LiNo", lineNo++));

                    await cmdInsertItem.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();

                return (true, "Stock deduction created", 201, trnNo, trnKy);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, "Error: " + ex.Message, 500, 0, 0);
            }
        }



        //=============================================================
        // PUT: UPDATE EXISTING STOCK DEDUCTION
        //=============================================================
        public async Task<(bool success, string message, int statusCode, int trnNo, int trnKy)>UpdateStockDeductionAsync(StockDeductionUpdateDTO dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
                if (userKey == null)
                    throw new Exception("User key not found");

                // --------------------------------------
                // 1. Validate transaction date
                // --------------------------------------
                if (!await IsValidTranDate("STKDED", dto.trnDate))
                {
                    return (false, "You cannot enter or alter transactions for this date", 400, 0, 0);
                }

                // --------------------------------------
                // 2. Find existing TrnKy
                // --------------------------------------
                var cmdFind = conn.CreateCommand();
                cmdFind.Transaction = tx;

                cmdFind.CommandText = @"
                            SELECT TrnKy 
                            FROM TrnMas 
                            WHERE OurCd='STKDED' AND TrnNo=@TrnNo AND CKy=@CKy AND fInAct=0
                        ";

                cmdFind.Parameters.Add(new SqlParameter("@TrnNo", dto.trnNo));
                cmdFind.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));

                object? trnKyObj = await cmdFind.ExecuteScalarAsync();

                if (trnKyObj == null)
                    return (false, "Invalid transaction number", 404, 0, 0);

                int trnKy = Convert.ToInt32(trnKyObj);

                // --------------------------------------
                // 3. Update TrnMas
                // --------------------------------------
                var cmdUpdateMas = conn.CreateCommand();
                cmdUpdateMas.Transaction = tx;

                cmdUpdateMas.CommandText = @"
                            UPDATE TrnMas
                            SET 
                                TrnDt=@TrnDt,
                                LocKy=@LocKy,
                                Des=@Des,
                                EntUsrKy=@UsrKy,
                                EntDtm=GETDATE(),
                                Status='U'
                            WHERE TrnKy=@TrnKy;
                        ";

                cmdUpdateMas.Parameters.Add(new SqlParameter("@TrnDt", dto.trnDate));
                cmdUpdateMas.Parameters.Add(new SqlParameter("@LocKy", dto.locKey));
                cmdUpdateMas.Parameters.Add(new SqlParameter("@Des", dto.description ?? (object)DBNull.Value));
                cmdUpdateMas.Parameters.Add(new SqlParameter("@UsrKy", userKey));
                cmdUpdateMas.Parameters.Add(new SqlParameter("@TrnKy", trnKy));

                await cmdUpdateMas.ExecuteNonQueryAsync();

                // --------------------------------------
                // 4. Delete existing ItmTrn rows
                // --------------------------------------
                var cmdDelete = conn.CreateCommand();
                cmdDelete.Transaction = tx;

                cmdDelete.CommandText = "DELETE FROM ItmTrn WHERE TrnKy=@TrnKy";
                cmdDelete.Parameters.Add(new SqlParameter("@TrnKy", trnKy));

                await cmdDelete.ExecuteNonQueryAsync();

                // --------------------------------------
                // 5. Insert updated items (negative Qty)
                // --------------------------------------
                int lineNo = 1;

                foreach (var item in dto.items)
                {
                    // Get next ItmTrnTrfLnkKy
                    var cmdGetItemTrf = conn.CreateCommand();
                    cmdGetItemTrf.Transaction = tx;
                    cmdGetItemTrf.CommandText = @"SELECT ISNULL(MAX(ItmTrnTrfLnkKy),0) + 1 FROM ItmTrn";

                    int itmTrf = Convert.ToInt32(await cmdGetItemTrf.ExecuteScalarAsync());

                    var cmdInsertItem = conn.CreateCommand();
                    cmdInsertItem.Transaction = tx;

                    cmdInsertItem.CommandText = @"
                                INSERT INTO ItmTrn
                                (TrnKy, ItmTrnTrfLnkKy, ItmKy, Qty, CosPri, SlsPri, TrnPri, LocKy, LiNo)
                                VALUES
                                (@TrnKy, @ItmTrf, @ItmKy, @Qty, @CosPri, @SlsPri, @TrnPri, @LocKy, @LiNo);
                            ";

                    cmdInsertItem.Parameters.Add(new SqlParameter("@TrnKy", trnKy));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@ItmTrf", itmTrf));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@ItmKy", item.itemKey));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@Qty", item.quantity * -1));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@CosPri", item.costPrice));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@SlsPri", item.salePrice));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@TrnPri", item.salePrice));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@LocKy", dto.locKey));
                    cmdInsertItem.Parameters.Add(new SqlParameter("@LiNo", lineNo++));

                    await cmdInsertItem.ExecuteNonQueryAsync();
                }

                // --------------------------------------
                // 6. Commit
                // --------------------------------------
                await tx.CommitAsync();

                return (true, "Stock deduction updated successfully", 200, dto.trnNo, trnKy);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, "Error: " + ex.Message, 500, 0, 0);
            }
        }


        //=============================================================
        // DELETE: DELETE EXISTING STOCK DEDUCTION
        //=============================================================
        public async Task<(bool success, string message, int statusCode)> DeleteStockDeductionAsync(int trnNo)
        {
            if (trnNo <= 0)
                return (false, "Invalid transaction number", 400);

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                // ------------------------------------------------------
                // 1. Get TrnKy from vewTrnNo
                // ------------------------------------------------------
                int trnKy;

                using (var cmdGet = conn.CreateCommand())
                {
                    cmdGet.Transaction = tx;
                    cmdGet.CommandText = @"
                SELECT TrnKy
                FROM vewTrnNo
                WHERE OurCd = 'STKDED'
                  AND TrnNo = @TrnNo
                  AND CKy   = @CKy
            ";

                    cmdGet.Parameters.Add(new SqlParameter("@TrnNo", trnNo));
                    cmdGet.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));

                    var trnKyObj = await cmdGet.ExecuteScalarAsync();

                    if (trnKyObj == null)
                        return (false, "Invalid transaction number", 404);

                    trnKy = Convert.ToInt32(trnKyObj);
                }

                // ------------------------------------------------------
                // 2. Get TrnDt from TrnMas
                // ------------------------------------------------------
                DateTime trnDate;

                using (var cmdDate = conn.CreateCommand())
                {
                    cmdDate.Transaction = tx;
                    cmdDate.CommandText = @"
                SELECT TrnDt
                FROM TrnMas
                WHERE TrnKy = @TrnKy
                  AND fInAct = 0
            ";

                    cmdDate.Parameters.Add(new SqlParameter("@TrnKy", trnKy));

                    var trnDateObj = await cmdDate.ExecuteScalarAsync();

                    if (trnDateObj == null)
                        return (false, "Transaction not found or already deleted", 404);

                    trnDate = (DateTime)trnDateObj;
                }

                // ------------------------------------------------------
                // 3. Validate transaction date
                // ------------------------------------------------------
                if (!await IsValidTranDate("STKDED", trnDate))
                    return (false, "You cannot delete transactions for this date", 400);

                // ------------------------------------------------------
                // 4. Delete from ItmTrn
                // ------------------------------------------------------
                using (var cmdDeleteDetails = conn.CreateCommand())
                {
                    cmdDeleteDetails.Transaction = tx;
                    cmdDeleteDetails.CommandText =
                        "DELETE FROM ItmTrn WHERE TrnKy = @TrnKy";

                    cmdDeleteDetails.Parameters.Add(new SqlParameter("@TrnKy", trnKy));
                    await cmdDeleteDetails.ExecuteNonQueryAsync();
                }

                // ------------------------------------------------------
                // 5. Logical delete TrnMas
                // ------------------------------------------------------
                using (var cmdInact = conn.CreateCommand())
                {
                    cmdInact.Transaction = tx;
                    cmdInact.CommandText =
                        "UPDATE TrnMas SET fInAct = 1 WHERE TrnKy = @TrnKy";

                    cmdInact.Parameters.Add(new SqlParameter("@TrnKy", trnKy));
                    await cmdInact.ExecuteNonQueryAsync();
                }

                // ------------------------------------------------------
                // 6. Commit
                // ------------------------------------------------------
                await tx.CommitAsync();

                return (true, "Stock deduction deleted successfully", 200);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, "Error: " + ex.Message, 500);
            }
        }


    }
}
