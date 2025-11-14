using SoowGoodWeb.Enums;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace SoowGoodWeb.DtoModels
{
    public class PlatformPackageDto : FullAuditedEntityDto<long>
    {
        public string? PackageTitle { get; set; }
        public string? PackageName { get; set; }
        public string? PackageCode { get; set; }
        public string? PackageDescription { get; set; }
        public List<PlatformPackageFacilityDto>? PackageFacilities { get; set; }
        public string? FacilityofPackage { get; set; }
        public string? Reason { get; set; }
        public decimal? Price { get; set; }
        public long? PackageProviderId { get; set; }
        public string? DoctorName { get; set; }
        public DoctorTitle? DoctorTitle { get; set; }
        public string? DoctorTitleName { get; set; }
        public List<DoctorDegreeDto>? DoctorDegrees { get; set; }
        public string? Qualifications { get; set; }
        public List<DoctorSpecializationDto>? DoctorSpecialization { get; set; }
        public string? AreaOfExperties { get; set; }
        public string? ProfilePic { get; set; }
        public string? DoctorCode { get; set; }
        public string? Slug { get; set; }

    }
}


