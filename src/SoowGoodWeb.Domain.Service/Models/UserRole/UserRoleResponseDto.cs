using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGood.Domain.Service.Models.UserRole
{
    public class UserRoleResponseDto
    {
        public Guid Id { get; set; }
        public Guid? TenantId { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public bool IsDefault { get; set; }
        public bool IsStatic { get; set; }
        public bool IsPublic { get; set; }
        public int EntityVersion { get; set; }
        public string ExtraProperties { get; set; }
        public string ConcurrencyStamp { get; set; }
    }
}
