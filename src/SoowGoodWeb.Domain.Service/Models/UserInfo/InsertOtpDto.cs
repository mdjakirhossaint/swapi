using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Domain.Service.Models.UserInfo
{
    public class InsertOtpDto
    {
        public int OtpNo { get; set; } 

        public string MobileNo { get; set; }

        public DateTime ExpireDateTime { get; set; }

        public int OtpStatus { get; set; }

        public int MaxAttempt { get; set; }

        public int? CreatorId { get; set; }
    }
}
