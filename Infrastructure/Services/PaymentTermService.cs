using Application.DTOs.Invoice;
using Application.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class PaymentTermService
    {
        private readonly IDynamicDbContextFactory _factory;

        public PaymentTermService(IDynamicDbContextFactory factory)
        {
            _factory = factory;
        }

        public async Task<List<PaymentTermDto>> GetAllAsync()
        {
            using var db = await _factory.CreateDbContextAsync();

            return await db.PaymentTerms
                .Select(x => new PaymentTermDto
                {
                    PmtTrmKy = x.PmtTrmKy,
                    PmtTrmCd = x.PmtTrmCd,
                    PmtTrmNm = x.PmtTrmNm,
                    PmtTrm = x.PmtTrm
                })
                .ToListAsync();
        }
    }
}
