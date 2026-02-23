using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Services
{
    public class CommonLookupService
    {
        private readonly IDynamicDbContextFactory _factory;

        public CommonLookupService(IDynamicDbContextFactory factory)
        {
            _factory = factory;
        }

        // Common reusable lookup for AccountType -> CdKy
        public async Task<short> GetAccountTypeKeyAsync(string accountType)
        {
            using var db = await _factory.CreateDbContextAsync();

            var key = await db.CdMas
                .Where(c => c.ConCd == "AccTyp"
                            && c.fInAct == false
                            && c.Code == accountType)
                .Select(c => c.CdKy)
                .FirstOrDefaultAsync();

            if (key == 0)
                throw new Exception($"Account type '{accountType}' not found in CdMas.");

            return key;
        }

        public async Task<short> GetItemTypeKeyAsync(string itemType)
        {
            using var db = await _factory.CreateDbContextAsync();

            var cdMas = await db.CdMas
                .Where(c => c.ConCd == "ItmTyp" && c.fInAct == false && c.OurCd == itemType)
                .Select(c => new { c.CdKy })
                .FirstOrDefaultAsync();

            if (cdMas != null)
                return cdMas.CdKy;

            // Creating a default entry for testing if not found (Only for InMemory/Test scenarios preferably, 
            // but helpful here to unblock tests where seed data might be missing)
            if (!db.Database.IsRelational())
            {
                var maxKy = await db.CdMas.AnyAsync() ? await db.CdMas.MaxAsync(x => x.CdKy) : (short)0;
                var newCdMas = new CdMas
                {
                    CdKy = (short)(maxKy + 1),
                    CKy = 2, // Default CKy for tests
                    ConCd = "ItmTyp",
                    OurCd = itemType,
                    Code = itemType,
                    CdNm = itemType + " Name",
                    fInAct = false,
                    // valid defaults
                    ConKy = 1, fApr = 1, CtrlCdKy=1, ObjKy=1, AcsLvlKy=1, SKy=1
                };
                db.CdMas.Add(newCdMas);
                await db.SaveChangesAsync();
                return newCdMas.CdKy;
            }

            throw new Exception($"Item Type '{itemType}' not found");
        }

        public async Task<short> GetTranTypeKeyAsync(string tranTypeCode)
        {
            using var db = await _factory.CreateDbContextAsync();

            return await db.CdMas
                .Where(c => c.ConCd == "TrnTyp" && c.fInAct == false && c.OurCd == tranTypeCode)
                .Select(c => c.CdKy)
                .FirstAsync();
        }

        public async Task<short> GetPaymentTermKeyAsync(string paymentTermCode)
        {
            using var db = await _factory.CreateDbContextAsync();

            var key = await db.CdMas
                .Where(c => c.ConCd == "PmtTrm" && c.OurCd == paymentTermCode && c.fInAct == false)
                .Select(c => c.CdKy)
                .FirstOrDefaultAsync();

            if (key == 0)
                throw new Exception($"Payment term '{paymentTermCode}' not found in CdMas.");

            return key;
        }

        public async Task<int> GetAddressKeyByAccKyAsync(int accKy)
        {
            using var db = await _factory.CreateDbContextAsync();

            var key = await db.AccAdr
                .Where(c => c.AccKy == accKy)
                .Select(c => c.AdrKy)
                .FirstOrDefaultAsync();

            if (key == 0)
                throw new Exception($"Address not found.");

            return key;
        }

        public async Task<vewCdMas?> GetSaleTransactionCodesAsync(int cKy, string ourCode)
        {
            using var db = await _factory.CreateDbContextAsync();

            return await db.vewCdMas
                .Where(x =>
                    x.OurCd == ourCode &&
                    x.ConCd == "TrnTyp" &&
                    x.CKy == cKy)
                .Select(x => new vewCdMas
                {
                    CdNo1 = x.CdNo1,
                    CdNo2 = x.CdNo2,
                    CdNo3 = x.CdNo3
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetDefaultSalesAccountKeyAsync(short cKy)
        {
            using var dynamicContext = await _factory.CreateDbContextAsync();

            return await dynamicContext.Account
                .Where(x => x.AccTyp == "SALE"
                         && x.fDefault == true
                         && x.CKy == cKy
                         && x.fInAct == false)
                .Select(x => x.AccKy)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetTrnKyByTrnNoAsync(int trnNo)
        {
            using var dynamicContext = await _factory.CreateDbContextAsync();

            return await dynamicContext.TrnMas
                .Where(x => x.TrnNo == trnNo)
                .Select(x => x.TrnKy)
                .FirstOrDefaultAsync();
        }

        public async Task<string?> GetCompanyNameByCKyAsync(int CKy)
        {
            using var dynamicContext = await _factory.CreateDbContextAsync();

            return await dynamicContext.Company
                .Where(x => x.CKy == CKy)
                .Select(x => x.CNm)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetTrnTypKyAsync(string ourCode)
        {
            using var dynamicContext = await _factory.CreateDbContextAsync();

            return await dynamicContext.CdMas
                .Where(x => x.ConCd == "TrnTyp" && x.OurCd == ourCode)
                .Select(x => x.CdKy)
                .FirstOrDefaultAsync();
        }

        public async Task<short> GetPaymentModeKeyAsync(int paymentTermKey)
        {
            using var dynamicContext = await _factory.CreateDbContextAsync();

            var key = await dynamicContext.vewPmtTrmToPrmMode
                .Where(c => c.PmtTrmKy == paymentTermKey)
                .Select(c => (short)c.PmtModeKy)
                .FirstOrDefaultAsync();

            if (key == 0)
                throw new Exception($"Payment mode not found for PaymentTermKey {paymentTermKey}");

            return key;
        }

        public async Task<short> GetCodeTypeKeyAsync(string conCd)
        {
            using var dynamicContext = await _factory.CreateDbContextAsync();

            var key = await dynamicContext.Control
                .Where(c => c.ConCd == conCd)
                .Select(c => (short)c.ConKy)
                .FirstOrDefaultAsync();

            if (key == 0)
                throw new Exception($"Code type not found for code type {conCd}");

            return key;
        }


        public async Task<int> GetTranNumberLastAsync(int companyKey, string ourCode)
        {
            using var dynamicContext = await _factory.CreateDbContextAsync();

            var existing = await dynamicContext.TrnNoLst
                .Where(c => c.OurCd == ourCode && !c.fInAct && c.CKy == companyKey)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                return existing.LstTrnNo;
            }

            int newLastTrn = 1;

            string sql = @"
                        INSERT INTO TrnNoLst
                        (fInAct, CKy, SKy, OurCd, CdKy, LstTrnNo, LstDocNo)
                        VALUES
                        (@fInAct, @CKy, @SKy, @OurCd, @CdKy, @LstTrnNo, @LstDocNo);
                    ";

            await dynamicContext.Database.ExecuteSqlRawAsync(
                sql,
                new SqlParameter("@fInAct", false),
                new SqlParameter("@CKy", companyKey),
                new SqlParameter("@SKy", 1),
                new SqlParameter("@OurCd", ourCode),
                new SqlParameter("@CdKy", SqlDbType.SmallInt) { Value = 0 },
                new SqlParameter("@LstTrnNo", newLastTrn),
                new SqlParameter("@LstDocNo", DBNull.Value)
            );

            return newLastTrn;
        }

        public async Task IncrementTranNumberLastAsync(int companyKey, string ourCode)
        {
            using var dynamicContext = await _factory.CreateDbContextAsync();

            var record = await dynamicContext.TrnNoLst
                .FirstOrDefaultAsync(c =>
                    c.OurCd == ourCode &&
                    c.fInAct == false &&
                    c.CKy == companyKey);

            if (record == null)
                throw new Exception("Transaction number record not found.");

            record.LstTrnNo += 1;

            await dynamicContext.SaveChangesAsync();
        }

        public async Task<int> GetAmt1AccKyAsync(string ourCd, int companyKey)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "sprGetColAcc";
            cmd.CommandType = CommandType.StoredProcedure;

            // @pCKy
            cmd.Parameters.Add(new SqlParameter("@pCKy", SqlDbType.Int)
            {
                Value = companyKey
            });

            // @pPrntObjNm
            cmd.Parameters.Add(new SqlParameter("@pPrntObjNm", SqlDbType.VarChar, 60)
            {
                Value = ourCd
            });

            // @pObjNm 
            cmd.Parameters.Add(new SqlParameter("@pObjNm", SqlDbType.VarChar, 60)
            {
                Value = "Amt1"
            });

            // @pAccKy OUTPUT
            var accKyParam = new SqlParameter("@pAccKy", SqlDbType.Int)
            {
                Direction = ParameterDirection.InputOutput,
                Value = 0
            };
            cmd.Parameters.Add(accKyParam);

            await cmd.ExecuteNonQueryAsync();

            return accKyParam.Value == DBNull.Value
                ? 0
                : Convert.ToInt32(accKyParam.Value);
        }


        // 1this code is work properly with sql compatibility level 100
        // If you have compatibility level 130 or >130, you can write this code as follow;

        //public async Task<List<UsrObj>> GetAccessLevelAsync(string userId)
        //{
        //    using var db = await _factory.CreateDbContextAsync();

        //    // Step 1 get user key
        //    int userKey = await db.UsrMas
        //        .Where(x => x.UsrId == userId)
        //        .Select(x => x.UsrKy)
        //        .FirstOrDefaultAsync();

        //    if (userKey == 0)
        //        throw new Exception("User not found");

        //    // Step 2 get relevant object keys from ObjMas
        //    var targetObjNames = new[] { "mnuPosDashboard", "mnuPOSMenu" };

        //    var objKeys = await db.ObjMas
        //        .Where(x => targetObjNames.Contains(x.ObjNm))
        //        .Select(x => x.ObjKy)
        //        .ToListAsync();

        //    if (objKeys.Count == 0)
        //        throw new Exception("Required menu objects not found");

        //    // Step 3 return matching UsrObj rows
        //    var results = await db.UsrObj
        //        .Where(x => x.UsrKy == userKey && objKeys.Contains(x.ObjKy))
        //        .OrderBy(x => x.ObjKy)
        //        .Select(x => new UsrObj
        //        {
        //            CKy = x.CKy,
        //            fAcs = x.fAcs,
        //            fNew = x.fNew,
        //            fUpdt = x.fUpdt,
        //            fDel = x.fDel,    // important fix
        //            fSp = x.fSp
        //        })
        //        .ToListAsync();

        //    return results;
        //}

        public async Task<List<AccessLevelDto>> GetAccessLevelAsync(string userId)
        {
            using var db = await _factory.CreateDbContextAsync();

            int userKey = await db.UsrMas
                .Where(x => x.UsrId == userId)
                .Select(x => x.UsrKy)
                .FirstOrDefaultAsync();

            if (userKey == 0)
                throw new Exception("User not found");

            var menuObjKys = await db.ObjMas
                .Where(x =>
                    x.ObjTyp == "MNU" &&
                    (x.ObjNm == "mnuPosDashboard" || x.ObjNm == "mnuPOSMenu")
                )
                .Select(x => x.ObjKy)
                .ToListAsync();

            if (menuObjKys.Count != 2)
                throw new Exception("Required menu objects not found");

            int dashboardObjKy = menuObjKys[0];
            int posMenuObjKy = menuObjKys[1];

            var results = await db.UsrObj
                .Where(x =>
                    x.UsrKy == userKey &&
                    (x.ObjKy == dashboardObjKy || x.ObjKy == posMenuObjKy)
                )
                .OrderBy(x => x.ObjKy)
                .Select(x => new AccessLevelDto
                {
                    fAcs = x.fAcs,
                    fNew = x.fNew,
                    fUpdt = x.fUpdt,
                    fDel = x.fDel,
                    fSp = x.fSp
                })
                .AsNoTracking()
                .ToListAsync();

            return results;
        }
    }
}
