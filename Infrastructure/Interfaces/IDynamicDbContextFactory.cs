using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Context;

namespace Application.Interfaces
{
    public interface IDynamicDbContextFactory
    {
        Task<DynamicDbContext> CreateDbContextAsync();
    }
}
