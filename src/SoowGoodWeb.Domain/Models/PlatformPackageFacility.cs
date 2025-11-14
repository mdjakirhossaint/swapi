using SoowGoodWeb.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SoowGoodWeb.Models
{
    public class PlatformPackageFacility : FullAuditedEntity<long>
    {
        public long? PlatformPackageId { get; set; }
        public PlatformPackage? PlatformPackage { get; set; }
        public string? FacilityName { get; set; }

    }
}
