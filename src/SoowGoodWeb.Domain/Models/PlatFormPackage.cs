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
    public class PlatformPackage : FullAuditedEntity<long>
    {
       
        public string? PackageTitle { get; set; }
        public string? PackageName { get; set; }
        public string? PackageCode { get; set; }
        public string? PackageDescription { get; set;}
        public List<PlatformPackageFacility>? PackageFacilities { get; set; }
        public string? Reason { get; set; }
        public long? PackageProviderId { get; set; }
        public DoctorProfile? PackageProvider { get; set; }
        public decimal? Price { get; set;}


    }
}
