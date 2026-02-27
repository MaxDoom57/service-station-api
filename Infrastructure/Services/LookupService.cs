using Application.DTOs.Lookups;
using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.Lookups;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class LookupService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;

        public LookupService(
            IDynamicDbContextFactory factory,
            IUserRequestContext userContext)
        {
            _factory = factory;
            _userContext = userContext;
        }

        // -------------------------------------------------------
        // Item Category 1
        // -------------------------------------------------------
        public async Task<List<ItemCategory1Dto>> GetItemCategory1Async()
        {
            using var db = await _factory.CreateDbContextAsync();

            return await db.Set<vewItmCat1Cd>()
                .AsNoTracking()
                .Where(x => x.CKy == _userContext.CompanyKey)
                .OrderBy(x => x.ItmCat1Nm)
                .Select(x => new ItemCategory1Dto
                {
                    ItmCat1Ky = x.ItmCat1Ky,   // NO CAST
                    ItmCat1Cd = x.ItmCat1Cd,
                    ItmCat1Nm = x.ItmCat1Nm,
                    CKy = x.CKy
                })
                .ToListAsync();
        }

        // -------------------------------------------------------
        // Item Category 2
        // -------------------------------------------------------
        public async Task<List<ItemCategory2Dto>> GetItemCategory2Async()
        {
            using var db = await _factory.CreateDbContextAsync();

            return await db.Set<vewItmCat2Cd>()
                .AsNoTracking()
                .Where(x => x.CKy == _userContext.CompanyKey)
                .OrderBy(x => x.ItmCat2Nm)
                .Select(x => new ItemCategory2Dto
                {
                    ItmCat2Ky = x.ItmCat2Ky,
                    ItmCat2Cd = x.ItmCat2Cd,
                    ItmCat2Nm = x.ItmCat2Nm,
                    CKy = x.CKy,
                    fUsrAcs = x.fUsrAcs
                })
                .ToListAsync();
        }

        // -------------------------------------------------------
        // Item Category 3
        // -------------------------------------------------------
        public async Task<List<ItemCategory3Dto>> GetItemCategory3Async()
        {
            using var db = await _factory.CreateDbContextAsync();

            return await db.Set<vewItmCat3Cd>()
                .AsNoTracking()
                .Where(x => x.CKy == _userContext.CompanyKey)
                .OrderBy(x => x.ItmCat3Nm)
                .Select(x => new ItemCategory3Dto
                {
                    ItmCat3Ky = x.ItmCat3Ky,
                    ItmCat3Cd = x.ItmCat3Cd,
                    ItmCat3Nm = x.ItmCat3Nm,
                    CKy = x.CKy
                })
                .ToListAsync();
        }

        // -------------------------------------------------------
        // Item Category 4
        // -------------------------------------------------------
        public async Task<List<ItemCategory4Dto>> GetItemCategory4Async()
        {
            using var db = await _factory.CreateDbContextAsync();

            return await db.Set<vewItmCat4Cd>()
                .AsNoTracking()
                .Where(x => x.CKy == _userContext.CompanyKey)
                .OrderBy(x => x.ItmCat4Nm)
                .Select(x => new ItemCategory4Dto
                {
                    ItmCat4Ky = x.ItmCat4Ky,
                    ItmCat4Cd = x.ItmCat4Cd,
                    ItmCat4Nm = x.ItmCat4Nm,
                    CKy = x.CKy
                })
                .ToListAsync();
        }

        // -------------------------------------------------------
        // TrnNo Last
        // -------------------------------------------------------
        public async Task<int> GetLastTransactionNoAsync(GetLastTrnNoRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OurCd))
                throw new ArgumentException("OurCd is required");

            using var db = await _factory.CreateDbContextAsync();

            var record = await db.TrnNoLst
                .Where(x =>
                    x.OurCd == request.OurCd &&
                    x.CKy == _userContext.CompanyKey &&
                    x.fInAct == false)
                .OrderByDescending(x => x.Period)
                .FirstOrDefaultAsync();

            // If record exists → return last transaction no
            if (record != null)
                return record.LstTrnNo;

            // If not exists → create record and return 0
            var newRecord = new TrnNoLst
            {
                fInAct = false,
                Status = "A",
                CKy = (short)_userContext.CompanyKey,
                SKy = 0,
                Period = DateTime.Now.Year,
                OurCd = request.OurCd,
                CdKy = 0,
                LstTrnNo = 0,
                LstDocNo = null
            };

            db.TrnNoLst.Add(newRecord);
            await db.SaveChangesAsync();

            return 0;
        }
    }
}
