using Application.DTOs.VehicleType;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class VehicleTypeService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;
        private const string ConCd = "OrdCat1"; // As per request

        public VehicleTypeService(
            IDynamicDbContextFactory factory,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _factory = factory;
            _userContext = userContext;
            _userKeyService = userKeyService;
        }

        public async Task<List<VehicleTypeDto>> GetVehicleTypesAsync()
        {
            using var db = await _factory.CreateDbContextAsync();
            
            return await db.CdMas
                .Where(x =>  x.ConCd == ConCd && !x.fInAct)
                .Select(x => new VehicleTypeDto
                {
                    CdKy = x.CdKy,
                    Code = x.Code,
                    CdNm = x.CdNm
                })
                .ToListAsync();
        }

        public async Task<(bool success, string message)> AddVehicleTypeAsync(CreateVehicleTypeDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            
            try
            {
                // Validation
                if (await db.CdMas.AnyAsync(x => x.CKy == _userContext.CompanyKey && x.ConCd == ConCd && x.Code == dto.Code && !x.fInAct))
                     return (false, "Vehicle Type Code already exists");
                     
                var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
                if (userKey == null) return (false, "User key not found");

                // Look up ConKy for OrdCat1 from Control Table
                var control = await db.Control.FirstOrDefaultAsync(x => x.ConCd == ConCd && x.CKy == _userContext.CompanyKey);
                if (control == null) return (false, $"Control Code '{ConCd}' not found");

                short conKy = (short)control.ConKy;

                var vehicleType = new CdMas
                {
                    CKy = (short)_userContext.CompanyKey,
                    ConKy = conKy, 
                    Code = dto.Code,
                    CdNm = dto.CdNm,
                    ConCd = ConCd,
                    fInAct = false,
                    fApr = 1,
                    EntUsrKy = userKey.Value,
                    EntDtm = DateTime.Now,
                    // Defaults
                    fCtrlCd = false, CtrlCdKy = 1, ObjKy = 1, AcsLvlKy = 1, SO = 0,
                    fUsrAcs = false, fCCAcs = false, fDefault = false,
                    CdF1=false, CdF2=false, CdF3=false, CdF4=false, CdF5=false,
                    CdF6=false, CdF7=false, CdF8=false, CdF9=false, CdF10=false,
                    CdF11=false, CdF12=false, CdF13=false, CdF14=false, CdF15=false,
                    CdInt1=0, CdInt2=0, CdInt3=0, CdNo1=0, CdNo2=0, CdNo3=0, CdNo4=0, CdNo5=0,
                    SKy=1
                };

                db.CdMas.Add(vehicleType);
                await db.SaveChangesAsync();
                return (true, "Vehicle Type added successfully");
            }
            catch (Exception ex)
            {
                return (false, "Error adding Vehicle Type: " + ex.Message);
            }
        }

        public async Task<(bool success, string message)> UpdateVehicleTypeAsync(CreateVehicleTypeDto dto)
        {
            if (!dto.CdKy.HasValue) return (false, "Vehicle Type Key is required");

            try
            {
                using var db = await _factory.CreateDbContextAsync();
                var vt = await db.CdMas.FindAsync((short)dto.CdKy.Value);
                
                if (vt == null) return (false, "Vehicle Type not found");

                vt.Code = dto.Code;
                vt.CdNm = dto.CdNm;
                 
                await db.SaveChangesAsync();
                return (true, "Vehicle Type updated successfully");
            }
            catch (Exception ex)
            {
                return (false, "Error updating Vehicle Type: " + ex.Message);
            }
        }

        public async Task<(bool success, string message)> DeleteVehicleTypeAsync(int cdKy)
        {
            try
            {
                using var db = await _factory.CreateDbContextAsync();
                var vt = await db.CdMas.FindAsync((short)cdKy);
                 
                if (vt == null) return (false, "Vehicle Type not found");
                 
                vt.fInAct = true;
                await db.SaveChangesAsync();
                
                return (true, "Vehicle Type deleted successfully");
            }
            catch (Exception ex)
            {
                 return (false, "Error deleting Vehicle Type: " + ex.Message);
            }
        }
    }
}
