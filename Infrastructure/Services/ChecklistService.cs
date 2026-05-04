using Application.DTOs.Checklist;
using Application.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ChecklistService
    {
        private readonly IDynamicDbContextFactory _factory;

        public ChecklistService(IDynamicDbContextFactory factory)
        {
            _factory = factory;
        }

        public async Task<List<ChecklistItemDto>> GetChecklistItemsAsync()
        {
            using var db = await _factory.CreateDbContextAsync();
            
            var items = await db.CdMas
                .Where(c => c.ConCd == "SvsList" && !c.fInAct)
                .Select(c => new ChecklistItemDto
                {
                    CdKy = c.CdKy,
                    CKy = c.CKy,
                    Code = c.Code,
                    CdNm = c.CdNm
                })
                .ToListAsync();

            return items;
        }

        public async Task<(bool success, string message)> CreateChecklistAsync(SaveChecklistDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();

            try
            {
                foreach (var item in dto.ChecklistItems)
                {
                    db.SvschkList.Add(new Domain.Entities.SvschkList
                    {
                        OrdKy = dto.OrdKy,
                        CdKy = item.CdKy,
                        bitValue1 = item.BitValue1,
                        Val1 = item.Value,
                        Remarks = item.Remarks
                    });
                }

                await db.SaveChangesAsync();
                return (true, "Checklist created successfully");
            }
            catch (System.Exception ex)
            {
                var inner = ex.InnerException != null ? ex.InnerException.Message : "";
                return (false, $"Error creating checklist: {ex.Message} | SQL Error: {inner}");
            }
        }

        public async Task<(bool success, string message)> UpdateChecklistAsync(int ordKy, List<ChecklistValueDto> items)
        {
            using var db = await _factory.CreateDbContextAsync();

            try
            {
                foreach (var item in items)
                {
                    var existing = await db.SvschkList
                        .FirstOrDefaultAsync(x => x.OrdKy == ordKy && x.CdKy == item.CdKy);

                    if (existing != null)
                    {
                        existing.bitValue1 = item.BitValue1;
                        existing.Val1 = item.Value;
                        existing.Remarks = item.Remarks;
                    }
                }

                await db.SaveChangesAsync();
                return (true, "Checklist updated successfully");
            }
            catch (System.Exception ex)
            {
                var inner = ex.InnerException != null ? ex.InnerException.Message : "";
                return (false, $"Error updating checklist: {ex.Message} | SQL Error: {inner}");
            }
        }

        public async Task<(bool success, string message)> CreateChecklistMasterAsync(CreateChecklistMasterDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();

            try
            {
                // Auto generate Code
                var svsItems = await db.CdMas
                    .Where(c => c.ConCd == "SvsList" && c.Code.StartsWith("SVS"))
                    .Select(c => c.Code)
                    .ToListAsync();

                int nextNum = 1;
                if (svsItems.Any())
                {
                    var maxNum = svsItems
                        .Select(c => {
                            if (int.TryParse(c.Substring(3), out int n)) return n;
                            return 0;
                        })
                        .Max();
                    nextNum = maxNum + 1;
                }

                string newCode = $"SVS{nextNum:D3}";

                // Ensure it's unique (just in case of gaps)
                while (await db.CdMas.AnyAsync(c => c.ConCd == "SvsList" && c.Code == newCode))
                {
                    nextNum++;
                    newCode = $"SVS{nextNum:D3}";
                }

                var maxSO = await db.CdMas.Where(c => c.ConCd == "SvsList").MaxAsync(c => (float?)c.SO) ?? 0;

                var newCdMas = new Domain.Entities.CdMas
                {
                    CKy = 1,
                    ConKy = 365,
                    Code = newCode,
                    fInAct = false,
                    fApr = 1,
                    ConCd = "SvsList",
                    CdNm = dto.CdNm,
                    fCtrlCd = false,
                    CtrlCdKy = 0,
                    OurCd = "SvsList",
                    ObjKy = 0,
                    AcsLvlKy = 0,
                    SO = maxSO + 1,
                    fUsrAcs = true,
                    fCCAcs = false,
                    fDefault = false,
                    SKy = 1,
                    EntUsrKy = 1, // Using 1 for system/admin
                    EntDtm = DateTime.Now
                };

                db.CdMas.Add(newCdMas);
                await db.SaveChangesAsync();

                return (true, "Checklist item created successfully");
            }
            catch (System.Exception ex)
            {
                return (false, "Error creating checklist item: " + ex.Message);
            }
        }
    }
}
