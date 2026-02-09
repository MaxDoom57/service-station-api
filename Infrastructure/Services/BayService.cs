using Application.DTOs.Bay;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class BayService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public BayService(
            IDynamicDbContextFactory factory,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _factory = factory;
            _userContext = userContext;
            _userKeyService = userKeyService;
        }

        public async Task<List<BayDto>> GetActiveBaysAsync()
        {
            using var db = await _factory.CreateDbContextAsync();
            Console.WriteLine("CKY===========================" + _userContext.CompanyKey);
            return await db.Bays
                .Where(x => x.CKy == _userContext.CompanyKey && !x.fInAct)
                .Select(x => new BayDto
                {
                    BayKy = x.BayKy,
                    BayCd = x.BayCd,
                    BayNm = x.BayNm,
                    IsReservationAvailable = x.IsReservationAvailable,
                    Description = x.Description
                })
                .ToListAsync();
        }

        public async Task<(bool success, string message)> AddBayAsync(CreateBayDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            
            if (await db.Bays.AnyAsync(x => x.CKy == _userContext.CompanyKey && x.BayCd == dto.BayCd && !x.fInAct))
                 return (false, "Bay Code already exists");
                 
            var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
            if (userKey == null) return (false, "User key not found");

            var bay = new Bay
            {
                CKy = (short)_userContext.CompanyKey,
                BayCd = dto.BayCd,
                BayNm = dto.BayNm,
                IsReservationAvailable = dto.IsReservationAvailable,
                Description = dto.Description,
                fInAct = false,
                EntUsrKy = userKey.Value,
                EntDtm = DateTime.Now
            };

            db.Bays.Add(bay);
            await db.SaveChangesAsync();
            return (true, "Bay added successfully");
        }

        public async Task<(bool success, string message)> UpdateBayAsync(UpdateBayDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            var bay = await db.Bays.FindAsync(dto.BayKy);
            
            if (bay == null) return (false, "Bay not found");
            if (bay.CKy != _userContext.CompanyKey) return (false, "Unauthorized access to this Bay");

            bay.BayCd = dto.BayCd;
            bay.BayNm = dto.BayNm;
            bay.IsReservationAvailable = dto.IsReservationAvailable;
            bay.Description = dto.Description;
             
            await db.SaveChangesAsync();
            return (true, "Bay updated successfully");
        }

        public async Task<(bool success, string message)> DeleteBayAsync(int bayKy)
        {
            using var db = await _factory.CreateDbContextAsync();
            var bay = await db.Bays.FindAsync(bayKy);
             
            if (bay == null) return (false, "Bay not found");
            if (bay.CKy != _userContext.CompanyKey) return (false, "Unauthorized access to this Bay");

            bay.fInAct = true;
            await db.SaveChangesAsync();
            
            return (true, "Bay deleted successfully");
        }
    }
}
