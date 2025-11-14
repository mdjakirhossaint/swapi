using SoowGoodWeb.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SoowGoodWeb.DtoModels
{
    public class PlatformPackageFacilityDto : FullAuditedEntityDto<long>
    {
        public long? PlatformPackageId { get; set; }
        //public PlatformPackage? PlatformPackage { get; set; }
        public string? FacilityName { get; set; }
    }
}
