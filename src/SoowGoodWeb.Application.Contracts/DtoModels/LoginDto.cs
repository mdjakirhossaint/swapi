using System;
using System.Collections.Generic;
using System.Text;

namespace SoowGoodWeb.DtoModels
{
    public class LoginDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class LoginResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public List<string> RoleName { get; set; }
        public string Role { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class DeleteUserDataDto
    {
        public string? UserName { get; set; }
    }
    public class OtpResultDto
    {
        public bool? OtpSent { get; set; }
        public bool? IsUserExists { get; set; }
        public int otp { get; set; }
    }
}
