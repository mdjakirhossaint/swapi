using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SoowGood.Domain.Service.Models.UserInfo;
using SoowGood.Domain.Service.Repositories.User;
using SoowGoodWeb.Application.Service.Services.Password;
using SoowGoodWeb.Application.Service.Services.UserRole;
using SoowGoodWeb.Core.GenericModels;
using SoowGoodWeb.Domain.Service.Models.UserInfo;
using SoowGoodWeb.Domain.Service.Models.UserRole;
using SoowGoodWeb.Domain.Service.Repositories;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.InputDto;
using SoowGoodWeb.Interfaces;
using SoowGoodWeb.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;

namespace SoowGoodWeb.Controllers
{
    [Route("api/firebase")]
    public class FirebaseController : AbpController
    {

        private readonly FirebaseAuthService _firebaseAuthService;
        private readonly HttpClient _httpClient;
        private readonly IAuthenticationQueryRepository _userService;
        private readonly PasswordHasherService _passwordHasherService;
        private readonly IUserCommanRepository _userCommandService;
        private readonly UserRoleService _userRoleService;
        public FirebaseController(FirebaseAuthService firebaseAuthService, IAuthenticationQueryRepository userService, PasswordHasherService passwordHasherService,
            IUserCommanRepository userCommandService, UserRoleService userRoleService)
        {
            _firebaseAuthService = firebaseAuthService;
            _httpClient = new HttpClient();
            _userService = userService;
            _passwordHasherService = passwordHasherService;
            _userCommandService = userCommandService;
            _userRoleService = userRoleService;
        }


        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirebaseIdToken))
                return BadRequest("Missing Firebase ID token");

            // 1️⃣ Verify Firebase ID Token
            var firebaseUser = await _firebaseAuthService.VerifyTokenAsync(request.FirebaseIdToken);
            if (firebaseUser == null)
                return Unauthorized("Invalid or expired Firebase token");

            // 2️⃣ Get Google profile info using Google Access Token
            dynamic? googleUser = null;
            if (!string.IsNullOrWhiteSpace(request.GoogleAccessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", request.GoogleAccessToken);

                var googleResponse = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
                if (googleResponse.IsSuccessStatusCode)
                {
                    var content = await googleResponse.Content.ReadAsStringAsync();
                    googleUser = System.Text.Json.JsonSerializer.Deserialize<object>(content);
                }
            }

            // 3️⃣ Return unified info
            var response = new
            {
                Message = "Login verified successfully ✅",
                FirebaseUser = new
                {
                    Uid = firebaseUser.Uid,
                    //Email = firebaseUser.Claims.GetValueOrDefault("email"),
                    //Name = firebaseUser.Claims.GetValueOrDefault("name"),
                    //Picture = firebaseUser.Claims.GetValueOrDefault("picture"),
                },
                GoogleUser = googleUser
            };

            return Ok(response);
        }


        [HttpGet("secure")]
        public IActionResult SecureEndpoint()
        {
            if (!HttpContext.Items.ContainsKey("FirebaseUser"))
                return Unauthorized("Missing or invalid token");

            var user = (FirebaseToken)HttpContext.Items["FirebaseUser"]!;
            return Ok(new
            {
                message = "Firebase Auth Successful ✅",
                uid = user.Uid,
                email = user.Claims.ContainsKey("email") ? user.Claims["email"] : null,
                name = user.Claims.ContainsKey("name") ? user.Claims["name"] : null
            });
        }
        [AllowAnonymous]
        [HttpPost("verify")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> VerifyToken([FromBody] TokenRequest? request)
        {
            var response = new ApiResponse<LoginResponseDto>();

            try
            {
                if (string.IsNullOrEmpty(request?.IdToken))
                {
                    response.is_success = false;
                    response.message = "Token is required.";
                    return Ok(response);
                }

                // ✅ Verify ID token
                FirebaseToken decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);

                // ✅ Extract user info safely
                decoded.Claims.TryGetValue("email", out var emailObj);
                decoded.Claims.TryGetValue("name", out var nameObj);
                decoded.Claims.TryGetValue("picture", out var pictureObj);

                string? email = emailObj?.ToString();
                string? name = nameObj?.ToString();

                if (string.IsNullOrWhiteSpace(email))
                {
                    response.is_success = false;
                    response.message = "Invalid token: email not found.";
                    return Ok(response);
                }

                var IsExistingUser = await _userService.GetUserByUserName(email);
                var role = await _userRoleService.GetAllRoles();
                var doctorRoleId = role.Result.Where(x => x.Name.ToLowerInvariant().Contains("doctor"))
                                              .Select(x => x.Id)
                                              .First();

                // --------------------------------------------------------------------
                // 1️⃣ IF USER DOES NOT EXIST → CREATE
                // --------------------------------------------------------------------
                if (IsExistingUser == null)
                {
                    var hashedPassword = _passwordHasherService.HashPassword("Prescripto@Zak.Com1431");

                    var user = new UserInsertDto
                    {
                        Id = Guid.NewGuid(),
                        UserName = email,
                        Name = name ?? "",
                        NormalizedUserName = email,
                        Email = email,
                        NormalizedEmail = email,
                        PhoneNumber = "01777606656",
                        PasswordHash = hashedPassword,
                        IsDeleted = false,
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true,
                        CreationTime = DateTime.Now,
                        SecurityStamp = Guid.NewGuid().ToString(),
                    };

                    var userInsertResponse = await _userCommandService.Insert(user);
                    var doctorProfileInsert = new DoctorProfileInputDto
                    {
                        UserId = user.Id,
                        FullName = name ?? "",
                        Email = email,
                        MobileNo = user.PhoneNumber,
                        CreationTime = DateTime.Now,
                        IsActive = true,
                        IsOnline=false,
                        IsDeleted=false
                    };

                    var doctorInsertResponse = await _userCommandService.DoctorProfileInsert(doctorProfileInsert);



                    if (!userInsertResponse.Result)
                    {
                        response.is_success = false;
                        response.message = userInsertResponse.Message;
                        response.status_code = 400;
                        return Ok(response);
                    }

                    // Assign Role
                    var userRole = new UserRoleInsertDto
                    {
                        UserId = user.Id,
                        RoleId = doctorRoleId
                    };

                    var roleResponse = await _userRoleService.InsertUserRole(userRole);

                    if (!roleResponse.Result)
                    {
                        // User created but role failed
                        LoginResponseDto loginReturn = new LoginResponseDto
                        {
                            AccessToken = "",
                            RefreshToken = "",
                            UserId = user.Id,
                            UserName = user.UserName,
                            Role = doctorRoleId.ToString(),
                            Success = true,
                            Message = "User created but role assignment failed."
                        };

                        response.results = loginReturn;
                        response.is_success = false;
                        response.message = roleResponse.Message;
                        return Ok(response);
                    }

                    // Final success for new user
                    response.is_success = true;
                    response.message = "User created and assigned to roles successfully.";
                    response.status_code = 200;

                    return Ok(response);
                }

                // --------------------------------------------------------------------
                // 2️⃣ EXISTING USER → LOGIN
                // --------------------------------------------------------------------
                string loginType = "unknown";

                // Check if claim exists
                if (decoded.Claims.TryGetValue("firebase", out var firebaseObj) && firebaseObj != null)
                {
                    // firebaseObj is JObject, cast safely
                    var firebaseClaim = firebaseObj as Newtonsoft.Json.Linq.JObject;

                    if (firebaseClaim != null && firebaseClaim.TryGetValue("sign_in_provider", out var providerToken))
                    {
                        loginType = providerToken.ToString(); // e.g., "password", "phone", "google.com"

                        // Map Firebase providers to your friendly names
                        loginType = loginType switch
                        {
                            "password" => "email",
                            "phone" => "phone",
                            "google.com" => "google",
                            "facebook.com" => "facebook",
                            _ => loginType
                        };
                    }
                }

                var login = new LoginResponseDto
                {
                    AccessToken = "",
                    RefreshToken = "",
                    UserId = IsExistingUser.Id,
                    UserName = IsExistingUser.UserName,
                    UserEmail= IsExistingUser.Email.ToString(),
                    LoginType = loginType,
                    Role = doctorRoleId.ToString(),
                    Success = true,
                    Message = "Login successful."
                };

                response.is_success = true;
                response.message = "Token is valid ✅";
                response.status_code = 200;
                response.results = login;

                return Ok(response);
            }
            catch (FirebaseAuthException)
            {
                response.is_success = false;
                response.message = "Invalid or expired token ❌";
                response.status_code = 401;
                return Unauthorized(response);
            }
            catch (Exception)
            {
                response.is_success = false;
                response.message = "An unexpected error occurred.";
                response.status_code = 500;
                return StatusCode(500, response);
            }
        }

    }

    public class TokenRequest
    {
        public string IdToken { get; set; } = string.Empty;
    }

    public class GoogleLoginRequest
    {
        public string? GoogleAccessToken { get; set; }
        public string? FirebaseIdToken { get; set; }
    }
}
