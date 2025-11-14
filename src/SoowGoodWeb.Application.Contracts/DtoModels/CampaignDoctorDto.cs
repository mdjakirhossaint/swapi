using SoowGoodWeb.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SoowGoodWeb.DtoModels
{
    public class CampaignDoctorDto : FullAuditedEntityDto<long>
    {
        public long? DoctorProfileId { get; set; }
        //public DoctorProfile? DoctorProfile { get; set; }
        public long? CampaignId { get; set; }
        //public Campaign? Campaign { get; set; }
        public string? DoctorName { get; set; }
        public DoctorTitle? DoctorTitle { get; set; }
        public string? DoctorTitleName { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsOnline { get; set; }
        public List<DoctorDegreeDto>? DoctorDegrees { get; set; }
        public string? Qualifications { get; set; }
        public List<DoctorSpecializationDto>? DoctorSpecialization { get; set; }
        public string? AreaOfExperties { get; set; }
        public string? ProfilePic { get; set; }
        public decimal? DisplayInstantFeeAsPatient { get; set; }
        public decimal? DoctorFee { get; set; }
        public string? DoctorCode { get; set; }
    }
}
