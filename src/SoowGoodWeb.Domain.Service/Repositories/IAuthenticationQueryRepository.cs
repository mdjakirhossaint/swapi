using SoowGood.Domain.Service.Models.UserInfo;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace SoowGoodWeb.Domain.Service.Repositories
{
    public interface IAuthenticationQueryRepository
    {
        Task<UserSignInReturnDto> GetUserByUserName(string username);
        Task<UserSignInReturnDto> GetUserByUserId(Guid userId);
        Task<List<UserDto>> GetUsersByUserRole(string RoleName);
        Task<List<Role>> GetUserRolesByUserId(Guid userId);
    }
}
