using SoowGood.Domain.Service.Models.UserRole;
using SoowGood.Domain.Service.Repositories.UserRole;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Domain.Service.Models.DoctorProfile;
using SoowGoodWeb.Domain.Service.Repositories.DoctorProfile;
using SoowGoodWeb.Domain.Service.Repositories.UserRole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Application.Service.Services.DoctorProfile
{
    public class DoctorProfilessService
    {
        private readonly IDoctorProfileQueryRepository _doctorProfileQueryRepository;
        public DoctorProfilessService(IDoctorProfileQueryRepository doctorProfileQueryRepository)
        {
            _doctorProfileQueryRepository = doctorProfileQueryRepository;
        }
        public async Task<Response<DoctorProfileResponseDto>> GetDoctorProfileId(long userId)
        {
            var response = new Response<DoctorProfileResponseDto>();
            var profileDetails = await _doctorProfileQueryRepository.GetDoctorProfileId(userId);
            if (profileDetails != null)
            {
                response.Result = profileDetails;
                response.IsSuccess = true;
                return response;
            }
            return response;
        }
    }
}
