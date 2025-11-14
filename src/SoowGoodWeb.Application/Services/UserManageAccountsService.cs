using Microsoft.AspNetCore.Authorization;
using SoowGoodWeb.DtoModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;
using SoowGoodWeb.Models;
using Volo.Abp.Domain.Repositories;
using System.Linq;
using SoowGood.Domain.Service.Models.UserInfo;
using SoowGoodWeb.Application.Service.Services.SMSService;
using SoowGoodWeb.Enums;
using SoowGood.Domain.Service.Models.OTP;
using SoowGood.Core.Service;
using SoowGoodWeb.Application.Service.Services.ResponseHelper;
using SoowGoodWeb.Core.GenericModels;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Core.Service;
using SoowGoodWeb.Domain.Service.Repositories;
using SoowGoodWeb.Application.Service.Services.Password;
using Nancy;
using SoowGoodWeb.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using SoowGood.Domain.Service.Repositories.User;
using SignalRTieredDemo.Users;
using SoowGoodWeb.Domain.Service.Models.UserInfo;
using SoowGoodWeb.Application.Service.Services.UserRole;
using SoowGoodWeb.Domain.Service.Models.UserRole;
//using System.IdentityModel.Tokens.Jwt;
////using System.IdentityModel.Tokens.Jwt;

namespace SoowGoodWeb.Services
{

    public class UserManageAccountsService : ApplicationService
    {
        string authClientUrl = PermissionHelper._identityClientUrl;
        string authUrl = PermissionHelper._authority;

        private readonly IdentityUserManager _userManager;

        private readonly IEmailSender _emailSender;
        private readonly GreenWebSmsService _greenWebSmsService;
        private readonly IRepository<Otp, int> _repository;
        private readonly IAuthenticationQueryRepository _userService;
        private readonly IUserCommanRepository _userCommandService;
        private readonly UserRoleService _userRoleService;
        private readonly PasswordHasherService _passwordHasherService;
        //private readonly SoowGoodWebDbContext _dbContext;
        public UserManageAccountsService(IdentityUserManager userManager
                                   ,
                                   IEmailSender emailSender,
                                   GreenWebSmsService greenWebSmsService,
                                   IRepository<Otp, int> repository,
                                   IAuthenticationQueryRepository userService,
                                   PasswordHasherService passwordHasherService,
                                   //SoowGoodWebDbContext dbContext
                                   IUserCommanRepository userCommandService,
                                   UserRoleService userRoleService
                                   )
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _greenWebSmsService = greenWebSmsService;
            _repository = repository;
            _userService = userService;
            _passwordHasherService = passwordHasherService;
            //_dbContext = dbContext;
            _userCommandService = userCommandService;
            _userRoleService = userRoleService;
        }

