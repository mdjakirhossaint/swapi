using SoowGoodWeb.DtoModels;
using SoowGoodWeb.InputDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SoowGoodWeb.Interfaces
{
    public interface ICampaignService : IApplicationService
    {
        Task<List<CampaignDto>> GetListAsync();
        Task<CampaignDto> GetAsync(int id);
        //Task<CampaignDto> GetByUserNameAsync(string userName);
        Task<CampaignDto> CreateAsync(CampaignInputDto input);
        Task<CampaignDto> UpdateAsync(CampaignInputDto input);
        //Task<CampaignDto> GetByUserIdAsync(Guid userId);
    }
}
