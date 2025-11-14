using SoowGoodWeb.Application.Service.Services.DoctorProfile;
using SoowGoodWeb.Core.GenericModels;
using SoowGoodWeb.Domain.Service.Models.DoctorProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Services
{
    public class DoctorProfileDetailsService: SoowGoodWebAppService
    {
        private readonly DoctorProfilessService _doctorProfilessService;
        public DoctorProfileDetailsService(DoctorProfilessService doctorProfilessService)
        {

            _doctorProfilessService = doctorProfilessService;
        }


        public virtual async Task<ApiResponse<DoctorProfileResponseDto>> GetDoctorProfile(int id)
        {
            var response = new ApiResponse<DoctorProfileResponseDto>();


            var IsExistingDoctorProfile = await _doctorProfilessService.GetDoctorProfileId(id);

            if (IsExistingDoctorProfile == null)
            {
                response.results = null;
                response.status_code = 422;
                response.message = "Doctor profile not exist";
                response.is_success = false;
                return response;
            }

            response.results = IsExistingDoctorProfile.Result;
            response.is_success = true;
            response.message = "Doctor Profile Get Successfully";
            response.status_code = 200;

            return response;
        }
    }
}
