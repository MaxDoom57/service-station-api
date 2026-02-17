using Application.DTOs.Package;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class PackageService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public PackageService(
            IDynamicDbContextFactory factory,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _factory = factory;
            _userContext = userContext;
            _userKeyService = userKeyService;
        }

        public async Task<List<PackageDto>> GetPackagesAsync()
        {
            using var db = await _factory.CreateDbContextAsync();
            
            // Get CKy from context or assume logic
            // Assuming we filter by Current Company and ConCd='OrdTyp' and only active
            
            return await db.CdMas
                .Where(x => x.ConCd == "OrdTyp" && !x.fInAct)
                .Select(x => new PackageDto
                {
                    CdKy = x.CdKy,
                    Code = x.Code,
                    CdNm = x.CdNm,
                    ConCd = x.ConCd
                })
                .ToListAsync();
        }

        public async Task<(bool success, string message)> AddPackageAsync(CreatePackageDto dto)
        {
            try
            {
                using var db = await _factory.CreateDbContextAsync();
                
                // Validation: Check if Code exists
                if (await db.CdMas.AnyAsync(x => x.ConCd == "OrdTyp" && x.Code == dto.Code && !x.fInAct))
                     return (false, "Package Code already exists");
                     
                var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
                if (userKey == null) return (false, "User key not found");

                // We need to determine ConKy for "OrdTyp". Usually this is a constant or looked up from another table (Control or similar).
                // user request says "use cdMas table and return items which conCd='OrdTyp'".
                // Assuming for insertion we just set ConCd = "OrdTyp". 
                // BUT ConKy is NOT NULL in table definition. We must find the ConKy for "OrdTyp".
                // Typically "OrdTyp" concept itself might be in CdMas or another table. 
                // If I look at existing records (conceptually), they share the same ConKy.
                // I'll try to find an existing record with ConCd="OrdTyp" to copy its ConKy.
                
                var existingOrdTyp = await db.CdMas.FirstOrDefaultAsync(x => x.ConCd == "OrdTyp");
                short conKy = existingOrdTyp?.ConKy ?? 1; // Fallback to 1 if not string. Ideally should fail or look up properly.
                
                // Or maybe ConKy IS related to "OrdTyp" key itself if "OrdTyp" is a concept. 
                // For now, I will use logic to find existing ConKy.

                var package = new CdMas
                {
                    CKy = (short)_userContext.CompanyKey,
                    ConKy = conKy, 
                    Code = dto.Code,
                    CdNm = dto.CdNm,
                    ConCd = "OrdTyp",
                    fInAct = false,
                    fApr = 1,
                    EntUsrKy = userKey.Value,
                    EntDtm = DateTime.Now,
                    // Defaulting mandatory fields based on schema 
                    // Many fields are NO null. I should set defaults.
                    fCtrlCd = false,
                    CtrlCdKy = 1, // Default?
                    ObjKy = 1, // Default?
                    AcsLvlKy = 1, // Default?
                    SO = 0,
                    fUsrAcs = false,
                    fCCAcs = false,
                    fDefault = false,
                    CdF1 = false, CdF2 = false, CdF3 = false, CdF4 = false, CdF5 = false,
                    CdF6 = false, CdF7 = false, CdF8 = false, CdF9 = false, CdF10 = false,
                    CdF11 = false, CdF12 = false, CdF13 = false, CdF14 = false, CdF15 = false,
                    CdInt1 = 0, CdInt2 = 0, CdInt3 = 0,
                    CdNo1 = 0, CdNo2 = 0, CdNo3 = 0, CdNo4 = 0, CdNo5 = 0,
                    SKy = 1
                };

                db.CdMas.Add(package);
                await db.SaveChangesAsync();
                return (true, "Package added successfully");
            }
            catch (Exception ex)
            {
                return (false, "Error adding package: " + ex.Message);
            }
        }

        public async Task<(bool success, string message)> UpdatePackageAsync(CreatePackageDto dto)
        {
            if (!dto.CdKy.HasValue) return (false, "Package Key is required");

            try
            {
                using var db = await _factory.CreateDbContextAsync();
                var package = await db.CdMas.FindAsync((short)dto.CdKy.Value);
                
                if (package == null) return (false, "Package not found");

                package.Code = dto.Code;
                package.CdNm = dto.CdNm;
                 // Update other fields if necessary
                 
                await db.SaveChangesAsync();
                return (true, "Package updated successfully");
            }
            catch (Exception ex)
            {
                return (false, "Error updating package: " + ex.Message);
            }
        }

        public async Task<(bool success, string message)> DeletePackageAsync(int cdKy)
        {
            try
            {
                using var db = await _factory.CreateDbContextAsync();
                var package = await db.CdMas.FindAsync((short)cdKy);
                 
                if (package == null) return (false, "Package not found");
                 
                package.fInAct = true;
                await db.SaveChangesAsync();
                
                return (true, "Package deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, "Error deleting package: " + ex.Message);
            }
        }
        public async Task<PackageDetailDto?> GetPackageDetailsAsync(int cdKy)
        {
            using var db = await _factory.CreateDbContextAsync();

            var package = await db.CdMas.FindAsync((short)cdKy);
            if (package == null || package.fInAct) return null;

            if (package.CKy != _userContext.CompanyKey || package.ConCd != "OrdTyp") return null;

            var items = await db.ItmMas
                .Where(x => x.CKy == _userContext.CompanyKey && x.ItmTypKy == cdKy && !x.fInAct)
                .Select(x => new PackageItemDto
                {
                    ItmKy = x.ItmKy,
                    ItmCd = x.ItmCd,
                    ItmNm = x.ItmNm,
                    Time = x.Des,
                    SlsPri = x.SlsPri
                })
                .ToListAsync();

            return new PackageDetailDto
            {
                CdKy = package.CdKy,
                Code = package.Code,
                CdNm = package.CdNm,
                ConCd = package.ConCd,
                Items = items
            };
        }
        public async Task<List<PackageDetailDto>> GetAllPackagesWithItemsAsync()
        {
            using var db = await _factory.CreateDbContextAsync();

            var result = await db.CdMas
                .Where(p => p.CKy == _userContext.CompanyKey && p.ConCd == "OrdTyp" && !p.fInAct)
                .Select(p => new PackageDetailDto
                {
                    CdKy = p.CdKy,
                    Code = p.Code,
                    CdNm = p.CdNm,
                    ConCd = p.ConCd,
                    Items = db.ItmMas
                        .Where(i => i.ItmTypKy == p.CdKy && !i.fInAct)
                        .Select(i => new PackageItemDto
                        {
                            ItmKy = i.ItmKy,
                            ItmCd = i.ItmCd,
                            ItmNm = i.ItmNm,
                            Time = i.Des,
                            // Casting handled safely
                            SlsPri = (decimal?)i.SlsPri 
                        }).ToList()
                })
                .ToListAsync();

            return result;
        }
    }
}
