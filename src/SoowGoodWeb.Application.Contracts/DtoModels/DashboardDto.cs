using SoowGoodWeb.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SoowGoodWeb.DtoModels
{
    public class DashboardDto
    {
        public long? todayAppointment { get; set; } = 0;
        public long? todayPatient { get; set;} = 0;
        public decimal? todayFeeAmount { get; set; } = 0;
        public decimal? todayPaidAmount { get; set; } = 0;
        public decimal? todayConsulted { get; set; } = 0;
        public long? totalAppointment { get; set; } = 0;
        public long? totalPatient { get; set; } = 0;
        public long? totalNewAppointment { get; set; } = 0;
        public long? totalFollowUpAppointment { get; set; } = 0;
    }
}
