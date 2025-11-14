using SoowGood.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGood.Domain.Service.Models.UserInfo
{
    public class UserSignInRequestDto
    {
        public string userName { get; set; }
        public string password { get; set; }
        public int? userLoginType { get; set; } = 0;
    }
}
