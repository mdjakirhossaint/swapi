using SoowGoodWeb.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SoowGoodWeb.Models
{
    public class CampaignDoctor : FullAuditedEntity<long>
    {
        public long? DoctorProfileId { get; set; }
        public DoctorProfile? DoctorProfile { get; set; }
        public long? CampaignId { get; set; }
        public Campaign? Campaign { get; set; }
       
    }
}
