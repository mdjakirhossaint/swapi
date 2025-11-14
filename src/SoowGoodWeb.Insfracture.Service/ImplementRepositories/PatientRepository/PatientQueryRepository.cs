using SoowGood.Domain.Service.Models.UserRole;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Domain.Service.Repositories.PatientRepository;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.Insfracture.Service.DataAccessService;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Insfracture.Service.ImplementRepositories.PatientRepository
{
    public class PatientQueryRepository : IPatientQueryRepository
    {
        private readonly SqlDataAccessLayer _dataAccess;
        public PatientQueryRepository(SqlDataAccessLayer dataAccess)
        {
            _dataAccess = dataAccess;
        }
        public async Task<Response<List<PatientReturnDto>>> GetPatientsBySearchByMobileNo(string? mobileNo = null, string? creatorEntityId = null)
        {
            var response = new Response<List<PatientReturnDto>>();
            try
            {
                var result = await _dataAccess.LoadDataUsingProcedure<PatientReturnDto, dynamic>("SgPatientProfile_GetByMobileNo", new
                {
                    mobileNo,
                    creatorEntityId,
                });

                response.Result = result;
            }
            catch (Exception ex)
            {
                response = new Response<List<PatientReturnDto>>();
            }

            return response;
        }
    }
}