        [AllowAnonymous]
        public virtual async Task<ApiResponse<UserSignUpResultDto>> SignupUser(UserSingupRequestDto request)
        {
            var response = new ApiResponse<UserSignUpResultDto>();

            //var IsExistingUser = await _userManager.FindByNameAsync(request.UserName);
            var IsExistingUser = await _userService.GetUserByUserName(request.UserName);

            if (IsExistingUser != null)
            {
                response.results = null;
                response.status_code = 422;
                response.message = "User already exists.";
                response.is_success = false;
                return response;
            }
            //var clientKey = "SoowGood_App";

            //var otpSend = _greenWebSmsService.SendOtp(clientKey,request.UserName);

            //var user = new Volo.Abp.Identity.IdentityUser(Guid.NewGuid(), request.UserName, request.Email);
            //var user = new CustomIdentityUser(Guid.NewGuid(), request.UserName, request.Email);
            //user.SetPhoneNumber(request.PhoneNumber, true);

            var hashedPassword = _passwordHasherService.HashPassword(request.Password);

            // Now you can set it directly
           // user.SetCustomPasswordHash(hashedPassword);

            var user = new UserInsertDto
            {
                Id = Guid.NewGuid(),
                UserName = request.UserName,
                Name = request.Name,
                NormalizedUserName = request.UserName,
                Email = request.Email,
                NormalizedEmail = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = hashedPassword,
                IsDeleted = false,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                CreationTime = DateTime.Now,
                SecurityStamp = "asdf",
            };
            //var userInsertResponse = await _userManager.CreateAsync(user);
            var userInsertResponse = await _userCommandService.Insert(user);

            if (userInsertResponse.Result)
            {
                try
                {
                    var role = await _userRoleService.GetAllRoles();
                    var userRole = new UserRoleInsertDto
                    {
                        UserId = user.Id,
                        RoleId = role.Result.Where(x=>x.Name.Contains(request.RoleId)).Select(x=>x.Id).First(),
                    };

                    //var roleResponse = await _userManager.AddToRoleAsync(user, request.RoleId.ToString());
                    var roleResponse = await _userRoleService.InsertUserRole(userRole);
                    if (!roleResponse.Result)
                    {
                        response.is_success = false;
                        response.message = roleResponse.Message;
                        return response;
                    }
                }
                catch (Exception)
                {

                    throw;
                }
                response.is_success = true;
                response.message = "User created and assigned to roles successfully.";
            }
            else
            {
                response.is_success = false;
                response.message = userInsertResponse.Message;
                response.results = null;
                response.status_code = 400;
                return response;
            }

            var userSingUpResponse = new UserSignUpResultDto
            {
                UserName = request.UserName,
                UserId = user.Id,
            };

            response.results = userSingUpResponse;
            response.is_success = true;
            response.message = "Successfully User Created";
            response.status_code = 200;

            return response;
        }

        public virtual async Task<ApiResponse<UserSignUpResultDto>> CheckUserExistByUserName(string mobileNo)
        {
            var response = new ApiResponse<UserSignUpResultDto>();

            //var IsExistingUser = await _userManager.FindByNameAsync(mobileNo);
            var IsExistingUser = await _userService.GetUserByUserName(mobileNo);

            if (IsExistingUser != null)
            {
                response.results = null;
                response.status_code = 422;
                response.message = "User already exists.";
                response.is_success = false;
                return response;
            }
           
            response.results = null;
            response.is_success = true;
            response.message = "User can not found";
            response.status_code = 200;

            return response;
        }

