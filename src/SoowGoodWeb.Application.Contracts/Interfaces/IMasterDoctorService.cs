using SoowGoodWeb.DtoModels;
using SoowGoodWeb.InputDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SoowGoodWeb.Interfaces
{
    public interface IMasterDoctorService : IApplicationService
    {
        Task<List<MasterDoctorDto>> GetListAsync();
        Task<List<MasterDoctorDto>> GetMasterDoctorListByAgentMasterIdAsync(int doctorId);
        Task<MasterDoctorDto> GetAsync(int id);
        Task<MasterDoctorDto> CreateAsync(MasterDoctorInputDto input);
        Task<MasterDoctorDto> UpdateAsync(MasterDoctorInputDto input);
        Task DeleteAsync(long id);
    }
}
