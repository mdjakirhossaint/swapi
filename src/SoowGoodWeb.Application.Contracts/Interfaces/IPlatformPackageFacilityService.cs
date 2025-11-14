using SoowGoodWeb.DtoModels;
using SoowGoodWeb.InputDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SoowGoodWeb.Interfaces
{
    public interface IPlatformPackageFacilityService : IApplicationService
    {
        Task<List<PlatformPackageFacilityDto>> GetListAsync();
        Task<List<PlatformPackageFacilityDto>> GetPlatformPackageFacilityListByPlatformPackageIdAsync(int platformPackageId);
        Task<PlatformPackageFacilityDto> GetAsync(int id);
        Task<PlatformPackageFacilityDto> CreateAsync(PlatformPackageFacilityInputDto input);
        Task<PlatformPackageFacilityDto> UpdateAsync(PlatformPackageFacilityInputDto input);
        Task DeleteAsync(long id);
    }
}
