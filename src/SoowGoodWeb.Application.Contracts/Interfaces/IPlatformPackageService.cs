using SoowGoodWeb.DtoModels;
using SoowGoodWeb.InputDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SoowGoodWeb.Interfaces
{
    public interface IPlatformPackageService : IApplicationService
    {
        Task<List<PlatformPackageDto>> GetListAsync();
        Task<List<PlatformPackageDto>> GetPlatformPackageListByAgentMasterIdAsync(int doctorId);
        Task<PlatformPackageDto> GetAsync(int id);
        Task<PlatformPackageDto> CreateAsync(PlatformPackageInputDto input);
        Task<PlatformPackageDto> UpdateAsync(PlatformPackageInputDto input);
        Task DeleteAsync(long id);
    }
}
