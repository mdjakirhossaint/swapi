using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Domain.Service.Models.AppointmentDto
{
    public class AppointmentPatientDto
    {
        public long PatientProfileId { get; set; }
        public string PatientCode { get; set; }
        public string PatientName { get; set; }
        public string PatientMobileNo { get; set; }
        public string PatientEmail { get; set; }
        public string PatientLocation { get; set; }
        public string BloodGroup { get; set; }
        public string GenderName { get; set; }
        public int? Age { get; set; }
        public long DoctorProfileId { get; set; }
    }
}
