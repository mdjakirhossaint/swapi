using SoowGoodWeb.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SoowGoodWeb.InputDto
{
    public class PlatformPackageFacilityInputDto : FullAuditedEntityDto<long>
    {
        public long? PlatformPackageId { get; set; }
        //public PlatformPackage? PlatformPackage { get; set; }
        public string? FacilityName { get; set; }
    }
}
