using Application.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class ValidationService : IValidationService
    {
        private readonly IDynamicDbContextFactory _factory;

        public ValidationService(IDynamicDbContextFactory factory)
        {
            _factory = factory;
        }

        public async Task<bool> IsExistCompanyKey(int companyKey)
        {
            using var db = await _factory.CreateDbContextAsync();
            return await db.Company.AnyAsync(c => c.CKy == companyKey);
        }

        public async Task<bool> IsExistItemCode(string itemCode)
        {
            using var db = await _factory.CreateDbContextAsync();
            return await db.Items.AnyAsync(i => i.ItmCd == itemCode);
        }

        public async Task<bool> IsExistItemType(string itemType)
        {
            using var db = await _factory.CreateDbContextAsync();
            return await db.CdMas.AnyAsync(x => x.ConCd == "ItmTyp" && x.OurCd == itemType);
        }

        public async Task<bool> IsValidUnitKey(short unitKey)
        {
            using var db = await _factory.CreateDbContextAsync();
            return await db.Units.AnyAsync(u => u.UnitKy == unitKey);
        }

        public async Task<bool> IsValidUserKey(int userKey)
        {
            using var db = await _factory.CreateDbContextAsync();
            return await db.UsrMas.AnyAsync(u => u.UsrKy == userKey);
        }

        public async Task<bool> IsExistAdrNm(string adrNm)
        {
            using var db = await _factory.CreateDbContextAsync();
            return await db.Addresses.AnyAsync(u => u.AdrNm == adrNm);
        }

        public async Task<bool> IsValidTranDate (DateTime trnDt)
        {
            using var db = await _factory.CreateDbContextAsync();

            var currentDate = DateTime.UtcNow.Date.ToLocalTime();
            return trnDt.Date >= currentDate.AddDays(1)  && trnDt.Date <= currentDate.AddDays(-60) ;
        }
    }
}
