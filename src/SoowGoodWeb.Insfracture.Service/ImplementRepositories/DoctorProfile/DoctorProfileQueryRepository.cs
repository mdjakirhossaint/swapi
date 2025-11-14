using SoowGood.Domain.Service.Models.UserRole;
using SoowGoodWeb.Domain.Service.Models.DoctorProfile;
using SoowGoodWeb.Domain.Service.Repositories.DoctorProfile;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.Insfracture.Service.DataAccessService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Insfracture.Service.ImplementRepositories.DoctorProfile
{
    public class DoctorProfileQueryRepository : IDoctorProfileQueryRepository
    {
        private readonly SqlDataAccessLayer _dataAccess;
        public DoctorProfileQueryRepository(SqlDataAccessLayer dataAccess)
        {
            _dataAccess = dataAccess;
        }
        public async Task<DoctorProfileResponseDto> GetDoctorProfileId(long profileId)
        {
            var response = new DoctorProfileResponseDto();
            try
            {
                var result = await _dataAccess.LoadSingleDataUsingProcedure<DoctorProfileResponseDto, dynamic>("getDoctorProfileById", new
                {
                    profileId = profileId
                });

                response = result;
            }
            catch (Exception ex)
            {
                response = new DoctorProfileResponseDto();
            }

            return response;

        }


  



    }
}
