using SoowGoodWeb.Core.GenericModels;
using SoowGood.Domain.Service.Models.UserRole;
using SoowGood.Domain.Service.Repositories.UserRole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Domain.Service.Repositories.UserRole;
using SoowGoodWeb.Domain.Service.Models.UserRole;

namespace SoowGoodWeb.Application.Service.Services.UserRole
{
    public class UserRoleService 
    {
        private readonly IUserRoleQueryRepository _roleService;
        private readonly IUserRoleManagementCommandRepository _userRoleManagementCommandRepository;
        public UserRoleService(IUserRoleQueryRepository roleService, IUserRoleManagementCommandRepository userRoleManagementCommandRepository)
        {
            _roleService = roleService;
            _userRoleManagementCommandRepository = userRoleManagementCommandRepository;
        }
        public async Task<Response<UserRoleResponseDto>> GetUserRoleByUserID(string userId)
        {
            var response = new Response<UserRoleResponseDto>();
            var userRole = await _roleService.GetUserRoleByUserId(userId);
            if (userRole != null)
            {
                response.Result = userRole;
                response.IsSuccess = true;
                return response;
            }
            return response;
        }
        public async Task<Response<List<UserRoleResponseDto>>> GetAllRoles()
        {
            var response = new Response<List<UserRoleResponseDto>>();
            var userRole = await _roleService.GetAll();
            if (userRole != null)
            {
                response.Result = userRole;
                response.IsSuccess = true;
                return response;
            }
            return response;
        }
        public async Task<Response<bool>> InsertUserRole(UserRoleInsertDto entity)
        {
            var response = new Response<bool>();
            var result = await _userRoleManagementCommandRepository.Insert(entity);
            if (result.IsSuccess)
            {
                response.Result = result.Result;
                response.IsSuccess = true;
                return response;
            }
            return response;
        }
    }
}
