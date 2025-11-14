using System;
using System.Collections.Generic;
using System.Text;

namespace SoowGoodWeb.DtoModels
{
    public class PatientReturnDto
    {
        public string PatientName { get; set; }
        public string PatientCode { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public string BloodGroup { get; set; }
        public string PatientMobileNo { get; set; }
        public string CreatedBy { get; set; }
        public string CratorCode { get; set; }
        public string CreatorRole { get; set; }
        public string CreatorEntityId { get; set; }
        public Guid UserId { get; set; }
        public int PatientProfileId { get; set; }
        public int Id { get; set; }
        public string PatientEmail { get; set; }
    }
}
