using SoowGoodWeb.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SoowGoodWeb.DtoModels
{
    public class CampaignDto : FullAuditedEntityDto<long>
    {
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public bool? IsActive { get; set; }
        public List<CampaignDoctorDto>? SelectedDoctor { get; set; }

    }
}
