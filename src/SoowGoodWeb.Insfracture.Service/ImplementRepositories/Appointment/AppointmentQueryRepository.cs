using SoowGoodWeb.Domain.Service.Models.AppointmentDto;
using SoowGoodWeb.Domain.Service.Repositories.Appointment;
using SoowGoodWeb.Insfracture.Service.DataAccessService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Insfracture.Service.ImplementRepositories.Appointment
{
    public class AppointmentQueryRepository : IAppointmentQueryRepository
    {
        private readonly SqlDataAccessLayer _dataAccess;

        public AppointmentQueryRepository(SqlDataAccessLayer dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public async Task<List<AppointmentPatientDto>> GetDoctorPatientProfileId(
            long profileId,
            string? searchTerm
        )
        {
            var response = new List<AppointmentPatientDto>();
            try
            {
                var result = await _dataAccess.LoadDataUsingProcedure<AppointmentPatientDto, dynamic>(
                    "sp_GetPatientsByDoctorId",
                    new
                    {
                        DoctorId = profileId,
                        SearchTerm = searchTerm
                    });

                response = result;
            }
            catch (Exception ex)
            {
                // log the exception if needed
                response = new List<AppointmentPatientDto>();
            }

            return response;
        }

        
    }
}
