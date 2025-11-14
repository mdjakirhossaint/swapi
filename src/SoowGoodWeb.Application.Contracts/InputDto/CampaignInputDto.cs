using SoowGoodWeb.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SoowGoodWeb.InputDto
{
    public class CampaignInputDto : FullAuditedEntityDto<long>
    {
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public bool? IsActive { get; set; }
        public List<CampaignDoctorInputDto>? SelectedDoctor { get; set; }
    }
}
