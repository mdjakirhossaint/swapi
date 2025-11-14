using SoowGood.Domain.Service.Models.UserRole;
using SoowGood.Domain.Service.Repositories.BaseInterface;
using SoowGoodWeb.Domain.Service.Models.DoctorProfile;
using SoowGoodWeb.Domain.Service.Models.UserRole;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Domain.Service.Repositories.DoctorProfile
{
    public interface IDoctorProfileQueryRepository 
    {
        Task<DoctorProfileResponseDto> GetDoctorProfileId(long profileId);
    }
}