        public async Task<List<string>> GetUserRolesAsync(Guid userId)
        {
            // Find the user by their ID
            //var user = await _userManager.FindByIdAsync(userId.ToString());

            var user = await _userService.GetUserByUserId(userId);
            //var users = new IdentityUser(user.Id, user.UserName, user.Email);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Get the roles assigned to the user
            //var roles = await _userManager.GetRolesAsync(users);

            var roles = await _userService.GetUserRolesByUserId(userId);
            var takeRoles = roles.Select(x=>x.Name).ToList();
            return takeRoles.ToList();
        }
        [AllowAnonymous]
        public virtual async Task<ApiResponse<SendOtpResponseDto>> SendOtp(SendOtpModel request)
        {
            var response = new ApiResponse<SendOtpResponseDto>();
            var result = new SendOtpResponseDto();
            var clientKey = "SoowGood_App";
            try
            {

           
           
            //var IsExistingUser = await _userManager.FindByNameAsync(request.UserName);

            var IsExistingUser = await _userService.GetUserByUserName(request.UserName);

            if (request.Type == (int)OtpType.Login)
            {
                if (IsExistingUser != null && !string.IsNullOrEmpty(IsExistingUser.UserName))
                {
                    var userRole = await GetUserRolesAsync(IsExistingUser.Id);

                    if (userRole.Where(x => x.Contains(request.RoleId)).ToList().Count == 0)
                    {
                        ApiResponseHelper.SetFailedResponse(response, null, "Role does not match", null, null);
                        return response;
                    }

                    var otpSendResult =  await SendOtpMessage(request, response, clientKey);
                        try
                        {
                            response.results = otpSendResult.results;
                            ApiResponseHelper.SetSuccessResponse(response, response.results, null, "Success", 200);
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                   
                }
                else
                {
                    ApiResponseHelper.SetFailedResponse(response, null, "User could not found!", null, null);
                    return response;
                }
            }
            else if (request.Type == (int)OtpType.ForgotPassword)
            {
                if (IsExistingUser != null && !string.IsNullOrEmpty(IsExistingUser.UserName))
                {
                    return await SendOtpMessage(request, response, clientKey);
                }
                else
                {
                    ApiResponseHelper.SetFailedResponse(response, null, "User could not found!", null, null);
                    return response;
                }
            }
            else
            {
                if (IsExistingUser != null)
                {
                    ApiResponseHelper.SetFailedResponse(response, null, "User already exists", null, null);
                    return response;
                }

                var otpSend = await _greenWebSmsService.SendSMS(clientKey, request.UserName);
                await SaveOtpForVerifyUser(request, otpSend);

                result = new SendOtpResponseDto
                {
                    Otp = otpSend.Result.otp,
                    IsActive = true,
                    Success = true,
                    Message = "Otp Sent",
                };

                    try
                    {
                        ApiResponseHelper.SetSuccessResponse(response, result, "Otp Sent", null, null);
                        return response;
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                
            }
            }
            catch (Exception ex)
            {

                throw;
            }
            try
            {

                return response;
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        private async Task<ApiResponse<SendOtpResponseDto>> SendOtpMessage(SendOtpModel request, ApiResponse<SendOtpResponseDto> response, string clientKey)
        {
            try
            {
                var otpSended = await _greenWebSmsService.SendSMS(clientKey, request.UserName);
                await SaveOtpForVerifyUser(request, otpSended);
                var result = new SendOtpResponseDto
                {
                    Otp = otpSended.Result.otp,
                    IsActive = true,
                    Success = true,
                    Message = "Otp Sent",
                };
                response.is_success = true;
                response.results = result;
                ApiResponseHelper.SetSuccessResponse(response, result, "Otp Sent", null, null);
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task SaveOtpForVerifyUser(SendOtpModel request, Response<OtpResultDto> otpSend)
        {
            try
            {
                Otp otpEntity = new Otp();
                otpEntity.OtpNo = otpSend.Result.otp;
                otpEntity.MobileNo = request.UserName;
                otpEntity.ExpireDateTime = DateTime.Now.AddMinutes(3);
                otpEntity.OtpStatus = OtpStatus.New;

                var InsertOtpDto = new InsertOtpDto
                {
                    OtpNo = otpSend.Result.otp,
                    MobileNo = request.UserName,
                    ExpireDateTime = DateTime.Now.AddMinutes(3),
                    OtpStatus = (int)OtpStatus.New,
                };

                //await _repository.InsertAsync(otpEntity);
                await _userCommandService.InsertOtp(InsertOtpDto);
            }
            catch (Exception)
            {

                throw;
            }
            
        }
         public async Task<Response<OtpResultDto>> SaveOtpForVerifyUserLater(SaveSendOtpModel request)
        {
            var response = new Response<OtpResultDto>();
            try
            {
                Otp otpEntity = new Otp();
                otpEntity.OtpNo = request.OTP;
                otpEntity.MobileNo = request.UserName;
                otpEntity.ExpireDateTime = DateTime.Now.AddMinutes(3);
                otpEntity.OtpStatus = OtpStatus.New;

                await _repository.InsertAsync(otpEntity);
            }
            catch (Exception)
            {

                throw;
            }
            return response;
        }

        public async Task<ApiResponse<UserSignInReturnDto>> VerifyOtp(OtpRequestDto request)
        {
            var response = new ApiResponse<UserSignInReturnDto>();
            var getOtpInfo = await _greenWebSmsService.ValidateOtp(request.MobileNo, request.Otp);
            if (getOtpInfo.IsSuccess)
            {
                var getUser = await _userService.GetUserByUserName(request.MobileNo);
                ApiResponseHelper.SetSuccessResponse(response,getUser,getOtpInfo.Message,getOtpInfo.Status, getOtpInfo.StatusCode);
                return response;
            }
            ApiResponseHelper.SetFailedResponse(response, null, getOtpInfo.Message, getOtpInfo.Status, getOtpInfo.StatusCode);
            return response;
        }
        [AllowAnonymous]
        public async Task<ApiResponse<ResetPasswordResponseDto>> ResetPassword(ResetPasswordInputDto inputDto)
        {
            var response = new ApiResponse<ResetPasswordResponseDto>();

            if (!string.IsNullOrEmpty(inputDto.NewPassword) && !string.IsNullOrEmpty(inputDto.UserId))
            {
                try
                {
                    // var user = await _dbContext.Users.FindAsync(Guid.Parse(inputDto.UserId));
                    var user = await _userService.GetUserByUserId(Guid.Parse(inputDto.UserId));
                    if (user != null)
                    {
                        // Hash the new password
                        var hashedPassword = _passwordHasherService.HashPassword(inputDto.NewPassword);

                        // Manually set the hashed password
                        //user.GetType().GetProperty("PasswordHash")?.SetValue(user, hashedPassword);

                        // Save changes in the database
                        //await _dbContext.SaveChangesAsync();
                        var userPasswordUpdateDto = new UserPasswordUpdateDto
                        {
                            userId = Guid.Parse(inputDto.UserId),
                            PasswordHash = hashedPassword
                        };

                        await _userCommandService.UpdateUserPassword(userPasswordUpdateDto);

                        var result = new ResetPasswordResponseDto
                        {
                            UserName = user.UserName,
                            Name = user.Name,
                            Success = true,
                            Message = ApiResponseMessage.common_update_success_message
                        };
                        ApiResponseHelper.SetSuccessResponse(response, result, ApiResponseMessage.common_update_success_message, null, null);
                    }
                    else
                    {
                        ApiResponseHelper.SetFailedResponse(response, null, "User not found.", null, null);
                    }
                }
                catch (Exception ex)
                {
                    ApiResponseHelper.SetFailedResponse(response, null, "An error occurred while resetting the password.", ex.Message, 500);
                }
            }
            else
            {
                ApiResponseHelper.SetFailedResponse(response, null, "Invalid input data.", null, null);
            }

            return response;
        }
        [AllowAnonymous]
        public async Task<ApiResponse<ResetPasswordResponseDto>> UserPasswordChangesScript(ResetPasswordRoleWiseInputDto inputDto)
        {
            var response = new ApiResponse<ResetPasswordResponseDto>();

            if (!string.IsNullOrEmpty(inputDto.Password) && !string.IsNullOrEmpty(inputDto.RoleName))
            {
                try
                {
                    var getUsers = await _userService.GetUsersByUserRole(inputDto.RoleName);

                    foreach (var getuser in getUsers)
                    {
                        //var user = await _dbContext.Users.FindAsync(getuser.Id);
                        var user = await _userService.GetUserByUserId(getuser.Id);
                        if (user != null)
                        {
                            var password = "";
                            if (inputDto.RoleName == "Doctor")
                            {
                                password = inputDto.Password + $"{user.UserName}";
                            }
                            else
                            {
                                password = inputDto.Password;
                            }

                            // Hash the new password
                            var hashedPassword = _passwordHasherService.HashPassword(password);

                            // Manually set the hashed password
                            //user.GetType().GetProperty("PasswordHash")?.SetValue(user, hashedPassword);

                            // Save changes in the database
                            var userPasswordUpdateDto = new UserPasswordUpdateDto
                            {
                                userId = getuser.Id,
                                PasswordHash = hashedPassword
                            };

                            await _userCommandService.UpdateUserPassword(userPasswordUpdateDto);

                            var result = new ResetPasswordResponseDto
                            {
                                UserName = user.UserName,
                                Name = user.Name,
                                Success = true,
                                Message = ApiResponseMessage.common_update_success_message
                            };
                            ApiResponseHelper.SetSuccessResponse(response, result, ApiResponseMessage.common_update_success_message, null, null);
                        }
                        else
                        {
                            ApiResponseHelper.SetFailedResponse(response, null, "User not found.", null, null);
                        }
                    }


                }
                catch (Exception ex)
                {
                    ApiResponseHelper.SetFailedResponse(response, null, "An error occurred while resetting the password.", ex.Message, 500);
                }
            }
            else
            {
                ApiResponseHelper.SetFailedResponse(response, null, "Invalid input data.", null, null);
            }

            return response;
        }



    }


}

