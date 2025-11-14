using SoowGoodWeb.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SoowGoodWeb.Models
{
    public class Campaign : FullAuditedEntity<long>
    {
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public bool? IsActive { get; set; }
        public List<CampaignDoctor>? SelectedDoctor { get; set; }

    }
}
