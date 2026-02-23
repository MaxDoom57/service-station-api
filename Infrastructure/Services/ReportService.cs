using Application.DTOs.Reports;
using Application.DTOs.Reports.Report_Account_Balance_As_At;
using Application.DTOs.Reports.Report_Account_Ledger;
using Application.DTOs.Reports.Report_Account_Transaction_Details;
using Application.DTOs.Reports.Report_Credit_Sales_Summary;
using Application.DTOs.Reports.Report_Creditor_Age_Analysis;
using Application.DTOs.Reports.Report_Creditors_Due_Statement;
using Application.DTOs.Reports.Report_Customer_Address_Label;
using Application.DTOs.Reports.Report_Gross_Profit;
using Application.DTOs.Reports.Report_Item_Batch_Details;
using Application.DTOs.Reports.Report_Item_Cat1_Wise_Transaction_Summary;
using Application.DTOs.Reports.Report_Non_Active_Customer_List;
using Application.DTOs.Reports.Report_Non_Moving_Item_List;
using Application.DTOs.Reports.Report_ReOrder_Details;
using Application.DTOs.Reports.Report_Return_Cheque_Details;
using Application.DTOs.Reports.Report_Sales_Details;
using Application.DTOs.Reports.Report_Sales_Details_By_Item;
using Application.DTOs.Reports.Report_Sales_Details_by_Payment_Mode;
using Application.DTOs.Reports.Report_Sales_Summary_By_Rep;
using Application.DTOs.Reports.Report_SetOff_Details_Report;
using Application.DTOs.Reports.Report_Stock_As_At;
using Application.DTOs.Reports.Report_Stock_Ledger;
using Application.DTOs.Reports.Report_Stock_Movement;
using Application.DTOs.Reports.Report_Transaction_Details;
using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.Reports;
using Infrastructure.Context;
using Infrastructure.Context.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using POS.Core.Entities.Reports;
using System.Data;

namespace Infrastructure.Services
{
    public class ReportService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;
        private readonly CommonLookupService _lookup;
        private readonly IUserKeyService _userKeyService;
        public ReportService(
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

        public async Task<ReportSalesByRepResponseDto> GetSalesByRepAsync(ReportSalesByRepRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            string reportTitle = "";

            using var db = await _factory.CreateDbContextAsync();
                using var conn = db.Database.GetDbConnection();
                await conn.OpenAsync();

                var sql = @"
                SELECT
                    TrnDt,
                    TrnNo,
                    SlsRepAdrNm,
                    ItmCd,
                    ItmNm,
                    Qty,
                    TrnPri,
                    DisAmt
                FROM vewSlsDtlsRpt
                WHERE TrnDt >= @FromDate
                  AND TrnDt <= @ToDate
            ";

                if (!string.IsNullOrWhiteSpace(request.RepName))
                    sql += " AND SlsRepAdrNm = @RepName";

                if (request.ItemKy.HasValue)
                    sql += " AND ItmKy = @ItemKy";

                if (!request.SalesReturn)
                    sql += " AND OurCd <> 'SLSRTN'";

                sql += " ORDER BY TrnDt, TrnNo, LiNo";

                var lookup = new Dictionary<int, SalesByRepTransactionDto>();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;

                    cmd.Parameters.Add(new SqlParameter("@FromDate", request.FromDate.Date));
                    cmd.Parameters.Add(new SqlParameter("@ToDate", request.ToDate.Date));

                    if (!string.IsNullOrWhiteSpace(request.RepName))
                        cmd.Parameters.Add(new SqlParameter("@RepName", request.RepName));

                    if (request.ItemKy.HasValue)
                        cmd.Parameters.Add(new SqlParameter("@ItemKy", request.ItemKy.Value));

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var trnNo = reader.GetInt32(1);

                        if (!lookup.TryGetValue(trnNo, out var trx))
                        {
                            trx = new SalesByRepTransactionDto
                            {
                                TrnDt = reader.GetDateTime(0),
                                TrnNo = trnNo,
                                SlsRepAdrNm = reader.IsDBNull(2) ? null : reader.GetString(2)
                            };
                            lookup.Add(trnNo, trx);
                        }

                        trx.Items.Add(new SalesByRepItemDto
                        {
                            ItmCd = reader.IsDBNull(3) ? null : reader.GetString(3),
                            ItmNm = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Qty = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                            TrnPri = reader.GetDecimal(6),
                            DisAmt = reader.GetDecimal(7)
                        });
                    }
                }

