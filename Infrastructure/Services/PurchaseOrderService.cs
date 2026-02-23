using Application.DTOs.PurchaseOrder;
using Application.Interfaces;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class PurchaseOrderService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IValidationService _validator;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;
        private readonly CommonLookupService _lookup;

        public PurchaseOrderService(
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


        // ------------------------------------------------
        // GET PURCHASE ORDER DETAILS
        // ------------------------------------------------
        public async Task<PurchaseOrderResponseDto?> GetPurchaseOrderAsync(int orderNo,int ordTypKy)
        {
            var cKy = _userContext.CompanyKey;

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            PurchaseOrderHeaderDto? header;

            // --------------------------------
            // Header Query (vewPOHdr)
            // --------------------------------
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT OrdKy, OrdNo, DocNo, OrdDt, Des, AccKy, OrdFrq, AdrKy
                    FROM vewPOHdr
                    WHERE OrdTypKy = @OrdTypKy
                      AND OrdNo = @OrdNo
                      AND CKy = @CKy
                ";

                cmd.Parameters.Add(new SqlParameter("@OrdTypKy", ordTypKy));
                cmd.Parameters.Add(new SqlParameter("@OrdNo", orderNo));
                cmd.Parameters.Add(new SqlParameter("@CKy", cKy));

                using var reader = await cmd.ExecuteReaderAsync();
                if (!reader.Read())
                    return null;

                header = new PurchaseOrderHeaderDto
                {
                    OrdKy = reader.GetInt32(0),
                    OrdNo = reader.GetInt32(1),
                    DocNo = reader.IsDBNull(2) ? null : reader.GetString(2),
                    OrdDt = reader.GetDateTime(3),
                    Des = reader.IsDBNull(4) ? null : reader.GetString(4),
                    AccKy = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                    OrdFrq = reader.IsDBNull(6) ? null : reader.GetString(6),
                    AdrKy = reader.IsDBNull(7) ? null : reader.GetInt32(7)
                };
            }

            // --------------------------------
            // Detail Query (vewPODtls)
            // --------------------------------
            var details = new List<PurchaseOrderDetailDto>();
            decimal taxAmount = 0;

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT ItmKy, ItmCd, ItmNm, OrdQty, Unit, CosPri,
                           CosPri * OrdQty AS Amount,
                           ReqDt, Des, OrdDetKy, Amt1
                    FROM vewPODtls
                    WHERE OrdNo = @OrdNo
                      AND OrdTypKy = @OrdTypKy
                      AND CKy = @CKy
                ";

                cmd.Parameters.Add(new SqlParameter("@OrdNo", orderNo));
                cmd.Parameters.Add(new SqlParameter("@OrdTypKy", ordTypKy));
                cmd.Parameters.Add(new SqlParameter("@CKy", cKy));

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var dto = new PurchaseOrderDetailDto
                    {
                        ItmKy = reader.GetInt32(0),
                        ItmCd = reader.GetString(1),
                        ItmNm = reader.GetString(2),
                        OrdQty = reader.GetDecimal(3),
                        Unit = reader.GetString(4),
                        CosPri = reader.GetDecimal(5),
                        Amount = reader.GetDecimal(6),
                        ReqDt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                        Des = reader.IsDBNull(8) ? null : reader.GetString(8),
                        OrdDetKy = reader.GetInt32(9),
                        Amt1 = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10)
                    };

                    taxAmount += dto.Amt1;
                    details.Add(dto);
                }
            }

            return new PurchaseOrderResponseDto
            {
                Header = header!,
                Details = details,
                TaxAmount = Math.Round(taxAmount, 2)
            };
        }

        // ------------------------------------------------
        // POST - Create New Purchase Order
        // ------------------------------------------------
        public async Task<int> CreatePurchaseOrderAsync(PurchaseOrderSaveDto dto)
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
                var cKy = _userContext.CompanyKey;

                // 1. Get new Order Number
                var cmdOrdNo = conn.CreateCommand();
                cmdOrdNo.Transaction = tx;
                cmdOrdNo.CommandText = "SELECT dbo.GetOrdNoLstSave(1,0,@OrdDt,'PURORD')";
                cmdOrdNo.Parameters.Add(new SqlParameter("@OrdDt", dto.OrdDt));
                var ordNo = Convert.ToInt32(await cmdOrdNo.ExecuteScalarAsync());

                // 2. Insert OrdMas
                var cmdHdr = conn.CreateCommand();
                cmdHdr.Transaction = tx;
                cmdHdr.CommandText = @"
                    INSERT INTO OrdMas
                    (OrdNo,OrdDt,AdrKy,AccKy,DocNo,Des,OrdTypKy,OrdFrqKy,OrdTyp,EntUsrKy,EntDtm)
                    VALUES
                    (@OrdNo,@OrdDt,@AdrKy,@AccKy,@DocNo,@Des,@OrdTypKy,@OrdFrqKy,'PURORD',@UsrKy,GETDATE())
                ";

                cmdHdr.Parameters.AddRange(new[]
                {
                    new SqlParameter("@OrdNo", ordNo),
                    new SqlParameter("@OrdDt", dto.OrdDt),
                    new SqlParameter("@AdrKy", dto.AdrKy),
                    new SqlParameter("@AccKy", dto.AccKy),
                    new SqlParameter("@DocNo", dto.DocNo),
                    new SqlParameter("@Des", dto.Des ?? string.Empty),
                    new SqlParameter("@OrdTypKy", dto.OrdTypKy),
                    new SqlParameter("@OrdFrqKy", dto.OrdFrqKy),
                    new SqlParameter("@UsrKy", userKey)
                });

                await cmdHdr.ExecuteNonQueryAsync();

                // 3. Get OrdKy
                var cmdOrdKy = conn.CreateCommand();
                cmdOrdKy.Transaction = tx;
                cmdOrdKy.CommandText = @"
                    SELECT OrdKy FROM vewOrdNo
                    WHERE OrdNo=@OrdNo AND OrdTyp='PURORD' AND CKy=@CKy
                ";
                cmdOrdKy.Parameters.Add(new SqlParameter("@OrdNo", ordNo));
                cmdOrdKy.Parameters.Add(new SqlParameter("@CKy", cKy));
                var ordKy = Convert.ToInt32(await cmdOrdKy.ExecuteScalarAsync());

                // 4. Insert Details
                int lineNo = 1;
                foreach (var item in dto.Items)
                {
                    if (item.IsDeleted) continue;

                    var cmdCost = conn.CreateCommand();
                    cmdCost.Transaction = tx;
                    cmdCost.CommandText = "SELECT CosPri FROM ItmMas WHERE ItmKy=@ItmKy";
                    cmdCost.Parameters.Add(new SqlParameter("@ItmKy", item.ItmKy));
                    var ordPri = Convert.ToDecimal(await cmdCost.ExecuteScalarAsync());

                    var cmdDet = conn.CreateCommand();
                    cmdDet.Transaction = tx;
                    cmdDet.CommandText = @"
                        INSERT INTO OrdDet
                        (OrdKy,ItmKy,OrdQty,CosPri,OrdPri,ReqDt,Des,LiNo,Amt1)
                        VALUES
                        (@OrdKy,@ItmKy,@Qty,@Rate,@OrdPri,@ReqDt,@Des,@LiNo,@Amt1)
                    ";

                    cmdDet.Parameters.AddRange(new[]
                    {
                        new SqlParameter("@OrdKy", ordKy),
                        new SqlParameter("@ItmKy", item.ItmKy),
                        new SqlParameter("@Qty", item.Qty),
                        new SqlParameter("@Rate", item.Rate),
                        new SqlParameter("@OrdPri", ordPri),
                        new SqlParameter("@ReqDt", (object?)item.ReqDt ?? DBNull.Value),
                        new SqlParameter("@Des", item.Des ?? string.Empty),
                        new SqlParameter("@LiNo", lineNo),
                        new SqlParameter("@Amt1", item.TaxAmt)
                    });

                    await cmdDet.ExecuteNonQueryAsync();
                    lineNo++;
                }

                tx.Commit();
                return ordNo;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // ------------------------------------------------
        // PUT - Update Purchase Order
        // ------------------------------------------------
        public async Task UpdatePurchaseOrderAsync(int orderNo, PurchaseOrderSaveDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                var cKy = _userContext.CompanyKey;

                // Get OrdKy
                var cmdOrdKy = conn.CreateCommand();
                cmdOrdKy.Transaction = tx;
                cmdOrdKy.CommandText = @"
                    SELECT OrdKy FROM vewOrdNo
                    WHERE OrdNo=@OrdNo AND OrdTyp='PURORD' AND CKy=@CKy
                ";
                cmdOrdKy.Parameters.Add(new SqlParameter("@OrdNo", orderNo));
                cmdOrdKy.Parameters.Add(new SqlParameter("@CKy", cKy));
                var ordKy = Convert.ToInt32(await cmdOrdKy.ExecuteScalarAsync());

                // Update OrdMas
                var cmdHdr = conn.CreateCommand();
                cmdHdr.Transaction = tx;
                cmdHdr.CommandText = @"
                    UPDATE OrdMas SET
                        OrdDt=@OrdDt,
                        AdrKy=@AdrKy,
                        DocNo=@DocNo,
                        Des=@Des,
                        OrdFrqKy=@OrdFrqKy,
                        AccKy=@AccKy,
                        OrdTyp='PURORD'
                    WHERE OrdKy=@OrdKy
                ";

                cmdHdr.Parameters.AddRange(new[]
                {
                    new SqlParameter("@OrdDt", dto.OrdDt),
                    new SqlParameter("@AdrKy", dto.AdrKy),
                    new SqlParameter("@DocNo", dto.DocNo),
                    new SqlParameter("@Des", dto.Des ?? string.Empty),
                    new SqlParameter("@OrdFrqKy", dto.OrdFrqKy),
                    new SqlParameter("@AccKy", dto.AccKy),
                    new SqlParameter("@OrdKy", ordKy)
                });

                await cmdHdr.ExecuteNonQueryAsync();

                int lineNo = 1;
                foreach (var item in dto.Items)
                {
                    if (item.IsDeleted)
                    {
                        var cmdDel = conn.CreateCommand();
                        cmdDel.Transaction = tx;
                        cmdDel.CommandText = "DELETE FROM OrdDet WHERE OrdDetKy=@OrdDetKy";
                        cmdDel.Parameters.Add(new SqlParameter("@OrdDetKy", item.OrdDetKy));
                        await cmdDel.ExecuteNonQueryAsync();
                        continue;
                    }

                    var cmdCost = conn.CreateCommand();
                    cmdCost.Transaction = tx;
                    cmdCost.CommandText = "SELECT CosPri FROM ItmMas WHERE ItmKy=@ItmKy";
                    cmdCost.Parameters.Add(new SqlParameter("@ItmKy", item.ItmKy));
                    var ordPri = Convert.ToDecimal(await cmdCost.ExecuteScalarAsync());

                    if (item.OrdDetKy == 0)
                    {
                        var cmdIns = conn.CreateCommand();
                        cmdIns.Transaction = tx;
                        cmdIns.CommandText = @"
                            INSERT INTO OrdDet
                            (OrdKy,ItmKy,OrdQty,CosPri,OrdPri,ReqDt,Des,LiNo,Amt1)
                            VALUES
                            (@OrdKy,@ItmKy,@Qty,@Rate,@OrdPri,@ReqDt,@Des,@LiNo,@Amt1)
                        ";
                        cmdIns.Parameters.AddRange(new[]
                        {
                            new SqlParameter("@OrdKy", ordKy),
                            new SqlParameter("@ItmKy", item.ItmKy),
                            new SqlParameter("@Qty", item.Qty),
                            new SqlParameter("@Rate", item.Rate),
                            new SqlParameter("@OrdPri", ordPri),
                            new SqlParameter("@ReqDt", (object?)item.ReqDt ?? DBNull.Value),
                            new SqlParameter("@Des", item.Des ?? string.Empty),
                            new SqlParameter("@LiNo", lineNo),
                            new SqlParameter("@Amt1", item.TaxAmt)
                        });
                        await cmdIns.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        var cmdUpd = conn.CreateCommand();
                        cmdUpd.Transaction = tx;
                        cmdUpd.CommandText = @"
                            UPDATE OrdDet SET
                                ItmKy=@ItmKy,
                                OrdQty=@Qty,
                                OrdPri=@OrdPri,
                                CosPri=@Rate,
                                ReqDt=@ReqDt,
                                Des=@Des,
                                LiNo=@LiNo,
                                Status='U',
                                Amt1=@Amt1
                            WHERE OrdDetKy=@OrdDetKy
                        ";
                        cmdUpd.Parameters.AddRange(new[]
                        {
                            new SqlParameter("@ItmKy", item.ItmKy),
                            new SqlParameter("@Qty", item.Qty),
                            new SqlParameter("@OrdPri", ordPri),
                            new SqlParameter("@Rate", item.Rate),
                            new SqlParameter("@ReqDt", (object?)item.ReqDt ?? DBNull.Value),
                            new SqlParameter("@Des", item.Des ?? string.Empty),
                            new SqlParameter("@LiNo", lineNo),
                            new SqlParameter("@Amt1", item.TaxAmt),
                            new SqlParameter("@OrdDetKy", item.OrdDetKy)
                        });
                        await cmdUpd.ExecuteNonQueryAsync();
                    }

                    lineNo++;
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }


        // ------------------------------------------------
        // Delete Purchase Order
        // ------------------------------------------------
        public async Task DeletePurchaseOrderAsync(int orderNo)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                int ordKy;

                // 1. Get OrdKy
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"
                SELECT OrdKy
                FROM vewOrdNo
                WHERE OrdTyp = 'PURORD'
                  AND OrdNo = @OrdNo
                  AND CKy = @CKy
            ";

                    cmd.Parameters.Add(new SqlParameter("@OrdNo", orderNo));
                    cmd.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));

                    var result = await cmd.ExecuteScalarAsync();
                    if (result == null)
                        throw new Exception("Purchase order not found");

                    ordKy = Convert.ToInt32(result);
                }

                // 2. Soft delete OrdMas
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"
                UPDATE OrdMas
                SET fInAct = 1,
                    Status = 'D'
                WHERE OrdNo = @OrdNo
                  AND OrdTyp = 'PURORD'
            ";

                    cmd.Parameters.Add(new SqlParameter("@OrdNo", orderNo));
                    await cmd.ExecuteNonQueryAsync();
                }

                // 3. Hard delete OrdDet
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"
                DELETE FROM OrdDet
                WHERE OrdKy = @OrdKy
            ";

                    cmd.Parameters.Add(new SqlParameter("@OrdKy", ordKy));
                    await cmd.ExecuteNonQueryAsync();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

    }
}
