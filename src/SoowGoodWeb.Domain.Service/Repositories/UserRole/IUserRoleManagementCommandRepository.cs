using SoowGood.Domain.Service.Repositories.BaseInterface;
using SoowGoodWeb.Domain.Service.Models.UserRole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Domain.Service.Repositories.UserRole
{
    public interface IUserRoleManagementCommandRepository : IBaseCommandRepository<UserRoleInsertDto>
    {
    }
}
