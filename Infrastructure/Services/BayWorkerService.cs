using Application.DTOs.BayWorker;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class BayWorkerService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;

        public BayWorkerService(IDynamicDbContextFactory factory, IUserRequestContext userContext)
        {
            _factory = factory;
            _userContext = userContext;
        }

        public async Task<(bool success, string message, int workerKy)> AddWorkerToBayAsync(CreateBayWorkerDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            try
            {
                // Validate Bay
                var bay = await db.Bays.FindAsync(dto.BayKy);
                if (bay == null) return (false, "Bay not found", 0);

                // Validate User
                var user = await db.UsrMas.FindAsync(dto.UsrKy);
                if (user == null) return (false, "User not found", 0);

                // Check if already assigned and active
                var existing = await db.BayWorkers
                    .FirstOrDefaultAsync(w => w.BayKy == dto.BayKy && w.UsrKy == dto.UsrKy && !w.fInAct);
                
                if (existing != null)
                    return (false, "Worker is already assigned to this bay", existing.BayWorkerKy);

                var worker = new BayWorker
                {
                    BayKy = dto.BayKy,
                    UsrKy = dto.UsrKy,
                    Remarks = dto.Remarks,
                    fInAct = false,
                    EntDtm = DateTime.Now,
                    EntUsrKy = _userContext.UserId != null ? int.Parse(_userContext.UserId) : 1,
                    CKy = _userContext.CompanyKey
                };

                db.BayWorkers.Add(worker);
                await db.SaveChangesAsync();

                return (true, "Worker assigned successfully", worker.BayWorkerKy);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, 0);
            }
        }

        public async Task<List<BayWorkerDto>> GetAllWorkersAsync()
        {
            using var db = await _factory.CreateDbContextAsync();
            
            var query = from w in db.BayWorkers
                        join b in db.Bays on w.BayKy equals b.BayKy
                        join u in db.UsrMas on w.UsrKy equals u.UsrKy
                        where !w.fInAct
                        select new BayWorkerDto
                        {
                            BayWorkerKy = w.BayWorkerKy,
                            BayKy = w.BayKy,
                            BayNm = b.BayNm,
                            UsrKy = w.UsrKy,
                            UsrNm = u.UsrNm ?? "Unknown",
                            Remarks = w.Remarks,
                            IsActive = !w.fInAct
                        };

            return await query.ToListAsync();
        }

        public async Task<(bool success, string message)> UpdateWorkerInBayAsync(UpdateBayWorkerDto dto)
        {
             using var db = await _factory.CreateDbContextAsync();
             try
             {
                 var worker = await db.BayWorkers.FindAsync(dto.BayWorkerKy);
                 if (worker == null) return (false, "Record not found");

                 // Validate Bay & User
                 if (!await db.Bays.AnyAsync(b => b.BayKy == dto.BayKy))
                     return (false, "Bay not found");
                 if (!await db.UsrMas.AnyAsync(u => u.UsrKy == dto.UsrKy))
                     return (false, "User not found");

                 worker.BayKy = dto.BayKy;
                 worker.UsrKy = dto.UsrKy;
                 worker.Remarks = dto.Remarks;
                 // Assuming EntUsrKy update is not required for just update, or maybe track LastUpd?

                 await db.SaveChangesAsync();
                 return (true, "Worker assignment updated");
             }
             catch(Exception ex)
             {
                 return (false, ex.Message);
             }
        }

        public async Task<(bool success, string message)> DeleteWorkerFromBayAsync(int bayWorkerKy)
        {
            using var db = await _factory.CreateDbContextAsync();
            try
            {
                var worker = await db.BayWorkers.FindAsync(bayWorkerKy);
                if (worker == null) return (false, "Record not found");

                worker.fInAct = true; // Soft Delete
                await db.SaveChangesAsync();

                return (true, "Worker removed from bay");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
