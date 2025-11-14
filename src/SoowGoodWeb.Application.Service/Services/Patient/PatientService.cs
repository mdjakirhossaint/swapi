using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Domain.Service.Repositories.PatientRepository;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Application.Service.Services.Patient
{
    public class PatientService
    {
        private readonly IPatientQueryRepository _patientQueryRepository;
        public PatientService(IPatientQueryRepository patientQueryRepository)
        {
            _patientQueryRepository = patientQueryRepository;
        }
        public async Task<Response<List<PatientReturnDto>>> GetPatientsBySearchByMobileNo(string? mobileNo = null, string? creatorEntityId = null)
        {
            return await _patientQueryRepository.GetPatientsBySearchByMobileNo(mobileNo, creatorEntityId);
        }
    }
}
