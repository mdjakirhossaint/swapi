using SoowGoodWeb.DtoModels;
using SoowGoodWeb.InputDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SoowGoodWeb.Interfaces
{
    public interface IPlatformPackageManagementService : IApplicationService
    {
        Task<List<PlatformPackageManagementDto>> GetListAsync();
        Task<PlatformPackageManagementDto> GetAsync(int id);
        Task<PlatformPackageManagementDto> CreateAsync(PlatformPackageManagementInputDto input);
        Task<PlatformPackageManagementDto> UpdateAsync(PlatformPackageManagementInputDto input);
    }
}
