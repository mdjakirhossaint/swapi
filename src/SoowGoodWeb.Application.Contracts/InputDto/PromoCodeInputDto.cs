using SoowGoodWeb.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SoowGoodWeb.InputDto
{
    public class PromoCodeInputDto : FullAuditedEntityDto<long>
    {
        public string? PromoCodeName { get; set; }
        public PromoType? DiscountAmountIn { get; set; }
        public decimal? DiscountAmount { get; set; }
        public DateTime? ValidDate { get; set; }
        public bool? IsActive { get; set; }
        public int MaxCouponUsedByUser { get; set; }
        public int MaxCouponAmount { get; set; }
    }
}
