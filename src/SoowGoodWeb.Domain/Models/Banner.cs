using SoowGoodWeb.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SoowGoodWeb.Models
{
    public class Banner : FullAuditedEntity<long>
    {
        public string? createFor { get; set; }
        public string? Title { get; set; }
        public string? Link { get; set; }
        public string? Description { get; set; }
    }
}
