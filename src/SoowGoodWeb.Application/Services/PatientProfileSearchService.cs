
using Abp.Application.Services;
using SoowGoodWeb.Application.Service.Services.Patient;
using SoowGoodWeb.Application.Service.Services.ResponseHelper;
using SoowGoodWeb.Core.GenericModels;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.Models;
using SoowGoodWeb.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoowGoodWeb.Services
{
    public class PatientProfileSearchService : SoowGoodWebAppService
    {
        private readonly PatientService _patientService;
        public PatientProfileSearchService(PatientService patientService)
        {
            _patientService = patientService;
        }
        public async Task<ApiResponse<List<PatientReturnDto>>> GetPatientProfileByMobileNo(string? mobileNo = null, string? creatorEntityId = null)
        {
            var apiResponse = new ApiResponse<List<PatientReturnDto>>();
            var getPatientProfile = await _patientService.GetPatientsBySearchByMobileNo(mobileNo, creatorEntityId);
            if (getPatientProfile.Result.Count == 0)
            {
                ApiResponseHelper.SetFailedResponse(apiResponse, null, "No Data Available", null);
                return apiResponse;
            }
            var result = getPatientProfile.Result;

            var mappedPatientProfile = await MapperCommonService.MapList<PatientReturnDto, PatientReturnDto>(result);

            apiResponse.results = mappedPatientProfile;

            ApiResponseHelper.SetSuccessResponse(apiResponse, apiResponse.results, "Success", null);

            return apiResponse;

        }
    }
}
