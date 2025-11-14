using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace SoowGoodWeb.InputDto
{
    public class PlatformPackageInputDto : FullAuditedEntityDto<long>
    {
        public string? PackageTitle { get; set; }
        public string? PackageName { get; set; }
        public string? PackageCode { get; set; }
        public string? PackageDescription { get; set; }
        public List<PlatformPackageFacilityInputDto>? PackageFacilities { get; set; }
        public string? Reason { get; set; }
        public decimal? Price { get; set; }
        public long? PackageProviderId { get; set; }
    }
}
