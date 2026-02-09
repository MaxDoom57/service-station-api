using Application.DTOs.Invoice;
using Application.Interfaces;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection.Emit;
using System.Security.Cryptography;

namespace Infrastructure.Services
{
    public class InvoiceService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;
        private readonly CommonLookupService _lookup;
        private readonly IUserKeyService _userKeyService;

        public InvoiceService(
            IDynamicDbContextFactory factory,
            IUserRequestContext userContext,
            CommonLookupService lookup,
            IUserKeyService userKeyService)
        {
            _factory = factory;
            _userContext = userContext;
            _lookup = lookup;
            _userKeyService = userKeyService;
        }


        public async Task<InvoiceDetailsDto> GetInvoiceByTrnKyAsync(int trnNo)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            // -------------------------------------------------
            // 0. Resolve TrnKy from TrnNo
            // -------------------------------------------------
            int trnKy = await _lookup.GetTrnKyByTrnNoAsync(trnNo);

            if (trnKy <= 0)
                throw new KeyNotFoundException($"Invoice not found for TrnNo {trnNo}");

            // REQUIRED MEMBERS INITIALIZATION
            var invoice = new InvoiceDto
            {
                AdrCd = string.Empty,
                AccKy = 0,
                PmtTrm1 = string.Empty,
                PmtTrm1Amt = 0,
                Amt = 0
            };

            decimal salesReturnAmount = 0;

            // -------------------------------------------------
            // 1. Invoice Header (vewInvHdr)
            // -------------------------------------------------
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM vewInvHdr WHERE TrnKy = @TrnKy";
                cmd.Parameters.Add(new SqlParameter("@TrnKy", trnKy));

                using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    throw new KeyNotFoundException($"Invoice not found for TrnNo {trnNo}");

                invoice.DocNo = reader["DocNo"] as string;
                invoice.YurRef = reader["YurRef"] as string;

                invoice.AccKy = Convert.ToInt32(reader["AccKy"]);
                invoice.AdrCd = reader["AdrCd"]?.ToString() ?? string.Empty;

                invoice.PmtTrm1 = reader["PmtTrmKy"]?.ToString() ?? string.Empty;
                //invoice.PmtTrm2 = reader["PmtTrmKy2"]?.ToString();

                //invoice.PmtTrm1Amt = Convert.ToDecimal(reader["PmtTrm1Amt"]);
                //invoice.PmtTrm2Amt = reader["PmtTrm2Amt"] == DBNull.Value
                //    ? null
                //    : Convert.ToDecimal(reader["PmtTrm2Amt"]);

                invoice.Amt = Convert.ToDecimal(reader["Amt"]);
                invoice.DisAmt = Convert.ToDecimal(reader["DisPer"]);

                // -------------------------------------------------
                // SAFE STrnKy handling (column may not exist)
                // -------------------------------------------------
                int salesReturnTrnKy = 0;

                var columnNames = Enumerable.Range(0, reader.FieldCount)
                    .Select(reader.GetName)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (columnNames.Contains("STrnKy") && reader["STrnKy"] != DBNull.Value)
                {
                    salesReturnTrnKy = Convert.ToInt32(reader["STrnKy"]);
                }

