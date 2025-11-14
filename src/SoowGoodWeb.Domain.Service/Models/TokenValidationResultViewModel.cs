using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGood.Domain.Service.Models
{
    public class TokenValidationResultViewModel
    {
        public bool userLoggedIn { get; set; }
        public string token { get; set; }
        public Guid? userId { get; set; }
    }
}
