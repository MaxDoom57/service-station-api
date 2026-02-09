using Application.DTOs.User;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class OrderService
    {

        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;
        private readonly CommonLookupService _lookup;
        private readonly IUserKeyService _userKeyService;
        private readonly IValidationService _validator;

        public OrderService(
        IDynamicDbContextFactory factory,
        IUserRequestContext userContext,
        CommonLookupService lookup,
        IUserKeyService userKeyService,
        IValidationService validator)
        {
            _factory = factory;
            _userContext = userContext;
            _lookup = lookup;
            _userKeyService = userKeyService;
            _validator = validator;
        }

        public async Task<List<UserLookupDto>> GetActiveUsersAsync()
        {
            using var db = await _factory.CreateDbContextAsync();

            var users = await db.UsrMas
                .Where(x => x.fInAct == false)
                .OrderBy(x => x.UsrId) // UsrMas has UsrId, OrdMas had OrdNo
                .ToListAsync();

            return users.Select(x => new UserLookupDto
            {
                UsrKy = x.UsrKy,
                UsrId = x.UsrId
            }).ToList();
        }
    }
}
