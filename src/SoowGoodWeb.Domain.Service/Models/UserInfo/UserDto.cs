using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGood.Domain.Service.Models.UserInfo
{
    public class UserDto
    {        
        public Guid Id { get; set; }
        public Guid? TenantId { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string Password { get; set; }
        public string SecurityStamp { get; set; }
        public bool IsExternal { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string ExtraProperties { get; set; }
        public string ConcurrencyStamp { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid? CreatorId { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public Guid? LastModifierId { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeleterId { get; set; }
        public DateTime? DeletionTime { get; set; }
        public Guid RoleId { get; set; }
    }
    public class UserSingupRequestDto
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string RoleId { get; set; }
    }

    public class SendOtpModel
    {
        public string UserName { get; set; }
        public string RoleId { get; set; }
        public int Type { get; set; }
    }
    public class SaveSendOtpModel
    {
        public int OTP { get; set; }
        public string UserName { get; set; }
    }
}
