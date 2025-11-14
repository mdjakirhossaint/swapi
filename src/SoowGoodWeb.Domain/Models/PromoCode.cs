using SoowGoodWeb.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SoowGoodWeb.Models
{
    public class PromoCode : FullAuditedEntity<long>
    {
        public string? PromoCodeName { get; set; }
        public PromoType? DiscountAmountIn { get; set; }
        public decimal? DiscountAmount { get; set; }
        public DateTime? ValidDate  { get; set; }
        public bool? IsActive { get; set; }
        public int MaxCouponUsedByUser { get; set; }
        public int MaxCouponAmount { get; set; }
    }
}
