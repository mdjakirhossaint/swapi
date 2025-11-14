using SoowGoodWeb.DtoModels;
using SoowGoodWeb.InputDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SoowGoodWeb.Interfaces
{
    public interface IPromoCodeService : IApplicationService
    {
        Task<List<PromoCodeDto>> GetListAsync();
        Task<PromoCodeDto> GetAsync(int id);
        //Task<AgentMasterDto> GetByUserNameAsync(string userName);
        Task<PromoCodeDto> CreateAsync(PromoCodeInputDto input);
        Task<PromoCodeDto> UpdateAsync(PromoCodeInputDto input);
        //Task<AgentMasterDto> GetByUserIdAsync(Guid userId);
    }
}
