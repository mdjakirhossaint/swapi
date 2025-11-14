using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace SoowGoodWeb.Models
{
    public class CustomIdentityUser : IdentityUser
    {
        public CustomIdentityUser(Guid id, string userName, string email)
            : base(id, userName, email) { }

        public void SetCustomPasswordHash(string passwordHash)
        {
            PasswordHash = passwordHash; // Directly set the hash
        }
    }
}
