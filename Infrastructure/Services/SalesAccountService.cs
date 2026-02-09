using Application.DTOs.Invoice;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class SalesAccountService
    {
        private readonly IDynamicDbContextFactory _factory;

        public SalesAccountService(IDynamicDbContextFactory factory)
        {
            _factory = factory;
        }

        public async Task<List<SalesAccountDto>> GetAllAsync()
        {
            using var db = await _factory.CreateDbContextAsync();

            return await db.SalesAccounts
                .Where(x => x.AccTyp == "SALE")
                .Select(x => new SalesAccountDto
                {
                    AccKy = x.AccKy,
                    AccCd = x.AccCd,
                    AccNm = x.AccNm
                })
                .ToListAsync();
        }
    }
}
