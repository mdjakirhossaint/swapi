using Microsoft.AspNetCore.Http;
using SoowGood.Domain.Service.Models.UserRole;
using SoowGoodWeb.Application.Service.Services.ResponseHelper;
using SoowGoodWeb.Application.Service.Services.TokenService;
using SoowGoodWeb.Application.Service.Services.UserRole;
using SoowGoodWeb.Core.GenericModels;
using SoowGoodWeb.Core.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SoowGoodWeb.Services
{
    public class RoleManagementService : SoowGoodWebAppService
    {
        private readonly UserRoleService _userRoleQueryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TokenService _tokenService;
        public RoleManagementService(UserRoleService userRoleQueryRepository
            , IHttpContextAccessor httpContextAccessor
            , TokenService tokenService)
        {
            _userRoleQueryRepository = userRoleQueryRepository;
            _httpContextAccessor = httpContextAccessor;
            _tokenService = tokenService;
        }
        public async Task<ApiResponseList<UserRoleResponseDto>> GetAllRoles()
        {
            var response = new ApiResponseList<UserRoleResponseDto>();
            // Find the user by their ID
            var roles = await _userRoleQueryRepository.GetAllRoles();

            if (roles.Result == null)
            {
                ApiResponseHelper.SetFailedResponse(response, roles.Result, ApiResponseMessage.common_no_data_available, null, null);
                return response;
            }
            ApiResponseHelper.SetSuccessResponse(response, roles.Result, ApiResponseMessage.common_success_message, null, null);
            return response;
        }

        public async Task<ApiResponseList<UserRoleResponseDto>> GetRolesAll()
        {
            var response = new ApiResponseList<UserRoleResponseDto>();
            // Get the HTTP Request from IHttpContextAccessor
            //var authorizationToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

            //var (userId, matchKey, status, message) = await _tokenService.GetUserIdFromAuthorizationToken(authorizationToken);

            //if (userId == Guid.Empty && !matchKey)
            //{
            //    ApiResponseHelper.SetFailedResponse(response, null, ApiResponseMessage.common_security_access_failed, null, null);
            //    return response;
            //}

            // Find the user by their ID
            var roles = await _userRoleQueryRepository.GetAllRoles();

            if (roles.Result == null)
            {
                ApiResponseHelper.SetFailedResponse(response, roles.Result, ApiResponseMessage.common_no_data_available, null, null);
                return response;
            }
            ApiResponseHelper.SetSuccessResponse(response, roles.Result, ApiResponseMessage.common_success_message, null, null);
            return response;
        }
    }
}