                return new ReportSalesByRepResponseDto
                {
                    Context = new ReportContextDto
                    {
                        CompanyName = companyName,
                        CurrentUserId = _userContext.UserId,
                        ReportTitle = reportTitle
                    },
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    RepName = request.RepName,
                    ItemKy = request.ItemKy,
                    SalesReturn = request.SalesReturn,
                    ViewType = request.ViewType,
                    Transactions = lookup.Values.ToList()
                };
        }

        public async Task<SalesDetailsByPaymentModeResponseDto>GetSalesDetailsByPaymentModeAsync(SalesDetailsByPaymentModeRequestDto request)
        {
            if (request.FromDate > request.ToDate)
                throw new Exception("From date cannot be greater than To date");

            int cKy = _userContext.CompanyKey;
            string companyName =
                await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();

            var query = db.Set<vewSlsDetByPmtTrm>()
                .AsNoTracking()
                .Where(x =>
                    x.TrnDt >= request.FromDate.ToDateTime(TimeOnly.MinValue) &&
                    x.TrnDt <= request.ToDate.ToDateTime(TimeOnly.MaxValue));

            // Crystal condition:
            // {vewSlsDetByPmtTrm.PmtModeNm}='...'
            if (!string.IsNullOrWhiteSpace(request.PaymentModeName))
            {
                query = query.Where(x =>
                    x.PmtModeNm == request.PaymentModeName);
            }

            var rows = await query
                .OrderBy(x => x.TrnDt)
                .ThenBy(x => x.TrnNo)
                .Select(x => new SalesDetailsByPaymentModeRowDto
                {
                    TrnDt = DateOnly.FromDateTime(x.TrnDt),
                    TrnNo = x.TrnNo,

                    PmtModeCd = x.PmtModeCd,
                    PmtModeNm = x.PmtModeNm,

                    AccCd = x.AccCd,
                    AccNm = x.AccNm,

                    Amt = x.Amt,
                    UsrId = x.UsrId
                })
                .ToListAsync();

            return new SalesDetailsByPaymentModeResponseDto
            {
                Rows = rows,

                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId
                }
            };
        }


        //TODO: add vewStkMovmntRpt into TF db
        //public async Task<StockMovementSummaryResponseDto>GetStockMovementSummaryAsync(StockMovementSummaryRequestDto request)
        //{
        //    if (request.FromDate > request.ToDate)
        //        throw new Exception("From date cannot be greater than To date");

        //    int cKy = _userContext.CompanyKey;
        //    string companyName =
        //        await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

        //    using var db = await _factory.CreateDbContextAsync();

        //    var query = db.Set<vewStkMovmntRpt>()
        //        .AsNoTracking()
        //        .Where(x =>
        //            x.TrnDt >= request.FromDate.ToDateTime(TimeOnly.MinValue) &&
        //            x.TrnDt <= request.ToDate.ToDateTime(TimeOnly.MaxValue) &&
        //            x.CKy == cKy);

        //    query = request.SortDescending
        //        ? query.OrderByDescending(x => x.Qty)
        //        : query.OrderBy(x => x.Qty);

        //    var rows = await query
        //        .Select(x => new StockMovementSummaryRowDto
        //        {
        //            ItmKy = x.ItmKy,
        //            ItmCd = x.ItmCd,
        //            ItmNm = x.ItmNm,
        //            Qty = x.Qty
        //        })
        //        .ToListAsync();

        //    return new StockMovementSummaryResponseDto
        //    {
        //        Rows = rows,

        //        Context = new ReportContextDto
        //        {
        //            CompanyName = companyName,
        //            CurrentUserId = _userContext.UserId
        //        }
        //    };
        //}

        public async Task<TrnDetReportResponseDto> GetTransactionDetailsReportAsync(TrnDetReportRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            var fromDate = request.FromDate.ToDateTime(TimeOnly.MinValue);
            var toDate = request.ToDate.ToDateTime(TimeOnly.MaxValue);

            string reportTitle;

            if (request.ShowDetails)
                reportTitle = "TRANSACTION DETAILS REPORT";
            else
                reportTitle = request.Summary
                    ? "TRANSACTION SUMMARY REPORT - ITEM CATEGORY1 WISE"
                    : "TRANSACTION DETAIL REPORT - ITEM CATEGORY1 WISE";

            var sql = @"
                        SELECT
                            TrnDt, TrnNo, TrnTyp, CdNm, DocNo, YurRef, AdrNm,
                            ItmCd, ItmNm, Qty, Unit,
                            ISNULL(TrnPri,0),
                            Amt1, Amt2, DisAmt, Amt3,
                            ItmCat1Nm
                        FROM dbo.TrnDetQry
                        WHERE TrnDt >= @FromDate
                          AND TrnDt <= @ToDate
                    ";

            if (!string.IsNullOrWhiteSpace(request.TrnTyp))
                sql += " AND TrnTyp = @TrnTyp";

            if (!string.IsNullOrWhiteSpace(request.AdrNm))
                sql += " AND AdrNm = @AdrNm";

            if (!string.IsNullOrWhiteSpace(request.ItmNm))
                sql += " AND ItmNm = @ItmNm";

            if (request.FreeItemsOnly)
                sql += " AND ISNULL(TrnPri,0) = 0";

            if (request.RepAdrKy.HasValue)
                sql += " AND RepAdrKy = @RepAdrKy";

            sql += " ORDER BY TrnDt, TrnNo";

            var rows = new List<TrnDetReportRowDto>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;

                cmd.Parameters.Add(new SqlParameter("@FromDate", fromDate));
                cmd.Parameters.Add(new SqlParameter("@ToDate", toDate));

                if (!string.IsNullOrWhiteSpace(request.TrnTyp))
                    cmd.Parameters.Add(new SqlParameter("@TrnTyp", request.TrnTyp));

                if (!string.IsNullOrWhiteSpace(request.AdrNm))
                    cmd.Parameters.Add(new SqlParameter("@AdrNm", request.AdrNm));

                if (!string.IsNullOrWhiteSpace(request.ItmNm))
                    cmd.Parameters.Add(new SqlParameter("@ItmNm", request.ItmNm));

                if (request.RepAdrKy.HasValue)
                    cmd.Parameters.Add(new SqlParameter("@RepAdrKy", request.RepAdrKy.Value));

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rows.Add(new TrnDetReportRowDto
                    {
                        TrnDt = reader.GetDateTime(0),
                        TrnNo = reader.GetInt32(1),
                        TrnTyp = reader.GetString(2),
                        CdNm = reader.GetString(3),
                        DocNo = reader.IsDBNull(4) ? null : reader.GetString(4),
                        YurRef = reader.IsDBNull(5) ? null : reader.GetString(5),
                        AdrNm = reader.IsDBNull(6) ? null : reader.GetString(6),
                        ItmCd = reader.GetString(7),
                        ItmNm = reader.IsDBNull(8) ? null : reader.GetString(8),
                        Qty = reader.GetDouble(9),
                        Unit = reader.IsDBNull(10) ? null : reader.GetString(10),
                        TrnPri = reader.GetDecimal(11),
                        Amt1 = reader.GetDecimal(12),
                        Amt2 = reader.GetDecimal(13),
                        DisAmt = reader.GetDecimal(14),
                        Amt3 = reader.GetDecimal(15),
                        ItmCat1Nm = reader.IsDBNull(16) ? null : reader.GetString(16),
                        BUNm = reader.IsDBNull(17) ? null : reader.GetString(17),
                        UsrId = reader.IsDBNull(18) ? null : reader.GetString(18)
                    });
                }
            }

            return new TrnDetReportResponseDto
            {
                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId,
                    ReportTitle = reportTitle
                },
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TrnTypLabel = string.IsNullOrWhiteSpace(request.TrnTyp)
                    ? "All"
                    : request.TrnTyp,
                Rows = rows
            };
        }

        public async Task<SalesRepWiseCustomerResponseDto>GetSalesRepWiseCustomerAsync(SalesRepWiseCustomerRequestDto request)
        {
            if (request.FromDate > request.ToDate)
                throw new Exception("Invalid date range");

            int cKy = _userContext.CompanyKey;
            string companyName =
                await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();

            var query = db.Set<VewSlsDtlsRpt>()
                .AsNoTracking()
                .Where(x =>
                    x.TrnDt >= request.FromDate.ToDateTime(TimeOnly.MinValue) &&
                    x.TrnDt <= request.ToDate.ToDateTime(TimeOnly.MaxValue));

            if (request.RepAdrKy.HasValue)
                query = query.Where(x => x.RepAdrKy == request.RepAdrKy.Value);

            if (request.ItmKy.HasValue)
                query = query.Where(x => x.ItmKy == request.ItmKy.Value);

            if (request.OnlySales)
                query = query.Where(x => x.OurCd == "SALE");

            var rows = await query
                .Select(x => new SalesRepWiseCustomerRowDto
                {
                    TrnDt = DateOnly.FromDateTime(x.TrnDt),
                    TrnNo = x.TrnNo,
                    AccNm = x.AccNm,
                    ItmNm = x.ItmNm,
                    Qty = (decimal)(x.Qty ?? 0),
                    Amt = (decimal)(x.Qty ?? 0d) * (x.TrnPri ?? 0m)
                })
                .ToListAsync();

            return new SalesRepWiseCustomerResponseDto
            {
                ReportTitle =
                    request.IsSummary
                        ? "SALES REP. WISE CUSTOMER SUMMARY"
                        : "SALES REP. WISE CUSTOMER DETAILS",

                FromDate = request.FromDate,
                ToDate = request.ToDate,

                Rows = rows,

                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId
                }
            };
        }

        public async Task<SalesItemReportResponseDto> GetSalesItemReportAsync(SalesItemReportRequestDto request)
        {
            using var db = await _factory.CreateDbContextAsync();

            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            var query = db.Set<VewSlsDtlsRpt>()
                .AsNoTracking()
                .Where(x =>
                    x.TrnDt >= request.FromDate.ToDateTime(TimeOnly.MinValue) &&
                    x.TrnDt <= request.ToDate.ToDateTime(TimeOnly.MaxValue));

            if (request.AccKy.HasValue)
                query = query.Where(x => x.AccKy == request.AccKy.Value);

            if (request.ItmKy.HasValue)
                query = query.Where(x => x.ItmKy == request.ItmKy.Value);

            if (request.SalesOnly)
                query = query.Where(x => x.OurCd == "SALE");

            List<SalesItemRowDto> rows;

            if (request.Summary)
            {
                rows = await query
                    .GroupBy(x => new
                    {
                        x.ItmKy,
                        x.ItmNm
                    })
                    .Select(g => new SalesItemRowDto
                    {
                        ItmKy = g.Key.ItmKy,
                        ItmNm = g.Key.ItmNm,
                        Qty = g.Sum(x => (decimal)(x.Qty ?? 0)),
                        Amount = g.Sum(x =>(decimal)(x.Qty ?? 0) * (x.TrnPri ?? 0m))
                    })
                    .OrderBy(x => x.ItmNm)
                    .ToListAsync();
            }
            else
            {
                rows = await query
                    .Select(x => new SalesItemRowDto
                    {
                        TrnDt = DateOnly.FromDateTime(x.TrnDt),
                        AccKy = x.AccKy,
                        AccNm = x.AccNm,
                        ItmKy = x.ItmKy,
                        ItmNm = x.ItmNm,
                        Qty = (decimal)(x.Qty ?? 0),
                        Amount = (decimal)(x.Qty ?? 0) * (x.TrnPri ?? 0m)
                    })
                    .OrderBy(x => x.TrnDt)
                    .ToListAsync();
            }

            return new SalesItemReportResponseDto
            {
                ReportTitle = request.Summary
                    ? "SALES ITEM SUMMARY REPORT"
                    : "SALES DETAILS REPORT",

                FromDate = request.FromDate,
                ToDate = request.ToDate,

                Rows = rows,

                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId
                }
            };
        }


        public async Task<NonPerformedItemResponseDto> GetNonPerformedItemsAsync(NonPerformedItemRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            var rows = new List<NonPerformedItemRowDto>();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "sprNonPerformedItemRpt";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter(
                "@DtFrm",
                SqlDbType.SmallDateTime)
            {
                Value = request.FromDate.ToDateTime(TimeOnly.MinValue)
            });

            cmd.Parameters.Add(new SqlParameter(
                "@DtTo",
                SqlDbType.SmallDateTime)
            {
                Value = request.ToDate.ToDateTime(TimeOnly.MinValue)
            });

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int itmKy = reader.GetInt32(reader.GetOrdinal("ItmKy"));

                // VB: {sprNonPerformedItemRpt;1.ItmKy} > 0
                if (itmKy <= 0)
                    continue;

                short cat1Ky = reader.GetInt16(reader.GetOrdinal("ItmCat1Ky"));
                short cat2Ky = reader.GetInt16(reader.GetOrdinal("ItmCat2Ky"));

                if (request.ItmCat1Ky.HasValue && cat1Ky != request.ItmCat1Ky.Value)
                    continue;

                if (request.ItmCat2Ky.HasValue && cat2Ky != request.ItmCat2Ky.Value)
                    continue;

                rows.Add(new NonPerformedItemRowDto
                {
                    ItmKy = itmKy,
                    ItmCd = reader.GetString(reader.GetOrdinal("ItmCd")),
                    ItmNm = reader.IsDBNull(reader.GetOrdinal("ItmNm"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("ItmNm")),
                    ItmCat1Ky = cat1Ky,
                    ItmCat2Ky = cat2Ky
                });
            }

            return new NonPerformedItemResponseDto
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Rows = rows,
                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId
                }
            };
        }

        public async Task<NonPerformedCustomerResponseDto> GetNonPerformedCustomersAsync(NonPerformedCustomerRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            var rows = new List<NonPerformedCustomerRowDto>();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "sprNonPerformedCustRpt";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@FromDt", SqlDbType.SmallDateTime)
            {
                Value = request.FromDate.ToDateTime(TimeOnly.MinValue)
            });

            cmd.Parameters.Add(new SqlParameter("@ToDt", SqlDbType.SmallDateTime)
            {
                Value = request.ToDate.ToDateTime(TimeOnly.MinValue)
            });

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                rows.Add(new NonPerformedCustomerRowDto
                {
                    AdrKy = reader.GetInt32(reader.GetOrdinal("AdrKy")),
                    AdrCd = reader.IsDBNull(reader.GetOrdinal("AdrCd"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("AdrCd")),
                    AdrNm = reader.IsDBNull(reader.GetOrdinal("AdrNm"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("AdrNm")),
                    AccCd = reader.IsDBNull(reader.GetOrdinal("AccCd"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("AccCd")),
                    AccNm = reader.IsDBNull(reader.GetOrdinal("AccNm"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("AccNm"))
                });
            }

            return new NonPerformedCustomerResponseDto
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Rows = rows,
                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId
                }
            };
        }

        public async Task<ChqReturnReportResponseDto> GetChequeReturnDetailsAsync(ChqReturnReportRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            var fromDate = request.FromDate.ToDateTime(TimeOnly.MinValue);
            var toDate = request.ToDate.ToDateTime(TimeOnly.MaxValue);

            var sql = @"
                        SELECT
                            TrnDt,
                            TrnNo,
                            OurCd,
                            DocNo,
                            Des,
                            ChqNo,
                            LiNo,
                            ISNULL(Amt,0),
                            AccKy
                        FROM dbo.vewChqRtnDet
                        WHERE TrnDt >= @FromDate
                          AND TrnDt <= @ToDate
                    ";

            if (request.AccKy.HasValue)
                sql += " AND AccKy = @AccKy";

            sql += " ORDER BY TrnDt, TrnNo, LiNo";

            var rows = new List<ChqReturnReportRowDto>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;

                cmd.Parameters.Add(new SqlParameter("@FromDate", fromDate));
                cmd.Parameters.Add(new SqlParameter("@ToDate", toDate));

                if (request.AccKy.HasValue)
                    cmd.Parameters.Add(new SqlParameter("@AccKy", request.AccKy.Value));

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rows.Add(new ChqReturnReportRowDto
                    {
                        TrnDt = reader.GetDateTime(0),
                        TrnNo = reader.GetInt32(1),
                        OurCd = reader.IsDBNull(2) ? null : reader.GetString(2),
                        DocNo = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Des = reader.IsDBNull(4) ? null : reader.GetString(4),
                        ChqNo = reader.IsDBNull(5) ? null : reader.GetString(5),
                        LiNo = reader.GetInt16(6),
                        Amt = reader.GetDecimal(7),
                        AccKy = reader.GetInt32(8)
                    });
                }
            }

            return new ChqReturnReportResponseDto
            {
                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId,
                    ReportTitle = "RETURNED CHEQUE DETAILS REPORT"
                },
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Rows = rows
            };
        }

        public async Task<AccLedgerReportResponseDto> GetAccountLedgerAsync(AccLedgerReportRequestDto request)
        {
            if (!request.AccKy.HasValue)
                throw new Exception("Account is required");

            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            var fromDate = request.FromDate.ToDateTime(TimeOnly.MinValue);
            var toDate = request.ToDate.ToDateTime(TimeOnly.MaxValue);

            // -----------------------------------------
            // 1. Opening Balance (SP_AccOPBal)
            // -----------------------------------------
            decimal opBal = 0;
            decimal? fOpBal = null;

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SP_AccOPBal";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@pTrnDt", fromDate));
                cmd.Parameters.Add(new SqlParameter("@pItmKy", request.AccKy.Value));

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    opBal = reader.IsDBNull(reader.GetOrdinal("AccOPBalAmt"))
                        ? 0
                        : reader.GetDecimal(reader.GetOrdinal("AccOPBalAmt"));

                    if (request.MultiCurrency)
                    {
                        fOpBal = reader.IsDBNull(reader.GetOrdinal("AccOPBalFrnAmt"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("AccOPBalFrnAmt"));
                    }
                }
            }

            // -----------------------------------------
            // 2. Main ledger query
            // -----------------------------------------
            var sql = @"
                        SELECT
                            TrnDt, TrnNo, TrnType, DocNo, YurRef, ChqNo, Des,
                            ISNULL(Amt,0),
                            FrnAmt,
                            AccCd, AccNm,
                            BUCd, RepAdrNm, LocNm
                        FROM dbo.vewAccTrnRpt
                        WHERE TrnDt >= @FromDate
                          AND TrnDt <= @ToDate
                    ";

            if (request.AccKy.HasValue)
                sql += " AND AccKy = @AccKy";

            if (!request.MultiCurrency && request.AccTypKy.HasValue)
                sql += " AND AccTypKy = @AccTypKy";

            sql += " ORDER BY TrnDt, TrnNo, LiNo";

            var rows = new List<AccLedgerReportRowDto>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;

                cmd.Parameters.Add(new SqlParameter("@FromDate", fromDate));
                cmd.Parameters.Add(new SqlParameter("@ToDate", toDate));
                cmd.Parameters.Add(new SqlParameter("@AccKy", request.AccKy.Value));

                if (!request.MultiCurrency && request.AccTypKy.HasValue)
                    cmd.Parameters.Add(new SqlParameter("@AccTypKy", request.AccTypKy.Value));

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rows.Add(new AccLedgerReportRowDto
                    {
                        TrnDt = reader.GetDateTime(0),
                        TrnNo = reader.GetInt32(1),
                        TrnType = reader.GetString(2),
                        DocNo = reader.IsDBNull(3) ? null : reader.GetString(3),
                        YurRef = reader.IsDBNull(4) ? null : reader.GetString(4),
                        ChqNo = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Des = reader.IsDBNull(6) ? null : reader.GetString(6),
                        Amt = reader.GetDecimal(7),
                        FrnAmt = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                        AccCd = reader.IsDBNull(9) ? null : reader.GetString(9),
                        AccNm = reader.IsDBNull(10) ? null : reader.GetString(10),
                        BUCd = reader.IsDBNull(11) ? null : reader.GetString(11),
                        RepAdrNm = reader.IsDBNull(12) ? null : reader.GetString(12),
                        LocNm = reader.IsDBNull(13) ? null : reader.GetString(13),
                        BUNm = reader.IsDBNull(14) ? null : reader.GetString(14)
                    });
                }
            }

            return new AccLedgerReportResponseDto
            {
                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId,
                    ReportTitle = request.MultiCurrency
                        ? "Multi Currency Account Ledger"
                        : "ACCOUNTS LEDGER"
                },
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                OpeningBalance = opBal,
                OpeningForeignBalance = fOpBal,
                Rows = rows
            };
        }

        public async Task<AccTrnDetailsReportResponseDto> GetAccountTransactionDetailsAsync(AccTrnDetailsReportRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            // -------------------------------------------------
            // 1. Opening Balance (SP_AccOPBal)
            // -------------------------------------------------
            decimal openingBalance = 0;

            if (request.AccKy.HasValue && request.FromDate.HasValue)
            {
                using var cmdBal = conn.CreateCommand();
                cmdBal.CommandText = "SP_AccOPBal";
                cmdBal.CommandType = System.Data.CommandType.StoredProcedure;

                cmdBal.Parameters.Add(new SqlParameter("@pTrnDt",
                    request.FromDate.Value.ToDateTime(TimeOnly.MinValue)));
                cmdBal.Parameters.Add(new SqlParameter("@pItmKy", request.AccKy.Value));

                using var r = await cmdBal.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    openingBalance = r.IsDBNull(r.GetOrdinal("AccOPBalAmt"))
                        ? 0
                        : r.GetDecimal(r.GetOrdinal("AccOPBalAmt"));
                }
            }

            // -------------------------------------------------
            // 2. Build SQL exactly like VB RecordSelectionFormula
            // -------------------------------------------------
            var sql = @"
                        SELECT
                            TrnDt, TrnNo, TrnType, DocNo, YurRef, Des, ChqNo,
                            ISNULL(Amt,0), FrnAmt, AccCd, AccNm
                        FROM dbo.vewAccTrnRpt
                        WHERE 1 = 1
                    ";

            var cmd = conn.CreateCommand();

            if (request.FromDate.HasValue)
            {
                sql += " AND TrnDt >= @FromDate";
                cmd.Parameters.Add(new SqlParameter("@FromDate",
                    request.FromDate.Value.ToDateTime(TimeOnly.MinValue)));
            }

            if (request.ToDate.HasValue)
            {
                sql += " AND TrnDt <= @ToDate";
                cmd.Parameters.Add(new SqlParameter("@ToDate",
                    request.ToDate.Value.ToDateTime(TimeOnly.MaxValue)));
            }

            if (request.FromTrnNo.HasValue)
            {
                sql += " AND TrnNo >= @FromTrnNo";
                cmd.Parameters.Add(new SqlParameter("@FromTrnNo", request.FromTrnNo.Value));
            }

            if (request.ToTrnNo.HasValue)
            {
                sql += " AND TrnNo <= @ToTrnNo";
                cmd.Parameters.Add(new SqlParameter("@ToTrnNo", request.ToTrnNo.Value));
            }

            if (!string.IsNullOrEmpty(request.FromDocNo))
            {
                sql += " AND DocNo >= @FromDocNo";
                cmd.Parameters.Add(new SqlParameter("@FromDocNo", request.FromDocNo));
            }

            if (!string.IsNullOrEmpty(request.ToDocNo))
            {
                sql += " AND DocNo <= @ToDocNo";
                cmd.Parameters.Add(new SqlParameter("@ToDocNo", request.ToDocNo));
            }

            if (!string.IsNullOrEmpty(request.FromYurRef))
            {
                sql += " AND YurRef >= @FromYurRef";
                cmd.Parameters.Add(new SqlParameter("@FromYurRef", request.FromYurRef));
            }

            if (!string.IsNullOrEmpty(request.ToYurRef))
            {
                sql += " AND YurRef <= @ToYurRef";
                cmd.Parameters.Add(new SqlParameter("@ToYurRef", request.ToYurRef));
            }

            if (request.AccKy.HasValue)
            {
                sql += " AND AccKy = @AccKy";
                cmd.Parameters.Add(new SqlParameter("@AccKy", request.AccKy.Value));
            }

            if (request.AccTypKy.HasValue)
            {
                sql += " AND AccTypKy = @AccTypKy";
                cmd.Parameters.Add(new SqlParameter("@AccTypKy", request.AccTypKy.Value));
            }

            if (request.TrnTypKy.HasValue)
            {
                sql += " AND TrnTypKy = @TrnTypKy";
                cmd.Parameters.Add(new SqlParameter("@TrnTypKy", request.TrnTypKy.Value));
            }

            sql += " ORDER BY TrnDt, TrnNo, LiNo";

            cmd.CommandText = sql;

            // -------------------------------------------------
            // 3. Execute
            // -------------------------------------------------
            var rows = new List<AccTrnDetailsRowDto>();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new AccTrnDetailsRowDto
                {
                    TrnDt = reader.GetDateTime(0),
                    TrnNo = reader.GetInt32(1),
                    TrnType = reader.GetString(2),
                    DocNo = reader.IsDBNull(3) ? null : reader.GetString(3),
                    YurRef = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Des = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ChqNo = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Amt = reader.GetDecimal(7),
                    FrnAmt = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                    AccCd = reader.IsDBNull(9) ? null : reader.GetString(9),
                    AccNm = reader.IsDBNull(10) ? null : reader.GetString(10)
                });
            }

            return new AccTrnDetailsReportResponseDto
            {
                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId,
                    ReportTitle = request.IsSummary
                        ? "ACCOUNTS SUMMARY"
                        : request.IsContra
                            ? "ACCOUNTS TRANSACTIONS WITH CONTRA"
                            : "ACCOUNTS TRANSACTIONS DETAILS"
                },
                OpeningBalance = openingBalance,
                Rows = rows
            };
        }

        public async Task<AccBalanceAsAtResponseDto> GetAccountBalanceAsAtAsync(AccBalanceAsAtRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            var asAtDate = request.AsAtDate.ToDateTime(TimeOnly.MaxValue);

            // ------------------------------------------------
            // Build SQL from VB RecordSelectionFormula
            // ------------------------------------------------
            var sql = @"
                        SELECT
                            AccKy,
                            AccCd,
                            AccNm,
                            AccTyp,
                            SUM(ISNULL(Amt,0)) AS Balance
                        FROM dbo.TransactionsQry
                        WHERE TrnDt <= @AsAtDate
                          AND fInAct = 0
                    ";

            var cmd = conn.CreateCommand();
            cmd.Parameters.Add(new SqlParameter("@AsAtDate", asAtDate));

            if (request.AccKy.HasValue)
            {
                sql += " AND AccKy = @AccKy";
                cmd.Parameters.Add(new SqlParameter("@AccKy", request.AccKy.Value));
            }

            if (request.AccTypes != null && request.AccTypes.Count > 0)
            {
                var typeParams = new List<string>();
                for (int i = 0; i < request.AccTypes.Count; i++)
                {
                    var p = "@AccTyp" + i;
                    typeParams.Add(p);
                    cmd.Parameters.Add(new SqlParameter(p, request.AccTypes[i]));
                }

                sql += $" AND AccTyp IN ({string.Join(",", typeParams)})";
            }

                sql += @"
                    GROUP BY AccKy, AccCd, AccNm, AccTyp
                    ORDER BY AccTyp, AccCd
                ";

            cmd.CommandText = sql;

            // ------------------------------------------------
            // Execute
            // ------------------------------------------------
            var rows = new List<AccBalanceAsAtRowDto>();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new AccBalanceAsAtRowDto
                {
                    AccKy = reader.GetInt32(0),
                    AccCd = reader.IsDBNull(1) ? null : reader.GetString(1),
                    AccNm = reader.IsDBNull(2) ? null : reader.GetString(2),
                    AccTyp = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Balance = reader.GetDecimal(4)
                });
            }

            return new AccBalanceAsAtResponseDto
            {
                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId,
                    ReportTitle = "ACCOUNT TYPE WISE SUMMARY"
                },
                AsAtDate = request.AsAtDate,
                Rows = rows
            };
        }

        public async Task<ItmCat1WiseTrnSumResponseDto> GetItemCat1WiseTransactionSummaryAsync(ItmCat1WiseTrnSumRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            var fromDate = request.FromDate.ToDateTime(TimeOnly.MinValue);
            var toDate = request.ToDate.ToDateTime(TimeOnly.MaxValue);

            var rows = new List<ItmCat1WiseTrnSumRowDto>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "sprItmCat1WiseTrnSumRpt";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@pFrmDt", fromDate));
                cmd.Parameters.Add(new SqlParameter("@pToDt", toDate));
                cmd.Parameters.Add(new SqlParameter("@pLocKy", request.LocKy));
                cmd.Parameters.Add(new SqlParameter("@pItmTypKy", request.ItmTypKy));
                cmd.Parameters.Add(new SqlParameter("@pCKy", _userContext.CompanyKey));

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rows.Add(new ItmCat1WiseTrnSumRowDto
                    {
                        ItmCat1Ky = reader.GetInt16(0),
                        ItmCat1Cd = reader.IsDBNull(1) ? null : reader.GetString(1),
                        ItmCat1Nm = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Qty = reader.GetDecimal(3),
                        Amount = reader.GetDecimal(4)
                    });
                }
            }

            return new ItmCat1WiseTrnSumResponseDto
            {
                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId,
                    ReportTitle = "ITEM CAT1 WISE TRANSACTION SUMMARY"
                },
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                LocKy = request.LocKy,
                ItmTypKy = request.ItmTypKy,
                Rows = rows
            };
        }

        public async Task<SetOffDetailsReportResponseDto> GetSetOffDetailsAsync(SetOffDetailsReportRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            var fromDate = request.FromDate.ToDateTime(TimeOnly.MinValue);
            var toDate = request.ToDate.ToDateTime(TimeOnly.MaxValue);

            var sql = @"
                        SELECT
                            SetOffDt, SetOffNo, LnNo,
                            DrTrnDate, DrTrnNo, DrTrnTyp, DrAmt,
                            CrTrnDt, CrTrnNo, CrTrnTyp, CrAmt,
                            SetOffAmt,
                            AdrNm, AdrCd,
                            ChqNo, ChqDt, BnkNm, ChqAmt,
                            DrAccCd, DrAccNm, CrAccCd, CrAccNm
                        FROM dbo.SetOffViewQry
                        WHERE SetOffDt >= @FromDate
                          AND SetOffDt <= @ToDate
                    ";

            var cmd = conn.CreateCommand();
            cmd.Parameters.Add(new SqlParameter("@FromDate", fromDate));
            cmd.Parameters.Add(new SqlParameter("@ToDate", toDate));

            if (request.AccKy.HasValue && request.AccKy.Value > 0)
            {
                sql += " AND AccKy = @AccKy";
                cmd.Parameters.Add(new SqlParameter("@AccKy", request.AccKy.Value));
            }

            sql += " ORDER BY SetOffDt, SetOffNo, LnNo";
            cmd.CommandText = sql;

            var rows = new List<SetOffDetailsReportRowDto>();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new SetOffDetailsReportRowDto
                {
                    SetOffDt = reader.IsDBNull(0) ? null : reader.GetDateTime(0),
                    SetOffNo = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    LnNo = reader.IsDBNull(2) ? null : reader.GetInt32(2),

                    DrTrnDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    DrTrnNo = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    DrTrnTyp = reader.IsDBNull(5) ? null : reader.GetString(5),
                    DrAmt = reader.IsDBNull(6) ? null : reader.GetDecimal(6),

                    CrTrnDt = reader.GetDateTime(7),
                    CrTrnNo = reader.GetInt32(8),
                    CrTrnTyp = reader.GetString(9),
                    CrAmt = reader.IsDBNull(10) ? null : reader.GetDecimal(10),

                    SetOffAmt = reader.IsDBNull(11) ? null : reader.GetDecimal(11),

                    AdrNm = reader.IsDBNull(12) ? null : reader.GetString(12),
                    AdrCd = reader.IsDBNull(13) ? null : reader.GetString(13),

                    ChqNo = reader.IsDBNull(14) ? null : reader.GetString(14),
                    ChqDt = reader.IsDBNull(15) ? null : reader.GetDateTime(15),
                    BnkNm = reader.IsDBNull(16) ? null : reader.GetString(16),
                    ChqAmt = reader.IsDBNull(17) ? null : reader.GetDecimal(17),

                    DrAccCd = reader.IsDBNull(18) ? null : reader.GetString(18),
                    DrAccNm = reader.IsDBNull(19) ? null : reader.GetString(19),
                    CrAccCd = reader.IsDBNull(20) ? null : reader.GetString(20),
                    CrAccNm = reader.IsDBNull(21) ? null : reader.GetString(21)
                });
            }

            return new SetOffDetailsReportResponseDto
            {
                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId,
                    ReportTitle = "SET OFF DETAILS REPORT"
                },
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                VarName = string.IsNullOrWhiteSpace(request.VarName)
                    ? "All Accounts"
                    : request.VarName,
                Rows = rows
            };
        }

        public async Task<GrossProfitItemLocationResponseDto> GetGrossProfitItemLocationAsync(GrossProfitItemLocationRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "sprGPSumItmNLocRpt";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@pFmDt", SqlDbType.SmallDateTime)
            {
                Value = request.FromDate.ToDateTime(TimeOnly.MinValue)
            });

            cmd.Parameters.Add(new SqlParameter("@pToDt", SqlDbType.SmallDateTime)
            {
                Value = request.ToDate.ToDateTime(TimeOnly.MaxValue)
            });

            cmd.Parameters.Add(new SqlParameter("@pCKy", SqlDbType.Int)
            {
                Value = _userContext.CompanyKey
            });

            var rows = new List<GrossProfitItemLocationRowDto>();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new GrossProfitItemLocationRowDto
                {
                    ItmKy = reader.GetInt32(reader.GetOrdinal("ItmKy")),
                    LocKy = reader.GetInt32(reader.GetOrdinal("LocKy")),
                    BUKy = reader.GetInt32(reader.GetOrdinal("BUKy")),
                    RepAdrKy = reader.GetInt32(reader.GetOrdinal("RepAdrKy")),
                    ItmTypKy = reader.GetInt32(reader.GetOrdinal("ItmTypKy")),

                    ItmCat1Ky = reader["ItmCat1Ky"] as int?,
                    ItmCat2Ky = reader["ItmCat2Ky"] as int?,
                    ItmCat3Ky = reader["ItmCat3Ky"] as int?,
                    ItmCat4Ky = reader["ItmCat4Ky"] as int?,
                    AdrKy = reader["AdrKy"] as int?,

                    ItmCd = reader["ItmCd"].ToString()!,
                    ItmNm = reader["ItmNm"].ToString()!,
                    LocCd = reader["LocCd"].ToString()!,
                    BUCd = reader["BUCd"].ToString()!,
                    ItmTypCd = reader["ItmTypCd"].ToString()!,
                    ItmCat1Cd = reader["ItmCat1Cd"].ToString()!,
                    ItmCat2Cd = reader["ItmCat2Cd"].ToString()!,
                    ItmCat3Cd = reader["ItmCat3Cd"].ToString()!,
                    ItmCat4Cd = reader["ItmCat4Cd"].ToString()!,
                    SlsRepAdrNm = reader["SlsRepAdrNm"].ToString()!,
                    AdrCd = reader["AdrCd"].ToString()!,

                    SalesAmount = reader["SalesAmt"] == DBNull.Value ? 0 : (decimal)reader["SalesAmt"],
                    CostAmount = reader["CostAmt"] == DBNull.Value ? 0 : (decimal)reader["CostAmt"],
                    GrossProfit = reader["GrossProfit"] == DBNull.Value ? 0 : (decimal)reader["GrossProfit"]
                });
            }

            var query = rows.AsQueryable();

            if (request.ItmKy.HasValue)
                query = query.Where(x => x.ItmKy == request.ItmKy.Value);

            if (request.LocKy.HasValue)
                query = query.Where(x => x.LocKy == request.LocKy.Value);

            if (request.BUKy.HasValue)
                query = query.Where(x => x.BUKy == request.BUKy.Value);

            if (request.RepAdrKy.HasValue)
                query = query.Where(x => x.RepAdrKy == request.RepAdrKy.Value);

            if (request.ItmTypKy.HasValue)
                query = query.Where(x => x.ItmTypKy == request.ItmTypKy.Value);

            if (request.ItmCat1Ky.HasValue)
                query = query.Where(x => x.ItmCat1Ky == request.ItmCat1Ky.Value);

            if (request.ItmCat2Ky.HasValue)
                query = query.Where(x => x.ItmCat2Ky == request.ItmCat2Ky.Value);

            if (request.ItmCat3Ky.HasValue)
                query = query.Where(x => x.ItmCat3Ky == request.ItmCat3Ky.Value);

            if (request.ItmCat4Ky.HasValue)
                query = query.Where(x => x.ItmCat4Ky == request.ItmCat4Ky.Value);

            if (request.AdrKy.HasValue)
                query = query.Where(x => x.AdrKy == request.AdrKy.Value);

            return new GrossProfitItemLocationResponseDto
            {
                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId,
                    ReportTitle = "GROSS PROFIT SUMMARY BY ITEM & LOCATION"
                },
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                GroupBy1 = request.GroupBy1 ?? string.Empty,
                GroupBy2 = request.GroupBy2 ?? string.Empty,
                Rows = query.ToList()
            };
        }

        public async Task<LssCreditInvoiceSummaryResponseDto>GetLssCreditInvoiceSummaryAsync(LssCreditInvoiceSummaryRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();

            DateTime fromDate = request.FromDate;
            DateTime toDate = request.ToDate;

            var query = db.LSSvewInvRpt
                .AsNoTracking()
                .Where(x =>
                    x.TrnDt >= fromDate &&
                    x.TrnDt <= toDate
                );

            if (request.AdrKy.HasValue)
            {
                query = query.Where(x => x.AdrKy == request.AdrKy.Value);
            }

            var rows = await query
                .OrderBy(x => x.TrnDt)
                .ThenBy(x => x.TrnNo)
                .Select(x => new LssCreditInvoiceSummaryRowDto
                {
                    TrnDt = x.TrnDt,
                    TrnNo = x.TrnNo,
                    DocNo = x.DocNo,
                    AdrKy = x.AdrKy,
                    AdrCd = x.AdrCd,
                    AdrNm = x.AdrNm,
                    Amt = x.Amt
                })
                .ToListAsync();

            return new LssCreditInvoiceSummaryResponseDto
            {
                FromDate = DateOnly.FromDateTime(fromDate),
                ToDate = DateOnly.FromDateTime(toDate),
                Rows = rows,
                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId
                }
            };
        }

        public async Task<AgeAnalysisResponseDto>GetAgeAnalysisAsync(AgeAnalysisRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();

            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "sprAgeAnl";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@pCKy", _userContext.CompanyKey));
            cmd.Parameters.Add(new SqlParameter("@pDt",
                request.AsAtDate.ToDateTime(TimeOnly.MaxValue)));
            cmd.Parameters.Add(new SqlParameter("@pAccTyp",
                request.ReportMode == "Cr" ? "SUP" : "CUS"));

            using var reader = await cmd.ExecuteReaderAsync();

            var rows = new List<AgeAnalysisRowDto>();

            while (await reader.ReadAsync())
            {
                rows.Add(new AgeAnalysisRowDto
                {
                    AccKy = reader.GetInt32(reader.GetOrdinal("AccKy")),
                    AccCd = reader["AccCd"] as string,
                    AccNm = reader["AccNm"] as string,
                    AreaNm = reader["AreaNm"] as string,
                    SlsRepAdrNm = reader["SlsRepAdrNm"] as string,
                    DueDt = reader.GetDateTime(reader.GetOrdinal("DueDt")),
                    Balance = reader.GetDecimal(reader.GetOrdinal("Balance"))
                });
            }

            // Apply VB RecordSelectionFormula logic AFTER fetch
            if (request.OverDueOnly)
            {
                DateTime asAt =
                    request.AsAtDate.ToDateTime(TimeOnly.MaxValue);

                rows = rows
                    .Where(x => x.DueDt <= asAt)
                    .ToList();
            }

            if (request.AccKy.HasValue)
                rows = rows.Where(x => x.AccKy == request.AccKy.Value).ToList();

            return new AgeAnalysisResponseDto
            {
                ReportTitle =
                    request.ReportMode == "Cr"
                        ? "CREDITORS' AGE ANALYSIS"
                        : "DEBTORS' AGE ANALYSIS",

                AsAtDate = request.AsAtDate,

                Rows = rows,

                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId
                }
            };
        }

        public async Task<DebtorsDueStatementResponseDto>GetDebtorsDueStatementAsync(DebtorsDueStatementRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();

            await conn.OpenAsync();

            // -------------------------------------------------
            // 1. Read ObjRem from vewObjPropDet
            // -------------------------------------------------
            string objRem;

            using (var objCmd = conn.CreateCommand())
            {
                objCmd.CommandText = @"
                                SELECT TOP 1 ObjRem
                                FROM vewObjPropDet
                                WHERE PrntObj = @PrntObj";

                objCmd.Parameters.Add(new SqlParameter("@PrntObj", request.FormTag));

                var result = await objCmd.ExecuteScalarAsync();

                objRem = result == null || result == DBNull.Value
                    ? "Credit limited to 30 days. On all overdues an additional 2% interest will be charged."
                    : result.ToString()!;
            }

            // -------------------------------------------------
            // 2. Execute sprAgeAnl
            // -------------------------------------------------
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "sprAgeAnl";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@pCKy", _userContext.CompanyKey));
            cmd.Parameters.Add(new SqlParameter(
                "@pDt",
                request.AsAtDate.ToDateTime(TimeOnly.MinValue)));
            cmd.Parameters.Add(new SqlParameter("@pAccTyp", "CUS"));

            var rows = new List<DebtorsDueStatementRowDto>();

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                decimal amount = reader.GetDecimal(reader.GetOrdinal("Amount"));

                // VB: Amount > 0
                if (amount <= 0)
                    continue;

                DateTime? dueDt = reader.IsDBNull(reader.GetOrdinal("DueDt"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("DueDt"));

                // VB: Overdue filter
                if (request.IsOverdue && dueDt.HasValue &&
                    dueDt.Value.Date > request.AsAtDate.ToDateTime(TimeOnly.MinValue))
                    continue;

                int accKy = reader.GetInt32(reader.GetOrdinal("AccKy"));

                if (request.AccKy.HasValue && accKy != request.AccKy.Value)
                    continue;

                string? trnTyp =
                    reader.IsDBNull(reader.GetOrdinal("TrnTypCd"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("TrnTypCd"));

                if (!string.IsNullOrEmpty(request.TrnTypCd) &&
                    trnTyp != request.TrnTypCd)
                    continue;

                rows.Add(new DebtorsDueStatementRowDto
                {
                    AccKy = accKy,
                    AccCd = reader.GetString(reader.GetOrdinal("AccCd")),
                    AccNm = reader.GetString(reader.GetOrdinal("AccNm")),
                    Amount = amount,
                    DueDt = dueDt == null
                        ? null
                        : DateOnly.FromDateTime(dueDt.Value),
                    TrnTypCd = trnTyp
                });
            }

            return new DebtorsDueStatementResponseDto
            {
                ReportTitle = request.IsOverdue
                    ? "DEBTORS' OVER DUE STATEMENT"
                    : "DEBTORS' DUE STATEMENT",

                AsAtDate = request.AsAtDate,
                ObjRemark = objRem,
                Rows = rows,

                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId
                }
            };
        }

        public async Task<StockLedgerResponseDto> GetStockLedgerAsync(StockLedgerRequestDto request)
        {
            if (request.LocKy <= 0)
                throw new Exception("Location is required");

            if (request.ItmKy <= 0)
                throw new Exception("Item is required");

            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            // -------------------------------------------------
            // 1. Opening Balance (sprItmOpBal)
            // -------------------------------------------------
            decimal openingBalance;

            using (var cmdOp = conn.CreateCommand())
            {
                cmdOp.CommandText = "sprItmOpBal";
                cmdOp.CommandType = CommandType.StoredProcedure;

                cmdOp.Parameters.Add(new SqlParameter("@pCKy", cKy));
                cmdOp.Parameters.Add(new SqlParameter(
                    "@pFrmDt",
                    SqlDbType.SmallDateTime)
                {
                    Value = request.FromDate.ToDateTime(TimeOnly.MinValue)
                });
                cmdOp.Parameters.Add(new SqlParameter("@pItmKy", request.ItmKy));
                cmdOp.Parameters.Add(new SqlParameter("@pLocKy", request.LocKy));

                var opBalParam = new SqlParameter("@pOpBal", SqlDbType.Decimal)
                {
                    Direction = ParameterDirection.Output,
                    Precision = 18,
                    Scale = 2
                };
                cmdOp.Parameters.Add(opBalParam);

                await cmdOp.ExecuteNonQueryAsync();

                openingBalance = opBalParam.Value == DBNull.Value
                    ? 0
                    : Convert.ToDecimal(opBalParam.Value);
            }

            // -------------------------------------------------
            // 2. Stock Ledger Rows (sprStkLdgrRpt)
            // -------------------------------------------------
            var rows = new List<StockLedgerRowDto>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "sprStkLdgrRpt";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@pCKy", cKy));

                cmd.Parameters.Add(new SqlParameter(
                    "@pFrmDt",
                    SqlDbType.SmallDateTime)
                {
                    Value = request.FromDate.ToDateTime(TimeOnly.MinValue)
                });

                cmd.Parameters.Add(new SqlParameter(
                    "@pToDt",
                    SqlDbType.SmallDateTime)
                {
                    Value = request.ToDate.ToDateTime(TimeOnly.MinValue)
                });

                cmd.Parameters.Add(new SqlParameter("@pLocKy", request.LocKy));

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    int itmKy = reader.GetInt32(reader.GetOrdinal("ItmKy"));

                    if (itmKy != request.ItmKy)
                        continue;

                    if (request.ItmTypKy.HasValue &&
                        reader.GetInt32(reader.GetOrdinal("ItmTypKy")) != request.ItmTypKy.Value)
                        continue;

                    rows.Add(new StockLedgerRowDto
                    {
                        TrnDt = DateOnly.FromDateTime(
                            reader.GetDateTime(reader.GetOrdinal("TrnDt"))),

                        ItmCd = reader.GetString(reader.GetOrdinal("ItmCd")),
                        ItmNm = reader.GetString(reader.GetOrdinal("ItmNm")),
                        Qty = reader.GetDecimal(reader.GetOrdinal("Qty")),
                        Balance = reader.GetDecimal(reader.GetOrdinal("Balance"))
                    });
                }
            }

            return new StockLedgerResponseDto
            {
                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId
                },
                OpeningBalance = openingBalance,
                Rows = rows
            };
        }

        public async Task<StockAsAtResponseDto> GetStockAsAtAsync(StockAsAtRequestDto request)
        {
            if (request.LocKy <= 0)
                throw new Exception("Location is required");

            if (!request.IsCurrentStockQty)
                throw new Exception("Only current stock quantity mode is supported");

            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();

            var query = db.Set<vewStkCurQtyRpt>()
                .AsNoTracking()
                .Where(x => x.CKy == cKy && x.LocKy == request.LocKy);

            if (request.BUKy.HasValue)
                query = query.Where(x => x.BUKy == request.BUKy.Value);

            if (request.ItmTypKy.HasValue)
                query = query.Where(x => x.ItmTypKy == request.ItmTypKy.Value);

            if (request.ItmCat1Ky.HasValue)
                query = query.Where(x => x.ItmCat1Ky == request.ItmCat1Ky.Value);

            if (request.ItmCat2Ky.HasValue)
                query = query.Where(x => x.ItmCat2Ky == request.ItmCat2Ky.Value);

            if (request.ItmCat3Ky.HasValue)
                query = query.Where(x => x.ItmCat3Ky == request.ItmCat3Ky.Value);

            if (request.ItmCat4Ky.HasValue)
                query = query.Where(x => x.ItmCat4Ky == request.ItmCat4Ky.Value);

            var rows = await query
                .OrderBy(x => x.ItmCd)
                .Select(x => new StockAsAtRowDto
                {
                    ItmCd = x.ItmCd,
                    ItmNm = x.ItmNm,
                    PartNo = x.PartNo,
                    Qty = x.Qty,
                    Unit = x.Unit,
                    ItmCosPri = x.ItmCosPri,
                    ItmSlsPri = x.ItmSlsPri,
                    LocNm = x.LocNm,
                    BUNm = x.BUNm,
                    ItmCat1Nm = x.ItmCat1Nm,
                    ItmCat2Nm = x.ItmCat2Nm,
                    ItmCat3Nm = x.ItmCat3Nm,
                    ItmCat4Nm = x.ItmCat4Nm
                })
                .ToListAsync();

            return new StockAsAtResponseDto
            {
                Rows = rows,
                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId
                }
            };
        }

        public async Task<ItemBatchReportResponseDto> GetItemBatchReportAsync(ItemBatchReportRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName = await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();

            IQueryable<vewItmBatch> query =
                db.Set<vewItmBatch>()
                  .Where(x => true);

            // ---------------------------------------------
            // Crystal RecordSelectionFormula conversion
            // ---------------------------------------------

            // {vewItmBatchRpt.ItmKy} = ?
            if (request.ItmKy.HasValue)
                query = query.Where(x => x.ItmKy == request.ItmKy.Value);

            // {vewItmBatchRpt.ExpirDt} <= Date(...)
            if (request.ExpiryDate.HasValue)
            {
                var expDate = request.ExpiryDate.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(x =>
                    x.ExpirDt.HasValue && x.ExpirDt.Value <= expDate);
            }

            var rows = await query
                .OrderBy(x => x.ItmCd)
                .ThenBy(x => x.BatchNo)
                .Select(x => new ItemBatchReportRowDto
                {
                    ItmKy = x.ItmKy,
                    ItmCd = x.ItmCd,
                    ItmNm = x.ItmNm,
                    PartNo = x.PartNo,

                    ItmBatchKy = x.ItmBatchKy,
                    BatchNo = x.BatchNo,

                    ExpirDt = x.ExpirDt.HasValue
                        ? DateOnly.FromDateTime(x.ExpirDt.Value)
                        : null,

                    CosPri = x.CosPri,
                    SalePri = x.SalePri,
                    Qty = x.Qty,
                    ItmLocQty = x.ItmLocQty
                })
                .ToListAsync();

            return new ItemBatchReportResponseDto
            {
                Rows = rows,
                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId
                }
            };
        }

        public async Task<ReOrderItemsResponseDto> GetReOrderItemsAsync(ReOrderItemsRequestDto request)
        {
            int cKy = _userContext.CompanyKey;
            string companyName =
                await _lookup.GetCompanyNameByCKyAsync(cKy) ?? "";

            using var db = await _factory.CreateDbContextAsync();

            // --------------------------------------------------
            // Base query (vewReOrdDet)
            // --------------------------------------------------
            var query = db.Set<vewReOrdDet>()
                .AsNoTracking()
                .Where(x =>
                    x.CKy == cKy &&
                    x.TrnDt >= request.FromDate.ToDateTime(TimeOnly.MinValue) &&
                    x.TrnDt <= request.ToDate.ToDateTime(TimeOnly.MaxValue) &&
                    x.ReOrdLvl >= x.ItmLocQty);

            // --------------------------------------------------
            // Filters (Crystal RecordSelectionFormula → LINQ)
            // --------------------------------------------------
            if (request.ItmKy.HasValue)
                query = query.Where(x => x.ItmKy == request.ItmKy.Value);

            if (request.ItmCat1Ky.HasValue)
                query = query.Where(x => x.ItmCat1Ky == request.ItmCat1Ky.Value);

            if (request.ItmCat2Ky.HasValue)
                query = query.Where(x => x.ItmCat2Ky == request.ItmCat2Ky.Value);

            // NOTE:
            // VB switches to vewReOrdDetBySup when AdrKy or ItmCat3Ky is used.
            // That requires a SECOND endpoint + entity.
            if (request.ItmCat3Ky.HasValue || request.AdrKy.HasValue)
                throw new Exception(
                    "Supplier-based reorder report requires vewReOrdDetBySup endpoint.");

            // --------------------------------------------------
            // Projection
            // --------------------------------------------------
            var rows = await query
                .OrderBy(x => x.ItmCd)
                .Select(x => new ReOrderItemsRowDto
                {
                    ItmKy = x.ItmKy,
                    ItmCd = x.ItmCd,
                    PartNo = x.PartNo,
                    ItmNm = x.ItmNm,
                    ReOrdLvl = x.ReOrdLvl,
                    ItmLocQty = x.ItmLocQty,
                    ReOrdQty = x.ReOrdQty,
                    ItmTyp = x.ItmTyp,
                    TrnNo = x.TrnNo,
                    TrnDt = DateOnly.FromDateTime(x.TrnDt)
                })
                .ToListAsync();

            // --------------------------------------------------
            // Response
            // --------------------------------------------------
            return new ReOrderItemsResponseDto
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Rows = rows,

                Context = new ReportContextDto
                {
                    CompanyName = companyName,
                    CurrentUserId = _userContext.UserId
                }
            };
        }

        public async Task<List<vewAdrDet>> GetAllAddressDetailsAsync()
        {
            using var db = await _factory.CreateDbContextAsync();

            var data = await db.Set<vewAdrDet>()
                .AsNoTracking()
                .Where(x => x.AdrTyp == "CUS")
                .OrderBy(x => x.AdrNm)
                .ToListAsync();

            return data;
        }

        public async Task<List<AccountAddressDto>> GetAccountAddressDetailsAsync()
        {
            using var db = await _factory.CreateDbContextAsync();

            var query =
                from accAdr in db.Set<vewAccAdrDet>().AsNoTracking()
                join adr in db.Set<vewAdrDet>().AsNoTracking()
                    on accAdr.AdrKy equals adr.AdrKy
                select new AccountAddressDto
                {
                    AccKy = accAdr.AccKy,
                    AccCd = accAdr.AccCd,
                    AccNm = accAdr.AccNm,
                    AccTyp = accAdr.AccTyp,

                    AdrKy = adr.AdrKy,
                    AdrCd = adr.AdrCd,
                    AdrNm = adr.AdrNm,
                    AdrTyp = adr.AdrTyp,
                    ActNm = accAdr.ActNm,

                    TP1 = adr.TP1,
                    TP2 = adr.TP2,
                    EMail = adr.EMail,
                    City = adr.City
                };

            return await query
                .OrderBy(x => x.AdrNm)
                .ThenBy(x => x.AccNm)
                .ToListAsync();
        }

        public async Task<List<vewItmMasVsf>> GetAllItemsVsfAsync()
        {
            using var db = await _factory.CreateDbContextAsync();

            var data = await db.Set<vewItmMasVsf>()
                .AsNoTracking()
                .Where(x => !x.fInAct)        // active items only
                .OrderBy(x => x.ItmNm)
                .ToListAsync();

            return data;
        }
    }
}
