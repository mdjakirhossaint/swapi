using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Domain.Service.Models.UserRole
{
    public class UserRoleInsertDto
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public Guid? TenantId { get; set; } = Guid.Empty;
    }
}
