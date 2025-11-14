using SoowGood.Domain.Service.Models.UserInfo;
using SoowGood.Domain.Service.Models.UserRole;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGood.Domain.Service.Repositories.UserRole
{
    public interface IUserRoleQueryRepository
    {
        Task<UserRoleResponseDto> GetUserRoleByUserId(string userId);
        Task<List<UserRoleResponseDto>> GetAll();
        Task<Role> GetRoleByRoleName(string roleName);
    }
}
