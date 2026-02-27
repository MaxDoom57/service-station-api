using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IValidationService
    {
        Task<bool> IsExistCompanyKey(int companyKey);
        Task<bool> IsExistItemCode(string itemCode);
        Task<bool> IsExistItemType(string itemType);
        Task<bool> IsValidUnitKey(short unitKey);
        Task<bool> IsValidUserKey(int userKey);
        Task<bool> IsExistAdrNm(string adrNm);
    }
}
