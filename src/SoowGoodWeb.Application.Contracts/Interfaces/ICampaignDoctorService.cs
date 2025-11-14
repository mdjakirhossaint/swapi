using SoowGoodWeb.DtoModels;
using SoowGoodWeb.InputDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SoowGoodWeb.Interfaces
{
    public interface ICampaignDoctorService : IApplicationService
    {
        Task<List<CampaignDoctorDto>> GetListAsync();
        Task<List<CampaignDoctorDto>> GetCampaignDoctorListByCampaignIdAsync(int doctorId);
        Task<CampaignDoctorDto> GetAsync(int id);
        Task<CampaignDoctorDto> CreateAsync(CampaignDoctorInputDto input);
        Task<CampaignDoctorDto> UpdateAsync(CampaignDoctorInputDto input);
        Task DeleteAsync(long id);
    }
}
