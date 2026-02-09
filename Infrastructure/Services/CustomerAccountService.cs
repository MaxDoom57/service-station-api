using Application.DTOs.Invoice;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class CustomerAccountService
    {
        private readonly IDynamicDbContextFactory _factory;

        public CustomerAccountService(IDynamicDbContextFactory factory)
        {
            _factory = factory;
        }

        public async Task<List<CustomerAccountDto>> GetAllAsync()
        {
            try
            {
                using var db = await _factory.CreateDbContextAsync();

                return await db.CustomerAccounts
                    .Select(x => new CustomerAccountDto
                    {
                        CusAccKy = x.CusAccKy,
                        CusAccCd = x.CusAccCd,
                        CusAccNm = x.CusAccNm
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
