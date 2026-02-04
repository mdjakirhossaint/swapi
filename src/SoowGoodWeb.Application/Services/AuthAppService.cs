using Abp.Authorization;
using Microsoft.AspNetCore.Identity;
using SoowGoodWeb.Core.GenericModels;
using SoowGood.Domain.Service.Models.UserInfo;
using SoowGoodWeb.Application.Service.Services.ResponseHelper;
using SoowGoodWeb.Application.Service.Services.TokenService;
using SoowGoodWeb.Application.Service.Services.UserRole;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.InputDto;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using SoowGoodWeb.Domain.Service.Repositories;
using SoowGoodWeb.Core.Service;
using Nancy;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;
using SoowGoodWeb.Application.Service.Services.Password;
using SoowGood.Core.Service;
namespace SoowGoodWeb.Services
{
    public class AuthAppService : ApplicationService
    {
        private readonly IdentityUserManager _userManager;
        private readonly IAuthenticationQueryRepository _authService;
        private readonly UserRoleService _userRoleService;
        private readonly TokenService _tokenService;
        private readonly PasswordHasherService _passwordHasherService;
        public AuthAppService(IdentityUserManager userManager, IAuthenticationQueryRepository authService,
            UserRoleService userRoleService,
            TokenService tokenService,
            PasswordHasherService passwordHasherService
            )
        {
            _userManager = userManager;
            _authService = authService;
            _userRoleService = userRoleService;
            _tokenService = tokenService;
            _passwordHasherService = passwordHasherService;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginApi(UserSignInRequestDto request)
        {
            var response = new ApiResponse<LoginResponseDto>();

            if (string.IsNullOrWhiteSpace(request.userName) || string.IsNullOrWhiteSpace(request.password))
            {
                ApiResponseHelper.SetFailedResponse(response, null, ApiResponseMessage.parameter_common_null_message, null, null);
                return response;

            }

            var getUser = await _authService.GetUserByUserName(request.userName);

            if (getUser != null)
            {
                Volo.Abp.Identity.IdentityUser user = new Volo.Abp.Identity.IdentityUser(getUser.Id, getUser.UserName, getUser.Email);

                //var isValid = VerifyPassword(user, getUser.PasswordHash, request.password);

                var isValid = _passwordHasherService.VerifyPassword(request.password,getUser.PasswordHash.Trim());



                if (!isValid)
                {
                    ApiResponseHelper.SetFailedResponse(response, null, ApiResponseMessage.user_incorrect_username_password, null, null);
                    return response;
                }

                var userRole = await _userRoleService.GetUserRoleByUserID(getUser.Id.ToString());

                if (userRole == null)
                {
                    ApiResponseHelper.SetFailedResponse(response, null, ApiResponseMessage.common_no_data_available, null, null);
                    return response;
                }
                getUser.info = userRole.Result;

                var roles = new List<string>();
                var loginType = 0;

                if (userRole.Result.Name.ToString() == "Patient")
                { 
                    loginType = (int)UserLoginType.Patient;
                }if (userRole.Result.Name.ToString() == "Doctor")
                { 
                    loginType = (int)UserLoginType.Doctor;
                }if (userRole.Result.Name.ToString() == "Agent")
                { 
                    loginType = (int)UserLoginType.Agent;
                }if (userRole.Result.Name.ToString() == "Admin")
                { 
                    loginType = (int)UserLoginType.Admin;
                }

                if (userRole.Result != null)
                {
                    roles.Add(userRole.Result.Name.ToLower());
                }

                //var token = await _tokenService.CreateToken(user);
                var token = await _tokenService.GenerateAccessToken(user);
                var refreshToken = await _tokenService.GenerateRefreshToken(user);

                if (request.userLoginType != loginType)
                {
                    ApiResponseHelper.SetFailedResponse(response, null, "Role does not match", null, null);
                    return response;
                }

                response.results = new LoginResponseDto
                {
                    UserId = getUser?.Id,
                    UserName = getUser?.UserName,
                    RoleName = roles,
                    Success = true,
                    Message = "Success",
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    LoginType= "google",
                    UserEmail=getUser?.Email
                };
                ApiResponseHelper.SetSuccessResponse(response, response.results, ApiResponseMessage.common_success_message, null, null);
                return response;
            }
            else
            {
                ApiResponseHelper.SetFailedResponse(response, null, ApiResponseMessage.user_incorrect_email, null, null);
                return response;
            }
        }
        /// <summary>
        /// Verifies if the provided password matches the hashed password stored in the database.
        /// </summary>
        /// <param name="user">The IdentityUser object associated with the user.</param>
        /// <param name="hashedPasswordFromDb">The hashed password retrieved from the database.</param>
        /// <param name="inputPassword">The plain text password entered by the user.</param>
        /// <returns>True if the password is valid, otherwise false.</returns>
        private static bool VerifyPassword(Volo.Abp.Identity.IdentityUser user, string hashedPasswordFromDb, string inputPassword)
        {
            var passwordHasher = new PasswordHasher<Volo.Abp.Identity.IdentityUser>();
            var result = passwordHasher.VerifyHashedPassword(user, hashedPasswordFromDb, inputPassword);

            // Treat both Success and SuccessRehashNeeded as valid password matches
            return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
        }

        public async Task<ApiResponse<LoginResponseDto>> RefreshToken(RefreshTokenInput input)
        {
            var response = new ApiResponse<LoginResponseDto>();
            // Validate the refresh token
            var principal = await _tokenService.ValidateRefreshToken(input.RefreshToken);
            if (principal == null)
            {
                throw new AbpAuthorizationException("Invalid refresh token.");
            }

            var userId = await _tokenService.GetUserIdFromJwtToken(input.RefreshToken);

            if (userId == null)
            {
                throw new AbpAuthorizationException("Invalid user.");
            }
            //var getUserInfo = await _userManager.GetByIdAsync(userId.Result);
            var getUserInfo = await _authService.GetUserByUserId(userId.Result);

            var getUser = await _authService.GetUserByUserName(getUserInfo.UserName);


            if (getUser != null)
            {
                Volo.Abp.Identity.IdentityUser user = new Volo.Abp.Identity.IdentityUser(getUser.Id, getUser.UserName, getUser.Email);

                var userRole = await _userRoleService.GetUserRoleByUserID(getUser.Id.ToString());

                if (userRole == null)
                {
                    return response;
                }
                getUser.info = userRole.Result;

                var roles = new List<string>();

                if (userRole.Result != null)
                {
                    roles.Add(userRole.Result.Name.ToLower());
                }
                //var token = await _tokenService.CreateToken(user);
                var token = await _tokenService.GenerateAccessToken(user);
                var refreshToken = await _tokenService.GenerateRefreshToken(user);
                response.results = new LoginResponseDto
                {
                    UserId = getUser?.Id,
                    UserName = getUser?.UserName,
                    RoleName = roles,
                    Success = true,
                    Message = "Success",
                    AccessToken = token,
                    RefreshToken = refreshToken,
                };
                ApiResponseHelper.SetSuccessResponse(response, response.results, ApiResponseMessage.validate_token_successfully, null, null);
                return response;
            }
            return response;
        }

        public async Task<ApiResponse<bool>> VerifyAccessToken(VerifyAccessTokenInput input)
        {
            var response = new ApiResponse<bool>();
            // Validate the refresh token
            var isValid = await _tokenService.TokenExpireOrNot(input.AccessToken);
            if (!isValid)
            {
                response.results = false;
                ApiResponseHelper.SetFailedResponse(response, response.results, ApiResponseMessage.invalidApiKeyOrToken, null, null);
                return response;
            }
            response.results = isValid;
            ApiResponseHelper.SetSuccessResponse(response, response.results, ApiResponseMessage.validate_token_successfully, null, null);
            return response;
        }
    }
}