                // -------------------------------------------------
                // 2. Sales Return Amount (qrySlsRtnRpt)
                // -------------------------------------------------
                if (salesReturnTrnKy > 0)
                {
                    using var rtnCmd = conn.CreateCommand();
                    rtnCmd.CommandText = @"
                SELECT SUM((TrnPri * Qty) - DisAmt)
                FROM qrySlsRtnRpt
                WHERE TrnKy = @TrnKy
            ";

                    rtnCmd.Parameters.Add(new SqlParameter("@TrnKy", salesReturnTrnKy));

                    var rtnResult = await rtnCmd.ExecuteScalarAsync();
                    salesReturnAmount = rtnResult == null || rtnResult == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(rtnResult);
                }
            }

            // -------------------------------------------------
            // 3. Invoice Items (vewInvDet)
            // -------------------------------------------------
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
            SELECT
                ItmKy,
                (Qty * -1) AS Qty,
                CosPri,
                SlsPri,
                TrnPri
            FROM vewInvDet
            WHERE TrnKy = @TrnKy
        ";

                cmd.Parameters.Add(new SqlParameter("@TrnKy", trnKy));

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    invoice.Items.Add(new InvoiceItemDto
                    {
                        ItmKy = Convert.ToInt32(reader["ItmKy"]),

                        Qty = reader["Qty"] == DBNull.Value
                            ? 0
                            : Convert.ToSingle(reader["Qty"]),

                        CosPri = Convert.ToDecimal(reader["CosPri"]),
                        SlsPri = Convert.ToDecimal(reader["SlsPri"]),
                        TrnPri = Convert.ToDecimal(reader["TrnPri"])
                    });
                }
            }

            // -------------------------------------------------
            // 4. Final Amount Adjustment (VB logic)
            // -------------------------------------------------
            invoice.Amt -= salesReturnAmount;

            // -------------------------------------------------
            // 5. Response Wrapper
            // -------------------------------------------------
            return new InvoiceDetailsDto
            {
                CustomerAccount = new
                {
                    invoice.AccKy,
                    invoice.AdrCd
                },

                Items = invoice.Items,

                PaymentTerm = new
                {
                    invoice.PmtTrm1,
                    invoice.PmtTrm2,
                    invoice.PmtTrm1Amt,
                    invoice.PmtTrm2Amt
                },

                SalesAccount = new
                {
                    invoice.DocNo,
                    invoice.YurRef,
                    invoice.Amt,
                    invoice.DisAmt
                }
            };
        }


        public async Task<int> AddInvoiceAsync(InvoiceDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                string ourCode = "SALE";
                short pmtTrm1Ky;
                short pmtTrm2Ky;
                var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
                if (userKey == null)
                    throw new Exception("User key not found");

                short locKy = 276; //TODO: Get from user context (url)

                // ------------------------------------------
                // Get TrnTypKy, TrnNo, AccountKey, PaymentTerm keys
                // ------------------------------------------
                Console.WriteLine("Step 1 OK");
                short trnTypKy = await _lookup.GetTranTypeKeyAsync("SALE");
                Console.WriteLine("Step 2 OK");

                int trnNo = await _lookup.GetTranNumberLastAsync(_userContext.CompanyKey, "SALE");
                Console.WriteLine("Step 3 OK");

                int accKy = dto.AccKy;

                pmtTrm1Ky = await _lookup.GetPaymentTermKeyAsync(dto.PmtTrm1);
                if (dto.PmtTrm1 == null)
                {
                    throw new Exception($"Payment mode1 can't empty or null!");
                }
                else
                {
                    pmtTrm1Ky = await _lookup.GetPaymentTermKeyAsync(dto.PmtTrm1);
                }
                if (dto.PmtTrm2 == null)
                {
                    pmtTrm2Ky = 0;
                }
                else
                {
                    pmtTrm2Ky = await _lookup.GetPaymentTermKeyAsync(dto.PmtTrm2);
                }

                // ------------------------------------------
                // 1) Insert into TrnMas
                // ------------------------------------------

                using var cmd1 = conn.CreateCommand();
                cmd1.Transaction = tx;

                cmd1.CommandText = @"
                INSERT INTO TrnMas
                (
                    CKy, TrnDt, TrnTypKy, TrnNo, fInAct, fApr,
                    OurCd, DocNo, YurRef, Adrky, AccKy,
                    PmtTrmKy,
                    Amt, DisAmt, EntUsrKy, EntDtm
                )
                OUTPUT INSERTED.TrnKy
                VALUES
                (
                    @CKy, @TrnDt, @TrnTypKy, @TrnNo, 0, 1,
                    'SALE', @DocNo, @YurRef, @Adrky, @AccKy,
                    @PmtTrmKy,
                    @Amt, @DisAmt, @EntUsrKy, @EntDtm
                );";

                cmd1.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
                cmd1.Parameters.Add(new SqlParameter("@TrnDt", DateTime.Now));
                cmd1.Parameters.Add(new SqlParameter("@TrnTypKy", trnTypKy));
                cmd1.Parameters.Add(new SqlParameter("@TrnNo", trnNo));
                cmd1.Parameters.Add(new SqlParameter("@DocNo", dto.DocNo ?? " "));
                cmd1.Parameters.Add(new SqlParameter("@YurRef", dto.YurRef ?? " "));
                cmd1.Parameters.Add(new SqlParameter("@Adrky", await _lookup.GetAddressKeyByAccKyAsync(dto.AccKy)));
                cmd1.Parameters.Add(new SqlParameter("@AccKy", accKy));
                cmd1.Parameters.Add(new SqlParameter("@PmtTrmKy", pmtTrm1Ky));
                //cmd1.Parameters.Add(new SqlParameter("@PmtTrmKy2", pmtTrm2Ky));
                //cmd1.Parameters.Add(new SqlParameter("@PmtTrm1Amt", dto.PmtTrm1Amt));
                //cmd1.Parameters.Add(new SqlParameter("@PmtTrm2Amt", dto.PmtTrm2Amt));
                cmd1.Parameters.Add(new SqlParameter("@Amt", dto.Amt));
                cmd1.Parameters.Add(new SqlParameter("@DisAmt", dto.DisAmt));
                cmd1.Parameters.Add(new SqlParameter("@EntUsrKy", userKey));
                cmd1.Parameters.Add(new SqlParameter("@EntDtm", DateTime.Now));

                var trnKyObj = await cmd1.ExecuteScalarAsync();
                int trnKy = Convert.ToInt32(trnKyObj);

                // ------------------------------------------
                // 2) Insert Each ItmTrn Line
                // ------------------------------------------

                int lineNo = 1;

                foreach (var item in dto.Items)
                {
                    using var cmd2 = conn.CreateCommand();
                    cmd2.Transaction = tx;

                    cmd2.CommandText = @"
                    INSERT INTO ItmTrn
                    (TrnKy, LiNo, CKy, ItmKy, Qty, CosPri, SlsPri, TrnPri, EntUsrKy, LocKy)
                    VALUES
                    (@TrnKy, @LiNo, @CKy, @ItmKy, @Qty, @CosPri, @SlsPri, @TrnPri, @EntUsrKy, @LocKy);";

                    cmd2.Parameters.Add(new SqlParameter("@TrnKy", trnKy));
                    cmd2.Parameters.Add(new SqlParameter("@LiNo", lineNo++));
                    cmd2.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
                    cmd2.Parameters.Add(new SqlParameter("@ItmKy", item.ItmKy));
                    cmd2.Parameters.Add(new SqlParameter("@Qty", item.Qty));
                    cmd2.Parameters.Add(new SqlParameter("@CosPri", item.CosPri));
                    cmd2.Parameters.Add(new SqlParameter("@SlsPri", item.SlsPri));
                    cmd2.Parameters.Add(new SqlParameter("@TrnPri", item.TrnPri));
                    cmd2.Parameters.Add(new SqlParameter("@EntUsrKy", userKey));
                    cmd2.Parameters.Add(new SqlParameter("@LocKy", locKy));

                    await cmd2.ExecuteNonQueryAsync();
                }

                // -------------------------------------------------------------
                // 3) Insert into AccTrn
                // -------------------------------------------------------------

                // Get sale transaction account codes
                var cds = await _lookup.GetSaleTransactionCodesAsync(_userContext.CompanyKey, "SALE");

                double cashAccKy = cds?.CdNo1 ?? 0;
                double cardAccKy = cds?.CdNo2 ?? 0;
                double voucherAccKy = cds?.CdNo3 ?? 0;

                int accountKey1 = 0;
                int accountKey2 = 0;

                // Map payment term 1
                switch (dto.PmtTrm1.ToUpper())
                {
                    case "CASH":
                        accountKey1 = (int)cashAccKy;
                        break;
                    case "CRCRD":
                        accountKey1 = (int)cardAccKy;
                        break;
                    case "CHEQUE":
                        accountKey1 = (int)voucherAccKy;
                        break;
                }

                // Map payment term 2
                if (!string.IsNullOrWhiteSpace(dto.PmtTrm2))
                {
                    switch (dto.PmtTrm2.ToUpper())
                    {
                        case "CASH":
                            accountKey2 = (int)cashAccKy;
                            break;
                        case "CRCRD":
                            accountKey2 = (int)cardAccKy;
                            break;
                        case "CHEQUE":
                            accountKey2 = (int)voucherAccKy;
                            break;
                    }
                }

                int salesAccountKey = await _lookup.GetDefaultSalesAccountKeyAsync((short)_userContext.CompanyKey);

                decimal totalInvoiceAmount = dto.Amt - dto.DisAmt;
                decimal secondTermAmount = dto.PmtTrm2Amt ?? 0;

                short paymentModeKey = await _lookup.GetPaymentModeKeyAsync(pmtTrm1Ky);
                Console.WriteLine("Step 4 OK");

                async Task InsertAccTrn(int trnKy, int liNo, int accKy, decimal amt)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;

                    cmd.CommandText = @"
                            INSERT INTO AccTrn
                            (TrnKy, LiNo, AccKy, PmtModeKy, Amt, EntUsrKy)
                            VALUES
                            (@TrnKy, @LiNo, @AccKy, @PmtModeKy, @Amt, @EntUsrKy);";

                    cmd.Parameters.Add(new SqlParameter("@TrnKy", trnKy));
                    cmd.Parameters.Add(new SqlParameter("@LiNo", liNo));
                    cmd.Parameters.Add(new SqlParameter("@AccKy", accKy));
                    cmd.Parameters.Add(new SqlParameter("@PmtModeKy", paymentModeKey));
                    cmd.Parameters.Add(new SqlParameter("@Amt", amt));
                    cmd.Parameters.Add(new SqlParameter("@EntUsrKy", userKey));

                    await cmd.ExecuteNonQueryAsync();
                }

                // 1) Customer debit
                await InsertAccTrn(trnKy, 1, accountKey1, totalInvoiceAmount - secondTermAmount);

                // 2) Sales income credit
                await InsertAccTrn(trnKy, 2, salesAccountKey, -(totalInvoiceAmount));

                // 3) Second payment term (if any)
                await InsertAccTrn(trnKy, 3, accountKey2, secondTermAmount);

                // Update transaction counter
                await _lookup.IncrementTranNumberLastAsync(_userContext.CompanyKey, ourCode);


                // ------------------------------------------
                // Commit
                // ------------------------------------------
                await tx.CommitAsync();
                return trnKy;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }


        public async Task<bool> UpdateInvoiceAsync(UpdateInvoiceDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();
            try
            {
                var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
                if (userKey == null)
                    throw new Exception("User key not found.");

                short pmtTrm1Ky = await _lookup.GetPaymentTermKeyAsync(dto.PmtTrm1);
                short pmtTrm2Ky = string.IsNullOrWhiteSpace(dto.PmtTrm2)
                    ? (short)0
                    : await _lookup.GetPaymentTermKeyAsync(dto.PmtTrm2);

                short locKy = 276; // TODO: Get from user context

                // ---------------------------------------------------------
                // 1) UPDATE TRNMAS
                // ---------------------------------------------------------
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"
                                UPDATE TrnMas SET
                                    DocNo = @DocNo,
                                    YurRef = @YurRef,
                                    AccKy = @AccKy,
                                    PmtTrmKy = @PmtTrmKy,
                                    Amt = @Amt,
                                    DisAmt = @DisAmt,
                                    Des = @Des
                                WHERE TrnKy = @TrnKy";

                    cmd.Parameters.Add(new SqlParameter("@DocNo", dto.DocNo ?? " "));
                    cmd.Parameters.Add(new SqlParameter("@YurRef", dto.YurRef ?? " "));
                    cmd.Parameters.Add(new SqlParameter("@AccKy", dto.AccKy));
                    cmd.Parameters.Add(new SqlParameter("@PmtTrmKy", pmtTrm1Ky));
                    //cmd.Parameters.Add(new SqlParameter("@PmtTrmKy2", pmtTrm2Ky));
                    //cmd.Parameters.Add(new SqlParameter("@PmtTrm1Amt", dto.PmtTrm1Amt));
                    //cmd.Parameters.Add(new SqlParameter("@PmtTrm2Amt", dto.PmtTrm2Amt ?? 0));
                    cmd.Parameters.Add(new SqlParameter("@Amt", dto.Amt));
                    cmd.Parameters.Add(new SqlParameter("@DisAmt", dto.DisAmt));
                    cmd.Parameters.Add(new SqlParameter("@Des", dto.Description ?? " "));
                    cmd.Parameters.Add(new SqlParameter("@TrnKy", dto.TrnKy));

                    await cmd.ExecuteNonQueryAsync();
                }

                // ---------------------------------------------------------
                // 2) DELETE OLD ITEM LINES
                // ---------------------------------------------------------
                using (var cmdDel = conn.CreateCommand())
                {
                    cmdDel.Transaction = tx;
                    cmdDel.CommandText = "DELETE FROM ItmTrn WHERE TrnKy = @TrnKy";
                    cmdDel.Parameters.Add(new SqlParameter("@TrnKy", dto.TrnKy));
                    await cmdDel.ExecuteNonQueryAsync();
                }

                // ---------------------------------------------------------
                // 3) INSERT NEW ITEM LINES
                // ---------------------------------------------------------
                int lineNo = 1;

                foreach (var item in dto.Items)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;

                    cmd.CommandText = @"
                            INSERT INTO ItmTrn
                            (TrnKy, LiNo, CKy, ItmKy, Qty, CosPri, SlsPri, TrnPri, EntUsrKy, LocKy)
                            VALUES
                            (@TrnKy, @LiNo, @CKy, @ItmKy, @Qty, @CosPri, @SlsPri, @TrnPri, @EntUsrKy, @LocKy)";

                    cmd.Parameters.Add(new SqlParameter("@TrnKy", dto.TrnKy));
                    cmd.Parameters.Add(new SqlParameter("@LiNo", lineNo++));
                    cmd.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
                    cmd.Parameters.Add(new SqlParameter("@ItmKy", item.ItmKy));
                    cmd.Parameters.Add(new SqlParameter("@Qty", item.Qty));
                    cmd.Parameters.Add(new SqlParameter("@CosPri", item.CosPri));
                    cmd.Parameters.Add(new SqlParameter("@SlsPri", item.SlsPri));
                    cmd.Parameters.Add(new SqlParameter("@TrnPri", item.TrnPri));
                    cmd.Parameters.Add(new SqlParameter("@EntUsrKy", userKey));
                    cmd.Parameters.Add(new SqlParameter("@LocKy", locKy));

                    await cmd.ExecuteNonQueryAsync();
                }

                // ---------------------------------------------------------
                // 4) UPDATE ACCTRN ROWS
                // ---------------------------------------------------------

                var cds = await _lookup.GetSaleTransactionCodesAsync(_userContext.CompanyKey, "SALE");

                double cashAccKy = cds?.CdNo1 ?? 0;
                double cardAccKy = cds?.CdNo2 ?? 0;
                double voucherAccKy = cds?.CdNo3 ?? 0;

                int accountKey1 = dto.PmtTrm1.ToUpper() switch
                {
                    "CASH" => (int)cashAccKy,
                    "CRCRD" => (int)cardAccKy,
                    "CHEQUE" => (int)voucherAccKy,
                    _ => 0
                };

                int accountKey2 = dto.PmtTrm2?.ToUpper() switch
                {
                    "CASH" => (int)cashAccKy,
                    "CRCRD" => (int)cardAccKy,
                    "CHEQUE" => (int)voucherAccKy,
                    _ => 0
                };

                int salesAccountKey = await _lookup.GetDefaultSalesAccountKeyAsync((short)_userContext.CompanyKey);

                decimal totalInvoiceAmount = dto.Amt - dto.DisAmt;
                decimal secondTermAmount = dto.PmtTrm2Amt ?? 0;
                short paymentModeKey = await _lookup.GetPaymentModeKeyAsync(pmtTrm1Ky);

                async Task UpdateAccTrn(int liNo, int accKy, decimal amt)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;

                    cmd.CommandText = @"
                            UPDATE AccTrn SET
                                Amt = @Amt,
                                AccKy = @AccKy,
                                PmtModeKy = @PmtModeKy,
                                Status = 'U'
                            WHERE TrnKy = @TrnKy AND LiNo = @LiNo";

                    cmd.Parameters.Add(new SqlParameter("@TrnKy", dto.TrnKy));
                    cmd.Parameters.Add(new SqlParameter("@LiNo", liNo));
                    cmd.Parameters.Add(new SqlParameter("@AccKy", accKy));
                    cmd.Parameters.Add(new SqlParameter("@Amt", amt));
                    cmd.Parameters.Add(new SqlParameter("@PmtModeKy", paymentModeKey));

                    await cmd.ExecuteNonQueryAsync();
                }

                await UpdateAccTrn(1, accountKey1, totalInvoiceAmount - secondTermAmount);
                await UpdateAccTrn(2, salesAccountKey, -(totalInvoiceAmount));
                await UpdateAccTrn(3, accountKey2, secondTermAmount);

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

    }
}
