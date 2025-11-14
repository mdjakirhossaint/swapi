using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGood.Domain.Service.Models.OTP
{
    public class OtpRequestDto
    {
        /// <summary>
        /// Gets or sets the mobile number.
        /// </summary>
        public string MobileNo { get; set; }

        /// <summary>
        /// Gets or sets the OTP.
        /// </summary>
        public int Otp { get; set; }
    }
}
